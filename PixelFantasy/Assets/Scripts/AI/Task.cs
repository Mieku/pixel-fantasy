using System;
using System.Collections.Generic;
using Characters;
using Managers;
using Newtonsoft.Json;
using Systems.Stats.Scripts;
using UnityEngine;

namespace AI
{
    [Serializable]
    public class Task
    {
        public string UniqueID;
        public string TaskID;
        public string RequesterID;
        public string ClaimedKinlingUID;
        public string DisplayName;
        public ETaskType Type;
        public ETaskStatus Status;
        public string BlackboardJSON;
        public Dictionary<string, object> TaskData = new Dictionary<string, object>();
        public List<SkillRequirement> SkillRequirements;
        [JsonIgnore] public Dictionary<string, DateTime> FailedLog = new Dictionary<string, DateTime>();

        private const int TASK_RETRY_DELAY_SECONDS = 5;

        public Task() { }

        public Task(string taskID, string displayName, ETaskType taskType, PlayerInteractable requester, Dictionary<string, object> taskData = null, List<SkillRequirement> skillRequirements = null)
        {
            taskData ??= new Dictionary<string, object>();

            TaskID = taskID;
            UniqueID = CreateUniqueID(taskID);
            RequesterID = requester.UniqueID;
            DisplayName = displayName;
            Type = taskType;
            SkillRequirements = skillRequirements;
            TaskData = taskData;
            TaskData.Add("RequesterUID", requester.UniqueID);
            Status = ETaskStatus.Pending;
            FailedLog = new Dictionary<string, DateTime>();
        }
        
        public string GetDisplayName()
        {
            return DisplayName;
        }

        public void ClaimTask(KinlingData kinlingData)
        {
            ClaimedKinlingUID = kinlingData.UniqueID;
            TaskData["KinlingUID"] = kinlingData.UniqueID;
        }

        public void UnClaimTask(KinlingData kinlingData)
        {
            if (string.IsNullOrEmpty(ClaimedKinlingUID)) return;
            
            if (ClaimedKinlingUID != kinlingData.UniqueID) Debug.LogError("Tried to unclaim task claimed by someone else");

            ClaimedKinlingUID = null;
            TaskData.Remove("KinlingUID");
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
            if (SkillRequirements == null) return true;

            return statsData.CheckSkillRequirements(SkillRequirements);
        }

        public void Cancel()
        {
            if (!string.IsNullOrEmpty(ClaimedKinlingUID))
            {
                var kinling = KinlingsDatabase.Instance.GetKinling(ClaimedKinlingUID);
                kinling.TaskHandler.CancelTask(this);
            }

            var requester = PlayerInteractableDatabase.Instance.Query(RequesterID);
            requester.OnTaskCancelled(this);
                
            Status = ETaskStatus.Canceled;
            TasksDatabase.Instance.RemoveTask(this);
        }

        public void TaskComplete(bool success)
        {
            var requester = PlayerInteractableDatabase.Instance.Query(RequesterID);
            requester.OnTaskComplete(this, success);
        }

        private string CreateUniqueID(string prefix)
        {
            return $"{prefix}_{Guid.NewGuid()}";
        }
    }

    [Serializable]
    public struct SkillRequirement
    {
        public ESkillType SkillType;
        public int MinSkillLevel;
    }

    public enum ETaskStatus
    {
        Pending, 
        InProgress, 
        Completed, 
        Canceled,
        Queued,
    }
    
    public enum ETaskType
    {
        Emergency = 0,
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
        Cleaning = 13,
        Research = 12,
         
        Personal = 20,
        Misc = 100
    }
}
