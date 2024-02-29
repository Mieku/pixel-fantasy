// using System.Collections.Generic;
// using System.Linq;
// using Characters;
// using Items;
// using Managers;
// using ScriptableObjects;
// using Sirenix.OdinInspector;
// using Systems.Crafting.Scripts;
// using TaskSystem;
// using UnityEngine;
//
// namespace Buildings.Building_Types
// {
//     public class TownCenterBuilding : Building, IStockpileBuilding, IProductionBuilding, IEateryBuilding, ICraftingBuilding
//     {
//         [SerializeField] private JobData _workersJob;
//         [SerializeField] private List<CraftedItemData> _productionOptions = new List<CraftedItemData>();
//         [SerializeField] private List<CraftedItemData> _craftingOptions = new List<CraftedItemData>();
//         
//         public override BuildingType BuildingType => BuildingType.TownHall;
//         
//         public override string OccupantAdjective => "Workers";
//         
//         private List<ItemData> _unallowedItems = new List<ItemData>();
//         
//
//         public List<ProductionSettings> ProductionSettings { get; } = new List<ProductionSettings>();
//
//         public List<CraftedItemData> CraftingOptions => _craftingOptions;
//         [ShowInInspector] public BuildingCraftQueue BuildingCraftQueue { get; } = new BuildingCraftQueue();
//         
//         protected override void Start()
//         {
//             base.Start();
//             CreateProductionSettings();
//         }
//
//         public bool IsItemStockpileAllowed(ItemData itemData)
//         {
//             return !_unallowedItems.Contains(itemData);
//         }
//         
//         public void SetAllowedStockpileItem(ItemData itemData, bool isAllowed)
//         {
//             if (isAllowed)
//             {
//                 if (_unallowedItems.Contains(itemData))
//                 {
//                     _unallowedItems.Remove(itemData);
//                 }
//             }
//             else
//             {
//                 if (!_unallowedItems.Contains(itemData))
//                 {
//                     _unallowedItems.Add(itemData);
//                 }
//             }
//         }
//         
//         private void CreateProductionSettings()
//         {
//             foreach (var craftedOption in _productionOptions)
//             {
//                 ProductionSettings newSetting = new ProductionSettings(craftedOption);
//                 ProductionSettings.Add(newSetting);
//             }
//         }
//         
//         public float GetProductionProgress(CraftedItemData item)
//         {
//             var craftingTable = FindCraftingTable(item);
//             if (craftingTable == null) return 0;
//
//             if (craftingTable.ItemBeingCrafted != item) return 0;
//
//             return craftingTable.GetPercentCraftingComplete();
//         }
//         
//         public override Task GetBuildingTask()
//         {
//             Task result = base.GetBuildingTask();
//             
//             // Crafting
//             if (result == null)
//             {
//                 if (CurrentCraftingOrder?.CraftedItem == null)
//                 {
//                     CurrentCraftingOrder = RequestNextCraftingOrder();
//                 }
//
//                 if (CurrentCraftingOrder?.State == CraftingOrder.EOrderState.Queued)
//                 {
//                     //result = CurrentCraftingOrder?.CreateTask(this, OnOrderComplete);
//                 }
//             }
//             
//             // Production
//             if (result == null)
//             {
//                 for (int i = 0; i < ProductionSettings.Count; i++)
//                 {
//                     var setting = ProductionSettings[i];
//                     if (setting.AreMaterialsAvailable() && !setting.IsLimitReached())
//                     {
//                         result = setting.CreateTask(this, OnTaskComplete);
//                         break;
//                     }
//                 }
//             }
//
//             if (result != null)
//             {
//                 GameEvents.Trigger_OnBuildingChanged(this);
//             }
//             
//             return result;
//         }
//
//         public override JobData GetBuildingJob()
//         {
//             return _workersJob;
//         }
//
//         private void OnTaskComplete(Task task)
//         {
//             
//         }
//         
//         private void OnOrderComplete(Task task)
//         {
//             CurrentCraftingOrder = null;
//         }
//
//         public bool PrioritizeOrdersWithMats { get; set; } = true;
//         public CraftingOrder CurrentCraftingOrder { get; set; }
//
//         public override List<Kinling> GetPotentialOccupants()
//         {
//             return KinlingsManager.Instance.UnemployedKinlings;
//         }
//
//         public List<CraftingOrder> QueuedOrders()
//         {
//             var allOrders = BuildingCraftQueue.GetAllOrders(this);
//             //allOrders.AddRange(CraftingOrdersManager.Instance.GetAllOrders(_workersJob));
//
//             return allOrders;
//         }
//
//         public void ReturnCurrentOrderToQueue()
//         {
//             if (CurrentCraftingOrder.IsGlobal)
//             {
//                 CraftingOrdersManager.Instance.SubmitOrder(CurrentCraftingOrder);
//             }
//             else
//             {
//                 CraftingTable tableForOrder = FindCraftingTable(CurrentCraftingOrder.CraftedItem);
//                 BuildingCraftQueue.SubmitOrder(CurrentCraftingOrder, tableForOrder);
//             }
//             
//             CurrentCraftingOrder = RequestNextCraftingOrder();
//         }
//         
//         private CraftingOrder RequestNextCraftingOrder()
//         {
//             CraftingOrder result = null;
//             
//             // Do Building's orders first
//             if (PrioritizeOrdersWithMats)
//             {
//                 result = BuildingCraftQueue.GetNextCraftableOrder(this);
//             }
//
//             if (result == null)
//             {
//                 result = BuildingCraftQueue.GetNextOrder(this);
//             }
//             
//             // Global Orders
//             if (result == null && PrioritizeOrdersWithMats)
//             {
//                 //result = CraftingOrdersManager.Instance.GetNextCraftableOrder(_workersJob, this);
//             }
//
//             if (result == null)
//             {
//                 //result = CraftingOrdersManager.Instance.GetNextOrder(_workersJob);
//             }
//
//             GameEvents.Trigger_OnBuildingChanged(this);
//             return result;
//         }
//
//         public CraftingTable FindCraftingTable(CraftedItemData craftedItemData)
//         {
//             var allCraftingTables = CraftingTables;
//             foreach (var table in allCraftingTables)
//             {
//                 if (craftedItemData.IsCraftingTableValid(table.FurnitureItemData))
//                 {
//                     return table;
//                 }
//             }
//
//             return null;
//         }
//
//         public Item ClaimBestAvailableFood()
//         {
//             var storages = GetBuildingStorages();
//             List<Item> storedFood = new List<Item>();
//             foreach (var storage in storages)
//             {
//                 var food = storage.GetAllFoodItems(false, false);
//                 storedFood.AddRange(food);
//             }
//
//             if (storedFood.Count == 0)
//             {
//                 return null;
//             }
//             
//             var selectedFood = storedFood.OrderByDescending(food => ((IFoodItem)food.GetItemData()).FoodNutrition).ToList()[0];
//             selectedFood.ClaimItem();
//             return selectedFood;
//         }
//
//         public void CreateLocalOrder(CraftedItemData itemToOrder)
//         {
//             CraftingTable tableForOrder = FindCraftingTable(itemToOrder);
//             CraftingOrder order = new CraftingOrder(itemToOrder, this, CraftingOrder.EOrderType.Item, false, null,
//                 null, null);
//             
//             BuildingCraftQueue.SubmitOrder(order, tableForOrder);
//         }
//     }
// }
