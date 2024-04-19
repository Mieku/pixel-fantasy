using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Systems.Stats.Scripts
{
    [CreateAssetMenu(fileName = "Experience Settings", menuName = "Skill System/Experience Settings")]
    public class ExperienceSettings : ScriptableObject
    {
        [field: SerializeField] public int BaseExpPerWork { get; private set; }
        
        
        [field: SerializeField] public float NoPassionExpMod { get; private set; }
        [field: SerializeField] public float MinorPassionExpMod { get; private set; }
        [field: SerializeField, Tooltip("100 Format")] public int ChanceForMinorPassion { get; private set; }
        [field: SerializeField] public float MajorPassionExpMod { get; private set; }
        [field: SerializeField, Tooltip("100 Format")] public int ChanceForMajorPassion { get; private set; }
        
        
        [TableList(AlwaysExpanded = true), SerializeReference]
        public List<ExpLevelData> Table = new List<ExpLevelData>();

        public int GetLevelForTotalExp(float totalExp)
        {
            for (int i = Table.Count - 1; i > 0; i--)
            {
                var minExp = Table[i].MinExp;
                if (totalExp >= minExp)
                {
                    return Table[i].Level;
                }
            }
            
            Debug.LogError($"Unknown level for total exp: {totalExp}");
            return 1;
        }
        
        public int GetMinExpForLevel(int level)
        {
            var expData = GetExpDataForLevel(level);
            return expData.MinExp;
        }

        public int GetDailyDecayRateForLevel(int level)
        {
            var expData = GetExpDataForLevel(level);
            return expData.DailyExpDecayRate;
        }

        private ExpLevelData GetExpDataForLevel(int level)
        {
            if (level > 0 && level <= Table.Count)
            {
                return Table[level];
            }
            Debug.LogError($"Level {level} is out of range for experience");
            return Table[0];
        }
        
        [Button("Initialize Table")]
        private void InitializeTable()
        {
            Debug.Log("Initializing tables..."); 
            Table.Clear();
            for (int i = 0; i <= 10; i++)
            {
                Table.Add(CreateTableData(i));
            }
        }
        
        private ExpLevelData CreateTableData(int level)
        {
            return new ExpLevelData(level);
        }
    }

    [Serializable]
    public class ExpLevelData
    {
        [Sirenix.OdinInspector.ReadOnly, DisplayAsString, TableColumnWidth(60, Resizable = false)]
        public int Level;

        [Sirenix.OdinInspector.ReadOnly, DisplayAsString, TableColumnWidth(100, Resizable = false)]
        public string Name;
        
        [TableColumnWidth(100)]
        public int MinExp;
        
        [TableColumnWidth(100)]
        public int DailyExpDecayRate;

        public ExpLevelData(int level)
        {
            Level = level;
            Name = DetermineLevelName(level);
        }

        private string DetermineLevelName(int level)
        {
            return Level switch
            {
                0 => "Incapable",
                1 => "Novice",
                2 => "Apprentice",
                3 => "Amateur",
                4 => "Proficient",
                5 => "Adept",
                6 => "Expert",
                7 => "Master",
                8 => "Sage",
                9 => "Virtuoso",
                10 => "Legendary",
                _ => "Unknown"
            };
        }
    }
}
