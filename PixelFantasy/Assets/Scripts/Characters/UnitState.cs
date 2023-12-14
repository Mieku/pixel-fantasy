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

        public string FullName => FirstName + " " + LastName;
        public string JobName => CurrentJob.JobName;
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
    }
}
