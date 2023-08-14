using Items;
using Managers;
using ScriptableObjects;
using UnityEngine;
using Zones;

namespace TaskSystem
{
    public class WithdrawItemAction : TaskAction
    {
        private Storage _storage;
        private PlayerInteractable _requestor;
        private bool _isHoldingItem;
        private Item _item;
        private bool _isMoving;
        private ItemData _itemData;
        
        public float DistanceToRequestor => Vector2.Distance(_requestor.transform.position, transform.position);
        public float DistanceToStorage => Vector2.Distance(_storage.transform.position, transform.position);

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
            if ( string.IsNullOrEmpty(payload))
            {
                return false;
            }

            _storage = FindItem(payload);
            return _storage != null;
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
            if (!_isHoldingItem && _storage != null && DistanceToStorage <= 1f)
            {
                _isMoving = false;
                _isHoldingItem = true;
                _storage.WithdrawItems(_itemData, 1);
                _item = Spawner.Instance.SpawnItem(_itemData, _storage.transform.position, false);
                _ai.HoldItem(_item);
                _item.SetHeld(true);
                _storage = null;
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
            if (!_isHoldingItem && _storage != null)
            {
                if (!_isMoving)
                {
                    _ai.Unit.UnitAgent.SetMovePosition(_storage.transform.position);
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
            _ai.Unit.UnitAgent.SetMovePosition(transform.position);
            _ai.DropCarriedItem();
            ConcludeAction();
        }
        
        public Storage FindItem(string itemName)
        {
            if (string.IsNullOrEmpty(itemName)) return null;
            
            _itemData = Librarian.Instance.GetItemData(itemName);
            _storage = InventoryManager.Instance.ClaimItem(_itemData);
            return _storage;
        }
    }
}
