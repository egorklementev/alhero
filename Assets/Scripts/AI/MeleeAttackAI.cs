using UnityEngine;

public class MeleeAttackAI : SomeAI 
{
    public float alertRadius;

    private AIManager _currentEnemy;

    public override void PrepareAction()
    {
        WalkingAI wai = _aiManager.GetAI<WalkingAI>();
        _currentEnemy = FindSomeEnemy(alertRadius);
        wai.SetDestination(_currentEnemy.transform.position);
        wai.SetNextState("Attacking");
        _aiManager.GetAI<AttackAI>().SetTarget(_currentEnemy);
    }

    public override void Act() 
    {
        _aiManager.Transition("Walking");
    }

    private void FixedUpdate() 
    {
        AIManager someAI = FindSomeEnemy(alertRadius);
        if (someAI != null && _currentEnemy == null)
        {
            _aiManager.Transition("MeleeAttack");
        }
        _currentEnemy = someAI;
    }

    private AIManager FindSomeEnemy(float radius)
    {
        AIManager ai = _aiManager.logic.GetClosestEntity(_aiManager);
        return (
            ai == null ? null : (Vector3.Distance(ai.transform.position, transform.position) > radius ? null : ai)
        );
    }
}