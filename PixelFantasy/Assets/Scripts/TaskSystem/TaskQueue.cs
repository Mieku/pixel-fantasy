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

        public void CancelTask(Task taskToCancel)
        {
            Task target = null;
            foreach (var task in _tasks)
            {
                if (task.IsEqual(taskToCancel))
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

    // public enum TaskType
    // {
    //     Haul,
    //     Mine,
    //     Construction,
    //     Harvest,
    //     Animals,
    //     Art,
    //     Craft,
    //     Hunt,
    //     Grow,
    //     Cook,
    //     Heal,
    //     Research,
    //     Clean,
    //     Emergency
    // }
}
