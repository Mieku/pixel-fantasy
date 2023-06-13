using System;
using System.Collections.Generic;
using Characters;
using Managers;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace TaskSystem
{
    [Serializable]
    public class Task
    {
        public string TaskId;
        public Interactable Requestor;
        public string Payload;
        public Family Owner;
        public List<CraftingBill.RequestedItemInfo> Materials;
        public Queue<Task> SubTasks = new Queue<Task>();
        public Action<Task> OnTaskComplete;
        public TaskType TaskType;

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
            
            return new Task()
            {
                TaskId = this.TaskId,
                Requestor = this.Requestor,
                Payload = this.Payload,
                Owner = this.Owner,
                TaskType = this.TaskType,
                SubTasks = subTasks,
            };
        }
    }
}
