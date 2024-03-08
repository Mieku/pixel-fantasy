using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaskSystem
{
    [Serializable]
    public class TaskQueue
    {
        [SerializeField] private List<Task> _tasks = new List<Task>();

        public ETaskType TaskType;

        public TaskQueue(ETaskType taskType)
        {
            TaskType = taskType;
            _tasks = new List<Task>();
        }
        
        public void AddTask(Task task)
        {
            _tasks.Add(task);
        }

        public int Count => _tasks.Count;
        
        public Task NextTask
        {
            get
            {
                if (_tasks.Count > 0)
                {
                    var result = _tasks[0];
                    _tasks.RemoveAt(0);
                    return result;
                }

                return null;
            }
        }

        public Task PeekTask(int index)
        {
            return _tasks[index];
        }

        public Task GetTask(int index)
        {
            var task = _tasks[index];
            _tasks.RemoveAt(index);
            return task;
        }

        public void CancelTask(string taskID, PlayerInteractable requestor)
        {
            Task target = null;
            foreach (var task in _tasks)
            {
                if (task.TaskId == taskID && task.Requestor == requestor)
                {
                    target = task;
                    break;
                }
            }

            if (target != null)
            {
                _tasks.Remove(target);
            }
        }

        public void CancelRequestorTasks(PlayerInteractable requestor)
        {
            List<Task> tasksToCancel = new List<Task>();
            
            foreach (var task in _tasks)
            {
                if (task.Requestor == requestor)
                {
                    tasksToCancel.Add(task);
                }
            }

            foreach (var task in tasksToCancel)
            {
                GameEvents.Trigger_OnTaskCancelled(task.TaskId, requestor);
                task.Cancel();
            }
        }
    }
}
