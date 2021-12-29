using System;
using System.Collections.Generic;
using CodeMonkey.Utils;
using Tasks;
using Unit;
using UnityEngine;

namespace Gods
{
    public class TaskMaster : God<TaskMaster>
    {
        private readonly TaskSystem<EmergencyTask> _emergencyTaskSystem = new TaskSystem<EmergencyTask>();
        private readonly TaskSystem<CookingTask> _cookingTaskSystem = new TaskSystem<CookingTask>();
        private readonly TaskSystem<HuntingTask> _huntingTaskSystem = new TaskSystem<HuntingTask>();
        private readonly TaskSystem<ConstructionTask> _constructionTaskSystem = new TaskSystem<ConstructionTask>();
        private readonly TaskSystem<FarmingTask> _farmingTaskSystem = new TaskSystem<FarmingTask>();
        private readonly TaskSystem<MiningTask> _miningTaskSystem = new TaskSystem<MiningTask>();
        private readonly TaskSystem<FellingTask> _fellingTaskSystem = new TaskSystem<FellingTask>();
        private readonly TaskSystem<SmithingTask> _smithingTaskSystem = new TaskSystem<SmithingTask>();
        private readonly TaskSystem<TailoringTask> _tailoringTaskSystem = new TaskSystem<TailoringTask>();
        private readonly TaskSystem<CarpentryTask> _carpentryTaskSystem = new TaskSystem<CarpentryTask>();
        private readonly TaskSystem<MasonryTask> _masonryTaskSystem = new TaskSystem<MasonryTask>();
        private readonly TaskSystem<HaulingTask> _haulingTaskSystem = new TaskSystem<HaulingTask>();
        private readonly TaskSystem<CleaningTask> _cleaningTaskSystem = new TaskSystem<CleaningTask>();
        private readonly TaskSystem<ResearchTask> _researchTaskSystem = new TaskSystem<ResearchTask>();

        private List<ItemSlot> itemSlots = new List<ItemSlot>();

        private const float FUNC_PERIODIC_TIMER = 0.2f; // 200ms
        
        // For Testing placeholder
        public Sprite rubble;
        public Sprite wood;
        public Sprite whitePixel;


