using Buildings;
using Data.Item;
using Items;
using Managers;
using ScriptableObjects;
using UnityEngine;

namespace TaskSystem
{
    public class WithdrawItemConstructionAction : TaskAction
    {
        private ItemData _targetItem;
        private PlayerInteractable _requestor;
        private bool _isHoldingItem;
        private bool _isMoving;
        private ItemSettings _itemSettings;
        private Vector2 _constructionPos;
        
        public float DistanceToRequestor => Vector2.Distance(_constructionPos, transform.position);
        public float DistanceToStorage => Vector2.Distance(_targetItem.AssignedStorage.LinkedItem.transform.position, transform.position);
        
        public override bool CanDoTask(Task task)
        {
            var item = task.Payload as ItemData;
            if ( item == null)
            {
                return false;
            }

            return InventoryManager.Instance.IsItemInStorage(item);
        }

        public override void PrepareAction(Task task)
        {
            _task = task;
            _requestor = _task.Requestor;
            _isHoldingItem = false;
            _isMoving = false;
            
            _targetItem = task.Payload as ItemData;
            ClaimItem(_targetItem);
            
            if (_targetItem == null)
            {
                OnTaskCancel();
            }

            var construction = _requestor as Construction;
            if (construction != null)
            {
                construction.AddToIncomingItems(_targetItem);
            }

            _constructionPos = _requestor.transform.position;
        }

        public override void DoAction()
        {
            // Pick Up Item
            if (!_isHoldingItem && _targetItem.AssignedStorage != null && DistanceToStorage <= 1f)
            {
                _isMoving = false;
                _isHoldingItem = true;
                
                _targetItem.AssignedStorage.WithdrawItem(_targetItem);
                _ai.HoldItem(_targetItem.LinkedItem);
                return;
            }
            
            // Drop Item Off
            if (_isHoldingItem && DistanceToRequestor <= 1f)
            {
                _isHoldingItem = false;
                _isMoving = false;
                _requestor.ReceiveItem(_targetItem);

                ConcludeAction();
                return;
            }
            
            // Move to Item
            if (!_isHoldingItem && _targetItem.AssignedStorage != null)
            {
                if (!_isMoving)
                {
                    _ai.Kinling.KinlingAgent.SetMovePosition(_targetItem.AssignedStorage.LinkedItem.transform.position);
                    _isMoving = true;
                    return;
                }
            }
            
            // Move to Requestor
            if (_isHoldingItem)
            {
                if (!_isMoving)
                {
                    _ai.Kinling.KinlingAgent.SetMovePosition(_constructionPos);
                    _isMoving = true;
                    return;
                }
            }
        }

        public override void ConcludeAction()
        {
            base.ConcludeAction();
            
            KinlingAnimController.SetUnitAction(UnitAction.Nothing);
            _task = null;
            _requestor = null;
            _isHoldingItem = false;
            _isMoving = false;
        }

        public override void OnTaskCancel()
        {
            _ai.DropCarriedItem();
            base.OnTaskCancel();
        }
        
        public void ClaimItem(ItemData item)
        {
            InventoryManager.Instance.ClaimItem(item);
        }
    }
}
