#include "stdafx.h"
#include "Sector.h"

extern "C"
{
  namespace TerrainData
  {
    Sector::Sector(int dim, int _xSize, int _ySize, Vector3<float> cornerPos, float nodeDiameter, int _sectorX, int _sectorY) :
      xSize((int)round(_xSize / nodeDiameter)), ySize((int)round(_ySize / nodeDiameter)),
      Position(cornerPos), sectorX(_sectorX), sectorY(_sectorY),
      dimension((int)log2(dim) - 8), subs(std::list<SubSector*>())
    {
      grid = new SectorNode*[xSize];
      for (int x = 0; x < xSize; ++x)
      {
        grid[x] = new SectorNode[ySize];
      }

      UpdateSubsectors();
    }

    Sector::~Sector()
    {
      for (int x = 0; x < xSize; ++x)
      {
        delete[] grid[x];
      }
      delete[] grid;

      for (auto it = subs.begin(); it != subs.end(); ++it)
      {
        delete (*it);
      }
    }

    void Sector::UpdateSubsectors()
    {
      for (int x = 0; x < xSize; ++x)
      {
        for (int y = 0; y < ySize; ++y)
        {
          grid[x][y].Subsector = NULL;
        }
      }

      for (auto it = subs.begin(); it != subs.end(); ++it)
      {
        delete (*it);
      }

      subs.clear();

      //HashSet<Node> touchedNodes = new HashSet<Node>();
      std::stack<SectorNodeWithCoord> neighborGettorList = std::stack<SectorNodeWithCoord>();

      for (int x = 0; x < xSize; ++x)
      {
        for (int y = 0; y < ySize; ++y)
        {
          SectorNode* n = &grid[x][y];
          if (n->Walkable && n->Subsector == NULL)//!touchedNodes.Contains(n))
          {
            SubSector* s = new SubSector(this);
            subs.push_back(s);

            //neighborGettorList.Add(new NodeWithCoord(n, x, y));
            neighborGettorList.push(SectorNodeWithCoord(n, x, y));

            while (neighborGettorList.size() > 0)
            {
              SectorNodeWithCoord nodeCoord = neighborGettorList.top();
              neighborGettorList.pop();//[neighborGettorList.Count - 1];
                                                                        //touchedNodes.Add(nodeCoord.node);
              s->AddNodeToSub(nodeCoord.Node);

              SectorNode* neighbor;
              int nextX = nodeCoord.X - 1;
              int nextY = nodeCoord.Y;

              //Get each of the 4 neighbors (a + shape)
              if (nextX >= 0)
              {
                neighbor = &grid[nextX][nextY];
                if (neighbor->Walkable && neighbor->Subsector == NULL)//!touchedNodes.Contains(neighbor))
                {
                  neighborGettorList.push(SectorNodeWithCoord(neighbor, nextX, nextY));
                  //touchedNodes.Add(neighbor);
                }
              }

              nextX += 2;
              if (nextX < xSize)
              {
                neighbor = &grid[nextX][nextY];
                if (neighbor->Walkable && neighbor->Subsector == NULL)//!touchedNodes.Contains(neighbor))
                {
                  neighborGettorList.push(SectorNodeWithCoord(neighbor, nextX, nextY));
                  //touchedNodes.Add(neighbor);
                }
              }

              --nextX;
              --nextY;
              if (nextY >= 0)
              {
                neighbor = &grid[nextX][nextY];
                if (neighbor->Walkable && neighbor->Subsector == NULL)//!touchedNodes.Contains(neighbor))
                {
                  neighborGettorList.push(SectorNodeWithCoord(neighbor, nextX, nextY));
                  //touchedNodes.Add(neighbor);
                }
              }

              nextY += 2;
              if (nextY < ySize)
              {
                neighbor = &grid[nextX][nextY];
                if (neighbor->Walkable && neighbor->Subsector == NULL)//!touchedNodes.Contains(neighbor))
                {
                  neighborGettorList.push(SectorNodeWithCoord(neighbor, nextX, nextY));
                  //touchedNodes.Add(neighbor);
                }
              }
            }
          }
        }
      }

      //subs.Add(new SubSector(this));
      //
      //for (int x = 0; x < xSize; ++x)
      //{
      //    for (int y = 0; y < ySize; ++y)
      //    {
      //        if (grid[x,y] != null)//grid[x, y].isWalkable)
      //            subs[0].AddNodeToSub(grid[x, y]);
      //    }
      //}

      //foreach(SubSector s in subs)
      //    s.GenerateMesh();

      //Debug.Log(string.Format("Generated Subsectors for Sector {0} in {1}ms", this.ToString(), timer.ElapsedMilliseconds));
    }

    bool Sector::GetWalkableAt(int x, int y)
    {
      return grid[x][y].Walkable;
    }

    bool Sector::SetWalkableAt(int x, int y, bool newCanWalk)
    {
      grid[x][y].Walkable = newCanWalk;

      return newCanWalk;//grid[x, y].SetWalkable(newCanWalk);
    }

    //Subsector Class
    Sector::SubSector::SubSector(Sector* parentSector) :
      sect(parentSector), grid(parentSector->grid),
      ConnectedSectors(std::unordered_set<SubSector*>())
    {

    }

    void Sector::SubSector::AddNodeToSub(SectorNode* n)
    {
      //n.subsectorInstance = SubsectorIdentity;
      n->Subsector = this;
      //NodesInSubsector.Add(n);
    }

    void Sector::SubSector::UpdateConnectedSubsectors()
    {
      ConnectedSectors.clear();

      Grid* g = Grid::GetGrid();

      Sector* s;

      if (sect->sectorY - 1 >= 0)
      {
        s = g->getSector(sect->dimension, sect->sectorX, sect->sectorY - 1);//[sect->dimension][sect->sectorX][sect->sectorY - 1];

        for (int x = 0; x < sect->xSize; ++x)
        {
          FindConnectedSubsectors(s, x, s->ySize - 1, x, 0);
        }
      }

      if (sect->sectorY + 1 < g->getSectorCount().y)
      {
        s = g->getSector(sect->dimension, sect->sectorX, sect->sectorY + 1);//GridSectors[sect.dimension, sect.sectorX, sect.sectorY + 1];

        for (int x = 0; x < sect->xSize; ++x)
        {
          FindConnectedSubsectors(s, x, 0, x, sect->ySize - 1);
        }
      }

      if (sect->sectorX - 1 >= 0)
      {
        s = g->getSector(sect->dimension, sect->sectorX - 1, sect->sectorY);//GridSectors[sect.dimension, sect.sectorX - 1, sect.sectorY];

        for (int y = 0; y < sect->ySize; ++y)
        {
          FindConnectedSubsectors(s, s->xSize - 1, y, 0, y);
        }
      }

      if (sect->sectorX + 1 < g->getSectorCount().x)
      {
        s = g->getSector(sect->dimension, sect->sectorX + 1, sect->sectorY);//GridSectors[sect.dimension, sect.sectorX + 1, sect.sectorY];

        for (int y = 0; y < sect->ySize; ++y)
        {
          FindConnectedSubsectors(s, 0, y, sect->xSize - 1, y);
        }
      }
    }

    void Sector::SubSector::FindConnectedSubsectors(Sector* s, int otherX, int otherY, int selfX, int selfY)
    {
      if (!s->grid[otherX][otherY].Walkable || !grid[selfX][selfY].Walkable)
        return;

      if (grid[selfX][selfY].Subsector == this)
      {
        ConnectedSectors.insert(s->grid[otherX][otherY].Subsector);
        //s.grid[otherX, otherY].subsector.ConnectedSectors.Add(this);
      }
    }

    Sector::SectorNode::SectorNode() : Subsector(NULL), Walkable(true)
    {

    }
  }
}