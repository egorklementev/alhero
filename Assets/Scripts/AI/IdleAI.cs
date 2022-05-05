using UnityEngine;

public class IdleAI : SomeAI 
{
    public string[] statesToSwitchTo;
    public Vector2 idleTimeRange;

    private float _idle = 0f;

    public override void PrepareAction()
    {
        _idle = Random.Range(idleTimeRange.x, idleTimeRange.y);
    }

    public override void Act()
    {
        if (statesToSwitchTo.Length > 0)
        {
            _idle -= Time.fixedDeltaTime;
            if (_idle < 0f)
            {
                _aiManager.Transition(statesToSwitchTo[Random.Range(0, statesToSwitchTo.Length)]);
            }
        }
    }
}