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
        [DllImport("RTS_DLL", EntryPoint = "NewGrid")]
        public static extern IntPtr NewGrid(float _nodeRadius = 0.1f, float _worldSizeX = 100.0f, float _worldSizeY = 100.0f, int _sectorSize = 10);
        [DllImport("RTS_DLL", EntryPoint = "DestroyGrid")]
        static extern void DestroyGrid(IntPtr pGrid);

        [DllImport("RTS_DLL", EntryPoint = "GetGrid")]
        static extern IntPtr GetGridInstance();

        [DllImport("RTS_DLL", EntryPoint = "ChangeSectorSize")]
        static extern void ChangeSectorSize(int newSize);
        [DllImport("RTS_DLL", EntryPoint = "ChangeWorldSize")]
        static extern void ChangeWorldSize(float x, float y);
        [DllImport("RTS_DLL", EntryPoint = "ChangeNodeRadius")]
        static extern void ChangeNodeRadius(float newRadius);

        [DllImport("RTS_DLL", EntryPoint = "ModifyBlockage")]
        static extern bool ModifyBlockage(char dimension, bool isWalkable, float blX, float blY, float blZ, float trX, float trY, float trZ);
        [DllImport("RTS_DLL", EntryPoint = "AreaHasObstacle")]
        static extern bool AreaHasObstacle(char dimension, float blX, float blY, float blZ, float trX, float trY, float trZ);

        IntPtr gridPtr;

        public bool DrawDebuglines = false;
        public int DebugBoxSize = 50;

        Selectable.Units.Unit[] units;

        Grid()
        {
            if (grid_Instance != null)
                if(grid_Instance.gridPtr != IntPtr.Zero)
                    DestroyGrid(grid_Instance.gridPtr);

            grid_Instance = this;
        }

        public static CustomLogger logger = new CustomLogger(@"..\Personal_RTS\Assets\Logs\SectorLog.log");

        public static Grid GetGrid
        {
            get
            {
                return grid_Instance;
            }
        }

        static Grid grid_Instance;

        //public LayerMask unwalkableMask;
        public Vector2 gridWorldSize;
        public float nodeRadius;
        public int SectorSize = 10;

        int prevSectorSize;
        Vector2 prevWorldSize;
        float prevNodeRadius;

        public LayerMask dim1;
        public LayerMask dim2;
        public LayerMask dim3;

        LayerMask view;

        private void Awake()
        {
            gridPtr = NewGrid(nodeRadius, gridWorldSize.x, gridWorldSize.y, SectorSize);

            prevSectorSize = SectorSize;
            prevWorldSize = gridWorldSize;
            prevNodeRadius = nodeRadius;

            view = dim1;
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                view = dim1;
            if (Input.GetKeyDown(KeyCode.Alpha2))
                view = dim2;
            if (Input.GetKeyDown(KeyCode.Alpha3))
                view = dim3;

            if (prevSectorSize != SectorSize)
            {
                ChangeSectorSize(SectorSize);
                prevSectorSize = SectorSize;
            }
            if (prevWorldSize != gridWorldSize)
            {
                ChangeWorldSize(gridWorldSize.x, gridWorldSize.y);
                prevWorldSize = gridWorldSize;
            }
            if (prevNodeRadius != nodeRadius)
            {
                ChangeNodeRadius(nodeRadius);
                prevNodeRadius = nodeRadius;
            }
        }

        public bool AreaHasObstacle(char _dimension, Vector3 _BottomLeft, Vector3 _TopRight)
        {
            return AreaHasObstacle(_dimension, _BottomLeft.x, _BottomLeft.y, _BottomLeft.z, _TopRight.x, _TopRight.y, _TopRight.z);
        }

        public bool ModifyBlockage(char _dimension, bool _blocksTerrain, Vector3 _BottomLeft, Vector3 _TopRight)
        {
            return ModifyBlockage(_dimension, _blocksTerrain, _BottomLeft.x, _BottomLeft.y, _BottomLeft.z, _TopRight.x, _TopRight.y, _TopRight.z);
        }

        void OnDrawGizmos()
        {
            if (DrawDebuglines)
            {
                if (units == null)
                    units = FindObjectsOfType<Selectable.Units.Unit>();

                Pathing.Goal g = null;

                foreach (Selectable.Units.Unit u in units)
                {
                    if (u.TargetGoal != null)
                    {
                        g = u.TargetGoal;
                        break;
                    }
                }

                if(g != null)
                {
                    RaycastHit hit;
                    Camera cam = Camera.main;
                    Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out hit, 1024f, TerrainData.Layers.Default))
                    {
                        Vector3 mouseCenter = hit.point + Vector3.up;
                        Debug.Log("Angle: " + g.GetDirFromPosition(mouseCenter));

                        for (float x = -DebugBoxSize * nodeRadius; x < DebugBoxSize * nodeRadius; x += (nodeRadius))
                        {
                            for (float y = -DebugBoxSize * nodeRadius; y < DebugBoxSize * nodeRadius; y += (nodeRadius))
                            {
                                Vector3 at = mouseCenter + new Vector3(x, 0, y);
                                float angleDir = g.GetDirFromPosition(at) * Mathf.Deg2Rad;
                                Gizmos.color = Color.red;
                                Gizmos.DrawLine(at, at + nodeRadius * new Vector3(Mathf.Sin(angleDir), 0, Mathf.Cos(angleDir)));
                                Gizmos.color = Color.blue;
                                Gizmos.DrawWireSphere(at, nodeRadius / 4);
                            }
                        }
                    }
                }
            }
        }
    }
}
