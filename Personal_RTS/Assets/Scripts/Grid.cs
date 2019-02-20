using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public bool displayGridGizmos;
    
    //public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    public TerrainType[] walkableRegions;
    //Tiles that aren't permanently blocked
    LayerMask walkableMask;
    Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>();

    //Traversible by all Land units
    public TerrainType[] landRegions;
    LayerMask landMask;
    Dictionary<int, int>landRegionsDictionary = new Dictionary<int, int>();
    //Traversible by all Water units
    public TerrainType[] waterRegions;
    LayerMask waterMask;
    Dictionary<int, int> waterRegionsDictionary = new Dictionary<int, int>();
    //Traversible only by Special Land units
    public TerrainType[] specialLandRegions;
    LayerMask specialLandMask;
    Dictionary<int, int> specialLandRegionsDictionary = new Dictionary<int, int>();
    //Traversible only by Special Water units
    public TerrainType[] specialWaterRegions;
    LayerMask specialWaterMask;
    Dictionary<int, int> specialWaterRegionsDictionary = new Dictionary<int, int>();

    //Tiles that are permanently blocked
    public TerrainType[] unwalkableAreas;
    LayerMask unwalkableMask;
    Dictionary<int, int> unwalkableRegionsDictionary = new Dictionary<int, int>();

    Node[,] grid;
    //Key Node with count of in
    Dictionary<Node, short> nonTerrainCorners = new Dictionary<Node, short>();
    float nodeDiameter;
    int gridSizeX, gridSizeY;

    int penaltyMin = int.MaxValue;
    int penaltyMax = int.MinValue;

    private void Awake()
    {
        nodeDiameter = nodeRadius * 2f;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        RegionDictionaries(walkableRegions, out walkableMask, walkableRegionsDictionary);

        RegionDictionaries(landRegions, out landMask, landRegionsDictionary);
        RegionDictionaries(waterRegions, out waterMask, waterRegionsDictionary);
        RegionDictionaries(specialLandRegions, out specialLandMask, specialLandRegionsDictionary);
        RegionDictionaries(specialWaterRegions, out specialWaterMask, specialWaterRegionsDictionary);

        RegionDictionaries(unwalkableAreas, out unwalkableMask, unwalkableRegionsDictionary);

        //foreach (TerrainType region in walkableRegions)
        //{
        //    walkableMask.value += region.terrainMask.value;
        //    walkableRegionsDictionary.Add((int)Mathf.Log(region.terrainMask.value, 2), region.TerrainPenalty);
        //}
        //
        //foreach (TerrainType region in unwalkableAreas)
        //{
        //    unwalkableMask.value += region.terrainMask.value;
        //    unwalkableRegionsDictionary.Add((int)Mathf.Log(region.terrainMask.value, 2), region.TerrainPenalty);
        //}

        CreateGrid();
    }

    public void ModifyBlockage(bool newWalkable, Vector3 bottomLeft, Vector3 topRight, out int leftX, out int topY, out int rightX, out int bottomY)
    {
        Node bl = NodeFromWorldPoint(bottomLeft);
        leftX = bl.gridX;
        bottomY = bl.gridY;

        Node tr = NodeFromWorldPoint(topRight);
        rightX = tr.gridX;
        topY = tr.gridY;

        for (int y = bottomY; y <= topY; ++y)
        {
            for (int x = leftX; x <= rightX; ++x)
            {
                grid[x, y].walkable = newWalkable;
            }
        }
        
        Node[] n = new Node[4];
        n[0] = grid[leftX - 1, bottomY - 1];
        n[1] = grid[leftX - 1, topY + 1];
        n[2] = grid[rightX + 1, bottomY - 1];
        n[3] = grid[rightX + 1, topY + 1];

        if (!newWalkable)
            AddNewCornerSet(n);
        else
            RemoveCornerSet(n);
    }

    public void AddNewCornerSet(Node[] newCorners)
    {
        foreach(Node n in newCorners)
        {
            //Increment the node
            if (nonTerrainCorners.ContainsKey(n))
            {
                nonTerrainCorners[n]++;
            }
            else
            {
                nonTerrainCorners.Add(n, 1);
                n.isCorner = true;
            }
        }
    }

    public void RemoveCornerSet(Node[] oldCorners)
    {
        foreach (Node n in oldCorners)
        {
            if (!nonTerrainCorners.ContainsKey(n))
            {
                continue;
            }
            else if(nonTerrainCorners[n] <= 1)
            {
                nonTerrainCorners.Remove(n);
                n.isCorner = false;
            }
            else
            {
                nonTerrainCorners[n]--;
            }
        }
    }

    void RegionDictionaries(TerrainType[] region, out LayerMask mask, Dictionary<int, int> dictionary)
    {
        mask = 0;
        foreach (TerrainType r in region)
        {
            mask.value += r.terrainMask.value;
            dictionary.Add((int)Mathf.Log(r.terrainMask.value, 2), r.TerrainPenalty);
        }
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

                bool walkable = false;//bool walkable = !Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask);
                uint nodeTerrain = 0;

                int movementPenalty = 0;

            //if (!walkable)
            //{
                Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 100, unwalkableMask))
                {
                    unwalkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
                        
                }
                else
                {
                    /*Ray */ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
                    //RaycastHithit;
                    if (Physics.Raycast(ray, out hit, 100, walkableMask))
                    {
                        walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
                        walkable = true;
                    }
                    
                    //uint nodeTerrain = 0;

                    RaycastHit Land;
                    if (Physics.Raycast(ray, out Land, 100, landMask))
                    {
                        landRegionsDictionary.TryGetValue(Land.collider.gameObject.layer, out movementPenalty);
                        nodeTerrain += (int)SelfTerrainType.Land;
                    }
                    RaycastHit Water;
                    if (Physics.Raycast(ray, out Water, 100, waterMask))
                    {
                        waterRegionsDictionary.TryGetValue(Water.collider.gameObject.layer, out movementPenalty);
                        nodeTerrain += (int)SelfTerrainType.Water;
                    }
                    RaycastHit SpecialLand;
                    if (Physics.Raycast(ray, out SpecialLand, 100, specialLandMask))
                    {
                        specialLandRegionsDictionary.TryGetValue(SpecialLand.collider.gameObject.layer, out movementPenalty);
                        nodeTerrain += (int)SelfTerrainType.SpecialLand;
                    }
                    RaycastHit SpecialWater;
                    if (Physics.Raycast(ray, out SpecialWater, 100, specialWaterMask))
                    {
                        specialWaterRegionsDictionary.TryGetValue(SpecialWater.collider.gameObject.layer, out movementPenalty);
                        nodeTerrain += (int)SelfTerrainType.SpecialWater;
                    }

                    
                }
                //}

                grid[x, y] = new Node(walkable,
                                      new Walkability((nodeTerrain & (uint)SelfTerrainType.Land) == 0,
                                                      (nodeTerrain & (uint)SelfTerrainType.Water) == 0,
                                                      (nodeTerrain & (uint)SelfTerrainType.SpecialLand) == 0,
                                                      (nodeTerrain & (uint)SelfTerrainType.SpecialWater) == 0),
                                      worldPoint,
                                      x,
                                      y);/*,
                                      movementPenalty);*/
            }
        }

        for (int x = 0; x < gridSizeX; ++x)
        {
            for (int y = 0; y < gridSizeY; ++y)
            {
                Node n = grid[x, y];
                short count = IsTerrainCorner(n);
                if (count > 0)
                {
                    //Increment the node
                    if (nonTerrainCorners.ContainsKey(n))
                    {
                        nonTerrainCorners[n] += count;
                    }
                    else
                    {
                        nonTerrainCorners.Add(n, count);
                        n.isCorner = true;
                    }
                }
            }
        }

        //BlurPenaltyMap(3);
    }


    short IsTerrainCorner(Node node)
    {
        bool[,] neighbors = new bool[3, 3];

        for (int x = -1; x <= 1; ++x)
        {
            for (int y = -1; y <= 1; ++y)
            {
                if (x == 0 && y == 0)
                    continue;
        
                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    neighbors[x+1, y+1] = grid[checkX, checkY].walkable;
                }
                else
                    neighbors[x+1, y+1] = false;
            }
        }

        short count = 0;

        if(!neighbors[0, 0] && neighbors[0, 1] && neighbors[1, 0])
        {
            ++count;
        }
        if(!neighbors[2, 0] && neighbors[2, 1] && neighbors[1, 0])
        {
            ++count;
        }
        if(!neighbors[0, 2] && neighbors[0, 1] && neighbors[1, 2])
        {
            ++count;
        }
        if (!neighbors[2, 2] && neighbors[2, 1] && neighbors[1, 2])
        {
            ++count;
        }

        return count;
    }

    [System.Obsolete("Not doing terrain blurring for now")]
    void BlurPenaltyMap(int blurSize)
    {
        int kernelSize = blurSize * 2 + 1;
        //int kernelExtents = (kernelSize - 1) / 2;
        int kernelExtents = blurSize;

        int[,] penaltiesHorPass = new int[gridSizeX, gridSizeY];
        int[,] penaltiesVerPass = new int[gridSizeX, gridSizeY];

        for(int y = 0; y < gridSizeY; ++y)
        {
            for (int x = -kernelExtents; x <= kernelExtents; ++x)
            {
                int sampleX = Mathf.Clamp(x, 0, kernelExtents);
                penaltiesHorPass[0, y] += grid[sampleX, y].movementPenalty;
            }

            for (int x = 1; x < gridSizeX; ++x)
            {
                int removeIndex = Mathf.Clamp(x - kernelExtents - 1, 0, gridSizeX);
                int addIndex = Mathf.Clamp(x + kernelExtents, 0, gridSizeX - 1);

                penaltiesHorPass[x, y] = penaltiesHorPass[x - 1, y] - grid[removeIndex, y].movementPenalty + grid[addIndex, y].movementPenalty;
            }
        }

        for (int x = 0; x < gridSizeX; ++x)
        {
            for (int y = -kernelExtents; y <= kernelExtents; ++y)
            {
                int sampleY = Mathf.Clamp(y, 0, kernelExtents);
                penaltiesVerPass[x, 0] += penaltiesHorPass[x, sampleY];
            }

            int blurredPenalty = Mathf.RoundToInt((float)penaltiesVerPass[x, 0] / (kernelSize * kernelSize));
            grid[x, 0].movementPenalty = blurredPenalty;

            for (int y = 1; y < gridSizeY; ++y)
            {
                int removeIndex = Mathf.Clamp(y - kernelExtents - 1, 0, gridSizeY);
                int addIndex = Mathf.Clamp(y + kernelExtents, 0, gridSizeY - 1);

                penaltiesVerPass[x, y] = penaltiesVerPass[x, y - 1] - penaltiesHorPass[x, removeIndex] + penaltiesHorPass[x, addIndex];

                blurredPenalty = Mathf.RoundToInt((float)penaltiesVerPass[x, y] / (kernelSize * kernelSize));
                grid[x, y].movementPenalty = blurredPenalty;

                if (blurredPenalty > penaltyMax)
                    penaltyMax = blurredPenalty;
                if (blurredPenalty < penaltyMin)
                    penaltyMin = blurredPenalty;
            }
        }
    }

    public List<Node> GetNeighbors(Node node)
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
            if (n.walkable)
            {
                left = true;
                neighbors.Add(n);
            }
        }
        if (node.gridY > 0)
        {
            n = grid[node.gridX, node.gridY - 1];
            if (n.walkable)
            {
                bottom = true;
                neighbors.Add(n);
            }
        }
        if (node.gridX < gridSizeX - 1)
        {
            n = grid[node.gridX + 1, node.gridY];
            if (n.walkable)
            {
                right = true;
                neighbors.Add(n);
            }
        }
        if (node.gridY < gridSizeY - 1)
        {
            n = grid[node.gridX, node.gridY + 1];
            if (n.walkable)
            {
                top = true;
                neighbors.Add(n);
            }
        }

        if(left && bottom)
        {
            n = grid[node.gridX - 1, node.gridY - 1];
            if (n.walkable)
                neighbors.Add(n);
        }
        if (left && top)
        {
            n = grid[node.gridX - 1, node.gridY + 1];
            if (n.walkable)
                neighbors.Add(n);
        }
        if (right && bottom)
        {
            n = grid[node.gridX + 1, node.gridY - 1];
            if (n.walkable)
                neighbors.Add(n);
        }
        if (right && top)
        {
            n = grid[node.gridX + 1, node.gridY + 1];
            if (n.walkable)
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

                Gizmos.color = (n.walkable ? Gizmos.color : Color.red);

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
    }

    [System.Serializable]
    public class TerrainType
    {
        public LayerMask terrainMask;
        public int TerrainPenalty;
    }
}
