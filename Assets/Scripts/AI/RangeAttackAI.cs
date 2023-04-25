using UnityEngine;

public class RangeAttackAI : SomeAI
{
    public float alertRadius;
    public float alertPeridiocity;
    public float shootingDistance; // Should be smaller than alert radius
    public string[] entitiesToExclude;
    public WalkingAI wai;
    public AttackAI aai;
    public GameObject projectile;


    private AIManager _currentEnemy = null;
    private float _alertTimer = 0f;

    public override void PrepareAction()
    {
        if (_currentEnemy != null)
        {
            Vector3 destVector = (_currentEnemy.transform.position - transform.position) / 2.5f;
            wai.SetDestination(transform.position + destVector);
            wai.SetNextState("Attack");
            aai.SetProjectile(projectile);
            aai.SetTarget(_currentEnemy);
        }
    }

    public override void Act()
    {
        if (_currentEnemy != null)
        {
            if (Vector3.SqrMagnitude(_currentEnemy.transform.position - transform.position)
                > shootingDistance * shootingDistance)
            {
                _aiManager.Transition("Walking");
            }
            else
            {
                _aiManager.Transition("Attack");
            }
        }
        else
        {
            _aiManager.Transition("Idle");
        }
    }

    private void FixedUpdate()
    {
        _alertTimer -= Time.fixedDeltaTime;
        if (_alertTimer < 0f)
        {
            _alertTimer = alertPeridiocity;
            AIManager someAI = FindSomeEnemy();
            if (someAI != null)
            {
                _aiManager.Transition("RangeAttack");
                _currentEnemy = someAI;
            }
        }
    }

    private AIManager FindSomeEnemy()
    {
        AIManager ai = _aiManager.logic.GetClosestEntity(_aiManager, alertRadius, entitiesToExclude: entitiesToExclude);
        return ai;
    }
}