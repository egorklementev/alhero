using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class WalkingAI : SomeAI
{
    public float moveSpeed = 2f;
    public float colliderBounds = 4f;
    public float colliderHeight = 4f;
    public Rigidbody Body;
    [Range(0.001f, 2f)] public float stuckEps = .33333f;

    protected List<Vector3> _walkRoute = new List<Vector3>();
    protected Vector3 _lastPosition;
    protected Vector3 _currentDest;
    protected float _stuckTime = 0f;
    protected string _nextState = "Idle";
    protected bool _routeFound = false;
    protected Action arrivalAction;

    public void SetDestination(Vector3 destination)
    {
        _currentDest = destination;
    }

    public void SetNextState(string nextState)
    {
        _nextState = nextState;
    }

    public void SetOnArrivalAction(Action action)
    {
        arrivalAction = action;
    }

    protected bool TestRaycasts(Vector3 position, Vector3 direction, float distance, float size = 3.25f)
    {
        var ratio = colliderHeight / colliderBounds;

        Vector3[] dirsToCheck = new Vector3[]
        {
            Vector3.left, 
            Vector3.right, 
            Vector3.forward, 
            Vector3.back, 
            Vector3.down * ratio,
            Vector3.down * ratio + Vector3.left,
            Vector3.down * ratio + Vector3.right,
            Vector3.down * ratio + Vector3.forward,
            Vector3.down * ratio + Vector3.back,
        };

        size /= 2f;

        Debug.DrawLine(position + Vector3.down * ratio * size,
            position + Vector3.down * ratio * size + direction * distance,
            new Color(1f, .5f, 0f, .5f), 5f);


        foreach (Vector3 v in dirsToCheck)
        {
            if (Physics.CheckSphere(position + v * size, 0f)
                || Physics.Raycast(position + v * size, direction, distance))
                return true;
        }

        return false;
    }

    public override void Act()
    {        
        if (_routeFound) 
        {
            if (_walkRoute.Count > 0)
            {
                transform.LookAt(_walkRoute[0]);
                transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
                float eps = colliderBounds / 3f;
                if ((transform.position - _walkRoute[0]).magnitude < eps)
                {
                    // To the next route point
                    _walkRoute.RemoveAt(0); 
                }
                else
                {
                    if ((_lastPosition - transform.position).magnitude < stuckEps * stuckEps)
                    {
                        _stuckTime += Time.fixedDeltaTime;
                    }
                    else
                    {
                        _stuckTime = 0f;
                    }

                    if (_stuckTime > 1.5f)
                    {
                        OnArrival();
                    }
                    else
                    {
                        _lastPosition = transform.position;
                        Body.AddForce((_walkRoute[0] - transform.position).normalized * moveSpeed);
                    }

                }
            }
            else
            {
                // No other points to reach
                OnArrival();
            }
        }
        else
        {
            // In case it takes more than 10 sec to build a route
            _stuckTime += Time.fixedDeltaTime;
            if (_stuckTime > 10f)
            {
                OnArrival();
            }
        }
    }

    private void OnArrival()
    {
        _aiManager.Transition(_nextState);
        _walkRoute.Clear();
        _stuckTime = 0f;

        arrivalAction?.Invoke();
    }
}