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
        private ICraftingBuilding _craftBuilding;
        private CraftingTable _craftingTable;
        private List<Item> _materials;
        private ETaskState _state;
        private Storage _receivingStorage;
        
        private Item _targetItem;
        private int _materialIndex;
        private float _timer;
        
        private const float WORK_SPEED = 1f; // TODO: Get the work speed from the Kinling's stats
        private const float WORK_AMOUNT = 1f; // TODO: Get the amount of work from the Kinling's stats

        private enum ETaskState
        {
            ClaimTable,
            GatherMats,
            WaitingOnMats,
            CraftItem,
            DeliverItem,
            WaitingOnDelivery,
        }
        
        public override void PrepareAction(Task task)
        {
            _itemToCraft = Librarian.Instance.GetItemData((string)task.Payload) as CraftedItemData;
            _craftBuilding = (ICraftingBuilding)task.Requestor;
            _craftingTable = _craftBuilding.FindCraftingTable(_itemToCraft);
            _materials = task.Materials;
            _state = ETaskState.ClaimTable;
        }

        public override void DoAction()
        {
            if (_state == ETaskState.ClaimTable)
            {
                _craftingTable.AssignItemToTable(_itemToCraft);
                _state = ETaskState.GatherMats;
            }

            if (_state == ETaskState.GatherMats)
            {
                _targetItem = _materials[_materialIndex];
                _ai.Unit.UnitAgent.SetMovePosition(_targetItem.AssignedStorage.UseagePosition(_ai.Unit.transform.position).position,
                    OnArrivedAtStorageForPickup);
                _state = ETaskState.WaitingOnMats;
            }

            if (_state == ETaskState.WaitingOnMats)
            {
                
            }

            if (_state == ETaskState.CraftItem)
            {
                UnitAnimController.SetUnitAction(UnitAction.Doing, _ai.GetActionDirection(_craftingTable.transform.position));
                _timer += TimeManager.Instance.DeltaTime;
                if(_timer >= WORK_SPEED) 
                {
                    _timer = 0;
                    if (_craftingTable.DoCraft(WORK_AMOUNT))
                    {
                        UnitAnimController.SetUnitAction(UnitAction.Nothing);
                        
                        _targetItem = Spawner.Instance.SpawnItem(_itemToCraft, _craftingTable.transform.position, false);
                        _targetItem.State.CraftersUID = _ai.Unit.UniqueId;
                        _ai.HoldItem(_targetItem);
                        _targetItem.SetHeld(true);
                        
                        _state = ETaskState.DeliverItem;
                    }
                }
            }

            if (_state == ETaskState.DeliverItem)
            {
                _receivingStorage = _craftBuilding.FindBuildingStorage(_targetItem.GetItemData());
                if (_receivingStorage == null)
                {
                    _receivingStorage = InventoryManager.Instance.GetAvailableStorage(_targetItem, true);
                    if (_receivingStorage == null)
                    {
                        // THROW IT ON THE GROUND!
                        _ai.DropCarriedItem();
                        ConcludeAction();
                        return;
                    }
                }
                
                _receivingStorage.SetIncoming(_targetItem);
                
                _ai.Unit.UnitAgent.SetMovePosition(_receivingStorage.UseagePosition(_ai.Unit.transform.position).position, OnProductDelivered);
                _state = ETaskState.WaitingOnDelivery;
            }
        }

        private void OnArrivedAtStorageForPickup()
        {
            _targetItem.AssignedStorage.WithdrawItem(_targetItem);
            _ai.HoldItem(_targetItem);
            _targetItem.SetHeld(true);
            _ai.Unit.UnitAgent.SetMovePosition(_craftingTable.UseagePosition(_ai.Unit.transform.position).position, OnArrivedAtCraftingTable);
        }

        private void OnArrivedAtCraftingTable()
        {
            _craftingTable.ReceiveMaterial(_targetItem);
            _targetItem = null;
            _materialIndex++;

            // Are there more items to gather?
            if (_materialIndex > _materials.Count - 1)
            {
                _ai.Unit.UnitAgent.SetMovePosition(_craftingTable.UseagePosition(_ai.Unit.transform.position).position, () =>
                {
                    _state = ETaskState.CraftItem;
                });
            }
            else
            {
                _targetItem = _materials[_materialIndex];
                _ai.Unit.UnitAgent.SetMovePosition(_targetItem.AssignedStorage.UseagePosition(_ai.Unit.transform.position).position,
                    OnArrivedAtStorageForPickup);
            }
        }

        private void OnProductDelivered()
        {
            _receivingStorage.DepositItems(_targetItem);
            
            _ai.DropCarriedItem();
            _targetItem = null;
            ConcludeAction();
        }

        public override void OnTaskCancel()
        {
            base.OnTaskCancel();
        }

        public override void ConcludeAction()
        {
            base.ConcludeAction();
            
            UnitAnimController.SetUnitAction(UnitAction.Nothing);
            _task = null;
            _itemToCraft = null;
            _materialIndex = 0;
            _receivingStorage = null;
        }
    }
}
