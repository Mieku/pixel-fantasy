using Gods;
using Items;
using UnityEngine;
using Zones;

namespace TaskSystem
{
    public class WithdrawItemAction : TaskAction
    {
        private StorageSlot _storageSlot;
        private Interactable _requestor;
        private bool _isHoldingItem;
        private Item _item;
        private bool _isMoving;
        
        public float DistanceToRequestor => Vector2.Distance(_requestor.transform.position, transform.position);
        public float DistanceToSlot => Vector2.Distance(_storageSlot.transform.position, transform.position);

        private void Awake()
        {
            GameEvents.OnStorageSlotDeleted += GameEvents_OnStorageSlotDeleted;
        }

        private void OnDestroy()
        {
            GameEvents.OnStorageSlotDeleted -= GameEvents_OnStorageSlotDeleted;
        }
        
        private void GameEvents_OnStorageSlotDeleted(StorageSlot storageSlot)
        {
            if (_task != null && _storageSlot != null && _storageSlot == storageSlot)
            {
                _task.Enqueue();
                OnTaskCancel();
            }
        }
        
        public override bool CanDoTask(Task task)
        {
            var payload = task.Payload;
            if ( string.IsNullOrEmpty(payload))
            {
                return false;
            }

            _storageSlot = FindItem(payload);
            return _storageSlot != null;
        }

        public override void PrepareAction(Task task)
        {
            _task = task;
            _requestor = _task.Requestor;
            _isHoldingItem = false;
            _isMoving = false;
        }

        public override void DoAction()
        {
            // Pick Up Item
            if (!_isHoldingItem && _storageSlot != null && DistanceToSlot <= 1f)
            {
                _isMoving = false;
                _isHoldingItem = true;
                _item = _storageSlot.GetItem();
                _ai.HoldItem(_item);
                _item.SetHeld(true);
                _storageSlot = null;
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
            if (!_isHoldingItem && _storageSlot != null)
            {
                if (!_isMoving)
                {
                    _ai.Unit.UnitAgent.SetMovePosition(_storageSlot.transform.position);
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
            UnitAnimController.SetUnitAction(UnitAction.Nothing);
            _task = null;
            _requestor = null;
            _isHoldingItem = false;
            _isMoving = false;
            _item = null;
            
            base.ConcludeAction();
        }

        public override void OnTaskCancel()
        {
            _ai.Unit.UnitAgent.SetMovePosition(transform.position);
            _ai.DropCarriedItem();
            ConcludeAction();
        }
        
        public StorageSlot FindItem(string itemName)
        {
            if (string.IsNullOrEmpty(itemName)) return null;
            
            var itemData = Librarian.Instance.GetItemData(itemName);
            _storageSlot = ControllerManager.Instance.InventoryController.ClaimItem(itemData);
            return _storageSlot;
        }
    }
}
