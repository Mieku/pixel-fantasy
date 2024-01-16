using System.Collections.Generic;
using Buildings;
using Managers;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Serialization;

namespace Characters
{
    public class UnitState : MonoBehaviour
    {
        [SerializeField] private UID _uid;
        [SerializeField] private Color _relevantStatColour;
        
        public string FirstName, LastName;
        [FormerlySerializedAs("Abilities")] public Stats Stats;
        
        public Schedule Schedule = new Schedule();

        private HouseholdBuilding _assignedHome;
        public HouseholdBuilding AssignedHome
        {
            get => _assignedHome;
            set
            {
                _assignedHome = value;
                GameEvents.Trigger_OnCoinsIncomeChanged();
            }
        }

        public Building AssignedWorkplace;

        public string FullName => FirstName + " " + LastName;
        public string JobName => CurrentJob.JobName;
        public string UID => _uid.uniqueID;

        public int RelevantAbilityScore(List<StatType> relevantAbilities)
        {
            int score = 0;
            if (relevantAbilities.Contains(StatType.Strength))
            {
                score += Stats.Strength.Level;
            }
            if (relevantAbilities.Contains(StatType.Vitality))
            {
                score += Stats.Vitality.Level;
            }
            if (relevantAbilities.Contains(StatType.Intelligence))
            {
                score += Stats.Intelligence.Level;
            }
            if (relevantAbilities.Contains(StatType.Expertise))
            {
                score += Stats.Expertise.Level;
            }

            return score;
        }
        
        public string GetAbilityList(List<StatType> relevantAbilities, Color relevantColourOverride = default)
        {
            Color relevantColour = _relevantStatColour;
            if (relevantColourOverride != default)
            {
                relevantColour = relevantColourOverride;
            }
            
            int strength = Stats.Strength.Level;
            int vitality = Stats.Vitality.Level;
            int intelligence = Stats.Intelligence.Level;
            int expertise = Stats.Expertise.Level;

            string result = "";
            // Strength
            if (relevantAbilities.Contains(StatType.Strength))
            {
                result += $"<color={Helper.ColorToHex(relevantColour)}>{strength} Strength</color><br>";
            }
            else
            {
                result += $"{strength} Strength<br>";
            }
            
            // Vitality
            if (relevantAbilities.Contains(StatType.Vitality))
            {
                result += $"<color={Helper.ColorToHex(relevantColour)}>{vitality} Vitality</color><br>";
            }
            else
            {
                result += $"{vitality} Vitality<br>";
            }
            
            // Intelligence
            if (relevantAbilities.Contains(StatType.Intelligence))
            {
                result += $"<color={Helper.ColorToHex(relevantColour)}>{intelligence} Intelligence</color><br>";
            }
            else
            {
                result += $"{intelligence} Intelligence<br>";
            }
            
            // Expertise
            if (relevantAbilities.Contains(StatType.Expertise))
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
        
        public JobData CurrentJob
        {
            get
            {
                if (AssignedWorkplace == null)
                {
                    return Librarian.Instance.GetJob("Worker");
                }
                else
                {
                    return AssignedWorkplace.GetBuildingJob();
                }
            }
        }

        public void AssignHome(HouseholdBuilding home)
        {
            
            
            GameEvents.Trigger_OnCoinsIncomeChanged();
        }
    }
}
