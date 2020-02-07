using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tools;

namespace TerrainData
{

    public class Sector
    {
        public Color sectorColor
        {
            get;
            private set;
        }
    
        //Table of node locations. null = impassible, not-null = walkable
        public SectorNode[,] grid
        {
            get;
            private set;
        }
        int xSize, ySize;
    
        public Vector3 Position
        {
            get;
            private set;
        }
        int dimension, sectorX, sectorY;
    
        public List<SubSector> subs
        {
            get;
            private set;
        }
    
        public Sector(LayerMask dim, int _xSize, int _ySize, Vector3 cornerPos, float nodeDiameter, int _sectorX, int _sectorY)
        {
            xSize = Mathf.RoundToInt(_xSize / nodeDiameter);
            ySize = Mathf.RoundToInt(_ySize / nodeDiameter);
    
            Color c = Random.ColorHSV();
            c.a = 0.5f;
            sectorColor = c;
    
            Position = cornerPos;
            sectorX = _sectorX;
            sectorY = _sectorY;
    
            dimension = (int)Mathf.Log(dim, 2) - 8;
    
            grid = new SectorNode[xSize, ySize];
    
            for (int x = 0; x < xSize; ++x)
            {
                for (int y = 0; y < ySize; ++y)
                {
                    //grid[x, y] = new Node(true);
                    bool walkable = true;
    
                    Vector3 worldPoint = cornerPos + Vector3.right * (x * nodeDiameter) + Vector3.forward * (y * nodeDiameter);
    
                    Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
    
                    //Dimension terrain block detection
                    RaycastHit[] hits = Physics.RaycastAll(ray, 100, dim);
                    foreach (RaycastHit h in hits)
                    {
                        if (h.transform.gameObject.tag == "Unit")
                            continue;
    
                        //grid[x, y].SetWalkable(false);
                        walkable = false;
                        break;
                    }
                    if (walkable/*grid[x, y].isWalkable*/ && (dim != 1 << 8 || dim != 1 << 9))
                    {
                        hits = Physics.RaycastAll(ray, 100, 1 << 10);
                        foreach (RaycastHit h in hits)
                        {
                            if (h.transform.gameObject.tag == "Unit")
                                continue;
    
                            //grid[x, y].SetWalkable(false);
                            walkable = false;
                            break;
                        }
                    }
    
                    if (walkable)
                    {
                        grid[x, y] = new SectorNode();
                    }
                }
            }
    
            UpdateSubsectors();
        }
    
        public void UpdateSubsectors()
        {
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();
    
            foreach (SectorNode n in grid)
                if (n != null)
                    n.Subsector = null;
    
            subs = new List<SubSector>();
    
            //HashSet<Node> touchedNodes = new HashSet<Node>();
            Stack<SectorNodeWithCoord> neighborGettorList = new Stack<SectorNodeWithCoord>();
    
            for (int x = 0; x < xSize; ++x)
            {
                for (int y = 0; y < ySize; ++y)
                {
                    SectorNode n = grid[x, y];
                    if (n != null && n.Subsector == null)//!touchedNodes.Contains(n))
                    {
                        SubSector s = new SubSector(this);
                        subs.Add(s);
    
                        //neighborGettorList.Add(new NodeWithCoord(n, x, y));
                        neighborGettorList.Push(new SectorNodeWithCoord(n, x, y));
    
                        while (neighborGettorList.Count > 0)
                        {
                            SectorNodeWithCoord nodeCoord = neighborGettorList.Pop();//[neighborGettorList.Count - 1];
                                                                               //touchedNodes.Add(nodeCoord.node);
                            s.AddNodeToSub(nodeCoord.Node);
    
                            SectorNode neighbor;
                            int nextX = nodeCoord.X - 1;
                            int nextY = nodeCoord.Y;
    
                            //Get each of the 4 neighbors (a + shape)
                            if (nextX >= 0)
                            {
                                neighbor = grid[nextX, nextY];
                                if (neighbor != null && neighbor.Subsector == null)//!touchedNodes.Contains(neighbor))
                                {
                                    neighborGettorList.Push(new SectorNodeWithCoord(neighbor, nextX, nextY));
                                    //touchedNodes.Add(neighbor);
                                }
                            }
    
                            nextX += 2;
                            if (nextX < xSize)
                            {
                                neighbor = grid[nextX, nextY];
                                if (neighbor != null && neighbor.Subsector == null)//!touchedNodes.Contains(neighbor))
                                {
                                    neighborGettorList.Push(new SectorNodeWithCoord(neighbor, nextX, nextY));
                                    //touchedNodes.Add(neighbor);
                                }
                            }
    
                            --nextX;
                            --nextY;
                            if (nextY >= 0)
                            {
                                neighbor = grid[nextX, nextY];
                                if (neighbor != null && neighbor.Subsector == null)//!touchedNodes.Contains(neighbor))
                                {
                                    neighborGettorList.Push(new SectorNodeWithCoord(neighbor, nextX, nextY));
                                    //touchedNodes.Add(neighbor);
                                }
                            }
    
                            nextY += 2;
                            if (nextY < ySize)
                            {
                                neighbor = grid[nextX, nextY];
                                if (neighbor != null && neighbor.Subsector == null)//!touchedNodes.Contains(neighbor))
                                {
                                    neighborGettorList.Push(new SectorNodeWithCoord(neighbor, nextX, nextY));
                                    //touchedNodes.Add(neighbor);
                                }
                            }
                        }
                    }
                }
            }
    
            //subs.Add(new SubSector(this));
            //
            //for (int x = 0; x < xSize; ++x)
            //{
            //    for (int y = 0; y < ySize; ++y)
            //    {
            //        if (grid[x,y] != null)//grid[x, y].isWalkable)
            //            subs[0].AddNodeToSub(grid[x, y]);
            //    }
            //}
    
            //foreach(SubSector s in subs)
            //    s.GenerateMesh();
    
            //Debug.Log(string.Format("Generated Subsectors for Sector {0} in {1}ms", this.ToString(), timer.ElapsedMilliseconds));
            timer.Stop();
        }
    
