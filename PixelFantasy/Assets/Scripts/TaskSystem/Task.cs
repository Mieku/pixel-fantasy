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

        public bool IsEqual(Task otherTask)
        {
            return TaskId == otherTask.TaskId && Requestor == otherTask.Requestor && Category == otherTask.Category;
        }

        public void Cancel()
        {
            TaskManager.Instance.CancelTask(this);
        }
    }
}
