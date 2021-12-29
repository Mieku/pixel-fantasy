using UnityEngine;

namespace Tasks
{
    public class EmergencyTask : TaskBase
    {
        /// <summary>
        /// Unit moves to the target position
        /// </summary>
        public class MoveToPosition : EmergencyTask
        {
            public Vector3 targetPosition;
        }
    }
}
