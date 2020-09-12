#include "stdafx.h"
#include "Grid.h"

namespace TerrainData
{
  Grid* Grid::grid_Instance = NULL;

  Grid::Grid(float _nodeRadius, float _worldSizeX, float _worldSizeY, int _sectorSize) :
    nodeRadius(_nodeRadius), nodeDiameter(nodeRadius * 2.0f),
    sectorSize(_sectorSize), worldSize(Vector2<float>(_worldSizeX, _worldSizeY)),
    nodesPerSector((int)round(sectorSize / nodeDiameter)),
    gridSize(Vector2<int>((int)round(worldSize.x / nodeDiameter), (int)round(worldSize.y / nodeDiameter))),
    sectorCount(Vector2<int>(((int)worldSize.x - 1) / sectorSize + 1, ((int)worldSize.y - 1) / sectorSize + 1))
  {
    if (grid_Instance != NULL)
      delete grid_Instance;

    grid_Instance = this;
      
    CreateGrid();
  }

  Vector2<int> Grid::getGridSize()
  {
    return gridSize;
  }
  Vector2<int> Grid::getSectorCount()
  {
    return sectorCount;
  }
  Vector2<float> Grid::getWorldSize()
  {
    return worldSize;
  }
  int Grid::getNodesPerSector()
  {
    return nodesPerSector;
  }
  int Grid::getSectorSize()
  {
    return sectorSize;
  }
  Sector* Grid::getSector(int dim, int x, int y)
  {
    return &gridSectors[dim][x][y];
  }
  void Grid::changeSectorSize(int newSize)
  {
    if (sectorSize == newSize)
      return;

    DeleteGrid();

    sectorSize = newSize;
    nodesPerSector = (int)round(sectorSize / nodeDiameter);
    sectorCount = Vector2<int>(((int)worldSize.x - 1) / sectorSize + 1, ((int)worldSize.y - 1) / sectorSize + 1);

    CreateGrid();
  }
  void Grid::changeWorldSize(float x, float y)
  {
    if (worldSize.x == x && worldSize.y == y)
      return;

    DeleteGrid();

    worldSize = Vector2<float>(x, y);

    gridSize = Vector2<int>((int)round(worldSize.x / nodeDiameter), (int)round(worldSize.y / nodeDiameter));
    sectorCount = Vector2<int>(((int)worldSize.x - 1) / sectorSize + 1, ((int)worldSize.y - 1) / sectorSize + 1);

    CreateGrid();
  }
  void Grid::changeNodeRadius(float newRadius)
  {
    if (nodeRadius == newRadius)
      return;

    DeleteGrid();

    nodeRadius = newRadius;
    nodeDiameter = nodeRadius * 2.0f;

    nodesPerSector = (int)round(sectorSize / nodeDiameter);
    gridSize = Vector2<int>((int)round(worldSize.x / nodeDiameter), (int)round(worldSize.y / nodeDiameter));

    CreateGrid();
  }

  Grid::~Grid()
  {
    grid_Instance = NULL;

    DeleteGrid();
  }

  bool Grid::GetWalkableAt(int dimension, int xCoord, int yCoord)
  {
    char dim;

    if (dimension == 1 << 8)//dim1)
    {
      dim = (char)0;
    }
    else if (dimension == 1 << 9)
    {
      dim = (char)1;
    }
    else if (dimension == 1 << 10)
    {
      dim = (char)2;
    }
    else
    {
      throw new InvalidDimensionException(dimension);
    }

    return GetWalkableAt(dim, xCoord, yCoord);
  }

  bool Grid::GetWalkableAt(char dim, int xCoord, int yCoord)
  {
    if (dim > 2)
      throw new InvalidDimensionException(dim);

    return gridSectors[dim][CoordToSectorNumber(xCoord)][CoordToSectorNumber(yCoord)].GetWalkableAt(xCoord % (nodesPerSector), yCoord % (nodesPerSector));//GridSectors[dim, CoordToSectorNumber(xCoord), CoordToSectorNumber(yCoord)].GetWalkableAt(xCoord % (nodesPerSector), yCoord % (nodesPerSector));
  }

