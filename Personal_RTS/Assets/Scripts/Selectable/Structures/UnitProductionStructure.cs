using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Selectable.Units;
using Tools;

namespace Selectable
{
    namespace Structures
    {

        public class UnitProductionStructure : Base_Structure
        {
            [SerializeField]
#pragma warning disable CS0649 // Field 'UnitProductionStructure.m_ObjectBase' is never assigned to, and will always have its default value null
            GameObject m_ObjectBase;
#pragma warning restore CS0649 // Field 'UnitProductionStructure.m_ObjectBase' is never assigned to, and will always have its default value null

            //public UnitVariables[] SpawnObject = new UnitVariables[0];
            public ProductionInfo[] m_SpawnObjects = new ProductionInfo[0];

            //Queue<UnitVariables> queuedUnits = new Queue<UnitVariables>();
            Queue<ProductionInfo> m_UnitQueue = new Queue<ProductionInfo>();

            float m_BuildPercent = 1;

            // Use this for initialization
            void Start()
            {
                Init();
            }

            // Update is called once per frame
            void Update()
            {

            }

            override public void OnRightMouse()
            {
                base.OnRightMouse();
            }

            /// <summary>
            /// Overrideable function for handling when a key is pressed
            /// </summary>
            /// <param name="_k">An int within Keycode corresponding to the key that was pressed</param>
            override public void OnKeyDown(KeyCode _k)
            {
                if (_k >= KeyCode.Keypad0 && _k <= KeyCode.Keypad9)
                {
                    //queuedUnits.Enqueue(SpawnObject[k - KeyCode.Keypad0]);
                    //if (queuedUnits.Count > 1)
                    m_UnitQueue.Enqueue(m_SpawnObjects[_k - KeyCode.Keypad0]);
                    if (m_UnitQueue.Count == 1)
                    {
                        StartCoroutine(BuildUnit());
                    }
                }
            }

            /// <summary>
            /// Overrideable function for handling when a key is released
            /// </summary>
            /// <param name="_k">An int within Keycode corresponding to the key that was released</param>
            override public void OnKeyUp(KeyCode _k)
            {

            }

            IEnumerator BuildUnit()
            {
                //while (queuedUnits.Count > 0)
                //{
                //    UnitVariables o = queuedUnits.Peek();
                //    for (float i = 0; i < o.BuildTime; i += Time.deltaTime)
                //    {
                //        BuildPercent = i / o.BuildTime;
                //
                //        yield return null;
                //    }
                //
                //    GameObject newUnit = Instantiate(objectBase);
                //}
                while (m_UnitQueue.Count > 0)
                {
                    ProductionInfo o = m_UnitQueue.Peek();
                    for (float i = 0; i < o.m_UnivStats.m_BuildTime; i += Time.deltaTime)
                    {
                        m_BuildPercent = i / o.m_UnivStats.m_BuildTime;
                        Debug.Log("Training: " + (m_BuildPercent * 100).ToString("F2") + "%");

                        yield return null;
                    }

                    GameObject newUnit = Instantiate(m_ObjectBase);
                    newUnit.layer = Utils.IntToLayer_s(o.m_UnitVar.m_Dimension);
                    //Unit unitComp = newUnit.AddComponent<Unit>();
                    Unit unitComp = newUnit.GetComponent<Unit>();
                    unitComp.SetUnitStats(o.m_UnitVar);

                    Debug.Log("Unit created");
                    yield return new WaitForSeconds(2);
                    unitComp.SetTargetGoal(TargetGoal);

                    m_UnitQueue.Dequeue();
                }
            }
        }

        [System.Serializable]
        public struct ProductionInfo
        {
            public UniversalStats m_UnivStats;
            public UnitVariables m_UnitVar;
        }

    }
}