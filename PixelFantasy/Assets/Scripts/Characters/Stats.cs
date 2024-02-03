using System;
using System.Collections.Generic;
using Items;
using ParadoxNotion.Design;
using ScriptableObjects;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Characters
{
    public class Stats : MonoBehaviour
    {
        public Stat Strength;
        public Stat Vitality;
        public Stat Intelligence;
        public Stat Expertise;

        private int TotalStrengthValue
        {
            get
            {
                int total = Strength.Level + GetTotalModifiersByStatType(StatType.Strength);
                total = Mathf.Clamp(total, 1, 20);
                return total;
            }
        }
        
        private int TotalVitalityValue
        {
            get
            {
                int total = Vitality.Level + GetTotalModifiersByStatType(StatType.Vitality);
                total = Mathf.Clamp(total, 1, 20);
                return total;
            }
        }
        
        private int TotalIntelligenceValue
        {
            get
            {
                int total = Intelligence.Level + GetTotalModifiersByStatType(StatType.Intelligence);
                total = Mathf.Clamp(total, 1, 20);
                return total;
            }
        }
        
        private int TotalExpertiseValue
        {
            get
            {
                int total = Expertise.Level + GetTotalModifiersByStatType(StatType.Expertise);
                total = Mathf.Clamp(total, 1, 20);
                return total;
            }
        }

        [ShowInInspector] private Dictionary<GearState, List<StatModifier>> _allModifiers =
            new Dictionary<GearState, List<StatModifier>>();

        [ShowInInspector] private List<ToolData> _allTools = new List<ToolData>();
        
        private const float DEFAULT_WORK_AMOUNT = 1f;
        private const float DEFAULT_ACTION_SPEED = 1f;
        private const float DEFAULT_GATHER_SPEED = 1f;
        private const float DEFAULT_KNOWLEDGE_SPEED = 1f;
        private const float DEFAULT_CRAFTING_SPEED = 1f;

        private const int MIN_NATURAL_STAT = 7;
        private const int MAX_NATURAL_STAT = 14;

        public void Init(StatsData data)
        {
            Strength = new Stat(data.GetStatByType(StatType.Strength).Level);
            Vitality = new Stat(data.GetStatByType(StatType.Vitality).Level);
            Intelligence = new Stat(data.GetStatByType(StatType.Intelligence).Level);
            Expertise = new Stat(data.GetStatByType(StatType.Expertise).Level);
        }

        public int GetTotalModifiersByStatType(StatType statType)
        {
            int result = 0;
            foreach (var kvp in _allModifiers)
            {
                foreach (var modifier in kvp.Value)
                {
                    if (modifier.StatType == statType)
                    {
                        result += modifier.Modifier;
                    }
                }
            }

            return result;
        }
        
        private void AddTool(ToolData toolData)
        {
            if (toolData == null)
            {
                Debug.LogError($"Attempted to add a tool that doesnt have ToolData");
                return;
            }

            if (_allTools.Contains(toolData))
            {
                Debug.LogError($"Attempted to add the same tool data twice");
                return;
            }
            
            _allTools.Add(toolData);
        }
        
        private void RemoveTool(ToolData toolData)
        {
            if (toolData == null)
            {
                Debug.LogError($"Attempted to remove a tool that doesnt have ToolData");
                return;
            }

            if (_allTools.Contains(toolData))
            {
                _allTools.Remove(toolData);
            }
            else
            {
                Debug.LogError($"Attempted to remove an unregistered tool data");
            }
        }

        public void ApplyStatModifiers(GearState gearState)
        {
            if (gearState == null)
            {
                Debug.LogError($"Attempted to add stat modifiers for a null gear");
                return;
            }

            var toolData = gearState.GearData as ToolData;
            if (toolData != null)
            {
                AddTool(toolData);
            }
            
            var modifiersToAdd = gearState.GearData.StatModifiers;
            if (modifiersToAdd != null)
            {
                if (_allModifiers.ContainsKey(gearState))
                {
                    Debug.LogError($"Attempted to add already registered Modifiers for: {gearState.GearData.ItemName}");
                    return;
                }
                else
                {
                    _allModifiers.Add(gearState, modifiersToAdd);
                }
            }
        }

        public void RemoveStatModifiers(GearState gearState)
        {
            if (gearState == null)
            {
                Debug.LogError($"Attempted to remove stat modifiers for a null gear");
                return;
            }
            
            var toolData = gearState.GearData as ToolData;
            if (toolData != null)
            {
                RemoveTool(toolData);
            }
            
            var modifiersToRemove = gearState.GearData.StatModifiers;
            if (modifiersToRemove != null)
            {
                if (_allModifiers.ContainsKey(gearState))
                {
                    _allModifiers.Remove(gearState);
                }
                else
                {
                    Debug.LogError($"Attempted to remove unregistered stat modifiers");
                }
            }
        }
        
        public Stat GetStatByType(StatType type)
        {
            switch (type)
            {
                case StatType.Strength:
                    return Strength;
                case StatType.Vitality:
                    return Vitality;
                case StatType.Intelligence:
                    return Intelligence;
                case StatType.Expertise:
                    return Expertise;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        [Button("Randomize Scores")]
        public void RandomizeScores()
        {
            Strength.Level = Random.Range(MIN_NATURAL_STAT, MAX_NATURAL_STAT);
            Vitality.Level = Random.Range(MIN_NATURAL_STAT, MAX_NATURAL_STAT);
            Intelligence.Level = Random.Range(MIN_NATURAL_STAT, MAX_NATURAL_STAT);
            Expertise.Level = Random.Range(MIN_NATURAL_STAT, MAX_NATURAL_STAT);
        }

        private float GetSpeedByStatLevel(int level, float defaultValue)
        {
            if (level <= 10)
            {
                return (-0.1f * level + 2.1f) * defaultValue;
            }
            else
            {
                return (-0.075f * level + 1.75f) * defaultValue;
            }
        }
        
        public float GatheringSpeed => GetSpeedByStatLevel(TotalStrengthValue, DEFAULT_GATHER_SPEED);
        public float KnowledgeSpeed => GetSpeedByStatLevel(TotalIntelligenceValue, DEFAULT_KNOWLEDGE_SPEED);
        public float CraftingSpeed => GetSpeedByStatLevel(TotalExpertiseValue, DEFAULT_CRAFTING_SPEED);
        
        public float GetWorkAmount(EToolType toolType)
        {
            if (toolType == EToolType.None)
            {
                return DEFAULT_WORK_AMOUNT;
            }

            float result = 0;
            foreach (var tool in _allTools)
            {
                if (tool.ToolType == toolType)
                {
                    result += tool.WorkValue;
                }
            }

            if(result <= 0)
            {
                Debug.LogError($"Attempted to return 0 work for tooltype: {toolType.GetDescription()}");
                return DEFAULT_WORK_AMOUNT;
            }
            else
            {
                return result;
            }
        }


        public float GetActionSpeed(StatType statType)
        {
            switch (statType)
            {
                case StatType.None:
                    return DEFAULT_ACTION_SPEED;
                case StatType.Strength:
                    return GatheringSpeed;
                case StatType.Vitality:
                    return DEFAULT_ACTION_SPEED;
                case StatType.Intelligence:
                    return KnowledgeSpeed;
                case StatType.Expertise:
                    return CraftingSpeed;
                default:
                    throw new ArgumentOutOfRangeException(nameof(statType), statType, null);
            }
        }
    }
    
    [Serializable]
    public class Stat
    {
        public int Level;

        public Stat(int level)
        {
            Level = level;
        }
    }

    public enum StatType
    {
        [Description("None")] None = 0,
        [Description("Strength")] Strength = 1,
        [Description("Vitality")] Vitality = 2,
        [Description("Intelligence")] Intelligence = 3,
        [Description("Expertise")]  Expertise = 4,
    }

    [Serializable]
    public class StatModifier
    {
        public StatType StatType;
        public int Modifier;

        public string ModifierString
        {
            get
            {
                if (Modifier < 0)
                {
                    return $"-{Modifier}";
                }

                if (Modifier > 0)
                {
                    return $"+{Modifier}";
                }
                
                return $"{Modifier}";
            }
        }

        public string StatName => StatType.GetDescription();
    }

    [Serializable]
    public class StatsData
    {
        public Stat Strength;
        public Stat Vitality;
        public Stat Intelligence;
        public Stat Expertise;
        
        private const int MIN_NATURAL_STAT = 7;
        private const int MAX_NATURAL_STAT = 14;
        
        public StatsData(StatsData motherStats, StatsData fatherStats)
        {
            Strength = new Stat(GetGeneticStatValue(StatType.Strength, motherStats, fatherStats));
            Vitality = new Stat(GetGeneticStatValue(StatType.Vitality, motherStats, fatherStats));
            Intelligence = new Stat(GetGeneticStatValue(StatType.Intelligence, motherStats, fatherStats));
            Expertise = new Stat(GetGeneticStatValue(StatType.Expertise, motherStats, fatherStats));
        }
        
        private int GetGeneticStatValue(StatType type, StatsData motherStat, StatsData fatherStat)
        {
            if (Helper.RollDice(50))
            {
                // Inherit
                var level = Helper.RollDice(50) ? motherStat.GetStatByType(type).Level : fatherStat.GetStatByType(type).Level;
                return level;
            }
            else
            {
                // Not Inherited
                var statLvl = Random.Range(2, 8);
                return statLvl;
            }
        }
        
        public Stat GetStatByType(StatType type)
        {
            switch (type)
            {
                case StatType.Strength:
                    return Strength;
                case StatType.Vitality:
                    return Vitality;
                case StatType.Intelligence:
                    return Intelligence;
                case StatType.Expertise:
                    return Expertise;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
        
        [Button("Randomize Scores")]
        public void RandomizeScores()
        {
            Strength.Level = Random.Range(MIN_NATURAL_STAT, MAX_NATURAL_STAT);
            Vitality.Level = Random.Range(MIN_NATURAL_STAT, MAX_NATURAL_STAT);
            Intelligence.Level = Random.Range(MIN_NATURAL_STAT, MAX_NATURAL_STAT);
            Expertise.Level = Random.Range(MIN_NATURAL_STAT, MAX_NATURAL_STAT);
        }
    }
}
