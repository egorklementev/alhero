using UnityEngine;

public class MagpieAI : SomeAI 
{
    public float memoryTime = 5f;
    public float threatRadius = 10f;

    private float _memory = 0f;
    private AIManager _currentThreat;

    private void Awake() 
    {
        _currentThreat = null;
    }

    private void FixedUpdate() 
    {
        if (GetSomeThreat() != null)
        {
            _aiManager.Transition("Threatening");
        }     
    }

    public override void PrepareAction()
    {
        _memory = memoryTime;
        _currentThreat = GetSomeThreat();
    }

    public override void Act()
    {
        if (_currentThreat != null)
        {
            transform.LookAt(_currentThreat.transform.position);
            _memory -= Time.deltaTime;
            if (_memory < 0f)
            {
                if (GetSomeThreat() != null)
                {
                    PrepareAction();
                }
                else
                {
                    _currentThreat = null;
                    _aiManager.Transition("Idle");
                }
            }
        }
        else
        {
            _aiManager.Transition("Idle");
        }
    }

    private AIManager GetSomeThreat()
    {
        AIManager ai = _aiManager.logic.GetClosestEntity(_aiManager);
        return (
            ai == null ? null : (
                ai.GetAI<MagpieAI>() != null ? null : (
                    Vector3.Distance(ai.transform.position, transform.position) > threatRadius ? null : ai
                )
            )
        );
    }
}   