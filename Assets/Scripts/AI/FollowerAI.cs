using UnityEngine;

public class FollowerAI : SomeAI 
{
    public string tagToFollow = "empty";
    public string defaultState = "Idle";
    public float searchRadius = 10f;
    public Transform _target;

    public override void PrepareAction()
    {
    }

    public override void Act()
    {
        if (_target != null)
        {
            float farEnoughDistance = searchRadius / 4f; // Since we do not want it to follow anytime
            farEnoughDistance *= farEnoughDistance;
            float actualDistance = (_target.position - transform.position).sqrMagnitude;

            if (actualDistance > farEnoughDistance)
            {
                WalkingAI wai = _aiManager.GetAI<WalkingAI>();
                wai.SetDestination(_target.position);
                _aiManager.Transition("Walking");
            }
            else
            {
                _aiManager.Transition(defaultState);
            }
        }
        else
        {
            AIManager nearestAI = _aiManager.logic.GetClosestEntity(_aiManager, searchRadius, tagToFollow);
            if (nearestAI != null)
            {
                SetTarget(nearestAI.transform);    
            }
            else
            {
                _aiManager.Transition(defaultState);
            }
        }
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }
}