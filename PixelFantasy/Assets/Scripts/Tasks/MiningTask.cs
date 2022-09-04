using System;
using Characters;
using UnityEngine;

namespace Tasks
{
    [Serializable]
    public class MiningTask : TaskBase
    {
        public class Mine : MiningTask
        {
            public override TaskType TaskType => TaskType.Mine;
            public Action<UnitTaskAI> claimMountain;
            public Vector3 mountainPosition;
            public Action<float, Action> OnWork;
        }
    }
}
