using System.Collections.Generic;
using Buildings;
using Items;
using Managers;
using ScriptableObjects;
using UnityEngine;

namespace TaskSystem
{
    public class CraftItemAction : TaskAction
    {
        private CraftedItemData _itemToCraft;
        private CraftingTable _craftingTable;
        private List<CraftingBill.RequestedItemInfo> _materials;
        private TaskState _state;
        private int _materialIndex;
        private int _quantityHauled;
        
        private Storage _targetStorage;
        private ItemState _targetItemState;
        private bool _isMoving;
        private bool _isHoldingItem;
        private Item _item;
        private float _timer;
        private Storage _receivingStorage;
        private bool _assignedIncomingStorage;
        
        private const float WORK_SPEED = 1f; // TODO: Get the work speed from the Kinling's stats
        private const float WORK_AMOUNT = 1f; // TODO: Get the amount of work from the Kinling's stats

        public float DistanceToStorage => Vector2.Distance(_targetStorage.transform.position, transform.position);
        public float DistanceToCraftingTable => Vector2.Distance(_craftingTable.transform.position, transform.position);
        public float DistanceToReceivingStorage => Vector2.Distance(_receivingStorage.transform.position, transform.position);

        public override void PrepareAction(Task task)
        {
            _itemToCraft = Librarian.Instance.GetItemData(task.Payload) as CraftedItemData;
            _craftingTable = task.Requestor as CraftingTable;
            _materials = task.Materials;
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
                if (_targetStorage == null || _targetItemState == null)
                {
                    _targetItemState = _materials[_materialIndex].ItemState;
                    _targetStorage = _targetItemState.Storage;
                }
                
                // Pick Up Item
                if (!_isHoldingItem && _targetStorage != null && DistanceToStorage <= 1f)
                {
                    _isMoving = false;
                    _isHoldingItem = true;
                    _item = Spawner.Instance.SpawnItem(_targetItemState.Data, _targetStorage.transform.position, false, _targetItemState);
                    _targetStorage.WithdrawItem(_targetItemState);
                    _ai.HoldItem(_item);
                    _item.SetHeld(true);
                    return;
                }
                
                // Drop Item Off
                if (_isHoldingItem && DistanceToCraftingTable <= 1f)
                {
                    _isHoldingItem = false;
                    _isMoving = false;
                    _craftingTable.ReceiveItem(_item);
                    _quantityHauled++;
                    _targetStorage = null;
                    _targetItemState = null;

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
                        
                        _isHoldingItem = true;
                        _item = Spawner.Instance.SpawnItem(_itemToCraft, _craftingTable.transform.position, false);
                        _ai.HoldItem(_item);
                        _item.SetHeld(true);
                        
                        return;
                    }
                }
            }
            
            if (_state == TaskState.HaulCraftedItem)
            {
                // Find storage to place the item, prefer the building's, if nothing... drop on the floor
                _receivingStorage = _craftingTable.ParentBuilding.FindBuildingStorage(_itemToCraft);
                if (_receivingStorage == null)
                {
                    _receivingStorage = InventoryManager.Instance.GetAvailableStorage(_item);
                    if (_receivingStorage == null)
                    {
                        // THROW IT ON THE GROUND!
                        _ai.DropCarriedItem();
                        ConcludeAction();
                        return;
                    }
                }

                if (!_assignedIncomingStorage)
                {
                    _receivingStorage.SetIncoming(_item.State);
                    _assignedIncomingStorage = true;
                }
                
                // Store the item
                if ((_isHoldingItem) && DistanceToReceivingStorage <= 1f)
                {
                    if (_isHoldingItem)
                    {
                        _receivingStorage.DepositItems(_item.State);
                        Destroy(_item.gameObject);
                        _item = null;
                        _isHoldingItem = false;
                    }
                
                    _isMoving = false;
                    ConcludeAction();
                    return;
                }
                
                // Move to Storage
                if (_isHoldingItem)
                {
                    if (!_isMoving)
                    {
                        _ai.Unit.UnitAgent.SetMovePosition(_receivingStorage.transform.position);
                        _isMoving = true;
                        return;
                    }
                }
            }
        }

        public override void OnTaskCancel()
        {
            
        }
        
        public override void ConcludeAction()
        {
            base.ConcludeAction();
            
            UnitAnimController.SetUnitAction(UnitAction.Nothing);
            _task = null;
            _targetStorage = null;
            _isHoldingItem = false;
            _isMoving = false;
            _targetItemState = null;
            _itemToCraft = null;
            _craftingTable = null;
            _materials.Clear();
            _state = TaskState.AssignTable;
            _materialIndex = 0;
            _quantityHauled = 0;
            _assignedIncomingStorage = false;
        }
        
        public enum TaskState
        {
            AssignTable,
            HaulMaterials,
            WorkAtTable,
            HaulCraftedItem,
        }
    }
}
