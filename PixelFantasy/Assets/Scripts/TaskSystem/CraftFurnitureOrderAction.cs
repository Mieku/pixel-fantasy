using System;
using System.Collections.Generic;
using System.Linq;
using Data.Item;
using Handlers;
using Items;
using Managers;
using Systems.Appearance.Scripts;

namespace TaskSystem
{
    public class CraftFurnitureOrderAction : TaskAction // ID: Craft Furniture Order
    {
        private FurnitureSettings _furnitureToCraft;
        private CraftingTable _craftingTable;
        private List<ItemData> _materials;
        private ETaskState _state;
        private Furniture _requestingFurniture;
        
        private ItemData _targetItem;
        //private int _materialIndex;
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
            if (!base.CanDoTask(task)) return false;
            
            // Check if a table can currently do the task
            var table = FurnitureManager.Instance.GetCraftingTableForItem((FurnitureSettings)task.Payload);
            return table != null;
        }

        public override void PrepareAction(Task task)
        {
            _furnitureToCraft = (FurnitureSettings)task.Payload;
            _craftingTable = FurnitureManager.Instance.GetCraftingTableForItem(_furnitureToCraft);
            _materials = new List<ItemData>(task.Materials); //task.Materials;
            _requestingFurniture = task.Requestor as Furniture;
            _state = ETaskState.ClaimTable;
        }

        public override void DoAction()
        {
            if (_state == ETaskState.ClaimTable)
            {
                _craftingTable.AssignItemToTable(_furnitureToCraft, _materials);
                _state = ETaskState.GatherMats;
            }

            if (_state == ETaskState.GatherMats)
            {
                _targetItem = _materials.First();
                var movePos = _targetItem.AssignedStorage.AccessPosition(_ai.transform.position, _targetItem);
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
                    if (_craftingTable.DoCraft(_ai.Kinling.Stats))
                    {
                        KinlingAnimController.SetUnitAction(UnitAction.Nothing);
                        
                        var targetItemObj = Spawner.Instance.SpawnItemWithRuntimeData(_requestingFurniture.RuntimeData, _craftingTable.transform.position, false);
                        _requestingFurniture.RuntimeData.CraftersUID = _ai.Kinling.UniqueId;
                        
                        _ai.HoldItem(targetItemObj);
                        
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
            var withdrawnItem = _targetItem.AssignedStorage.WithdrawItem(_targetItem);
            _ai.HoldItem(withdrawnItem);
            _ai.Kinling.KinlingAgent.SetMovePosition(_craftingTable.UseagePosition(_ai.Kinling.transform.position), OnArrivedAtCraftingTable, OnTaskCancel);
        }

        private void OnArrivedAtCraftingTable()
        {
            _craftingTable.ReceiveMaterial(_targetItem);
            _targetItem = null;

            // Are there more items to gather?
            if (_materials.Count == 0)
            {
                _ai.Kinling.KinlingAgent.SetMovePosition(_craftingTable.UseagePosition(_ai.Kinling.transform.position), () =>
                {
                    _state = ETaskState.CraftItem;
                }, OnTaskCancel);
            }
            else
            {
                _targetItem = _materials.First();
                _ai.Kinling.KinlingAgent.SetMovePosition(_targetItem.AssignedStorage.AccessPosition(_ai.transform.position, _targetItem),
                    OnArrivedAtStorageForPickup, OnTaskCancel);
            }
        }

        private void OnFurnitureDelivered()
        {
            var droppedItem = _ai.DropCarriedItem(false);
            _requestingFurniture.PlaceFurniture(droppedItem);
            _targetItem = null;
            _furnitureToCraft = null;
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
            _furnitureToCraft = null;
            _requestingFurniture = null;
        }
    }
}
