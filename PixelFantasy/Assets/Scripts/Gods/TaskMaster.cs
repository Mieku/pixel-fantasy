using System;
using System.Collections.Generic;
using CodeMonkey.Utils;
using Controllers;
using Tasks;
using Unit;
using UnityEngine;

namespace Gods
{
    public class TaskMaster : God<TaskMaster>
    {
        public readonly TaskSystem<EmergencyTask> EmergencyTaskSystem = new TaskSystem<EmergencyTask>();
        public readonly TaskSystem<HealingTask> HealingTaskSystem = new TaskSystem<HealingTask>();
        public readonly TaskSystem<CookingTask> CookingTaskSystem = new TaskSystem<CookingTask>();
        public readonly TaskSystem<HuntingTask> HuntingTaskSystem = new TaskSystem<HuntingTask>();
        public readonly TaskSystem<ConstructionTask> ConstructionTaskSystem = new TaskSystem<ConstructionTask>();
        public readonly TaskSystem<FarmingTask> FarmingTaskSystem = new TaskSystem<FarmingTask>();
        public readonly TaskSystem<MiningTask> MiningTaskSystem = new TaskSystem<MiningTask>();
        public readonly TaskSystem<FellingTask> FellingTaskSystem = new TaskSystem<FellingTask>();
        public readonly TaskSystem<SmithingTask> SmithingTaskSystem = new TaskSystem<SmithingTask>();
        public readonly TaskSystem<TailoringTask> TailoringTaskSystem = new TaskSystem<TailoringTask>();
        public readonly TaskSystem<CarpentryTask> CarpentryTaskSystem = new TaskSystem<CarpentryTask>();
        public readonly TaskSystem<MasonryTask> MasonryTaskSystem = new TaskSystem<MasonryTask>();
        public readonly TaskSystem<HaulingTask> HaulingTaskSystem = new TaskSystem<HaulingTask>();
        public readonly TaskSystem<CleaningTask> CleaningTaskSystem = new TaskSystem<CleaningTask>();
        public readonly TaskSystem<ResearchTask> ResearchTaskSystem = new TaskSystem<ResearchTask>();

        private const float FUNC_PERIODIC_TIMER = 0.2f; // 200ms
        
        public TaskBase GetNextTaskByCategory(TaskCategory category)
        {
            TaskBase nextTask = category switch
            {
                TaskCategory.Emergency => EmergencyTaskSystem.RequestNextTask(),
                TaskCategory.Healing => HealingTaskSystem.RequestNextTask(),
                TaskCategory.Cooking => CookingTaskSystem.RequestNextTask(),
                TaskCategory.Hunting => HuntingTaskSystem.RequestNextTask(),
                TaskCategory.Construction => ConstructionTaskSystem.RequestNextTask(),
                TaskCategory.Farming => FarmingTaskSystem.RequestNextTask(),
                TaskCategory.Mining => MiningTaskSystem.RequestNextTask(),
                TaskCategory.Felling => FellingTaskSystem.RequestNextTask(),
                TaskCategory.Smithing => SmithingTaskSystem.RequestNextTask(),
                TaskCategory.Tailoring => TailoringTaskSystem.RequestNextTask(),
                TaskCategory.Carpentry => CarpentryTaskSystem.RequestNextTask(),
                TaskCategory.Masonry => MasonryTaskSystem.RequestNextTask(),
                TaskCategory.Hauling => HaulingTaskSystem.RequestNextTask(),
                TaskCategory.Cleaning => CleaningTaskSystem.RequestNextTask(),
                TaskCategory.Research => ResearchTaskSystem.RequestNextTask(),
                _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
            };

            return nextTask;
        }
        
        private void Start()
        {
            InitFunctionPeriodics();
        }
        
        private void InitFunctionPeriodics()
        {
            FunctionPeriodic.Create(DequeueTasksAllTaskSystems, FUNC_PERIODIC_TIMER);
        }

        /// <summary>
        /// Dequeues tasks from all of the stored Task Systems
        /// </summary>
        private void DequeueTasksAllTaskSystems()
        {
            EmergencyTaskSystem.DequeueTasks();
            HealingTaskSystem.DequeueTasks();
            CookingTaskSystem.DequeueTasks();
            HuntingTaskSystem.DequeueTasks();
            ConstructionTaskSystem.DequeueTasks();
            FarmingTaskSystem.DequeueTasks();
            MiningTaskSystem.DequeueTasks();
            FellingTaskSystem.DequeueTasks();
            SmithingTaskSystem.DequeueTasks();
            TailoringTaskSystem.DequeueTasks();
            CarpentryTaskSystem.DequeueTasks();
            MasonryTaskSystem.DequeueTasks();
            HaulingTaskSystem.DequeueTasks();
            CleaningTaskSystem.DequeueTasks();
            ResearchTaskSystem.DequeueTasks();
        }
    }

    [Serializable]
    public enum TaskCategory
    {
        Emergency,
        Healing,
        Cooking,
        Hunting,
        Construction,
        Farming, 
        Mining,
        Felling,
        Smithing,
        Tailoring,
        Carpentry,
        Masonry,
        Hauling,
        Cleaning,
        Research
    }
}