        public void UpdateMeshes()
        {
            foreach (SubSector s in subs)
            {
                s.SetMeshValues();
            }
        }
    
        public bool GetWalkableAt(int x, int y)
        {
            return grid[x, y] != null;//.isWalkable;
        }
    
        public bool SetWalkableAt(int x, int y, bool newCanWalk)
        {
            if (newCanWalk && grid[x, y] == null)
                grid[x, y] = new SectorNode();
            else if (!newCanWalk)
                grid[x, y] = null;
            return newCanWalk;//grid[x, y].SetWalkable(newCanWalk);
        }
    
        //Subsector Class
        public class SubSector
        {
            public Sector sect
            {
                get;
                private set;
            }
    
            public HashSet<SubSector> ConnectedSectors = new HashSet<SubSector>();
    
            public Mesh mesh
            {
                get;
                private set;
            }
    
            //List<Vector3> verts = new List<Vector3>();
            Vector3[] vertices;
            int[] indices;
    
            SectorNode[,] grid
            {
                get
                {
                    return sect.grid;
                }
            }
    
            public Color subSectorColor
            {
                get;
                private set;
            }
    
            public SubSector(Sector parentSector)
            {
                sect = parentSector;
    
                //Color c = Random.ColorHSV(0.2f, 0.8f, 0.1f, 0.5f, 0.5f, 1f);
                ////c.a = 0.5f;
                //subSectorColor = c;
            }
    
            public void AddNodeToSub(SectorNode n)
            {
                //n.subsectorInstance = SubsectorIdentity;
                n.Subsector = this;
                //NodesInSubsector.Add(n);
            }
    
            public void UpdateConnectedSubsectors()
            {
                ConnectedSectors.Clear();
    
                Grid g = Grid.GetGrid;
    
                Sector s;
    
                if (sect.sectorY - 1 >= 0)
                {
                    s = g.GridSectors[sect.dimension, sect.sectorX, sect.sectorY - 1];
    
                    for (int x = 0; x < sect.xSize; ++x)
                    {
                        FindConnectedSubsectors(s, x, s.ySize - 1, x, 0);
                    }
                }
    
                if (sect.sectorY + 1 < Grid.GetGrid.ySectorCount)
                {
                    s = g.GridSectors[sect.dimension, sect.sectorX, sect.sectorY + 1];
    
                    for (int x = 0; x < sect.xSize; ++x)
                    {
                        FindConnectedSubsectors(s, x, 0, x, sect.ySize - 1);
                    }
                }
    
                if (sect.sectorX - 1 >= 0)
                {
                    s = g.GridSectors[sect.dimension, sect.sectorX - 1, sect.sectorY];
    
                    for (int y = 0; y < sect.ySize; ++y)
                    {
                        FindConnectedSubsectors(s, s.xSize - 1, y, 0, y);
                    }
                }
    
                if (sect.sectorX + 1 < Grid.GetGrid.xSectorCount)
                {
                    s = g.GridSectors[sect.dimension, sect.sectorX + 1, sect.sectorY];
    
                    for (int y = 0; y < sect.ySize; ++y)
                    {
                        FindConnectedSubsectors(s, 0, y, sect.xSize - 1, y);
                    }
                }
            }
    
