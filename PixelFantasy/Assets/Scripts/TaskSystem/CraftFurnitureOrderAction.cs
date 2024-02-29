using System;
using System.Collections.Generic;
using Handlers;
using Items;
using Managers;
using ScriptableObjects;
using UnityEngine;

namespace TaskSystem
{
    public class CraftFurnitureOrderAction : TaskAction // ID: Craft Furniture Order
    {
        private CraftedItemData _itemToCraft;
        private CraftingTable _craftingTable;
        private List<Item> _materials;
        private ETaskState _state;
        private Furniture _requestingFurniture;
        
        private Item _targetItem;
        private int _materialIndex;
        private float _timer;

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

        public override bool CanDoTask(Task task)
        {
            var result = base.CanDoTask(task);

            if (!result) return false;
            
            var itemToCraft = Librarian.Instance.GetItemData((string)task.Payload) as CraftedItemData;
            var table = FurnitureManager.Instance.GetCraftingTableForItem(itemToCraft);

            return table != null;
        }

        public override void PrepareAction(Task task)
        {
            _itemToCraft = Librarian.Instance.GetItemData((string)task.Payload) as CraftedItemData;
            _craftingTable = FurnitureManager.Instance.GetCraftingTableForItem(_itemToCraft);
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
                var movePos = _targetItem.AssignedStorage.UseagePosition(_ai.Kinling.transform.position);
                _ai.Kinling.KinlingAgent.SetMovePosition(movePos, OnArrivedAtStorageForPickup, OnTaskCancel);
                _state = ETaskState.WaitingOnMats;
            }

            if (_state == ETaskState.WaitingOnMats)
            {
                
            }

            if (_state == ETaskState.CraftItem)
            {
                KinlingAnimController.SetUnitAction(UnitAction.Doing, _ai.GetActionDirection(_craftingTable.transform.position));
                _timer += TimeManager.Instance.DeltaTime;
                if(_timer >= ActionSpeed) 
                {
                    _timer = 0;
                    if (_craftingTable.DoCraft(WorkAmount))
                    {
                        KinlingAnimController.SetUnitAction(UnitAction.Nothing);
                        
                        _targetItem = Spawner.Instance.SpawnItem(_itemToCraft, _craftingTable.transform.position, false);
                        _targetItem.State.CraftersUID = _ai.Kinling.UniqueId;
                        _ai.HoldItem(_targetItem);
                        
                        _state = ETaskState.DeliverItem;
                    }
                }
            }

            if (_state == ETaskState.DeliverItem)
            {
                _ai.Kinling.KinlingAgent.SetMovePosition(_requestingFurniture.UseagePosition(_ai.Kinling.transform.position), OnFurnitureDelivered, OnTaskCancel);
                _state = ETaskState.WaitingOnDelivery;
            }
        }

        private void OnArrivedAtStorageForPickup()
        {
            _targetItem.AssignedStorage.WithdrawItem(_targetItem);
            _ai.HoldItem(_targetItem);
            _ai.Kinling.KinlingAgent.SetMovePosition(_craftingTable.UseagePosition(_ai.Kinling.transform.position), OnArrivedAtCraftingTable, OnTaskCancel);
        }

        private void OnArrivedAtCraftingTable()
        {
            _craftingTable.ReceiveMaterial(_targetItem);
            _targetItem = null;
            _materialIndex++;

            // Are there more items to gather?
            if (_materialIndex > _materials.Count - 1)
            {
                _ai.Kinling.KinlingAgent.SetMovePosition(_craftingTable.UseagePosition(_ai.Kinling.transform.position), () =>
                {
                    _state = ETaskState.CraftItem;
                }, OnTaskCancel);
            }
            else
            {
                _targetItem = _materials[_materialIndex];
                _ai.Kinling.KinlingAgent.SetMovePosition(_targetItem.AssignedStorage.UseagePosition(_ai.Kinling.transform.position),
                    OnArrivedAtStorageForPickup, OnTaskCancel);
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
            
            KinlingAnimController.SetUnitAction(UnitAction.Nothing);
            _task = null;
            _itemToCraft = null;
            _requestingFurniture = null;
            _materialIndex = 0;
        }
    }
}
