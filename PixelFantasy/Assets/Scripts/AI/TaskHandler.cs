using System;
using System.Collections.Generic;
using System.Linq;
using AI.Task_Settings;
using Characters;
using DataPersistence;
using Managers;
using Systems.Appearance.Scripts;
using UnityEngine;

namespace AI
{
    public class TaskHandler : MonoBehaviour
    {
        [SerializeField] private Kinling _kinling;
        
        private KinlingData _kinlingData => _kinling.RuntimeData;
        private List<TaskTree> _taskTrees;
        
        public Task CurrentTask => TasksDatabase.Instance.QueryTask(_kinlingData.CurrentTaskID);
        
        private const float WAIT_TIMER_MAX = 0.05f; // 50ms
        private const float IDLE_TIME = 10f;

        private void Awake()
        {
            FindAllTaskTrees();
        }

        private void FindAllTaskTrees()
        {
            _taskTrees = GetComponentsInChildren<TaskTree>().ToList();
        }

        private TaskTree FindTaskTreeFor(Task task)
        {
            return _taskTrees.Find(t => t.TaskID == task.TaskID);
        }

        private void Update()
        {
            if (!_kinling.HasInitialized) return;
            if (CurrentTask != null) return;
            
            _kinlingData.WaitingTimer -= TimeManager.Instance.DeltaTime;
            if (_kinlingData.WaitingTimer <= 0)
            {
                _kinlingData.WaitingTimer = WAIT_TIMER_MAX;
                RequestNextTask();
            }
        }

        private void RequestNextTask()
        {
            var currentSchedule = _kinlingData.Schedule.GetCurrentScheduleOption();
            switch (currentSchedule)
            {
                case ScheduleOption.Sleep:
                    RequestSleepTask();
                    break;
                case ScheduleOption.Work:
                    RequestNextWorkTask();
                    break;
                case ScheduleOption.Recreation:
                    RequestNextRecreationTask();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (CurrentTask == null)
            {
                RequestIdleTask();
            }
        }

        private void RequestNextWorkTask()
        {
            var nextTask = TasksDatabase.Instance.RequestTask(_kinlingData);
            if (nextTask != null)
            {
                ExecuteTask(nextTask);
            }
        }

        private void RequestNextRecreationTask()
        {
            
        }
        
        private void RequestSleepTask()
        {
            // // If the kinling lacks a bed, claim one if available
            // if (_kinling.RuntimeData.AssignedBed == null)
            // {
            //     var bed = FurnitureDatabase.Instance.FindClosestUnclaimedBed(_kinling);
            //     if (bed != null)
            //     {
            //         bed.AssignKinling(_kinling);
            //     }
            // }
            //
            // if (_kinling.RuntimeData.AssignedBed != null)
            // {
            //     Task goToSleepTask = TasksDatabase.Instance.TaskCreator.CreateGoToBedTask();
            // }
            // else
            // {
            //     Task goToSleepTask = TasksDatabase.Instance.TaskCreator.CreateSleepOnFloorTask();
            // }
        }

        private void RequestIdleTask()
        {
            
        }

        /// <summary>
        /// This is for scene reloading
        /// </summary>
        public void StopTask()
        {
            if (CurrentTask != null)
            {
                var tree = FindTaskTreeFor(CurrentTask);
                tree.BTOwner.StopBehaviour();
            }
        }
        
        private void ExecuteTask(Task task)
        {
            // If there is a task currently running, cancel it and return it
            if (CurrentTask != null)
            {
                CurrentTask.Cancel();
            }
            
            task.ClaimTask(_kinlingData);
            _kinlingData.CurrentTaskID = task.UniqueID;
            
            var tree = FindTaskTreeFor(task);
            
            tree.ExecuteTask(task.TaskData, OnFinished);
        }

        public void CancelTask(Task task)
        {
            if (task != null)
            {
                var tree = FindTaskTreeFor(task);
                tree.BTOwner.StopBehaviour(false);
            }
        }

        public void LoadCurrentTask(Task task)
        {
            var tree = FindTaskTreeFor(task);

            tree.Blackboard.Deserialize(task.BlackboardJSON, null);
            tree.ExecuteTask(task.TaskData, OnFinished);
            tree.BTOwner.Tick(); // Ticks after loading the task, so it is in a safe state
        }

        public void SaveBBState()
        {
            if (CurrentTask != null)
            {
                var tree = FindTaskTreeFor(CurrentTask);

                CurrentTask.BlackboardJSON = tree.Blackboard.Serialize(null);
            }
        }

        private void OnFinished(bool success)
        {
            if(DataPersistenceManager.WorldIsClearing) return;

            if (CurrentTask != null)
            {
                var curTask = CurrentTask;
            
                if (!success)
                {
                    if (curTask.Status != ETaskStatus.Canceled)
                    {
                        curTask.LogFailedAttempt(_kinlingData.UniqueID);
                        curTask.Status = ETaskStatus.Pending;
                    }
                }
                else
                {
                    curTask.Status = ETaskStatus.Completed;
                    TasksDatabase.Instance.RemoveTask(curTask);
                }
            
                curTask.TaskComplete(success);
            }
            
            _kinlingData.CurrentTaskID = null;
        }
        
        public AvatarLayer.EAppearanceDirection GetActionDirection(Vector3 targetPos)
        {
            return DetermineUnitActionDirection(targetPos, transform.position);
        }
        
        private AvatarLayer.EAppearanceDirection DetermineUnitActionDirection(Vector3 workPos, Vector3 standPos)
        {
            const float threshold = 0.25f;

            if (standPos.y >= workPos.y + threshold)
            {
                return AvatarLayer.EAppearanceDirection.Down;
            } 
            else if (standPos.y <= workPos.y - threshold)
            {
                return AvatarLayer.EAppearanceDirection.Up;
            }
            else if (standPos.x >= workPos.x)
            {
                return AvatarLayer.EAppearanceDirection.Left;
            }
            else
            {
                return AvatarLayer.EAppearanceDirection.Right;
            }
        }
    }
}
