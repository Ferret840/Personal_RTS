// FlowField.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "FlowField.h"
#ifdef _DEBUG
#include <iostream>
#include <fstream>
#include <iomanip>
#endif

namespace Pathing
{
  using namespace TerrainData;

  void FlowField::CalculateSection(int x, int y, IntegrationField* iField, int gridSizeX, int gridSizeY)
  {
    int minD = iField->GetDistAt(x, y);
    int minX = 0, minY = 0;

    //-1,0
    //1,0
    //0,1
    //0,-1

    //-1,-1
    //1,1
    //-1,1
    //1,-1

    //Orthogonal
    GetMins(iField, x, y, -1, 0, gridSizeX, gridSizeY, minD, minX, minY);
    GetMins(iField, x, y,  1, 0, gridSizeX, gridSizeY, minD, minX, minY);
    GetMins(iField, x, y,  0, 1, gridSizeX, gridSizeY, minD, minX, minY);
    GetMins(iField, x, y,  0,-1, gridSizeX, gridSizeY, minD, minX, minY);
    //Diagonal
    GetMins(iField, x, y, -1, -1, gridSizeX, gridSizeY, minD, minX, minY);
    GetMins(iField, x, y,  1,  1, gridSizeX, gridSizeY, minD, minX, minY);
    GetMins(iField, x, y, -1,  1, gridSizeX, gridSizeY, minD, minX, minY);
    GetMins(iField, x, y,  1, -1, gridSizeX, gridSizeY, minD, minX, minY);

    //Because Unity is stupid and circles start at up and rotate clockwise
    if (minD == 0)
      grid[x][y] = ENDDIRECTION;
    else if (minX == 1 && minY == 0)
      grid[x][y] = RIGHT;
    else if (minX == -1 && minY == 0)
      grid[x][y] = LEFT;
    else if (minX == 0 && minY == 1)
      grid[x][y] = UP;
    else if (minX == 0 && minY == -1)
      grid[x][y] = DOWN;
    else if (minX == 1 && minY == 1)
      grid[x][y] = UPRIGHT;
    else if (minX == -1 && minY == 1)
      grid[x][y] = UPLEFT;
    else if (minX == 1 && minY == -1)
      grid[x][y] = DOWNRIGHT;
    else if (minX == -1 && minY == -1)
      grid[x][y] = DOWNLEFT;
    else//if (minD == iField->GetDistAt(x, y))
      grid[x][y] = STUCKDIRECTION;
  }

  void FlowField::GetMins(IntegrationField* iField, int x, int y, int deltaX, int deltaY, int gridSizeX, int gridSizeY, int& minD, int& minX, int& minY)
  {
    int atX = x + deltaX;
    int atY = y + deltaY;

    if (deltaX == 0 && deltaY == 0)
      return;
    else if (atX < 0 || atX >= gridSizeX || atY < 0 || atY >= gridSizeY)
      return;

    int checkD = iField->GetDistAt(atX, atY);
    if (checkD < minD)
    {
      minD = checkD;
      minX = deltaX;
      minY = deltaY;
    }
  }

  void FlowField::ThreadFunction(int sizeX, int sizeY, int thread_Num, IntegrationField* iField)
  {
    for (int x = 0; x < sizeX; ++x)
    {
      for (int y = thread_Num * (sizeY / iFlowField_Thread_Count); y < (thread_Num + 1) * (sizeY / iFlowField_Thread_Count); ++y)
      {
        CalculateSection(x, y, iField, sizeX, sizeY);
      }
    }
  }

  float FlowField::getDirAt(int x, int y)
  {
    return grid[x][y];
  }

  FlowField::FlowField(IntegrationField * iField, int numThreads)
  {
    Grid* g = Grid::GetGrid();
    int gridSizeX = g->getGridSize().x, gridSizeY = g->getGridSize().y;

    grid = new float*[gridSizeX];
    for (int i = 0; i < gridSizeX; ++i)
      grid[i] = new float[gridSizeY];

    generateThreads = std::queue<std::thread>();

    for (int i = 0; i < numThreads; ++i)
    {
      generateThreads.push(std::thread(&FlowField::ThreadFunction, this, gridSizeX, gridSizeY, i, iField));
    }

    for (int x = 0; x < gridSizeX; ++x)
    {
      for (int y = numThreads * (gridSizeY / numThreads); y < gridSizeY; ++y)
      {
        CalculateSection(x, y, iField, gridSizeX, gridSizeY);
      }
    }

    while (generateThreads.size() > 0)
    {
      generateThreads.front().join();
      generateThreads.pop();
    }

#ifdef _DEBUG
    std::ofstream myFile;
    myFile.open("FlowFieldOutput.ffdbg");
    for (int y = gridSizeY - 1; y >= 0; --y)
    {
      for (int x = 0; x < gridSizeX; ++x)
      {
        myFile << std::setw(3) << grid[x][y] << " - ";
      }
      myFile << std::endl;
    }
    myFile.close();
#endif
  }

  FlowField::~FlowField()
  {
    while (generateThreads.size() > 0)
    {
      generateThreads.front().std::thread::~thread();
      generateThreads.pop();
    }

    Grid* g = Grid::GetGrid();
    int gridSizeX = g->getGridSize().x, gridSizeY = g->getGridSize().y;
    
    if (grid != nullptr)
    {
      for (int i = 0; i < gridSizeX; ++i)
        if(grid[i] != nullptr)
          delete[] grid[i];
      delete[] grid;
    }
  }

}
