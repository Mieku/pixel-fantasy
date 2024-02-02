using System;
using Buildings;
using Items;
using Managers;
using UnityEngine;

namespace TaskSystem
{
    public class StoreFoodAction : TaskAction
    {
        private Item _claimedFood;
        
        private Storage _buildingStorage;
        private TaskState _state;
        
        public float DistanceToBuildingStorage => Vector2.Distance(_buildingStorage.transform.position, transform.position);
        public float DistanceToGlobalStorage => Vector2.Distance(_claimedFood.AssignedStorage.transform.position, transform.position);
        
        public override void PrepareAction(Task task)
        {
            
            _state = TaskState.GoingToGlobalStorage;
            
            if (_claimedFood == null)
            {
                Debug.LogError($"Task: {task.TaskId} attempted to claim an item but failed");
                OnTaskCancel();
                return;
            }
            _ai.Kinling.KinlingAgent.SetMovePosition(_claimedFood.AssignedStorage.transform.position);
            _buildingStorage.SetIncoming(_claimedFood);
        }

        public override bool CanDoTask(Task task)
        {
            var building = (Building)task.Requestor;
            _claimedFood = InventoryManager.Instance.FindAndClaimBestAvailableFoodGlobal();
            if (_claimedFood == null)
            {
                return false;
            }
            
            _buildingStorage = building.FindBuildingStorage(_claimedFood.GetItemData());
            if (_buildingStorage == null)
            {
                _claimedFood.UnclaimItem();
                return false;
            }

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
                _claimedFood.AssignedStorage.WithdrawItem(_claimedFood);
                _ai.HoldItem(_claimedFood);
                _claimedFood.SetHeld(true);
                
                _state = TaskState.GoingToBuildingStorage;
                _ai.Kinling.KinlingAgent.SetMovePosition(_buildingStorage.transform.position);
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
            _ai.DropCarriedItem();
            base.OnTaskCancel();
        }
        
        public override void ConcludeAction()
        {
            base.ConcludeAction();
            
            _task = null;
            _claimedFood = null;
            _buildingStorage = null;
        }

        public enum TaskState
        {
            GoingToGlobalStorage,
            GoingToBuildingStorage,
        }
    }
}
