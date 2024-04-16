namespace InfinityPBR.Modules
{
    public interface IUseGameModules : IHaveGameId
    {
        string GetOwnerName();
        void SetOwner(object newOwner);
    }
}