using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public static Grid GetGrid
    {
        get { return grid_Instance; }
    }

    static Grid grid_Instance;

    public bool displayGridGizmos;
    
    //public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    public TerrainType[] walkableRegions;
    //Tiles that aren't permanently blocked
    public LayerMask walkableMask;
    Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>();

    //Tiles that are permanently blocked
    public TerrainType[] unwalkableAreas;
    LayerMask unwalkableMask;
    Dictionary<int, int> unwalkableRegionsDictionary = new Dictionary<int, int>();

    public LayerMask dimensions;

    Node[,] grid;
    //Key Node with count of in
    Dictionary<Node, short> nonTerrainCorners = new Dictionary<Node, short>();
    float nodeDiameter;
    int gridSizeX, gridSizeY;

    int penaltyMin = int.MaxValue;
    int penaltyMax = int.MinValue;

    public List<Node> debugPath = new List<Node>();

    public LayerMask dim1;
    public LayerMask dim2;
    public LayerMask dim3;

    LayerMask view;

    private void Awake()
    {
        grid_Instance = this;

        nodeDiameter = nodeRadius * 2f;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        CreateGrid();
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
    }

    public void ModifyBlockage(LayerMask dimension, bool isWalkable, Vector3 bottomLeft, Vector3 topRight)
    {
        Node bl = NodeFromWorldPoint(bottomLeft);

        Node tr = NodeFromWorldPoint(topRight);

        char walkableBool = (char)0;
        
        if (dimension == dim1)
        {
            walkableBool = (char)1;
        }
        else if(dimension == dim2)
        {
            walkableBool = (char)2;
        }
        else if (dimension == dim3)
        {
            walkableBool = (char)3;
        }

        for (int y = bl.gridY; y <= tr.gridY; ++y)
        {
            for (int x = bl.gridX; x <= tr.gridX; ++x)
            {
                if(isWalkable)
                    grid[x, y].SetWalkable((char)(grid[x, y].GetWalkable() | walkableBool));
                else
                    grid[x, y].SetWalkable((char)(grid[x, y].GetWalkable() & walkableBool ^ grid[x, y].GetWalkable()));
            }
        }
    }

    /// <summary>
    /// Returns True if the rectangular area between bottomLeft and topRight has any obstacles blocking the given dimension.
    /// </summary>
    /// <param name="dimension">The dimension to be checked against as a LayerMask. 2^8, 2^9, 2^10)</param>
    /// <param name="bottomLeft">Location of Bottom Left corner. (X, UNUSED, Y)</param>
    /// <param name="topRight">Location of Top Right corner. (X, UNUSED, Y)</param>
    /// <returns></returns>
    public bool AreaHasObstacle(LayerMask dimension, Vector3 bottomLeft, Vector3 topRight)
    {
        Node bl = NodeFromWorldPoint(bottomLeft);

        Node tr = NodeFromWorldPoint(topRight);

        for (int y = bl.gridY; y <= tr.gridY; ++y)
        {
            for (int x = bl.gridX; x <= tr.gridX; ++x)
            {
                if (!grid[x, y].GetWalkableByDimension(dimension))
                    return true;
            }
        }

        return false;
    }

    public int MaxSize
    {
        get { return gridSizeX * gridSizeY; }
    }

    void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; ++x)
        {
            for(int y = 0; y < gridSizeY; ++y)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);

                char walkable = (char)3;

                Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
                
                RaycastHit[] hits = Physics.RaycastAll(ray, 100, dim1);
                foreach (RaycastHit h in hits)
                {
                    if (h.transform.gameObject.tag == "Unit")
                        continue;
                        
                    walkable ^= (char)1;
                    break;
                }
                
                hits = Physics.RaycastAll(ray, 100, dim2);
                foreach (RaycastHit h in hits)
                {
                    if (h.transform.gameObject.tag == "Unit")
                        continue;

                    walkable ^= (char)2;
                    break;
                }

                hits = Physics.RaycastAll(ray, 100, dim3);
                foreach (RaycastHit h in hits)
                {
                    if (h.transform.gameObject.tag == "Unit")
                        continue;

                    walkable = (char)0;
                    break;
                }

                grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }

        //BlurPenaltyMap(3);
    }

    public List<Node> GetNeighbors(LayerMask dimension, Node node)
    {
        List<Node> neighbors = new List<Node>();

        bool top = false;
        bool bottom = false;
        bool left = false;
        bool right = false;

        Node n;

        if (node.gridX > 0)
        {
            n = grid[node.gridX - 1, node.gridY];
            if (n.GetWalkableByDimension(dimension))
            {
                left = true;
                neighbors.Add(n);
            }
        }
        if (node.gridY > 0)
        {
            n = grid[node.gridX, node.gridY - 1];
            if (n.GetWalkableByDimension(dimension))
            {
                bottom = true;
                neighbors.Add(n);
            }
        }
        if (node.gridX < gridSizeX - 1)
        {
            n = grid[node.gridX + 1, node.gridY];
            if (n.GetWalkableByDimension(dimension))
            {
                right = true;
                neighbors.Add(n);
            }
        }
        if (node.gridY < gridSizeY - 1)
        {
            n = grid[node.gridX, node.gridY + 1];
            if (n.GetWalkableByDimension(dimension))
            {
                top = true;
                neighbors.Add(n);
            }
        }

        if(left && bottom)
        {
            n = grid[node.gridX - 1, node.gridY - 1];
            if (n.GetWalkableByDimension(dimension))
                neighbors.Add(n);
        }
        if (left && top)
        {
            n = grid[node.gridX - 1, node.gridY + 1];
            if (n.GetWalkableByDimension(dimension))
                neighbors.Add(n);
        }
        if (right && bottom)
        {
            n = grid[node.gridX + 1, node.gridY - 1];
            if (n.GetWalkableByDimension(dimension))
                neighbors.Add(n);
        }
        if (right && top)
        {
            n = grid[node.gridX + 1, node.gridY + 1];
            if (n.GetWalkableByDimension(dimension))
                neighbors.Add(n);
        }

        //for (int x = -1; x <= 1; ++x)
        //{
        //    for (int y = -1; y <= 1; ++y)
        //    {
        //        if (x == 0 && y == 0)
        //            continue;
        //
        //        int checkX = node.gridX + x;
        //        int checkY = node.gridY + y;
        //
        //        if()
        //
        //        if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
        //        {
        //            neighbors.Add(grid[checkX, checkY]);
        //        }
        //    }
        //}

        return neighbors;
    }

    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        return grid[x, y];
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

        if (grid != null && displayGridGizmos)
        {
            foreach (Node n in grid)
            {
                Gizmos.color = Color.white;//Color.Lerp(Color.white, Color.black, Mathf.InverseLerp(penaltyMin, penaltyMax, n.movementPenalty));

                Gizmos.color = (n.GetWalkableByDimension(view) ? Gizmos.color : Color.red);

                if (Gizmos.color == Color.white && nonTerrainCorners.ContainsKey(n))
                {
                    Gizmos.color = Color.blue;
                }
                else if (Gizmos.color == Color.red && nonTerrainCorners.ContainsKey(n))
                {
                    Gizmos.color = (Color.red + Color.blue) / 2;
                }

                //if (!n.walkable 
                    Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter));
            }
        }

        if (debugPath.Count > 0)
        {
            Gizmos.color = Color.magenta;

            foreach (Node n in debugPath)
            {
                Gizmos.DrawCube(n.worldPosition, Vector3.one * nodeDiameter);
            }
        }
    }

    [System.Serializable]
    public class TerrainType
    {
        public string[] terrainMask;
        public int TerrainPenalty;
    }
}
