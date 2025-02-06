using UnityEngine;
using System.Collections.Generic;
using System;
using System.Reflection;

namespace StateMachines
{
    public interface I_StateMachine
    {
        void ChangeState<U>() where U : BaseState, new();

        void PushToHistory(string _name);

        //void Init<U>() where U : BaseState, new();

        I_StateMachine GetInstance();
    }

    class IncompatibleStateChangeException : Exception
    {
        public IncompatibleStateChangeException(string _message) : base(_message) { }
    }

    abstract class StateMachine<T> : MonoBehaviour, I_StateMachine where T : BaseState, new()
    {
        public T CurrentState
        {
            get; private set;
        }
        public T PreviousState
        {
            get; private set;
        }
        Dictionary<Type, BaseState> m_StateDictionary = new Dictionary<Type, BaseState>();

        public void ChangeState<U>() where U : BaseState, new()
        {
            CreateStateInternal<U>();

            CurrentState.OnExit();
            PreviousState = CurrentState;
            CurrentState = (T)m_StateDictionary[typeof(U)];

            PushToHistory(CurrentState.ToString());
            CurrentState.OnEnter();
        }

        public void ChangeState(BaseState _state)
        {
            CurrentState.OnExit();
            PreviousState = CurrentState;
            CurrentState = (T)m_StateDictionary[_state.GetType()];

            PushToHistory(CurrentState.ToString());
            CurrentState.OnEnter();
        }

        void CreateStateInternal<U>() where U : BaseState, new()
        {
            try
            {
                if (!m_StateDictionary.ContainsKey(typeof(U)))
                {
                    U newState = new U();
                    newState.Init(this);
                    m_StateDictionary.Add(typeof(U), (T)(object)newState);
                }
            }
            catch (InvalidCastException)
            {
                throw new IncompatibleStateChangeException(string.Format("{0} couldn't cast state <{1}> to type <{2}>", MethodBase.GetCurrentMethod().Name, typeof(U).ToString(), typeof(T).ToString()));
            }
        }

        public void Init<U>() where U : BaseState, new()
        {
            CreateStateInternal<U>();

            CurrentState = (T)m_StateDictionary[typeof(U)];

            PushToHistory(CurrentState.ToString());
            CurrentState.OnEnter();
        }

        public virtual void Start()
        {

        }

        public virtual void Update()
        {
            CurrentState.OnUpdate();
        }

        public abstract I_StateMachine GetInstance();
        
        public void PushToHistory(string _name)
        {
            m_HistoryStack.Push(_name);

            string historyString = "";

            foreach (var scene in m_HistoryStack)
            {
                historyString = string.Concat(scene, ", ", historyString);
            }
            Debug.Log(historyString);
        }

        // Private
        private Stack<string> m_HistoryStack = new Stack<string>();
    }

}
