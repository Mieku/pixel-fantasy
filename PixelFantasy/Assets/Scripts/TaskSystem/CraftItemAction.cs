using System.Collections.Generic;
using Buildings;
using Items;
using Managers;
using ScriptableObjects;
using UnityEngine;

namespace TaskSystem
{
    public class CraftItemAction : TaskAction // ID: Craft Item
    {
         private CraftedItemSettings _itemToCraft;
        private CraftingTable _craftingTable;
        private List<Item> _materials;
        private ETaskState _state;
        private Storage _receivingStorage;
        
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
        }
        
        public override void PrepareAction(Task task)
        {
            _itemToCraft = Librarian.Instance.GetItemData((string)task.Payload) as CraftedItemSettings;
            _craftingTable = (CraftingTable)task.Requestor;
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
                _ai.Kinling.KinlingAgent.SetMovePosition(_targetItem.AssignedStorage.UseagePosition(_ai.Kinling.transform.position),
                    OnArrivedAtStorageForPickup, OnTaskCancel);
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
                _receivingStorage = InventoryManager.Instance.GetAvailableStorage(_targetItem, true);
                if (_receivingStorage == null)
                {
                    // THROW IT ON THE GROUND!
                    _ai.DropCarriedItem();
                    ConcludeAction();
                    return;
                }
                
                _receivingStorage.SetIncoming(_targetItem);
                
                _ai.Kinling.KinlingAgent.SetMovePosition(_receivingStorage.UseagePosition(_ai.Kinling.transform.position), OnProductDelivered, OnTaskCancel);
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
            
            KinlingAnimController.SetUnitAction(UnitAction.Nothing);
            _task = null;
            _itemToCraft = null;
            _materialIndex = 0;
            _receivingStorage = null;
        }
    }
}
