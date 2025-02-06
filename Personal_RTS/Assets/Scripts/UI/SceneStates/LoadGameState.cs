using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

namespace StateMachines
{
    namespace UIStates
    {
        using System.Linq;
        using System.Text.RegularExpressions;
        using UIScenes;
        using UnityEngine.UI;

        public interface I_LoadGameController
        {
            void SortByName();
            void SortByScenario();
            void SortByDate();
            void Back();

            string[] GetTabNames();
            
            Dictionary<string, Action> GetSortDictionary();
        }

        class LoadGameState : UIBaseState, I_LoadGameController
        {
            enum eSortEnum
            {
                Name = -3,
                Scenario = -2,
                Date =  -1,
                None = 0,
                NameReverse = 3,
                ScenarioReverse = 2,
                DateReverse = 1
            }
            eSortEnum m_CurrentSort =  eSortEnum.None;

            string[] m_TabDictionary = { "Dimensional Runs", "Skirmishes", "Ascendant Runs" };
            
            Dictionary<string, Action> m_SortDictionary;

            LoadGame m_LoadGame;

            public override void OnEnter()
            {
                base.OnEnter();

                m_SortDictionary = new Dictionary<string, Action>
                {
                    { "Save Name", SortByName },
                    { "Scenario", SortByScenario },
                    { "Save Date", SortByDate }
                };

                if (LoadScene())
                {

                }
                else
                {
                    m_LoadGame = new LoadGame(this);
                }
            }

            public override void OnUpdate()
            {
                //base.OnUpdate();
            }

            public override void OnExit()
            {
                m_LoadGame = null;
                base.OnExit();
            }

            override protected void OnSceneLoaded(Scene _scene, LoadSceneMode _mode)
            {
                base.OnSceneLoaded(_scene, _mode);
                m_LoadGame = new LoadGame(this);
                SortByDate();
            }

            public void Back()
            {
                GetMachine().ChangeState<MainMenuState>();
                //GetMachine().ChangeState(GetMachine().PreviousState);
            }

            public string[] GetTabNames()
            {
                return m_TabDictionary;
            }

            public Dictionary<string, Action> GetSortDictionary()
            {
                return m_SortDictionary;
            }

            public void SortByName()
            {
                SortBy("Name", eSortEnum.Name);
            }

            public void SortByScenario()
            {
                SortBy("Scenario", eSortEnum.Scenario);
            }

            public void SortByDate()
            {
                SortBy("Date", eSortEnum.Date);
            }

            void SortBy(string _name, eSortEnum _newSort)
            {
                bool sortByDate = Math.Abs((int)m_CurrentSort) == (int)eSortEnum.Date;
                if (m_CurrentSort != _newSort)
                {
                    m_CurrentSort = _newSort;
                }
                else
                {
                    m_CurrentSort = (eSortEnum)((int)_newSort * -1);
                }

                foreach (var header in m_LoadGame.GetHeaders())
                {
                    Button[] items = header.GetComponentsInChildren<Button>();
                    Button[] itemsOrdered;
                    
                    Func<string, object> convert = str =>
                    {
                        try
                        {
                            return int.Parse(str);
                        }
                        catch { return str; }
                    };

                    if (m_CurrentSort < 0)
                    {
                        if (_name == "Date")
                        {
                            itemsOrdered = items.OrderByDescending(x => System.DateTime.Parse(x.transform.Find(_name).GetComponent<Text>().text)).ToArray();
                        }
                        else
                        {

                            itemsOrdered = items.OrderByDescending(x => Regex.Split((x.transform.Find(_name).GetComponent<Text>().text).Replace(" ", ""), "([0-9]+)").Select(convert), new NaturalSort.EnumerableComparer<object>()).ToArray();
                            
                            //itemsOrdered = items.OrderByDescending(x => x.transform.Find(_name).GetComponent<Text>().text).ToArray();
                        }
                    }
                    else
                    {
                        if (_name == "Date")
                        {
                            itemsOrdered = items.OrderBy(x => System.DateTime.Parse(x.transform.Find(_name).GetComponent<Text>().text)).ToArray();
                        }
                        else
                        {
                            itemsOrdered = items.OrderBy(x => Regex.Split((x.transform.Find(_name).GetComponent<Text>().text).Replace(" ", ""), "([0-9]+)").Select(convert), new NaturalSort.EnumerableComparer<object>()).ToArray();

                            //itemsOrdered = items.OrderBy(x => x.transform.Find(_name).GetComponent<Text>().text).ToArray();
                        }
                    }
                    
                    for (int i = 0; i < header.childCount; ++i)
                    {
                        itemsOrdered[i].transform.SetSiblingIndex(i);

                        //Sort broken
                        //throw new System.NotImplementedException();
                    }
                }
            }
        }
    }
}
