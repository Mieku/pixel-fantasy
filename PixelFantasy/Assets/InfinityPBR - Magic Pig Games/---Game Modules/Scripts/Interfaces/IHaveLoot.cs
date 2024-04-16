namespace InfinityPBR.Modules
{
    public interface IHaveLoot : IUseGameModules
    {
        void GenerateLoot(bool overwrite);
        
    }
}