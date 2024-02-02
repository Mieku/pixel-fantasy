using Items;
using UnityEngine;
using Zones;

namespace TaskSystem
{
    public class StoreItemAction : TaskAction
    {
        private Item _item;
        private Storage _storage;
        private bool _isHoldingItem;
        private bool _isMoving;
        
        public float DistanceToItem => Vector2.Distance(_item.transform.position, transform.position);
        public float DistanceToSlot => Vector2.Distance(_storage.transform.position, transform.position);
        
        public override void PrepareAction(Task task)
        {
            _task = task;
            _isHoldingItem = false;
            _item = _task.Requestor as Item;
            _storage = _item.AssignedStorage;
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
                _item.AddItemToSlot();
                
                ConcludeAction();
                return;
            }

            if (_isHoldingItem) // Go to the slot
            {
                if (!_isMoving)
                {
                    _ai.Kinling.KinlingAgent.SetMovePosition(_storage.transform.position);
                    _isMoving = true;
                    return;
                }
            }
            else // Go to the item
            {
                if (!_isMoving && _item != null)
                {
                    _ai.Kinling.KinlingAgent.SetMovePosition(_item.transform.position);
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
            _isHoldingItem = false;
            _item = null;
            _storage = null;
            _isMoving = false;
        }

        public override void OnTaskCancel()
        {
            _ai.DropCarriedItem();
            base.OnTaskCancel();
        }
    }
}
