using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InfinityPBR.Modules
{
    [Serializable]
    public class Dictionaries : IHaveUid
    {
        public string name;
        public List<KeyValue> keyValues = new List<KeyValue>();
        
        public string uid => Uid();
        private string _uid;
        
        // These are all the types that Dictionaries supports
        public enum Types
        {
            Int, Float, Bool, String, Stat, ItemObject, ItemAttribute, Quest, QuestCondition, QuestReward, Condition, LookupTable
            , LootBox, LootItems, Texture2D, Sprite, AnimationClip, AudioClip, Vector2, Vector3, Vector4, Color, GameObject
        }

        public Dictionaries(string newName = "") // Jan 4, 2023 -- Made the value of name default to empty. I'm not sure we utilize this.
        {
            name = newName;
            keyValues = new List<KeyValue> {new KeyValue()};
            keyValues.Clear();
        }
        
        // **********************************************************************
        // Custom Inspectors
        // **********************************************************************

        public void Draw(DictionariesDrawer dictionariesDrawer, string newClassName, string objectName = "", string objectType = "") 
            => dictionariesDrawer.Draw(this, newClassName, objectName, objectType);

        public void DrawStructure(DictionariesDrawer dictionariesDrawer, string className, string objectType = "")
            => dictionariesDrawer.DrawStructure(this, className, objectType);
        
        // **********************************************************************
        // Save and Load
        // **********************************************************************

        public string SaveData()
        {
            // Set up a data structure for this dictionaries
            var dataToSave = new DictionariesSavedData
            {
                uid = Uid(),
                name = name,
                keyValues = new List<string>()
            };

            // Add the json encoded value from each KeyValue
            foreach (var keyValue in keyValues)
                dataToSave.keyValues.Add(JsonUtility.ToJson(keyValue));

            Debug.Log($"Dictionaries: {dataToSave}");
            return JsonUtility.ToJson(dataToSave);
        }

        public void LoadData(string savedData, ModulesScriptableObject modulesScriptableObject = null)
        {
            Debug.Log("Load DictionaryData");
            var dataFromSave = JsonUtility.FromJson<DictionariesSavedData>(savedData); // Get the data out
            _uid = dataFromSave.uid;
            name = dataFromSave.name;
            
            // Ensure the KeyValues is clear, then add the data back into the List
            keyValues.Clear();
            foreach (var savedKeyValue in dataFromSave.keyValues)
                keyValues.Add(JsonUtility.FromJson<KeyValue>(savedKeyValue));
        }

        // **********************************************************************
        // Add KeyValue
        // **********************************************************************

        /// <summary>
        /// Adds an empty KeyValue with the provided key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="allowDuplicates"></param>
        /// <returns></returns>
        public KeyValue AddNewKeyValue(string key, bool allowDuplicates = false)
        {
            if (!allowDuplicates && HasKeyValue(key))
                return default;

            var newKeyValue = new KeyValue {key = key};
            keyValues.Add(newKeyValue);
            return newKeyValue;
        }

        /// <summary>
        /// Clones a key value and then adds it.
        /// </summary>
        /// <param name="keyValueToClone"></param>
        /// <param name="allowDuplicates"></param>
        /// <returns></returns>
        public KeyValue AddNewKeyValue(KeyValue keyValueToClone, bool allowDuplicates = false)
        {
            if (!allowDuplicates && HasKeyValue(keyValueToClone.key))
                return default;

            var newKeyValue = keyValueToClone.Clone();
            keyValues.Add(newKeyValue);
            return newKeyValue;
        }

        // **********************************************************************
        // REPLACE, APPEND, COPY
        // **********************************************************************

        /// <summary>
        /// Given a Dictionaries object, will replace this Dictionaries structure with that. Will retain values if the
        /// Key matches, but will remove all Keys that are not in the copied structure.
        /// </summary>
        /// <param name="copyThis"></param>
        public void ReplaceStructureWith(Dictionaries copyThis)
        {
            if (copyThis == null) return;
            
            // Remove keyValues that aren't in copyThis
            for (var i = keyValues.Count - 1; i >= 0; i--)
            {
                if (copyThis.HasKeyValue(keyValues[i].key)) 
                    continue;
                
                keyValues.RemoveAt(i);
            }

            AppendStructureWith(copyThis);
        }
        
        /// <summary>
        /// Adds Keys from the copied Dictionaries to this. Will not remove or replace any existing content.
        /// </summary>
        /// <param name="copyThis"></param>
        /// <param name="clone"></param>
        public void AppendStructureWith(Dictionaries copyThis, bool clone = false)
        {
            if (copyThis == null) return;
            keyValues ??= new List<KeyValue>();
            
            // Add keyValues from copyThis that aren't in this keyValues
            for (var i = 0; i < copyThis.keyValues.Count; i++)
            {
                if (keyValues.Any(x => x.key == copyThis.keyValues[i].key)) 
                    continue;

                keyValues.Insert(i, clone ? copyThis.keyValues[i].Clone() : copyThis.keyValues[i]);
            }
        }
        
        /// <summary>
        /// Replaces all of the content from one Dictionaries to this, but will not change the structure. Only shared
        /// keys will be updated.
        /// </summary>
        /// <param name="copyThis"></param>
        public void ReplaceContentWith(Dictionaries copyThis)
        {
            
            if (copyThis == null) return;
            for (var i = keyValues.Count - 1; i >= 0; i--)
            {
                var copyKeyValue = copyThis.keyValues.FirstOrDefault(x => x.key == keyValues[i].key);
                if (copyKeyValue == null)
                    continue;
                
                keyValues[i].ReplaceContentWith(copyKeyValue);
            }
        }
        
        // **********************************************************************
        // OTHER METHODS
        // **********************************************************************

        public Dictionaries Clone() => JsonUtility.FromJson<Dictionaries>(JsonUtility.ToJson(this));
        
        /// <summary>
        /// Removes all named keys and associated other values.
        /// </summary>
        /// <param name="key"></param>
        public void RemoveKey(string key) => keyValues.RemoveAll(x => x.key == key);

        /// <summary>
        /// Returns true if the provided key exists in the dictionary
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool HasKeyValue(string key)
        {
            if (keyValues != null) return keyValues.Any(x => x.key == key);
            keyValues = new List<KeyValue>();
            return false;
        }

        /// <summary>
        /// Returns the KeyValue with the given key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public KeyValue Key(string key) => keyValues.FirstOrDefault(x => x.key == key);
        
        /// <summary>
        /// Returns a value with the provided key of the type set. This will return the default value if the
        /// key is not found.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="random"></param>
        /// <param name="index"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Value<T>(string key, bool random = false, int index = 0)
        {
            var keyValue = keyValues.FirstOrDefault(x => x.key == key);
            Debug.Log($"keyValue is null? {keyValue == null}");
            if (keyValue != null)
            {
                Debug.Log($"There are {keyValue.values.Count} values in this KeyValue. ");
                if (keyValue.values.Count > 0)
                {
                    Debug.Log($"Index 0 is null? {keyValue.values[0] == null}");
                }
            }
            return keyValue == null ? default : keyValue.Value<T>(random, index);
        }

        /// <summary>
        /// Returns the number of entries which utilize the provided key. Helpful for knowing
        /// how many entries exist with this key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int Count(string key = "") => string.IsNullOrWhiteSpace(key) 
            ? keyValues.Count : keyValues.Count(x => x.key == key);
        
        /// <summary>
        /// Renames all keys in the key list.
        /// </summary>
        /// <param name="oldKey"></param>
        /// <param name="newKey"></param>
        public void RenameKey(string oldKey, string newKey)
        {
            foreach (var keyValue in keyValues)
                keyValue.Rename(oldKey, newKey);
        }
        
        // Feb 14, 2023 -- Do these need to have UIDs? I don't think they do.
        public string Uid()
        {
            if (!string.IsNullOrWhiteSpace(_uid)) return _uid;
            _uid = Guid.NewGuid().ToString();
            return _uid;
        }
    }
}