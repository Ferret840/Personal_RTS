using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class LoS_Calculator
{
    static Vector3 RoundVec3_s(Vector3 _point)
    {
        return new Vector3(Mathf.Round(_point.x), Mathf.Round(_point.y), Mathf.Round(_point.z));
    }

    static public List<Vector3> LinearInterpolation_s(Vector3 _point1, Vector3 _point2)
    {
        //Make them level
        _point1.y = 0;
        _point2.y = 0;

        TerrainData.TerrainGrid terrainGrid = TerrainData.TerrainGrid.Instance;
        float nodeSize = terrainGrid.m_NodeRadius * 2.0f;

        List<Vector3> pointsList = new List<Vector3>();
        float numOfSteps = Vector3.Distance(_point2, _point1) / nodeSize;

        for (float step = 0; step < numOfSteps; step += nodeSize)
        {
            float t = step == 0 ? 0.0f : step / numOfSteps;

            //Multiply to factor in node size
            Vector3 newPoint = Vector3.Lerp(_point1, _point2, t) / nodeSize;
            Vector3 roundedPoint = RoundVec3_s(newPoint);
            //Divide by node size to get back to correct distance
            pointsList.Add(roundedPoint * nodeSize);
        }

        return pointsList;
    }

    static public List<Vector3> GridWalkInterpolation_s(Vector3 _point1, Vector3 _point2)
    {
        TerrainData.TerrainGrid terrainGrid = TerrainData.TerrainGrid.Instance;
        float nodeRadius = terrainGrid.m_NodeRadius;
        float nodeSize = terrainGrid.m_NodeRadius * 2.0f;

        _point1 = RoundVec3_s(_point1 / nodeSize) * nodeSize;
        _point2 = RoundVec3_s(_point2 / nodeSize) * nodeSize;

        Vector3 delta = (_point2 - _point1);
        Vector3 magDelta = new Vector3(Mathf.Abs(delta.x), Mathf.Abs(delta.y), Mathf.Abs(delta.z));
        Vector3 signDelta = new Vector3(delta.x > 0 ? 1 : -1, delta.y > 0 ? 1 : -1, delta.z > 0 ? 1 : -1) * nodeSize;

        Vector3 currPoint = _point1;
        List<Vector3> pointsList = new List<Vector3>();
        for (float iX = 0, iZ = 0; iX < magDelta.x || iZ < magDelta.z;)
        {
            if ((nodeRadius + iX) / magDelta.x < (nodeRadius + iZ) / magDelta.z)
            {
                currPoint.x += signDelta.x;
                iX += nodeSize;
            }
            else
            {
                currPoint.z += signDelta.z;
                iZ += nodeSize;
            }
            pointsList.Add(currPoint);
        }

        return pointsList;
    }
}
