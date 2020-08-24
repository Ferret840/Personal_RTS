#pragma once

#ifndef FLOWFIELD
#define FLOWFIELD

#include "stdafx.h"
#include "Goal.h"
#include "Grid.h"
#include "IntegrationField.h"


namespace Pathing
{
#define ENDDIRECTION 69.0f
#define STUCKDIRECTION -69.0f
#define UP 0.0f
#define UPRIGHT 45.0f
#define UPLEFT 315.0f
#define LEFT 270.0f
#define RIGHT 90.0f
#define DOWN 180.0f
#define DOWNRIGHT 135.0f
#define DOWNLEFT 225.0f

  class IntegrationField;

  class FlowField
  {
  private:
    float** grid;
    std::queue<std::thread> generateThreads;

    void CalculateSection(int x, int y, IntegrationField* iField, int gridSizeX, int gridSizeY);
    void GetMins(IntegrationField* iField, int x, int y, int deltaX, int deltaY, int gridSizeX, int gridSizeY, int& minD, int& minX, int& minY);
    void ThreadFunction(int sizeX, int sizeY, int thread_Num, IntegrationField* iField);
  protected:

  public:
    float getDirAt(int x, int y);

    FlowField(IntegrationField* iField, int numThreads = 1);
    ~FlowField();
  };
}

#endif