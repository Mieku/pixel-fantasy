using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static InfinityPBR.Modules.GameModuleUtilities;

namespace InfinityPBR.Modules
{
    [Serializable]
    public class ModificationLevel
    {
        public string ownerName;
        public string masteryLevel;
        public List<StatModification> modifications = new List<StatModification>();

        //public List<Stat> multipliers = new List<Stat>();
        public List<Stat> sources = new List<Stat>();
        public List<Stat> targets = new List<Stat>();

        // Editor / Inspector
        [HideInInspector] public Stat[] modifiableStats;
        [HideInInspector] public string[] modifiableStatNames;
        [HideInInspector] public Stat[] availableSources;
        [HideInInspector] public string[] availableSourcesNames;
        [HideInInspector] public Stat[] availableModifiers;
        [HideInInspector] public string[] availableModifiersNames;
        
        [HideInInspector] public int addStatIndex = -1;

        public bool HasTarget(Stat stat) => targets.Contains(stat);
        public bool HasTarget(string statUid) => targets.Any(x => x.Uid() == statUid);

        public bool TryGetStatModification(Stat stat, out StatModification outValue)
        {
            if (TryGetStatModification(stat.Uid(), out var found))
            {
                outValue = found;
                return true;
            }

            outValue = null;
            return false;
        }

        public bool TryGetStatModification(string statUid, out StatModification outValue)
        {
            outValue = modifications.FirstOrDefault(x => x.target.Uid() == statUid);
            return outValue != null;
        }
        
        public void AddModification(Stat newSource, Stat newTarget, float newValue, float newProficiency, bool isBase = false, bool isPerSkillPoint = false)
        {
            if (isBase || isPerSkillPoint)
            {
                modifications.Add(new StatModification(null, newTarget, newValue, newProficiency, 0, isBase, isPerSkillPoint));
                CacheSourcesAndTargets();
                return;
            }

            modifications.Add(new StatModification(newSource, newTarget, newValue, newProficiency));
            CacheSourcesAndTargets();
        }
        
        public (float, float) GetValueAndProficiency(Stat source, Stat target, bool isBase = false, bool isPerSkillPoint = false)
        {
            if (source != null)
            {
                if ((!sources.Contains(source) || !targets.Contains(target)) && (!isBase && !isPerSkillPoint))
                    return (0, 0);
            }

            var mod = GetModification(source, target, isBase, isPerSkillPoint);
            return mod == null ? (0, 0) : (mod.value, mod.proficiency);
        }
        
        public float GetValue(Stat source, Stat target, bool isBase = false, bool isPerSkillPoint = false)
        {
            return GetValueAndProficiency(source, target, isBase, isPerSkillPoint).Item1;
        }
        
        public float GetProficiency(Stat source, Stat target, bool isBase = false, bool isPerSkillPoint = false)
        {
            return GetValueAndProficiency(source, target, isBase, isPerSkillPoint).Item2;
        }
        
        public void SetValueAndProficiency(Stat source, Stat target, float newValue, float newProficiency, bool isBase = false, bool isPerSkillPoint = false)
        {
            StatModification mod = GetModification(source, target, isBase, isPerSkillPoint);
            if (mod != null)
            {
                mod.value = newValue;
                mod.proficiency = newProficiency;
                return;
            }

            AddModification(source, target, newValue, newProficiency, isBase, isPerSkillPoint);
        }

        public void SetValue(Stat source, Stat target, float newValue, bool isBase = false, bool isPerSkillPoint = false)
        {
            StatModification mod = GetModification(source, target, isBase, isPerSkillPoint);
            if (mod != null)
            {
                mod.value = newValue;
                return;
            }
            AddModification(source, target, newValue, 0f, isBase, isPerSkillPoint);
        }

        public void SetProficiency(Stat source, Stat target, float newProficiency, bool isBase = false, bool isPerSkillPoint = false)
        {
            StatModification mod = GetModification(source, target, isBase, isPerSkillPoint);
            if (mod != null)
            {
                mod.proficiency = newProficiency;
                return;
            }
            AddModification(source, target, 0f, newProficiency, isBase, isPerSkillPoint);
        }

