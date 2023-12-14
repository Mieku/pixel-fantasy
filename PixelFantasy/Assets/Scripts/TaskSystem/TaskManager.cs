using System;
using System.Collections.Generic;
using Buildings;
using Characters;
using Items;
using Managers;
using ScriptableObjects;
using UnityEngine;

namespace TaskSystem
{
    public class TaskManager : Singleton<TaskManager>
    {
        public List<TaskQueue> AllTasks = new List<TaskQueue>();

        private TaskQueue GetTaskQueue(JobData job)
        {
            var queue = AllTasks.Find(q => q.Job == job);
            return queue;
        }
        
        public Task GetTask(JobData job)
        {
            var queue = GetTaskQueue(job);
            if (queue == null) return null;
            
            return queue.NextTask;
        }
        
        public void AddTask(Task task)
        {
            var queue = GetTaskQueue(task.Job);
            if (queue == null)
            {
                queue = new TaskQueue(task.Job);
                AllTasks.Add(queue);
            }

            queue.AddTask(task);
        }
        
        public void CancelTask(Task task)
        {
            GameEvents.Trigger_OnTaskCancelled(task);
            
            var queue = GetTaskQueue(task.Job);
            if (queue != null)
            {
                queue.CancelTask(task);
            }
        }
    }
}
