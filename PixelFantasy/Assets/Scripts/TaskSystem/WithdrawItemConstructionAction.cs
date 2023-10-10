using Buildings;
using Items;
using Managers;
using ScriptableObjects;
using UnityEngine;

namespace TaskSystem
{
    public class WithdrawItemConstructionAction : TaskAction
    {
        private Item _targetItem;
        private PlayerInteractable _requestor;
        private bool _isHoldingItem;
        private bool _isMoving;
        private ItemData _itemData;
        private Vector2 _constructionPos;
        
        public float DistanceToRequestor => Vector2.Distance(_constructionPos, transform.position);
        public float DistanceToStorage => Vector2.Distance(_targetItem.AssignedStorage.transform.position, transform.position);
        
        public override bool CanDoTask(Task task)
        {
            var payload = task.Payload;
            if ( string.IsNullOrEmpty(payload))
            {
                return false;
            }

            _targetItem = FindItem(payload);
            return _targetItem != null;
        }

        public override void PrepareAction(Task task)
        {
            _task = task;
            _requestor = _task.Requestor;
            _isHoldingItem = false;
            _isMoving = false;

            var building = _requestor as Building;
            _constructionPos = building.ConstructionStandPosition();
        }

        public override void DoAction()
        {
            // Pick Up Item
            if (!_isHoldingItem && _targetItem.AssignedStorage != null && DistanceToStorage <= 1f)
            {
                _isMoving = false;
                _isHoldingItem = true;
                
                _targetItem.AssignedStorage.WithdrawItem(_targetItem);
                _ai.HoldItem(_targetItem);
                _targetItem.SetHeld(true);
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
                    _ai.Unit.UnitAgent.SetMovePosition(_targetItem.AssignedStorage.transform.position);
                    _isMoving = true;
                    return;
                }
            }
            
            // Move to Requestor
            if (_isHoldingItem)
            {
                if (!_isMoving)
                {
                    _ai.Unit.UnitAgent.SetMovePosition(_constructionPos);
                    _isMoving = true;
                    return;
                }
            }
        }

        public override void ConcludeAction()
        {
            base.ConcludeAction();
            
            UnitAnimController.SetUnitAction(UnitAction.Nothing);
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
        
        public Item FindItem(string itemName)
        {
            if (string.IsNullOrEmpty(itemName)) return null;
            
            _itemData = Librarian.Instance.GetItemData(itemName);
            return InventoryManager.Instance.ClaimItem(_itemData);
        }
    }
}
