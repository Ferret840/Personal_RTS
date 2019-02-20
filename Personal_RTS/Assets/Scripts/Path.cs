using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Needs fixing as they cut corners */
public class Path
{
    public readonly Vector3[] lookPoints;
    [System.Obsolete("Use circle distance check")]
    public readonly Line[] turnBoundaries;
    readonly float turnDistForDebug;
    public readonly int finishLineIndex;
    public readonly int slowDownIndex;

    public Path(Vector3[] waypoints, Vector3 startPos, float turnDist, float stoppingDist)
    {
        turnDistForDebug = turnDist;

        lookPoints = waypoints;
        //turnBoundaries = new Line[lookPoints.Length];
        finishLineIndex = lookPoints.Length - 1;

        Vector2 previousPoint = V3ToV2(startPos);
        for (int i = 0; i < finishLineIndex; ++i)
        {
            previousPoint = AddTurnBoundary(previousPoint, turnDist, i);
        }

        //Add final point, with turn dist set to 0 to turn into it instead
        AddTurnBoundary(previousPoint, 0, finishLineIndex);

        float distFromEnd = 0;
        for (int i = lookPoints.Length - 1; i > 0; --i)
        {
            distFromEnd += Vector3.Distance(lookPoints[i], lookPoints[i - 1]);
            if(distFromEnd > stoppingDist)
            {
                slowDownIndex = i;
                break;
            }
        }
    }

    Vector2 AddTurnBoundary(Vector2 previousPoint, float turnDist, int i)
    {
        Vector2 currentPoint = V3ToV2(lookPoints[i]);
        Vector2 dirToCurrentPoint = (currentPoint - previousPoint).normalized;
        Vector2 turnBoundaryPoint = currentPoint - dirToCurrentPoint * turnDist;
        //turnBoundaries[i] = new Line(turnBoundaryPoint, previousPoint - dirToCurrentPoint * turnDist);
        return previousPoint = turnBoundaryPoint;
    }

    Vector2 V3ToV2(Vector3 v3)
    {
        return new Vector2(v3.x, v3.z);
    }

    public void DrawWithGizmos()
    {
        Gizmos.color = Color.black;
        foreach(Vector3 p in lookPoints)
        {
            Gizmos.DrawCube(p + Vector3.up, Vector3.one);
        }

        Gizmos.color = Color.white;
        for (int i = 0; i < lookPoints.Length - 2; ++i)
        {
            Gizmos.DrawLine(lookPoints[i], lookPoints[i + 1]);
        }

        Gizmos.color = Color.cyan;
        //foreach (Line l in turnBoundaries)
        //{
        //    l.DrawWithGizmos(10);
        //}
        foreach (Vector3 p in lookPoints)
        {
            Gizmos.DrawWireSphere(p, turnDistForDebug);
        }
    }
}
