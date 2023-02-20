using System;
using UnityEngine;

namespace SGoap
{
    [Serializable]
    public class Goal : State
    {
        public int Priority;
        public bool Once;
        public float TotalCost;
    }

    [Serializable]
    public class GoalRequest
    {
        public Goal Goal;
        public GameObject Requestor;

        public GoalRequest(GameObject requestor, Goal goal)
        {
            Requestor = requestor;
            Goal = goal;
        }
    }
}