            void FindConnectedSubsectors(Sector s, int otherX, int otherY, int selfX, int selfY)
            {
                if (s.grid[otherX, otherY] == null || grid[selfX, selfY] == null)
                    return;
    
                if (grid[selfX, selfY].Subsector == this)
                {
                    ConnectedSectors.Add(s.grid[otherX, otherY].Subsector);
                    //s.grid[otherX, otherY].subsector.ConnectedSectors.Add(this);
                }
            }
    
#if UNITY_EDITOR
            public void GenerateMesh()
            {
    
                //if (NodesInSubsector.Count <= 1)
                //    try
                //    {
                //        mesh = Resources.Load<GameObject>("CubeMesh").GetComponent<MeshFilter>().sharedMesh;
                //    }
                //    catch { return; }
                //else
                //    mesh = new Mesh();
    
                int x = 0, y = 0;
                bool broke = false;
                for (; x < sect.xSize; ++x)
                {
                    for (y = 0; y < sect.ySize; ++y)
                    {
                        if (grid[x, y] != null && grid[x, y].Subsector == this)//NodesInSubsector.Contains(grid[x, y]))
                        {
                            broke = true;
                            break;
                        }
                    }
                    if (broke)
                        break;
                }
    
                if (broke == false)
                {
                    Debug.Log(string.Format("Error: Subsector {0} contains no nodes", this.ToString()));
                    return;
                }
    
                try
                {
                    List<Vector2> verts = CheckNext(x, y, 1, 1, false);
                    verts = RemoveRedundantVerts(verts);
    
                    CalcMesh(verts);
                }
                catch (SubsectorMeshException)
                {
                    //List<Vector2> verts = new List<Vector2>();
                    //verts.Add(new Vector2(x, y));
                    //verts.Clear();
                    //verts.Add(Vector2.zero);
                    //verts.Add(new Vector2(0, sect.ySize));
                    //verts.Add(new Vector2(sect.xSize, sect.ySize));
                    //verts.Add(new Vector2(sect.xSize, 0));
                    //
                    //CalcMesh(verts);
                    //
                    //Debug.Log(e.Message);
                    //
                    //subSectorColor = Color.red;
                }
    
                //verts = null;
            }
    
            void CalcMesh(List<Vector2> verts)
            {
                Triangulator tr = new Triangulator(verts);
                /*int[]*/
                indices = tr.Triangulate();
    
                // Create the Vector3 vertices
                /*Vector3[]*/
                vertices = new Vector3[verts.Count];
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] = new Vector3(verts[i].x, verts[i].y, 0);
                }
            }
    
            public void SetMeshValues()
            {
                if (subSectorColor != Color.red)
                {
                    Color c = Random.ColorHSV(0.2f, 0.8f, 0.1f, 0.5f, 0.5f, 1f);
                    //c.a = 0.5f;
                    subSectorColor = c;
                }
    
                mesh = new Mesh();
                mesh.vertices = vertices;
                mesh.triangles = indices;
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
    
                if (mesh.normals[0] == Vector3.zero)
                {
                    ReverseNormals();
                }
            }
    
            List<Vector2> RemoveRedundantVerts(List<Vector2> verts)
            {
                for (int x = 1; x < verts.Count - 1; ++x)
                {
                    while (x < verts.Count - 1)
                    {
                        if (Mathf.Abs(verts[x].x - verts[x + 1].x) == 1f && Mathf.Abs(verts[x].y - verts[x + 1].y) == 1f)
                            verts.RemoveAt(x);
                        else
                            break;
                    }
                }
                for (int x = 1; x < verts.Count - 1; ++x)
                {
                    while (x < verts.Count - 1)
                    {
                        if ((verts[x - 1].x == verts[x].x && verts[x].x == verts[x + 1].x) ||
                            (verts[x - 1].y == verts[x].y && verts[x].y == verts[x + 1].y))
                            verts.RemoveAt(x);
                        else
                            break;
                    }
                }
                //if (verts.Count >= 3 &&
                //    ((verts[0].x == verts[verts.Count - 1].x && verts[verts.Count - 1].x == verts[verts.Count - 2].x) ||
                //     (verts[0].x == verts[verts.Count - 1].y && verts[verts.Count - 1].y == verts[verts.Count - 2].y))    )
                if (verts[0] == verts[verts.Count - 1])
                {
                    verts.RemoveAt(verts.Count - 1);
                }
    
                return verts;
            }
    
