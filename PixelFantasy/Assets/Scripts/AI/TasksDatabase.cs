using System.Collections.Generic;
using System.Linq;
using Characters;
using Managers;
using TaskSystem;

namespace AI
{
    public class TasksDatabase : Singleton<TasksDatabase>
    {
        public List<TaskQueue> TaskQueues = new List<TaskQueue>();

        public TaskCreator TaskCreator;

        private TaskQueue GetTaskQueue(ETaskType taskType)
        {
            return TaskQueues.Find(q => q.TaskType == taskType);
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
                if (queue.QueuedTasks.Count == 0)
                {
                    TaskQueues.Remove(queue);
                }
            }
        }

        public Task QueryTask(string taskId)
        {
            if(string.IsNullOrEmpty(taskId)) return null;
            
            return TaskQueues.SelectMany(q => q.QueuedTasks).FirstOrDefault(task => task.UniqueID == taskId);
        }

        public List<Task> GetAllTasks()
        {
            return TaskQueues.SelectMany(queue => queue.QueuedTasks).ToList();
        }

        public bool AllTasksChecked(HashSet<string> checkedTaskIDs)
        {
            return TaskQueues.SelectMany(queue => queue.QueuedTasks).All(task => task.Status != ETaskStatus.Pending || checkedTaskIDs.Contains(task.UniqueID));
        }

        public List<TaskQueue> SaveTaskData()
        {
            return TaskQueues;
        }

        public void LoadTasksData(List<TaskQueue> taskQueues)
        {
            TaskQueues = taskQueues;
        }

        public void ClearAllTasks()
        {
            foreach (var queue in TaskQueues)
            {
                queue.ClearTasks();
            }
        }

        public Task RequestTask(KinlingData kinlingData, HashSet<string> checkedTaskIDs)
        {
            List<ETaskType> prioritizedTasks = kinlingData.TaskPriorities.SortedPriorities();
            Task nextTask = null;

            foreach (var taskType in prioritizedTasks)
            {
                var queue = GetTaskQueue(taskType);
                if (queue != null)
                {
                    foreach (var task in queue.QueuedTasks)
                    {
                        if (task.Status == ETaskStatus.Pending && !checkedTaskIDs.Contains(task.UniqueID))
                        {
                            checkedTaskIDs.Add(task.UniqueID);

                            if (kinlingData.Stats.CanDoTaskType(task.Type) && task.AreSkillsValid(kinlingData.Stats))
                            {
                                lock (task)
                                {
                                    if (task.Status == ETaskStatus.Pending)
                                    {
                                        task.ClaimTask(kinlingData);
                                        nextTask = task;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                task.LogFailedAttempt(kinlingData.UniqueID);
                            }
                        }
                    }
                }

                if (nextTask != null)
                    break;
            }

            if (nextTask == null && AllTasksChecked(checkedTaskIDs))
            {
                checkedTaskIDs.Clear();
            }

            return nextTask;
        }

        public void CancelRequesterTasks(PlayerInteractable requester)
        {
            var requesterTasks = TaskQueues.SelectMany(queue => queue.QueuedTasks)
                .Where(t => t.RequesterID == requester.UniqueID).ToList();

            foreach (var task in requesterTasks)
            {
                task.Cancel();
            }
        }
    }
}
