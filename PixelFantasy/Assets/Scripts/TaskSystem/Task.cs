using System;
using System.Collections.Generic;
using System.Linq;
using Characters;
using Managers;

namespace TaskSystem
{
    [Serializable]
    public class Task
    {
        public string TaskId;
        public ETaskType TaskType;
        public PlayerInteractable Requestor => _requestor;
        public object Payload;
        public List<ItemData> Materials;
        public Action<Task> OnTaskComplete;
        public Action OnTaskCancel;
        public EToolType RequiredToolType;
        public bool IsKinlingSpecific; // Kinling was selected and the command was assigned via right click

        private PlayerInteractable _requestor;

        public Task(string taskID, ETaskType taskType, PlayerInteractable requestor, EToolType requiredToolType)
        {
            TaskId = taskID;
            TaskType = taskType;
            SetRequestor(requestor);
            RequiredToolType = requiredToolType;
        }

        public void SetRequestor(PlayerInteractable requestor)
        {
            _requestor = requestor;

            if (requestor != null)
            {
                //requestor.AddTaskToRequested(this);
            }
        }

        public bool IsEqual(Task otherTask)
        {
            return TaskId == otherTask.TaskId
                   && Requestor == otherTask.Requestor
                   && Payload == otherTask.Payload
                   && TaskType == otherTask.TaskType
                   && RequiredToolType == otherTask.RequiredToolType
                   && IsKinlingSpecific == otherTask.IsKinlingSpecific;
        }

        public void Cancel()
        {
            // if (!IsKinlingSpecific)
            // {
            //     TaskManager.Instance.CancelTask(TaskId, Requestor);
            // }
            //
            // var kinlingAssignedToTask = KinlingsDatabase.Instance.GetKinlingsDataList().Find(k => k.).Values
            //     .FirstOrDefault(k => k.CurrentTaskAction?.TaskId == TaskId);
            // if (kinlingAssignedToTask != null)
            // {
            //     kinlingAssignedToTask.GetKinling().TaskAI.CancelTask(TaskId);
            // }
            //
            // OnTaskCancel?.Invoke();
        }

        public void Enqueue()
        {
            TaskManager.Instance.AddTask(this);
        }
    }
}
