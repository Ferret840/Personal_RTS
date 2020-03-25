#pragma once

#ifndef INTEGRATIONFIELD
#define INTEGRATIONFIELD

#include "stdafx.h"
#include "Goal.h"
#include "Grid.h"

namespace Pathing
{
  class Goal;
  class IntegrationField;

  class IntegrationField
  {
  private:
    class IFieldNode
    {
    private:
      int xPos, yPos;
      IntegrationField* iField;
    protected:

    public:
      unsigned int Distance;
      bool Used;// = false;

      IFieldNode(int x, int y, IntegrationField* i);

      std::list<IFieldNode*> GetNeighbors(char dim);
    };

    Goal* goal;
    IFieldNode**** grid;

  public:

    IntegrationField(Goal* _goal);
    ~IntegrationField();

    void Calculate();

    int GetDistAt(int x, int y);
  };
}

#endif