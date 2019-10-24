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

    //Node[,] grid;

    float nodeDiameter;
    int gridSizeX, gridSizeY;
    
    public int SectorSize = 10;
    Sector[,,] GridSectors;

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

        return GridSectors[dim, xCoord / SectorSize, yCoord / SectorSize].GetWalkableAt(xCoord % SectorSize, yCoord % SectorSize);
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

        if (dim == 2)
        {
            for (char d = (char)0; d < (char)3; ++d)
                GridSectors[d, xCoord / SectorSize, yCoord / SectorSize].SetWalkableAt(xCoord % SectorSize, yCoord % SectorSize, newCanWalk);
        }
        else if (dim == 0)
        {
            GridSectors[dim, xCoord / SectorSize, yCoord / SectorSize].SetWalkableAt(xCoord % SectorSize, yCoord % SectorSize, newCanWalk);
            if (GetWalkableAt((char)1, xCoord, yCoord))
                GridSectors[(char)2, xCoord / SectorSize, yCoord / SectorSize].SetWalkableAt(xCoord % SectorSize, yCoord % SectorSize, newCanWalk);
        }
        else if (dim == 1)
        {
            GridSectors[dim, xCoord / SectorSize, yCoord / SectorSize].SetWalkableAt(xCoord % SectorSize, yCoord % SectorSize, newCanWalk);
            if (GetWalkableAt((char)0, xCoord, yCoord))
                GridSectors[(char)2, xCoord / SectorSize, yCoord / SectorSize].SetWalkableAt(xCoord % SectorSize, yCoord % SectorSize, newCanWalk);
        }
    }

    public void ModifyBlockage(LayerMask dimension, bool isWalkable, Vector3 bottomLeft, Vector3 topRight)
    {
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

        for (int y = yBL; y <= yTR; ++y)
        {
            for (int x = xBL; x <= xTR; ++x)
            {
                SetWalkableAt(dimension, x, y, isWalkable);
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
        GridSectors = new Sector[3, gridSizeX / SectorSize, gridSizeY / SectorSize];

        GenerateSectors(dim1);
        GenerateSectors(dim2);
        GenerateSectors(dim3);
    }

    void GenerateSectors(LayerMask mask)
    {
        for (int x = 0; x < gridSizeX / SectorSize; ++x)
        {
            for (int y = 0; y < gridSizeY / SectorSize; ++y)
            {
                Vector3 worldPoint = transform.position + Vector3.right * ((x * SectorSize) * nodeDiameter + nodeRadius) + Vector3.forward * ((y * SectorSize) * nodeDiameter + nodeRadius);

                GridSectors[(int)Mathf.Log(mask, 2) - 8, x, y] = new Sector(mask,
                                                  (int)(x * SectorSize <= gridWorldSize.x ? SectorSize : x * SectorSize - gridWorldSize.x),
                                                  (int)(y * SectorSize <= gridWorldSize.y ? SectorSize : y * SectorSize - gridWorldSize.y),
                                                  worldPoint,
                                                  nodeRadius);
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

        xPos = (int)(x / nodeDiameter);
        yPos = (int)(y / nodeDiameter);

        return;// grid[x, y];
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position + new Vector3(gridWorldSize.x / 2, 0, gridWorldSize.y / 2), new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

        if (GridSectors != null && displayGridGizmos)
        {
            int dimSector = (int)Mathf.Log(view, 2) - 8;

            for (int x = 0; x < gridSizeX; ++x)
            {
                for (int y = 0; y < gridSizeY; ++y)
                {
                    if (view == dim1)
                    {
                        if (GetWalkableAt(dim1, x, y))
                            Gizmos.color = GridSectors[dimSector, x / SectorSize, y / SectorSize].sectorColor;
                        else
                            Gizmos.color = Color.black;
                    }
                    else if (view == dim2)
                    {
                        if (GetWalkableAt(dim2, x, y))
                            Gizmos.color = GridSectors[dimSector, x / SectorSize, y / SectorSize].sectorColor;
                        else
                            Gizmos.color = Color.black;
                    }
                    else if (view == dim3)
                    {
                        if (GetWalkableAt(dim1, x, y) && GetWalkableAt(dim2, x, y) && GetWalkableAt(dim3, x, y))
                            Gizmos.color = GridSectors[dimSector, x / SectorSize, y / SectorSize].sectorColor;
                        else
                            Gizmos.color = Color.black;
                    }

                    Gizmos.DrawCube(Vector3.right * (x * (nodeRadius * 2) + nodeRadius) + Vector3.forward * (y * (nodeRadius * 2) + nodeRadius), Vector3.one * (nodeDiameter));
                }
            }

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