        public TaskBase GetNextTaskByCategory(TaskCategory category)
        {
            TaskBase nextTask = category switch
            {
                TaskCategory.Emergency => _emergencyTaskSystem.RequestNextTask(),
                TaskCategory.Cooking => _cookingTaskSystem.RequestNextTask(),
                TaskCategory.Hunting => _huntingTaskSystem.RequestNextTask(),
                TaskCategory.Construction => _constructionTaskSystem.RequestNextTask(),
                TaskCategory.Farming => _farmingTaskSystem.RequestNextTask(),
                TaskCategory.Mining => _miningTaskSystem.RequestNextTask(),
                TaskCategory.Felling => _fellingTaskSystem.RequestNextTask(),
                TaskCategory.Smithing => _smithingTaskSystem.RequestNextTask(),
                TaskCategory.Tailoring => _tailoringTaskSystem.RequestNextTask(),
                TaskCategory.Carpentry => _carpentryTaskSystem.RequestNextTask(),
                TaskCategory.Masonry => _masonryTaskSystem.RequestNextTask(),
                TaskCategory.Hauling => _haulingTaskSystem.RequestNextTask(),
                TaskCategory.Cleaning => _cleaningTaskSystem.RequestNextTask(),
                TaskCategory.Research => _researchTaskSystem.RequestNextTask(),
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
                CreateWood();
                //CreateGarbageOnMousePos();
            }
            
            // Right Click
            if (Input.GetMouseButtonDown(1))
            {
                CreateItemSlot();
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
            _emergencyTaskSystem.DequeueTasks();
            _cookingTaskSystem.DequeueTasks();
            _huntingTaskSystem.DequeueTasks();
            _constructionTaskSystem.DequeueTasks();
            _farmingTaskSystem.DequeueTasks();
            _miningTaskSystem.DequeueTasks();
            _fellingTaskSystem.DequeueTasks();
            _smithingTaskSystem.DequeueTasks();
            _tailoringTaskSystem.DequeueTasks();
            _carpentryTaskSystem.DequeueTasks();
            _masonryTaskSystem.DequeueTasks();
            _haulingTaskSystem.DequeueTasks();
            _cleaningTaskSystem.DequeueTasks();
            _researchTaskSystem.DequeueTasks();
        }

        // TODO: Remove these when no longer needed
        #region Used For Testing

        private void CreateItemSlot()
        {
            var itemSlotGO = SpawnItemSlot(UtilsClass.GetMouseWorldPosition());
            ItemSlot itemSlot = new ItemSlot(itemSlotGO.transform);
            itemSlots.Add(itemSlot);
        }

        private void CreateWood()
        {
            var woodGO = SpawnWood(UtilsClass.GetMouseWorldPosition());
            _haulingTaskSystem.EnqueueTask(() =>
            {
                ItemSlot emptySlot = null;
                foreach (var itemSlot in itemSlots)
                {
                    if (itemSlot.IsEmpty())
                    {
                        emptySlot = itemSlot;
                        break;
                    }
                }

                if (emptySlot != null)
                {
                    emptySlot.HasItemIncoming(true);
                    var task = new HaulingTask.TakeItemToItemSlot
                    {
                        itemPosition = woodGO.transform.position,
                        itemSlotPosition = emptySlot.GetPosition(),
                        grabItem = (UnitTaskAI unitTaskAI) =>
                        {
                            woodGO.transform.SetParent(unitTaskAI.transform);
                        },
                        dropItem = () =>
                        {
                            woodGO.transform.SetParent(null);
                            emptySlot.SetItemTransform(woodGO.transform);
                        },
                    };
                    return task;
                }
                else
                {
                    return null;
                }
            });
        }
        
        private void AssignMoveLocation()
        {
            var newTask = new EmergencyTask.MoveToPosition
            {
                targetPosition = UtilsClass.GetMouseWorldPosition()
            };
            _emergencyTaskSystem.AddTask(newTask);
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
            
            _cleaningTaskSystem.EnqueueTask(() =>
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
            _cleaningTaskSystem.AddTask(newTask);
        }

        private GameObject SpawnRubble(Vector3 position)
        {
            GameObject gameObject = new GameObject("Rubble", typeof(SpriteRenderer));
            gameObject.GetComponent<SpriteRenderer>().sprite = rubble;
            gameObject.GetComponent<SpriteRenderer>().sortingLayerName = "Item";
            gameObject.transform.position = position;

            return gameObject;
        }
        
        private GameObject SpawnWood(Vector3 position)
        {
            GameObject gameObject = new GameObject("Wood", typeof(SpriteRenderer));
            gameObject.GetComponent<SpriteRenderer>().sprite = wood;
            gameObject.GetComponent<SpriteRenderer>().sortingLayerName = "Item";
            gameObject.transform.position = position;
            gameObject.transform.localScale = new Vector3(.75f, .75f);

            return gameObject;
        }
        
        private GameObject SpawnItemSlot(Vector3 position)
        {
            GameObject gameObject = new GameObject("Item Slot", typeof(SpriteRenderer));
            gameObject.GetComponent<SpriteRenderer>().sprite = whitePixel;
            gameObject.GetComponent<SpriteRenderer>().color = new Color(.5f, .5f, .5f);
            gameObject.GetComponent<SpriteRenderer>().sortingLayerName = "Ground";
            gameObject.GetComponent<SpriteRenderer>().sortingOrder = 10;
            gameObject.transform.position = position;
            gameObject.transform.localScale = new Vector3(1, 1);

            return gameObject;
        }

        #endregion
        
        // TODO: For testing
        private class ItemSlot
        {
            private Transform itemSlotTransform;
            private Transform itemTransform;
            private bool hasItemIncoming;

            public ItemSlot(Transform itemSlotTransform)
            {
                this.itemSlotTransform = itemSlotTransform;
                SetItemTransform(null);
            }

            public bool IsEmpty()
            {
                return itemTransform == null && !hasItemIncoming;
            }

            public void HasItemIncoming(bool hasItemIncoming)
            {
                this.hasItemIncoming = hasItemIncoming;
            }

            public void SetItemTransform(Transform itemTransform)
            {
                this.itemTransform = itemTransform;
                hasItemIncoming = false;
                UpdateSprite();
            }

            public Vector3 GetPosition()
            {
                return itemSlotTransform.position;
            }

            public void UpdateSprite()
            {
                itemSlotTransform.GetComponent<SpriteRenderer>().color = IsEmpty() ? Color.gray : Color.red;
            }
        }
    }

    [Serializable]
    public enum TaskCategory
    {
        Emergency,
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
