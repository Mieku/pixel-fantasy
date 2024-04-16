namespace InfinityPBR.Modules
{
    public interface IAmGameModuleObject
    {
        public string Uid();
        public string GameId(bool forceNew = false);
        public string ObjectName();
        public string ObjectType();
        //public void SetAffectedStatsDirty(IAmGameModuleObject gameModuleObject);
        //public void AddThisToBlackboard(bool allowNotification = true);

        //void SetDirty(bool dirtyValue = true);
    }
}