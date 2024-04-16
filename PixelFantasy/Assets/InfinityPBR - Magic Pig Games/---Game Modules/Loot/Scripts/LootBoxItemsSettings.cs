using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static InfinityPBR.Modules.Utilities;

namespace InfinityPBR.Modules
{
    [Serializable]
    public class LootBoxItemsSettings
    {
        public LootItems lootItems;
        public bool stopAfterFirstFailure = true; // If true, when populating items above min, process stops if one item fails to populate
        public List<LootBoxItemToSpawn> itemsToSpawn = new List<LootBoxItemToSpawn>();
        public AnimationCurve itemChances;// = AnimationCurve.Linear(0, 1, 1, 0.1f);
        public AnimationCurve attributeChances;// = AnimationCurve.Linear(0, 0.9f, 1, 0.1f);
        
        public bool randomizeAttributeOrder = true;
        public bool stopAfterAttributeFailure;

        // Private for the editor script
        [HideInInspector] public bool show;
        [HideInInspector] public int itemsIndex;
        [HideInInspector] public int prefixIndex;
        [HideInInspector] public int itemIndex;
        [HideInInspector] public int suffixIndex;
        
        [HideInInspector] public List<string> attributeTypes = new List<string>();
        [HideInInspector] public List<bool> attributeForce = new List<bool>();
        [HideInInspector] public Dictionary<string, List<ItemAttribute>> attributeNames = new Dictionary<string, List<ItemAttribute>>();
        [HideInInspector] public Vector2 scrollPos;

        // Attribute Type Selection
        public void CacheAttributeTypeSelection()
        {
            cachedAvailableAllowedAttributeTypes = AvailableAllowedAttributeTypes;
        }
        
        public int newAttributeTypeIndex;
        public string[] cachedAvailableAllowedAttributeTypes = Array.Empty<string>();
        
        public List<string> activeAttributeTypes = new List<string>();
        public string[] AllowedAttributeTypes() 
            => lootItems.itemObjects
                .Select(x => x.allowedItemAttributes)
                .SelectMany(x => x)
                .Select(x => x.objectType)
                .Distinct()
                .OrderBy(x => x)
                .ToArray();
            
        public string[] AvailableAllowedAttributeTypes => AllowedAttributeTypes()
            .Where(x => !activeAttributeTypes.Contains(x))
            .ToArray();
        
        public void SetAttributeTypes()
        {
            var newListBefore = new List<ItemAttribute>();
            var newListAfter = new List<ItemAttribute>();
            var fullList = new List<ItemAttribute>();
            foreach (var itemObject in lootItems.itemObjects)
            {
                if (itemObject.allowedItemAttributes == null) continue;
                if (itemObject.allowedItemAttributes.Count == 0) continue;

                itemObject.ClearMissingAttributes();
                
                newListBefore.AddRange(itemObject.allowedItemAttributes
                    .Where(x => x.nameOrder < 0)
                    .Where(x => !String.IsNullOrWhiteSpace(x.humanName))
                    .ToList());
                newListAfter.AddRange(itemObject.allowedItemAttributes
                    .Where(x => x.nameOrder > 0)
                    .Where(x => !String.IsNullOrWhiteSpace(x.humanName))
                    .ToList());
                
                fullList.AddRange(itemObject.allowedItemAttributes
                    .Where(x => !String.IsNullOrWhiteSpace(x.humanName))
                    .ToList());
            }

            var stringList = newListBefore
                .Distinct()
                .OrderBy(x => x.nameOrder)
                .ThenBy(x => x.humanName)
                .Select(x => x.objectType)
                .Distinct()
                .ToList();
            
            var stringListAfter = newListAfter
                .Distinct()
                .OrderBy(x => x.nameOrder)
                .ThenBy(x => x.humanName)
                .Select(x => x.objectType)
                .Distinct()
                .ToList();

            var finalList = stringList;
            finalList.Add("Item Object");
            finalList.AddRange(stringListAfter);

            // Add any new attributes
            foreach (var finalListAttribute in finalList)
            {
                if (attributeTypes.Contains(finalListAttribute)) continue;
                
                attributeTypes.Add(finalListAttribute);
                attributeForce.Add(false);
            }

            // Remove any attributes no longer available
            for (var i = attributeTypes.Count - 1; i >= 0; i--)
            {
                if (finalList.Contains(attributeTypes[i])) continue;
                
                attributeTypes.RemoveAt(i);
            }

            foreach (var itemAttribute in fullList)
            {
                if (!attributeNames.ContainsKey(itemAttribute.objectType))
                    attributeNames.Add(itemAttribute.objectType, new List<ItemAttribute>());

                attributeNames[itemAttribute.objectType].Add(itemAttribute);
            }
        }

        // Count the number of items to spawn that are not forced. Set true to count only those which are forced.
        public int CountItemsToSpawn(bool force = false) => itemsToSpawn.Count(itemToSpawn => force == itemToSpawn.force);
        
        // Counts the number of attribute which are not forced to spawn. Set true to count only those which are forced.
        public int CountAttributes(bool force = false) => attributeForce.Count(x => x == force);
        
        // Gets the float value from the animation curves.
        public float ValueFromCurve(float percent) => itemChances.Evaluate(percent);
        public float ValueFromAttributeCurve(float percent) => attributeChances.Evaluate(percent);

        public string AttributeFromType(string attributeObjectType, LootBoxItemToSpawn itemToSpawn, ItemObject itemObject)
        {
            var thisAttributeList = itemToSpawn
                .availableItemAttributes
                .FirstOrDefault(x => x.objectType == attributeObjectType)
                ?.attributes;

            if (thisAttributeList == null)
            {
                Debug.LogWarning($"Did not find any attributes for {attributeObjectType}!");
                return null;
            }
            
            var potentialAttributes = new List<ItemAttribute>(); // Setup list of potential ItemAttributes for this
            foreach (var itemAttribute in thisAttributeList)
            {
                if (!itemObject.CanUseAttribute(itemAttribute.Uid())) continue; // If we can't use the ItemAttribute, continue

                potentialAttributes.Add(itemAttribute); // Add it to the list!
            }
            
            return potentialAttributes.Count == 0 ? null : // There were no potential ItemAttributes for the chosen ItemObject, return null
                potentialAttributes[RandomInt(0, potentialAttributes.Count)].Uid(); // Return a random ItemAttribute
        }

        public ItemObject GenerateItemObject(LootBoxItemToSpawn itemToSpawn) 
            => itemToSpawn.itemObjects[RandomInt(0, itemToSpawn.itemObjects.Count)];
    }
}