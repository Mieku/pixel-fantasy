namespace InfinityPBR.Modules
{
    public interface DictionariesDrawer
    {
        void Draw(Dictionaries dictionaries, string className, string objectName, string objectType);
        void DrawStructure(Dictionaries dictionaries, string className, string objectType);
        void DrawItemObject(Dictionaries dictionaries, string objectName, string objectType);
        void DrawItemAttribute(Dictionaries dictionaries, string objectName, string objectType);
        void ResetCache(string newClassName, string objectType);
    }
}