        public StatModification GetModification(Stat source, Stat target, bool isBase = false, bool isPerSkillPoint = false)
        {
            foreach (StatModification mod in modifications)
            {
                if (mod.target != target) continue;
                if (mod.source != source) continue;
                if (isBase && !mod.isBase) continue;
                if (isPerSkillPoint && !mod.isPerSkillPoint) continue;
                return mod;
            }

            return null;
        }

        public IEnumerable<Stat> AffectsList()
        {
            return targets;
        }

        public void CacheSourcesAndTargets()
        {
            sources = modifications
                .Where(x => x.isBase == false)
                .Where(x => x.isPerSkillPoint == false)
                .Select(x => x.source)
                .Where(x => x != null)
                .Distinct()
                .ToList();
            
            
            
            targets = modifications
                .Select(x => x.target)
                .Distinct()
                .ToList();

            var modifierSourceUids = modifications
                .Where(x => x.sourceCalculationStyle == 4)
                .Select(x => x.multiplierUid)
                .Distinct()
                .ToList();

            /*
            multipliers.Clear();
            foreach (string uid in modifierSourceUids)
            {
                multipliers.Add(GetStatsArray().Where(x => x.uid == uid).ToList()[0]);
            }*/

            /*
            foreach (StatsAndSkills statsAndSkills in multipliers)
            {
                statsAndSkills.AffectedByList();
            }*/
        }
        
        public ModificationLevel Clone() => JsonUtility.FromJson<ModificationLevel>(JsonUtility.ToJson(this));

        public void CacheModifiableStats(Stat thisObject = null, bool recompute = true)
        {
            modifiableStats = ModifiableStats(recompute) // Start with all StatsAndSkills
                //.Except(sources) // Exclude all sources we already have
                .Except(targets) // Exclude all targets we already have
                .OrderBy(x => x.objectType)
                .ThenBy(x => x.objectName)
                .ToArray();

            if (thisObject != null)
            {
                modifiableStats = ModifiableStats()
                    .Except(new List<Stat> {thisObject}) // Exclude this object
                    .Except(thisObject.allAffectedBy) // Exclude others that this is affected by
                    .ToArray();
            }
            
            modifiableStatNames = modifiableStats
                .Select(x => $"[{x.objectType}] {x.objectName}")
                .ToArray();

            CacheSourcesAndTargets();
        }

        public void CacheSourceStats(Stat[] thisStatsAndSkillsArray, Stat thisObject = null)
        {
            availableSources = thisStatsAndSkillsArray // Start with all StatsAndSkills
                //.Except(sources) // Exclude all sources we already have // Oct 20 2021 - I think we can have multiple sources, so they can have different effects
                //.Except(targets) // Exclude all targets we already have // Oct 20 2021 - I think we can have multiple sources, so they can have different effects, will need to block it in the editor
                .OrderBy(x => x.objectName)
                .ToArray();

            if (thisObject != null)
            {
                availableSources = availableSources
                    //.Except(new List<StatsAndSkills> {thisObject}) // Exclude this object // Oct 20 2021 - I think we can include this object...shouldn't have a circular logic impact
                    .Except(thisObject.allAffectedBy) // Exclude others that this is affected by
                    .ToArray();
            }
            
            availableSourcesNames = availableSources
                .Select(x => x.objectName)
                .ToArray();
            
            // MODIFIERS
            
            availableModifiers = thisStatsAndSkillsArray // Start with all StatsAndSkills
                //.Except(targets) // Exclude all targets we already have
                .ToArray();

            if (thisObject != null)
            {
                availableModifiers = availableModifiers
                    .Except(new List<Stat> {thisObject}) // Exclude this object
                    .ToArray();
            }
            
            availableModifiersNames = availableModifiers
                .Select(x => x.objectName)
                .ToArray();
        }

        public void RemoveTarget(Stat target)
        {
            modifications.RemoveAll(x => x.target == target);
            CacheSourcesAndTargets();
        }

