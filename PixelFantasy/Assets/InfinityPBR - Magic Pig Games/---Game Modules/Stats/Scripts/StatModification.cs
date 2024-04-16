using System;
using UnityEngine;

namespace InfinityPBR.Modules
{
    [Serializable]
    public class StatModification
    {
        public bool isBase;
        public bool isPerSkillPoint;
        public Stat source;
        public Stat target;

        public int sourceCalculationStyle;
        public string multiplierUid = "";
        public string sourceUid; // Source UID is the uid of the Stat as the multiplier. "Base" and "per Skill point" are put here for those specific types
        public string targetUid; // Uid of the Stat being affected
        public float value;
        public float proficiency;
        public string lookupTableUid;
        public bool isActivated = true;

        public StatModification(Stat newSource, Stat newTarget, float newValue, float newProficiency, int newSourceCalculationStyle = 0, bool newIsBase = false, bool newIsPerSkillPoint = false)
        {
            isBase = newIsBase;
            isPerSkillPoint = newIsPerSkillPoint;
            target = newTarget;
            targetUid = newTarget.Uid();
            if (newSource != null)
            {
                source = newSource;
                sourceUid = newSource.Uid();
            }
            value = newValue;
            proficiency = newProficiency;
            sourceCalculationStyle = newSourceCalculationStyle;
        }

        public float ComputeBaseValueEffect(IHaveStats owner)
        {
            var finalValue = value;

            return finalValue;
        }
        
        public float ComputeBaseProficiencyEffect(IHaveStats owner)
        {
            var finalProficiency = proficiency;

            return finalProficiency;
        }

        public (float, float) GetEffectOn(float points = 0f, IHaveStats owner = null, bool debug = false)
        {
            var valueAdd = 0f;
            var proficiencyAdd = 0f;
            
            // If isBase is true, it means the base values are the only affect, so return those
            if (isBase)
            {
                valueAdd += value;
                proficiencyAdd += proficiency;
                return (valueAdd, proficiencyAdd);
            }
            
            // If isPerSkillPoint is true, it means the effect is base multiplied by points, return that.
            if (isPerSkillPoint)
            {
                valueAdd += value * points;
                proficiencyAdd += proficiency * points;
                return (valueAdd, proficiencyAdd);
            }

            // If there is no owner, then we can't do any other computation, so return our values
            if (owner == null) return (valueAdd, proficiencyAdd);
            
            // If the owner doesn't have the source stat, then return our values
            if (!owner.TryGetGameStat(sourceUid, out var sourceStat)) return (valueAdd, proficiencyAdd);

            if (sourceCalculationStyle == 0) // Raw value/proficiency
            {
                valueAdd += value;
                proficiencyAdd += proficiency;
                return (valueAdd, proficiencyAdd);
            }
            
            if (sourceCalculationStyle == 1) // Points
            {
                valueAdd += value * sourceStat.Points;
                proficiencyAdd += proficiency * sourceStat.Points;
                return (valueAdd, proficiencyAdd);
            }
            
            if (sourceCalculationStyle == 2) // Proficiency
            {
                valueAdd += value * (1 + sourceStat.FinalProficiency);
                proficiencyAdd += proficiency * (1 + sourceStat.FinalProficiency);
                return (valueAdd, proficiencyAdd);
            }
            
            if (sourceCalculationStyle == 3) // Final Stat
            {
                valueAdd += value * sourceStat.FinalStat( true);
                proficiencyAdd += proficiency * sourceStat.FinalStat( true, true);
                return (valueAdd, proficiencyAdd);
            } 
            
            if (sourceCalculationStyle == 4 || sourceCalculationStyle == 5) // 4 = * Lookup Value of another [Stat]  || 5 = * Lookup Value (of source)
            {
                var lookupTable = GameModuleRepository.Instance.Get<LookupTable>(lookupTableUid); // statsRepository.GetLookupTableByUid(lookupTableUid);
                
                // If we can't find the lookupTable ,return the values as they are
                if (lookupTable == null)
                    return (valueAdd, proficiencyAdd);

                // If the owner doesn't have teh required stat, then return the values as they are
                if (!owner.TryGetGameStat(multiplierUid, out var multiplierStat))
                    return (valueAdd, proficiencyAdd);

                var lookupOutput = lookupTable.ResultFrom(multiplierStat.FinalStat(true, true));
                
                // If the calculation style is 4, then multiply by Final Stat, otherwise, multiply by 1 (i.e. no effect)
                var valueIfStyleIs4 = sourceCalculationStyle == 4 ? sourceStat.FinalStat(true, true) : 1;
                valueAdd += value * valueIfStyleIs4 * lookupOutput;
                proficiencyAdd += proficiency * valueIfStyleIs4 * lookupOutput;
                
                return (valueAdd, proficiencyAdd);
            }

            return (valueAdd, proficiencyAdd);
        }
        
        public StatModification Clone() => JsonUtility.FromJson<StatModification>(JsonUtility.ToJson(this));
        
        public Stat StatThatAffects(Stat affectedStat) => isBase || isPerSkillPoint ? null : target == affectedStat ? source : null;
        
        /// <summary>
        /// Returns true if this mod has no effect.
        /// </summary>
        /// <returns></returns>
        public bool HasNoEffect() => !isActivated && !isBase && !isPerSkillPoint || value == 0 && proficiency == 0;
    }
}