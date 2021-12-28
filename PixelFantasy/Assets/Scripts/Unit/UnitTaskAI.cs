using System;
using Character;
using Character.Interfaces;
using Gods;
using UnityEngine;

namespace Unit
{
    public class UnitTaskAI : MonoBehaviour
    {
        private enum State
        {
            WaitingForNextTask,
            ExecutingTask,
        }
    
        private IMovePosition workerMover;
        private State state;
        private float waitingTimer;
        private TaskSystem<TaskMaster.Task> taskSystem;
        private UnitThought thought;

        private const float WAIT_TIMER_MAX = .2f; // 200ms

        private static TaskMaster taskMaster => TaskMaster.Instance;

        private void Awake()
        {
            taskSystem = taskMaster.GetTaskSystem();
            workerMover = GetComponent<IMovePosition>();
            thought = GetComponent<UnitThought>();
        }

        private void Start()
        {
            state = State.WaitingForNextTask;
        }

        // public void Setup(IMovePosition workerMover, TaskSystem taskSystem)
        // {
        //     this.workerMover = workerMover;
        //     this.taskSystem = taskSystem;
        //     state = State.WaitingForNextTask;
        // }

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
            var task = taskSystem.RequestNextTask();
            if (task == null)
            {
                state = State.WaitingForNextTask;
                thought.SetThought(UnitThought.ThoughtState.None);
            }
            else
            {
                state = State.ExecutingTask;
                // Move to location
                if (task is TaskMaster.Task.MoveToPosition)
                {
                    ExecuteTask_MoveToPosition(task as TaskMaster.Task.MoveToPosition);
                    return;
                }
                
                // Clean up garbage
                if (task is TaskMaster.Task.GarbageCleanup)
                {
                    ExecuteTask_CleanUpGarbage(task as TaskMaster.Task.GarbageCleanup);
                    return;
                }
                
                // Pick up item and move to slot
                if (task is TaskMaster.Task.TakeItemToItemSlot)
                {
                    ExecuteTask_TakeItemToItemSlot(task as TaskMaster.Task.TakeItemToItemSlot);
                    return;
                }
                
                // Other task types go here
            }
        }

        #region Execute Tasks

        private void ExecuteTask_MoveToPosition(TaskMaster.Task.MoveToPosition task)
        {
            thought.SetThought(UnitThought.ThoughtState.Moving);
            workerMover.SetMovePosition(task.targetPosition, () =>
            {
                state = State.WaitingForNextTask;
            });
        }

        private void ExecuteTask_CleanUpGarbage(TaskMaster.Task.GarbageCleanup cleanupTask)
        {
            thought.SetThought(UnitThought.ThoughtState.Cleaning);
            workerMover.SetMovePosition(cleanupTask.targetPosition, () =>
            {
                cleanupTask.cleanUpAction();
                state = State.WaitingForNextTask;
            });
        }

        private void ExecuteTask_TakeItemToItemSlot(TaskMaster.Task.TakeItemToItemSlot task)
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

        #endregion
        
    }
}