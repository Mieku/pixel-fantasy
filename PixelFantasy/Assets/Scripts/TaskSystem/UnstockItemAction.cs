using System;
using Buildings;
using Items;
using Managers;
using ScriptableObjects;
using UnityEngine;

namespace TaskSystem
{
    public class UnstockItemAction : TaskAction
    {
        private Building _building;
        private ItemData _itemData;
        
        private Storage _availableItemStorage;
        private Storage _buildingStorage;
        private TaskState _state;
        
        public float DistanceToBuildingStorage => Vector2.Distance(_buildingStorage.transform.position, transform.position);
        public float DistanceToGlobalStorage => Vector2.Distance(_availableItemStorage.transform.position, transform.position);
        
        public override void PrepareAction(Task task)
        {
            _building = task.Requestor as Building;
            _itemData = Librarian.Instance.GetItemData(task.Payload);

            _buildingStorage = _building.FindBuildingStorage(_itemData);
            _buildingStorage.SetClaimed(_itemData, 1);
            _availableItemStorage.SetIncoming(_itemData, 1);

            _state = TaskState.GoingToBuildingStorage;
            _ai.Unit.UnitAgent.SetMovePosition(_buildingStorage.transform.position);
        }
        
        public override bool CanDoTask(Task task)
        {
            var itemData = Librarian.Instance.GetItemData(task.Payload);
            
            // Check if there is accepting Storage available
            _availableItemStorage = InventoryManager.Instance.FindAvailableGlobalStorage(itemData);
            return _availableItemStorage != null;
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

        private void GoToBuildingStorage()
        {
            // Pick Up Item
            if (DistanceToBuildingStorage <= 1f)
            {
                _buildingStorage.WithdrawItems(_itemData, 1);
                var item = Spawner.Instance.SpawnItem(_itemData, _buildingStorage.transform.position, false);
                _ai.HoldItem(item);
                item.SetHeld(true);
                
                _state = TaskState.GoingToGlobalStorage;
                _ai.Unit.UnitAgent.SetMovePosition(_availableItemStorage.transform.position);
            }
        }

        private void GoToGlobalStorage()
        {
            if (DistanceToGlobalStorage <= 1f)
            {
                _ai.DepositHeldItemInStorage(_availableItemStorage);
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
            _building = null;
            _itemData = null;
            
            _availableItemStorage = null;
            _buildingStorage = null;
        }
        
        public enum TaskState
        {
            GoingToGlobalStorage,
            GoingToBuildingStorage,
        }
    }
}
