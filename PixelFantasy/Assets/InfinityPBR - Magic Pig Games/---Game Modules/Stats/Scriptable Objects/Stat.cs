using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using static InfinityPBR.Modules.Utilities;
using static InfinityPBR.Modules.GameModuleUtilities;

namespace InfinityPBR.Modules
{
    [HelpURL("https://infinitypbr.gitbook.io/infinity-pbr/game-modules/stats-and-skills")]
    [Serializable]
    [CreateAssetMenu(fileName = "Stat", menuName = "Game Modules/Stat", order = 1)]
    public class Stat : ModulesScriptableObject
    {
        public bool canBeTrained; // Whether this can be trained (And will have mastery levels)
        public bool canBeModified; // Whether this can be modified by others

        public Stat modifyPointsByProficiency; // If populated, when adding points, value added will be modified by this proficiency
        public float points; // Points, intended to be player-defined. Use for "Gold" and "Experience", and assignable stat points
        public float baseValue; // A base value. Similar to points, but intended to be flexible, for use with things like conditions etc.
        public float baseProficiency; // A base proficiency. Same as base value.

        // Blackboard posting
        public bool postPointsToBlackboard;
        public bool postFinalValueToBlackboard;
        public bool postFinalProficiencyToBlackboard;
        public bool postFinalStatToBlackboard;
        public bool notifyPointsToBlackboard;
        public bool notifyFinalValueToBlackboard;
        public bool notifyFinalProficiencyToBlackboard;
        public bool notifyFinalStatToBlackboard;
        
        // Whether or not this stat has min/max values for each aspect
        public bool HasMinPoints => minPointsType > 0;
        public bool HasMaxPoints => maxPointsType > 0;
        public bool HasMinBaseValue => minBaseValueType > 0;
        public bool HasMaxBaseValue => maxBaseValueType > 0;
        public bool HasMinBaseProficiency => minBaseProficiencyType > 0;
        public bool HasMaxBaseProficiency => maxBaseProficiencyType > 0;
        public bool HasMinFinal => minFinalType > 0; // Whether this has a minimum final stat
        public bool HasMaxFinal => maxFinalType > 0; // Whether this has a maximum final stat

        public string[] minMaxOptions = { "None", "float", "Points", "Value", "Proficiency", "Final Stat" };

        public enum MinMaxOptions
        {
            None,
            Float,
            Points,
            Value,
            Proficiency,
            FinalStat
        }
        
        // The type of comparison to make
        // 0 = none, 1 = float, 2 = points, 3 = final value, 4 = final proficiency, 5 = final stat
        public int minPointsType;
        public int maxPointsType;
        public int minBaseValueType;
        public int maxBaseValueType;
        public int minBaseProficiencyType;
        public int maxBaseProficiencyType;
        public int minFinalType;
        public int maxFinalType;

        // These are the numbers input, or the stat selected to compare against
        public float minPoints;
        public float maxPoints;
        public Stat minPointsStat;
        public Stat maxPointsStat;
        public float minBaseValue;
        public float maxBaseValue;
        public Stat minBaseValueStat;
        public Stat maxBaseValueStat;
        public float minBaseProficiency;
        public float maxBaseProficiency;
        public Stat minBaseProficiencyStat;
        public Stat maxBaseProficiencyStat;
        public float minFinal; // The minimum final stat
        public float maxFinal; // the maximum final stat
        public Stat minFinalStat;
        public Stat maxFinalStat;

        // Rounding
        public Rounding roundingMethod = 0;
        public int decimals; // Number of decimal places when we round to decimal places

        // These are the Modification Levels. There will always be at least 1. For isTrainable, will match the masteryLevels.Count
        public List<ModificationLevel> statModificationLevels = new List<ModificationLevel>();
        public int ModificationLevelsCount => statModificationLevels.Count;
        
        [HideInInspector] public MasteryLevels masteryLevels; // The Mastery Levels object we are using on this Stat
        
        // These lists are used in the inspector, and to compute the final list. The computeList is (in order) the order
        // in which other Stat need to be computed before this one can be correctly computed.
        public List<Stat> computeList = new List<Stat>();
        [FormerlySerializedAs("affectedBy")] public List<Stat> allAffectedBy = new List<Stat>();
        [FormerlySerializedAs("affects")] public List<Stat> allAffects = new List<Stat>();
        public List<Stat> directlyAffects = new List<Stat>();
        public List<Stat> directlyAffectedBy = new List<Stat>();

