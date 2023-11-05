using System;
using System.Collections.Generic;
using Managers;

namespace TaskSystem
{
    [Serializable]
    public class Task
    {
        public string TaskId;
        public PlayerInteractable Requestor;
        public string Payload;
        public Family Owner;
        public List<CraftingBill.RequestedItemInfo> Materials;
        public Queue<Task> SubTasks = new Queue<Task>();
        public Action<Task> OnTaskComplete;
        public TaskType TaskType;

        public Task(string taskID, PlayerInteractable requestor)
        {
            TaskId = taskID;
            Requestor = requestor;

            if (Requestor != null)
            {
                Requestor.RegisterTask(this);
            }
        }

        public bool IsEqual(Task otherTask)
        {
            return TaskId == otherTask.TaskId
                   && Requestor == otherTask.Requestor
                   && Payload == otherTask.Payload
                   && Owner == otherTask.Owner
                   && TaskType == otherTask.TaskType;
        }

        public void Cancel()
        {
            TaskManager.Instance.CancelTask(this);
            foreach (var subTask in SubTasks)
            {
                TaskManager.Instance.CancelTask(subTask);
            }
        }

        public void Enqueue()
        {
            TaskManager.Instance.AddTask(this);
        }

        public Task Clone()
        {
            Queue<Task> subTasks = new Queue<Task>();
            foreach (var subTask in SubTasks)
            {
                subTasks.Enqueue(subTask);
            }
            
            return new Task(this.TaskId, this.Requestor)
            {
                TaskType =  this.TaskType,
                Payload = this.Payload,
                Owner = this.Owner,
                SubTasks = subTasks,
            };
        }
    }
}
