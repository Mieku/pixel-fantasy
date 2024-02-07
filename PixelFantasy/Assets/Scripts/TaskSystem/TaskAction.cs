using System;
using Characters;
using Items;
using Managers;
using ScriptableObjects;
using UnityEngine;

namespace TaskSystem
{
    public abstract class TaskAction : MonoBehaviour
    {
        public string TaskId;
        public StatType RelevantStatType;

        protected TaskAI _ai;
        protected Task _task;
        protected const float MIN_DISTANCE_FROM_REQUESTOR = 0.5f;

        public float ActionSpeed => _ai.Kinling.Stats.GetActionSpeed(RelevantStatType);
        public float WorkAmount => _ai.Kinling.Stats.GetWorkAmount(_task.RequiredToolType);
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
                        bool foundTool = false;
                        
                        if (_ai.Kinling.AssignedWorkplace != null)
                        {
                            foundTool = InventoryManager.Instance.HasToolTypeBuilding(task.RequiredToolType,
                                _ai.Kinling.AssignedWorkplace);
                        }
                        
                        if (!foundTool && _ai.Kinling.AssignedHome != null)
                        {
                            foundTool = InventoryManager.Instance.HasToolTypeBuilding(task.RequiredToolType,
                                _ai.Kinling.AssignedHome);
                        }

                        if (!foundTool)
                        {
                            foundTool = InventoryManager.Instance.HasToolTypeGlobal(task.RequiredToolType);
                        }

                        if (!foundTool)
                        {
                            return false;
                        }
                    }
                }
                
                return task.Requestor.UseagePosition(_ai.Kinling.transform.position) != null;
            }
            
            return true;
        }

        public void InitAction(Task task)
        {
            _task = task;
        }

        /// <summary>
        /// Triggers once on Action Begin
        /// </summary>
        public abstract void PrepareAction(Task task);

        public virtual void PickupRequiredTool(Action onReadyForTask, Action onFailedToGetTool)
        {
            if (_task.RequiredToolType == EToolType.None)
            {
                onReadyForTask.Invoke();
                return;
            }

            if (_ai.HasToolTypeEquipped(_task.RequiredToolType))
            {
                onReadyForTask.Invoke();
                return;
            }

            Item claimedTool = null;
            // Find the tool
            if (_ai.Kinling.AssignedWorkplace != null)
            {
                claimedTool = InventoryManager.Instance.ClaimToolTypeBuilding(_task.RequiredToolType,
                    _ai.Kinling.AssignedWorkplace);
            }
            
            if (claimedTool == null && _ai.Kinling.AssignedHome != null)
            {
                claimedTool = InventoryManager.Instance.ClaimToolTypeBuilding(_task.RequiredToolType,
                    _ai.Kinling.AssignedHome);
            }

            if (claimedTool == null)
            {
                claimedTool = InventoryManager.Instance.ClaimToolTypeGlobal(_task.RequiredToolType);
            }

            if (claimedTool == null)
            {
                onFailedToGetTool.Invoke();
                return;
            }

            _ai.Kinling.KinlingAgent.SetMovePosition(claimedTool.AssignedStorage.UseagePosition(_ai.Kinling.transform.position), () =>
            {
                // Unequip current item if there is one
                Item droppedItem = null;
                var claimedToolsStorage = claimedTool.AssignedStorage;
                var claimedToolData = (GearData)claimedTool.GetItemData();
                var curEquippedItem = _ai.Kinling.Equipment.EquipmentState.GetGearByType(claimedToolData.Type);
                if (curEquippedItem != null)
                {
                    droppedItem = _ai.Kinling.Equipment.UnequipTool(curEquippedItem);
                    
                    // Pick it up
                    _ai.HoldItem(droppedItem);
                    droppedItem.SetHeld(true);
                }
                
                // Equip item
                claimedToolsStorage.WithdrawItem(claimedTool);
                _ai.Kinling.Equipment.EquipTool(claimedTool);
                
                // put unequipped item away
                if (droppedItem == null)
                {
                    onReadyForTask.Invoke();
                }
                else
                {
                    // Try to put old tool in same storage as equipped tool
                    if (claimedToolsStorage != null && claimedToolsStorage.AmountCanBeDeposited(droppedItem.GetItemData()) > 0)
                    {
                        claimedToolsStorage.SetIncoming(droppedItem);
                        claimedToolsStorage.DepositItems(droppedItem);
                        onReadyForTask.Invoke();
                        return;
                    }
                    
                    Storage storageToPlaceOldItem = null;
                    // Try put tool in workplace
                    if (_ai.Kinling.AssignedWorkplace != null)
                    {
                        storageToPlaceOldItem = _ai.Kinling.AssignedWorkplace.FindBuildingStorage(droppedItem.GetItemData());
                    }
                    
                    // Try put tool in home
                    if (storageToPlaceOldItem == null && _ai.Kinling.AssignedHome != null)
                    {
                        storageToPlaceOldItem = _ai.Kinling.AssignedHome.FindBuildingStorage(droppedItem.GetItemData());
                    }
                    
                    // Try put tool in global
                    if (storageToPlaceOldItem == null)
                    {
                        storageToPlaceOldItem =
                            InventoryManager.Instance.FindAvailableGlobalStorage(droppedItem.GetItemData());
                    }
                    
                    if (storageToPlaceOldItem == null)
                    {
                        // Put on ground
                        _ai.DropCarriedItem();
                        onReadyForTask.Invoke();
                        return;
                    }
                    else
                    {
                        // Put in storage
                        storageToPlaceOldItem.SetIncoming(droppedItem);
                        _ai.Kinling.KinlingAgent.SetMovePosition(storageToPlaceOldItem.UseagePosition(_ai.Kinling.transform.position), () =>
                        {
                            storageToPlaceOldItem.DepositItems(droppedItem);
                            onReadyForTask.Invoke();
                            return;
                        }, OnTaskCancel);
                    }
                }
                
            }, OnTaskCancel);
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
        }
        
        private void Event_OnTaskCancelled(Task task)
        {
            if (_task != null && _task.IsEqual(task))
            {
                OnTaskCancel();
            }
        }
    }
}
