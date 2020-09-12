using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathing
{
    public class GoalManager : MonoBehaviour
    {
        static public GoalManager Instance
        {
            get;
            private set;
        }

        List<Pathing.Goal> m_Goals = new List<Pathing.Goal>();

        GoalManager()
        {
            if (Instance != null)
                Destroy(Instance);

            Instance = this;
        }

        public void AddGoal(Pathing.Goal _goal)
        {
            m_Goals.Add(_goal);
        }

        public void RemoveGoal(Pathing.Goal _goal)
        {
            m_Goals.Remove(_goal);
        }
    }
}
