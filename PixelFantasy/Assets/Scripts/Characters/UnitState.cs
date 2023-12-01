using System.Collections.Generic;
using System.Linq;
using Buildings;
using Items;
using Managers;
using ScriptableObjects;
using TaskSystem;
using UnityEngine;
using Zones;

namespace Characters
{
    public class UnitState : MonoBehaviour
    {
        [SerializeField] private UID _uid;
        [SerializeField] private Color _relevantStatColour;
        
        public string FirstName, LastName;
        public Abilities Abilities;
        public TaskPriorities Priorities;
        
        public Schedule Schedule = new Schedule();

        private Building _assignedHome;
        public Building AssignedHome
        {
            get => _assignedHome;
            set
            {
                _assignedHome = value;
                GameEvents.Trigger_OnCoinsIncomeChanged();
            }
        }

        public Building AssignedWorkplace;
        
        private List<JobState> _jobHistory = new List<JobState>();

        public string FullName => FirstName + " " + LastName;
        public string JobName => CurrentJob.JobNameWithTitle;
        public string UID => _uid.uniqueID;

        public int RelevantAbilityScore(List<AbilityType> relevantAbilities)
        {
            int score = 0;
            if (relevantAbilities.Contains(AbilityType.Strength))
            {
                score += Abilities.Strength.Level;
            }
            if (relevantAbilities.Contains(AbilityType.Vitality))
            {
                score += Abilities.Vitality.Level;
            }
            if (relevantAbilities.Contains(AbilityType.Intelligence))
            {
                score += Abilities.Intelligence.Level;
            }
            if (relevantAbilities.Contains(AbilityType.Expertise))
            {
                score += Abilities.Expertise.Level;
            }

            return score;
        }
        
        public string GetAbilityList(List<AbilityType> relevantAbilities, Color relevantColourOverride = default)
        {
            Color relevantColour = _relevantStatColour;
            if (relevantColourOverride != default)
            {
                relevantColour = relevantColourOverride;
            }
            
            int strength = Abilities.Strength.Level;
            int vitality = Abilities.Vitality.Level;
            int intelligence = Abilities.Intelligence.Level;
            int expertise = Abilities.Expertise.Level;

            string result = "";
            // Strength
            if (relevantAbilities.Contains(AbilityType.Strength))
            {
                result += $"<color={Helper.ColorToHex(relevantColour)}>{strength} Strength</color><br>";
            }
            else
            {
                result += $"{strength} Strength<br>";
            }
            
            // Vitality
            if (relevantAbilities.Contains(AbilityType.Vitality))
            {
                result += $"<color={Helper.ColorToHex(relevantColour)}>{vitality} Vitality</color><br>";
            }
            else
            {
                result += $"{vitality} Vitality<br>";
            }
            
            // Intelligence
            if (relevantAbilities.Contains(AbilityType.Intelligence))
            {
                result += $"<color={Helper.ColorToHex(relevantColour)}>{intelligence} Intelligence</color><br>";
            }
            else
            {
                result += $"{intelligence} Intelligence<br>";
            }
            
            // Expertise
            if (relevantAbilities.Contains(AbilityType.Expertise))
            {
                result += $"<color={Helper.ColorToHex(relevantColour)}>{expertise} Expertise</color>";
            }
            else
            {
                result += $"{expertise} Expertise";
            }

            return result;
        }
        
        public void SetLoadData(UnitStateData data)
        {
            FirstName = data.FirstName;
            LastName = data.LastName;
        }

        public UnitStateData GetStateData()
        {
            return new UnitStateData
            {
                FirstName = FirstName,
                LastName = LastName,
            };
        }
        
        public struct UnitStateData
        {
            public string FirstName, LastName;
        }

        public JobState CurrentJob
        {
            get
            {
                foreach (var job in _jobHistory)
                {
                    if (job.IsCurrentJob)
                    {
                        return job;
                    }
                }
                
                // Create a Worker Job State and return it
                var workerData = Librarian.Instance.GetJob("Worker");
                JobState workerJobState = new JobState(workerData, 0, true);
                return workerJobState;
            }
        }

        public void ChangeJob(JobData newJob)
        {
            // Unassign currentJob
            CurrentJob.IsCurrentJob = false;

            // Check if the job already is in history
            foreach (var job in _jobHistory)
            {
                if (job.JobData == newJob)
                {
                    job.IsCurrentJob = true;
                    return;
                }
            }

            if (newJob == null)
            {
                newJob = Librarian.Instance.GetJob("Worker");
            }
            
            // If doesn't already exist, create new state
            JobState newJobState = new JobState(newJob, 0, true);
            if (newJob.JobName != "Worker")
            {
                _jobHistory.Add(newJobState);
            }

            if (newJob.RequiredTool != null)
            {
                var claimedTool = InventoryManager.Instance.ClaimItemGlobal(newJob.RequiredTool);
                if (claimedTool != null)
                {
                    var unit = GetComponent<Unit>();
                    unit.Equipment.AssignDesiredEquipment(claimedTool.State as GearState);
                }
            }
        }

        public List<JobState> JobHistory => _jobHistory;
    }
}
