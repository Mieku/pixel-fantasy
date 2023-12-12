using System.Collections.Generic;
using System.Linq;
using Characters;
using Items;
using Managers;
using ScriptableObjects;
using Systems.Crafting.Scripts;
using Systems.Notifications.Scripts;
using TaskSystem;

namespace Buildings
{
    public class CraftingBuilding : Building
    {
        private CraftingBuildingData _prodBuildingData => _buildingData as CraftingBuildingData;
        //public ProductOrderQueue OrderQueue = new ProductOrderQueue();

        public override string OccupantAdjective => "Workers";
        public bool AcceptNewOrders = true;
        public bool PrioritizeOrdersWithMats = true;
        public CraftingOrder CurrentCraftingOrder;

        public override List<Unit> GetPotentialOccupants()
        {
            var relevantAbilites = _prodBuildingData.RelevantAbilityTypes;
            
            var unemployed = UnitsManager.Instance.UnemployedKinlings;
            List<Unit> sortedKinlings = unemployed
                .OrderByDescending(kinling => kinling.GetUnitState().RelevantAbilityScore(relevantAbilites)).ToList();
            return sortedKinlings;
        }

        private CraftingOrder RequestNextCraftingOrder()
        {
            CraftingOrder result = null;
            if (PrioritizeOrdersWithMats)
            {
                result = CraftingOrdersManager.Instance.GetNextCraftableOrder(_prodBuildingData.WorkersJob, this);
            }

            if (result == null)
            {
                result = CraftingOrdersManager.Instance.GetNextOrder(_prodBuildingData.WorkersJob);
            }

            return result;
        }

        public override Task GetBuildingTask()
        {
            Task result = base.GetBuildingTask();
            if (result == null)
            {
                if (CurrentCraftingOrder == null)
                {
                    CurrentCraftingOrder = RequestNextCraftingOrder();
                    result = CurrentCraftingOrder?.CreateTask(this);
                }
            }

            return result;
        }

        // public Task CreateProductionTask(CraftingTable craftingTable)
        // {
        //     var task = OrderQueue.RequestTask(craftingTable);
        //     if (task != null)
        //     {
        //         task.Enqueue();
        //     }
        //
        //     if (task == null)
        //     {
        //         // Check Global
        //         var bill = TaskManager.Instance.GetNextCraftingBillByCraftingTable(craftingTable);
        //         if (bill != null)
        //         {
        //             task = bill.CreateTask(craftingTable);
        //             if (task != null)
        //             {
        //                 task.Enqueue();
        //             }
        //         }
        //     }
        //     
        //     return task;
        // }
        
        // public List<CraftedItemData> GetProductionOptions()
        // {
        //     return new List<CraftedItemData>(_prodBuildingData.CraftingOptions);
        // }

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
    }
}
