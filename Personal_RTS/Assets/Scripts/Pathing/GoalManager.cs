using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathing
{
    public class GoalManager : MonoBehaviour
    {
        static public GoalManager GoalManager_Instance;

        List<Pathing.Goal> goals = new List<Pathing.Goal>();

        GoalManager()
        {
            if (GoalManager_Instance != null)
                Destroy(GoalManager_Instance);

            GoalManager_Instance = this;
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
