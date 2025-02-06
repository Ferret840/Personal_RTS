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

    public class LoadGame
    {
        I_LoadGameController m_Controller;
        string m_LoadGamePrefabName = "LoadGame";

        List<Transform> m_SaveHeaders =  new List<Transform>();

        public List<Transform> GetHeaders()
        {
            return m_SaveHeaders;
        }

        public LoadGame(I_LoadGameController _controller)
        {
            //Debug.Log(String.Format("{0} {1}", GetType().Name, MethodBase.GetCurrentMethod().Name));
            m_Controller = _controller;

            Init();
        }

        void Init()
        {
            if (GameObject.Find(m_LoadGamePrefabName))
            {
                return;
            }

            Transform loadGame = GameObject.Instantiate(Resources.Load<GameObject>("MenuPrefabs/Prefab_LoadGame")).transform;
            loadGame.name = m_LoadGamePrefabName;
            
            GameObject backPrefab = Resources.Load<GameObject>("MenuPrefabs/Prefab_BackButton");
            GameObject backButton = GameObject.Instantiate(backPrefab, loadGame, false);
            backButton.GetComponent<Button>().onClick.AddListener(() => m_Controller.Back());

            Transform tabPanel = GameObject.FindObjectOfType<HorizontalLayoutGroup>().transform;
            Transform savePanels = tabPanel.parent;

            GameObject buttonPrefab = Resources.Load<GameObject>("MenuPrefabs/Prefab_Button");
            GameObject savePanelPrefab = Resources.Load<GameObject>("MenuPrefabs/Prefab_SavePanel");

            foreach (var tabName in m_Controller.GetTabNames())
            {
                GameObject newButton = GameObject.Instantiate(buttonPrefab, tabPanel, false);
                GameObject newSavesPanel = GameObject.Instantiate(savePanelPrefab, savePanels, false);

                newButton.name = tabName;
                newButton.GetComponentInChildren<Text>().text = tabName;

                newSavesPanel.name = tabName + "Panel";

                newButton.GetComponent<Button>().onClick.AddListener(newSavesPanel.transform.SetAsLastSibling);

                newSavesPanel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                Transform sortPanel = newSavesPanel.GetComponentInChildren<HorizontalLayoutGroup>().transform;
                foreach (var sortDict in m_Controller.GetSortDictionary())
                {
                    GameObject newSort = GameObject.Instantiate(buttonPrefab, sortPanel, false);
                    newSort.name = sortDict.Key;
                    newSort.GetComponentInChildren<Text>().text = sortDict.Key;
                    newSort.GetComponent<Button>().onClick.AddListener(() => sortDict.Value());
                }

                DeserializeSavedGames(newSavesPanel);
            }
        }

        void DeserializeSavedGames(GameObject _savesPanel)
        {
            GameObject savedGamePrefab = Resources.Load<GameObject>("MenuPrefabs/Prefab_SavedGame");

            Transform content = _savesPanel.GetComponentInChildren<VerticalLayoutGroup>().transform;
            m_SaveHeaders.Add(content);

            System.Random rand = new System.Random();
            int randNum = UnityEngine.Random.Range(15, 100);
            for (int i = 0; i < randNum; ++i)
            {
                GameObject save = GameObject.Instantiate(savedGamePrefab, content, false);
                Text[] texts = save.GetComponentsInChildren<Text>();

                texts[0].name = "Name";
                texts[0].text = "Save " + i.ToString();
                texts[1].name = "Scenario";
                texts[1].text = "Mission " + UnityEngine.Random.Range(1, 20);
                texts[2].name = "Date";
                texts[2].text = RandomDay().ToString();
            }
        }

        private System.Random m_Gen = new System.Random();
        DateTime RandomDay()
        {
            DateTime start = new DateTime(1995, 1, 1);
            int range = (DateTime.Today - start).Days;
            return start.AddDays(m_Gen.Next(range));
        }
    }
}
