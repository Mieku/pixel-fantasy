using System;
using System.Collections;
using Characters;
using Characters.Interfaces;
using DataPersistence;
using Gods;
using Items;
using Tasks;
using UnityEngine;
using Action = System.Action;
using Random = UnityEngine.Random;

namespace Characters
{
    public class UnitTaskAI : MonoBehaviour
    {
        // TODO: Make this no longer be able to be changed in inspector later
        [SerializeField] private ProfessionData professionData;
        [SerializeField] private UnitAnimController _unitAnim;
        [SerializeField] private GameObject _itemPrefab;
        
        public enum State
        {
            WaitingForNextTask,
            ExecutingTask
        }
    
        private UnitThought thought;
        private IMovePosition workerMover;

        private TaskBase _curTask;
        private State state;
        private float waitingTimer;
        private Action _onWorkComplete;
        private float _workSpeed = 1f;
        private Item _heldItem;

        private const float WAIT_TIMER_MAX = .2f; // 200ms

        private static TaskMaster taskMaster => TaskMaster.Instance;

        private void Awake()
        {
            workerMover = GetComponent<IMovePosition>();
            thought = GetComponent<UnitThought>();
        }
        
        private void Update()
        {
            switch (state)
            {
                case State.WaitingForNextTask:
                    // Waiting to request the next task
                    waitingTimer -= Time.deltaTime;
                    if (waitingTimer <= 0)
                    {
                        waitingTimer = WAIT_TIMER_MAX;
                        RequestNextTask();
                    }
                    break;
                case State.ExecutingTask:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Gets a position next to the workPosition so unit isn't standing on their target
        /// </summary>
        private Vector2 GetAdjacentPosition(Vector2 workPosition, float distanceAway = 1f)
        {
            // Choose a random side of the object
            // TODO: Add in a check to ensure the location can be reached
            var sideMod = distanceAway;
            var rand = Random.Range(0, 2);
            if (rand == 1)
            {
                sideMod *= -1;
            }

            var standPos = workPosition;
            standPos.x += sideMod;

            return standPos;
        }

        public void AssignHeldItem(Item itemToHold)
        {
            _heldItem = itemToHold;
            itemToHold.transform.SetParent(transform);
        }

        private void RequestNextTask()
        {
            TaskBase task = null;
            foreach (var category in professionData.SortedPriorities)
            {
                task = taskMaster.GetNextTaskByCategory(category);
                if (task != null)
                {
                    break;
                }
            }

            _curTask = task;
            if (task == null)
            {
                state = State.WaitingForNextTask;
                _unitAnim.SetUnitAction(UnitAction.Nothing);
            }
            else
            {
                ExecuteTask(task);
            }
        }

        private void ExecuteTask(TaskBase task)
        {
            state = State.ExecutingTask;
                // Move to location
                if (task is EmergencyTask.MoveToPosition)
                {
                    ExecuteTask_MoveToPosition(task as EmergencyTask.MoveToPosition);
                    return;
                }
                
                // Clean up garbage
                if (task is CleaningTask.GarbageCleanup)
                {
                    ExecuteTask_CleanUpGarbage(task as CleaningTask.GarbageCleanup);
                    return;
                }
                
                // Pick up item and move to slot
                if (task is HaulingTask.TakeItemToItemSlot)
                {
                    ExecuteTask_TakeItemToItemSlot(task as HaulingTask.TakeItemToItemSlot);
                    return;
                }
                
                if (task is HaulingTask.TakeResourceToBlueprint)
                {
                    ExecuteTask_TakeResourceToBlueprint(task as HaulingTask.TakeResourceToBlueprint);
                    return;
                }
                
                if (task is ConstructionTask.ConstructStructure)
                {
                    ExecuteTask_ConstructStructure(task as ConstructionTask.ConstructStructure);
                    return;
                }

                if (task is ConstructionTask.DeconstructStructure)
                {
                    ExecuteTask_DeconstructStructure(task as ConstructionTask.DeconstructStructure);
                    return;
                }

                if (task is FellingTask.CutTree)
                {
                    ExecuteTask_CutTree(task as FellingTask.CutTree);
                    return;
                }

                if (task is FarmingTask.CutPlant)
                {
                    ExecuteTask_CutPlant(task as FarmingTask.CutPlant);
                    return;
                }

                if (task is FarmingTask.HarvestFruit)
                {
                    ExecuteTask_HarvestFruit(task as FarmingTask.HarvestFruit);
                    return;
                }

                if (task is FarmingTask.ClearGrass)
                {
                    ExecuteTask_ClearGrass(task as FarmingTask.ClearGrass);
                    return;
                }

                if (task is CarpentryTask.CraftItem)
                {
                    ExecuteTask_CraftItem_Carpentry(task as CarpentryTask.CraftItem);
                    return;
                }
                
                if (task is CarpentryTask.GatherResourceForCrafting)
                {
                    ExecuteTask_GatherResourceForCrafting_Carpentry(task as CarpentryTask.GatherResourceForCrafting);
                    return;
                }

                if (task is FarmingTask.DigHole)
                {
                    ExecuteTask_DigHole(task as FarmingTask.DigHole);
                    return;
                }
                
                if (task is FarmingTask.HarvestCrop)
                {
                    ExecuteTask_HarvestCrop(task as FarmingTask.HarvestCrop);
                    return;
                }
                
                if (task is FarmingTask.PlantCrop)
                {
                    ExecuteTask_PlantCrop(task as FarmingTask.PlantCrop);
                    return;
                }
                
                if (task is FarmingTask.WaterCrop)
                {
                    ExecuteTask_WaterCrop(task as FarmingTask.WaterCrop);
                    return;
                }
                
                // Other task types go here
        }

        #region Execute Tasks

        private void ExecuteTask_MoveToPosition(EmergencyTask.MoveToPosition task)
        {
            //thought.SetThought(UnitThought.ThoughtState.Moving);
            workerMover.SetMovePosition(task.targetPosition, () =>
            {
                state = State.WaitingForNextTask;
            });
        }

        private void ExecuteTask_CleanUpGarbage(CleaningTask.GarbageCleanup cleanupTask)
        {
            //thought.SetThought(UnitThought.ThoughtState.Cleaning);
            workerMover.SetMovePosition(cleanupTask.targetPosition, () =>
            {
                cleanupTask.cleanUpAction();
                state = State.WaitingForNextTask;
            });
        }

        public StorageSlot claimedSlot;
        private void ExecuteTask_TakeItemToItemSlot(HaulingTask.TakeItemToItemSlot task)
        {
            task.claimItemSlot(this);
            workerMover.SetMovePosition(task.itemPosition, () =>
            {
                task.grabItem(this);
                workerMover.SetMovePosition(claimedSlot.GetPosition(), () =>
                {
                    task.dropItem();
                    claimedSlot.StoreItem(task.item);
                    state = State.WaitingForNextTask;
                    claimedSlot = null;
                });
            });
        }

        private void ExecuteTask_TakeResourceToBlueprint(HaulingTask.TakeResourceToBlueprint task)
        {
            workerMover.SetMovePosition(task.resourcePosition, () =>
            {
                task.grabResource(this);
                workerMover.SetMovePosition(task.blueprintPosition, () =>
                {
                    task.useResource(_heldItem);
                    state = State.WaitingForNextTask;
                });
            });
        }

        private void ExecuteTask_ConstructStructure(ConstructionTask.ConstructStructure task)
        {
            workerMover.SetMovePosition(GetAdjacentPosition(task.structurePosition), () =>
            {
                _unitAnim.LookAtPostion(task.structurePosition);
                _unitAnim.SetUnitAction(UnitAction.Building);
                DoWork(task.workAmount, () =>
                {
                    task.completeWork();
                    state = State.WaitingForNextTask;
                    _unitAnim.SetUnitAction(UnitAction.Nothing);
                });
            });
        }

        private void ExecuteTask_DeconstructStructure(ConstructionTask.DeconstructStructure task)
        {
            task.claimStructure(this);
            workerMover.SetMovePosition(GetAdjacentPosition(task.structurePosition), () =>
            {
                _unitAnim.LookAtPostion(task.structurePosition);
                _unitAnim.SetUnitAction(UnitAction.Building);
                DoWork(task.workAmount, () =>
                {
                    task.completeWork();
                    state = State.WaitingForNextTask;
                    _unitAnim.SetUnitAction(UnitAction.Nothing);
                });
            });
        }

        private void ExecuteTask_CutTree(FellingTask.CutTree task)
        {
            task.claimTree(this);
            workerMover.SetMovePosition(GetAdjacentPosition(task.treePosition), () =>
            {
                _unitAnim.LookAtPostion(task.treePosition);
                _unitAnim.SetUnitAction(UnitAction.Axe);
                DoWork(task.workAmount, () =>
                {
                    task.completeWork();
                    state = State.WaitingForNextTask;
                    _unitAnim.SetUnitAction(UnitAction.Nothing);
                });
            });
        }
        
        private void ExecuteTask_CutPlant(FarmingTask.CutPlant task)
        {
            task.claimPlant(this);
            workerMover.SetMovePosition(GetAdjacentPosition(task.plantPosition, 0.5f), () =>
            {
                _unitAnim.LookAtPostion(task.plantPosition);
                _unitAnim.SetUnitAction(UnitAction.Doing);
                DoWork(task.workAmount, () =>
                {
                    task.completeWork();
                    state = State.WaitingForNextTask;
                    _unitAnim.SetUnitAction(UnitAction.Nothing);
                });
            });
        }
        
        private void ExecuteTask_HarvestFruit(FarmingTask.HarvestFruit task)
        {
            task.claimPlant(this);
            workerMover.SetMovePosition(GetAdjacentPosition(task.plantPosition, 0.5f), () =>
            {
                _unitAnim.LookAtPostion(task.plantPosition);
                _unitAnim.SetUnitAction(UnitAction.Doing);
                DoWork(task.workAmount, () =>
                {
                    task.completeWork();
                    state = State.WaitingForNextTask;
                    _unitAnim.SetUnitAction(UnitAction.Nothing);
                });
            });
        }
        
        private void ExecuteTask_ClearGrass(FarmingTask.ClearGrass task)
        {
            task.claimDirt(this);
            workerMover.SetMovePosition(GetAdjacentPosition(task.grassPosition), () =>
            {
                _unitAnim.LookAtPostion(task.grassPosition);
                _unitAnim.SetUnitAction(UnitAction.Digging);
                DoWork(task.workAmount, () =>
                {
                    task.completeWork();
                    state = State.WaitingForNextTask;
                    _unitAnim.SetUnitAction(UnitAction.Nothing);
                });
            });
        }

        private void ExecuteTask_DigHole(FarmingTask.DigHole task)
        {
            task.claimHole(this);
            workerMover.SetMovePosition(GetAdjacentPosition(task.holePosition), () =>
            {
                _unitAnim.LookAtPostion(task.holePosition);
                _unitAnim.SetUnitAction(UnitAction.Digging);
                DoWork(task.workAmount, () =>
                {
                    task.completeWork();
                    state = State.WaitingForNextTask;
                    _unitAnim.SetUnitAction(UnitAction.Nothing);
                });
            });
        }
        
        private void ExecuteTask_PlantCrop(FarmingTask.PlantCrop task)
        {
            task.claimHole(this);
            workerMover.SetMovePosition(GetAdjacentPosition(task.holePosition), () =>
            {
                _unitAnim.LookAtPostion(task.holePosition);
                _unitAnim.SetUnitAction(UnitAction.Doing);
                DoWork(task.workAmount, () =>
                {
                    task.completeWork();
                    state = State.WaitingForNextTask;
                    _unitAnim.SetUnitAction(UnitAction.Nothing);
                });
            });
        }
        
        private void ExecuteTask_WaterCrop(FarmingTask.WaterCrop task)
        {
            task.claimCrop(this);
            workerMover.SetMovePosition(GetAdjacentPosition(task.cropPosition), () =>
            {
                _unitAnim.LookAtPostion(task.cropPosition);
                _unitAnim.SetUnitAction(UnitAction.Watering);
                DoWork(task.workAmount, () =>
                {
                    task.completeWork();
                    state = State.WaitingForNextTask;
                    _unitAnim.SetUnitAction(UnitAction.Nothing);
                });
            });
        }
        
        private void ExecuteTask_HarvestCrop(FarmingTask.HarvestCrop task)
        {
            task.claimCrop(this);
            workerMover.SetMovePosition(GetAdjacentPosition(task.cropPosition, 0.5f), () =>
            {
                _unitAnim.LookAtPostion(task.cropPosition);
                _unitAnim.SetUnitAction(UnitAction.Digging);
                DoWork(task.workAmount, () =>
                {
                    task.completeWork();
                    state = State.WaitingForNextTask;
                    _unitAnim.SetUnitAction(UnitAction.Nothing);
                });
            });
        }

        private void ExecuteTask_CraftItem_Carpentry(CarpentryTask.CraftItem task)
        {
            workerMover.SetMovePosition(task.craftPosition, () =>
            {
                _unitAnim.LookAtPostion(task.craftPosition);
                _unitAnim.SetUnitAction(UnitAction.Doing);
                DoWork(task.workAmount, () =>
                {
                    task.completeWork();
                    state = State.WaitingForNextTask;
                    _unitAnim.SetUnitAction(UnitAction.Nothing);
                });
            });
        }
        
        private void ExecuteTask_GatherResourceForCrafting_Carpentry(CarpentryTask.GatherResourceForCrafting task)
        {
            workerMover.SetMovePosition(task.resourcePosition, () =>
            {
                task.grabResource(this);
                workerMover.SetMovePosition(task.craftingTable.transform.position, () =>
                {
                    task.useResource(_heldItem);
                    state = State.WaitingForNextTask;
                });
            });
        }

        #endregion

        private void DoWork(float workAmount, Action onWorkComplete)
        {
            _onWorkComplete = onWorkComplete;
            StartCoroutine(WorkSequence(workAmount));
        }

        private IEnumerator WorkSequence(float workAmount)
        {
            float waitTimeS = workAmount * _workSpeed;
            yield return new WaitForSeconds(waitTimeS);
            
            _onWorkComplete.Invoke();
        }

        public void CancelTask()
        {
            state = State.WaitingForNextTask;
            StopAllCoroutines();
            workerMover.SetMovePosition(transform.position, () => {});
        }
        
        public UnitTaskData GetSaveData()
        {
            TaskType taskType = TaskType.None;
            string targetUID = "";
            string slotUID = "";
            object heldItemData = null;
            
            if(claimedSlot != null)
            {
                slotUID = claimedSlot.UniqueId;
            }

            if (state == State.ExecutingTask)
            {
                taskType = _curTask.TaskType;
                targetUID = _curTask.TargetUID;
            }

            if (_heldItem != null)
            {
                heldItemData = _heldItem.CaptureState();
            }

            return new UnitTaskData
            {
                CurTask = taskType,
                WasExecutingTask = state == State.ExecutingTask,
                TargetUID = targetUID,
                ClaimedSlotUID = slotUID,
                HeldItemData = heldItemData,
            };
        }

        public void SetLoadData(UnitTaskData taskData)
        {
            if (!string.IsNullOrEmpty(taskData.ClaimedSlotUID))
            {
                var slotGO = UIDManager.Instance.GetGameObject(taskData.ClaimedSlotUID);
                if (slotGO != null)
                {
                    claimedSlot = slotGO.GetComponent<StorageSlot>();
                }
            }

            // If the unit had a held item, spawn it
            if (taskData.HeldItemData != null)
            {
                var itemData = (Item.Data)taskData.HeldItemData;
                var childObj = Instantiate(_itemPrefab, transform);
                childObj.GetComponent<IPersistent>().RestoreState(itemData);
                _heldItem = childObj.GetComponent<Item>();
            }

            if (taskData.WasExecutingTask)
            {
                ResumeLoadedTask(taskData);
            }
        }

        private void ResumeLoadedTask(UnitTaskData taskData)
        {
            var curTask = taskData.CurTask;
            var targetGO = UIDManager.Instance.GetGameObject(taskData.TargetUID);

            switch (curTask)
            {
                case TaskType.None:
                    break;
                case TaskType.CutTree:
                    var tree = targetGO.GetComponent<TreeResource>();
                    ExecuteTask(tree.CreateCutTreeTask(false));
                    break;
                case TaskType.HarvestFruit:
                    var fruitingPlant = targetGO.GetComponent<GrowingResource>();
                    ExecuteTask(fruitingPlant.CreateHarvestFruitTask(false));
                    break;
                case TaskType.CutPlant:
                    var plantToCut = targetGO.GetComponent<GrowingResource>();
                    ExecuteTask(plantToCut.CreateCutPlantTask(false));
                    break;
                case TaskType.ClearGrass:
                case TaskType.DigHole:
                case TaskType.PlantCrop:
                case TaskType.WaterCrop:
                case TaskType.HarvestCrop:
                case TaskType.Carpentry_CraftItem:
                case TaskType.Carpentry_GatherResourceForCrafting:
                case TaskType.GarbageCleanup:
                case TaskType.ConstructStructure:
                case TaskType.DeconstructStructure:
                case TaskType.EmergencyTask_MoveToPosition:
                case TaskType.TakeItemToItemSlot:
                    var item = targetGO.GetComponent<Item>();
                    ControllerManager.Instance.InventoryController.AddItemToPending(item);
                    claimedSlot.AddItemIncoming(item);
                    ExecuteTask(item.CreateHaulTaskForSlot(claimedSlot));
                    break;
                case TaskType.TakeResourceToBlueprint:
                    var structure = targetGO.GetComponent<Structure>();
                    if (structure != null)
                    {
                        if (_heldItem != null)
                        {
                            var task_resourceToBlueprint = structure.CreateTakeResourceToBlueprintTask(_heldItem);
                            ExecuteTask(task_resourceToBlueprint);
                        }
                        else if(claimedSlot != null)
                        {
                            var task_resourceToBlueprint = structure.CreateTakeResourceFromSlotToBlueprintTask(claimedSlot);
                            ExecuteTask(task_resourceToBlueprint);
                        }
                    }

                    var floor = targetGO.GetComponent<Floor>();
                    if (floor != null)
                    {
                        // TODO: Build me!
                    }
                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public struct UnitTaskData
        {
            public TaskType CurTask;
            public string TargetUID;
            public bool WasExecutingTask;
            public string ClaimedSlotUID;
            public object HeldItemData;
        }
    }
}