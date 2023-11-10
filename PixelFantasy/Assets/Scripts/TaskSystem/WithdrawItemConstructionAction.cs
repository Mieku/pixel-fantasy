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

            return InventoryManager.Instance.IsItemInStorage(payload);
        }

        public override void PrepareAction(Task task)
        {
            _task = task;
            _requestor = _task.Requestor;
            _isHoldingItem = false;
            _isMoving = false;
            
            var payload = task.Payload;
            _targetItem = ClaimItem(payload);

            var building = _requestor as Building;
            if (building != null)
            {
                _constructionPos = building.ConstructionStandPosition();
            }
            else
            {
                _constructionPos = _requestor.transform.position;
            }
            
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
        
        public Item ClaimItem(string itemName)
        {
            if (string.IsNullOrEmpty(itemName)) return null;
            
            _itemData = Librarian.Instance.GetItemData(itemName);
            return InventoryManager.Instance.ClaimItem(_itemData);
        }
    }
}