  void Grid::SetWalkableAt(int dimension, int xCoord, int yCoord, bool newCanWalk)
  {
    char dim;

    if (dimension == 1 << 8)
    {
      dim = (char)0;
    }
    else if (dimension == 1 << 9)
    {
      dim = (char)1;
    }
    else if (dimension == 1 << 10)
    {
      dim = (char)2;
    }
    else
    {
      throw new InvalidDimensionException(dimension);
    }

    SetWalkableAt(dim, xCoord, yCoord, newCanWalk);
  }

  void Grid::SetWalkableAt(char dim, int xCoord, int yCoord, bool newCanWalk)
  {
    if (dim > 2)
      throw new InvalidDimensionException(dim);

    //If shared dimension, both other dims get set
    if (dim == 2)
    {
      for (char d = (char)0; d < (char)2; ++d)
        AssignWalkable(d, xCoord, yCoord, newCanWalk);
    }
    //Else it must be dim 1 or 2, in which case only change shared dimension if the other dim is clear
    else if (GetWalkableAt((char)(dim ^ 1), xCoord, yCoord))
      AssignWalkable((char)2, xCoord, yCoord, newCanWalk);

    //Always set own dimension
    AssignWalkable(dim, xCoord, yCoord, newCanWalk);
  }

  void Grid::AssignWalkable(char dim, int x, int y, bool newCanWalk)
  {
    //try
    //{
    gridSectors[dim][CoordToSectorNumber(x)][CoordToSectorNumber(y)].SetWalkableAt(x % nodesPerSector, y % nodesPerSector, newCanWalk);
    //}
    //catch (System.IndexOutOfRangeException)
    //{
    //    Debug.Log(string.Format("Dim: {0}, X: {1}, Y: {2}, Sector X: {3}, Sector Y: {4}, Adjusted X: {5}, Adjusted Y: {6}", (int)dim, x, y, CoordToSectorNumber(x), CoordToSectorNumber(y), x % nodesPerSector, y % nodesPerSector));
    //    throw new InvalidDimensionException();
    //}
  }

  void Grid::ModifyArea(char dimension, bool isWalkable, Vector3<float> bottomLeft, Vector3<float> topRight)
  {
    Vector2<int> BL = NodeFromWorldPoint(bottomLeft);

    Vector2<int> TR = NodeFromWorldPoint(topRight);

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

    for (int x = BL.x; x <= TR.x; ++x)
    {
      for (int y = BL.y; y <= TR.y; ++y)
      {
        SetWalkableAt(dimension, x, y, isWalkable);
      }
    }

    //Queue of threads along with a queue to store index of the sector being updated
    std::queue<std::thread> modifyThreads = std::queue<std::thread>();
    std::queue<std::pair<int, int>> sectorQueue = std::queue<std::pair<int, int>>();

    //For each x sector
    for (int x = CoordToSectorNumber(BL.x); x <= CoordToSectorNumber(TR.x); ++x)
    {
      //For each y sector
      for (int y = CoordToSectorNumber(BL.y); y <= CoordToSectorNumber(TR.y); ++y)
      {
        int locX = x;
        int locY = y;
        //New thread
        std::thread t = std::thread(&Grid::ThreadUpdateSubsectors, this, dimension, locX, locY);
        //Add the new thread and the location to the queues
        modifyThreads.push(std::move(t));
        sectorQueue.push(std::pair<int, int>(x, y));
      }
    }

    while (modifyThreads.size() > 0)
    {
      modifyThreads.front().join();
      modifyThreads.pop();
    }

    //For each x sector
    for (int x = clamp(CoordToSectorNumber(BL.x) - 1, 0, sectorCount.x); x <= clamp(CoordToSectorNumber(TR.x) + 1, 0, sectorCount.x); ++x)
    {
      //For each y sector
      for (int y = clamp(CoordToSectorNumber(BL.y) - 1, 0, sectorCount.y); y <= clamp(CoordToSectorNumber(TR.y) + 1, 0, sectorCount.y); ++y)
      {
        int locX = x;
        int locY = y;
        //New thread
        std::thread t = std::thread(&Grid::ThreadUpdateSubsectorConnectionsMultiDim, this, dimension, locX, locY);
        //Add the new thread and the location to the queues
        modifyThreads.push(std::move(t));
      }
    }

    while (modifyThreads.size() > 0)
    {
      modifyThreads.front().join();
      modifyThreads.pop();
    }
  }

