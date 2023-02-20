using Characters;
using Gods;
using Items;
using Tasks;
using UnityEngine;
using Zones;

namespace Actions
{
    [CreateAssetMenu(fileName = "ActionTakeItemToItemSlot", menuName ="Actions/TakeItemToItemSlot", order = 50)]
    public class ActionTakeItemToItemSlot : ActionBase
    {
        public override TaskBase CreateTask(Interactable requestor, bool autoAssign = true)
        {
            var item = requestor as Item;
            var task = new HaulingTask.TakeItemToItemSlot()
            {
                item = item,
                RequestorUID = item.UniqueId,
                TaskAction = this,
                OnTaskAccepted = item.OnTaskAccepted,
                itemPosition = item.transform.position,
                grabItem = (UnitTaskAI unitTaskAI, Item itemToGrab) =>
                {
                    itemToGrab.SetHeld(true);
                    unitTaskAI.AssignHeldItem(itemToGrab);
                },
                dropItem = (Item itemToDrop) =>
                {
                    OnTaskComplete(itemToDrop);
                }
            };

            if (autoAssign)
            {
                taskMaster.HaulingTaskSystem.AddTask(task);
            }

            return task;
        }

        public override bool CanExecute()
        {
            // TODO": build to approve if there is space available in storage
            return true;
        }

        public override void CancelTask(Interactable requestor)
        {
            taskMaster.HaulingTaskSystem.CancelTask(requestor.UniqueId);
        }
        
        public void OnTaskComplete(Item item)
        {
            item.AddItemToSlot();
            item.SetAssignedSlot(null);
            item.OnTaskCompleted();
        }
    }
}
