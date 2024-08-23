using System;
using System.Collections.Generic;

namespace AI
{
    [Serializable]
    public class TaskQueue
    {
        public ETaskType TaskType;
        public List<Task> QueuedTasks;
        
        public TaskQueue(ETaskType taskType)
        {
            TaskType = taskType;
            QueuedTasks = new List<Task>();
        }
        
        public void AddTask(Task task)
        {
            QueuedTasks.Add(task);
        }
        
        public Task GetTask(int index)
        {
            var task = QueuedTasks[index];
            QueuedTasks.RemoveAt(index);
            return task;
        }
        
        public Task QueryTask(string taskID)
        {
            return QueuedTasks.Find(t => t.UniqueID == taskID);
        }

        public void ClearTasks()
        {
            QueuedTasks.Clear();
        }
    }
}