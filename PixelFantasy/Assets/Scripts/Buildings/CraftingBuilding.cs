using System;
using System.Collections.Generic;
using System.Linq;
using Characters;
using Items;
using Managers;
using ScriptableObjects;
using Sirenix.OdinInspector;
using Systems.Crafting.Scripts;
using Systems.Notifications.Scripts;
using TaskSystem;
using UnityEngine;

namespace Buildings
{
    public interface ICraftingBuilding : IBuilding
    {
        public bool PrioritizeOrdersWithMats { get; set; }
        public CraftingOrder CurrentCraftingOrder { get; set; }
        public List<CraftingOrder> QueuedOrders();
        public void ReturnCurrentOrderToQueue();
        public CraftingTable FindCraftingTable(CraftedItemData craftedItemData);
        public List<CraftedItemData> CraftingOptions { get; }
        public BuildingCraftQueue BuildingCraftQueue { get; }
        public void CreateLocalOrder(CraftedItemData itemToOrder);
    }
    
    public class CraftingBuilding : Building, ICraftingBuilding
    {
        [SerializeField] private JobData _workersJob;
        [SerializeField] private List<CraftedItemData> _craftingOptions = new List<CraftedItemData>();
        public override BuildingType BuildingType => BuildingType.Crafting;

        public override string OccupantAdjective => "Workers";
        public bool PrioritizeOrdersWithMats { get; set; } = true;
        public CraftingOrder CurrentCraftingOrder { get; set; }

        public List<CraftedItemData> CraftingOptions => _craftingOptions;
        [ShowInInspector] public BuildingCraftQueue BuildingCraftQueue { get; } = new BuildingCraftQueue();
        
        public override List<Unit> GetPotentialOccupants()
        {
            var relevantAbilites = _buildingData.RelevantAbilityTypes;
            
            var unemployed = UnitsManager.Instance.UnemployedKinlings;
            List<Unit> sortedKinlings = unemployed
                .OrderByDescending(kinling => kinling.RelevantStatScore(relevantAbilites)).ToList();
            return sortedKinlings;
        }

        public void ReturnCurrentOrderToQueue()
        {
            if (CurrentCraftingOrder.IsGlobal)
            {
                CraftingOrdersManager.Instance.SubmitOrder(CurrentCraftingOrder);
            }
            else
            {
                CraftingTable tableForOrder = FindCraftingTable(CurrentCraftingOrder.CraftedItem);
                BuildingCraftQueue.SubmitOrder(CurrentCraftingOrder, tableForOrder);
            }
            
            CurrentCraftingOrder = RequestNextCraftingOrder();
        }

        private CraftingOrder RequestNextCraftingOrder()
        {
            CraftingOrder result = null;
            
            // Do Building's orders first
            if (PrioritizeOrdersWithMats)
            {
                result = BuildingCraftQueue.GetNextCraftableOrder(this);
            }

            if (result == null)
            {
                result = BuildingCraftQueue.GetNextOrder(this);
            }
            
            // Global Orders
            if (result == null && PrioritizeOrdersWithMats)
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
                if (craftedItemData.IsCraftingTableValid(table.FurnitureItemData))
                {
                    return table;
                }
            }

            return null;
        }

        public override Task GetBuildingTask()
        {
            Task result = base.GetBuildingTask();
            if (result == null)
            {
                if (CurrentCraftingOrder?.CraftedItem == null)
                {
                    CurrentCraftingOrder = RequestNextCraftingOrder();
                }

                if (CurrentCraftingOrder?.State == CraftingOrder.EOrderState.Queued)
                {
                    result = CurrentCraftingOrder?.CreateTask(this, OnOrderComplete);
                }
            }

            GameEvents.Trigger_OnBuildingChanged(this);
            return result;
        }

        public List<CraftingOrder> QueuedOrders()
        {
            var allOrders = BuildingCraftQueue.GetAllOrders(this);
            allOrders.AddRange(CraftingOrdersManager.Instance.GetAllOrders(_workersJob));

            return allOrders;
        }

        private void OnOrderComplete(Task task)
        {
            CurrentCraftingOrder = null;
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
        
        public override JobData GetBuildingJob()
        {
            return _workersJob;
        }
        
        public void CreateLocalOrder(CraftedItemData itemToOrder)
        {
            CraftingTable tableForOrder = FindCraftingTable(itemToOrder);
            CraftingOrder order = new CraftingOrder(itemToOrder, this, CraftingOrder.EOrderType.Item, false, null,
                null, null);
            
            BuildingCraftQueue.SubmitOrder(order, tableForOrder);
        }
    }
}
