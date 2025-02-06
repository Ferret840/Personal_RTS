using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Runtime.Serialization;

namespace StateMachines
{
    namespace UIStateMachine
    {
        using UIStates;

        class UIStateMachine : StateMachine<UIBaseState>
        {
            static UIStateMachine m_s_Instance;
            static Dictionary<string, string> m_s_StateSceneDictionary = new Dictionary<string, string>()
            {
                { typeof(MainMenuState).ToString(), "MainMenu" },
                { typeof(LoadGameState).ToString(), "LoadGame" },
                { typeof(OptionsMenuState).ToString(), "OptionsMenu" },
                { typeof(SkirmishState).ToString(), "PathfindingTest" },
                { typeof(QuitState).ToString(), "QuitGame" },
            };


            static bool m_s_DoStart = true;

            public string GetSceneName(UIBaseState _state)
            {
                return m_s_StateSceneDictionary[_state.GetType().ToString()];
            }

            public override I_StateMachine GetInstance()
            {
                return m_s_Instance;
            }

            public override void Start()
            {
                if (m_s_DoStart)
                {
                    DontDestroyOnLoad(this);

                    m_s_DoStart = false;

                    if (m_s_Instance != null)
                        return;

                    m_s_Instance = this;
                    Init<MainMenuState>();
                }
                else
                {
                    Destroy(this);
                }
            }

            public override void Update()
            {
                base.Update();
            }
        }
    }
}


// Simple helper class that allows you to serialize System.Type objects.
// Use it however you like, but crediting or even just contacting the author would be appreciated (Always 
// nice to see people using your stuff!)
//
// Written by Bryan Keiren (http://www.bryankeiren.com)



[System.Serializable]
public class SerializableSystemType
{
    [SerializeField]
    private string m_Name;

    public string Name
    {
        get
        {
            return m_Name;
        }
    }

    [SerializeField]
    private string m_AssemblyQualifiedName;

    public string AssemblyQualifiedName
    {
        get
        {
            return m_AssemblyQualifiedName;
        }
    }

    [SerializeField]
    private string m_AssemblyName;

    public string AssemblyName
    {
        get
        {
            return m_AssemblyName;
        }
    }

    private System.Type m_SystemType;
    public System.Type SystemType
    {
        get
        {
            if (m_SystemType == null)
            {
                GetSystemType();
            }
            return m_SystemType;
        }
    }

    private void GetSystemType()
    {
        m_SystemType = System.Type.GetType(m_AssemblyQualifiedName);
    }

    public SerializableSystemType(System.Type _systemType)
    {
        m_SystemType = _systemType;
        m_Name = _systemType.Name;
        m_AssemblyQualifiedName = _systemType.AssemblyQualifiedName;
        m_AssemblyName = _systemType.Assembly.FullName;
    }

    public override bool Equals(System.Object _obj)
    {
        SerializableSystemType temp = _obj as SerializableSystemType;
        if ((object)temp == null)
        {
            return false;
        }
        return this.Equals(temp);
    }

    public bool Equals(SerializableSystemType _object)
    {
        //return m_AssemblyQualifiedName.Equals(_Object.m_AssemblyQualifiedName);
        return _object.SystemType.Equals(SystemType);
    }

    public static bool operator ==(SerializableSystemType _a, SerializableSystemType _b)
    {
        // If both are null, or both are same instance, return true.
        if (System.Object.ReferenceEquals(_a, _b))
        {
            return true;
        }

        // If one is null, but not both, return false.
        if (((object)_a == null) || ((object)_b == null))
        {
            return false;
        }

        return _a.Equals(_b);
    }

    public static bool operator !=(SerializableSystemType _a, SerializableSystemType _b)
    {
        return !(_a == _b);
    }

    public override int GetHashCode()
    {
        return SystemType != null ? SystemType.GetHashCode() : 0;
    }
}