using System;
using Buildings;
using Characters;
using Items;
using Managers;
using ScriptableObjects;
using UnityEngine;

namespace TaskSystem
{
    public class TaskManager : Singleton<TaskManager>
    {
        public TaskQueue HaulTasks = new TaskQueue(TaskType.Haul, AbilityType.Strength);
        public TaskQueue MineTasks = new TaskQueue(TaskType.Mine, AbilityType.Strength);
        public TaskQueue ConstructionTasks = new TaskQueue(TaskType.Construction, AbilityType.Strength);
        public TaskQueue HarvestTasks = new TaskQueue(TaskType.Harvest, AbilityType.Strength);
        public TaskQueue AnimalsTasks = new TaskQueue(TaskType.Animals, AbilityType.Strength);
        public TaskQueue ArtTasks = new TaskQueue(TaskType.Art, AbilityType.Strength);
        public TaskQueue CraftTasks = new TaskQueue(TaskType.Craft, AbilityType.Strength);
        public TaskQueue HuntTasks = new TaskQueue(TaskType.Hunt, AbilityType.Strength);
        public TaskQueue GrowTasks = new TaskQueue(TaskType.Grow, AbilityType.Strength);
        public TaskQueue CookTasks = new TaskQueue(TaskType.Cook, AbilityType.Strength);
        public TaskQueue HealTasks = new TaskQueue(TaskType.Heal, AbilityType.Intelligence);
        public TaskQueue ResearchTasks = new TaskQueue(TaskType.Research, AbilityType.Intelligence);
        public TaskQueue CleanTasks = new TaskQueue(TaskType.Clean, AbilityType.Strength);
        public TaskQueue Emergency = new TaskQueue(TaskType.Emergency, AbilityType.Strength);
        
        public Task GetNextTaskByType(TaskType taskType)
        {
            var queue = GetQueueByType(taskType);
            return queue.NextTask;
        }

        public TaskQueue GetQueueByType(TaskType taskType)
        {
            switch (taskType)
            {
                case TaskType.Haul:
                    return HaulTasks;
                case TaskType.Mine:
                    return MineTasks;
                case TaskType.Construction:
                    return ConstructionTasks;
                case TaskType.Harvest:
                    return HarvestTasks;
                case TaskType.Animals:
                    return AnimalsTasks;
                case TaskType.Art:
                    return ArtTasks;
                case TaskType.Craft:
                    return CraftTasks;
                case TaskType.Hunt:
                    return HuntTasks;
                case TaskType.Grow:
                    return GrowTasks;
                case TaskType.Cook:
                    return CookTasks;
                case TaskType.Heal:
                    return HealTasks;
                case TaskType.Research:
                    return ResearchTasks;
                case TaskType.Clean:
                    return CleanTasks;
                case TaskType.Emergency:
                    return Emergency;
                default:
                    throw new ArgumentOutOfRangeException(nameof(taskType), taskType, null);
            }
        }
        
        public void AddTask(Task task)
        {
            var queue = GetQueueByType(task.TaskType);
            queue.AddTask(task);
        }
        
        public void CancelTask(Task task)
        {
            GameEvents.Trigger_OnTaskCancelled(task);
            
            var queue = GetQueueByType(task.TaskType);
            queue.CancelTask(task);
        }
    }
}
