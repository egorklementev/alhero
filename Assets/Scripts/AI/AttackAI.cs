using UnityEngine;

public class AttackAI : SomeAI 
{
    public float attackDuration;

    private AIManager _target = null;
    private float _currentAttackDuration;

    public override void PrepareAction()
    {
        _currentAttackDuration = attackDuration;
    }

    public override void Act()
    {
        _currentAttackDuration -= Time.fixedDeltaTime;
        if (_currentAttackDuration < 0f)
        {
            _aiManager.Transition("Idle");
        }
        else
        {
            if (_target != null)
            {
                transform.LookAt(_target.transform);
            }
        }
    }

    public void SetTarget(AIManager target)
    {
        _target = target;
    }
}