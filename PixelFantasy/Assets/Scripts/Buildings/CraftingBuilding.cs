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
    public class CraftingBuilding : Building
    {
        private CraftingBuildingData _craftingBuildingData => _buildingData as CraftingBuildingData;
        public override BuildingType BuildingType => BuildingType.Crafting;

        public override string OccupantAdjective => "Workers";
        public CraftingTable CraftingTable;
        public bool AcceptNewOrders = true;
        public bool PrioritizeOrdersWithMats = true;
        public CraftingOrder CurrentCraftingOrder = null;

        public override List<Unit> GetPotentialOccupants()
        {
            var relevantAbilites = _craftingBuildingData.RelevantAbilityTypes;
            
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
                result = CraftingOrdersManager.Instance.GetNextCraftableOrder(_craftingBuildingData.WorkersJob, this);
            }

            if (result == null)
            {
                result = CraftingOrdersManager.Instance.GetNextOrder(_craftingBuildingData.WorkersJob);
            }

            GameEvents.Trigger_OnBuildingChanged(this);
            return result;
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
            return CraftingOrdersManager.Instance.GetAllOrders(_craftingBuildingData.WorkersJob);
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
            return _craftingBuildingData.WorkersJob;
        }
    }
}
