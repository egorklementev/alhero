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

    private float walkTime = 0f;
    private Vector3 destination;
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
        destination = transform.position;

        AIState idleState = new AIState("Idle");
        AIState rotateState = new AIState("Rotating");
        AIState walkState = new AIState("Walking");
        AIState threateningState = new AIState("Threatening");

        idleState.AddAdjState(rotateState, walkState, threateningState);
        rotateState.AddAdjState(idleState, threateningState);
        walkState.AddAdjState(idleState, threateningState);
        threateningState.AddAdjState(idleState);

        SetState(idleState);

        actions = new List<Action>();
        for (int i = 0; i < 3; i++)
        {
            actions.Add(() => { memory = memoryTime * Random.value; }); // Just idle around
        }
        actions[0] = () => DecideToWalk();
        actions[1] = () => DecideToRotate();
        // TODO: add other actions here
    }

    private void FixedUpdate() {

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

    private BaseAI GetSomeThreat()
    {
        return LogicController.entities.Find(
            ai => (ai.transform.position - transform.position).magnitude < threatRadius &&
            !ai.Equals(this) && ai as MagpieAI == null
            );
    }

    private void UpdateMemory()
    {
        memory = memoryTime;
    }

    private void WalkToTheDestination()
    {
        anim.SetBool("IsWalking", true);

        float eps = .1f;
        if (
            (transform.position - destination).magnitude < eps || 
            (lastPosition - transform.position).magnitude < eps * eps && walkTime > 1f // Agent is stuck
            )
        {
            // "Destination" reached
            walkTime = 0f;
            destination = transform.position;
            ResetAnimationParams();
            Transition("Idle");
        }
        else
        {
            // Continue walking
            lastPosition = transform.position;
            body.AddForce((transform.position - destination).normalized * moveSpeed);
            walkTime += Time.fixedDeltaTime;
        }
    }

    private void BecomeThreatening()
    {
        transform.LookAt(currentThreat.transform.position);
        transform.Rotate(0f, 180f, 0f);
        anim.SetBool("IsThreatening", true);

    }

    private void RotateBody()
    {
        if (framesOfRotation >= rotationFramesAmount)
        {
            Transition("Idle");
            ResetAnimationParams();
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
        actions[Random.Range(0, actions.Count)].Invoke();
    }

    private void DecideToWalk()
    {
        // We try here at max 8 times to find a free way
        for (int i = 0; i < 8; i++)
        {
            float distance = 3f + Random.value * 50f;
            Vector2 rnd = Random.insideUnitCircle;
            Vector3 direction = new Vector3(rnd.x, 0f, rnd.y);
            if (!Physics.Raycast(transform.position, direction, distance))
            {
                destination = transform.position + direction * distance;
                break;
            }
        }
        lastPosition = destination;
        transform.LookAt(destination);
        Transition("Walking");
    }

    private void DecideToRotate()
    {
        initialRotation = transform.rotation.y;
        desiredRotation = transform.rotation.y + (Random.Range(0f, 90f) - 45f);
        rotationFramesAmount = Random.Range(30, 60);
        framesOfRotation = 0;
        Transition("Rotating");
    }
}   