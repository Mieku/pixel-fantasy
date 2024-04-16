using System;
using Random = UnityEngine.Random;

/*
 * This class is used in the "Starting Stats" section, and can be useful for setting up the values of starting
 * GameStats.
 */

namespace InfinityPBR.Modules
{
    [Serializable]
    public class StatSettings
    {
        public Stat stat;
        public int pointsMin;
        public int pointsMax;

        public GameStat CreateStat(IHaveStats owner, GameStatList parentList)
        {
            if (stat == null)
                throw new NullReferenceException("The StatSettings is missing a stat! This is required.");

            var gameStat = new GameStat(stat, parentList);
            gameStat.SetParentList(parentList);
            gameStat.SetPoints(Random.Range(pointsMin, pointsMax), true, false);
            gameStat.FinalStat(true);
            return gameStat;
        }
    }
}
