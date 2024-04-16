using System;
using System.Collections.Generic;
using UnityEngine;

namespace InfinityPBR.Modules
{
    public abstract class Repository<T> : MonoBehaviour, IAmRepository where T : ModulesScriptableObject
    {
        public List<T> scriptableObjects = new List<T>(); // List of the objects, i.e. "Stat", "ItemObject", "Condition" etc
        
        // This is used at runtime for caching the data, so when we look something up, it is saved here.
        // Things used often will be faster to lookup, than things not used very often.
        protected readonly Dictionary<string, T> itemDictionary = new Dictionary<string, T>();
        protected readonly Dictionary<string, T> itemDictionaryByName = new Dictionary<string, T>();

        [Obsolete("Use GameModulesRepository.Instance.Get<T>(string uid) instead.")]
        public virtual T GetByUid(string uid)
        {
            Debug.LogWarning("This method is obsolete. Use GameModulesRepository.Instance.Get<T>(string uid) instead.");

            if (string.IsNullOrWhiteSpace(uid))
            {
                Debug.LogError("Attempting to get an object with null or empty uid. That is not allowed");
                return default;
            }
            
            // First try to get the item from the Dictionary
            if (itemDictionary.TryGetValue(uid, out var found)) return found;
            
            // If we couldn't find it, try to add it, and return that.
            if (TryAddItemByUid(uid, out var newEntry)) return newEntry;

            // If we still couldn't find it, likely the uid provided is incorrect. Throw a warning, as this
            // generally should not happen. Devs can remove this comment if they are OK with this result.
            // (But really, it should not happen!)
            Debug.LogWarning($"Warning: Could not find an item of type {typeof(T)} " +
                             $"with uid {uid}. Comment out this Debug.Log if you do not want to be warned.");
            return default;
        }
        
        [Obsolete("Use GameModulesRepository.Instance.Get<T>(string name) instead.")]
        public virtual T GetByName(string objectName)
        {
            Debug.LogWarning("This method is obsolete. Use GameModulesRepository.Instance.Get<T>(string name) instead.");

            if (string.IsNullOrWhiteSpace(objectName))
            {
                Debug.LogError("Attempting to get an object with null or empty objectName. That is not allowed");
                return default;
            }
            
            // First try to get the item from the Dictionary
            if (itemDictionaryByName.TryGetValue(objectName, out var found)) return found;
            
            // If we couldn't find it, try to add it, and return that.
            if (TryAddItemByObjectName(objectName, out var newEntry)) return newEntry;

            // If we still couldn't find it, likely the name provided is incorrect. Throw a warning, as this
            // generally should not happen. Devs can remove this comment if they are OK with this result.
            // (But really, it should not happen!)
            Debug.LogWarning($"Warning: Could not find an item of type {typeof(T)} " +
                             $"with ObjectName {objectName}. Comment out this Debug.Log if you do not want to be warned.");
            return default;
        }

        protected bool TryAddItemByUid(string uid, out T newEntry)
        {
            bool found = false;
            foreach(T item in scriptableObjects)
            {
                if (item.Uid() != uid) continue;
                itemDictionary.Add(uid, item);
                found = true;
                break;
            }

            if (!found)
            {
                newEntry = default;
                return false;
            }

            newEntry = itemDictionary[uid];
            return newEntry != null;
        }
        
        protected bool TryAddItemByObjectName(string objectName, out T newEntry)
        {
            bool found = false;
            foreach(T item in scriptableObjects)
            {
                if (item.ObjectName != objectName) continue;
                itemDictionaryByName.Add(objectName, item);
                found = true;
                break;
            }

            if (!found)
            {
                newEntry = default;
                return false;
            }

            newEntry = itemDictionaryByName[objectName];
            return newEntry != null;
        }
        
        // We populate the list automatically OnValidate(), so it's all just magical!
        protected virtual void OnValidate()
        {
#if UNITY_EDITOR
            PopulateList();
#endif
        }

        // Each type of Repository will need to have its own PopulateList() method because each will
        // need to do different things than the others.
        public abstract void PopulateList();
    }
}