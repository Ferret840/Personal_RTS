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



        IntPtr m_GoalPtr;

        public Vector3 Position
        {
            get;
            private set;
        }

        public Transform Target
        {
            get;
            private set;
        }
        public delegate void TargetDeathDelegate(Goal _goal);
        event TargetDeathDelegate OnTargetDeathEvent = delegate { };
        public void AddOnTargetDeathCall(TargetDeathDelegate _method)
        {
            OnTargetDeathEvent += _method;
        }
        public void RemoveOnTargetDeathCall(TargetDeathDelegate _method)
        {
            OnTargetDeathEvent -= _method;
        }

        bool m_IsUnit = false;
        bool m_IsStructure = false;
        Selectable.Units.Unit m_TargetUnitComp = null;
        Selectable.Structures.NonTerrainObstacle m_TargetStructureComp = null;
        Selectable.Owner m_TargetOwnerComp = null;
        int m_PlayerNum;
        char m_Dimension;

        public int XPos
        {
            get
            {
                return NativeMethods.GetXPos_s(m_GoalPtr);
            }
        }

        public int YPos
        {
            get
            {
                return NativeMethods.GetYPos_s(m_GoalPtr);
            }
        }

        public int XSector
        {
            get
            {
                return NativeMethods.GetXSector_s(m_GoalPtr);
            }
        }

        public int YSector
        {
            get
            {
                return NativeMethods.GetYSector_s(m_GoalPtr);
            }
        }

        public char Dimension
        {
            get
            {
                return NativeMethods.GetDimension_s(m_GoalPtr);
            }
        }

        public Goal(int _playerNum, char _dimension, Vector3 _position)
        {
            Init(_playerNum, _dimension, _position);
            m_GoalPtr = NativeMethods.NewGoal_s(_playerNum, _dimension, _position.x, _position.y, _position.z);
        }

        public Goal(int _playerNum, char _dimension, Transform _target)
        {
            Init(_playerNum, _dimension, _target.position);

            Target = _target;

            m_TargetStructureComp = Target.GetComponent<Selectable.Structures.NonTerrainObstacle>();
            m_TargetUnitComp = Target.GetComponent<Selectable.Units.Unit>();
            m_IsStructure = m_TargetStructureComp == null ? false : true;
            m_IsUnit = m_TargetUnitComp == null ? false : true;

            if (m_IsUnit)
            {
                m_GoalPtr = NativeMethods.NewGoal_s(_playerNum, _dimension, _target.position.x, _target.position.y, _target.position.z);
                GoalManager.Instance.StartCoroutine(this.UpdateTargetLocationAndRepath());
            }
            else if (m_IsStructure)
            {
                Vector3 bottomLeft = m_TargetStructureComp.BottomLeftCorner;
                Vector3 topRight = m_TargetStructureComp.TopRightCorner;
                m_GoalPtr = NativeMethods.NewStructureGoal_s(_playerNum, _dimension, bottomLeft.x, bottomLeft.y, bottomLeft.z, topRight.x, topRight.y, topRight.z);
            }

            m_TargetOwnerComp = Target.GetComponent<Selectable.Owner>();
            if (m_TargetOwnerComp != null)
            {
                m_TargetOwnerComp.AddOnDeathCall(TargetDied);
            }
        }

        void Init(int _playerNum, char _dimension, Vector3 _position)
        {
            Position = _position;
            m_PlayerNum = _playerNum;
            m_Dimension = _dimension;
            GoalManager.Instance.AddGoal(this);
        }

        public IEnumerator UpdateTargetLocationAndRepath()
        {
            float distToTravel = TerrainData.Grid.Instance.m_NodeRadius * 2;
            float sqrDistToTravel = distToTravel * distToTravel;

            yield return null;

            while (m_GoalPtr != (IntPtr)0 && NativeMethods.GetOwnerCount_s(m_GoalPtr) > 0)
            {
                if (Target != null && Vector3.SqrMagnitude(Target.position - Position) > sqrDistToTravel)
                {
                    Position = Target.position;

                    IntPtr newGoalPtr = NativeMethods.NewGoal_s(m_PlayerNum, m_Dimension, Position.x, Position.y, Position.z);
                    
                    NativeMethods.TransferOwners_s(m_GoalPtr, newGoalPtr);

                    m_GoalPtr = newGoalPtr;
                }

                yield return new WaitForSeconds(distToTravel / m_TargetUnitComp.GetUnitStats().m_Speed);

            }

            yield return null;
        }

        public float GetDirFromPosition(Vector3 _position)
        {
            return NativeMethods.GetDirFromPosition_s(m_GoalPtr, _position.x, _position.y, _position.z);
        }

        public void AddOwner(Selectable.Owner _own)
        {
            NativeMethods.AddOwner_s(m_GoalPtr, _own.gameObject.GetInstanceID());
            AddOnTargetDeathCall(_own.TargetDeathFunction());
        }

        public void RemoveOwner(Selectable.Owner _own)
        {
            if (NativeMethods.RemoveOwner_s(m_GoalPtr, _own.gameObject.GetInstanceID()))
            {
                GoalManager.Instance.StopCoroutine(this.UpdateTargetLocationAndRepath());
                GoalManager.Instance.RemoveGoal(this);
                if (m_TargetOwnerComp != null)
                {
                    m_TargetOwnerComp.RemoveOnDeathCall(TargetDied);
                }
                m_GoalPtr = (IntPtr)0;
            }
            RemoveOnTargetDeathCall(_own.TargetDeathFunction());
        }

        public void TargetDied(Selectable.Owner _owner)
        {
            Target = null;
            OnTargetDeathEvent(null);
        }
    }

    internal static class NativeMethods
    {
        [DllImport("RTS_DLL", EntryPoint = "NewGoal")]
        public static extern IntPtr NewGoal_s(int _playerNum, char _dimension, float _posX, float _posY, float _posZ);
        [DllImport("RTS_DLL", EntryPoint = "NewStructureGoal")]
        public static extern IntPtr NewStructureGoal_s(int _playerNum, char _dimension, float _bottomLeftX, float _bottomLeftY, float _bottomLeftZ, float _topRightX, float _topRightY, float _topRightZ);

        [DllImport("RTS_DLL", EntryPoint = "GetDirFromPosition")]
        public static extern float GetDirFromPosition_s(IntPtr _pGoal, float _posX, float _posY, float _posZ);

        [DllImport("RTS_DLL", EntryPoint = "GetXPos")]
        public static extern int GetXPos_s(IntPtr _pGoal);
        [DllImport("RTS_DLL", EntryPoint = "GetYPos")]
        public static extern int GetYPos_s(IntPtr _pGoal);
        [DllImport("RTS_DLL", EntryPoint = "GetXSector")]
        public static extern int GetXSector_s(IntPtr _pGoal);
        [DllImport("RTS_DLL", EntryPoint = "GetYSector")]
        public static extern int GetYSector_s(IntPtr _pGoal);
        [DllImport("RTS_DLL", EntryPoint = "GetDimension")]
        public static extern char GetDimension_s(IntPtr _pGoal);
        [DllImport("RTS_DLL", EntryPoint = "GetOwnerCount")]
        public static extern int GetOwnerCount_s(IntPtr _pGoal);

        [DllImport("RTS_DLL", EntryPoint = "AddOwner")]
        public static extern void AddOwner_s(IntPtr _pGoal, int _oID);
        //Returns true if the list of owners is now empty, i.e. goal was destroyed
        [DllImport("RTS_DLL", EntryPoint = "RemoveOwner")]
        public static extern bool RemoveOwner_s(IntPtr _pGoal, int _oID);
        [DllImport("RTS_DLL", EntryPoint = "ClearOwners")]
        public static extern void ClearOwners_s(IntPtr _pGoal);
        [DllImport("RTS_DLL", EntryPoint = "TransferOwners")]
        public static extern void TransferOwners_s(IntPtr _pOrigGoal, IntPtr _pNewGoal);
    }
}
