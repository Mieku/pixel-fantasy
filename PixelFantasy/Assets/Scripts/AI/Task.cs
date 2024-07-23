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
        public Dictionary<string, string> TaskData = new Dictionary<string, string>();
        public List<SkillRequirement> SkillRequirements;
        [JsonIgnore] public Dictionary<string, DateTime> FailedLog = new Dictionary<string, DateTime>();

        public bool IsCanceled { get; private set; } // Track if the task is canceled
        private const int TASK_RETRY_DELAY_SECONDS = 5;
        
        [JsonIgnore] private TaskSettings Settings => GameSettings.Instance.LoadTaskSettings(TaskID);

        public Task()
        {
        }

        public Task(string taskID, PlayerInteractable requester, List<SkillRequirement> skillRequirements = null)
        {
            TaskID = taskID;
            UniqueID = CreateUniqueID(Settings.name);
            Type = Settings.Type;
            SkillRequirements = skillRequirements;
            TaskData = new Dictionary<string, string>() { { "RequesterUID", requester.UniqueID } };
            Status = ETaskStatus.Pending;
            FailedLog = new Dictionary<string, DateTime>();
            //Context = new TaskContext();
            //IsCanceled = false;
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
        
        public void Cancel()
        {
            IsCanceled = true;
            Status = ETaskStatus.Canceled;
            TasksDatabase.Instance.RemoveTask(this);
        }
        
        private string CreateUniqueID(string prefix)
        {
            return $"{prefix}_{Guid.NewGuid()}";
        }
        
        public void SaveBlackboardState(IBlackboard blackboard)
        {
            foreach (var variable in blackboard.GetVariables())
            {
                TaskData[variable.name] = variable.value.ToString();
            }
        }

        public void LoadBlackboardState(IBlackboard blackboard)
        {
            foreach (var kvp in TaskData)
            {
                blackboard.SetVariableValue(kvp.Key, kvp.Value);
            }
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
