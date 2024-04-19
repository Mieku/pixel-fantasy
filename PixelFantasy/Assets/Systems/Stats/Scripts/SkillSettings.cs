using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Systems.Stats.Scripts
{
    [CreateAssetMenu(fileName = "SkillSettings", menuName = "Skill System/Skill Settings")]
    public abstract class SkillSettings : ScriptableObject
    {
        public abstract ESkillType SkillType { get; }
        
        [TableList(AlwaysExpanded = true), SerializeReference]
        public List<SkillLevelData> Table = new List<SkillLevelData>();

        [Button("Initialize Tables")]
        protected virtual void InitializeTables()
        {
            Debug.Log("Initializing tables...");
            Table.Clear();
            for (int i = 0; i < 10; i++)
            {
                Table.Add(CreateTableData(i + 1));
            }
        }

        protected abstract SkillLevelData CreateTableData(int level);

        public abstract float GetValueForLevel(EAttributeType attributeType, int level);

        private void OnValidate()
        {
            if (Table.Count != 10)
            {
                InitializeTables();
            }
        }
    }

    [Serializable]
    public abstract class SkillLevelData
    {
        [ReadOnly, DisplayAsString, TableColumnWidth(60, Resizable = false)]
        public int Level;

        public abstract float GetAttribute(EAttributeType attributeType);
    }

    public enum ESkillType
    {
        Mining, 
        Cooking,
        Melee,
        Ranged,
        Construction,
        Botany,
        Crafting,
        BeastMastery,
        Medical,
        Social,
        Intelligence,
    }
}