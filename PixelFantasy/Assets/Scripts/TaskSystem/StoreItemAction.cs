using Items;
using UnityEngine;
using Zones;

namespace TaskSystem
{
    public class StoreItemAction : TaskAction
    {
        private Item _item;
        private StorageSlot _storageSlot;
        private bool _isHoldingItem;
        private bool _isMoving;
        
        public float DistanceToItem => Vector2.Distance(_item.transform.position, transform.position);
        public float DistanceToSlot => Vector2.Distance(_storageSlot.transform.position, transform.position);
        
        public override void PrepareAction(Task task)
        {
            _task = task;
            _isHoldingItem = false;
            _item = _task.Requestor as Item;
            _storageSlot = _item.AssignedStorageSlot;
        }

        public override void DoAction()
        {
            // Pick Up Item
            if (DistanceToItem <= 1f && !_isHoldingItem)
            {
                _isMoving = false;
                _isHoldingItem = true;
                _ai.HoldItem(_item);
                _item.SetHeld(true);
                return;
            } 
            
            // Deposit Item
            if (DistanceToSlot <= 1f && _isHoldingItem)
            {
                _isMoving = false;
                _isHoldingItem = false;
                _storageSlot.StoreItem(_item);
                _item.AddItemToSlot();
                
                ConcludeAction();
                return;
            }

            if (_isHoldingItem) // Go to the slot
            {
                if (!_isMoving)
                {
                    _ai.Unit.UnitAgent.SetMovePosition(_storageSlot.transform.position);
                    _isMoving = true;
                    return;
                }
            }
            else // Go to the item
            {
                if (!_isMoving && _item != null)
                {
                    _ai.Unit.UnitAgent.SetMovePosition(_item.transform.position);
                    _isMoving = true;
                    return;
                }
            }
        }

        public override void ConcludeAction()
        {
            UnitAnimController.SetUnitAction(UnitAction.Nothing);
            _task = null;
            _isHoldingItem = false;
            _item = null;
            _storageSlot = null;
            _isMoving = false;
            
            base.ConcludeAction();
        }

        public override void OnTaskCancel()
        {
            _ai.Unit.UnitAgent.SetMovePosition(transform.position);
            _ai.DropCarriedItem();
            ConcludeAction();
        }
    }
}
