using System;
using Characters;
using UnityEngine;

namespace Tasks
{
    [Serializable]
    public class FarmingTask : TaskBase
    {
        public class CutPlant : FarmingTask
        {
            public override TaskType TaskType => TaskType.CutPlant;
            public Action<UnitTaskAI> claimPlant;
            public Vector3 plantPosition;
            public Action completeWork;
            public float workAmount;
        }

        public class HarvestFruit : FarmingTask
        {
            public override TaskType TaskType => TaskType.HarvestFruit;
            public Action<UnitTaskAI> claimPlant;
            public Vector3 plantPosition;
            public Action completeWork;
            public float workAmount;
        }

        public class ClearGrass : FarmingTask
        {
            public override TaskType TaskType => TaskType.ClearGrass;
            public Action<UnitTaskAI> claimDirt;
            public Vector3 grassPosition;
            public Action completeWork;
            public float workAmount;
        }

        public class DigHole : FarmingTask
        {
            public override TaskType TaskType => TaskType.DigHole;
            public Action<UnitTaskAI> claimHole;
            public Vector3 holePosition;
            public Action completeWork;
            public float workAmount;
        }

        public class PlantCrop : FarmingTask
        {
            public override TaskType TaskType => TaskType.PlantCrop;
            public Action<UnitTaskAI> claimHole;
            public Vector3 holePosition;
            public Action completeWork;
            public float workAmount;
        }

        public class WaterCrop : FarmingTask
        {
            public override TaskType TaskType => TaskType.WaterCrop;
            public Action<UnitTaskAI> claimCrop;
            public Vector3 cropPosition;
            public Action completeWork;
            public float workAmount;
        }

        public class HarvestCrop : FarmingTask
        {
            public override TaskType TaskType => TaskType.HarvestCrop;
            public Action<UnitTaskAI> claimCrop;
            public Vector3 cropPosition;
            public Action completeWork;
            public float workAmount;
        }
    }
}
