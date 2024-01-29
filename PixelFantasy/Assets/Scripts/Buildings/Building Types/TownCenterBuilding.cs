using System.Collections.Generic;
using System.Linq;
using Items;
using ScriptableObjects;
using Systems.Crafting.Scripts;
using TaskSystem;
using UnityEngine;

namespace Buildings.Building_Types
{
    public class TownCenterBuilding : Building, IStockpileBuilding, IProductionBuilding, IEateryBuilding, ICraftingBuilding
    {
        [SerializeField] private JobData _workersJob;
        [SerializeField] private List<CraftedItemData> _productionOptions = new List<CraftedItemData>();
        [SerializeField] private List<CraftedItemData> _craftingOptions = new List<CraftedItemData>();
        
        public override BuildingType BuildingType => BuildingType.TownHall;
        
        public override string OccupantAdjective => "Workers";
        
        private List<ItemData> _unallowedItems = new List<ItemData>();
        

        public List<ProductionSettings> ProductionSettings { get; } = new List<ProductionSettings>();

        protected override void Start()
        {
            base.Start();
            CreateProductionSettings();
        }

        public bool IsItemStockpileAllowed(ItemData itemData)
        {
            return !_unallowedItems.Contains(itemData);
        }
        
        public void SetAllowedStockpileItem(ItemData itemData, bool isAllowed)
        {
            if (isAllowed)
            {
                if (_unallowedItems.Contains(itemData))
                {
                    _unallowedItems.Remove(itemData);
                }
            }
            else
            {
                if (!_unallowedItems.Contains(itemData))
                {
                    _unallowedItems.Add(itemData);
                }
            }
        }
        
        private void CreateProductionSettings()
        {
            foreach (var craftedOption in _productionOptions)
            {
                ProductionSettings newSetting = new ProductionSettings(craftedOption);
                ProductionSettings.Add(newSetting);
            }
        }
        
        public float GetProductionProgress(CraftedItemData item)
        {
            var craftingTable = FindCraftingTable(item);
            if (craftingTable == null) return 0;

            if (craftingTable.ItemBeingCrafted != item) return 0;

            return craftingTable.GetPercentCraftingComplete();
        }
        
        public override Task GetBuildingTask()
        {
            Task result = base.GetBuildingTask();
            if (result == null)
            {
                for (int i = 0; i < ProductionSettings.Count; i++)
                {
                    var setting = ProductionSettings[i];
                    if (setting.AreMaterialsAvailable() && !setting.IsLimitReached())
                    {
                        result = setting.CreateTask(this, OnTaskComplete);
                        break;
                    }
                }
            }

            if (result != null)
            {
                GameEvents.Trigger_OnBuildingChanged(this);
            }
            
            return result;
        }
        
        private void OnTaskComplete(Task task)
        {
            
        }

        public bool AcceptNewOrders { get; set; }
        public bool PrioritizeOrdersWithMats { get; set; }
        public CraftingOrder CurrentCraftingOrder { get; set; }

        public List<CraftingOrder> QueuedOrders()
        {
            return CraftingOrdersManager.Instance.GetAllOrders(_workersJob);
        }

        public void ReturnCurrentOrderToQueue()
        {
            CraftingOrdersManager.Instance.SubmitOrder(CurrentCraftingOrder);
            CurrentCraftingOrder = RequestNextCraftingOrder();
        }
        
        private CraftingOrder RequestNextCraftingOrder()
        {
            CraftingOrder result = null;
            if (PrioritizeOrdersWithMats)
            {
                result = CraftingOrdersManager.Instance.GetNextCraftableOrder(_workersJob, this);
            }

            if (result == null)
            {
                result = CraftingOrdersManager.Instance.GetNextOrder(_workersJob);
            }

            GameEvents.Trigger_OnBuildingChanged(this);
            return result;
        }

        public CraftingTable FindCraftingTable(CraftedItemData craftedItemData)
        {
            var allCraftingTables = CraftingTables;
            foreach (var table in allCraftingTables)
            {
                if (craftedItemData.RequiredCraftingTable == table.FurnitureItemData)
                {
                    return table;
                }
            }

            return null;
        }

        public Item ClaimBestAvailableFood()
        {
            var storages = GetBuildingStorages();
            List<Item> storedFood = new List<Item>();
            foreach (var storage in storages)
            {
                var food = storage.GetAllFoodItems(false, false);
                storedFood.AddRange(food);
            }

            if (storedFood.Count == 0)
            {
                return null;
            }
            
            var selectedFood = storedFood.OrderByDescending(food => ((IFoodItem)food.GetItemData()).FoodNutrition).ToList()[0];
            selectedFood.ClaimItem();
            return selectedFood;
        }
    }
}
