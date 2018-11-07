using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Line
{
    const float VerticalLineGradient = float.MaxValue;

    float gradient;
    float yIntercept;
    Vector2 pointOnLine_1, pointOnLine_2;

    float gradientPerp;

    bool approachSide;

    public Line(Vector2 pointOnLine, Vector2 pointPerp)
    {
        float dx = pointOnLine.x - pointPerp.x;
        float dy = pointOnLine.y - pointPerp.y;

        if (dx == 0)
            gradientPerp = VerticalLineGradient;
        else
            gradientPerp = dy / dx;

        if (gradientPerp == 0)
            gradient = VerticalLineGradient;
        else
            gradient = -1 / gradientPerp;

        yIntercept = pointOnLine.y - gradient * pointOnLine.x;
        pointOnLine_1 = pointOnLine;
        pointOnLine_2 = pointOnLine + new Vector2(1, gradient);

        approachSide = false;
        approachSide = GetSide(pointPerp);
    }

    bool GetSide(Vector2 p)
    {
        return (p.x - pointOnLine_1.x) * (pointOnLine_2.y - pointOnLine_1.y) > (p.y - pointOnLine_1.y) * (pointOnLine_2.x - pointOnLine_1.x);
    }

    public bool HasCrossedLine(Vector2 p)
    {
        return GetSide(p) != approachSide;
    }

    public float DistanceFromPoint(Vector2 point)
    {
        float y_InterceptPerp = point.y - gradientPerp * point.x;
        float IntersectX = (y_InterceptPerp - yIntercept) / (gradient - gradientPerp);
        float IntersectY = gradient * IntersectX + yIntercept;
        return Vector2.Distance(point, new Vector2(IntersectX, IntersectY));
    }

    public void DrawWithGizmos(float length)
    {
        Vector3 lineDir = new Vector3(1, 0, gradient).normalized;
        Vector3 lineCenter = new Vector3(pointOnLine_1.x, 0, pointOnLine_1.y) + Vector3.up;
        Gizmos.DrawLine(lineCenter - lineDir * length / 2f, lineCenter + lineDir * length / 2f);
    }
}
