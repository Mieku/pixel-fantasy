using System.Collections.Generic;

namespace InfinityPBR.Modules
{
    [System.Serializable]
    public class RepositoryList
    {
        public string typeName;
        public List<RepositoryItems> items = new List<RepositoryItems>();
    }
}