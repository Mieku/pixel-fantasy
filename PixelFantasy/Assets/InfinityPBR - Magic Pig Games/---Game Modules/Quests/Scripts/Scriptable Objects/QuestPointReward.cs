using System;
using Random = UnityEngine.Random;

namespace InfinityPBR.Modules
{
    [Serializable]
    public class QuestPointReward
    {
        public Stat stat;
        public int pointsMin;
        public int pointsMax;

        public int Points => Random.Range(pointsMin, pointsMax); // Returns a random Point value
        
        // ------------------------------------------------------------------------------------------
        // CONSTRUCTOR
        // ------------------------------------------------------------------------------------------

        public QuestPointReward(Stat newStat, int min, int max)
        {
            stat = newStat;
            pointsMin = min;
            pointsMax = max;
        }
    }
}