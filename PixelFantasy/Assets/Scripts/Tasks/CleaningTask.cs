

using System;
using UnityEngine;

namespace Tasks
{
    [Serializable]
    public class CleaningTask : TaskBase
    {
        /// <summary>
        /// Unit moves to the target position, and executes the cleanup action
        /// </summary>
        public class GarbageCleanup : CleaningTask
        {
            public override TaskType TaskType => TaskType.GarbageCleanup;
            public Vector3 targetPosition;
            public Action cleanUpAction;
        }
    }
}
