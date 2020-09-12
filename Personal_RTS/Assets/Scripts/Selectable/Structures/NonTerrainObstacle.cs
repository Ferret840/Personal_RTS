using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TerrainData;

namespace Selectable
{
    namespace Structures
    {

        [RequireComponent(typeof(Collider))]
        public class NonTerrainObstacle : Owner
        {
            public bool m_DrawGizmos = false;

            Collider m_ObjCollider;

            Vector3[] m_Corners = new Vector3[4];

            /*public */
            LayerMask m_Dimension = 0;

            Vector3 m_BottomLeft, m_TopRight;
            public Vector3 BottomLeftCorner
            {
                get
                {
                    return m_BottomLeft;
                }
            }
            public Vector3 TopRightCorner
            {
                get
                {
                    return m_TopRight;
                }
            }

            bool m_Exists;

            //void Awake()
            //{
            //    
            //}

            // Use this for initialization
            void Start()
            {
                Init();
            }

            protected void Init()
            {
                UpdateMinimapLayer();

                //Get the collider
                m_ObjCollider = gameObject.GetComponent<Collider>();

                m_Dimension = gameObject.layer;

                m_ObjCollider.enabled = true;

                GetCorners();
                CheckForBlockedTerrain();//DoesBlockTerrain(true);

                SetSelectionSlicedMeshSizes(m_SelectedEffect.GetComponent<SlicedMesh>());
                SetSelectionSlicedMeshSizes(m_HighlightedEffect.GetComponent<SlicedMesh>());
                //SetChildGlobalScale(m_HighlightedEffect.transform,  Vector3.one);
                //SetChildGlobalScale(m_SelectedEffect.transform,     Vector3.one);
            }

            override protected void SetSelectionSlicedMeshSizes(SlicedMesh _sliced)
            {
                _sliced.BorderVertical = 0.5f / transform.lossyScale.z;
                _sliced.BorderHorizontal = 0.5f / transform.lossyScale.x;

                _sliced.Height = 1f + _sliced.BorderVertical   * 2f;
                _sliced.Width =  1f + _sliced.BorderHorizontal * 2f;

                _sliced.MarginVertical = 0.5f;
                _sliced.MarginHorizontal = 0.5f;
            }

            override protected void SetChildGlobalScale(Transform _transform, Vector3 _globalScale)
            {
                _transform.localScale = Vector3.one;
                _transform.localScale = new Vector3(_globalScale.x / transform.lossyScale.x, _globalScale.y / transform.lossyScale.y, _globalScale.z / transform.lossyScale.z);
            }

            override protected void OnDestroy()
            {
                if (m_Exists)
                    DoesBlockTerrain(false);
            }

            protected void GetCorners()
            {
                //Get the starting rotation and set the object to not rotated
                Quaternion startRot = transform.rotation;
                float radians = -startRot.eulerAngles.y * Mathf.Deg2Rad;
                transform.rotation = Quaternion.identity;
                Vector3 extent = m_ObjCollider.bounds.extents;

                //Get each un-rotated corner
                m_Corners[0] = new Vector3(extent.x, 0, extent.z);
                m_Corners[1] = new Vector3(extent.x, 0, -extent.z);
                m_Corners[2] = new Vector3(-extent.x, 0, extent.z);
                m_Corners[3] = new Vector3(-extent.x, 0, -extent.z);

                //Rotate each corner and move them outward by 1 tile
                for (int i = 0; i < m_Corners.Length; ++i)
                {
                    float oldX = m_Corners[i].x;
                    float oldZ = m_Corners[i].z;

                    m_Corners[i].x = oldX * Mathf.Cos(radians) - oldZ * Mathf.Sin(radians);
                    m_Corners[i].z = oldX * Mathf.Sin(radians) + oldZ * Mathf.Cos(radians);

                    m_Corners[i] += transform.position;
                }

                float minX = float.MaxValue;
                float minY = float.MaxValue;
                float maxX = float.MinValue;
                float maxY = float.MinValue;

                foreach (Vector3 v in m_Corners)
                {
                    if (v.x < minX)
                        minX = v.x;
                    if (v.x > maxX)
                        maxX = v.x;
                    if (v.z < minY)
                        minY = v.z;
                    if (v.z > maxY)
                        maxY = v.z;
                }

                m_BottomLeft = new Vector3(minX, 0, minY);
                m_TopRight = new Vector3(maxX, 0, maxY);

                transform.rotation = startRot;
            }

            protected void CheckForBlockedTerrain()
            {
                Grid gDimension = Grid.Instance;

                if (!gDimension.AreaHasObstacle((char)(m_Dimension - 8), m_BottomLeft, m_TopRight))
                    DoesBlockTerrain(true);
                else
                    Destroy(gameObject);
            }

            protected void DoesBlockTerrain(bool _blocksTerrain)
            {
                //Grid gDimension = DimensionManager.GetGridOfDimension(dimension);
                Grid gDimension = Grid.Instance;

                gDimension.ModifyBlockage((char)(m_Dimension - 8), !_blocksTerrain, m_BottomLeft, m_TopRight);

                m_Exists = _blocksTerrain;
            }

            private void OnDrawGizmos()
            {
                if (m_DrawGizmos)
                {
                    Gizmos.color = Color.blue;

                    foreach (Vector3 v in m_Corners)
                        Gizmos.DrawSphere(v, 0.5f);
                }
            }
        }

    }
}