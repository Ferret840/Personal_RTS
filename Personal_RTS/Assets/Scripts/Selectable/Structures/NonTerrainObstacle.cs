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

            //private void Awake()
            //{
            //    Grid gDimension = DimensionManager.GetGridOfDimension(dimension);
            //    transform.position = gDimension.NodeFromWorldPoint(transform.position).worldPosition;
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

                SetChildGlobalScale(m_HighlightedEffect.transform,  transform.lossyScale + Vector3.one * 0.25f);
                SetChildGlobalScale(m_SelectedEffect.transform,     transform.lossyScale + Vector3.one * 0.25f);
            }

            virtual protected void SetChildGlobalScale(Transform _transform, Vector3 _globalScale)
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