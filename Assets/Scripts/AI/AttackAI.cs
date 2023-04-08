using UnityEngine;

public class AttackAI : SomeAI 
{
    public float attackDuration;
    public float attackRange;

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

            if (_target == null) // The target is already dead
                return;

            float distanceToTarget = Vector3.Distance(_target.transform.position, transform.position);
            if (distanceToTarget < attackRange)
            {
                _target.Transition("Death");
            }
            else
            {
                $"out of range: ({distanceToTarget}/{attackRange})".Log(this);
            }
        }
        else
        {
            if (_target != null)
            {
                transform.LookAt(_target.transform);
                Vector3 euler = transform.rotation.eulerAngles;
                transform.rotation = Quaternion.Euler(0f, euler.y, euler.z);
            }
        }
    }

    public void SetTarget(AIManager target)
    {
        _target = target;
    }
}