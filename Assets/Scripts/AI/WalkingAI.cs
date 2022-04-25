using System.Collections.Generic;
using UnityEngine;

public abstract class WalkingAI : SomeAI
{
    public float moveSpeed = 2f;
    public float colliderBounds = 4f;
    public Rigidbody Body;

    protected List<Vector3> _walkRoute = new List<Vector3>();
    protected Vector3 _lastPosition;
    protected Vector3 _currentDest;
    protected float _stuckTime = 0f;
    protected string _nextState = "Idle";

    public void SetDestination(Vector3 destination)
    {
        _currentDest = destination;
    }

    public void SetNextState(string nextState)
    {
        _nextState = nextState;
    }

    protected bool TestRaycasts(Vector3 position, Vector3 direction, float distance, float size = 3.25f)
    {
        // Debug.DrawLine(position, position + direction * distance, Color.yellow, 5f);
        return 
            Physics.Raycast(position, direction, distance) ||
            Physics.Raycast(position + Vector3.forward * size, direction, distance) ||
            Physics.Raycast(position + Vector3.back * size, direction, distance) ||
            Physics.Raycast(position + Vector3.left * size, direction, distance) ||
            Physics.Raycast(position + Vector3.right * size, direction, distance) ||
            Physics.Raycast(position + Vector3.up * size, direction, distance) ||
            Physics.Raycast(position + Vector3.down * size, direction, distance);
    }

    public override void Act()
    {        
        if (_walkRoute.Count > 0)
        {
            transform.LookAt(_walkRoute[0]);
            transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
            float eps = colliderBounds / 3f;
            float stuckEps = .33333f;
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
                    _aiManager.Transition(_nextState);
                    _walkRoute.Clear();
                    _stuckTime = 0f;
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
            _aiManager.Transition(_nextState);
            _walkRoute.Clear();
        }
    }
}