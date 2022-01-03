using System;
using System.Collections;
using Character;
using Character.Interfaces;
using Gods;
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

                if (task is ConstructionTask.ConstructResourceIntoStructure)
                {
                    ExecuteTask_ConstructResourceIntoStructure(task as ConstructionTask.ConstructResourceIntoStructure);
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

        private void ExecuteTask_TakeItemToItemSlot(HaulingTask.TakeItemToItemSlot task)
        {
            workerMover.SetMovePosition(task.itemPosition, () =>
            {
                task.grabItem(this);
                workerMover.SetMovePosition(task.itemSlotPosition, () =>
                {
                    task.dropItem();
                    state = State.WaitingForNextTask;
                });
            });
        }

        private void ExecuteTask_ConstructResourceIntoStructure(ConstructionTask.ConstructResourceIntoStructure task)
        {
            workerMover.SetMovePosition(task.resourcePosition, () =>
            {
                task.grabResource(this);
                workerMover.SetMovePosition(task.structurePosition, () =>
                {
                    task.useResource();
                    DoWork(task.workAmount, () =>
                    {
                        task.completeWork();
                        state = State.WaitingForNextTask;
                    });
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