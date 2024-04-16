namespace InfinityPBR.Modules
{
    public interface IHaveDictionaries
    {
        void AddDictionaryKey(string key);
        KeyValue GetKeyValue(string key);
        bool HasKeyValue(string key);
        void CheckForMissingObjectReferences(); // Used only for Editor objects! Check ModulesScriptableObject for an example
    }
}