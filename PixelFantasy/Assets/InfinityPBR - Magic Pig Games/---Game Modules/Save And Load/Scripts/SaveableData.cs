using System;
using System.Collections.Generic;
using System.Linq;

namespace InfinityPBR.Modules
{
    [Serializable]
    public class SaveableData
    {
        public List<SaveableKeyValue> keyValues = new List<SaveableKeyValue>();
        //public Dictionary<string, string> jsonEncodedSaveables = new Dictionary<string, string>();
        public void AddKeyValue(string addKey, string addValue)
        {
            var foundKeyValue = keyValues.FirstOrDefault(x => x.key == addKey);

            // This means the key is new, so create a new key/value pair
            if (foundKeyValue == null)
            {
                keyValues.Add(new SaveableKeyValue(addKey, addValue));
                return;
            }

            // Replace the value with the updated value
            foundKeyValue.value = addValue;
        }

        public bool TryGetValue(string searchKey, out string value)
        {
            var found = keyValues.FirstOrDefault(x => x.key == searchKey);
            
            // If it is not found, that does not mean an error, it just means this save file has not yet
            // saved any data for this object. This occurs, as an example, when a scene is first loaded. At that
            // point, the current save file will have no data on the Saveable objects in this scene. Once the
            // data is saved in this scene, it will.
            if (found == null)
            {
                value = default;
                return false;
            }

            value = found.value;
            return true;
        }
    }

    [Serializable]
    public class SaveableKeyValue
    {
        public string key;
        public string value; // This will be a json string

        public SaveableKeyValue(string newKey, string newValue)
        {
            key = newKey;
            value = newValue;
        }
    }
}