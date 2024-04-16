using System;
using System.Collections.Generic;

namespace InfinityPBR.Modules
{
    [Serializable]
    public struct DictionariesSavedData
    {
        public string name;
        public List<string> keyValues;
        public string uid;

        public DictionariesSavedData(string newName, string newUid)
        {
            name = newName;
            uid = newUid;
            keyValues = new List<string>();
        }
    }
}