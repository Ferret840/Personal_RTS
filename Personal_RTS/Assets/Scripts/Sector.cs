using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sector
{
    static uint SubsectorIdentifier = 0;

    public Color sectorColor
    {
        get;
        private set;
    }

    //Table of node locations. null = impassible, not-null = walkable
    public Node[,] grid
    {
        get;
        private set;
    }
    int xSize, ySize;

    public List<SubSector> subs
    {
        get;
        private set;
    }

    public Sector(LayerMask dim, int _xSize, int _ySize, Vector3 cornerPos, float nodeDiameter)
    {
        xSize = Mathf.RoundToInt(_xSize / nodeDiameter);
        ySize = Mathf.RoundToInt(_ySize / nodeDiameter);

        Color c = Random.ColorHSV();
        c.a = 0.5f;
        sectorColor = c;

        grid = new Node[xSize, ySize];
        
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
                if (walkable/*grid[x, y].isWalkable*/ && (dim != 1 << 8 || dim == 1 << 9))
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
                    grid[x, y] = new Node();
                }
            }
        }

        UpdateSubsectors();
    }

    public void UpdateSubsectors()
    {
        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
        timer.Start();

        subs = new List<SubSector>();

        //HashSet<Node> touchedNodes = new HashSet<Node>();
        Stack<NodeWithCoord> neighborGettorList = new Stack<NodeWithCoord>();

        uint minNewSubsector = SubsectorIdentifier;

        for (int x = 0; x < xSize; ++x)
        {
            for (int y = 0; y < ySize; ++y)
            {
                Node n = grid[x, y];
                if (n != null && (n.subsectorInstance < minNewSubsector || n.subsectorInstance > SubsectorIdentifier))//!touchedNodes.Contains(n))
                {
                    SubSector s = new SubSector(this);
                    subs.Add(s);
                    s.SubsectorIdentity = SubsectorIdentifier++;

                    //neighborGettorList.Add(new NodeWithCoord(n, x, y));
                    neighborGettorList.Push(new NodeWithCoord(n, x, y));

                    while (neighborGettorList.Count > 0)
                    {
                        NodeWithCoord nodeCoord = neighborGettorList.Pop();//[neighborGettorList.Count - 1];
                        //touchedNodes.Add(nodeCoord.node);
                        s.AddNodeToSub(nodeCoord.node);

                        Node neighbor;
                        int nextX = nodeCoord.x - 1;
                        int nextY = nodeCoord.y;

                        //Get each of the 4 neighbors (a + shape)
                        if (nextX >= 0)
                        {
                            neighbor = grid[nextX, nextY];
                            if (neighbor != null && (neighbor.subsectorInstance < minNewSubsector || neighbor.subsectorInstance > SubsectorIdentifier))//!touchedNodes.Contains(neighbor))
                            {
                                neighborGettorList.Push(new NodeWithCoord(neighbor, nextX, nextY));
                                //touchedNodes.Add(neighbor);
                            }
                        }

                        nextX += 2;
                        if (nextX < xSize)
                        {
                            neighbor = grid[nextX, nextY];
                            if (neighbor != null && (neighbor.subsectorInstance < minNewSubsector || neighbor.subsectorInstance > SubsectorIdentifier))//!touchedNodes.Contains(neighbor))
                            {
                                neighborGettorList.Push(new NodeWithCoord(neighbor, nextX, nextY));
                                //touchedNodes.Add(neighbor);
                            }
                        }

                        --nextX;
                        --nextY;
                        if (nextY >= 0)
                        {
                            neighbor = grid[nextX, nextY];
                            if (neighbor != null && (neighbor.subsectorInstance < minNewSubsector || neighbor.subsectorInstance > SubsectorIdentifier))//!touchedNodes.Contains(neighbor))
                            {
                                neighborGettorList.Push(new NodeWithCoord(neighbor, nextX, nextY));
                                //touchedNodes.Add(neighbor);
                            }
                        }

                        nextY += 2;
                        if (nextY < ySize)
                        {
                            neighbor = grid[nextX, nextY];
                            if (neighbor != null && (neighbor.subsectorInstance < minNewSubsector || neighbor.subsectorInstance > SubsectorIdentifier))//!touchedNodes.Contains(neighbor))
                            {
                                neighborGettorList.Push(new NodeWithCoord(neighbor, nextX, nextY));
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

        foreach(SubSector s in subs)
            s.GenerateMesh();

        //Debug.Log(string.Format("Generated Subsectors for Sector {0} in {1}ms", this.ToString(), timer.ElapsedMilliseconds));
        timer.Stop();
    }

    public bool GetWalkableAt(int x, int y)
    {
        return grid[x, y] != null;//.isWalkable;
    }

    public bool SetWalkableAt(int x, int y, bool newCanWalk)
    {
        if (newCanWalk && grid[x, y] == null)
            grid[x, y] = new Node();
        else if (!newCanWalk)
            grid[x, y] = null;
        return newCanWalk;//grid[x, y].SetWalkable(newCanWalk);
    }

    //Subsector Class
    public class SubSector
    {
        Sector sect;
        public uint SubsectorIdentity;//HashSet<Node> NodesInSubsector = new HashSet<Node>();

        public Mesh mesh
        {
            get;
            private set;
        }

        List<Vector2> verts = new List<Vector2>();

        Node[,] grid
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

            Color c = Random.ColorHSV(0.2f, 0.8f, 0.1f, 0.5f, 0.5f, 1f);
            //c.a = 0.5f;
            subSectorColor = c;

            mesh = new Mesh();
        }

        public void AddNodeToSub(Node n)
        {
            n.subsectorInstance = SubsectorIdentity;
            //NodesInSubsector.Add(n);
        }

        public void GenerateMesh()
        {
            verts = new List<Vector2>();

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
                    if (grid[x, y] != null && grid[x, y].subsectorInstance == SubsectorIdentity)//NodesInSubsector.Contains(grid[x, y]))
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
                verts.Add(new Vector2(x, y));
                CheckNext(x, y, 1, 1, false);
                verts = RemoveRedundantVerts(verts);

                CalcMesh();
            }
            catch (SubsectorMeshException e)
            {
                verts.Clear();
                verts.Add(Vector2.zero);
                verts.Add(new Vector2(0, sect.ySize));
                verts.Add(new Vector2(sect.xSize, sect.ySize));
                verts.Add(new Vector2(sect.xSize, 0));

                CalcMesh();

                Debug.Log(e.Message);

                subSectorColor = Color.red;
            }
            
            verts = null;
        }

        void CalcMesh()
        {
            Triangulator tr = new Triangulator(verts);
            int[] indices = tr.Triangulate();

            // Create the Vector3 vertices
            Vector3[] vertices = new Vector3[verts.Count];
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new Vector3(verts[i].x, verts[i].y, 0);
            }

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
            if(verts[0] == verts[verts.Count-1])
            {
                verts.RemoveAt(verts.Count - 1);
            }

            return verts;
        }

        //Extreme logic
        void CheckNext(int x, int y, int xN, int yM, bool success)
        {
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
                Node n = grid[nextX, nextY];

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
                        return;
                    else if (verts[0] == nextVertNext)
                    {
                    //    verts.Add(nextVertNext);
                        return;
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
    }

    struct NodeWithCoord
    {
        public Node node;
        public int x;
        public int y;

        public NodeWithCoord(Node n, int _x, int _y)
        {
            node = n;
            x = _x;
            y = _y;
        }
    }
}
