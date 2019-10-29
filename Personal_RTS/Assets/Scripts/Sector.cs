using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sector
{
    public Color sectorColor
    {
        get;
        private set;
    }

    //Table of node locations. 1 = Walkable, 0 = Impassable terrain
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
                if (walkable/*grid[x, y].isWalkable*/ && (dim == 1 << 8 || dim == 1 << 9))
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
        subs.Add(new SubSector(this));

        for (int x = 0; x < xSize; ++x)
        {
            for (int y = 0; y < ySize; ++y)
            {
                if (grid[x,y] != null)//grid[x, y].isWalkable)
                    subs[0].AddNodeToSub(grid[x, y]);
            }
        }

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
        HashSet<Node> NodesInSubsector = new HashSet<Node>();

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

            Color c = Random.ColorHSV();
            c.a = 0.5f;
            subSectorColor = c;
        }

        public void AddNodeToSub(Node n)
        {
            NodesInSubsector.Add(n);
        }

        public void GenerateMesh()
        {
            verts = new List<Vector2>();

            if (NodesInSubsector.Count <= 1)
                try
                {
                    mesh = Resources.Load<GameObject>("CubeMesh").GetComponent<MeshFilter>().sharedMesh;
                }
                catch { return; }
            else
                mesh = new Mesh();

            int x = 0, y = 0;
            bool broke = false;
            for (; x < sect.xSize; ++x)
            {
                for (y = 0; y < sect.ySize; ++y)
                {
                    if (NodesInSubsector.Contains(grid[x, y]))
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

            verts.Add(new Vector2(x + 0.5f, y + 0.5f));
            CheckNext(x, y, 1, 1);
            verts = RemoveRedundantVerts(verts);
            
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
            
            verts = null;
        }

        List<Vector2> RemoveRedundantVerts(List<Vector2> verts)
        {
            for (int x = 1; x < verts.Count - 1; ++x)
            {
                while (x < verts.Count - 1 &&
                        ((verts[x - 1].x == verts[x].x && verts[x].x == verts[x + 1].x) ||
                         (verts[x - 1].y == verts[x].y && verts[x].y == verts[x + 1].y))   )
                {
                    verts.RemoveAt(x);
                }
            }
            if (verts.Count >= 3 &&
                ((verts[0].x == verts[verts.Count - 1].x && verts[verts.Count - 1].x == verts[verts.Count - 2].x) ||
                (verts[0].x == verts[verts.Count - 1].y && verts[verts.Count - 1].y == verts[verts.Count - 2].y))    )
            {
                verts.RemoveAt(verts.Count - 1);
            }

            return verts;
        }

        //Extreme logic
        void CheckNext(int x, int y, int xN, int yM)
        {
            int sameLimit = 0;
            while (sameLimit < 4)
            {
                int nextX = x + xN, nextY = y + yM;

                ++sameLimit;

                if (xN == yM)
                {
                    xN = 0;
                    nextX = x;

                    //Check for left and right bounds
                    if (nextY < 0)
                    {
                        xN = -1;
                        yM = 1;
                        //CheckNext(x, y, -1, 1);
                        continue;
                    }
                    else if (nextY >= sect.ySize)
                    {
                        xN = 1;
                        yM = -1;
                        //CheckNext(x, y, 1, -1);
                        continue;
                    }
                }
                else
                {
                    yM = 0;
                    nextY = y;

                    //Check for bottom and top bounds
                    if (nextX < 0)
                    {
                        xN = 1;
                        yM = 1;
                        //CheckNext(x, y, 1, 1);
                        continue;
                    }
                    else if (nextX >= sect.xSize)
                    {
                        xN = -1;
                        yM = -1;
                        //CheckNext(x, y, -1, -1);
                        continue;
                    }
                }

                //Get node to check
                Node n = grid[nextX, nextY];

                //If this isn't in the subsector
                if (!NodesInSubsector.Contains(n))
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
                    continue;
                    //CheckNext(x, y, xN, yM);
                }
                else
                {
                    Vector2 nextVert = new Vector2(nextX, nextY) + Vector2.one * 0.5f;
                    if (verts[0] == nextVert)
                        return;
                    verts.Add(nextVert);

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
                    continue;
                    //CheckNext(nextX, nextY, xN, yM);
                }
            }
        }
    }
}
