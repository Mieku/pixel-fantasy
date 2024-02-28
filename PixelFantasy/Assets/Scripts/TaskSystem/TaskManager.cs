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

        public Task RequestTask(Kinling kinling)
        {
            // Get the next job, check if the kinling can do it.
            // if not, get the next one until either a task is found or hit the end of available tasks, if so return null

            var job = kinling.Job;
            var queue = GetTaskQueue(job);
            if (queue == null)
            {
                return null;
            }

            for (int i = 0; i < queue.Count; i++)
            {
                var potentialTask = queue.PeekTask(i);
                if (potentialTask == null)
                {
                    return null;
                }
            
                var taskAction = kinling.TaskAI.FindTaskActionFor(potentialTask);
                if (taskAction.CanDoTask(potentialTask))
                {
                    var task = queue.GetTask(i);
                    return task;
                }
            }

            return null;
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

        public void CancelTask(string taskID, PlayerInteractable requestor)
        {
            GameEvents.Trigger_OnTaskCancelled(taskID, requestor);
            foreach (var queue in AllTasks)
            {
                queue.CancelTask(taskID, requestor);
            }
        }

        public void CancelRequestorTasks(PlayerInteractable requestor)
        {
            foreach (var queue in AllTasks)
            {
                queue.CancelRequestorTasks(requestor);
            }
        }
    }
}
