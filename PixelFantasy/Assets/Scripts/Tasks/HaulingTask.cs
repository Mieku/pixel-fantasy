using System;
using Unit;
using UnityEngine;

namespace Tasks
{
    public class HaulingTask : TaskBase
    {
        /// <summary>
        /// Unit moves to the item position, grabs the item, takes it to the item slot, drops item
        /// </summary>
        public class TakeItemToItemSlot : HaulingTask
        {
            public Vector3 itemPosition;
            public Action<UnitTaskAI> grabItem;
            public Vector3 itemSlotPosition;
            public Action dropItem;
        }
    }
}