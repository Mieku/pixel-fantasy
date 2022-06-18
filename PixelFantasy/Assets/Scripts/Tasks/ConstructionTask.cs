using System;
using Characters;
using UnityEngine;

namespace Tasks
{
    [Serializable]
    public class ConstructionTask : TaskBase
    {
        /// <summary>
        /// When a building has all of the resources loaded into it, it creates a ConstructStructure task.
        /// Unit moves to the structure
        /// Unit begin works on structure filling its progress
        /// </summary>
        public class ConstructStructure : ConstructionTask
        {
            public override TaskType TaskType => TaskType.ConstructStructure;
            public Vector3 structurePosition;
            public Action completeWork;
            public float workAmount;
        }

        /// <summary>
        /// Unit moves to the structure,
        /// Works on the structure for its work amount
        /// When done the structure is deleted
        /// Spawns some of the resources used in its construction
        /// </summary>
        public class DeconstructStructure : ConstructionTask
        {
            public override TaskType TaskType => TaskType.DeconstructStructure;
            public Action<UnitTaskAI> claimStructure;
            public Vector3 structurePosition;
            public Action completeWork;
            public float workAmount;
        }
    }
}
