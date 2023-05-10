using System;
using Buildings;
using Characters;
using Gods;
using UnityEngine;

namespace TaskSystem
{
    public class TaskManager : God<TaskManager>
    {
        public TaskQueue LabourerTasks = new TaskQueue();
        public TaskQueue BuilderTasks = new TaskQueue();

        public CraftingBillQueue CraftingBills = new CraftingBillQueue();
        
        public Task GetNextTaskByProfession(Profession profession)
        {
            Task nextTask = profession switch
            {
                Profession.Labourer => LabourerTasks.NextTask,
                Profession.Builder => BuilderTasks.NextTask,
                _ => throw new ArgumentOutOfRangeException(nameof(profession), profession, null)
            };

            return nextTask;
        }

        public void AddBill(CraftingBill bill)
        {
            CraftingBills.Add(bill);
        }

        public void CancelBill(CraftingBill bill)
        {
            CraftingBills.Cancel(bill);
        }

        public CraftingBill GetNextCraftingBillByBuilding(ProductionBuilding building)
        {
            CraftingBill result = CraftingBills.GetNextCraftingBillByBuilding(building);
            return result;
        }
        
        public void AddTask(Task task)
        {
            switch (task.Profession)
            {
                case Profession.Labourer:
                    LabourerTasks.AddTask(task);
                    break;
                case Profession.Builder:
                    BuilderTasks.AddTask(task);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(task.Profession), task.Profession, null);
            }
        }
        
        public void CancelTask(Task task)
        {
            GameEvents.Trigger_OnTaskCancelled(task);
            
            switch (task.Profession)
            {
                case Profession.Labourer:
                    LabourerTasks.CancelTask(task);
                    break;
                case Profession.Builder:
                    BuilderTasks.CancelTask(task);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(task.Profession), task.Profession, null);
            }
        }
    }
}
