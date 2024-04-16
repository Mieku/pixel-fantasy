namespace InfinityPBR.Modules
{
    public interface IHaveQuests : IUseGameModules
    {
        GameQuest AddQuest(string uid);
    }
}