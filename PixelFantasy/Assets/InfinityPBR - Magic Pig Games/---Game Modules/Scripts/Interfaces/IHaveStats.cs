using System.Collections;
using System.Collections.Generic;

namespace InfinityPBR.Modules
{
    public interface IHaveStats : IUseGameModules
    {
        bool TryGetGameStat(string uid, out GameStat gameStat);
        List<ModificationLevel> GetOtherLevels(bool cache = false);
        void SetStatsDirty(List<Stat> statList);

        public GameStat GetStat(string uid, bool addIfNull = true);
    }
}