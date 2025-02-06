using UnityEngine;
using System.Collections.Generic;
using System;
using System.Reflection;

namespace StateMachines
{
    public class BaseState
    {
        protected I_StateMachine Machine
        {
            get; set;
        }
        public BaseState Init<T>(T _machine) where T : I_StateMachine
        {
            Machine = _machine;
            return this;
        }

        public virtual void OnEnter()
        {
            //Debug.Log(String.Format("{0} {1}", GetType().Name, MethodBase.GetCurrentMethod().Name));
        }

        public virtual void OnUpdate()
        {
            //Debug.Log(String.Format("{0} {1}", GetType().Name, MethodBase.GetCurrentMethod().Name));
        }

        public virtual void OnExit()
        {
            //Debug.Log(String.Format("{0} {1}", GetType().Name, MethodBase.GetCurrentMethod().Name));
        }
    }
}