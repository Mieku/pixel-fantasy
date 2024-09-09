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
        private HashSet<string> _checkedTaskIDs = new HashSet<string>();
        private Task _cachedTask;

        public Task CurrentTask
        {
            get
            {
                if (_cachedTask == null || _kinlingData.CurrentTaskID != _cachedTask.TaskID)
                {
                    _cachedTask = TasksDatabase.Instance.QueryTask(_kinlingData.CurrentTaskID);
                }
                return _cachedTask;
            }
        }

        private const float WAIT_TIMER_MAX = 0.05f; // 50ms
        private const float IDLE_TIME = 10f;

        private void Awake()
        {
            FindAllTaskTrees();
        }

        public string GetCurrentTaskDisplay()
        {
            return CurrentTask?.GetDisplayName() ?? "Idle";
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
            
            // Check for any enqueued tasks first
            if (_kinlingData.EnqueuedTaskUIDs.Count > 0)
            {
                ExecuteNextEnqueuedTask();
            }
            else
            {
                _kinlingData.WaitingTimer -= TimeManager.Instance.DeltaTime;
                if (_kinlingData.WaitingTimer <= 0)
                {
                    _kinlingData.WaitingTimer = WAIT_TIMER_MAX;
                    RequestNextTask();
                }
            }
        }

        private void ExecuteNextEnqueuedTask()
        {
            var enqueuedTaskID = _kinlingData.EnqueuedTaskUIDs[0];
            _kinlingData.EnqueuedTaskUIDs.RemoveAt(0);

            var task = TasksDatabase.Instance.QueryTask(enqueuedTaskID);
            ExecuteTask(task);
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
                case ScheduleOption.Anything:
                    RequestNextWorkTask();
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
            var nextTask = TasksDatabase.Instance.RequestTask(_kinlingData, _checkedTaskIDs);
            if (nextTask != null)
            {
                ExecuteTask(nextTask);
            }
        }

        private void RequestNextRecreationTask()
        {
            // Implementation for recreation tasks
        }

        private void RequestSleepTask()
        {
            // Implementation for sleep tasks
        }

        private void RequestIdleTask()
        {
            // Implementation for idle tasks
        }

        public void StopTask()
        {
            if (CurrentTask != null)
            {
                var tree = FindTaskTreeFor(CurrentTask);
                tree?.BTOwner.StopBehaviour();
            }
        }

        private void ExecuteTask(Task task)
        {
            task.ClaimTask(_kinlingData);
            
            if (CurrentTask != null)
            {
                // If they are in the middle of a task, enqueue it
                task.Status = ETaskStatus.Queued;
                _kinlingData.EnqueuedTaskUIDs.Add(task.UniqueID);
            }
            else
            {
                task.Status = ETaskStatus.InProgress;
                _kinlingData.CurrentTaskID = task.UniqueID;
                var tree = FindTaskTreeFor(task);
                tree?.ExecuteTask(task.TaskData, OnFinished);
            }
        }

        public void CancelTask(Task task)
        {
            if (task != null)
            {
                if (CurrentTask.UniqueID == task.UniqueID)
                {
                    var tree = FindTaskTreeFor(task);
                    tree?.BTOwner.StopBehaviour(false);
                    task?.UnClaimTask(_kinlingData);
                }
                else if (_kinlingData.EnqueuedTaskUIDs.Contains(task.UniqueID))
                {
                    Debug.Log($"Removed enqueued task: {task.UniqueID}");
                    _kinlingData.EnqueuedTaskUIDs.Remove(task.UniqueID);
                }
            }
        }

        public void AssignSpecificTask(Task task)
        {
            if (task == null) return;
            
            ExecuteTask(task);
        }

        public void LoadCurrentTask(Task task)
        {
            var tree = FindTaskTreeFor(task);
            if (tree != null)
            {
                tree.Blackboard.Deserialize(task.BlackboardJSON, null);
                tree.ExecuteTask(task.TaskData, OnFinished);
                tree.BTOwner.Tick(); // Ticks after loading the task, so it is in a safe state
            }
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
            if (DataPersistenceManager.WorldIsClearing) return;

            if (CurrentTask != null)
            {
                var curTask = CurrentTask;

                if (!success)
                {
                    if (curTask.Status != ETaskStatus.Canceled)
                    {
                        curTask.LogFailedAttempt(_kinlingData.UniqueID);
                        curTask.Status = ETaskStatus.Pending;
                        curTask.UnClaimTask(_kinlingData);
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

        public bool HasTreeForTask(string taskID)
        {
            if (string.IsNullOrEmpty(taskID)) return false;
            
            return _taskTrees.Any(t => t.TaskID == taskID);
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
