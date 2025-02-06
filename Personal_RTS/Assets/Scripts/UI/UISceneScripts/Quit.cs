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
    using UnityEngine.SceneManagement;

    public class Quit
    {
        I_QuitController m_Controller;
        string m_QuitPrefabName = "Quit";

        public Quit(I_QuitController _controller)
        {
            //Debug.Log(String.Format("{0} {1}", GetType().Name, MethodBase.GetCurrentMethod().Name));
            m_Controller = _controller;

            Init();
        }

        void Init()
        {
            if (GameObject.Find(m_QuitPrefabName))
            {
                return;
            }
            
            GameObject.Instantiate(Resources.Load<GameObject>("MenuPrefabs/Prefab_Quit")).name = m_QuitPrefabName;

            Transform buttonPanel = GameObject.FindObjectOfType<HorizontalLayoutGroup>().transform;

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
