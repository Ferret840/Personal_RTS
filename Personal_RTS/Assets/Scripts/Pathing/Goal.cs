using UnityEngine;
using System.Runtime.InteropServices;
using System;

namespace Pathing
{
    public class Goal
    {
        [DllImport("RTS_DLL", EntryPoint = "NewGoal")]
        static extern IntPtr NewGoal(int _playerNum, char _dimension, float _posX, float _posY, float _posZ);

        [DllImport("RTS_DLL", EntryPoint = "GetDirFromPosition")]
        static extern float GetDirFromPosition(IntPtr pGoal, float _posX, float _posY, float _posZ);

        [DllImport("RTS_DLL", EntryPoint = "GetXPos")]
        static extern int GetXPos(IntPtr pGoal);
        [DllImport("RTS_DLL", EntryPoint = "GetYPos")]
        static extern int GetYPos(IntPtr pGoal);
        [DllImport("RTS_DLL", EntryPoint = "GetXSector")]
        static extern int GetXSector(IntPtr pGoal);
        [DllImport("RTS_DLL", EntryPoint = "GetYSector")]
        static extern int GetYSector(IntPtr pGoal);
        [DllImport("RTS_DLL", EntryPoint = "GetDimension")]
        static extern char GetDimension(IntPtr pGoal);

        [DllImport("RTS_DLL", EntryPoint = "AddOwner")]
        static extern void AddOwner(IntPtr pGoal, int oID);
        [DllImport("RTS_DLL", EntryPoint = "RemoveOwner")]
        static extern void RemoveOwner(IntPtr pGoal, int oID);

        IntPtr goalPtr;

        public Vector3 position
        {
            get;
            private set;
        }

        public int xPos
        {
            get
            {
                return GetXPos(goalPtr);
            }
        }

        public int yPos
        {
            get
            {
                return GetYPos(goalPtr);
            }
        }

        public int xSector
        {
            get
            {
                return GetXSector(goalPtr);
            }
        }

        public int ySector
        {
            get
            {
                return GetYSector(goalPtr);
            }
        }

        public char Dimension
        {
            get
            {
                return GetDimension(goalPtr);
            }
        }

        public Goal(int _playerNum, char _dimension, Vector3 _position)
        {
            goalPtr = NewGoal(_playerNum, _dimension, _position.x, _position.y, _position.z);
            position = _position;
        }

        public float GetDirFromPosition(Vector3 _position)
        {
            return GetDirFromPosition(goalPtr, _position.x, _position.y, _position.z);
        }

        public void AddOwner(int oID)
        {
            AddOwner(goalPtr, oID);
        }

        public void RemoveOwner(int oID)
        {
            RemoveOwner(goalPtr, oID);
        }
    }

}
