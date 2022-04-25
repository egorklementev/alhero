using UnityEngine;

public class SleepingAI : SomeAI
{
    public float sleepDuration;

    private float _currentDuration;

    public override void PrepareAction()
    {
        _currentDuration = sleepDuration;
    }

    public override void Act()
    {
        _currentDuration -= Time.fixedDeltaTime;
        if (_currentDuration < 0f)
        {
            _aiManager.Transition("Idle");
        }
    }
}