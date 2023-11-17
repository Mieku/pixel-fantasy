using System.Collections.Generic;
using Items;
using ScriptableObjects;
using Systems.Notifications.Scripts;
using TaskSystem;
using UnityEngine;

namespace Buildings
{
    public class ProductionBuilding : Building
    {
        private ProductionBuildingData _prodBuildingData => _buildingData as ProductionBuildingData;
        public ProductOrderQueue OrderQueue = new ProductOrderQueue();
        
        public Task CreateProductionTask(CraftingTable craftingTable)
        {
            var task = OrderQueue.RequestTask(craftingTable);
            if (task != null)
            {
                task.Enqueue();
            }

            if (task == null)
            {
                // Check Global
                var bill = TaskManager.Instance.GetNextCraftingBillByCraftingTable(craftingTable);
                if (bill != null)
                {
                    task = bill.CreateTask(craftingTable);
                    if (task != null)
                    {
                        task.Enqueue();
                    }
                }
            }
            
            return task;
        }
        
        public List<CraftedItemData> GetProductionOptions()
        {
            return new List<CraftedItemData>(_prodBuildingData.ProductionOptions);
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
    }
}
