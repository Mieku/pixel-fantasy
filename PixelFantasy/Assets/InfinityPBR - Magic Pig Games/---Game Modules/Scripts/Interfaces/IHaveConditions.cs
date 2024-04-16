using System.Collections.Generic;

namespace InfinityPBR.Modules
{
    public interface IHaveConditions : IUseGameModules
    {
        void ConditionPointEffect(string statUid, float pointValue);
        //void DoInstantActions(GameCondition gameCondition); // June 8 2023 - ConditionPointEffect will be called for Instant Actions
        //void SetStatDirty(string uid, bool dirty = true); // June 8 2023 - this appears to not be in use, and
        // instead is using SetStatsDirty() below, which was in the IHaveStats interface. We will copy it here.
        void SetStatsDirty(List<Stat> statList);
        GameCondition AddCondition(string uid, IHaveStats source = null);
        
    }
}