// using System.Collections.Generic;
// using Handlers;
// using Items;
// using Managers;
// using Systems.Appearance.Scripts;
//
// namespace TaskSystem
// {
//     public class ProduceItemAction : TaskAction // ID: Produce Item
//     {
//         private CraftedItemSettings _itemToCraft;
//         private CraftingTable _craftingTable;
//         private List<ItemData> _materials;
//         private ETaskState _state;
//         private IStorage _receivingStorage;
//         
//         private Item _targetItem;
//         private int _materialIndex;
//         private float _timer;
//
//         private enum ETaskState
//         {
//             ClaimTable,
//             GatherMats,
//             WaitingOnMats,
//             CraftItem,
//             DeliverItem,
//             WaitingOnDelivery,
//         }
//         
//         public override void PrepareAction(Task task)
//         {
//             _itemToCraft = task.Payload as CraftedItemSettings;
//             _craftingTable = (CraftingTable)task.Requestor;
//             _materials = task.Materials;
//             _state = ETaskState.ClaimTable;
//         }
//
//         public override void DoAction()
//         {
//             if (_state == ETaskState.ClaimTable)
//             {
//                 //_craftingTable.AssignItemToTable(_itemToCraft, _materials);
//                 _state = ETaskState.GatherMats;
//             }
//
//             if (_state == ETaskState.GatherMats)
//             {
//                 _targetItem = _materials[_materialIndex].GetLinkedItem();
//                 _ai.Kinling.KinlingAgent.SetMovePosition(_targetItem.RuntimeData.AssignedStorage.AccessPosition(_ai.Kinling.transform.position, _targetItem.RuntimeData),
//                     OnArrivedAtStorageForPickup, OnTaskCancel);
//                 _state = ETaskState.WaitingOnMats;
//             }
//
//             if (_state == ETaskState.WaitingOnMats)
//             {
//                 
//             }
//
//             if (_state == ETaskState.CraftItem)
//             {
//                 KinlingAnimController.SetUnitAction(UnitAction.Doing, _ai.GetActionDirection(_craftingTable.transform.position));
//                 _timer += TimeManager.Instance.DeltaTime;
//                 if(_timer >= ActionSpeed) 
//                 {
//                     _timer = 0;
//                     if (_craftingTable.DoCraft(_ai.Kinling.Stats))
//                     {
//                         KinlingAnimController.SetUnitAction(UnitAction.Nothing);
//
//                         var data = _itemToCraft.CreateItemData();
//                         _targetItem = ItemsDatabase.Instance.CreateItemObject(data, _craftingTable.transform.position, false);
//                         
//                         _ai.HoldItem(_targetItem);
//                         
//                         _state = ETaskState.DeliverItem;
//                     }
//                 }
//             }
//
//             if (_state == ETaskState.DeliverItem)
//             {
//                 _receivingStorage = InventoryManager.Instance.GetAvailableStorage(_targetItem.RuntimeData.Settings);
//                 if (_receivingStorage == null)
//                 {
//                     // THROW IT ON THE GROUND!
//                     _ai.DropCarriedItem(true);
//                     ConcludeAction();
//                     return;
//                 }
//                 
//                 _receivingStorage.SetIncoming(_targetItem.RuntimeData);
//                 
//                 _ai.Kinling.KinlingAgent.SetMovePosition(_receivingStorage.AccessPosition(_ai.Kinling.transform.position, _targetItem.RuntimeData), OnProductDelivered,OnTaskCancel);
//                 _state = ETaskState.WaitingOnDelivery;
//             }
//         }
//
//         private void OnArrivedAtStorageForPickup()
//         {
//             _targetItem.RuntimeData.AssignedStorage.WithdrawItem(_targetItem.RuntimeData);
//             _ai.HoldItem(_targetItem);
//             _ai.Kinling.KinlingAgent.SetMovePosition(_craftingTable.UseagePosition(_ai.Kinling.transform.position), OnArrivedAtCraftingTable, OnTaskCancel);
//         }
//
//         private void OnArrivedAtCraftingTable()
//         {
//             _craftingTable.ReceiveMaterial(_targetItem.RuntimeData);
//             _targetItem = null;
//             _materialIndex++;
//
//             // Are there more items to gather?
//             if (_materialIndex > _materials.Count - 1)
//             {
//                 _ai.Kinling.KinlingAgent.SetMovePosition(_craftingTable.UseagePosition(_ai.Kinling.transform.position), () =>
//                 {
//                     _state = ETaskState.CraftItem;
//                 }, OnTaskCancel);
//             }
//             else
//             {
//                 _targetItem = _materials[_materialIndex].GetLinkedItem();
//                 _ai.Kinling.KinlingAgent.SetMovePosition(_targetItem.RuntimeData.AssignedStorage.AccessPosition(_ai.Kinling.transform.position, _targetItem.RuntimeData),
//                     OnArrivedAtStorageForPickup, OnTaskCancel);
//             }
//         }
//
//         private void OnProductDelivered()
//         {
//             _ai.DepositHeldItemInStorage(_receivingStorage);
//             _targetItem = null;
//             ConcludeAction();
//         }
//
//         public override void OnTaskCancel()
//         {
//             base.OnTaskCancel();
//         }
//
//         public override void ConcludeAction()
//         {
//             base.ConcludeAction();
//             
//             KinlingAnimController.SetUnitAction(UnitAction.Nothing);
//             _task = null;
//             _itemToCraft = null;
//             _materialIndex = 0;
//             _receivingStorage = null;
//         }
//     }
// }
