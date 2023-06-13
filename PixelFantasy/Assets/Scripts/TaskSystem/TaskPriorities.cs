using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TaskSystem
{
    [Serializable]
    public class TaskPriorities
    {
        public List<TaskPriority> AllPriorities = new List<TaskPriority>();
        
        private const int MAX_PRIORITY = 5;

        public TaskPriorities()
        {
            AllPriorities.Add(new TaskPriority(TaskType.Emergency));
            AllPriorities.Add(new TaskPriority(TaskType.Heal));
            AllPriorities.Add(new TaskPriority(TaskType.Animals));
            AllPriorities.Add(new TaskPriority(TaskType.Cook));
            AllPriorities.Add(new TaskPriority(TaskType.Hunt));
            AllPriorities.Add(new TaskPriority(TaskType.Construction));
            AllPriorities.Add(new TaskPriority(TaskType.Grow));
            AllPriorities.Add(new TaskPriority(TaskType.Mine));
            AllPriorities.Add(new TaskPriority(TaskType.Harvest));
            AllPriorities.Add(new TaskPriority(TaskType.Craft));
            AllPriorities.Add(new TaskPriority(TaskType.Art));
            AllPriorities.Add(new TaskPriority(TaskType.Haul));
            AllPriorities.Add(new TaskPriority(TaskType.Clean));
            AllPriorities.Add(new TaskPriority(TaskType.Research));
        }

        public List<TaskPriority> SortedPriorities
        {
            get
            {
                List<TaskPriority> result = new List<TaskPriority>();
                for (int p = 1; p < MAX_PRIORITY; p++)
                {
                    foreach (var priority in AllPriorities)
                    {
                        if (priority.PriorityLevel == p && priority.IsEnabled)
                        {
                            result.Add(priority);
                        }
                    }
                }

                return result;
            }
        }
    }

    [Serializable]
    public class TaskPriority
    {
        public TaskType TaskType;
        public int PriorityLevel;
        public bool IsEnabled;

        public TaskPriority(TaskType taskType)
        {
            TaskType = taskType;
            PriorityLevel = 3;
            IsEnabled = true;
        }
    }
}
