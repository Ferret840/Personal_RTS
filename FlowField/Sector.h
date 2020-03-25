#pragma once

#ifndef SECTOR
#define SECTOR

#include "stdafx.h"
#include <list>
#include "Vectors.h"
#include <stack>
#include "Grid.h"

namespace TerrainData
{
  class Sector;

  class Sector
  {
  public:
    class SectorNode;
    class SubSector;

  private:
    int xSize, ySize, dimension, sectorX, sectorY;

    Sector::SectorNode** grid;//need public getter
    Vector3<float> Position;//need public getter

  protected:


  public:
    std::list<SubSector*> subs;

    Sector(int dim, int _xSize, int _ySize, Vector3<float> cornerPos, float nodeDiameter, int _sectorX, int _sectorY);
    ~Sector();

    void UpdateSubsectors();

    bool GetWalkableAt(int x, int y);
    bool SetWalkableAt(int x, int y, bool newCanWalk);

    //Subsector Class
    class SubSector
    {
    private:
      Sector* sect;//Need public getter
      //List<Vector3> verts = new List<Vector3>();
      //Vector3<float>* vertices;
      //int* indices;
      SectorNode**& grid;//return sect.grid;

      void FindConnectedSubsectors(Sector* s, int otherX, int otherY, int selfX, int selfY);

    protected:


    public:

      std::unordered_set<SubSector*> ConnectedSectors;

      SubSector(Sector* parentSector);

      void AddNodeToSub(SectorNode* n);

      void UpdateConnectedSubsectors();
    };

    class SectorNode
    {
    public:
      //public uint subsectorInstance = uint.MaxValue;
      SubSector* Subsector;
      bool Walkable;
      SectorNode();
    };

    struct SectorNodeWithCoord
    {
    public:
      SectorNode* Node;
      int X;
      int Y;

      SectorNodeWithCoord(SectorNode* n, int _x, int _y)
      {
        Node = n;
        X = _x;
        Y = _y;
      }
    };
  };
}

#endif