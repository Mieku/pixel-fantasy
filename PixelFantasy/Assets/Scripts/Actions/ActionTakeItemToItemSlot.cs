using Characters;
using Gods;
using Items;
using Tasks;
using UnityEngine;

namespace Actions
{
    [CreateAssetMenu(fileName = "ActionTakeItemToItemSlot", menuName ="Actions/TakeItemToItemSlot", order = 50)]
    public class ActionTakeItemToItemSlot : ActionBase
    {
        public void EnqueueTask(Item item, bool autoAssign = true)
        {
            taskMaster.HaulingTaskSystem.EnqueueTask(() =>
            {
                if (!SaveManager.Instance.IsLoading &&
                    ControllerManager.Instance.InventoryController.HasSpaceForItem(item))
                {
                    ControllerManager.Instance.InventoryController.AddItemToPending(item);
                    var slot = ControllerManager.Instance.InventoryController.GetAvailableStorageSlot(item);
                    return CreateTaskWithSlot(item, slot, false);
                }
                else
                {
                    return null;
                }
            });
        }
        
        public HaulingTask.TakeItemToItemSlot CreateTaskWithSlot(Item taskItem, StorageSlot slot, bool autoAssign = true)
        {
            taskItem.SetAssignedSlot(slot);
            var task = new HaulingTask.TakeItemToItemSlot()
            {
                item = taskItem,
                RequestorUID = taskItem.UniqueId,
                TaskAction = this,
                OnTaskAccepted = taskItem.OnTaskAccepted,
                claimItemSlot = (UnitTaskAI unitTaskAI) =>
                {
                    taskItem.AssignUnit(unitTaskAI);

                    if (slot == null && !string.IsNullOrEmpty(taskItem._assignedSlotUID))
                    {
                        slot = UIDManager.Instance.GetGameObject(taskItem._assignedSlotUID).GetComponent<StorageSlot>();
                    }
                    
                    unitTaskAI.claimedSlot = slot;
                },
                itemPosition = taskItem.transform.position,
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
