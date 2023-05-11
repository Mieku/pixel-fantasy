using System.Collections.Generic;
using Gods;
using Items;
using ScriptableObjects;
using UnityEngine;

namespace TaskSystem
{
    public class CraftPlacedItemAction : TaskAction
    {
        private CraftedItemData _itemToCraft;
        private CraftingTable _craftingTable;
        private List<CraftingBill.RequestedItemInfo> _materials;
        private Interactable _requestor;
        private Furniture _furniture;
        private TaskState _state;
        private int _materialIndex;
        private int _quantityHauled;

        private Storage _targetStorage;
        private ItemData _targetItemData;
        private bool _isMoving;
        private bool _isHoldingItem;
        private Item _item;
        private float _timer;
        
        private const float WORK_SPEED = 1f; // TODO: Get the work speed from the Kinling's stats
        private const float WORK_AMOUNT = 1f; // TODO: Get the amount of work from the Kinling's stats
        
        public float DistanceToRequestor => Vector2.Distance(_requestor.transform.position, transform.position);
        public float DistanceToStorage => Vector2.Distance(_targetStorage.transform.position, transform.position);
        public float DistanceToCraftingTable => Vector2.Distance(_craftingTable.transform.position, transform.position);
        
        public override void PrepareAction(Task task)
        {
            _requestor = task.Requestor;
            _materials = task.Materials;
            _craftingTable = UIDManager.Instance.GetGameObject(task.Payload).GetComponent<CraftingTable>();
            _furniture = _requestor as Furniture;
            _itemToCraft = _furniture.FurnitureItemData;
            _state = TaskState.AssignTable;
            _materialIndex = 0;
            _quantityHauled = 0;
        }

        public override void DoAction()
        {
            if (_state == TaskState.AssignTable)
            {
                // Trigger the Crafting Table to be in use, and show a preview of item
                _craftingTable.AssignItemToTable(_itemToCraft);
                _state = TaskState.HaulMaterials;
            }

            if (_state == TaskState.HaulMaterials)
            {
                // for each of the materials, haul the material to the crafting table
                if (_targetStorage == null || _targetItemData == null)
                {
                    _targetStorage = _materials[_materialIndex].Storage;
                    _targetItemData = _materials[_materialIndex].ItemData;
                }
                
                // Pick Up Item
                if (!_isHoldingItem && _targetStorage != null && DistanceToStorage <= 1f)
                {
                    _isMoving = false;
                    _isHoldingItem = true;
                    _targetStorage.WithdrawItems(_targetItemData, 1);
                    _item = Spawner.Instance.SpawnItem(_targetItemData, _targetStorage.transform.position, false);
                    _ai.HoldItem(_item);
                    _item.SetHeld(true);
                    return;
                }
                
                // Drop Item Off
                if (_isHoldingItem && DistanceToCraftingTable <= 1f)
                {
                    _isHoldingItem = false;
                    _isMoving = false;
                    _requestor.ReceiveItem(_item);
                    _quantityHauled++;
                    _targetStorage = null;
                    _targetItemData = null;

                    if (_quantityHauled >= _materials[_materialIndex].Quantity)
                    {
                        _materialIndex++;
                        if (_materialIndex >= _materials.Count)
                        {
                            _state = TaskState.WorkAtTable;
                        }
                        else
                        {
                            _quantityHauled = 0;
                        }
                    }
                    
                    return;
                }
                
                // Move to Item
                if (!_isHoldingItem && _targetStorage != null)
                {
                    if (!_isMoving)
                    {
                        _ai.Unit.UnitAgent.SetMovePosition(_targetStorage.transform.position);
                        _isMoving = true;
                        return;
                    }
                }
                
                // Move to table
                if (_isHoldingItem)
                {
                    if (!_isMoving)
                    {
                        _ai.Unit.UnitAgent.SetMovePosition(_craftingTable.UseagePositions()[0].transform.position);
                        _isMoving = true;
                        return;
                    }
                }
            }

            if (_state == TaskState.WorkAtTable)
            {
                // Do work at the crafting table with doing animation
                // When done work, free up the crafting table
                UnitAnimController.SetUnitAction(UnitAction.Doing, _ai.GetActionDirection(_craftingTable.transform.position));
                _timer += TimeManager.Instance.DeltaTime;
                if(_timer >= WORK_SPEED) 
                {
                    _timer = 0;
                    if (_craftingTable.DoCraft(WORK_AMOUNT))
                    {
                        UnitAnimController.SetUnitAction(UnitAction.Nothing);
                        _state = TaskState.HaulCraftedItem;
                        return;
                    }
                }
            }

            if (_state == TaskState.HaulCraftedItem)
            {
                // Place the furniture
                if ((_isHoldingItem) && DistanceToRequestor <= 1f)
                {
                    if (_isHoldingItem)
                    {
                        _furniture.ReceiveItem(_item);
                        _item = null;
                        _isHoldingItem = false;
                    }
                
                    _isMoving = false;

                    _state = TaskState.PlaceItem;
                    return;
                }
                
                // haul the item to the requestor
                if (!_isHoldingItem)
                {
                    _isHoldingItem = true;
                    _item = Spawner.Instance.SpawnItem(_itemToCraft, _craftingTable.transform.position, false);
                    _ai.HoldItem(_item);
                    _item.SetHeld(true);
                    return;
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
            }

            if (_state == TaskState.PlaceItem)
            {
                DoPlacement();
            }
        }
        
        private void DoPlacement()
        {
            UnitAnimController.SetUnitAction(UnitAction.Doing, _ai.GetActionDirection(_furniture.transform.position));
            
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
            UnitAnimController.SetUnitAction(UnitAction.Nothing);
            _task = null;
            _targetStorage = null;
            _isHoldingItem = false;
            _isMoving = false;
            _targetItemData = null;
            _itemToCraft = null;
            _craftingTable = null;
            _materials.Clear();
            _furniture = null;
            _state = TaskState.AssignTable;
            _materialIndex = 0;
            _quantityHauled = 0;
            
            base.ConcludeAction();
        }

        public override void OnTaskCancel()
        {
            throw new System.NotImplementedException();
        }

        public enum TaskState
        {
            AssignTable,
            HaulMaterials,
            WorkAtTable,
            HaulCraftedItem,
            PlaceItem,
        }
    }
}
