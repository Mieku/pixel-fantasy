using Items;
using Managers;
using ScriptableObjects;
using UnityEngine;
using Zones;

namespace TaskSystem
{
    public class WithdrawItemAction : TaskAction
    {
        private Item _targetItem;
        private PlayerInteractable _requestor;
        private bool _isHoldingItem;
        private Item _item;
        private bool _isMoving;
        private ItemData _itemData;
        
        public float DistanceToRequestor => Vector2.Distance(_requestor.transform.position, transform.position);
        public float DistanceToStorage => Vector2.Distance(_targetItem.AssignedStorage.transform.position, transform.position);

        private void Awake()
        {
            //GameEvents.OnStorageSlotDeleted += GameEvents_OnStorageSlotDeleted;
        }

        private void OnDestroy()
        {
            //GameEvents.OnStorageSlotDeleted -= GameEvents_OnStorageSlotDeleted;
        }
        
        // private void GameEvents_OnStorageSlotDeleted(StorageTile storageTile)
        // {
        //     if (_task != null && _storageTile != null && _storageTile == storageTile)
        //     {
        //         _task.Enqueue();
        //         OnTaskCancel();
        //     }
        // }
        
        public override bool CanDoTask(Task task)
        {
            var payload = task.Payload;
            if ( string.IsNullOrEmpty((string)payload))
            {
                return false;
            }

            return InventoryManager.Instance.IsItemInStorage((string)payload);
        }

        public override void PrepareAction(Task task)
        {
            _task = task;
            _requestor = _task.Requestor;
            _isHoldingItem = false;
            _isMoving = false;
            
            var payload = task.Payload;
            _targetItem = ClaimItem((string)payload);
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
                _requestor.ReceiveItem(_item);

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
                    _ai.Unit.UnitAgent.SetMovePosition(_requestor.transform.position);
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
            _item = null;
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
