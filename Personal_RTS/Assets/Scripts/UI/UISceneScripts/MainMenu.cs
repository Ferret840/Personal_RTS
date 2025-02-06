using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UIScenes
{
    using StateMachines.UIStates;

    public class MainMenu
    {
        I_MainMenuController m_Controller;

        public MainMenu(I_MainMenuController _controller)
        {
            //Debug.Log(String.Format("{0} {1}", GetType().Name, MethodBase.GetCurrentMethod().Name));
            m_Controller = _controller;

            Init();
        }

        void Init()
        {
            if (GameObject.Find("MainMenu"))
            {
                return;
            }

            GameObject.Instantiate(Resources.Load<GameObject>("MenuPrefabs/Prefab_MainMenu")).name = "MainMenu";
            
            Transform buttonPanel = GameObject.FindObjectOfType<VerticalLayoutGroup>().transform;
            
            GameObject buttonPrefab = Resources.Load<GameObject>("MenuPrefabs/Prefab_Button");
            foreach (var dict in m_Controller.GetButtonDictionary())
            {
                GameObject newButton = GameObject.Instantiate(buttonPrefab, buttonPanel, false);
                newButton.name = dict.Key;
                newButton.GetComponent<Button>().onClick.AddListener(() => dict.Value());
                newButton.GetComponentInChildren<Text>().text = dict.Key;
            }

            foreach (var button in buttonPanel.GetComponentsInChildren<Button>())
            {
                if (button.enabled)
                {
                    EventSystem.current.SetSelectedGameObject(button.gameObject);
                    break;
                }
            }
        }
    }
}