            //Extreme logic
            List<Vector2> CheckNext(int x, int y, int xN, int yM, bool success)
            {
                List<Vector2> verts = new List<Vector2>();
                verts.Add(new Vector2(x, y));
    
                System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
                timer.Start();
                //In the case of a single node, ensure it doesn't loop forever in the same spot
                int sameLimit = 0;
                while (sameLimit < 4)
                {
                    //if (timer.ElapsedMilliseconds > 1500)
                    //    throw new SubsectorMeshException();
    
                    int nextX = x + xN, nextY = y + yM;
    
                    Vector2 nextVert = new Vector2(nextX, nextY) + new Vector2(0.5f * -yM, 0.5f * xN) + Vector2.one * 0.5f;
                    Vector2 nextVertNext = new Vector2(nextX, nextY) + new Vector2(0.5f * xN, 0.5f * yM) + Vector2.one * 0.5f;
    
                    ++sameLimit;
    
                    //Checking vertically
                    if (xN == yM)
                    {
                        nextVert.x -= xN;
                        nextVertNext.x -= xN;
    
                        xN = 0;
                        nextX = x;
    
                        //Check for top and bottom bounds
                        if (nextY < 0)
                        {
                            xN = -1;
                            yM = 1;
                            //CheckNext(x, y, -1, 1);
                            success = false;
                            continue;
                        }
                        else if (nextY >= sect.ySize)
                        {
                            xN = 1;
                            yM = -1;
                            //CheckNext(x, y, 1, -1);
                            success = false;
                            continue;
                        }
                    }
                    //Checking horizontally
                    else
                    {
                        nextVert.y -= yM;
                        nextVertNext.y -= yM;
    
                        yM = 0;
                        nextY = y;
    
                        //Check for left and right bounds
                        if (nextX < 0)
                        {
                            xN = 1;
                            yM = 1;
                            //CheckNext(x, y, 1, 1);
                            success = false;
                            continue;
                        }
                        else if (nextX >= sect.xSize)
                        {
                            xN = -1;
                            yM = -1;
                            //CheckNext(x, y, -1, -1);
                            success = false;
                            continue;
                        }
                    }
    
                    //Get node to check
                    SectorNode n = grid[nextX, nextY];
    
                    //If this isn't in the subsector
                    if (n == null)// || n.subsectorInstance != SubsectorIdentity)//!NodesInSubsector.Contains(n))
                    {
                        if (xN == 0)
                        {
                            yM = -(xN = yM);
                        }
                        else
                        {
                            yM = xN = -xN;
                        }
    
                        //Check next clockwise for this same node as origin
                        success = false;
                        continue;
                        //CheckNext(x, y, xN, yM);
                    }
                    else
                    {
    
    
                        if (success)
                            verts[verts.Count - 1] = (nextVert);
                        else
                        {
                            verts.Add(nextVert);
                        }
    
                        if (verts[0] == nextVert || verts[0] == nextVertNext)
                            return verts;
                        else if (verts[0] == nextVertNext)
                        {
                            //    verts.Add(nextVertNext);
                            return verts;
                        }
                        else
                            verts.Add(nextVertNext);
    
                        if (xN == 0)
                        {
                            xN = -yM;
                        }
                        else
                        {
                            yM = xN;
                        }
    
                        //Check next counter-clockwise
                        x = nextX;
                        y = nextY;
                        sameLimit = 0;
                        success = true;
                        continue;
                        //CheckNext(nextX, nextY, xN, yM);
                    }
                }
    
                if (sameLimit >= 4 && verts.Count < 4)
                {
                    verts.Add(new Vector2(verts[0].x, verts[0].y + 1f));
                    verts.Add(new Vector2(verts[0].x + 1f, verts[0].y + 1f));
                    verts.Add(new Vector2(verts[0].x + 1f, verts[0].y));
                }
                return verts;
            }
    
            void ReverseNormals()
            {
                Vector3[] normals = mesh.normals;
                for (int i = 0; i < normals.Length; i++)
                    normals[i] = -normals[i];
                mesh.normals = normals;
    
                for (int m = 0; m < mesh.subMeshCount; m++)
                {
                    int[] triangles = mesh.GetTriangles(m);
                    for (int i = 0; i < triangles.Length; i += 3)
                    {
                        int temp = triangles[i + 0];
                        triangles[i + 0] = triangles[i + 1];
                        triangles[i + 1] = temp;
                    }
                    mesh.SetTriangles(triangles, m);
                }
            }
#endif
        }
    
        struct SectorNodeWithCoord
        {
            public SectorNode Node;
            public int X;
            public int Y;
    
            public SectorNodeWithCoord(SectorNode n, int _x, int _y)
            {
                Node = n;
                X = _x;
                Y = _y;
            }
        }
    
        public class SectorNode
        {
            //public uint subsectorInstance = uint.MaxValue;
            public SubSector Subsector = null;
    
            ///<summary>
            ///Always 0, 1, 2, or 3. Set bits indicate walkable in that dimension
            ///</summary>
            /*public bool isWalkable
            {
                get;
                private set;
            }
    
            public Node(bool _isWalkable)
            {
                isWalkable = _isWalkable;
            }
    
            public bool SetWalkable(bool _newIsWalkable)
            {
                return isWalkable = _newIsWalkable;
            }*/
        }
    }

}