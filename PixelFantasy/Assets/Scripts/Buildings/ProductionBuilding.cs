using System.Collections.Generic;
using Items;
using ScriptableObjects;
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
    }
}
