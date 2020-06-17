#pragma once

#ifndef GOAL
#define GOAL

#define GOAL_API __declspec(dllexport)

#include "stdafx.h"
#include "IntegrationField.h"
#include "FlowField.h"
#include "Vectors.h"
#include <unordered_set>
#include "Grid.h"
#include <math.h>

#ifndef PI
#define PI 3.14159265358979323846
#endif

namespace Pathing
{
  static const int iFlowField_Thread_Count = std::thread::hardware_concurrency();

  class FlowField;
  class IntegrationField;

  class Goal
  {
  private:
    const Vector3<float> position;
    std::unordered_set<int>* ownerIDs;// = new HashSet<Owner>();
    int xPos, yPos, xSector, ySector;

    IntegrationField* iField;
    FlowField* fField;

    bool threadIsComplete;
    std::thread calculateThread;

    void GenerateFields();
    static void DelayedGoalDestruction(Goal* goal);
  protected:

  public:
    // ;
    //int PlayerNum;
    const char dimension;
    int getxPos();
    int getyPos();
    int getxSector();
    int getySector();
    char getDimension();
    int getOwnerCount();

    Goal(int _playerNum, char _dimension, float _posX, float _posY, float _posZ);
    ~Goal();

    void AddOwner(int oID);

    void RemoveOwner(int oID);

    void ClearOwners();

    void TransferOwners(Goal* newGoal);

    float GetDirFromPosition(float worldX, float worldY, float worldZ);
  };
}

extern "C"
{
  using namespace::Pathing;

  GOAL_API int GetXPos(Goal* pGoal);
  GOAL_API int GetYPos(Goal* pGoal);
  GOAL_API int GetXSector(Goal* pGoal);
  GOAL_API int GetYSector(Goal* pGoal);
  GOAL_API char GetDimension(Goal* pGoal);
  GOAL_API int GetOwnerCount(Goal* pGoal);

  GOAL_API void AddOwner(Goal* pGoal, int oID);
  GOAL_API void RemoveOwner(Goal* pGoal, int oID);
  GOAL_API void ClearOwners(Goal* pGoal);
  GOAL_API void TransferOwners(Goal* originalGoal, Goal* newGoal);

  GOAL_API float GetDirFromPosition(Goal* pGoal, float worldX, float worldY, float worldZ);

  GOAL_API Goal* NewGoal(int _playerNum, char _dimension, float _posX, float _posY, float _posZ);
}

#endif