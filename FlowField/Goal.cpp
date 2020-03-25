#include "stdafx.h"
#include "Goal.h"

namespace Pathing
{
  static std::unordered_set<Goal*> goalList = std::unordered_set<Goal*>();

  void Goal::GenerateFields()
  {
    iField = new IntegrationField(this);
    fField = new FlowField(iField, iFlowField_Thread_Count);

    threadIsComplete = true;
  }

  int Goal::getxPos()
  {
    return xPos;
  }

  int Goal::getyPos()
  {
    return yPos;
  }

    int Goal::getxSector()
  {
    return xSector;
  }

  int Goal::getySector()
  {
    return ySector;
  }

  char Goal::getDimension()
  {
    return dimension;
  }

  Goal::Goal(int _playerNum, char _dimension, float _posX, float _posY, float _posZ) :
    dimension(_dimension), position(Vector3<float>(_posX, _posY, _posZ)),
    threadIsComplete(false), calculateThread(std::thread(&Goal::GenerateFields, this)),
    ownerIDs(std::unordered_set<int>())
  {
    using namespace::TerrainData;
    Grid* g = Grid::GetGrid();

    Vector2<int> nodePos = g->NodeFromWorldPoint(position);

    xPos = nodePos.x % g->getNodesPerSector();
    yPos = nodePos.y % g->getNodesPerSector();

    xSector = g->CoordToSectorNumber(nodePos.x);
    ySector = g->CoordToSectorNumber(nodePos.y);

    //PlayerNum = _playerNum;
  }

  Goal::~Goal()
  {
    calculateThread.join();
    
    delete fField;
    delete iField;
  }

  void Goal::AddOwner(int oID)
  {
    ownerIDs.insert(oID);
  }

  void Goal::RemoveOwner(int oID)
  {
    ownerIDs.erase(oID);
    if (ownerIDs.size() == 0)
      delete this;
  }

  float Goal::GetDirFromPosition(float worldX, float worldY, float worldZ)
  {
    using namespace::TerrainData;

    if (threadIsComplete == false)
      //return (float)(atan2(position.X - worldX, position.Z - worldZ) * PI / 180.0f);
      return STUCKDIRECTION;

    Grid* g = Grid::GetGrid();
    Vector2<int> node = g->NodeFromWorldPoint(Vector3<float>(worldX, worldY, worldZ));

    return fField->getDirAt(node.x, node.y);
  }
}

extern "C"
{
  using namespace::Pathing;

  GOAL_API int GetXPos(Goal* pGoal)
  {
    return pGoal->getxPos();
  }
  GOAL_API int GetYPos(Goal* pGoal)
  {
    return pGoal->getyPos();
  }
  GOAL_API int GetXSector(Goal* pGoal)
  {
    return pGoal->getxSector();
  }
  GOAL_API int GetYSector(Goal* pGoal)
  {
    return pGoal->getySector();
  }
  GOAL_API char GetDimension(Goal* pGoal)
  {
    return pGoal->getDimension();
  }

  GOAL_API void AddOwner(Goal* pGoal, int oID)
  {
    pGoal->AddOwner(oID);
  }

  GOAL_API void RemoveOwner(Goal* pGoal, int oID)
  {
    pGoal->RemoveOwner(oID);
  }

  GOAL_API float GetDirFromPosition(Goal* pGoal, float worldX, float worldY, float worldZ)
  {
    return pGoal->GetDirFromPosition(worldX, worldY, worldZ);
  }

  GOAL_API Goal* NewGoal(int _playerNum, char _dimension, float _posX, float _posY, float _posZ)
  {
    return new Goal(_playerNum, _dimension, _posX, _posY, _posZ);
  }
}
