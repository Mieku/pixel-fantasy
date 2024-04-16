using System.Collections.Generic;

namespace InfinityPBR.Modules
{
    public interface IAffectStats
    {
        List<Stat> DirectlyAffectedByList(Stat stat = null);
        List<Stat> DirectlyAffectsList(Stat stat = null);
        List<ModificationLevel> GetModificationLevels();
        ModificationLevel GetModificationLevel();
        // void SetOwner(IHaveStats newOwner); // This is required for IAmGameModulesObject
        IHaveStats GetOwner();
        void SetAffectedStatsDirty(object obj);
    }
}