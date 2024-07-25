using System;
using System.Collections.Generic;
using AI.Action_Tasks;
using Characters;
using Newtonsoft.Json;
using NodeCanvas.BehaviourTrees;
using NodeCanvas.Framework;
using ScriptableObjects;
using Sirenix.OdinInspector;
using Systems.Stats.Scripts;
using TaskSystem;
using UnityEngine;

namespace AI
{
    [Serializable]
    public class Task
    {
        public string UniqueID;
        public string TaskID;
        public ETaskType Type;
        public ETaskStatus Status;
        public string BlackboardJSON;
        public Dictionary<string, string> TaskData = new Dictionary<string, string>();
        public List<SkillRequirement> SkillRequirements;
        [JsonIgnore] public Dictionary<string, DateTime> FailedLog = new Dictionary<string, DateTime>();
        
        [JsonIgnore] public Action OnCancelledCallback;
        [JsonIgnore] public Action<Task, bool> OnCompletedCallback;
        
        // For serialization of callbacks
        public string CancelledCallbackState { get; set; }
        public string CompletedCallbackState { get; set; }
        
        private const int TASK_RETRY_DELAY_SECONDS = 5;

        public Task()
        {
        }

        public Task(string taskID, ETaskType taskType, PlayerInteractable requester, Dictionary<string, string> taskData = null, List<SkillRequirement> skillRequirements = null)
        {
            taskData ??= new Dictionary<string, string>();
            
            TaskID = taskID;
            UniqueID = CreateUniqueID(taskID);
            Type = taskType;
            SkillRequirements = skillRequirements;
            TaskData = taskData;
            TaskData.Add("RequesterUID", requester.UniqueID);
            Status = ETaskStatus.Pending;
            FailedLog = new Dictionary<string, DateTime>();
        }

        public void OnSave()
        {
            // Convert the state of the actions to a serializable form
            CancelledCallbackState = OnCancelledCallback?.Method.Name;
            CompletedCallbackState = OnCompletedCallback?.Method.Name;
        }

        public void OnLoad()
        {
            // Convert the serialized state back to actions
            if (CancelledCallbackState != null)
            {
                OnCancelledCallback = GetActionByName(CancelledCallbackState);
            }

            if (CompletedCallbackState != null)
            {
                OnCompletedCallback = GetActionByName<Task, bool>(CompletedCallbackState);
            }
        }
        
        private Action GetActionByName(string methodName)
        {
            // Implement a way to get the method by name
            // This is just an example and may need adjustment
            return (Action)Delegate.CreateDelegate(typeof(Action), this, methodName);
        }

        private Action<Task, bool> GetActionByName<T1, T2>(string methodName)
        {
            // Implement a way to get the method by name
            // This is just an example and may need adjustment
            return (Action<Task, bool>)Delegate.CreateDelegate(typeof(Action<Task, bool>), this, methodName);
        }
        
        public bool CanBeRetriedByKinling(string kinlingID)
        {
            if (FailedLog.TryGetValue(kinlingID, out var lastFailedTime))
            {
                return (DateTime.UtcNow - lastFailedTime).TotalSeconds >= TASK_RETRY_DELAY_SECONDS;
            }
            return true;
        }

        public void LogFailedAttempt(string kinlingID)
        {
            FailedLog[kinlingID] = DateTime.UtcNow;
        }

        public bool AreSkillsValid(StatsData statsData)
        {
            if(SkillRequirements == null) return true;

            bool skillsPass = statsData.CheckSkillRequirements(SkillRequirements);
            return skillsPass;
        }
        
        public void Cancel(bool shouldRequeue)
        {
            Debug.Log($"Cancelling Task: {TaskID} : {UniqueID}");

            Status = ETaskStatus.Canceled;
            
            OnCancelledCallback?.Invoke();
            
            // if (!shouldRequeue)
            // {
            //     Status = ETaskStatus.Canceled;
            //     TasksDatabase.Instance.RemoveTask(this);
            // }
            TasksDatabase.Instance.RemoveTask(this);
        }

        public void TaskComplete(bool success)
        {
            OnCompletedCallback?.Invoke(this, success);
        }
        
        private string CreateUniqueID(string prefix)
        {
            return $"{prefix}_{Guid.NewGuid()}";
        }
    }

    public struct SkillRequirement
    {
        public ESkillType SkillType;
        public int MinSkillLevel;
    }
    
    public enum ETaskStatus { Pending, InProgress, Completed, Canceled }
    
    // public enum ETaskType
    // {
    //     Emergency = 0,
    //     Healing = 1,
    //     Construction = 2,
    //     AnimalHandling = 3,
    //     Cooking = 4,
    //     Hunting = 5,
    //     Farming = 6,
    //     Mining = 7,
    //     Harvesting = 8,
    //     Forestry = 9,
    //     Crafting = 10,
    //     Hauling = 11,
    //     Research = 12,
    //     
    //     Personal = 20,
    //     Misc = 100
    // }
}
