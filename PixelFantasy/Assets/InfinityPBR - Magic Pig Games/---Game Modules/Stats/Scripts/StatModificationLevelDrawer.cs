namespace InfinityPBR.Modules
{
    public interface StatModificationLevelDrawer
    {
        void Draw(ModificationLevel modificationLevel, Stat thisObject);
        void DrawSimple(ModificationLevel modificationLevel, Stat thisObject);
    }
}