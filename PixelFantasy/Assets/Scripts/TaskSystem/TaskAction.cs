using System;
using Characters;
using Data.Item;
using Items;
using Managers;
using ScriptableObjects;
using UnityEngine;

namespace TaskSystem
{
    public abstract class TaskAction : MonoBehaviour
    {
        public string TaskId;

        protected TaskAI _ai;
        protected Task _task;
        protected const float MIN_DISTANCE_FROM_REQUESTOR = 0.5f;

        public float ActionSpeed => 1f;
        public float WorkAmount => _ai.Kinling.Skills.GetWorkAmount(_task.TaskType);
        public Task Task => _task;

        protected KinlingAnimController KinlingAnimController => _ai.Kinling.kinlingAnimController;

        private void Awake()
        {
            GameEvents.OnTaskCancelled += Event_OnTaskCancelled;
        }

        private void OnDestroy()
        {
            GameEvents.OnTaskCancelled -= Event_OnTaskCancelled;
        }
        
        public void AssignOwner(TaskAI ai)
        {
            _ai = ai;
        }
        
        public virtual bool CanDoTask(Task task)
        {
            if (task.Requestor != null)
            {
                // Check if there is a possible tool to use, if needed and not held
                if (task.RequiredToolType != EToolType.None)
                {
                    if (!_ai.HasToolTypeEquipped(task.RequiredToolType))
                    {
                        bool foundTool = InventoryManager.Instance.HasToolType(task.RequiredToolType);
                        return foundTool;
                    }
                }
                
                return task.Requestor.UseagePosition(_ai.Kinling.transform.position) != null;
            }
            
            return true;
        }

        public void InitAction(Task task, Kinling kinling)
        {
            _task = task;
            _task.KinlingAssignedToTask = kinling;
        }

        /// <summary>
        /// Triggers once on Action Begin
        /// </summary>
        public abstract void PrepareAction(Task task);

        public virtual void PickupRequiredTool(Action onReadyForTask, Action onFailedToGetTool)
        {
            // Temp override
            onReadyForTask.Invoke();
            return;
            
            //
            // if (_task.RequiredToolType == EToolType.None)
            // {
            //     onReadyForTask.Invoke();
            //     return;
            // }
            //
            // if (_ai.HasToolTypeEquipped(_task.RequiredToolType))
            // {
            //     onReadyForTask.Invoke();
            //     return;
            // }
            //
            // ToolData claimedTool = InventoryManager.Instance.ClaimToolType(_task.RequiredToolType);
            //
            // if (claimedTool == null)
            // {
            //     onFailedToGetTool.Invoke();
            //     return;
            // }
            //
            // _ai.Kinling.KinlingAgent.SetMovePosition(claimedTool.AssignedStorage.LinkedFurniture.UseagePosition(_ai.Kinling.transform.position), () =>
            // {
            //     // Unequip current item if there is one
            //     Item droppedItem = null;
            //     var claimedToolsStorage = claimedTool.AssignedStorage;
            //     var curEquippedItem = _ai.Kinling.Equipment.EquipmentState.GetGearByType(claimedTool.Type);
            //     if (curEquippedItem != null)
            //     {
            //         droppedItem = _ai.Kinling.Equipment.UnequipTool(curEquippedItem);
            //         
            //         // Pick it up
            //         _ai.HoldItem(droppedItem);
            //     }
            //     
            //     // Equip item
            //     claimedToolsStorage.WithdrawItem(claimedTool);
            //     _ai.Kinling.Equipment.EquipTool(claimedTool);
            //     
            //     // put unequipped item away
            //     if (droppedItem == null)
            //     {
            //         onReadyForTask.Invoke();
            //     }
            //     else
            //     {
            //         // Try to put old tool in same storage as equipped tool
            //         if (claimedToolsStorage != null && claimedToolsStorage.StorageData.AmountCanBeDeposited(droppedItem.GetItemData()) > 0)
            //         {
            //             claimedToolsStorage.StorageData.SetIncoming(droppedItem);
            //             claimedToolsStorage.DepositItems(droppedItem);
            //             onReadyForTask.Invoke();
            //             return;
            //         }
            //         
            //         Storage storageToPlaceOldItem = InventoryManager.Instance.FindAvailableStorage(droppedItem.GetItemData());
            //         
            //         if (storageToPlaceOldItem == null)
            //         {
            //             // Put on ground
            //             _ai.DropCarriedItem();
            //             onReadyForTask.Invoke();
            //             return;
            //         }
            //         else
            //         {
            //             // Put in storage
            //             storageToPlaceOldItem.StorageData.SetIncoming(droppedItem);
            //             _ai.Kinling.KinlingAgent.SetMovePosition(storageToPlaceOldItem.UseagePosition(_ai.Kinling.transform.position), () =>
            //             {
            //                 storageToPlaceOldItem.DepositItems(droppedItem);
            //                 onReadyForTask.Invoke();
            //                 return;
            //             }, OnTaskCancel);
            //         }
            //     }
            //     
            // }, OnTaskCancel);
        }
        
        /// <summary>
        /// Runs on an update loop to do the action
        /// </summary>
        public abstract void DoAction();

        public virtual void OnTaskCancel()
        {
            if (!_task.IsKinlingSpecific)
            {
                TaskManager.Instance.AddTask(_task);
            }
            
            _ai.Kinling.KinlingAgent.SetMovePosition(transform.position);
            _task.OnTaskCancel?.Invoke();
            ConcludeAction();
        }
        
        /// <summary>
        /// Trigger on Action End
        /// </summary>
        public virtual void ConcludeAction()
        {
            if (_task.OnTaskComplete != null)
            {
                _task.OnTaskComplete.Invoke(_task);
            }
            
            _ai.CurrentTaskDone();
            _task.KinlingAssignedToTask = null;
        }
        
        private void Event_OnTaskCancelled(string taskID, PlayerInteractable requestor)
        {
            if (_task != null && _task.TaskId == taskID && _task.Requestor == requestor)
            {
                OnTaskCancel();
            }
        }
    }
}
