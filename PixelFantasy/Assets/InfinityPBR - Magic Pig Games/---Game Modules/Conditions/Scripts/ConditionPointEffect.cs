using System;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

/*
 * June 17, 2022
 * Updated this to have min/max values for points.  The old version is "min", to minimize the disruption to existing projects.
 * Custom code may need to be adjusted to account for this update.
 */

namespace InfinityPBR.Modules
{
    [Serializable]
    public class ConditionPointEffect
    {
        public Stat statAffected;
        public float minPoints; // base number of points
        public float maxPoints; // base number of points
        public Stat stat; // Optional stat required
        public float minPerPoint; // Per point of Stat
        public float maxPerPoint; // Per point of Stat
        public float minPerFinalStat; // Per final value of Stat
        public float maxPerFinalStat; // Per final value of Stat
        public LookupTable lookupTable; // Optional lookup Table
        public Stat perOutputStat; // Optional stat for lookup table
        public Stat maxPerOutputStat; // Optional stat for lookup table
        public float minPerOutput; // Per output value of either Stat in lookupTable
        public float maxPerOutput; // Per output value of either Stat in lookupTable

        public float finalValue; // This is the final value computed. Used in GameCondition.cs

        public float ComputeEffect(IHaveStats sourceOfCondition = null) // "sourceOfCondition" == caster in this context
        {
            var final = Random.Range(minPoints, maxPoints);
            if (sourceOfCondition == null)
                return final;
            
            // Add owner-based ("caster"-based) effects
            final += RequiredStatValue(sourceOfCondition);
            final += LookupTableValue(sourceOfCondition);
            return final;
        }

        private float LookupTableValue(IHaveStats owner)
        {
            if (lookupTable == null) return 0f; // No lookup table, so no effect
            if (perOutputStat == null) return 0f; // This should never be null, but in case it is... no effect
            if (!owner.TryGetGameStat(perOutputStat.Uid(), out var gameStat)) return 0f; // Owner doesn't have the required stat
            return Random.Range(minPerOutput, maxPerOutput) * lookupTable.ResultFrom(gameStat.FinalStat());
        }

        private float RequiredStatValue(IHaveStats owner)
        {
            if (stat == null) return 0f; // No stat, so no effect
            if (!owner.TryGetGameStat(stat.Uid(), out var gameStat)) return 0f; // Owner doesn't have stat, no effect
            return gameStat.Points * Random.Range(minPerPoint, maxPerPoint) + gameStat.FinalStat() * Random.Range(minPerFinalStat, maxPerFinalStat);
        }
    }
}