using System;
using System.Collections.Generic;
using UnityEngine;
using static InfinityPBR.Modules.Utilities;
using Random = UnityEngine.Random;

namespace InfinityPBR.Modules
{
    [CreateAssetMenu(fileName = "Point Reward...", menuName = "Game Modules/Quest Reward/Point Reward", order = 1)]
    [Serializable]
    public class PointReward : QuestReward
    {
        public List<StatPoints> statPoints = new List<StatPoints>();
        public override void GiveReward(IUseGameModules owner)
        { 
            var statOwner = (IHaveStats)owner;
            foreach (var statPoint in statPoints)
            {
                // Try to get the stat, add if it is null and we are opted in to do that.
                // If found, add the points, and the option for useProficiencyModifier
                var gameStat = statOwner.GetStat(statPoint.stat.Uid(), statPoint.addIfNull);
                gameStat?.AddPoints(statPoint.Points, statPoint.useProficiencyModifier);
            }
        }
    }

    [Serializable]
    public class StatPoints
    {
        public Stat stat;
        public float pointsMin;
        public float pointsMax;
        public Rounding roundingMethod = 0;
        public int decimals = 2;
        public bool useProficiencyModifier = true;
        public bool addIfNull;

        public float Points => RoundThis(Random.Range(pointsMin, pointsMax), decimals, roundingMethod);
    }
}