using System;
using UnityEngine;

namespace Tasks
{
    [Serializable]
    public class EmergencyTask : TaskBase
    {
        /// <summary>
        /// Unit moves to the target position
        /// </summary>
        public class MoveToPosition : EmergencyTask
        {
            public override TaskType TaskType => TaskType.EmergencyTask_MoveToPosition;
            public Vector3 targetPosition;
        }
    }
}
