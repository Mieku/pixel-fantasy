using System;
using System.Collections.Generic;
using Items;
using Managers;
using ScriptableObjects;
using UnityEngine;

namespace TaskSystem
{
    public class CraftFurnitureOrderAction : TaskAction
    {
        private CraftedItemData _itemToCraft;
        private CraftingTable _craftingTable;
        private List<Item> _materials;
        private ETaskState _state;
        private Furniture _requestingFurniture;
        
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
            PlaceItem,
        }
        
        public override void PrepareAction(Task task)
        {
            _itemToCraft = Librarian.Instance.GetItemData(task.Payload) as CraftedItemData;
            _craftingTable = _ai.Unit.GetUnitState().AssignedWorkplace.GetAvailableFurniture(_itemToCraft.RequiredCraftingTable) as CraftingTable;
            _materials = task.Materials;
            _requestingFurniture = task.Requestor as Furniture;
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
                _ai.Unit.UnitAgent.SetMovePosition(_targetItem.AssignedStorage.UseagePosition().position,
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
                _ai.Unit.UnitAgent.SetMovePosition(_requestingFurniture.UseagePosition().position, OnFurnitureDelivered);
                _state = ETaskState.WaitingOnDelivery;
            }
        }

        private void OnArrivedAtStorageForPickup()
        {
            _targetItem.AssignedStorage.WithdrawItem(_targetItem);
            _ai.HoldItem(_targetItem);
            _targetItem.SetHeld(true);
            _ai.Unit.UnitAgent.SetMovePosition(_craftingTable.UseagePosition().position, OnArrivedAtCraftingTable);
        }

        private void OnArrivedAtCraftingTable()
        {
            _craftingTable.ReceiveItem(_targetItem);
            _targetItem = null;
            _materialIndex++;

            // Are there more items to gather?
            if (_materialIndex > _materials.Count - 1)
            {
                _ai.Unit.UnitAgent.SetMovePosition(_craftingTable.UseagePosition().position, () =>
                {
                    _state = ETaskState.CraftItem;
                });
            }
            else
            {
                _targetItem = _materials[_materialIndex];
                _ai.Unit.UnitAgent.SetMovePosition(_targetItem.AssignedStorage.UseagePosition().position,
                    OnArrivedAtStorageForPickup);
            }
        }

        private void OnFurnitureDelivered()
        {
            _ai.DropCarriedItem();
            _requestingFurniture.PlaceFurniture(_targetItem);
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
            _requestingFurniture = null;
            _materialIndex = 0;
        }
    }
}
