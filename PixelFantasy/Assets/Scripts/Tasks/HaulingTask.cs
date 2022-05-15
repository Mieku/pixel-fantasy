using System;
using Items;
using Unit;
using UnityEngine;
using Action = System.Action;

namespace Tasks
{
    public class HaulingTask : TaskBase
    {
        /// <summary>
        /// Unit moves to the item position, grabs the item, takes it to the item slot, drops item
        /// </summary>
        public class TakeItemToItemSlot : HaulingTask
        {
            public Action<UnitTaskAI> claimItemSlot;
            public Vector3 itemPosition;
            public Action<UnitTaskAI> grabItem;
            public Action dropItem;
            public Item item;
        }

        /// <summary>
        /// When a blueprint is placed, a task is made for each of the resources required in order to be taken to the blueprint
        /// Unit moves to the resource's position,
        /// Grabs the resource
        /// Applies it to the blueprint
        /// If the blueprint has all the needed resources it creates a ConstructStructure ConstructionTask
        /// </summary>
        public class TakeResourceToBlueprint : HaulingTask
        {
            public Vector3 resourcePosition;
            public Action<UnitTaskAI> grabResource;
            public Vector3 blueprintPosition;
            public Action useResource;
        }
    }
}