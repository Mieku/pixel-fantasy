using System;
using System.Collections.Generic;
using Characters;
using ScriptableObjects;
using UnityEngine;

namespace TaskSystem
{
    [Serializable]
    public class TaskQueue
    {
        [SerializeField] private List<Task> _tasks = new List<Task>();

        public JobData Job;

        public TaskQueue(JobData job)
        {
            Job = job;
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
    }
}
