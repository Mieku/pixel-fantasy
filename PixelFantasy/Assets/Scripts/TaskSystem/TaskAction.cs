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

        protected TaskAI _ai;
        protected Task _task;
        protected const float MIN_DISTANCE_FROM_REQUESTOR = 0.5f;

        protected UnitAnimController UnitAnimController => _ai.Unit.UnitAnimController;

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
            if (_ai.Unit.GetUnitState().AssignedWorkplace != null)
            {
                claimedTool = InventoryManager.Instance.ClaimToolTypeBuilding(_task.RequiredToolType,
                    _ai.Unit.GetUnitState().AssignedWorkplace);
            }
            
            if (claimedTool == null && _ai.Unit.GetUnitState().AssignedHome != null)
            {
                claimedTool = InventoryManager.Instance.ClaimToolTypeBuilding(_task.RequiredToolType,
                    _ai.Unit.GetUnitState().AssignedHome);
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

            _ai.Unit.UnitAgent.SetMovePosition(claimedTool.AssignedStorage.UseagePosition().position, () =>
            {
                // Unequip current item if there is one
                Item droppedItem = null;
                var claimedToolsStorage = claimedTool.AssignedStorage;
                var claimedToolData = (GearData)claimedTool.GetItemData();
                var curEquippedItem = _ai.Unit.Equipment.EquipmentState.GetGearByType(claimedToolData.Type);
                if (curEquippedItem != null)
                {
                    droppedItem = _ai.Unit.Equipment.Unequip(curEquippedItem);
                    
                    // Pick it up
                    _ai.HoldItem(droppedItem);
                    droppedItem.SetHeld(true);
                }
                
                // Equip item
                claimedToolsStorage.WithdrawItem(claimedTool);
                _ai.Unit.Equipment.Equip(claimedTool);
                
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
                    if (_ai.Unit.GetUnitState().AssignedWorkplace != null)
                    {
                        storageToPlaceOldItem = _ai.Unit.GetUnitState().AssignedWorkplace.FindBuildingStorage(droppedItem.GetItemData());
                    }
                    
                    // Try put tool in home
                    if (storageToPlaceOldItem == null && _ai.Unit.GetUnitState().AssignedHome != null)
                    {
                        storageToPlaceOldItem = _ai.Unit.GetUnitState().AssignedHome.FindBuildingStorage(droppedItem.GetItemData());
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
                        _ai.Unit.UnitAgent.SetMovePosition(storageToPlaceOldItem.UseagePosition().position, () =>
                        {
                            storageToPlaceOldItem.DepositItems(droppedItem);
                            onReadyForTask.Invoke();
                            return;
                        });
                    }
                }
                
            });
        }
        
        /// <summary>
        /// Runs on an update loop to do the action
        /// </summary>
        public abstract void DoAction();

        public virtual void OnTaskCancel()
        {
            TaskManager.Instance.AddTask(_task);
            _ai.Unit.UnitAgent.SetMovePosition(transform.position);
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
