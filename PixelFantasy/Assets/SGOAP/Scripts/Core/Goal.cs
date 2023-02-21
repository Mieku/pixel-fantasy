using System;
using Gods;
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
        public TaskCategory Category;

        public GoalRequest(GameObject requestor, Goal goal, TaskCategory category)
        {
            Requestor = requestor;
            Goal = goal;
            Category = category;
        }

        public bool IsEqual(GoalRequest otherRequest)
        {
            return Requestor == otherRequest.Requestor && Goal == otherRequest.Goal && Category == otherRequest.Category;
        }
    }
}