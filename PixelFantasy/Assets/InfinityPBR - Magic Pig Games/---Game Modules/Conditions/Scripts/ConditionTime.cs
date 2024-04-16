using System;
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
    public class ConditionTime
    {
        [FormerlySerializedAs("minutes")] public float minMinutes; // base number of in-game minutes
        public float maxMinutes; // base number of in-game minutes
        public Stat stat; // Optional stat required
        [FormerlySerializedAs("perPoint")] public float minPerPoint; // Per point of Stat
        public float maxPerPoint; // Per point of Stat
        [FormerlySerializedAs("perFinalValue")] public float minPerFinalValue; // Per final value of Stat
        public float maxPerFinalValue; // Per final value of Stat
        public LookupTable lookupTable; // Optional lookup Table
        public Stat perOutputStat; // Optional stat for lookup table
        [FormerlySerializedAs("perOutput")] public float minPerOutput; // Per output value of either Stat in lookupTable
        public float maxPerOutput; // Per output value of either Stat in lookupTable

        public float Time(IHaveStats owner = null)
        {
            var time = Random.Range(minMinutes, maxMinutes);
            if (owner == null) return time;
            if (stat == null) return time; // There is no stat, so we will return the base time.
            if (!owner.TryGetGameStat(stat.Uid(), out var gameStat)) return time; // Owner doesn't have stat, then only minutes can occur
            
            time += Random.Range(minPerPoint, maxPerPoint) * gameStat.Points;
            time += Random.Range(minPerFinalValue, maxPerFinalValue) * gameStat.FinalStat(true, true);

            if (lookupTable == null) return time; // No lookup table, so nothing else to add
            
            // Owner doesn't have lookup stat, so use the required stat instead
            if (!owner.TryGetGameStat(perOutputStat.Uid(), out var lookupStat))
            {
                time += Random.Range(minPerOutput, maxPerOutput) * lookupTable.ResultFrom(gameStat.FinalStat());
                return time;
            }

            time += Random.Range(minPerOutput, maxPerOutput) * lookupTable.ResultFrom(lookupStat.FinalStat(true, true));
            return time;
        }
    }
}