using UnityEngine;

public class UnskipAnimationAI : SomeAI 
{
    public float Duration = 1f;
    public string NextState = "Idle";

    private float _timer = 0;

    public override void PrepareAction()
    {
        _timer = Duration;
    }

    public override void Act()
    {
        _timer -= Time.fixedDeltaTime;
        if (_timer < 0f)
        {
            _aiManager.Transition(NextState);
        }
    }
}