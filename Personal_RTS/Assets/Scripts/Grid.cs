using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public static Logger logger = new Logger(@"C:\Users\drago\Documents\GitHub\Personal_RTS\Personal_RTS\Assets\Logs\SectorLog.log");

    public static Grid GetGrid
    {
        get { return grid_Instance; }
    }

    static Grid grid_Instance;

    public bool displayGridGizmos;
    public bool slowDebug;
    public int DebugXSector, DebugYSector;
    public LayerMask DebugClickLocation;

    //public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    float prevNodeRadius;

    //Node[,] grid;

    float nodeDiameter;
    int gridSizeX, gridSizeY;
    int xSectorCount, ySectorCount;
    int nodesPerSector;
    
    public int SectorSize = 10;
    Sector[,,] GridSectors;

    public List<Node> debugPath = new List<Node>();

    public LayerMask dim1;
    public LayerMask dim2;
    public LayerMask dim3;

    LayerMask view;

    private void Awake()
    {
        Initialize();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            view = dim1;
        if (Input.GetKeyDown(KeyCode.Alpha2))
            view = dim2;
        if (Input.GetKeyDown(KeyCode.Alpha3))
            view = dim3;

        if (displayGridGizmos && Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Input.mousePosition;

            RaycastHit hit;
            Camera cam = Camera.main;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 1024f, DebugClickLocation))
            {
                int x, y;
                NodeFromWorldPoint(hit.point, out x, out y);
                bool walkable = GetWalkableAt(view, x, y);

                Debug.Log(string.Format("The node at location X,Y: {0},{1} is {2}", x, y, walkable ? "Walkable" : "Not Walkable"));

                DebugXSector = CoordToSectorNumber(x);
                DebugYSector = CoordToSectorNumber(y);
            }
        }

        if (prevNodeRadius != nodeRadius)
        {
            Initialize();
        }
    }

    private void Initialize()
    {
        {
            grid_Instance = this;

            nodeDiameter = nodeRadius * 2f;
            nodesPerSector = Mathf.RoundToInt(SectorSize / nodeDiameter);
            gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
            gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

            CreateGrid();
            prevNodeRadius = nodeRadius;
            view = dim1;
        }
    }

    bool GetWalkableAt(LayerMask dimension, int xCoord, int yCoord)
    {
        char dim;

        if (dimension == dim1)
        {
            dim = (char)0;
        }
        else if (dimension == dim2)
        {
            dim = (char)1;
        }
        else if (dimension == dim3)
        {
            dim = (char)2;
        }
        else
        {
            throw new InvalidDimensionException(dimension);
        }

        return GetWalkableAt(dim, xCoord, yCoord);
    }

    bool GetWalkableAt(char dim, int xCoord, int yCoord)
    {
        if (dim > 2 || dim < 0)
            throw new InvalidDimensionException(dim);

        return GridSectors[dim, CoordToSectorNumber(xCoord), CoordToSectorNumber(yCoord)].GetWalkableAt(xCoord % (nodesPerSector), yCoord % (nodesPerSector));
    }

    void SetWalkableAt(LayerMask dimension, int xCoord, int yCoord, bool newCanWalk)
    {
        char dim;

        if (dimension == dim1)
        {
            dim = (char)0;
        }
        else if (dimension == dim2)
        {
            dim = (char)1;
        }
        else if (dimension == dim3)
        {
            dim = (char)2;
        }
        else
        {
            throw new InvalidDimensionException(dimension);
        }

        SetWalkableAt(dim, xCoord, yCoord, newCanWalk);
    }

    void SetWalkableAt(char dim, int xCoord, int yCoord, bool newCanWalk)
    {
        if (dim > 2 || dim < 0)
            throw new InvalidDimensionException(dim);

        
        if (dim == 0)
        {
            if (GetWalkableAt((char)1, xCoord, yCoord))
                AssignWalkable((char)2, xCoord, yCoord, newCanWalk);
        }
        else if (dim == 1)
        {
            if (GetWalkableAt((char)0, xCoord, yCoord))
                AssignWalkable((char)2, xCoord, yCoord, newCanWalk);
        }
        else if (dim == 2)
        {
            for (char d = (char)0; d < (char)2; ++d)
                AssignWalkable(d, xCoord, yCoord, newCanWalk);
        }
        AssignWalkable(dim, xCoord, yCoord, newCanWalk);
    }

    void AssignWalkable(char dim, int x, int y, bool newCanWalk)
    {
        try
        {
            GridSectors[dim, CoordToSectorNumber(x), CoordToSectorNumber(y)].SetWalkableAt(x % nodesPerSector, y % nodesPerSector, newCanWalk);
        }
        catch (System.IndexOutOfRangeException)
        {
            Debug.Log(string.Format("Dim: {0}, X: {1}, Y: {2}, Sector X: {3}, Sector Y: {4}, Adjusted X: {5}, Adjusted Y: {6}", (int)dim, x, y, CoordToSectorNumber(x), CoordToSectorNumber(y), x % nodesPerSector, y % nodesPerSector));
            throw new InvalidDimensionException();
        }
    }

    public void ModifyBlockage(LayerMask dimension, bool isWalkable, Vector3 bottomLeft, Vector3 topRight)
    {
        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
        timer.Start();

        int xBL, yBL;
        NodeFromWorldPoint(bottomLeft, out xBL, out yBL);

        int xTR, yTR;
        NodeFromWorldPoint(topRight, out xTR, out yTR);

        //char walkableBool = (char)0;
        //
        //if (dimension == dim1)
        //{
        //    walkableBool = (char)1;
        //}
        //else if(dimension == dim2)
        //{
        //    walkableBool = (char)2;
        //}
        //else if (dimension == dim3)
        //{
        //    walkableBool = (char)3;
        //}

        for (int x = xBL; x <= xTR; ++x)
        {
            for (int y = yBL; y <= yTR; ++y)
            {
                SetWalkableAt(dimension, x, y, isWalkable);
            }
        }

        for (int x = CoordToSectorNumber(xBL); x <= CoordToSectorNumber(xTR); ++x)
        {
            for (int y = CoordToSectorNumber(yBL); y <= CoordToSectorNumber(yTR); ++y)
            {
                if(dimension == dim3)
                {
                    GridSectors[0, x, y].UpdateSubsectors();
                    GridSectors[1, x, y].UpdateSubsectors();
                }
                else if (dimension == dim1)
                {
                    GridSectors[0, x, y].UpdateSubsectors();
                }
                else if (dimension == dim2)
                {
                    GridSectors[1, x, y].UpdateSubsectors();
                }
                GridSectors[2, x, y].UpdateSubsectors();
                //GridSectors[(int)Mathf.Log(dimension, 2) - 8, x, y].UpdateSubsectors();
            }
        }

        Debug.Log(string.Format("Updated terrain data for structure in {0}ms", timer.ElapsedMilliseconds));
    }

    int CoordToSectorNumber(int n)
    {
        return (int)Mathf.RoundToInt(n / nodesPerSector);
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
        int xBL, yBL;
        NodeFromWorldPoint(bottomLeft, out xBL, out yBL);
        
        int xTR, yTR;
        NodeFromWorldPoint(topRight, out xTR, out yTR);

        for (int y = yBL; y <= yTR; ++y)
        {
            for (int x = xBL; x <= xTR; ++x)
            {
                if(!GetWalkableAt(dimension, x, y))
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
        xSectorCount = ((int)gridWorldSize.x - 1) / SectorSize + 1;
        ySectorCount = ((int)gridWorldSize.y - 1) / SectorSize + 1;

        GridSectors = new Sector[3, xSectorCount, ySectorCount];

        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
        timer.Start();
        GenerateSectors(dim1);
        Debug.Log(string.Format("Generated Dimension 1 Grid, sectors, and subsectors in {0}ms", timer.ElapsedMilliseconds));
        timer.Reset();
        timer.Start();
        GenerateSectors(dim2);
        Debug.Log(string.Format("Generated Dimension 2 Grid, sectors, and subsectors in {0}ms", timer.ElapsedMilliseconds));
        timer.Reset();
        timer.Start();
        GenerateSectors(dim3 ^ dim2 ^ dim1);
        Debug.Log(string.Format("Generated Dimension 3 Grid, sectors, and subsectors in {0}ms", timer.ElapsedMilliseconds));
        timer.Stop();
    }

    void GenerateSectors(LayerMask mask)
    {
        Logger logger = new Logger(@"C:\Users\drago\Documents\GitHub\Personal_RTS\Personal_RTS\Assets\Logs\SectorLog.log");
        for (int x = 0; x < ((int)gridWorldSize.x - 1) / SectorSize + 1; ++x)
        {
            for (int y = 0; y < ((int)gridWorldSize.y - 1)/ SectorSize + 1; ++y)
            {
                Vector3 worldPoint = transform.position + Vector3.right * ((x * SectorSize) + nodeRadius) + Vector3.forward * ((y * SectorSize) + nodeRadius);

                GridSectors[(int)Mathf.Log(mask, 2) - 8, x, y] = new Sector(mask,
                                                  (int)((x + 1) * SectorSize <= gridWorldSize.x ? SectorSize : gridWorldSize.x % SectorSize),
                                                  (int)((y + 1) * SectorSize <= gridWorldSize.y ? SectorSize : gridWorldSize.y % SectorSize),
                                                  worldPoint,
                                                  nodeDiameter);
            }
        }
    }

    public void NodeFromWorldPoint(Vector3 worldPosition, out int xPos, out int yPos)
    {
        float percentX = worldPosition.x / gridWorldSize.x;
        float percentY = worldPosition.z / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        xPos = x;//(int)(x / nodeDiameter);
        yPos = y;//(int)(y / nodeDiameter);

        return;// grid[x, y];
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position + new Vector3(gridWorldSize.x / 2, 0, gridWorldSize.y / 2), new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

        if (GridSectors != null && displayGridGizmos)
        {
            int dimSector = (int)Mathf.Log(view, 2) - 8;

            if (slowDebug)
            {
                Sector s = GridSectors[dimSector, DebugXSector, DebugYSector];
                for (int x = 0; x < nodesPerSector; ++x)
                {
                    for (int y = 0; y < nodesPerSector; ++y)
                    {
                        if (GetWalkableAt((char)dimSector, (x + DebugXSector * nodesPerSector), (y + DebugYSector * nodesPerSector)))
                        {
                            Gizmos.color = s.sectorColor;
                            Vector3 v = Vector3.right * x * nodeDiameter + Vector3.forward * y * nodeDiameter + Vector3.right * nodeRadius + Vector3.forward * nodeRadius;
                            v += Vector3.right * DebugXSector * SectorSize + Vector3.forward * DebugYSector * SectorSize;
                            Gizmos.DrawCube(v, Vector3.one * nodeDiameter);
                        }
                    }
                }
            }
            else
                for (int x = 0; x < ((int)gridWorldSize.x - 1) / SectorSize + 1; ++x)
                {
                    for (int y = 0; y < ((int)gridWorldSize.y - 1) / SectorSize + 1; ++y)
                    {
                        foreach (Sector.SubSector s in GridSectors[dimSector, x, y].subs)
                        {
                            Gizmos.color = s.subSectorColor;
                            try
                            {
                                Gizmos.DrawMesh(s.mesh, Vector3.right * SectorSize * x + Vector3.forward * SectorSize * y + Vector3.up * 0.001f, Quaternion.Euler(90, 0, 0), Vector3.one * nodeDiameter);
                            }
                            catch { continue; }
                        }
                    }
                }

            //for (int x = 0; x < gridSizeX; ++x)
            //{
            //    for (int y = 0; y < gridSizeY; ++y)
            //    {
            //        if (view == dim1)
            //        {
            //            if (GetWalkableAt(dim1, x, y))
            //                Gizmos.color = GridSectors[dimSector, x / SectorSize, y / SectorSize].sectorColor;
            //            else
            //                Gizmos.color = Color.black;
            //        }
            //        else if (view == dim2)
            //        {
            //            if (GetWalkableAt(dim2, x, y))
            //                Gizmos.color = GridSectors[dimSector, x / SectorSize, y / SectorSize].sectorColor;
            //            else
            //                Gizmos.color = Color.black;
            //        }
            //        else if (view == dim3)
            //        {
            //            if (GetWalkableAt(dim1, x, y) && GetWalkableAt(dim2, x, y) && GetWalkableAt(dim3, x, y))
            //                Gizmos.color = GridSectors[dimSector, x / SectorSize, y / SectorSize].sectorColor;
            //            else
            //                Gizmos.color = Color.black;
            //        }
            //
            //        Gizmos.DrawCube(Vector3.right * (x * (nodeRadius * 2) + nodeRadius) + Vector3.forward * (y * (nodeRadius * 2) + nodeRadius), Vector3.one * (nodeDiameter));
            //    }
            //}

            //foreach (Node n in grid)
            //{
            //    Gizmos.color = Color.white;//Color.Lerp(Color.white, Color.black, Mathf.InverseLerp(penaltyMin, penaltyMax, n.movementPenalty));
            //
            //    Gizmos.color = (n.GetWalkableByDimension(view) ? Gizmos.color : Color.black);
            //
            //
            //
            //
            //    //if (!n.walkable 
            //        Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter));
            //}
        }

        //if (debugPath.Count > 0)
        //{
        //    Gizmos.color = Color.magenta;
        //
        //    foreach (Node n in debugPath)
        //    {
        //        Gizmos.DrawCube(n.worldPosition, Vector3.one * nodeDiameter);
        //    }
        //}
    }

    [System.Serializable]
    public class TerrainType
    {
        public string[] terrainMask;
        public int TerrainPenalty;
    }
}

[System.Serializable]
class InvalidDimensionException : System.Exception
{
    public InvalidDimensionException() : base(string.Format("Invalid Dimension Given"))
    {
    }

    public InvalidDimensionException(int dimension) : base(string.Format("Invalid Dimension Given: Dimension {0}", dimension))
    {

    }
}

[System.Serializable]
class SubsectorMeshException : System.Exception
{
    public SubsectorMeshException() : base(string.Format("Unable to find subsector bounds"))
    {
    }
}