  void Grid::ThreadUpdateSubsectors(char dimension, int locX, int locY)
  {
    {
      //If not the shared dimension, update just their own
      if (dimension != 2)
      {
        gridSectors[dimension][locX][locY].UpdateSubsectors();
      }
      //Else, this is the shared dimension and both other dimensions need to be updated
      else//if (dimension == 2)
      {
        gridSectors[0][locX][locY].UpdateSubsectors();
        gridSectors[1][locX][locY].UpdateSubsectors();
      }
      //The shared dimension is always updated
      gridSectors[2][locX][locY].UpdateSubsectors();
    }
  }

  void Grid::ThreadUpdateSubsectorConnectionsMultiDim(char dimension, int locX, int locY)
  {
    //If not the shared dimension, update just their own
    if (dimension != 2)
    {
      for (auto it = gridSectors[dimension][locX][locY].subs.begin(); it != gridSectors[dimension][locX][locY].subs.end(); ++it)
      {
        (*it)->UpdateConnectedSubsectors();
      }
    }
    //Else, this is the shared dimension and both other dimensions need to be updated
    else//if (dimension == 2)
    {
      for (auto it = gridSectors[0][locX][locY].subs.begin(); it != gridSectors[0][locX][locY].subs.end(); ++it)
      {
        (*it)->UpdateConnectedSubsectors();
      }
      for (auto it = gridSectors[1][locX][locY].subs.begin(); it != gridSectors[1][locX][locY].subs.end(); ++it)
      {
        (*it)->UpdateConnectedSubsectors();
      }
    }
    //The shared dimension is always updated
    for (auto it = gridSectors[2][locX][locY].subs.begin(); it != gridSectors[2][locX][locY].subs.end(); ++it)
    {
      (*it)->UpdateConnectedSubsectors();
    }
  }

  int Grid::CoordToSectorNumber(int n)
  {
    return n / nodesPerSector;
  }

  /// <summary>
  /// Returns True if the rectangular area between bottomLeft and topRight has any obstacles blocking the given dimension.
  /// </summary>
  /// <param name="dimension">The dimension to be checked against as a LayerMask. 2^8, 2^9, 2^10)</param>
  /// <param name="bottomLeft">Location of Bottom Left corner. (X, UNUSED, Y)</param>
  /// <param name="topRight">Location of Top Right corner. (X, UNUSED, Y)</param>
  /// <returns></returns>
  bool Grid::AreaHasObstacle(char dimension, Vector3<float> bottomLeft, Vector3<float> topRight)
  {
    Vector2<int> BL = grid_Instance->NodeFromWorldPoint(bottomLeft);

    Vector2<int> TR = grid_Instance->NodeFromWorldPoint(topRight);

    for (int y = BL.y; y <= TR.y; ++y)
    {
      for (int x = BL.x; x <= TR.x; ++x)
      {
        if (!GetWalkableAt(dimension, x, y))
          return true;
      }
    }

    return false;
  }

  void Grid::CreateGrid()
  {
    sectorCount = Vector2<int>(((int)worldSize.x - 1) / sectorSize + 1, ((int)worldSize.y - 1) / sectorSize + 1);

    gridSectors = new Sector**[3];
    for (int d = 0; d < 3; ++d)
    {
      gridSectors[d] = new Sector*[sectorCount.x];
      for (int x = 0; x < sectorCount.x; ++x)
      {
        typedef std::aligned_storage<sizeof(Sector), std::alignment_of<Sector>::value>::type sectorStorage;
        gridSectors[d][x] = reinterpret_cast<Sector*>(new sectorStorage[sectorCount.y]);
      }
    }

    GenerateSectors(1 << 8);//dim1);
    GenerateSectors(1 << 9);//dim2);
    GenerateSectors(1 << 8 ^ 1 << 9 ^ 1 << 10);//dim3 ^ dim2 ^ dim1);

    std::queue<std::thread> modifyThreads = std::queue<std::thread>();

    //For each x sector
    for (int x = 0; x < sectorCount.x; ++x)
    {
      //For each y sector
      for (int y = 0; y < sectorCount.y; ++y)
      {
        for (int dimension = 0; dimension < 3; ++dimension)
        {
          int locX = x;
          int locY = y;
          int locDim = dimension;
          //New thread
          std::thread t = std::thread(&Grid::ThreadUpdateSubsectorConnectionsOneDim, this, locDim, locX, locY);
          //Add the new thread and the location to the queues
          modifyThreads.push(std::move(t));
        }
      }
    }

    while (modifyThreads.size() > 0)
    {
      modifyThreads.front().join();
      modifyThreads.pop();
    }
  }

