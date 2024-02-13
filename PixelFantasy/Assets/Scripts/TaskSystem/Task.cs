using System;
using System.Collections.Generic;
using Items;
using Managers;
using ScriptableObjects;
using Systems.Skills.Scripts;
using UnityEngine;

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
        public Action OnTaskCancel;
        public EToolType RequiredToolType;
        public SkillType SkillType;
        public bool IsEmergancy;
        public bool IsKinlingSpecific; // Kinling was selected and the command was assigned via right click

        public Task(string taskID, PlayerInteractable requestor, JobData job, EToolType requiredToolType, SkillType skillType, bool isEmergancy = false)
        {
            TaskId = taskID;
            Requestor = requestor;
            Job = job;
            IsEmergancy = isEmergancy;
            RequiredToolType = requiredToolType;
            SkillType = skillType;
        }

        public bool IsEqual(Task otherTask)
        {
            return TaskId == otherTask.TaskId
                   && Requestor == otherTask.Requestor
                   && Payload == otherTask.Payload
                   && IsEmergancy == otherTask.IsEmergancy
                   && RequiredToolType == otherTask.RequiredToolType
                   && SkillType == otherTask.SkillType
                   && IsKinlingSpecific == otherTask.IsKinlingSpecific;
        }

        public void Cancel()
        {
            if (!IsKinlingSpecific)
            {
                TaskManager.Instance.CancelTask(TaskId, Requestor);
                foreach (var subTask in SubTasks)
                {
                    TaskManager.Instance.CancelTask(subTask.TaskId, subTask.Requestor);
                }
            }
            
            OnTaskCancel?.Invoke();
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
            
            return new Task(this.TaskId, this.Requestor, this.Job, this.RequiredToolType, this.SkillType, this.IsEmergancy)
            {
                Payload = this.Payload,
                SubTasks = subTasks,
                IsKinlingSpecific = this.IsKinlingSpecific,
                RequiredToolType = this.RequiredToolType,
                SkillType = this.SkillType,
            };
        }
    }
}
