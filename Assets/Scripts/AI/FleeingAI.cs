using UnityEngine;

public class FleeingAI : SomeAI
{
    [SerializeField] private float alertRange = 20f;
    [SerializeField] private float noConciousTime = 5f;
    [SerializeField] private float fleeRange = 30f;
    [SerializeField] private string[] entitiesToExclude;

    private float timer = 0f;
    private AIManager currentTarget;
    private WalkingAI wai;

    private void FixedUpdate()
    {
        if (currentTarget == null)
        {
            noConciousTime = 0f;
            currentTarget = _aiManager.logic.GetClosestEntity(_aiManager, alertRange, entitiesToExclude: entitiesToExclude);

            if (currentTarget != null && timer > noConciousTime)
            {
                timer = 0f;

                if (wai == null)
                {
                    wai = _aiManager.GetAI<WalkingAI>();
                }

                Vector3 awayDest = transform.position
                    + (transform.position - currentTarget.transform.position).normalized * fleeRange;

                awayDest.y = transform.position.y;

                Debug.DrawLine(transform.position, awayDest, Color.yellow, 7f);

                wai.SetDestination(awayDest);
                wai.SetOnArrivalAction(() =>
                {
                    currentTarget = null;
                });

                _aiManager.Transition("Walking");
            }
            else
            {
                timer += Time.fixedDeltaTime;
            }
        }
    }

    public override void PrepareAction()
    {
    }

    public override void Act()
    {
    }
}