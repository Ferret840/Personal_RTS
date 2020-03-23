#include "stdafx.h"
#include "IntegrationField.h"

namespace Pathing
{
  using namespace TerrainData;

  IntegrationField::IFieldNode::IFieldNode(int x, int y, IntegrationField* i) :xPos(x), yPos(y), iField(i), Used(false), Distance(USHRT_MAX)
  {}

  std::list<IntegrationField::IFieldNode*> IntegrationField::IFieldNode::GetNeighbors(char dim)
  {
    Grid* g = Grid::GetGrid();

    std::list<IFieldNode*> neighbors = std::list<IFieldNode*>();

    for (int x = -1; x < 2; ++x)
    {
      for (int y = -1; y < 2; ++y)
      {
        int checkX = xPos + x;
        int checkY = yPos + y;
        if (abs(x) == abs(y))
          continue;
        else if (checkX < 0 || checkX >= g->getGridSize().x || checkY < 0 || checkY >= g->getGridSize().y)
          continue;
      
        if (g->GetWalkableAt(dim, checkX, checkY))
        {
          //int atX = (int)((checkX + 1) * g.SectorSize <= g.gridWorldSize.x ? g.SectorSize / (g.nodeRadius * 2) : g.gridWorldSize.x % g.SectorSize / (g.nodeRadius * 2)),
          //    atY = (int)((checkY + 1) * g.SectorSize <= g.gridWorldSize.y ? g.SectorSize / (g.nodeRadius * 2) : g.gridWorldSize.y % g.SectorSize / (g.nodeRadius * 2));
            
          //push back             iField grid            at X sector                      at Y sector                      at X node                        at Y node
          neighbors.push_back( &(iField->grid[checkX / g->getNodesPerSector()][checkY / g->getNodesPerSector()][checkX % g->getNodesPerSector()][checkY % g->getNodesPerSector()]) );
        }
      }
    }

    return neighbors;
  }

  IntegrationField::IntegrationField(Goal* _goal) : goal(_goal)
  {
    Grid* g = Grid::GetGrid();

    //grid = new IFieldNode***[g->getSectorCount().x];
    //for (int i = 0; i < g->getSectorCount().x; ++i)
    //{
    //  grid[i] = new IFieldNode**[g->getSectorCount().y];
    //}

    int lastSectorRemainderX = (int)g->getWorldSize().x % g->getNodesPerSector(),
        lastSectorRemainderY = (int)g->getWorldSize().y % g->getNodesPerSector();

    grid = new IFieldNode***[g->getSectorCount().x];
    for (int xS = 0; xS < g->getSectorCount().x; ++xS)
    {
      grid[xS] = new IFieldNode**[g->getSectorCount().y];

      for (int yS = 0; yS < g->getSectorCount().y; ++yS)
      {
        int xSize = (xS + 1) * g->getSectorSize() <= g->getWorldSize().x ? g->getNodesPerSector() : lastSectorRemainderX,
            ySize = (yS + 1) * g->getSectorSize() <= g->getWorldSize().y ? g->getNodesPerSector() : lastSectorRemainderY;

        grid[xS][yS] = new IFieldNode*[xSize];

        for (int x = 0; x < xSize; ++x)
        {
          typedef std::aligned_storage<sizeof(IFieldNode), std::alignment_of<IFieldNode>::value>::type storage_type;
          grid[xS][yS][x] = reinterpret_cast<IFieldNode*>(new storage_type[ySize]);

          for (int y = 0; y < ySize; ++y)
            new (grid[xS][yS][x] + y) IFieldNode(x + xS * g->getNodesPerSector(), y + yS * g->getNodesPerSector(), this);
        }
      }
    }

    Calculate();
  }

  IntegrationField::~IntegrationField()
  {
    Grid* g = Grid::GetGrid();

    int lastSectorRemainderX = (int)g->getWorldSize().x % g->getNodesPerSector(),
        lastSectorRemainderY = (int)g->getWorldSize().y % g->getNodesPerSector();

    for (int i = 0; i < g->getSectorCount().x; ++i)
    {
      for (int j = 0; j < g->getSectorCount().y; ++j)
      {
        int xSize = (i + 1) * g->getSectorSize() <= g->getWorldSize().x ? g->getNodesPerSector() : lastSectorRemainderX,
            ySize = (j + 1) * g->getSectorSize() <= g->getWorldSize().y ? g->getNodesPerSector() : lastSectorRemainderY;

        for (int x = 0; x < xSize; ++x)
        {
          for (int y = 0; y < ySize; ++y)
            grid[i][j][x][y].~IFieldNode();
          delete[] grid[i][j][x];
        }
        delete[] grid[i][j];
      }
      delete[] grid[i];
    }
    delete[] grid;
  }

  void IntegrationField::Calculate()
  {
    Grid* g = Grid::GetGrid();

    IFieldNode* n = &grid[goal->getxSector()][goal->getySector()][goal->getxPos()][goal->getyPos()];

    int circumference = (int)(min(g->getGridSize().x, g->getGridSize().y) * 2 * PI);
    std::queue<IFieldNode*> openList = std::queue<IFieldNode*>();

    n->Distance = 0;

    openList.push(n);

    while (openList.size() > 0)
    {
      n = openList.front();

      n->Used = true;
      std::list<IntegrationField::IFieldNode*> neighbors = n->GetNeighbors(goal->dimension);
      for (auto it = neighbors.begin(); it != neighbors.end(); ++it)
      {
        if ((*it)->Used)
          continue;

        (*it)->Distance = n->Distance + 1;
        (*it)->Used = true;
        openList.push(*it);
      }
    }
  }

  int IntegrationField::GetDistAt(int x, int y)
  {
    Grid* g = Grid::GetGrid();

    return grid[x / g->getNodesPerSector()][y / g->getNodesPerSector()][x % g->getNodesPerSector()][y % g->getNodesPerSector()].Distance;
  }
}