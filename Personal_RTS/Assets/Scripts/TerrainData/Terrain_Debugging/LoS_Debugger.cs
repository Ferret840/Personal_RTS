using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class LoS_Debugger : MonoBehaviour
{
    [SerializeField]
    GameObject m_PointA;
    [SerializeField]
    GameObject m_PointB;

    public enum eLoSType
    {
        Linear,
        GridWalk,

        EnumCount
    }

    public eLoSType m_CalculationType;
    
    public int m_StressTestLoops = 0;

    // Start is called before the first frame update
    void Start()
    {
        m_PointA = GameObject.CreatePrimitive(PrimitiveType.Cube);
        m_PointA.transform.position   = Vector3.one;
        m_PointA.transform.localScale = Vector3.one * TerrainData.TerrainGrid.Instance.m_NodeRadius * 2.0f;

        m_PointB = GameObject.CreatePrimitive(PrimitiveType.Cube);
        m_PointB.transform.position   = Vector3.one * TerrainData.TerrainGrid.Instance.m_NodeRadius * 4.0f;
        m_PointB.transform.localScale = Vector3.one * TerrainData.TerrainGrid.Instance.m_NodeRadius * 2.0f;
    }

    // Update is called once per frame
    void Update()
    {
        //Loop to stress test
        for (int i = 0; i < m_StressTestLoops; ++i)
        {
            switch (m_CalculationType)
            {
                case eLoSType.Linear:
                    LoS_Calculator.LinearInterpolation_s(m_PointA.transform.position, m_PointB.transform.position);
                    break;
                case eLoSType.GridWalk:
                    LoS_Calculator.GridWalkInterpolation_s(m_PointA.transform.position, m_PointB.transform.position);
                    break;
                default:
                    break;
            }
        }
    }

    void OnDrawGizmos()
    {
        if (m_CalculationType != eLoSType.EnumCount)
        {
            if (m_PointA == null || m_PointB == null)
            {
                return;
            }

            List<Vector3> interpPoints;

            switch(m_CalculationType)
            {
                case eLoSType.Linear:
                    interpPoints = LoS_Calculator.LinearInterpolation_s(m_PointA.transform.position, m_PointB.transform.position);
                    break;
                case eLoSType.GridWalk:
                    interpPoints = LoS_Calculator.GridWalkInterpolation_s(m_PointA.transform.position, m_PointB.transform.position);
                    break;
            default:
                    interpPoints = new List<Vector3>();
                    break;
        }

        foreach (var point in interpPoints)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(point, Vector3.one * TerrainData.TerrainGrid.Instance.m_NodeRadius * 2);
            }
        }
    }
}
