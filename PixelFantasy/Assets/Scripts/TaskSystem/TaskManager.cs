using System;
using Gods;
using UnityEngine;

namespace TaskSystem
{
    public class TaskManager : God<TaskManager>
    {
        public TaskQueue EmergencyTasks = new TaskQueue();
        public TaskQueue HealingTasks = new TaskQueue();
        public TaskQueue CookingTasks = new TaskQueue();
        public TaskQueue HuntingTasks = new TaskQueue();
        public TaskQueue ConstructionTasks = new TaskQueue();
        public TaskQueue FarmingTasks= new TaskQueue();
        public TaskQueue MiningTasks = new TaskQueue();
        public TaskQueue FellingTasks = new TaskQueue();
        public TaskQueue SmithingTasks = new TaskQueue();
        public TaskQueue TailoringTasks = new TaskQueue();
        public TaskQueue CarpentryTasks = new TaskQueue();
        public TaskQueue MasonryTasks = new TaskQueue();
        public TaskQueue HaulingTasks = new TaskQueue();
        public TaskQueue CleaningTasks = new TaskQueue();
        public TaskQueue ResearchTasks = new TaskQueue();
        
        public Task GetNextTaskByCategory(TaskCategory category)
        {
            Task nextTask = category switch
            {
                TaskCategory.Emergency => EmergencyTasks.NextTask,
                TaskCategory.Healing => HealingTasks.NextTask,
                TaskCategory.Cooking => CookingTasks.NextTask,
                TaskCategory.Hunting => HuntingTasks.NextTask,
                TaskCategory.Construction => ConstructionTasks.NextTask,
                TaskCategory.Farming => FarmingTasks.NextTask,
                TaskCategory.Mining => MiningTasks.NextTask,
                TaskCategory.Felling => FellingTasks.NextTask,
                TaskCategory.Smithing => SmithingTasks.NextTask,
                TaskCategory.Tailoring => TailoringTasks.NextTask,
                TaskCategory.Carpentry => CarpentryTasks.NextTask,
                TaskCategory.Masonry => MasonryTasks.NextTask,
                TaskCategory.Hauling => HaulingTasks.NextTask,
                TaskCategory.Cleaning => CleaningTasks.NextTask,
                TaskCategory.Research => ResearchTasks.NextTask,
                _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
            };

            return nextTask;
        }
        
        public void AddTask(Task task)
        {
            // Debug.Log($"New Task Created: {task.TaskId}");
            switch (task.Category)
            {
                case TaskCategory.Emergency:
                    EmergencyTasks.AddTask(task);
                    break;
                case TaskCategory.Healing:
                    HealingTasks.AddTask(task);
                    break;
                case TaskCategory.Cooking:
                    CookingTasks.AddTask(task);
                    break;
                case TaskCategory.Hunting:
                    HuntingTasks.AddTask(task);
                    break;
                case TaskCategory.Construction:
                    ConstructionTasks.AddTask(task);
                    break;
                case TaskCategory.Farming:
                    FarmingTasks.AddTask(task);
                    break;
                case TaskCategory.Mining:
                    MiningTasks.AddTask(task);
                    break;
                case TaskCategory.Felling:
                    FellingTasks.AddTask(task);
                    break;
                case TaskCategory.Smithing:
                    SmithingTasks.AddTask(task);
                    break;
                case TaskCategory.Tailoring:
                    TailoringTasks.AddTask(task);
                    break;
                case TaskCategory.Carpentry:
                    CarpentryTasks.AddTask(task);
                    break;
                case TaskCategory.Masonry:
                    MasonryTasks.AddTask(task);
                    break;
                case TaskCategory.Hauling:
                    HaulingTasks.AddTask(task);
                    break;
                case TaskCategory.Cleaning:
                    CleaningTasks.AddTask(task);
                    break;
                case TaskCategory.Research:
                    ResearchTasks.AddTask(task);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(task.Category), task.Category, null);
            }
        }
        
        public void CancelTask(Task task)
        {
            GameEvents.Trigger_OnTaskCancelled(task);
            
            switch (task.Category)
            {
                case TaskCategory.Emergency:
                    EmergencyTasks.CancelTask(task);
                    break;
                case TaskCategory.Healing:
                    HealingTasks.CancelTask(task);
                    break;
                case TaskCategory.Cooking:
                    CookingTasks.CancelTask(task);
                    break;
                case TaskCategory.Hunting:
                    HuntingTasks.CancelTask(task);
                    break;
                case TaskCategory.Construction:
                    ConstructionTasks.CancelTask(task);
                    break;
                case TaskCategory.Farming:
                    FarmingTasks.CancelTask(task);
                    break;
                case TaskCategory.Mining:
                    MiningTasks.CancelTask(task);
                    break;
                case TaskCategory.Felling:
                    FellingTasks.CancelTask(task);
                    break;
                case TaskCategory.Smithing:
                    SmithingTasks.CancelTask(task);
                    break;
                case TaskCategory.Tailoring:
                    TailoringTasks.CancelTask(task);
                    break;
                case TaskCategory.Carpentry:
                    CarpentryTasks.CancelTask(task);
                    break;
                case TaskCategory.Masonry:
                    MasonryTasks.CancelTask(task);
                    break;
                case TaskCategory.Hauling:
                    HaulingTasks.CancelTask(task);
                    break;
                case TaskCategory.Cleaning:
                    CleaningTasks.CancelTask(task);
                    break;
                case TaskCategory.Research:
                    ResearchTasks.CancelTask(task);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(task.Category), task.Category, null);
            }
        }
    }
}
