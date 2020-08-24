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
            public bool DrawGizmos = false;

            Collider objCollider;

            Vector3[] corners = new Vector3[4];

            /*public */
            LayerMask dimension = 0;

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

            bool exists;

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
                objCollider = gameObject.GetComponent<Collider>();

                dimension = gameObject.layer;

                objCollider.enabled = true;

                GetCorners();
                CheckForBlockedTerrain();//DoesBlockTerrain(true);
            }

            override protected void OnDestroy()
            {
                if (exists)
                    DoesBlockTerrain(false);
            }

            protected void GetCorners()
            {
                //Get the starting rotation and set the object to not rotated
                Quaternion startRot = transform.rotation;
                float radians = -startRot.eulerAngles.y * Mathf.Deg2Rad;
                transform.rotation = Quaternion.identity;
                Vector3 extent = objCollider.bounds.extents;

                //Get each un-rotated corner
                corners[0] = new Vector3(extent.x, 0, extent.z);
                corners[1] = new Vector3(extent.x, 0, -extent.z);
                corners[2] = new Vector3(-extent.x, 0, extent.z);
                corners[3] = new Vector3(-extent.x, 0, -extent.z);

                //Rotate each corner and move them outward by 1 tile
                for (int i = 0; i < corners.Length; ++i)
                {
                    float oldX = corners[i].x;
                    float oldZ = corners[i].z;

                    corners[i].x = oldX * Mathf.Cos(radians) - oldZ * Mathf.Sin(radians);
                    corners[i].z = oldX * Mathf.Sin(radians) + oldZ * Mathf.Cos(radians);

                    corners[i] += transform.position;
                }

                float minX = float.MaxValue;
                float minY = float.MaxValue;
                float maxX = float.MinValue;
                float maxY = float.MinValue;

                foreach (Vector3 v in corners)
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
                Grid gDimension = Grid.GetGrid;

                if (!gDimension.AreaHasObstacle((char)(dimension - 8), m_BottomLeft, m_TopRight))
                    DoesBlockTerrain(true);
                else
                    Destroy(gameObject);
            }

            protected void DoesBlockTerrain(bool blocksTerrain)
            {
                //Grid gDimension = DimensionManager.GetGridOfDimension(dimension);
                Grid gDimension = Grid.GetGrid;

                gDimension.ModifyBlockage((char)(dimension - 8), !blocksTerrain, m_BottomLeft, m_TopRight);

                exists = blocksTerrain;
            }

            private void OnDrawGizmos()
            {
                if (DrawGizmos)
                {
                    Gizmos.color = Color.blue;

                    foreach (Vector3 v in corners)
                        Gizmos.DrawSphere(v, 0.5f);
                }
            }
        }

    }
}