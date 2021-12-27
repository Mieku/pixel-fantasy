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
        private TaskSystem taskSystem;
        private UnitThought thought;

        private const float WAIT_TIMER_MAX = .2f;

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
                if (task is TaskSystem.Task.MoveToPosition)
                {
                    ExecuteTask_MoveToPosition(task as TaskSystem.Task.MoveToPosition);
                    return;
                }
                // TODO: Other task types go here
                
            }
        }

        private void ExecuteTask_MoveToPosition(TaskSystem.Task.MoveToPosition task)
        {
            thought.SetThought(UnitThought.ThoughtState.Moving);
            workerMover.SetMovePosition(task.targetPosition, () =>
            {
                state = State.WaitingForNextTask;
            });
        }
    }
}