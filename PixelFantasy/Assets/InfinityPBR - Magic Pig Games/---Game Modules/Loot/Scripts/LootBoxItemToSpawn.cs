using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static InfinityPBR.Modules.Utilities;

/*
 * Each LootBoxItemToSpawn contains the data required to spawn one item, including a list of Item Objects, and
 * lists of Item Attributes. Item Attribute lists are serialized into a list of strings, which is transformed into
 * a Dictionary at runtime, once. This is automatic.
 */

namespace InfinityPBR.Modules
{
    
    [Serializable]
    public class AvailableItemAttributes
    {
        public string objectType;
        public List<ItemAttribute> attributes;
    }

    [Serializable]
    public class LootBoxItemToSpawn
    {
        public LootItems lootItems; // The lootItems object this belongs to
        public bool force; // True if this will be forced on Loot Box Generation
        public List<ItemObject> itemObjects = new List<ItemObject>(); // List of potential ItemObjects this might spawn

        public List<AvailableItemAttributes> availableItemAttributes = new List<AvailableItemAttributes>();
        public string[] availableItemAttributeTypes 
            => availableItemAttributes
            .Select(x => x.objectType)
            .ToArray();
        public string[] availableItemAttributesOfType(string objectType) 
            => availableItemAttributes
                .FirstOrDefault(x => x.objectType == objectType)
                ?.attributes
                .Select(x => x.ObjectName)
                .ToArray();
        
        public List<string> itemAttributeSelections; // A comma delimited list of ItemAttribute type & list of potentials. Serializable.
        public int ItemsSelectedCount => itemObjects.Count;
        
        // Since we can not serialize the dictionary, it starts empty. _dictionaryLoaded will be false, and the dictionary will
        // populate from itemAttributeSelections as soon as the script is engaged with. This should only happen once per run time.
        public Dictionary<string, List<ItemAttribute>> attributeNames = new Dictionary<string, List<ItemAttribute>>();
        public bool _dictionaryLoaded;
        
        [SerializeField] public Dictionaries attributeData = new Dictionaries("no name");
        
        public List<ItemAttribute> ItemAttributesList(string objectType) => GetItemAttributes(objectType);

        public void ResetDictionary() => _dictionaryLoaded = false;
        
        public void CheckListsForDeletions() => itemObjects.RemoveAll(x => !lootItems.itemObjects.Contains(x));

        public List<ItemAttribute> GetItemAttributes(string objectType)
        {
            LoadAttributeDictionary();

            if (attributeNames.TryGetValue(objectType, out var outList)) return outList;
            
            AddItemAttribute(objectType, null);
            return default;
        }

        public int GetItemAttributesCount(string objectType)
        {
            LoadAttributeDictionary();
            
            var attributes = GetItemAttributes(objectType);
            if (attributes == null)
                return 0;
            return attributes.Count;
        }

        public void AddItemAttribute(string objectType, ItemAttribute itemAttribute)
        {
            LoadAttributeDictionary();
            
            if (!attributeNames.ContainsKey(objectType))
                attributeNames.Add(objectType, new List<ItemAttribute>());

            if (itemAttribute == null)
                return;
            
            if (!attributeNames.TryGetValue(objectType, out var outList))
                return;

            outList.Add(itemAttribute);
        }

        /// <summary>
        /// This will save the Dictionary data in to the comma delimited list
        /// </summary>
        public void SaveAttributeDictionary()
        {
            if (itemAttributeSelections == null) return;
            itemAttributeSelections.Clear();
            foreach (string key in attributeNames.Keys)
            {
                string newAttributeString = $"{key}";

                foreach (ItemAttribute itemAttribute in attributeNames[key])
                    newAttributeString = $"{newAttributeString},{itemAttribute.Uid()}";

                itemAttributeSelections.Add(newAttributeString);
            }
        }

        /// <summary>
        /// This will load the comma delimited list into the Dictionary format
        /// </summary>
        public void LoadAttributeDictionary()
        {
            if (_dictionaryLoaded) return;
            _dictionaryLoaded = true;

            attributeNames.Clear();
            if (itemAttributeSelections == null) return;
            foreach (string attributeString in itemAttributeSelections)
            {
                var values = attributeString.Split(',').ToList(); // Split the string
                string objectType = values[0]; // The first value is the objectType
                for (int i = 1; i < values.Count; i++) // Go through each of the other values, which are ItemAttributes
                    AddItemAttribute(objectType, GetItemAttribute(values[i])); // Add them to the dictionary
            }
        }
    }
}