        public void RemoveSource(Stat source)
        {
            modifications.RemoveAll(x => x.source == source);
            CacheSourcesAndTargets();
        }

        public bool TryAddModification(StatModification mod, Stat thisObject = null)
        {
            if (CanAddModification(mod, thisObject))
            {
                AddModification(mod.source, mod.target, mod.value, mod.proficiency, mod.isBase, mod.isPerSkillPoint);
                return true;
            }

            return false;
        }

        private bool CanAddModification(StatModification mod, Stat thisObject = null)
        {
            //if (mod.source == thisObject && !mod.isBase && !mod.isPerSkillPoint) return false;
            if (mod.source != null && mod.source == thisObject) return false;
            if (mod.target == thisObject) return false;

            return true;
        }

        /// <summary>
        /// Removes missing StatsAndSkills (perhaps the user deleted one?)
        /// </summary>
        public void RemoveMissingStats()
        {
            sources.RemoveAll(x => x == null);
            targets.RemoveAll(x => x == null);
            for (var i = modifications.Count - 1; i >= 0; i--)
            {
                if (modifications[i].target == null)
                {
                    modifications.RemoveAt(i);
                    continue;
                }
                if (modifications[i].isBase) continue;
                if (modifications[i].isPerSkillPoint) continue;
                if (modifications[i].source != null) continue;
                
                modifications.RemoveAt(i);
            }
        }

        /// <summary>
        /// Returns the effects on the provided target
        /// </summary>
        /// <param name="targetUid"></param>
        /// <param name="points"></param>
        /// <param name="proficiency"></param>
        /// <param name="owner"></param>
        /// <param name="targetOwner"></param>
        /// <param name="debug"></param>
        /// <returns></returns>
        public (float, float) GetEffectOn(string targetUid, float points = 0f, float proficiency = 0f, IHaveStats owner = null)
        {
            var valueAdd = 0f;
            var proficiencyAdd = 0f;
            
            foreach (var statModification in modifications)
            {
                var targetName = GameModuleRepository.Instance.Get<Stat>(targetUid).objectName;
                var statTarget = GameModuleRepository.Instance.Get<Stat>(statModification.targetUid).objectName;
                var sourceName = statModification.source == null ? "null" : statModification.source.objectName;
                
                
                if (statModification.HasNoEffect())
                {
                    continue;
                }
                if (statModification.targetUid != targetUid)
                {
                    continue;
                }
                
                //if (targetName == "Might" && targetName == statTarget)
                //    Debug.Log($"--- Getting effect on {targetName} (StatTarget is {statTarget} by {sourceName}) value is {statModification.value} prof is {statModification.proficiency}");

                var modEffect = statModification.GetEffectOn(points, owner, (targetName == "Might" && targetName == statTarget));
                //if (targetName == "Might" && targetName == statTarget)
                //    Debug.Log($"--- modEffect is {modEffect}");
                valueAdd += modEffect.Item1;
                proficiencyAdd += modEffect.Item2;
            }
            
            valueAdd *= 1 + proficiency;
            proficiencyAdd *= 1 + proficiency;
            
            return (valueAdd, proficiencyAdd);
        }

        public int GetComputationIndex(Stat source)
        {
            foreach (StatModification mod in modifications)
            {
                if (mod.source != source) continue;
                return mod.sourceCalculationStyle;
            }

            return 0;
        }
        
        public int GetComputationIndex(Stat source, Stat target)
        {
            foreach (StatModification mod in modifications)
            {
                if (mod.source != source) continue;
                if (mod.target != target) continue;
                return mod.sourceCalculationStyle;
            }

            return 0;
        }
        
        public void SetComputationIndex(Stat source, int newIndex)
        {
            foreach (StatModification mod in modifications)
            {
                if (mod.source != source) continue;
                mod.sourceCalculationStyle = newIndex;
            }
        }
        
