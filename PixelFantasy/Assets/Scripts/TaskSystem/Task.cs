using System;
using System.Collections.Generic;
using Items;
using Managers;
using ScriptableObjects;

namespace TaskSystem
{
    [Serializable]
    public class Task
    {
        public string TaskId;
        public JobData Job;
        public PlayerInteractable Requestor;
        public object Payload;
        public List<Item> Materials;
        public Queue<Task> SubTasks = new Queue<Task>();
        public Action<Task> OnTaskComplete;
        public EToolType RequiredToolType;
        public bool IsEmergancy;

        public Task(string taskID, PlayerInteractable requestor, JobData job, EToolType requiredToolType, bool isEmergancy = false)
        {
            TaskId = taskID;
            Requestor = requestor;
            Job = job;
            IsEmergancy = isEmergancy;
            RequiredToolType = requiredToolType;

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
                   && IsEmergancy == otherTask.IsEmergancy
                   && RequiredToolType == otherTask.RequiredToolType;
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
            
            return new Task(this.TaskId, this.Requestor, this.Job, this.RequiredToolType, this.IsEmergancy)
            {
                Payload = this.Payload,
                SubTasks = subTasks,
            };
        }
    }
}
