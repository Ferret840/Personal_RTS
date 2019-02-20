using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum LandNeighbors { Up         = 1 << 0,
                     Down       = 1 << 1,
                     Left       = 1 << 2,
                     Right      = 1 << 3,
                     UpLeft     = 1 << 4,
                     UpRight    = 1 << 5,
                     DownLeft   = 1 << 6,
                     DownRight  = 1 << 7 };
enum WaterNeighbors { Up         = LandNeighbors.Up << 8,
                      Down       = LandNeighbors.Down << 8,
                      Left       = LandNeighbors.Left << 8,
                      Right      = LandNeighbors.Right << 8,
                      UpLeft     = LandNeighbors.UpLeft << 8,
                      UpRight    = LandNeighbors.UpRight << 8,
                      DownLeft   = LandNeighbors.DownLeft << 8,
                      DownRight  = LandNeighbors.DownRight << 8 };
enum SpecialLandNeighbors { Up         = WaterNeighbors.Up << 8,
                            Down       = WaterNeighbors.Down << 8,
                            Left       = WaterNeighbors.Left << 8,
                            Right      = WaterNeighbors.Right << 8,
                            UpLeft     = WaterNeighbors.UpLeft << 8,
                            UpRight    = WaterNeighbors.UpRight << 8,
                            DownLeft   = WaterNeighbors.DownLeft << 8,
                            DownRight  = WaterNeighbors.DownRight << 8 };
enum SpecialWaterNeighbors { Up         = SpecialLandNeighbors.Up << 8,
                             Down       = SpecialLandNeighbors.Down << 8,
                             Left       = SpecialLandNeighbors.Left << 8,
                             Right      = SpecialLandNeighbors.Right << 8,
                             UpLeft     = SpecialLandNeighbors.UpLeft << 8,
                             UpRight    = SpecialLandNeighbors.UpRight << 8,
                             DownLeft   = SpecialLandNeighbors.DownLeft << 8,
                             DownRight  = SpecialLandNeighbors.DownRight << 8 };

enum SelfTerrainType { Land = LandNeighbors.Up, Water = WaterNeighbors.Up, SpecialLand = SpecialLandNeighbors.Up, SpecialWater = SpecialWaterNeighbors.Up };

public class Node : IHeapItem<Node>
{
    public bool walkable;
    public Walkability walkability;
    public Vector3 worldPosition;
    public int gridX, gridY;
    [System.Obsolete("No movement penalties for now")]
    public int movementPenalty;
    public string Terrain;

    public int gCost;
    public int hCost;
    public Node parent;
    int heapIndex;

    public bool isCorner;

    uint WalkableNeighbors = 0;

    public Node(bool _blocked, Walkability _walkability, Vector3 _worldPos, int _gridX, int _gridY)
    {
        walkable = _blocked;
        walkability = _walkability;
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
    }

    public int fCost
    {
        get
        {
            return gCost + hCost;
        }
    }

    public int HeapIndex
    {
        get
        {
            return heapIndex;
        }
        set
        {
            heapIndex = value;
        }
    }

    public int CompareTo(Node other)
    {
        int compare = fCost.CompareTo(other.fCost);
        if(compare == 0)
        {
            compare = hCost.CompareTo(other.hCost);
        }

        return -compare;
    }

    public void SetWalkable(bool newWalkable)
    {
        if (newWalkable)
        {
            
        }
        else
        {

        }
    }
}

public class Walkability
{
    bool Land, Water;
    bool SpecialLand, SpecialWater;

    public Walkability(bool _land, bool _water, bool _specialLand, bool _specialWater)
    {
        Land = _land;
        Water = _water;
        SpecialLand = _specialLand;
        SpecialWater = _specialWater;
    }
}
