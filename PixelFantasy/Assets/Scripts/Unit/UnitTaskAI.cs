using System;
using System.Collections;
using Character;
using Character.Interfaces;
using Gods;
using Items;
using Tasks;
using UnityEngine;

namespace Unit
{
    public class UnitTaskAI : MonoBehaviour
    {
        // TODO: Make this no longer be able to be changed in inspector later
        [SerializeField] private ProfessionData professionData;
        
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
                thought.SetThought(UnitThought.ThoughtState.None);
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
                
                // Other task types go here
            }
        }

        #region Execute Tasks

        private void ExecuteTask_MoveToPosition(EmergencyTask.MoveToPosition task)
        {
            thought.SetThought(UnitThought.ThoughtState.Moving);
            workerMover.SetMovePosition(task.targetPosition, () =>
            {
                state = State.WaitingForNextTask;
            });
        }

        private void ExecuteTask_CleanUpGarbage(CleaningTask.GarbageCleanup cleanupTask)
        {
            thought.SetThought(UnitThought.ThoughtState.Cleaning);
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
            workerMover.SetMovePosition(task.structurePosition, () =>
            {
                DoWork(task.workAmount, () =>
                {
                    task.completeWork();
                    state = State.WaitingForNextTask;
                });
            });
        }

        #endregion

        private void DoWork(float workAmount, Action onWorkComplete)
        {
            _onWorkComplete = onWorkComplete;
            thought.SetThought(UnitThought.ThoughtState.Building);
            StartCoroutine(WorkSequence(workAmount));
        }

        private IEnumerator WorkSequence(float workAmount)
        {
            float waitTimeS = workAmount * _workSpeed;
            yield return new WaitForSeconds(waitTimeS);
            
            thought.SetThought(UnitThought.ThoughtState.None);
            _onWorkComplete.Invoke();
        }
    }
}