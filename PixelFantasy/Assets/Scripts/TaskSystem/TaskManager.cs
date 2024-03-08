using System.Collections.Generic;
using Characters;
using Handlers;
using Managers;
using UnityEngine.Serialization;

namespace TaskSystem
{
    public class TaskManager : Singleton<TaskManager>
    {
        public List<TaskQueue> AllTasks = new List<TaskQueue>();
        
        private TaskQueue GetTaskQueue(ETaskType taskType)
        {
            var queue = AllTasks.Find(q => q.TaskType == taskType);
            return queue;
        }

        public Task RequestTask(Kinling kinling, List<ETaskType> priorities)
        {
            // Get the next job, check if the kinling can do it.
            // if not, get the next one until either a task is found or hit the end of available tasks, if so return null
            foreach (var taskType in priorities)
            {
                var queue = GetTaskQueue(taskType);
                if (queue != null)
                {
                    for (int i = 0; i < queue.Count; i++)
                    {
                        var potentialTask = queue.PeekTask(i);
                        if (potentialTask != null)
                        {
                            var taskAction = kinling.TaskAI.FindTaskActionFor(potentialTask);
                            if (taskAction.CanDoTask(potentialTask))
                            {
                                var task = queue.GetTask(i);
                                return task;
                            }
                        }
                    }
                }
            }

            return null;
        }
        
        public void AddTask(Task task)
        {
            var queue = GetTaskQueue(task.TaskType);
            if (queue == null)
            {
                queue = new TaskQueue(task.TaskType);
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
    
    public enum ETaskType
    {
        Emergancy = 0,
        Healing = 1,
        Construction = 2,
        AnimalHandling = 3,
        Cooking = 4,
        Hunting = 5,
        Farming = 6,
        Mining = 7,
        Harvesting = 8,
        Forestry = 9,
        Crafting = 10,
        Hauling = 11,
        
        Personal = 20,
        Misc = 100
    }
}
