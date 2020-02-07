using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathing
{

    /* Needs fixing as they cut corners */
    public class Path
    {
        public readonly Vector3[] LookPoints;
        readonly float turnDistForDebug;
        public readonly int FinishLineIndex;
        public readonly int SlowDownIndex;

        public Path(Vector3[] waypoints, Vector3 startPos, float turnDist, float stoppingDist)
        {
            turnDistForDebug = turnDist;

            LookPoints = waypoints;
            FinishLineIndex = LookPoints.Length - 1;

            float distFromEnd = 0;
            for (int i = LookPoints.Length - 1; i > 0; --i)
            {
                distFromEnd += Vector3.Distance(LookPoints[i], LookPoints[i - 1]);
                if (distFromEnd > stoppingDist)
                {
                    SlowDownIndex = i;
                    break;
                }
            }
        }

        Vector2 V3ToV2(Vector3 v3)
        {
            return new Vector2(v3.x, v3.z);
        }

        public void DrawWithGizmos()
        {
            Gizmos.color = Color.black;
            foreach (Vector3 p in LookPoints)
            {
                Gizmos.DrawCube(p + Vector3.up, Vector3.one);
            }

            Gizmos.color = Color.white;
            for (int i = 0; i < LookPoints.Length - 2; ++i)
            {
                Gizmos.DrawLine(LookPoints[i], LookPoints[i + 1]);
            }

            Gizmos.color = Color.cyan;
            //foreach (Line l in turnBoundaries)
            //{
            //    l.DrawWithGizmos(10);
            //}
            foreach (Vector3 p in LookPoints)
            {
                Gizmos.DrawWireSphere(p, turnDistForDebug);
            }
        }
    }

}