        // Editor/Inspector
        [HideInInspector] public bool showDictionaries;
        [HideInInspector] public bool showMastery;
        [HideInInspector] public bool showMainSettings;
        [HideInInspector] public bool hasBeenSetup;
        [HideInInspector] public int menubarIndex;

        //private ItemObject[] cachedItemObjectArray;
        //private ItemAttribute[] cachedItemAttributeArray;
        //private Stat[] cachedStatsArray;
        //private string _statsArrayType;
        //private Stat[] cachedStatsTypeArray;
        
        public void Cache(bool recompute = true)
        {
            GameModuleObjects<ItemObject>(recompute);
            GameModuleObjects<ItemAttribute>(recompute);
            GameModuleObjects<Stat>(recompute);
            GameModuleObjectsOfType<Stat>(objectType, recompute);
        }

       

        /// <summary>
        /// This will build the final computeList, which determines which Stat, and in which order
        /// will be computed before we can successful compute this final stat
        /// </summary>
        public void BuildComputeList() => computeList = MakeComputeOrder();
        
        /// <summary>
        /// This will set all the connections between this Stat and others, including ItemObjects and
        /// ItemAttributes.
        /// </summary>
        public void BuildTreeConnections()
        {
            EnsureOneModificationLevel(); // Always ensure we have at least one level
            
            // Collect all the Modification Levels
            var allModLevels = GameModuleObjects<Stat>()
                .SelectMany(x => x.statModificationLevels)
                .ToList();

            // Add Modification Levels from Item Objects, Item Attributes
            allModLevels.AddRange(GameModuleObjects<ItemObject>()
                .SelectMany(x => x.GetModificationLevels()));
            allModLevels.AddRange(GameModuleObjects<ItemAttribute>()
                .SelectMany(x => x.GetModificationLevels()));

            // Get a list of all the Modifications on all the levels
            var allModifications = allModLevels
                .SelectMany(x => x.modifications)
                .ToList();

            // Build the lists which show in the inspector. This is useful to help visualize the connections.
            directlyAffectedBy = StatsThisIsDirectlyAffectedBy(allModifications);
            allAffectedBy = StatsThisIsAffectedBy(allModifications);
            directlyAffects = StatsThisDirectlyAffects(allModifications);
            allAffects = StatsThisAffects(allModifications);
        }

        /// <summary Stat="and return the List">
        /// Tries to add a Stat to a List
        /// </summary>
        /// <param name="thisList"></param>
        /// <param name="stat"></param>
        /// <returns></returns>
        private List<Stat> TryToAdd(List<Stat> thisList, Stat stat)
        {
            if (thisList.Contains(stat)) 
                return thisList;
            
            thisList.Add(stat);
            return thisList;
        }

        // Adds any stats that are directly affected by the Stat stat to the tempList, and also runs the same
        // method for each of those children.
        private void AddDirectlyAffectedStats(List<Stat> tempList, Stat stat, List<StatModification> allModifications)
        {
            var childDirectlyAffects = stat.StatsThisDirectlyAffects(allModifications);
            foreach (var statAffected in childDirectlyAffects)
            {
                if (tempList.Contains(statAffected)) continue;
                tempList.Add(statAffected);
                AddDirectlyAffectedStats(tempList, statAffected, allModifications);
            }
        }

        // Gets all the Stat that this affects, directly and indirectly, using the
        // AddDirectlyAffectedStats() recursive call
        private List<Stat> StatsThisAffects(List<StatModification> allModifications)
        {
            var tempList = directlyAffects.ToList();
            foreach (var stat in directlyAffects)
                AddDirectlyAffectedStats(tempList, stat, allModifications);
            return tempList;
        }

