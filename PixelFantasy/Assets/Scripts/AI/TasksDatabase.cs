using System.Collections.Generic;
using Characters;
using Managers;
using Newtonsoft.Json;
using TaskSystem;

namespace AI
{
    public class TasksDatabase : Singleton<TasksDatabase> // TODO: rename to Tasks Manager when the old one is removed
    {
        public List<TaskQueue> TaskQueues = new List<TaskQueue>();
        
        public TaskCreator TaskCreator;
        
        private TaskQueue GetTaskQueue(ETaskType taskType)
        {
            var queue = TaskQueues.Find(q => q.TaskType == taskType);
            return queue;
        }
        
        public void AddTask(Task task)
        {
            var queue = GetTaskQueue(task.Type);
            if (queue == null)
            {
                queue = new TaskQueue(task.Type);
                TaskQueues.Add(queue);
            }

            queue.AddTask(task);
        }
        
        public void RemoveTask(Task task)
        {
            var queue = GetTaskQueue(task.Type);
            if (queue != null)
            {
                queue.QueuedTasks.Remove(task);
                
                // Optionally, remove empty queues
                if (queue.QueuedTasks.Count == 0)
                {
                    TaskQueues.Remove(queue);
                }
            }
        }

        public Task QueryTask(string taskId)
        {
            if (string.IsNullOrEmpty(taskId)) return null;
            
            foreach (var queue in TaskQueues)
            {
                var task = queue.QueryTask(taskId);
                if (task != null)
                {
                    return task;
                }
            }

            return null;
        }
        
        public Task RequestTask(KinlingData kinlingData)
        {
            List<ETaskType> prioritizedTasks = kinlingData.TaskPriorities.SortedPriorities();
            
            // Get the next job, check if the kinling can do it.
            // if not, get the next one until either a task is found or hit the end of available tasks, if so return null
            foreach (var taskType in prioritizedTasks)
            {
                var queue = GetTaskQueue(taskType);
                if (queue != null)
                {
                    foreach (var task in queue.QueuedTasks)
                    {
                        if (task.Status == ETaskStatus.Pending)
                        {
                            if(!task.CanBeRetriedByKinling(kinlingData.UniqueID)) continue;
                            if(!task.AreSkillsValid(kinlingData.Stats)) continue;

                            task.Status = ETaskStatus.InProgress;
                            
                            return task;
                        }
                    }
                }
            }

            return null;
        }
        
        public string GetSerializeTasks()
        {
            string json = JsonConvert.SerializeObject(this);
            return json;
        }
        
        public void DeserializeTasks(string json)
        {
            // JsonConvert.PopulateObject(json, this);
            //
            // // Ensure any necessary initialization
            // foreach (var queue in TaskQueues)
            // {
            //     foreach (var task in queue.QueuedTasks)
            //     {
            //         task.LoadActions(); // Reload the actions from ScriptableObjects
            //     }
            // }
        }
    }
}
