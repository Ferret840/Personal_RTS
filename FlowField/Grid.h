#pragma once

#ifndef GRID
#define GRID

#define GRID_API __declspec(dllexport)

#include "stdafx.h"
#include "Vectors.h"
#include "Sector.h"

namespace TerrainData
{
  class Sector;

  class Grid
  {
  private:
    float nodeRadius;
    float nodeDiameter;
    int sectorSize;
    Vector2<float> worldSize;
    int nodesPerSector;
    Vector2<int> gridSize, sectorCount;
    Sector*** gridSectors;
    static Grid* grid_Instance;

    bool GetWalkableAt(int dimension, int xCoord, int yCoord);
    void SetWalkableAt(int dimension, int xCoord, int yCoord, bool newCanWalk);
    void SetWalkableAt(char dim, int xCoord, int yCoord, bool newCanWalk);
    void AssignWalkable(char dim, int x, int y, bool newCanWalk);
    void CreateGrid();
    void GenerateSectors(int mask);
    void DeleteGrid();

    void ThreadUpdateSubsectors(char dimension, int locX, int locY);
    void ThreadUpdateSubsectorConnectionsMultiDim(char dimension, int locX, int locY);
    void ThreadUpdateSubsectorConnectionsOneDim(char dimension, int locX, int locY);
      
  protected:

  public:
    Grid(float _nodeRadius = 0.1f, float _worldSizeX = 100.0f, float _worldSizeY = 100.0f, int _sectorSize = 10);
    ~Grid();

    static Grid* GetGrid() { return Grid::grid_Instance; }

    Vector2<int> getGridSize();
    Vector2<int> getSectorCount();
    Vector2<float> getWorldSize();
    int getNodesPerSector();
    int getSectorSize();
    Sector* getSector(int dim, int x, int y);
    void changeSectorSize(int newSize);
    void changeWorldSize(float x, float y);
    void changeNodeRadius(float newRadius);
      
    bool GetWalkableAt(char dim, int xCoord, int yCoord);
    void ModifyArea(char dimension, bool isWalkable, Vector3<float> bottomLeft, Vector3<float> topRight);
    int CoordToSectorNumber(int n);

    /// <summary>
    /// Returns True if the rectangular area between bottomLeft and topRight has any obstacles blocking the given dimension.
    /// </summary>
    /// <param name="dimension">The dimension to be checked against as a LayerMask. 2^8, 2^9, 2^10)</param>
    /// <param name="bottomLeft">Location of Bottom Left corner. (X, UNUSED, Y)</param>
    /// <param name="topRight">Location of Top Right corner. (X, UNUSED, Y)</param>
    /// <returns></returns>
    bool AreaHasObstacle(char dimension, Vector3<float> bottomLeft, Vector3<float> topRight);

    Vector2<int> NodeFromWorldPoint(Vector3<float> worldPosition);
    //void Update();

    GRID_API static Grid* NewGrid(float _nodeRadius = 0.1f, float _worldSizeX = 100.0f, float _worldSizeY = 100.0f, int _sectorSize = 10);
    GRID_API static void DestroyGrid(Grid* pGrid);
  };

    //---------------------------------------------Non-converted------------------------------//





  //--------------------------------------------------------------------------------------------//
  class InvalidDimensionException : std::exception
  {
  public:
    InvalidDimensionException() : std::exception("Invalid Dimension Given")
    {
    }

    InvalidDimensionException(int dimension) : std::exception("Invalid Dimension Given: Dimension {0}", dimension)
    {

    }
  };

  class SubsectorMeshException : std::exception
  {
  public:
    SubsectorMeshException() : std::exception("Unable to find subsector bounds")
    {
    }
  };
}

extern "C"
{
  using namespace::TerrainData;

  GRID_API void ChangeSectorSize(int newSize);
  GRID_API void ChangeWorldSize(float x, float y);
  GRID_API void ChangeNodeRadius(float newRadius);

  GRID_API void ModifyBlockage(char dimension, bool isWalkable, float blX, float blY, float blZ, float trX, float trY, float trZ);

  GRID_API bool AreaHasObstacle(char dimension, float blX, float blY, float blZ, float trX, float trY, float trZ);

  GRID_API Grid* NewGrid(float _nodeRadius = 0.1f, float _worldSizeX = 100.0f, float _worldSizeY = 100.0f, int _sectorSize = 10);

  GRID_API void DestroyGrid(Grid* pGrid);
}

#endif