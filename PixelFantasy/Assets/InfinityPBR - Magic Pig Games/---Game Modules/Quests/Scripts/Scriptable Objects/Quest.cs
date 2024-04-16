using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static InfinityPBR.Modules.Utilities;

/*
 * NOTES:
 * - Will need to make a GameQuestObject -- stuff about failed, complete, etc will live there, not here.  This is the
 * Scriptable object!!
 *
 * - Start with the basic quest types, and then add from tehre. Dont' go too complex too fast!
 */

namespace InfinityPBR.Modules
{
    [HelpURL("https://infinitypbr.gitbook.io/infinity-pbr/game-modules/quests")]
    [CreateAssetMenu(fileName = "Quest", menuName = "Game Modules/Quest/Quest", order = 1)]
    [Serializable]
    public class Quest : ModulesScriptableObject, IAffectStats
    {
        public List<QuestStep> questSteps = new List<QuestStep>();

        public bool subscribeToBlackboard = true; // if true, the GameQuest object of this quest will subscribe to blackboard when it is created
        public bool sequentialSteps = true; // If true, each step must be completed in order. If false, all can be completed out of order.
        public bool hidden; // Intended to mark some quests as hidden. Up to developer on how to handle that.
        public bool autoSucceed = true; // If true, the Tick() method will check for completion
        public bool autoFail = true; // If true, the Tick() method will check for failure
        public bool queryEveryFrame; // If true, the QuestSteps will check for success or failure every frame.
        public bool hasEndTime; // If true, the quest will auto expire
        public bool succeedOnExpiration; // If true, when the quest expires, it will succeed
        public bool failOnExpiration; // If true, when the quest expires, it will fail
        public float timeLimit; // Time limit, meant to be "in game minutes" if you're using Gametime module.
        
        // Rewards for completing the entire quest (Each step can also have rewards)
        public List<QuestReward> successRewards = new List<QuestReward>();
        public List<QuestReward> failureRewards = new List<QuestReward>();
        
        [HideInInspector] public int toolbarIndex;
        [HideInInspector] public bool showQuestMain = true;
        [HideInInspector] public bool showQuestSteps = true;
        [HideInInspector] public int menubarIndex;

        // Properties
        public int Steps => questSteps.Count; // Number of steps in this quest
        public bool HasFailConditions => FailConditionsCount() > 0;
        public bool HasSuccessConditions => SuccessConditionsCount() > 0;

        public int FailConditionsCount() => questSteps.Sum(step => step.failureConditions.Count);
        public int SuccessConditionsCount() => questSteps.Sum(step => step.successConditions.Count);

        // FROM ITEM OBJECT
        public string FullName => GetFullName();
        private string GetFullName() => objectName; // In this context, the objectName IS the full name.

        public List<ItemAttribute> itemAttributes = new List<ItemAttribute>(); // These are ItemAttributes active on this object
        public List<ItemAttribute> availableItemAttributes = new List<ItemAttribute>(); // Item attributes which can be on this object
        public List<string> availableItemAttributeObjectTypes = new List<string>(); // Types of Item Attributes which can be on this object
        public ModificationLevel modificationLevel = new ModificationLevel(); // The modification level for this object
        
        public ModificationLevel modificationLevelInProgress = new ModificationLevel(); // The modification level for this object
        public ModificationLevel modificationLevelFailed = new ModificationLevel(); // The modification level for this object
        public ModificationLevel modificationLevelSucceeded = new ModificationLevel(); // The modification level for this object

        // INVENTORY MODULE / IFitInInventory
        public int GetSpotY() => inventorySpotY;
        public int GetSpotX() => inventorySpotX;
        public int GetInventoryWidth() => inventoryWidth;
        public int GetInventoryHeight() => inventoryHeight;
        public GameObject GetWorldPrefab() => prefabWorld;
        public GameObject GetInventoryPrefab() => prefabInventory;
        
        public int inventorySpotY;
        public int inventorySpotX;
        public int inventoryHeight;
        public int inventoryWidth;
        public GameObject prefabWorld;
        public GameObject prefabInventory;
        
        // Editor/Inspector
        [HideInInspector] public bool showItemAttributes;
        [HideInInspector] public bool showInventory;
        [HideInInspector] public bool showItemAttributeTypes;
        [HideInInspector] public bool showItemAttributesAttributes;
        [HideInInspector] public bool showLootItems;
        [HideInInspector] public bool showDictionaries;
        [HideInInspector] public bool showStats;
        [HideInInspector] public bool showQuest;
        [HideInInspector] public bool hasBeenSetup;
        [HideInInspector] public int questStatsState = 2;

        // -------------------------------------------------------------------------------------
        // PUBLIC METHODS
        // -------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the Modification Level on this as a List<StatsModificationLevel>
        /// </summary>
        /// <returns></returns>
        public List<ModificationLevel> GetModificationLevels()
        {
            List<ModificationLevel> levels = new List<ModificationLevel>();
            levels.Add(modificationLevel);

            return levels;
        }

        // Returns the Modification Level object attached to this Item Object
        public ModificationLevel GetModificationLevel() => modificationLevel;
        
        // Unused in this context, required by IAffectStats
        public void SetOwner(IHaveStats newOwner) { }
        public IHaveStats GetOwner() => default;
        public void SetAffectedStatsDirty(object obj)
        {
            throw new NotImplementedException();
        }

        // These will return true if this Item Object can use the specific Item Attribute provided
        public bool CanUseAttribute(ItemAttribute itemAttribute) => availableItemAttributes.Contains(itemAttribute);
        public bool CanUseAttribute(string itemAttributeUid) => availableItemAttributes.FirstOrDefault(x => x.Uid() == itemAttributeUid);
        
        // Will return true if ALL of the provided Item Attribute uids can be used.
        public bool CanUseAttributes(string[] itemAttributeUids)
        {
            foreach (string uid in itemAttributeUids)
                if (!CanUseAttribute(uid)) return false;

            return true;
        }
        
        // Returns true if the Item Object can use attributes of this type. It does not check if the individual attribute
        // provided can be used, just the type that the Item Attribute is part of.
        public bool CanUseAttributeType(string type) => availableItemAttributeObjectTypes.Contains(type);
        public bool CanUseAttributeType(ItemAttribute itemAttribute) => CanUseAttributeType(itemAttribute.objectType);

        // -------------------------------------------------------------------------------------
        // EDITOR / INSPECTOR
        // -------------------------------------------------------------------------------------

        /// <summary>
        /// Returns a List<Stat> of objects that are directly affected by the Stat on this object
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
        /// Returns a List<Stat> of objects that are directly affected by the provided stat on this object
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
        public ItemObject Clone() => JsonUtility.FromJson<ItemObject>(JsonUtility.ToJson(this));

        public void SetSpots(int spotY, int spotX)
        {
            inventorySpotY = spotY;
            inventorySpotX = spotX;
        }
        
        public void ClearMissingAttributes() => availableItemAttributes.RemoveAll(x => x == null); // Removes any empty Item Attribute entries
    }
}