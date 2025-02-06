using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace StateMachines
{
    namespace UIStates
    {
        using UIScenes;
        using UnityEngine.SceneManagement;

        public interface I_QuitController
        {
            void ConfirmQuit();
            void AbortQuit();

            Dictionary<string, Action> GetButtonDictionary();
        }

        class QuitState : UIBaseState, I_QuitController
        {
            Dictionary<string, Action> m_ButtonDictionary;
            Quit m_Quit;

            public override void OnEnter()
            {
                base.OnEnter();

                m_ButtonDictionary = new Dictionary<string, Action>
                {
                    { "No", AbortQuit },
                    { "Yes", ConfirmQuit }
                };

                LoadSceneAdditive();
            }

            public override void OnUpdate()
            {
                //base.OnUpdate();
            }

            public override void OnExit()
            {
                //m_MainMenu = null;
                base.OnExit();
            }

            protected override void OnSceneLoadedAdditive(Scene _scene, LoadSceneMode _mode)
            {
                base.OnSceneLoadedAdditive(_scene, _mode);
                m_Quit = new Quit(this);
            }

            protected override void OnSceneUnloadedAdditive(Scene _scene)
            {
                base.OnSceneUnloadedAdditive(_scene);
                GetMachine().ChangeState(GetMachine().PreviousState);
            }

            public void ConfirmQuit()
            {
                // save any game data here
#if UNITY_EDITOR
         // Application.Quit() does not work in the editor so
         // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
         UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }

            public void AbortQuit()
            {
                UnloadSceneAdditive();
            }

            public Dictionary<string, Action> GetButtonDictionary()
            {
                return m_ButtonDictionary;
            }
        }
    }
}
