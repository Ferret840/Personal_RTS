using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StateMachines
{
    namespace UIStates
    {
        using UIStateMachine;

        class UIBaseState : BaseState
        {
            virtual protected bool LoadScene()
            {
                string sceneName = GetMachine().GetSceneName(this);
                if (SceneManager.GetActiveScene().name != sceneName)
                {
                    //GetMachine().AddSceneToHistoryStack(sceneName);
                    Scene scene = SceneManager.GetSceneByName(sceneName);
                    if (scene.isLoaded)
                    {
                        SceneManager.SetActiveScene(scene);
                        return false;
                    }
                    else
                    {
                        SceneManager.LoadScene(sceneName);
                        SceneManager.sceneLoaded += OnSceneLoaded;
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }

            virtual protected bool LoadSceneAdditive()
            {
                string sceneName = GetMachine().GetSceneName(this);
                if (SceneManager.GetActiveScene().name != sceneName)
                {
                    //GetMachine().AddSceneToHistoryStack(sceneName);
                    SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
                    SceneManager.sceneLoaded += OnSceneLoadedAdditive;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            virtual protected bool UnloadSceneAdditive()
            {
                string sceneName = GetMachine().GetSceneName(this);
                if (SceneManager.GetActiveScene().name == sceneName)
                {
                    SceneManager.UnloadSceneAsync(sceneName);
                    SceneManager.sceneUnloaded += OnSceneUnloadedAdditive;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            virtual protected void OnSceneLoaded(Scene _scene, LoadSceneMode _mode)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
            virtual protected void OnSceneLoadedAdditive(Scene _scene, LoadSceneMode _mode)
            {
                SceneManager.SetActiveScene(_scene);
                SceneManager.sceneLoaded -= OnSceneLoadedAdditive;
            }
            virtual protected void OnSceneUnloadedAdditive(Scene _scene)
            {
                //GetMachine().AddSceneToHistoryStack(SceneManager.GetActiveScene().name);
                SceneManager.sceneUnloaded -= OnSceneUnloadedAdditive;
            }

            protected UIStateMachine GetMachine()
            {
                return (UIStateMachine)Machine;
            }

            public override void OnEnter()
            {
                base.OnEnter();
            }

            public override void OnUpdate()
            {
                base.OnUpdate();
            }

            public override void OnExit()
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                SceneManager.sceneLoaded -= OnSceneLoadedAdditive;
                SceneManager.sceneUnloaded -= OnSceneUnloadedAdditive;
                base.OnExit();
            }

        }
    }
}