        public void SetComputationIndex(Stat source, Stat target, int newIndex)
        {
            foreach (StatModification mod in modifications)
            {
                if (mod.source != source) continue;
                if (mod.target != target) continue;
                mod.sourceCalculationStyle = newIndex;
            }
        }
        
        public string GetLookupTableMultiplier(Stat source)
        {
            foreach (StatModification mod in modifications)
            {
                if (mod.source != source) continue;
                return mod.multiplierUid;
            }

            return "";
        }
        
        public string GetLookupTableMultiplier(Stat source, Stat target)
        {
            foreach (StatModification mod in modifications)
            {
                if (mod.source != source) continue;
                if (mod.target != target) continue;
                return mod.multiplierUid;
            }

            return "";
        }
        
        public void SetLookupTableMultiplier(Stat source, string multiplierUid)
        {
            foreach (StatModification mod in modifications)
            {
                if (mod.source != source) continue;
                mod.multiplierUid = multiplierUid;
            }
        }
        
        public void SetLookupTableMultiplier(Stat source, Stat target, string multiplierUid)
        {
            foreach (StatModification mod in modifications)
            {
                if (mod.source != source) continue;
                if (mod.target != target) continue;
                mod.multiplierUid = multiplierUid;
            }
        }

        public int MultiplierIndex(Stat source)
        {
            for (int i = 0; i < availableModifiers.Length; i++)
            {
                foreach (StatModification mod in modifications)
                {
                    if (mod.source != source) continue;
                    if (mod.multiplierUid != availableModifiers[i].Uid()) continue;
                    return i;
                }
            }

            return 0;
        }
        
        public int MultiplierIndex(Stat source, Stat target)
        {
            for (int i = 0; i < availableModifiers.Length; i++)
            {
                foreach (StatModification mod in modifications)
                {
                    if (mod.source != source) continue;
                    if (mod.target != target) continue;
                    if (mod.multiplierUid != availableModifiers[i].Uid()) continue;
                    return i;
                }
            }

            return 0;
        }

        public void RemoveMods(Stat source, Stat target)
        {
            for (int i = modifications.Count - 1; i >= 0; i--)
            {
                if (modifications[i].source != source || modifications[i].target != target) continue;
                modifications.RemoveAt(i);
            }
        }

        public int GetLookupTableIndex(Stat source, LookupTable[] availableLookupTables)
        {
            for (int i = 0; i < availableLookupTables.Length; i++)
            {
                foreach (StatModification mod in modifications)
                {
                    if (mod.source != source) continue;
                    if (mod.lookupTableUid != availableLookupTables[i].Uid()) continue;
                    return i;
                }
            }

            return 0;
        }
        
        public int GetLookupTableIndex(Stat source, Stat target, LookupTable[] availableLookupTables)
        {
            if (availableLookupTables == null)
                return 0;
            
            for (int i = 0; i < availableLookupTables.Length; i++)
            {
                foreach (StatModification mod in modifications)
                {
                    if (mod.source != source) continue;
                    if (mod.target != target) continue;
                    if (mod.lookupTableUid != availableLookupTables[i].Uid()) continue;
                    return i;
                }
            }

            return 0;
        }

        public void SetLookupTableUid(Stat source, string uid)
        {
            foreach (StatModification mod in modifications)
            {
                if (mod.source != source) continue;
                mod.lookupTableUid = uid;
            }
        }
        
        public void SetLookupTableUid(Stat source, Stat target, string uid)
        {
            foreach (StatModification mod in modifications)
            {
                if (mod.source != source) continue;
                if (mod.target != target) continue;
                mod.lookupTableUid = uid;
            }
        }

        public void SetShowInInspector(Stat source, Stat target, bool value)
        {
            foreach (StatModification mod in modifications)
            {
                if (mod.source != source) continue;
                if (mod.target != target) continue;
                mod.isActivated = value;
            }
        }

        public bool GetShowInInspector(Stat source, Stat target)
        {
            foreach (StatModification mod in modifications)
            {
                if (mod.source != source) continue;
                if (mod.target != target) continue;
                return mod.isActivated;
            }

            return true;
        }
    }
}