        /// <summary>
        /// Gets all the Stat that this directly affects
        /// </summary>
        /// <param name="allModifications"></param>
        /// <returns></returns>
        private List<Stat> StatsThisDirectlyAffects(List<StatModification> allModifications)
        {
            // Add modifications from Item Objects
            var itemObjectList = new List<Stat>();
            itemObjectList = GameModuleObjects<ItemObject>()
                .SelectMany(itemObject => itemObject.DirectlyAffectsList(this))
                .Aggregate(itemObjectList, TryToAdd);

            // Add modifications from Item Attributes
            var itemAttributeList = itemObjectList;
            itemAttributeList = GameModuleObjects<ItemAttribute>()
                .SelectMany(itemAttribute => itemAttribute.DirectlyAffectsList(this))
                .Aggregate(itemAttributeList, TryToAdd);
            
            // Targets of this
            var targetsList = Targets().Aggregate(itemAttributeList, TryToAdd);

            // All the mods
            var finalList = targetsList;
            foreach (var statModification in allModifications // For all modifications
                         .Where(statModification => !statModification.HasNoEffect()) // Where there is an effect
                         .Where(statModification => !statModification.isBase // And the modification is not base
                                                    && !statModification.isPerSkillPoint)) // And it is not per skill point
            {
                // If this stat is the source, try to add it to the final list, and continue
                if (statModification.source == this)
                {
                    finalList = TryToAdd(finalList, statModification.target);
                    continue;
                }
                        
                // If the multiplierUid isn't this Uid(), or the sourceCalculationStyle is not "4", continue
                if (statModification.multiplierUid != Uid() || statModification.sourceCalculationStyle != 4) continue;
                
                // Try to add it to the list
                finalList = TryToAdd(finalList, statModification.target);
            }

            return finalList;
        }

        // Gets all the Stats that is affected by, directly and indirectly, this stat
        private List<Stat> StatsThisIsAffectedBy(List<StatModification> allModifications)
        {
            allAffectedBy.Clear(); // Clear the list to start

            foreach (var stat in directlyAffectedBy)
                AddStatsThisIsAffectedBy(allAffectedBy, stat, allModifications);

            return allAffectedBy;
        }
        
        // Finalized the compute order so that the Stat which affect this are computed in the right order,
        // with those that have nothing affecting them computed first, and moving up the chain until we are able to
        // successfully calculate the final Stat value for this.
        private List<Stat> MakeComputeOrder()
        {
            var newComputeList = new List<Stat>();

            // Create a temporary copy of the affectedBy List
            var affectedByList = allAffectedBy.ToList();

            var breakCounter = 0; // Just in case there's an infinite loop. There shouldn't be!
            var foundNone = false; // We will keep going until we find no more to do
            while (!foundNone)
            {
                foundNone = true; // Set this true -- helps avoid a mistake in the while loop
                foreach (var stat in affectedByList.Where(stat => stat != null))
                {
                    // If everything that this other Stat is directly affected by is already in our new list,
                    // then we will add it ot the new list, and continue.
                    if (stat.directlyAffectedBy.All(x => newComputeList.Contains(x)))
                    {
                        newComputeList = TryToAdd(newComputeList, stat);
                        continue;
                    }

                    foundNone = false; // We must have found some that aren't in the list, so we need to repeat
                }

                breakCounter++;
                if (breakCounter <= 500) continue;
                
                Debug.Log("<color=#ff0000>Break! This should not have happened</color> ");
                break;
                // ...just in case...
            }
            
            return newComputeList;
        }
        
        private void AddAffectedStatsToComputeOrder(List<Stat> addToList, Stat stat)
        {
            // Add the stat
            TryToAdd(addToList, stat);
            
            // If all of this stats children are in the list, we are done
            if (stat.directlyAffectedBy.All(addToList.Contains))
                return;

            // Add all the children
            foreach (var affectedBy in stat.directlyAffectedBy)
                AddAffectedStatsToComputeOrder(addToList, affectedBy);
        }

        private void AddStatsThisIsAffectedBy(List<Stat> allStatsAffectedBy, Stat stat, List<StatModification> allModifications)
        {
            if (stat == null || allModifications == null) return;
            
            // Add the stat
            TryToAdd(allStatsAffectedBy, stat);
            
            // If all of this stats children are in the list, we are done
            if (stat.StatsThisIsDirectlyAffectedBy(allModifications).All(allStatsAffectedBy.Contains))
                return;

            // Add all the children
            foreach (var affectingStat in stat.StatsThisIsDirectlyAffectedBy(allModifications))
                AddStatsThisIsAffectedBy(allStatsAffectedBy, affectingStat, allModifications);
        }

