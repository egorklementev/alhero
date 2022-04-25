using System.Collections.Generic;
using UnityEngine;

public class RayWalkingAI : WalkingAI
{
    [Range(0f, 360f)]
    public float maxTurnAngle = 180f;
    public float minimalStep = 4f;
    public float pathFindRange = 180f;

    public override void PrepareAction()
    {
        _currentDest.y = transform.position.y; // Debatable
        _walkRoute.Clear();
        _walkRoute.Add(transform.position); // Initial point
        int maxPoints = 16;
        int rotSegments = 36;
        int distProbes = 12;
        float distSeg = 2f;
        Vector3 curPointPos = _walkRoute[0];
        Vector3 lastDirection = Vector3.forward;
        float eps = 3f; // If less that that, it means we reached destination

        for (int p = 0; p < maxPoints; p++)
        {
            Vector3 trueRoute = (_currentDest - curPointPos).normalized;
            List<Vector3> possibleRoutes = new List<Vector3>();
            List<float> possibleDists = new List<float>();

            if (!TestRaycasts(curPointPos, trueRoute, Vector3.Distance(curPointPos, _currentDest), colliderBounds))
            {
                _walkRoute.Add(_currentDest);
                Debug.DrawLine(curPointPos, _currentDest, Color.green, 5f);
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
                    float dist = (pRoute * possibleDists[index] - _walkRoute.Last()).sqrMagnitude;
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
                _walkRoute.Add(curPointPos);
            }

            if ((_currentDest - curPointPos).sqrMagnitude < eps)
            {
                break;
            }
        }

        if (_walkRoute.Count > 1)
        {
            for (int i = 1; i < _walkRoute.Count - 1; i++)
            {
                Debug.DrawLine(_walkRoute[i - 1], _walkRoute[i], Color.red, 5f, false);
            }
        }

        if (_walkRoute.Count > 0)
        {
            _lastPosition = _walkRoute[0];
        }
    }
}
