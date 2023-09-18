using Items;
using Managers;
using ScriptableObjects;
using UnityEngine;

namespace TaskSystem
{
    public class PlaceFurnitureAction : TaskAction
    {
        private ItemState _itemToPlace;
        private Furniture _furniture => _task.Requestor as Furniture;
        private bool _isHoldingItem;
        private Item _item;
        private bool _isMoving;
        private ItemData _itemData;
        private float _timer;
        private bool _isPlacingItem;
        
        private const float WORK_SPEED = 1f; // TODO: Get the work speed from the Kinling's stats
        private const float WORK_AMOUNT = 1f; // TODO: Get the amount of work from the Kinling's stats
        public float DistanceToRequestor => Vector2.Distance(_furniture.transform.position, transform.position);
        public float DistanceToStorage => Vector2.Distance(_itemToPlace.Storage.transform.position, transform.position);
        
        public override void PrepareAction(Task task)
        {
            _task = task;
            _itemToPlace = task.Materials[0].ItemState;
            _isHoldingItem = false;
            _isMoving = false;
            _itemData = _furniture.FurnitureItemData;
            _isPlacingItem = false;
        }

        public override void DoAction()
        {
            // Pick Up Item
            if (!_isHoldingItem && _itemToPlace != null && DistanceToStorage <= 1f)
            {
                _isMoving = false;
                _isHoldingItem = true;
                _item = Spawner.Instance.SpawnItem(_itemData, _itemToPlace.Storage.transform.position, false, _itemToPlace);
                _itemToPlace.Storage.WithdrawItem(_itemToPlace);
                _ai.HoldItem(_item);
                _item.SetHeld(true);
                _itemToPlace = null;
                return;
            }
            
            // Move to Item
            if (!_isHoldingItem && _itemToPlace != null)
            {
                if (!_isMoving)
                {
                    _ai.Unit.UnitAgent.SetMovePosition(_itemToPlace.Storage.transform.position);
                    _isMoving = true;
                    return;
                }
            }
            
            // Move to Requestor
            if (_isHoldingItem)
            {
                if (!_isMoving)
                {
                    _ai.Unit.UnitAgent.SetMovePosition(_furniture.transform.position);
                    _isMoving = true;
                    return;
                }
            }
            
            // Place the furniture
            if ((_isHoldingItem || _isPlacingItem) && DistanceToRequestor <= 1f)
            {
                if (_isHoldingItem)
                {
                    _furniture.ReceiveItem(_item);
                    _item = null;
                    _isHoldingItem = false;
                }
                
                _isMoving = false;

                DoPlacement();
                return;
            }
        }

        private void DoPlacement()
        {
            UnitAnimController.SetUnitAction(UnitAction.Doing, _ai.GetActionDirection(_furniture.transform.position));
            _isPlacingItem = true;
            
            _timer += TimeManager.Instance.DeltaTime;
            if(_timer >= WORK_SPEED) 
            {
                _timer = 0;
                if (_furniture.DoPlacement(WORK_AMOUNT)) 
                {
                    // When work is complete
                    ConcludeAction();
                } 
            }
        }
        
        public override void ConcludeAction()
        {
            base.ConcludeAction();
            
            UnitAnimController.SetUnitAction(UnitAction.Nothing);
            _task = null;
            _itemToPlace = null;
            _isHoldingItem = false;
            _isMoving = false;
            _itemData = null;
            _isPlacingItem = false;
        }

        public override void OnTaskCancel()
        {
            throw new System.NotImplementedException();
        }
    }
}