        /// <summary>
        /// Gets all the Stat that is directly affected by
        /// </summary>
        /// <param name="allModifications"></param>
        /// <returns></returns>
        private List<Stat> StatsThisIsDirectlyAffectedBy(List<StatModification> allModifications)
        {
            // Add modifications from Item Objects
            var itemObjectList = new List<Stat>();
            itemObjectList = GameModuleObjects<ItemObject>()
                .SelectMany(itemObject => itemObject.DirectlyAffectedByList(this))
                .Aggregate(itemObjectList, TryToAdd);
            
            // Add modifications from Item Objects
            var itemAttributeList = itemObjectList;
            itemAttributeList = GameModuleObjects<ItemAttribute>()
                .SelectMany(itemAttribute => itemAttribute.DirectlyAffectedByList(this))
                .Aggregate(itemAttributeList, TryToAdd);

            // All other Stat
            var otherStatList = itemAttributeList;
            otherStatList = GameModuleObjects<Stat>()
                .Where(stat => stat.Targets().Contains(this))
                .Aggregate(otherStatList, TryToAdd);

            var finalList = otherStatList;
            
            // All the mods
            foreach (var modification in allModifications)
            {
                if (modification.HasNoEffect() 
                    || modification.target != this 
                    || modification.source == null) continue;
                
                finalList = TryToAdd(finalList, modification.source);
               
                if (modification.sourceCalculationStyle != 4) continue;
                finalList = TryToAdd(finalList, GetStatByUid(modification.multiplierUid));
            }

            return finalList;
        }

        // This will look for another Stat of this objectType which has masteryLevels, and if so, assigns
        // that to this.
        private void TryGetMasteryLevels()
        {
            var found = GameModuleObjectsOfType<Stat>(objectType)
                .FirstOrDefault(x => x.masteryLevels != null);

            if (found == null) 
                return;
            
            masteryLevels = found.masteryLevels;
        }
        
        // This will ensure we have Modification Levels to match the Mastery Levels count. This will not delete any
        // existing data, so could be more convenient than simply replacing what we have with a brand new set.
        public void CheckMasteryCount()
        {
            if (!canBeTrained) return; // If it can't be trained, then it doesn't have mastery levels
            
            // If it doesn't have mastery levels, then try to match the levels of another Stat of it's type,
            // and return regardless of that outcome.
            if (!masteryLevels)
            {
                TryGetMasteryLevels();
                return;
            }

            var masteryLevelsCount = masteryLevels.levels.Count;
            
            // Add or remove based on the difference
            var difference = statModificationLevels.Count - masteryLevelsCount;
            switch (difference)
            {
                case > 0:
                    statModificationLevels.RemoveRange(statModificationLevels.Count - difference, difference);
                    break;
                case < 0:
                    statModificationLevels.AddRange(new List<ModificationLevel>(new ModificationLevel[Mathf.Abs(difference)]));
                    break;
            }

            for (var i = 0; i < statModificationLevels.Count; i++)
            {
                if (statModificationLevels?[i]?.masteryLevel == null) continue;
                statModificationLevels[i].masteryLevel = masteryLevels.levels[i].name;
            }
        }
        
        public Stat Clone() => JsonUtility.FromJson<Stat>(JsonUtility.ToJson(this));

        /// <summary>
        /// Returns the active Mastery Level
        /// Oct 22 2021 -- not sure if this is used or needed?
        /// </summary>
        /// <returns></returns>
        public MasteryLevel ActiveMasteryLevel() => masteryLevels.levels.FirstOrDefault(x => x.showThis);

        /// <summary>
        /// Returns a list of all the sources from the Modification levels. Does not include modifiers used in
        /// calculations.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Stat> Sources()
        {
            var sources = new List<Stat>();
            foreach (ModificationLevel modLevel in statModificationLevels)
                sources.AddRange(modLevel.sources);

            return sources
                .Where(x => x != null)
                .Distinct();
        }
        
        /// <summary>
        /// Returns a list of all targets from the Modification Levels.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Stat> Targets()
        {
            var targets = new List<Stat>();
            foreach (var modificationLevel in statModificationLevels)
                targets.AddRange(modificationLevel.targets);

            return targets
                .Where(x => x != null)
                .Distinct();
        }

        /// <summary>
        /// Removes any missing references to Stat that have been deleted
        /// </summary>
        public void RemoveMissingStats()
        {
            foreach (var modificationLevel in statModificationLevels)
                modificationLevel?.RemoveMissingStats();
        }

        /// <summary>
        /// Ensures that we always have at least one Modification Level
        /// </summary>
        public void EnsureOneModificationLevel()
        {
            if (statModificationLevels.Count == 0)
                statModificationLevels.Add(new ModificationLevel());
            if (canBeTrained) return;
            
            // If we can't be trained, remove any extra levels, as there will only be one.
            for (var i = statModificationLevels.Count - 1; i >= 1; i--)
                statModificationLevels.RemoveAt(i);
        }
    }
}
