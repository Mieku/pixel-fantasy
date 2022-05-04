using System;
using System.Collections;
using Character;
using Character.Interfaces;
using Gods;
using Items;
using Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Unit
{
    public class UnitTaskAI : MonoBehaviour
    {
        // TODO: Make this no longer be able to be changed in inspector later
        [SerializeField] private ProfessionData professionData;
        [SerializeField] private UnitAnimController _unitAnim;
        
        private enum State
        {
            WaitingForNextTask,
            ExecutingTask,
        }
    
        private IMovePosition workerMover;
        private State state;
        private float waitingTimer;
        private UnitThought thought;
        private Action _onWorkComplete;
        private float _workSpeed = 1f;

        private const float WAIT_TIMER_MAX = .2f; // 200ms

        private static TaskMaster taskMaster => TaskMaster.Instance;

        private void Awake()
        {
            workerMover = GetComponent<IMovePosition>();
            thought = GetComponent<UnitThought>();
        }

        private void Start()
        {
            state = State.WaitingForNextTask;
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

            if (task == null)
            {
                state = State.WaitingForNextTask;
                _unitAnim.SetUnitAction(UnitAction.Nothing);
            }
            else
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
                
                // Other task types go here
            }
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
                    claimedSlot.SetItem(task.item);
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
                    task.useResource();
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
                    task.useResource();
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
    }
}