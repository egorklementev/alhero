using UnityEngine;

public class MeleeAttackAI : SomeAI 
{
    public float alertRadius;
    public float alertPeridiocity;
    public string[] entitiesToExclude;
    public WalkingAI wai;
    public AttackAI aai;

    private AIManager _currentEnemy = null;
    private float _alertTimer = 0f;

    public override void PrepareAction()
    {
        if (_currentEnemy != null)
        {
            wai.SetDestination(_currentEnemy.transform.position);
            wai.SetNextState("Attacking");
            aai.SetTarget(_currentEnemy);
        }
    }

    public override void Act() 
    {
        if (_currentEnemy != null)
        {
            _aiManager.Transition("Walking");
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
                _aiManager.Transition("MeleeAttack");
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