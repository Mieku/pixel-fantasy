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
        
        // For Testing placeholder
        public Sprite rubble;
        public ItemData wood;

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

        private void Update()
        {
            // Left Click
            if (Input.GetMouseButtonDown(0))
            {
                //CreateWood();
            }
            
            // Right Click
            if (Input.GetMouseButtonDown(1))
            {
                CreateWood();
            }
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

        // TODO: Remove these when no longer needed
        #region Used For Testing
        
        private void CreateWood()
        {
            ItemSpawner.Instance.SpawnItem(wood, UtilsClass.GetMouseWorldPosition(), true);
        }
        
        private void AssignMoveLocation()
        {
            var newTask = new EmergencyTask.MoveToPosition
            {
                targetPosition = UtilsClass.GetMouseWorldPosition()
            };
            EmergencyTaskSystem.AddTask(newTask);
        }

        /// <summary>
        /// A variation of CreateGarbageOnMousePos, that doesn't allow for the task to be available immediately
        /// This is used to demo a task with a conditional
        /// </summary>
        /// <param name="secToWait">The amount of time to wait before this task can be executed</param>
        private void CreateGarbageAndWait(float secToWait)
        {
            var garbage = SpawnRubble(UtilsClass.GetMouseWorldPosition());
            var garbageSpriteRenderer = garbage.GetComponent<SpriteRenderer>();
            float cleanupTime = Time.time + secToWait;
            
            CleaningTaskSystem.EnqueueTask(() =>
            {
                if (Time.time >= cleanupTime)
                {
                    var newTask = new CleaningTask.GarbageCleanup
                    {
                        targetPosition = garbage.transform.position,
                        cleanUpAction = () =>
                        {
                            float alpha = 1f;
                            FunctionUpdater.Create(() =>
                            {
                                alpha -= Time.deltaTime;
                                garbageSpriteRenderer.color = new Color(1, 1, 1, alpha);
                                if (alpha <= 0f)
                                {
                                    Destroy(garbage);
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            });
                        }
                    };
                    return newTask;
                }
                else
                {
                    return null;
                }
            });
        }
        
        private void CreateGarbageOnMousePos()
        {
            var garbage = SpawnRubble(UtilsClass.GetMouseWorldPosition());
            var garbageSpriteRenderer = garbage.GetComponent<SpriteRenderer>();
            var newTask = new CleaningTask.GarbageCleanup
            {
                targetPosition = garbage.transform.position,
                cleanUpAction = () =>
                {
                    float alpha = 1f;
                    FunctionUpdater.Create(() =>
                    {
                        alpha -= Time.deltaTime * 1.1f;
                        garbageSpriteRenderer.color = new Color(1, 1, 1, alpha);
                        if (alpha <= 0f)
                        {
                            Destroy(garbage);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    });
                }
            };
            CleaningTaskSystem.AddTask(newTask);
        }

        private GameObject SpawnRubble(Vector3 position)
        {
            GameObject gameObject = new GameObject("Rubble", typeof(SpriteRenderer));
            gameObject.GetComponent<SpriteRenderer>().sprite = rubble;
            gameObject.GetComponent<SpriteRenderer>().sortingLayerName = "Item";
            gameObject.transform.position = position;

            return gameObject;
        }
        
        #endregion
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
