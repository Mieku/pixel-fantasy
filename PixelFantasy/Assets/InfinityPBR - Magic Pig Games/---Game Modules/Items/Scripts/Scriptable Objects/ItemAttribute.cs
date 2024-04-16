using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static InfinityPBR.Modules.Utilities;

/*
 * ITEM ATTRIBUTES
 *
 * Item Attributes are things that describe something. Could be attached to Item Objects, or your own custom objects
 * in your project. These can affect StatsAndSkills, and should be included whenever computing final stats for any
 * player or other "actor" in your game.
 */

namespace InfinityPBR.Modules
{
    [HelpURL("https://infinitypbr.gitbook.io/infinity-pbr/game-modules/items")]
    [CreateAssetMenu(fileName = "Items", menuName = "Game Modules/Items/Item Attribute", order = 1)]
    [Serializable]
    public class ItemAttribute : ModulesScriptableObject
    {
        public int nameOrder;
        public string humanName = "";
        public bool distinct;
        public bool replaceOthers = true;
        public bool affectsActor = true;
        public ModificationLevel modificationLevel;

        public List<RequiredItemAttribute> requiredAttributes = new List<RequiredItemAttribute>();
        public List<ItemAttribute> incompatibleAttributes = new List<ItemAttribute>();
        
        // Editor/Inspector
        [HideInInspector] public bool showDictionaries;
        [HideInInspector] public bool showStats;
        [HideInInspector] public bool showSettings;
        [HideInInspector] public bool hasBeenSetup;
        [HideInInspector] public int menubarIndex;

        /// <summary>
        /// Gets the Modification Level on this as a List<StatsAndSkillsModificationLevel>
        /// </summary>
        /// <returns></returns>
        public List<ModificationLevel> GetModificationLevels()
        {
            List<ModificationLevel> levels = new List<ModificationLevel>();
            levels.Add(modificationLevel);

            return levels;
        }

        /// <summary>
        /// Returns a List<StatsAndSkills> of objects that are directly affected by the StatsAndSkills on this object
        /// </summary>
        /// <param name="stat"></param>
        /// <returns></returns>
        public List<Stat> DirectlyAffectsList(Stat stat)
        {
            var tempList = new List<Stat>();
            foreach (StatModification mod in modificationLevel.modifications)
            {
                if (mod.HasNoEffect()) continue;
                if (mod.isBase || mod.isPerSkillPoint) continue;
                if (mod.source == stat || mod.multiplierUid == stat.Uid() && mod.sourceCalculationStyle == 4)
                {
                    if (tempList.Contains(mod.target)) continue;
                    tempList.Add(mod.target);
                }
            }

            return tempList;
        }
        
        /// <summary>
        /// Returns a List<StatsAndSkills> of objects that are directly affected by the provided statsAndSkills on this object
        /// </summary>
        /// <param name="stat"></param>
        /// <returns></returns>
        public List<Stat> DirectlyAffectedByList(Stat stat)
        {
            var tempList = new List<Stat>();
            if (modificationLevel.targets.Contains(stat))
            {
                foreach (StatModification mod in modificationLevel.modifications)
                {
                    if (mod.HasNoEffect()) continue;
                    if (mod.source == null) continue;
                    if (mod.target != stat) continue;
                    
                    if (!tempList.Contains(mod.source))
                        tempList.Add(mod.source);
                    
                    if (mod.sourceCalculationStyle != 4) continue;
                    
                    if (!tempList.Contains(GetStatByUid(mod.multiplierUid)))
                        tempList.Add(GetStatByUid(mod.multiplierUid));
                }
            }

            return tempList;
        }
        
        /// <summary>
        /// Returns a clone of this object
        /// </summary>
        /// <returns></returns>
        public ItemAttribute Clone()
        {
            return JsonUtility.FromJson<ItemAttribute>(JsonUtility.ToJson(this));
        }
        
        public void CopyRequisiteAttributes(ItemAttribute copyItemAttribute)
        {
            if (copyItemAttribute == this) return;
            
            requiredAttributes.Clear();
            incompatibleAttributes.Clear();

            foreach (var attribute in copyItemAttribute.requiredAttributes.Where(attribute => attribute.itemAttribute != this))
                requiredAttributes.Add(attribute);

            foreach (var attribute in copyItemAttribute.incompatibleAttributes.Where(attribute => attribute != this))
                incompatibleAttributes.Add(attribute);
        }
    }
}