using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MagpieAI : BaseAI 
{

    public float moveSpeed = 2f;
    public float memoryTime = 5f;
    public float threatRadius = 10f;
    public Animator anim;
    public Rigidbody body;
    public ItemWorld PickedItem;
    [Space(20f)]
    [Range(0f, 360f)]
    public float pathFindRange = 180f;
    [Range(0f, 180f)]
    public float maxTurnAngle = 180f;
    public float minimalStep = 4f;
    public float colliderBounds = 4f;
    

    private float stuckTime = 0f;
    private List<Vector3> walkRoute;
    private Vector3 lastPosition;
    private int rotationFramesAmount = 0;
    private int framesOfRotation = 0;
    private float initialRotation = 0f;
    private float desiredRotation = 0f;
    private float memory = 0f;
    private BaseAI currentThreat;
    private List<Action> actions;

    private void Awake() {
        currentThreat = null;
        memory = memoryTime;
        walkRoute = new List<Vector3>();

        AIState idleState = new AIState("Idle");
        AIState rotateState = new AIState("Rotating");
        AIState walkState = new AIState("Walking");
        AIState threateningState = new AIState("Threatening");

        idleState.AddAdjState(rotateState, walkState, threateningState);
        rotateState.AddAdjState(idleState, threateningState);
        walkState.AddAdjState(idleState, threateningState);
        threateningState.AddAdjState(idleState);

        SetState(idleState);

        actions = new List<Action>()
        {
            () => { memory = memoryTime * Random.value; },
            () => { 
                float maxDist = 50f;
                float x = Random.value * maxDist - maxDist / 2f; 
                float z = Random.value * maxDist - maxDist / 2f; 
                Vector3 randDest = new Vector3(x, 0f, z) + transform.position; 
                DecideToWalk(randDest); 
                },
            () => DecideToRotate(),
            () => DecideToPick(),
            () => DecideToThrow(),
        };
    }

    private void FixedUpdate() 
    {
        if (enabled) 
        {
            if (memory > 0f)
            {
                memory -= Time.fixedDeltaTime;

                // Locate threats if any
                BaseAI threat = GetSomeThreat();
                if (threat != null)
                {
                    currentThreat = threat;
                    UpdateMemory();
                    ResetAnimationParams();
                    Transition("Threatening");
                }

                switch (GetState().Name)
                {
                    case "Threatening":
                        BecomeThreatening();
                        break;
                    case "Walking":
                        WalkToTheDestination();
                        break;
                    case "Rotating":
                        RotateBody();
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (GetState().Name)
                {
                    case "Idle":
                        DecideWhatToDo();
                        break;
                    case "Threatening":
                        if (GetSomeThreat() != null)
                        {
                            BecomeThreatening();
                        }
                        else
                        {
                            Transition("Idle");
                            ResetAnimationParams();
                        }
                        break;
                    default:
                        break;
                }

                UpdateMemory();
            }
        }
    }

    private BaseAI GetSomeThreat()
    {
        BaseAI ai = logic.GetClosestEntity(this);
        return (
            ai == null ? null : (
                ai as MagpieAI != null ? null : (
                    Vector3.Distance(ai.transform.position, transform.position) > threatRadius ? null : ai
                )
            )
        );
    }

    private void UpdateMemory()
    {
        memory = memoryTime;
    }

    private void WalkToTheDestination()
    {
        anim.SetBool("IsWalking", true);

        if (walkRoute.Count > 0)
        {
            float eps = .5f;
            if ((transform.position - walkRoute[0]).magnitude < eps)
            {
                // To the next route point
                walkRoute.RemoveAt(0); 
                if (walkRoute.Count > 0)
                {
                    transform.LookAt(walkRoute[0]);
                }
            }
            else
            {
                if ((lastPosition - transform.position).magnitude < eps * eps)
                {
                    stuckTime += Time.fixedDeltaTime;
                }
                else
                {
                    stuckTime = 0f;
                }

                if (stuckTime > 1.5f)
                {
                    ResetAnimationParams();
                    Transition("Idle");
                    walkRoute.Clear();
                    stuckTime = 0f;
                }
                else
                {
                    lastPosition = transform.position;
                    body.AddForce((walkRoute[0] - transform.position).normalized * moveSpeed);
                }

            }
        }
        else
        {
            // No other points to reach
            ResetAnimationParams();
            Transition("Idle");
            walkRoute.Clear();
        }
    }

    private void BecomeThreatening()
    {
        transform.LookAt(currentThreat.transform.position);
        anim.SetBool("IsThreatening", true);

    }

    private void RotateBody()
    {
        if (framesOfRotation >= rotationFramesAmount)
        {
            Transition("Idle");
            ResetAnimationParams();
            framesOfRotation = 0;
        }
        else
        {
            transform.rotation = Quaternion.Euler(Vector3.Lerp(
                new Vector3(0f, initialRotation, 0f),
                new Vector3(0f, desiredRotation, 0f),
                (float) framesOfRotation / rotationFramesAmount
            ));
            framesOfRotation++;
        }
    }

    private void ResetAnimationParams()
    {
        anim.SetBool("IsThreatening", false);
        anim.SetBool("IsWalking", false);
    }

    private void DecideWhatToDo()
    {
        int decision = Random.Range(0, actions.Count);
        actions[decision].Invoke();
        // $"Decision: {decision}".Log<MagpieAI>();
    }

    private void DecideToWalk(Vector3 dest)
    {
        dest.y = transform.position.y;
        walkRoute.Clear();
        walkRoute.Add(transform.position); // Initial point
        int maxPoints = 16;
        int rotSegments = 36;
        int distProbes = 8;
        float distSeg = 3f;
        Vector3 curPointPos = walkRoute[0];
        Vector3 lastDirection = Vector3.forward;
        float eps = 1.5f; // If less that that, it means we reached destination

        for (int p = 0; p < maxPoints; p++)
        {
            Vector3 trueRoute = (dest - curPointPos).normalized;
            List<Vector3> possibleRoutes = new List<Vector3>();
            List<float> possibleDists = new List<float>();

            if (!TestRaycasts(curPointPos, trueRoute, Vector3.Distance(curPointPos, dest), colliderBounds))
            {
                walkRoute.Add(dest);
                Debug.DrawLine(curPointPos, dest, Color.green, 5f);
                break;
            }
            else
            {
                for (int i = 0; i < rotSegments; i++) // Over rotation angle
                {
                    float angle =  (i * pathFindRange / rotSegments + (360f - pathFindRange)) * Mathf.PI / 180f;
                    Vector3 dir = (curPointPos + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle))) - curPointPos;
                    dir = dir.normalized;

                    for (int j = 0; j < distProbes; j++) // Over segment distance
                    {
                        float distance = minimalStep + distProbes * distSeg - j * distSeg;
                        if (!TestRaycasts(curPointPos, dir, distance, colliderBounds))
                        {
                            possibleDists.Add(distance);
                            possibleRoutes.Add(dir);
                            // Debug.DrawRay(curPointPos, dir * possibleDists[possibleDists.Count - 1], Color.blue, 5f);
                            break;
                        }
                    }
                }

                float maxDist = 0f;
                float newDist = 0f;
                Vector3 newRoute = trueRoute;
                int index = 0;
                foreach (Vector3 pRoute in possibleRoutes)
                {
                    float angle = Vector3.Angle(pRoute, lastDirection);
                    float dist = (pRoute * possibleDists[index] - walkRoute.Last()).sqrMagnitude;
                    // $"AngChange: {angle}".Log(this);
                    if (angle < maxTurnAngle && dist > maxDist)
                    {
                        newDist = possibleDists[index];
                        newRoute = pRoute;
                        maxDist = dist;
                    }
                    index++;
                }

                lastDirection = newRoute;
                curPointPos = curPointPos + newRoute * newDist;
                walkRoute.Add(curPointPos);
            }

            if ((dest - curPointPos).sqrMagnitude < eps)
            {
                break;
            }
        }

        if (walkRoute.Count > 1)
        {
            for (int i = 1; i < walkRoute.Count - 1; i++)
            {
                Debug.DrawLine(walkRoute[i - 1], walkRoute[i], Color.red, 5f, false);
            }
        }

        if (walkRoute.Count > 0)
        {
            lastPosition = walkRoute[0];
            Transition("Walking");
        }
    }

    private void DecideToRotate()
    {
        initialRotation = transform.rotation.y;
        desiredRotation = transform.rotation.y + (Random.Range(0f, 90f) - 45f);
        rotationFramesAmount = Random.Range(30, 60);
        framesOfRotation = 0;
        Transition("Rotating");
    }

    private void DecideToPick()
    {
        ItemWorld item = logic.GetClosestItem(this);
        if (item != null && !item.Equals(PickedItem))
        {
            DecideToWalk(item.transform.position);
        }
    }

    private void DecideToThrow()
    {
        if (PickedItem != null)
        {
            float force = 3f;
            float rot = transform.rotation.y;
            PickedItem.SetPickedUp(false);
            PickedItem.GetBody().AddRelativeForce(
                new Vector3(force * Mathf.Cos(rot), force, .25f * force * Mathf.Sin(rot)), ForceMode.Impulse
                );
            PickedItem = null;
        }
    }
    
    private bool TestRaycasts(Vector3 position, Vector3 direction, float distance, float size = 3.25f)
    {
        return 
            Physics.Raycast(position, direction, distance) ||
            Physics.Raycast(position + Vector3.forward * size, direction, distance) ||
            Physics.Raycast(position + Vector3.back * size, direction, distance) ||
            Physics.Raycast(position + Vector3.left * size, direction, distance) ||
            Physics.Raycast(position + Vector3.right * size, direction, distance);
    }
}   