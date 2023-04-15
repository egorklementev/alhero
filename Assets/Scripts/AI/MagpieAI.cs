using System.Collections.Generic;
using UnityEngine;

public class MagpieAI : SomeAI 
{
    public float memoryTime = 5f;
    public float threatRadius = 10f;
    public string[] entitiesToExclude; 

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
            Vector3 euler = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(0f, euler.y, euler.z);
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
        return _aiManager.logic.GetClosestEntity(_aiManager, threatRadius, entitiesToExclude: entitiesToExclude);
    }
}   