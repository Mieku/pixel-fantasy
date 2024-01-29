using System;
using System.Collections.Generic;
using System.Linq;
using Characters;
using Items;
using Managers;
using ScriptableObjects;
using Systems.Notifications.Scripts;
using TaskSystem;
using UnityEngine;

namespace Buildings
{
    public interface IProductionBuilding : IBuilding
    {
        public float GetProductionProgress(CraftedItemData item);
        public List<ProductionSettings> ProductionSettings { get; }
        public CraftingTable FindCraftingTable(CraftedItemData craftedItemData);
    }
    
    public class ProductionBuilding : Building, IProductionBuilding
    {
        [SerializeField] private JobData _workersJob;
        [SerializeField] private List<CraftedItemData> _productionOptions = new List<CraftedItemData>();
        
        public override BuildingType BuildingType => BuildingType.Production;
        
        public List<ProductionSettings> ProductionSettings { get; } = new List<ProductionSettings>();

        public override string OccupantAdjective => "Workers";

        protected override void Start()
        {
            base.Start();

            CreateProductionSettings();
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

        public override List<Unit> GetPotentialOccupants()
        {
            var relevantAbilites = _buildingData.RelevantAbilityTypes;
            
            var unemployed = UnitsManager.Instance.UnemployedKinlings;
            List<Unit> sortedKinlings = unemployed
                .OrderByDescending(kinling => kinling.RelevantStatScore(relevantAbilites)).ToList();
            return sortedKinlings;
        }

        protected override bool CheckForIssues()
        {
            bool hasIssue = base.CheckForIssues();

            // Check if has no workers
            if (_state == BuildingState.Built && GetOccupants().Count == 0)
            {
                hasIssue = true;
                var noWorkersNote = _buildingNotes.Find(note => note.ID == "No Workers");
                if (noWorkersNote == null)
                {
                    _buildingNotes.Add(new BuildingNote("There are no workers!", false, "No Workers"));
                }
            }
            else
            {
                var noWorkersNote = _buildingNotes.Find(note => note.ID == "No Workers");
                if (noWorkersNote != null)
                {
                    _buildingNotes.Remove(noWorkersNote);
                }
            }


            if (hasIssue)
            {
                _buildingNotification.SetNotification(BuildingNotification.ENotificationType.Issue);
            }
            else
            {
                _buildingNotification.SetNotification(BuildingNotification.ENotificationType.None);
            }

            return hasIssue;
        }

        private void CreateProductionSettings()
        {
            foreach (var craftedOption in _productionOptions)
            {
                ProductionSettings newSetting = new ProductionSettings(craftedOption);
                ProductionSettings.Add(newSetting);
            }
        }
        
        public override JobData GetBuildingJob()
        {
            return _workersJob;
        }
    }
    
    [Serializable]
    public class ProductionSettings
    {
        public CraftedItemData CraftedItem;
        public bool HasLimit;
        public int Limit;
        public bool IsPaused;

        public ProductionSettings(CraftedItemData craftedItemData)
        {
            CraftedItem = craftedItemData;
            HasLimit = false;
            Limit = 0;
            IsPaused = false;
        }

        public bool IsLimitReached()
        {
            if (IsPaused) return true;
            if (!HasLimit) return false;

            int curAmount = InventoryManager.Instance.GetAmountAvailable(CraftedItem, true);
            return curAmount >= Limit;
        }

        public bool AreMaterialsAvailable()
        {
            foreach (var resourceCost in CraftedItem.GetResourceCosts())
            {
                if (!InventoryManager.Instance.CanAfford(resourceCost.Item, resourceCost.Quantity))
                {
                    return false;
                }
            }
            return true;
        }
        
        private List<Item> ClaimRequiredMaterials(IBuilding building)
        {
            var requiredItems = CraftedItem.GetResourceCosts();
            List<Item> claimedItems = new List<Item>();
            
            foreach (var requiredItem in requiredItems)
            {
                for (int i = 0; i < requiredItem.Quantity; i++)
                {
                    // Check building storage first, then check global
                    var claimedItem = InventoryManager.Instance.ClaimItemBuilding(requiredItem.Item, building);
                    if (claimedItem == null)
                    {
                        claimedItem = InventoryManager.Instance.ClaimItemGlobal(requiredItem.Item);
                    }

                    if (claimedItem == null)
                    {
                        // If for some reason the building can't get everything, unclaim all the materials and return null
                        foreach (var itemToUnclaim in claimedItems)
                        {
                            itemToUnclaim.UnclaimItem();
                        }

                        return null;
                    }
                    else
                    {
                        claimedItems.Add(claimedItem);
                    }
                }
            }

            return claimedItems;
        }

        public Task CreateTask(IProductionBuilding building, Action<Task> onTaskComplete)
        {
            List<Item> claimedMats = ClaimRequiredMaterials(building);
            if (claimedMats == null)
            {
                return null;
            }
            
            Task task = new Task("Produce Item", (Building)building, building.GetBuildingJob(), EToolType.None)
            {
                Payload = CraftedItem.ItemName,
                OnTaskComplete = onTaskComplete,
                Materials = claimedMats,
            };

            return task;
        }
    }
}
