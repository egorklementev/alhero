using UnityEngine;

public class WanderingAI : SomeAI 
{
    public float wanderDistance = 50f;

    private Vector3 _destination;
    public override void PrepareAction()
    {
        float x = DataController.random.Value() * wanderDistance - wanderDistance / 2f; 
        float z = DataController.random.Value() * wanderDistance - wanderDistance / 2f;
        _destination = new Vector3(x, 0f, z) + transform.position;
    }

    public override void Act()
    {
        WalkingAI wai = _aiManager.GetAI<WalkingAI>();
        wai.SetDestination(_destination);
        wai.SetNextState("Idle");
        _aiManager.Transition("Walking");
    }
}