using Characters;
using Controllers;
using TaskSystem;
using UnityEngine;

namespace Buildings
{
    public class ProductionBuilding : Building
    {
        protected override void OnBuildingClicked()
        {
            HUDController.Instance.ShowBuildingDetails(this);
        }

        public Task GetTask()
        {
            var buildingTask = BuildingTasks.NextTask;
            if (buildingTask != null)
            {
                return buildingTask;
            }
            
            // Grab a possible crafting bill
            CraftingBill bill = TaskManager.Instance.GetNextCraftingBillByBuilding(this);
            if (bill != null)
            {
                Task billTask = bill.CreateTask();
                return billTask;
            }

            return null;
        }
        
        public void AssignWorker(UnitState unit)
        {
            Occupants.Add(unit);
            unit.Occupation = this;
            unit.Profession = BuildingData.WorkersProfession;
        }
    }
}
