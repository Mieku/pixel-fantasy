using System;
using System.Collections.Generic;
using Buildings;
using Items;
using Managers;
using ScriptableObjects;
using UnityEngine;

namespace TaskSystem
{
    public class StockItemAction : TaskAction
    {
        private ItemData _itemData;
        private ItemState _claimedItem;

        //private Storage _availableItemStorage;
        private Storage _buildingStorage;
        private TaskState _state;
        
        public float DistanceToBuildingStorage => Vector2.Distance(_buildingStorage.transform.position, transform.position);
        public float DistanceToGlobalStorage => Vector2.Distance(_claimedItem.Storage.transform.position, transform.position);
        
        public override void PrepareAction(Task task)
        {
            _itemData = Librarian.Instance.GetItemData(task.Payload);
            _state = TaskState.GoingToGlobalStorage;
            
            _claimedItem = InventoryManager.Instance.ClaimItemGlobal(_itemData);
            if (_claimedItem == null)
            {
                Debug.LogError($"Task: {task.TaskId} attempted to claim an item but failed");
                OnTaskCancel();
                return;
            }
            
            _ai.Unit.UnitAgent.SetMovePosition(_claimedItem.Storage.transform.position);
            _buildingStorage.SetIncoming(_claimedItem);
        }

        public override bool CanDoTask(Task task)
        {
            var building = task.Requestor as Building;
            var itemData = Librarian.Instance.GetItemData(task.Payload);
            
            // Check both if the building can accept storage and if there is an item available to fill it
            if (!InventoryManager.Instance.IsItemInStorage(itemData, true))
            {
                return false;
            }
            
            _buildingStorage = building.FindBuildingStorage(itemData);
            if (_buildingStorage == null) return false;

            return true;
        }

        public override void DoAction()
        {
            switch (_state)
            {
                case TaskState.GoingToGlobalStorage:
                    GoToGlobalStorage();
                    break;
                case TaskState.GoingToBuildingStorage:
                    GoToBuildingStorage();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void GoToGlobalStorage()
        {
            // Pick Up Item
            if (DistanceToGlobalStorage <= 1f)
            {
                var item = Spawner.Instance.SpawnItem(_itemData, _claimedItem.Storage.transform.position, false, _claimedItem);
                _claimedItem.Storage.WithdrawItem(_claimedItem);
                _ai.HoldItem(item);
                item.SetHeld(true);
                
                _state = TaskState.GoingToBuildingStorage;
                _ai.Unit.UnitAgent.SetMovePosition(_buildingStorage.transform.position);
            }
        }

        private void GoToBuildingStorage()
        {
            if (DistanceToBuildingStorage <= 1f)
            {
                _ai.DepositHeldItemInStorage(_buildingStorage);
                ConcludeAction();
            }
        }

        public override void OnTaskCancel()
        {
            _ai.Unit.UnitAgent.SetMovePosition(transform.position);
            _ai.DropCarriedItem();
            ConcludeAction();
        }
        
        public override void ConcludeAction()
        {
            base.ConcludeAction();
            
            _task = null;
            _itemData = null;

            _claimedItem = null;
            _buildingStorage = null;
        }

        public enum TaskState
        {
            GoingToGlobalStorage,
            GoingToBuildingStorage,
        }
    }
}
