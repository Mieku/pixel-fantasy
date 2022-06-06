

using System;
using Items;
using Unit;
using UnityEngine;
using Action = System.Action;

namespace Tasks
{
    [Serializable]
    public class CarpentryTask : TaskBase
    {
        public class CraftItem : CarpentryTask
        {
            public CraftingTable craftingTable;
            public Vector3 craftPosition;
            public Action completeWork;
            public float workAmount;
        }
        
        public class GatherResourceForCrafting : CarpentryTask
        {
            public Vector3 resourcePosition;
            public CraftingTable craftingTable;
            public Action<UnitTaskAI> grabResource;
            public Action useResource;
        }
    }
}
