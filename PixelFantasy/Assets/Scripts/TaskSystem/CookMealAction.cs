using System.Collections.Generic;
using Items;
using Managers;
using Systems.Appearance.Scripts;

namespace TaskSystem
{
    public class CookMealAction : TaskAction // ID: Cook Meal
    {
        private MealSettings _mealToCook;
        private CraftingTable _craftingTable;
        private List<ItemData> _claimedIngredients;
        private ETaskState _state;
        private IStorage _receivingStorage;
        
        private ItemData _targetItem;
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
            _mealToCook = task.Payload as MealSettings;
            _craftingTable = (CraftingTable)task.Requestor;
            _claimedIngredients = task.Materials;
            _state = ETaskState.ClaimTable;
        }

        public override void DoAction()
        {
            if (_state == ETaskState.ClaimTable)
            {
                _craftingTable.AssignMealToTable(_mealToCook, _claimedIngredients);
                _state = ETaskState.GatherMats;
            }

            if (_state == ETaskState.GatherMats)
            {
                _targetItem = _claimedIngredients[0];
                _ai.Kinling.KinlingAgent.SetMovePosition(_targetItem.AssignedStorage.AccessPosition(_ai.transform.position, _targetItem),
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
                    if (_craftingTable.DoCraft(_ai.Kinling.Stats))
                    {
                        KinlingAnimController.SetUnitAction(UnitAction.Nothing);
                        
                        var item = Spawner.Instance.SpawnItem(_mealToCook, _craftingTable.transform.position, false);
                        _targetItem = item.RuntimeData;
                        _ai.HoldItem(item);
                        
                        _state = ETaskState.DeliverItem;
                    }
                }
            }

            if (_state == ETaskState.DeliverItem)
            {
                _receivingStorage = InventoryManager.Instance.GetAvailableStorage(_targetItem.Settings);
                if (_receivingStorage == null)
                {
                    // THROW IT ON THE GROUND!
                    _ai.DropCarriedItem(true);
                    ConcludeAction();
                    return;
                }
                
                _receivingStorage.SetIncoming(_targetItem);
                
                _ai.Kinling.KinlingAgent.SetMovePosition(_receivingStorage.AccessPosition(_ai.Kinling.transform.position, _targetItem), OnProductDelivered, OnTaskCancel);
                _state = ETaskState.WaitingOnDelivery;
            }
        }

        private void OnArrivedAtStorageForPickup()
        {
            var item = _targetItem.AssignedStorage.WithdrawItem(_targetItem);
            _ai.HoldItem(item);
            _ai.Kinling.KinlingAgent.SetMovePosition(_craftingTable.UseagePosition(_ai.Kinling.transform.position), OnArrivedAtCraftingTable, OnTaskCancel);
        }

        private void OnArrivedAtCraftingTable()
        {
            _craftingTable.ReceiveMaterial(_targetItem);
            _targetItem = null;

            if (_claimedIngredients.Count > 0)
            {
                _targetItem = _claimedIngredients[0];
                _ai.Kinling.KinlingAgent.SetMovePosition(_targetItem.AssignedStorage.AccessPosition(_ai.transform.position, _targetItem),
                    OnArrivedAtStorageForPickup, OnTaskCancel);
            }
            else
            {
                _ai.Kinling.KinlingAgent.SetMovePosition(_craftingTable.UseagePosition(_ai.Kinling.transform.position), () =>
                {
                    _state = ETaskState.CraftItem;
                }, OnTaskCancel);
            }
        }

        private void OnProductDelivered()
        {
            _ai.DepositHeldItemInStorage(_receivingStorage);
            _targetItem = null;
            ConcludeAction();
        }

        public override void OnTaskCancel()
        {
            _ai.DropCarriedItem(true);
            base.OnTaskCancel();
        }

        public override void ConcludeAction()
        {
            base.ConcludeAction();
            
            KinlingAnimController.SetUnitAction(UnitAction.Nothing);
            _task = null;
            _mealToCook = null;
            _receivingStorage = null;
        }
    }
}
