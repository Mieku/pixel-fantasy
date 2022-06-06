using System;
using Unit;
using UnityEngine;

namespace Tasks
{
    [Serializable]
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

        public class DigHole : FarmingTask
        {
            public Action<UnitTaskAI> claimHole;
            public Vector3 holePosition;
            public Action completeWork;
            public float workAmount;
        }

        public class PlantCrop : FarmingTask
        {
            public Action<UnitTaskAI> claimHole;
            public Vector3 holePosition;
            public Action completeWork;
            public float workAmount;
        }

        public class WaterCrop : FarmingTask
        {
            public Action<UnitTaskAI> claimCrop;
            public Vector3 cropPosition;
            public Action completeWork;
            public float workAmount;
        }

        public class HarvestCrop : FarmingTask
        {
            public Action<UnitTaskAI> claimCrop;
            public Vector3 cropPosition;
            public Action completeWork;
            public float workAmount;
        }
    }
}
