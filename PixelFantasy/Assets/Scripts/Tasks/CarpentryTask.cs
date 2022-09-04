

using System;
using Items;
using Characters;
using UnityEngine;
using Action = System.Action;

namespace Tasks
{
    [Serializable]
    public class CarpentryTask : TaskBase
    {
        public class CraftItem : CarpentryTask
        {
            public override TaskType TaskType => TaskType.Carpentry_CraftItem;
            public CraftingTable craftingTable;
            public Vector3 craftPosition;
            public Action<float, Action> OnWork;
        }
        
        public class GatherResourceForCrafting : CarpentryTask
        {
            public override TaskType TaskType => TaskType.Carpentry_GatherResourceForCrafting;
            public Vector3 resourcePosition;
            public CraftingTable craftingTable;
            public Action<UnitTaskAI> grabResource;
            public Action<Item> useResource;
        }
    }
}