  void Grid::DeleteGrid()
  {
    if (gridSectors == NULL)
      return;

    for (int d = 0; d < 3; ++d)
    {
      for (int xS = 0; xS < sectorCount.x; ++xS)
      {
        for (int yS = 0; yS < sectorCount.y; ++yS)
          gridSectors[d][xS][yS].~Sector();
         //delete[] gridSectors[d][xS];
        ::operator delete(gridSectors[d][xS]);
      }
      delete[] gridSectors[d];
    }
    delete[] gridSectors;

    gridSectors = NULL;
  }

  void  Grid::ThreadUpdateSubsectorConnectionsOneDim(char dimension, int locX, int locY)
  {
    for (auto it = gridSectors[dimension][locX][locY].subs.begin(); it != gridSectors[dimension][locX][locY].subs.end(); ++it)
    {
      (*it)->UpdateConnectedSubsectors();
    }
  }

  void Grid::GenerateSectors(int mask)
  {
    //CustomLogger logger = new CustomLogger(@"C:\Users\drago\Documents\GitHub\Personal_RTS\Personal_RTS\Assets\Logs\SectorLog.log");
    for (int x = 0; x < sectorCount.x; ++x)
    {
      for (int y = 0; y < sectorCount.y; ++y)
      {
        Vector3<float> worldPoint = Vector3<float>((x * sectorSize) + nodeRadius, 0, (y * sectorSize) + nodeRadius);//transform.position + Vector3.right * ((x * SectorSize) + nodeRadius) + Vector3.forward * ((y * SectorSize) + nodeRadius);


        new (gridSectors[(int)log2(mask) - 8][x] + y) Sector(mask,
                                                            (int)((x + 1) * sectorSize <= worldSize.x ? sectorSize : (int)worldSize.x % sectorSize),
                                                            (int)((y + 1) * sectorSize <= worldSize.y ? sectorSize : (int)worldSize.y % sectorSize),
                                                            worldPoint, nodeDiameter, x, y);
      }
    }
  }

  Vector2<int> Grid::NodeFromWorldPoint(Vector3<float> worldPosition)
  {
    float percentX = worldPosition.X / worldSize.x;
    float percentY = worldPosition.Z / worldSize.y;
    percentX = clamp(percentX, 0.0f, 1.0f);
    percentY = clamp(percentY, 0.0f, 1.0f);

    int x = (int)round((gridSize.x - 1) * percentX);
    int y = (int)round((gridSize.y - 1) * percentY);

    return Vector2<int>(x, y);// grid[x, y];
  }
}

extern "C"
{
  using namespace::TerrainData;

  GRID_API void ChangeSectorSize(int newSize)
  {
    Grid::GetGrid()->changeSectorSize(newSize);
  }
  GRID_API void ChangeWorldSize(float x, float y)
  {
    Grid::GetGrid()->changeWorldSize(x, y);
  }
  GRID_API void ChangeNodeRadius(float newRadius)
  {
    Grid::GetGrid()->changeNodeRadius(newRadius);
  }

  GRID_API void ModifyBlockage(char dimension, bool isWalkable, float blX, float blY, float blZ, float trX, float trY, float trZ)
  {
    Grid::GetGrid()->ModifyArea(dimension, isWalkable, Vector3<float>(blX, blY, blZ), Vector3<float>(trX, trY, trZ));
  }

  GRID_API bool AreaHasObstacle(char dimension, float blX, float blY, float blZ, float trX, float trY, float trZ)
  {
    return Grid::GetGrid()->AreaHasObstacle(dimension, Vector3<float>(blX, blY, blZ), Vector3<float>(trX, trY, trZ));
  }

  GRID_API Grid* NewGrid(float _nodeRadius, float _worldSizeX, float _worldSizeY, int _sectorSize)
  {
    return new Grid(_nodeRadius, _worldSizeX, _worldSizeY, _sectorSize);
  }

  GRID_API void DestroyGrid(Grid* pGrid)
  {
    delete pGrid;
  }
}
