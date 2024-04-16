namespace InfinityPBR.Modules
{
    public interface IHaveModificationLevels
    {
        MasteryLevel GetMasteryLevel(bool recompute = false);
        MasteryLevel SetMasteryLevel(int index);
        int GetMasteryLevelIndex(bool recompute = false);
        string GetMasteryLevelName(bool recompute = false);

        MasteryLevel AddToMasteryLevel(int value);
        MasteryLevel RecomputeActiveMasteryLevel();
        
        ModificationLevel GetModificationLevel(bool recompute = false);
        int ModificationLevelsCount();
        
    }
}