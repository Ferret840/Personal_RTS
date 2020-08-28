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

        List<Pathing.Goal> goals = new List<Pathing.Goal>();

        GoalManager()
        {
            if (Instance != null)
                Destroy(Instance);

            Instance = this;
        }

        public void AddGoal(Pathing.Goal _goal)
        {
            goals.Add(_goal);
        }

        public void RemoveGoal(Pathing.Goal _goal)
        {
            goals.Remove(_goal);
        }
    }
}
