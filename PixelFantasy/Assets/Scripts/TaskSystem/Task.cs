using System;
using Gods;
using UnityEngine;

namespace TaskSystem
{
    [Serializable]
    public class Task
    {
        public string TaskId;
        public TaskCategory Category;
        public Interactable Requestor;
        public string Payload;

        public bool IsEqual(Task otherTask)
        {
            return TaskId == otherTask.TaskId && Requestor == otherTask.Requestor && Category == otherTask.Category && Payload == otherTask.Payload;
        }

        public void Cancel()
        {
            TaskManager.Instance.CancelTask(this);
        }

        public void Enqueue()
        {
            TaskManager.Instance.AddTask(this);
        }

        public Task Clone()
        {
            return new Task()
            {
                Category = this.Category,
                TaskId = this.TaskId,
                Requestor = this.Requestor,
                Payload = this.Payload,
            };
        }
    }
}
