using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Collections;
using System.Collections.Generic;


namespace Pathing
{
    public class Goal
    {
        //[TODO: Make goal listen to targeted building/unit and remove goal on target death]

        [DllImport("RTS_DLL", EntryPoint = "NewGoal")]
        static extern IntPtr NewGoal(int _playerNum, char _dimension, float _posX, float _posY, float _posZ);
        [DllImport("RTS_DLL", EntryPoint = "NewStructureGoal")]
        static extern IntPtr NewStructureGoal(int _playerNum, char _dimension, float _bottomLeftX, float _bottomLeftY, float _bottomLeftZ, float _topRightX, float _topRightY, float _topRightZ);

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
        [DllImport("RTS_DLL", EntryPoint = "GetOwnerCount")]
        static extern int GetOwnerCount(IntPtr pGoal);

        [DllImport("RTS_DLL", EntryPoint = "AddOwner")]
        static extern void AddOwner(IntPtr pGoal, int oID);
        [DllImport("RTS_DLL", EntryPoint = "RemoveOwner")]
        static extern void RemoveOwner(IntPtr pGoal, int oID);
        [DllImport("RTS_DLL", EntryPoint = "ClearOwners")]
        static extern void ClearOwners(IntPtr pGoal);
        [DllImport("RTS_DLL", EntryPoint = "TransferOwners")]
        static extern void TransferOwners(IntPtr pOrigGoal, IntPtr pNewGoal);

        IntPtr goalPtr;

        public Vector3 position
        {
            get;
            private set;
        }

        public Transform target
        {
            get;
            private set;
        }

        bool isUnit = false;
        bool isStructure = false;
        Selectable.Units.Unit unitComp = null;
        Selectable.Structures.NonTerrainObstacle structureComp = null;
        int playerNum;
        char dimension;

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
            playerNum = _playerNum;
            dimension = _dimension;

            GoalManager.Instance.AddGoal(this);
        }

        public Goal(int _playerNum, char _dimension, Transform _target) : this(_playerNum, _dimension, _target.position)
        {
            target = _target;

            structureComp = target.GetComponent<Selectable.Structures.NonTerrainObstacle>();
            unitComp = target.GetComponent<Selectable.Units.Unit>();
            isStructure = structureComp == null ? false : true;
            isUnit = unitComp == null ? false : true;

            if (isStructure)
            {
                Vector3 bottomLeft = structureComp.BottomLeftCorner;
                Vector3 topRight = structureComp.TopRightCorner;
                goalPtr = NewStructureGoal(_playerNum, _dimension, bottomLeft.x, bottomLeft.y, bottomLeft.z, topRight.x, topRight.y, topRight.z);
            }
            else if (isUnit)
            {
                goalPtr = NewGoal(_playerNum, _dimension, _target.position.x, _target.position.y, _target.position.z);
                GoalManager.Instance.StartCoroutine(UpdateTargetLocationAndRepath());
            }
        }

        public IEnumerator UpdateTargetLocationAndRepath()
        {
            float distToTravel = TerrainData.Grid.Instance.nodeRadius * 2;
            float sqrDistToTravel = distToTravel * distToTravel;

            yield return null;

            while (GetOwnerCount(goalPtr) > 0)
            {
                yield return new WaitForSeconds(distToTravel / unitComp.GetUnitStats.speed);

                if (Vector3.SqrMagnitude(target.position - position) > sqrDistToTravel)
                {
                    position = target.position;

                    IntPtr newGoalPtr = NewGoal(playerNum, dimension, position.x, position.y, position.z);

                    TransferOwners(goalPtr, newGoalPtr);

                    goalPtr = newGoalPtr;
                }
            }

            yield return null;
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

        ~Goal()
        {
            GoalManager.Instance.RemoveGoal(this);
        }
    }

}
