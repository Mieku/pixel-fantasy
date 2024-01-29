using System.Collections.Generic;
using System.Linq;
using Characters;
using Items;
using Managers;
using ScriptableObjects;
using Systems.Crafting.Scripts;
using Systems.Notifications.Scripts;
using TaskSystem;
using UnityEngine;

namespace Buildings
{
    public interface ICraftingBuilding : IBuilding
    {
        public bool AcceptNewOrders { get; set; }
        public bool PrioritizeOrdersWithMats { get; set; }
        public CraftingOrder CurrentCraftingOrder { get; set; }
        public List<CraftingOrder> QueuedOrders();
        public void ReturnCurrentOrderToQueue();
        public CraftingTable FindCraftingTable(CraftedItemData craftedItemData);
    }
    
    public class CraftingBuilding : Building, ICraftingBuilding
    {
        [SerializeField] private JobData _workersJob;
        [SerializeField] private List<CraftedItemData> _craftingOptions = new List<CraftedItemData>();
        public override BuildingType BuildingType => BuildingType.Crafting;

        public override string OccupantAdjective => "Workers";
        public bool AcceptNewOrders { get; set; }
        public bool PrioritizeOrdersWithMats { get; set; }
        public CraftingOrder CurrentCraftingOrder { get; set; }

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
                if (table.ItemBeingCrafted == craftedItemData)
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
            return CraftingOrdersManager.Instance.GetAllOrders(_workersJob);
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
    }
}
