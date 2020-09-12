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

  int Goal::getxPos() const
  {
    return xPos;
  }

  int Goal::getyPos() const
  {
    return yPos;
  }

  int Goal::getxSector() const
  {
    return xSector;
  }

  int Goal::getySector() const
  {
    return ySector;
  }

  const Vector2<int> Goal::getBLPosition() const
  {
    return bottomLeftNode;
  }

  const Vector2<int> Goal::getTRPosition() const
  {
    return topRightNode;
  }

  char Goal::getDimension()
  {
    return dimension;
  }

  int Goal::getOwnerCount()
  {
    return (int)ownerIDs->size();
  }

  Goal::Goal(int _playerNum, char _dimension, float _posX, float _posY, float _posZ) : Goal(_playerNum, _dimension, _posX, _posY, _posZ, _posX, _posY, _posZ)
  {
  }

  Goal::Goal(int _playerNum, char _dimension, float _bottomLeftX, float _bottomLeftY, float _bottomLeftZ, float _topRightX, float _topRightY, float _topRightZ) :
    dimension(_dimension),
    bottomLeft(Vector3<float>(_bottomLeftX, _bottomLeftY, _bottomLeftZ)), topRight(Vector3<float>(_topRightX, _topRightY, _topRightZ)),
    position(Vector3<float>((bottomLeft + topRight) / 2.0f)), bottomLeftNode(Grid::GetGrid()->NodeFromWorldPoint(bottomLeft)), topRightNode(Grid::GetGrid()->NodeFromWorldPoint(topRight)),
    threadIsComplete(false), calculateThread(std::thread(&Goal::GenerateFields, this)),
    ownerIDs(new std::unordered_set<int>())
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
    if(calculateThread.joinable())
      calculateThread.join();
    
    if (ownerIDs != nullptr)
      delete ownerIDs;

    delete fField;
    delete iField;
  }

  void Goal::AddOwner(int oID)
  {
    ownerIDs->insert(oID);
  }

  void Goal::RemoveOwner(int oID)
  {
    ownerIDs->erase(oID);
    if (ownerIDs->size() == 0)
      delete this;
  }

  void Goal::ClearOwners()
  {
    ownerIDs->clear();
    delete this;
  }

  void Goal::TransferOwners(Goal* newGoal)
  {
    delete newGoal->ownerIDs;
    newGoal->ownerIDs = ownerIDs;
    ownerIDs = nullptr;
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

    //int xPlus1 = node.x + 1 < g->getGridSize().x ? 1 : 0;
    //int yPlus1 = node.y + 1 < g->getGridSize().y ? 1 : 0;
    //int xMinus1 = node.x == 0 ? 0 : -1;
    //int yMinus1 = node.y == 0 ? 0 : -1;
    //
    //int count = (xPlus1 - xMinus1) * (yPlus1 - yMinus1);
    //
    //float xAngle = 0;
    //float yAngle = 0;
    //
    //for (int x = xMinus1; x < xPlus1; ++x)
    //{
    //  for (int y = yMinus1; y < yPlus1; ++y)
    //  {
    //    float angle = fField->getDirAt(node.x + x, node.y + y);
    //
    //    if (x == 0 && y == 0 && (angle == UP || angle == DOWN || angle == RIGHT || angle == LEFT))
    //      return angle;
    //
    //    xAngle += cosf(angle * DEG2RAD);
    //    yAngle += sinf(angle * DEG2RAD);
    //  }
    //}
    //
    //float avg = atan2f(yAngle, xAngle) / DEG2RAD;
    //
    //return avg;
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
  GOAL_API int GetOwnerCount(Goal* pGoal)
  {
    return pGoal->getOwnerCount();
  }

  GOAL_API void AddOwner(Goal* pGoal, int oID)
  {
    pGoal->AddOwner(oID);
  }
  GOAL_API void RemoveOwner(Goal* pGoal, int oID)
  {
    pGoal->RemoveOwner(oID);
  }
  GOAL_API void ClearOwners(Goal* pGoal)
  {
    pGoal->ClearOwners();
  }
  GOAL_API void TransferOwners(Goal* originalGoal, Goal* newGoal)
  {
    originalGoal->TransferOwners(newGoal);
  }

  GOAL_API float GetDirFromPosition(Goal* pGoal, float worldX, float worldY, float worldZ)
  {
    return pGoal->GetDirFromPosition(worldX, worldY, worldZ);
  }

  GOAL_API Goal* NewGoal(int _playerNum, char _dimension, float _posX, float _posY, float _posZ)
  {
    return new Goal(_playerNum, _dimension, _posX, _posY, _posZ);
  }

  GOAL_API Goal* NewStructureGoal(int _playerNum, char _dimension, float _bottomLeftX, float _bottomLeftY, float _bottomLeftZ, float _topRightX, float _topRightY, float _topRightZ)
  {
    return new Goal(_playerNum, _dimension, _bottomLeftX, _bottomLeftY, _bottomLeftZ, _topRightX, _topRightY, _topRightZ);
  }
}
