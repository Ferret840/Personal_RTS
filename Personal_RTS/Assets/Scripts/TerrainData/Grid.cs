using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Tools;
using System.Runtime.InteropServices;
using System;

namespace TerrainData
{

    public class Grid : MonoBehaviour
    {
        IntPtr m_GridPtr;

        public bool m_DrawDebuglines = false;
        public int m_DebugBoxSize = 50;

        Selectable.Units.Unit[] m_Units;

        public static CustomLogger m_s_Logger = new CustomLogger(@"..\Personal_RTS\Assets\Logs\SectorLog.log");

        public static Grid Instance
        {
            get;
            private set;
        }

        Grid()
        {
            if (Instance != null)
            {
                if (Instance.m_GridPtr != IntPtr.Zero)
                    NativeMethods.DestroyGrid_s(Instance.m_GridPtr);
                Destroy(Instance);
            }

            Instance = this;
        }

        //public LayerMask unwalkableMask;
        public Vector2 m_GridWorldSize;
        public float m_NodeRadius;
        public int m_SectorSize = 10;

        int m_PrevSectorSize;
        Vector2 m_PrevWorldSize;
        float m_PrevNodeRadius;

        public LayerMask m_Dim1;
        public LayerMask m_Dim2;
        public LayerMask m_Dim3;

        LayerMask m_View;

        private void Awake()
        {
            m_GridPtr = NativeMethods.NewGrid_s(m_NodeRadius, m_GridWorldSize.x, m_GridWorldSize.y, m_SectorSize);

            m_PrevSectorSize = m_SectorSize;
            m_PrevWorldSize = m_GridWorldSize;
            m_PrevNodeRadius = m_NodeRadius;

            m_View = m_Dim1;
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                m_View = m_Dim1;
            if (Input.GetKeyDown(KeyCode.Alpha2))
                m_View = m_Dim2;
            if (Input.GetKeyDown(KeyCode.Alpha3))
                m_View = m_Dim3;

            if (m_PrevSectorSize != m_SectorSize)
            {
                NativeMethods.ChangeSectorSize_s(m_SectorSize);
                m_PrevSectorSize = m_SectorSize;
            }
            if (m_PrevWorldSize != m_GridWorldSize)
            {
                NativeMethods.ChangeWorldSize_s(m_GridWorldSize.x, m_GridWorldSize.y);
                m_PrevWorldSize = m_GridWorldSize;
            }
            if (m_PrevNodeRadius != m_NodeRadius)
            {
                NativeMethods.ChangeNodeRadius_s(m_NodeRadius);
                m_PrevNodeRadius = m_NodeRadius;
            }
        }

        public bool AreaHasObstacle(char _dimension, Vector3 _bottomLeft, Vector3 _topRight)
        {
            return NativeMethods.AreaHasObstacle_s(_dimension, _bottomLeft.x, _bottomLeft.y, _bottomLeft.z, _topRight.x, _topRight.y, _topRight.z);
        }

        public bool ModifyBlockage(char _dimension, bool _blocksTerrain, Vector3 _bottomLeft, Vector3 _topRight)
        {
            return NativeMethods.ModifyBlockage_s(_dimension, _blocksTerrain, _bottomLeft.x, _bottomLeft.y, _bottomLeft.z, _topRight.x, _topRight.y, _topRight.z);
        }

        void OnDrawGizmos()
        {
            if (m_DrawDebuglines)
            {
                if (m_Units == null)
                    m_Units = FindObjectsOfType<Selectable.Units.Unit>();

                Pathing.Goal g = null;

                foreach (Selectable.Units.Unit u in m_Units)
                {
                    if (u.TargetGoal != null)
                    {
                        g = u.TargetGoal;
                        break;
                    }
                }

                if (g != null)
                {
                    RaycastHit hit;
                    Camera cam = Camera.main;
                    Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out hit, 1024f, TerrainData.Layers.m_s_Default))
                    {
                        Vector3 mouseCenter = hit.point + Vector3.up;
                        Debug.Log("Angle: " + g.GetDirFromPosition(mouseCenter));

                        for (float x = -m_DebugBoxSize * m_NodeRadius; x < m_DebugBoxSize * m_NodeRadius; x += (m_NodeRadius))
                        {
                            for (float y = -m_DebugBoxSize * m_NodeRadius; y < m_DebugBoxSize * m_NodeRadius; y += (m_NodeRadius))
                            {
                                Vector3 at = mouseCenter + new Vector3(x, 0, y);
                                float angleDir = g.GetDirFromPosition(at) * Mathf.Deg2Rad;
                                Gizmos.color = Color.red;
                                Gizmos.DrawLine(at, at + m_NodeRadius * new Vector3(Mathf.Sin(angleDir), 0, Mathf.Cos(angleDir)));
                                Gizmos.color = Color.blue;
                                Gizmos.DrawWireSphere(at, m_NodeRadius / 4);
                            }
                        }
                    }
                }
            }
        }
    }

    internal static class NativeMethods
    {
        [DllImport("RTS_DLL", EntryPoint = "NewGrid")]
        public static extern IntPtr NewGrid_s(float _nodeRadius = 0.1f, float _worldSizeX = 100.0f, float _worldSizeY = 100.0f, int _sectorSize = 10);
        [DllImport("RTS_DLL", EntryPoint = "DestroyGrid")]
        public static extern void DestroyGrid_s(IntPtr _pGrid);

        [DllImport("RTS_DLL", EntryPoint = "GetGrid")]
        public static extern IntPtr GetGridInstance_s();

        [DllImport("RTS_DLL", EntryPoint = "ChangeSectorSize")]
        public static extern void ChangeSectorSize_s(int _newSize);
        [DllImport("RTS_DLL", EntryPoint = "ChangeWorldSize")]
        public static extern void ChangeWorldSize_s(float _x, float _y);
        [DllImport("RTS_DLL", EntryPoint = "ChangeNodeRadius")]
        public static extern void ChangeNodeRadius_s(float _newRadius);

        [DllImport("RTS_DLL", EntryPoint = "ModifyBlockage")]
        public static extern bool ModifyBlockage_s(char _dimension, bool _isWalkable, float _blX, float _blY, float _blZ, float _trX, float _trY, float _trZ);
        [DllImport("RTS_DLL", EntryPoint = "AreaHasObstacle")]
        public static extern bool AreaHasObstacle_s(char _dimension, float _blX, float _blY, float _blZ, float _trX, float _trY, float _trZ);
    }
}
