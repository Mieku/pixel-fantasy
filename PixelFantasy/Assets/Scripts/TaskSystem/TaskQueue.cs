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

        private TaskType _taskType;
        private AbilityType _abilityType;

        public TaskQueue(TaskType taskType, AbilityType abilityType)
        {
            _taskType = taskType;
            _abilityType = abilityType;
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

    public enum TaskType
    {
        Haul,
        Mine,
        Construction,
        Harvest,
        Animals,
        Art,
        Craft,
        Hunt,
        Grow,
        Cook,
        Heal,
        Research,
        Clean,
        Emergency
    }
}
