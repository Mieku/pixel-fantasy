using System;
using System.Collections.Generic;
using CodeMonkey.Utils;
using Controllers;
using DataPersistence;
using DataPersistence.States;
using Tasks;
using Characters;
using UnityEngine;

namespace Gods
{
    [Serializable]
    public class TaskMaster : God<TaskMaster>
    {
        public TaskSystem<EmergencyTask> EmergencyTaskSystem = new TaskSystem<EmergencyTask>();
        public TaskSystem<HealingTask> HealingTaskSystem = new TaskSystem<HealingTask>();
        public TaskSystem<CookingTask> CookingTaskSystem = new TaskSystem<CookingTask>();
        public TaskSystem<HuntingTask> HuntingTaskSystem = new TaskSystem<HuntingTask>();
        public TaskSystem<ConstructionTask> ConstructionTaskSystem = new TaskSystem<ConstructionTask>();
        public TaskSystem<FarmingTask> FarmingTaskSystem = new TaskSystem<FarmingTask>();
        public TaskSystem<MiningTask> MiningTaskSystem = new TaskSystem<MiningTask>();
        public TaskSystem<FellingTask> FellingTaskSystem = new TaskSystem<FellingTask>();
        public TaskSystem<SmithingTask> SmithingTaskSystem = new TaskSystem<SmithingTask>();
        public TaskSystem<TailoringTask> TailoringTaskSystem = new TaskSystem<TailoringTask>();
        public TaskSystem<CarpentryTask> CarpentryTaskSystem = new TaskSystem<CarpentryTask>();
        public TaskSystem<MasonryTask> MasonryTaskSystem = new TaskSystem<MasonryTask>();
        public TaskSystem<HaulingTask> HaulingTaskSystem = new TaskSystem<HaulingTask>();
        public TaskSystem<CleaningTask> CleaningTaskSystem = new TaskSystem<CleaningTask>();
        public TaskSystem<ResearchTask> ResearchTaskSystem = new TaskSystem<ResearchTask>();

        private const float FUNC_PERIODIC_TIMER = 0.2f; // 200ms
        
        private bool _isGameLoading;
        private bool _isGameSaving;

        public void CancelQueuedTask(int taskRef)
        {
            EmergencyTaskSystem.CancelTask(taskRef);
            HealingTaskSystem.CancelTask(taskRef);
            CookingTaskSystem.CancelTask(taskRef);
            HuntingTaskSystem.CancelTask(taskRef);
            ConstructionTaskSystem.CancelTask(taskRef);
            FarmingTaskSystem.CancelTask(taskRef);
            MiningTaskSystem.CancelTask(taskRef);
            FellingTaskSystem.CancelTask(taskRef);
            SmithingTaskSystem.CancelTask(taskRef);
            TailoringTaskSystem.CancelTask(taskRef);
            CarpentryTaskSystem.CancelTask(taskRef);
            MasonryTaskSystem.CancelTask(taskRef);
            HaulingTaskSystem.CancelTask(taskRef);
            CleaningTaskSystem.CancelTask(taskRef);
            ResearchTaskSystem.CancelTask(taskRef);
        }
        
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

        protected override void Awake()
        {
            GameEvents.OnLoadingGameBeginning += OnGameLoadBegin;
            GameEvents.OnSavingGameBeginning += OnGameSaveStarted;
            GameEvents.OnSavingGameEnd += OnGameSaveEnded;
            GameEvents.OnLoadingGameEnd += OnGameLoadEnded;
        }

        private void OnDestroy()
        {
            GameEvents.OnLoadingGameBeginning -= OnGameLoadBegin;
            GameEvents.OnSavingGameBeginning -= OnGameSaveStarted;
            GameEvents.OnSavingGameEnd -= OnGameSaveEnded;
            GameEvents.OnLoadingGameEnd -= OnGameLoadEnded;
        }

        private void OnGameLoadBegin()
        {
            _isGameLoading = true;
            ClearAllTasksSystems();
        }

        private void Start()
        {
            InitFunctionPeriodics();
        }
        
        private void InitFunctionPeriodics()
        {
            FunctionPeriodic.Create(DequeueTasksAllTaskSystems, FUNC_PERIODIC_TIMER);
        }

        private void ClearAllTasksSystems()
        {
            EmergencyTaskSystem.ClearTasks();
            HealingTaskSystem.ClearTasks();
            CookingTaskSystem.ClearTasks();
            HuntingTaskSystem.ClearTasks();
            ConstructionTaskSystem.ClearTasks();
            FarmingTaskSystem.ClearTasks();
            MiningTaskSystem.ClearTasks();
            FellingTaskSystem.ClearTasks();
            SmithingTaskSystem.ClearTasks();
            TailoringTaskSystem.ClearTasks();
            CarpentryTaskSystem.ClearTasks();
            MasonryTaskSystem.ClearTasks();
            HaulingTaskSystem.ClearTasks();
            CleaningTaskSystem.ClearTasks();
            ResearchTaskSystem.ClearTasks();
        }

        /// <summary>
        /// Dequeues tasks from all of the stored Task Systems
        /// </summary>
        private void DequeueTasksAllTaskSystems()
        {
            if (_isGameLoading || _isGameSaving)
            {
                return;
            }

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
        
        private void OnGameLoadEnded()
        {
            _isGameLoading = false;
        }

        private void OnGameSaveStarted()
        {
            _isGameSaving = true;
        }

        private void OnGameSaveEnded()
        {
            _isGameSaving = false;
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

[Serializable]
public enum TaskType
{
    None,
    CutTree,
    HarvestFruit,
    CutPlant,
    ClearGrass,
    DigHole,
    PlantCrop,
    WaterCrop,
    HarvestCrop,
    Carpentry_CraftItem,
    Carpentry_GatherResourceForCrafting,
    GarbageCleanup,
    ConstructStructure,
    DeconstructStructure,
    EmergencyTask_MoveToPosition,
    TakeItemToItemSlot,
    TakeResourceToBlueprint
}
