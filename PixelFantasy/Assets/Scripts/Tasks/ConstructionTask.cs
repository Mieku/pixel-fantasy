using System;
using Unit;
using UnityEngine;

namespace Tasks
{
    public class ConstructionTask : TaskBase
    {
        /// <summary>
        /// When a building has all of the resources loaded into it, it creates a ConstructStructure task.
        /// Unit moves to the structure
        /// Unit begin works on structure filling its progress
        /// </summary>
        public class ConstructStructure : ConstructionTask
        {
            public Vector3 structurePosition;
            public Action completeWork;
            public float workAmount;
        }
    }
}
