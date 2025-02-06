using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

namespace StateMachines
{
    namespace UIStates
    {
        using UIScenes;
        public interface I_MainMenuController
        {
            void GoToContinueLastSave();
            //void GoToContinueLast[Gametype]();
            //void GoToNew[Gametype]();
            void GoToNewSkirmish();
            void GoToLoadGame();
            void GoToMultiplayer();
            void GoToOptions();
            void GoToQuit();
            void GoToCustomization();

            Dictionary<string, Action> GetButtonDictionary();
        }

        class MainMenuState : UIBaseState, I_MainMenuController
        {
            Dictionary<string, Action> m_ButtonDictionary;
            MainMenu m_MainMenu;

            public override void OnEnter()
            {
                base.OnEnter();

                m_ButtonDictionary = new Dictionary<string, Action>
                {
                    { "Continue Newest Save", GoToContinueLastSave },
                    //{ "Continue Last [Gametype]", GoToContinueLast[Gametype] },
                    //{ "New [Gametype]", GoToNew[Gametype] },
                    { "New Skirmish", GoToNewSkirmish },
                    { "Load Game", GoToLoadGame },
                    { "Multiplayer", GoToMultiplayer },
                    { "Options", GoToOptions },
                    { "Customization", GoToCustomization },
                    { "Quit Game", GoToQuit }
                };

                if (LoadScene())
                {

                }
                else
                {
                    m_MainMenu = new MainMenu(this);
                }
            }

            public override void OnUpdate()
            {
                base.OnUpdate();
            }

            public override void OnExit()
            {
                m_MainMenu = null;
                base.OnExit();
            }

            override protected void OnSceneLoaded(Scene _scene, LoadSceneMode _mode)
            {
                base.OnSceneLoaded(_scene, _mode);
                m_MainMenu = new MainMenu(this);
            }

            public void GoToContinueLastSave()
            {
                throw new System.NotImplementedException();
            }

            public void GoToNewSkirmish()
            {
                GetMachine().ChangeState<SkirmishState>();
                throw new System.NotImplementedException();
            }

            public void GoToLoadGame()
            {
                GetMachine().ChangeState<LoadGameState>();
            }

            public void GoToMultiplayer()
            {
                throw new System.NotImplementedException();
            }

            public void GoToOptions()
            {
                GetMachine().ChangeState<OptionsMenuState>();
                throw new System.NotImplementedException();
            }

            public void GoToQuit()
            {
                GetMachine().ChangeState<QuitState>();
            }

            public void GoToCustomization()
            {
                throw new System.NotImplementedException();
            }

            public Dictionary<string, Action> GetButtonDictionary()
            {
                return m_ButtonDictionary;
            }
        }
    }
}
