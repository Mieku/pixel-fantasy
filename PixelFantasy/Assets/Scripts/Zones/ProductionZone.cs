using System.Collections.Generic;
using Buildings;
using Controllers;
using Items;
using ScriptableObjects;
using TaskSystem;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Zones
{
    public class ProductionZone : RoomZone
    {
        private ProductionRoomData _prodRoomData => _roomData as ProductionRoomData;
        public override ZoneType ZoneType => ZoneType.Workshop;
        public ProductOrderQueue OrderQueue = new ProductOrderQueue();
        
        public ProductionZone(string uid, List<Vector3Int> gridPositions, LayeredRuleTile layeredRuleTile, ProductionRoomData roomData) : base(uid, gridPositions, layeredRuleTile, roomData)
        {
            
        }

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
            return new List<CraftedItemData>(_prodRoomData.ProductionOptions);
        }
        
        public override void ClickZone()
        {
            base.ClickZone();
            
            HUDController.Instance.ShowRoomDetails(this);
        }

        public override void UnclickZone()
        {
            base.UnclickZone();
        }
    }
}
