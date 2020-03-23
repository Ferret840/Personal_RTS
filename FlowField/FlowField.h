#pragma once

#ifndef FLOWFIELD
#define FLOWFIELD

#include "stdafx.h"
#include "Goal.h"
#include "Grid.h"
#include "IntegrationField.h"


namespace Pathing
{
  class IntegrationField;

  class FlowField
  {
  private:
    float** grid;
    std::queue<std::thread> generateThreads;

    void CalculateSection(int x, int y, IntegrationField* iField, int gridSizeX, int gridSizeY);
    void ThreadFunction(int sizeX, int sizeY, int thread_Num, IntegrationField* iField);
  protected:

  public:
    float getDirAt(int x, int y);

    FlowField(IntegrationField* iField, int numThreads = 1);
    ~FlowField();
  };
}

#endif