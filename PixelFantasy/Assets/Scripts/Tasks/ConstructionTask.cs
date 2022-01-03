

using System;
using Unit;
using UnityEngine;

namespace Tasks
{
    public class ConstructionTask : TaskBase
    {

        /// <summary>
        /// Each of the resources involved in the construction gets a task to construct the resource into the structure
        /// The unit fetches the resource from a storage,
        /// Brings the resource to the structure,
        /// Unit spends time at the structure based on the work cost of the resource,
        /// Once time is done, resource is destroyed and the progress of that resource is added to the structure
        /// Task is complete
        /// </summary>
        public class ConstructResourceIntoStructure : ConstructionTask
        {
            public Vector3 resourcePosition;
            public Action<UnitTaskAI> grabResource;
            public Vector3 structurePosition;
            public Action useResource;
            public Action completeWork;
            public float workAmount;
        }
    }
}
