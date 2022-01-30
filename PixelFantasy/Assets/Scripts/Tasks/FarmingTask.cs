using System;
using Unit;
using UnityEngine;

namespace Tasks
{
    public class FarmingTask : TaskBase
    {
        public class CutPlant : FarmingTask
        {
            public Action<UnitTaskAI> claimPlant;
            public Vector3 plantPosition;
            public Action completeWork;
            public float workAmount;
        }

        public class HarvestFruit : FarmingTask
        {
            public Action<UnitTaskAI> claimPlant;
            public Vector3 plantPosition;
            public Action completeWork;
            public float workAmount;
        }

        public class ClearGrass : FarmingTask
        {
            public Action<UnitTaskAI> claimDirt;
            public Vector3 grassPosition;
            public Action completeWork;
            public float workAmount;
        }
    }
}
