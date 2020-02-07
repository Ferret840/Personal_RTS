using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Selectable;
using TerrainData;

namespace Pathing
{

    public class Goal
    {
        //int PlayerNum;
        public char Dimension
        {
            get;
            private set;
        }

        IntegrationField iField;
        FlowField fField;

        HashSet<Owner> owners = new HashSet<Owner>();

        public int xPos
        {
            get;
            private set;
        }
        public int yPos
        {
            get;
            private set;
        }
        public int xSector
        {
            get;
            private set;
        }
        public int ySector
        {
            get;
            private set;
        }

        public Goal(int _playerNum, char _dimension, Vector3 _position)
        {
            //PlayerNum = _playerNum;
            Dimension = _dimension;

            int x, y;
            Grid g = Grid.GetGrid;

            Grid.NodeFromWorldPoint(_position, out x, out y);

            xPos = x % g.nodesPerSector;
            yPos = y % g.nodesPerSector;

            xSector = g.CoordToSectorNumber(x);
            ySector = g.CoordToSectorNumber(y);

            iField = new IntegrationField(this);

            fField = new FlowField(iField);
        }

        public void AddOwner(Owner o)
        {
            owners.Add(o);
        }

        public void RemoveOwner(Owner o)
        {
            owners.Remove(o);
        }

        public float GetDirFromPosition(Vector3 position)
        {
            int x, y;

            Grid.NodeFromWorldPoint(position, out x, out y);

            return fField[x, y];
        }
    }

}
