using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static InfinityPBR.Modules.Utilities;

/*
 * This scriptable object holds all the settings for a particular Loot Box. It is not itself run-time enabled, but will
 * be referred to by a Game Loot Box, which is something that is serializable at runtime.
 *
 * Call GenerateLoot to return the GameItemObjectList with the results from this LootBox. Each time it is called,
 * a different list of Item Objects with various Item Attributes will be returned.
 */

namespace InfinityPBR.Modules
{
    [HelpURL("https://infinitypbr.gitbook.io/infinity-pbr/game-modules/loot")]
    [CreateAssetMenu(fileName = "Loot Box", menuName = "Game Modules/Create/Loot Box", order = 1)]
    [Serializable]
    public class LootBox : ModulesScriptableObject, IHaveUid, IHaveDictionaries
    {
        public List<LootBoxItemsSettings> itemsSettings = new List<LootBoxItemsSettings>();

        [HideInInspector] public int menubarIndex;

        public int ForcedItems => itemsSettings.Sum(x => x.CountItemsToSpawn(true));
        public int PotentialItems => itemsSettings.Sum(x => x.CountItemsToSpawn());
        public int MaxItems => itemsSettings.Sum(x => x.CountItemsToSpawn()) + ForcedItems;
        
        public bool ContainsLootItems(LootItems value) 
            => itemsSettings.FirstOrDefault(x => x.lootItems == value) != null;
        
        /// <summary>
        /// Return a GameItemObjectList with generated results from this Loot Box.
        /// </summary>
        /// <returns></returns>
        public virtual GameItemObjectList GenerateLoot(IHandleLootBoxes handler)
        {
            var newList = new GameItemObjectList();
            
            // If we have a handler, use it to generate the items. Otherwise, use the default GenerateItems method.
            newList.list.AddRange(handler != null
                ? itemsSettings.SelectMany(handler.GenerateItems).ToList()
                : itemsSettings.SelectMany(GenerateItems).ToList());
            
            return newList;
        }
        
        public virtual GameItemObjectList GenerateLoot() => GenerateLoot(null);
        
        
        // ---------------------------------------------------------------------------------------
        // OTHER METHODS
        // ---------------------------------------------------------------------------------------

        // This will generate items from each LootBoxItemsSettings attached to this LootBox
        // This is the default generation handler method.
        public virtual IEnumerable<GameItemObject> GenerateItems(LootBoxItemsSettings itemsSetting)
        {
            var generatedItems = new List<GameItemObject>();

            var totalItemsToSpawn = itemsSetting.itemsToSpawn.Count;
            for (var i = 0; i < totalItemsToSpawn; i++)
            {
                var itemToSpawn = itemsSetting.itemsToSpawn[i];
                itemToSpawn.ResetDictionary();
                
                // Determine if we will generate this itemToSpawn -- if not, either return the list or continue to the next.
                var spawnChance = SpawnChance(i, itemsSetting);
                
                // If there is not a 100% chance, and RandomFloat() is greater than the chances, then we do not spawn.
                if (spawnChance < 1f && RandomFloat() > spawnChance) // CUSTOM CODE ZONE
                {
                    if (itemsSetting.stopAfterFirstFailure)
                        return generatedItems;
                    
                    continue;
                }
                
                // Determine the Item itself & create the object & add any Attributes
                var newItem = itemsSetting.GenerateItemObject(itemToSpawn);
                var newGameItemObject = new GameItemObject(newItem)
                {
                    attributeList = GenerateAttributes(itemsSetting, newItem, itemToSpawn)
                };
                
                // Reset the human name
                newGameItemObject.FullName(true);

                // Add the new item to the list
                generatedItems.Add(newGameItemObject);
            }
            
            return generatedItems;
        }
        
        // This will return the chances that any given item will spawn, based on it's position in the list and
        // the itemsSettings values set in the Inspector
        public virtual float SpawnChance(int itemToSpawnIndex, LootBoxItemsSettings itemsSetting)
        {
            if (itemsSetting.itemsToSpawn[itemToSpawnIndex].force)
                return 1f;
            
            var forcedItems = itemsSetting.itemsToSpawn.Count(x => x.force);

            var percentInList = (float)(itemToSpawnIndex - forcedItems) 
                                / (itemsSetting.itemsToSpawn.Count - forcedItems);

            return Mathf.Clamp(itemsSetting.ValueFromCurve(percentInList), 0f, 1f);
        }


        /// <summary>
        /// This will return a GameItemAttributeList with all the ItemAttributes generated for an individual ItemObject
        /// </summary>
        /// <param name="itemsSetting"></param>
        /// <param name="newItemObject"></param>
        /// <param name="itemToSpawn"></param>
        /// <returns></returns>
        public GameItemAttributeList GenerateAttributes(LootBoxItemsSettings itemsSetting, ItemObject newItemObject, LootBoxItemToSpawn itemToSpawn)
        {
            // Create a new GameItemAttributeList and set up the order the attributes will be in, as it may be random.
            var newItemAttributeList = new GameItemAttributeList();
            var newAttributeOrder = GenerateAttributeOrder(itemsSetting); 
            
            newItemAttributeList = AddForcedAttributes(itemsSetting, newItemObject, itemToSpawn, newItemAttributeList, newAttributeOrder);
            
            if (newAttributeOrder.Count == 0) 
                return newItemAttributeList; 

            if (itemsSetting.randomizeAttributeOrder)
                newAttributeOrder.Shuffle(); 

            newItemAttributeList = AddRandomAttributes(itemsSetting, newItemObject, itemToSpawn, newItemAttributeList, newAttributeOrder);

            return newItemAttributeList;
        }
        
        // Method for generating initial attribute order
        private List<string> GenerateAttributeOrder(LootBoxItemsSettings itemsSetting) 
        {
            var newAttributeOrder = itemsSetting.activeAttributeTypes.Clone();
            
            if (!itemsSetting.randomizeAttributeOrder) return newAttributeOrder;
            
            // Randomize the list
            var skipIndex = itemsSetting.attributeForce.Count(x => x);
            newAttributeOrder.Shuffle(skipIndex);

            return newAttributeOrder;
        }

        // Method for adding forced attributes
        private GameItemAttributeList AddForcedAttributes(LootBoxItemsSettings itemsSetting, ItemObject newItemObject, LootBoxItemToSpawn itemToSpawn, GameItemAttributeList newItemAttributeList, List<string> newAttributeOrder)
        {
            for (var forcedAttribute = 0; forcedAttribute < itemsSetting.attributeForce.Count; forcedAttribute++) 
            {
                // Break if we are no longer forcing attributes. All "True" values are at the start of the list.
                if (!itemsSetting.attributeForce[forcedAttribute]) break;
                
                var newAttribute = itemsSetting.AttributeFromType(itemsSetting.activeAttributeTypes[forcedAttribute], itemToSpawn, newItemObject); 
                
                if (string.IsNullOrWhiteSpace(newAttribute)) continue; 
                
                newItemAttributeList.Add(newAttribute); 
                newAttributeOrder.Remove(itemsSetting.activeAttributeTypes[forcedAttribute]); 
            }

            return newItemAttributeList;
        }

        // Method for adding random attributes
        private GameItemAttributeList AddRandomAttributes(LootBoxItemsSettings itemsSetting, ItemObject newItemObject, LootBoxItemToSpawn itemToSpawn, GameItemAttributeList newItemAttributeList, IReadOnlyList<string> newAttributeOrder)
        {
            for (var i = 0; i < newAttributeOrder.Count; i++)
            {
                var attributeObjectType = newAttributeOrder[i];
                var percentInList = newAttributeOrder.Count == 1 
                    ? 0 
                    : (float)i / (newAttributeOrder.Count - 1); // Cast i to float, -1 to zero-index it

                var randomFloat = RandomFloat();

                // If RandomFloat() is greater, then the attribute will NOT be added
                if (randomFloat > itemsSetting.ValueFromAttributeCurve(percentInList))
                {
                    if (itemsSetting.stopAfterAttributeFailure) return newItemAttributeList; 
                    continue;
                }

                var newAttribute = itemsSetting.AttributeFromType(attributeObjectType, itemToSpawn, newItemObject); 
                if (string.IsNullOrWhiteSpace(newAttribute)) continue; 
                
                newItemAttributeList.Add(newAttribute); 
            }

            return newItemAttributeList;
        }
        
        // EDITOR / INSPECTOR
        [HideInInspector] public bool hasBeenSetup;
        
        // Adds a lootItems object to the list
        public void AddLootItems(LootItems lootItems)
        {
            var newLootBoxItemsSettings = new LootBoxItemsSettings {lootItems = lootItems};

            var keyStart90 = new Keyframe(0, 0.9f);  // Keyframe at time=0 with value= 90%
            var keyEnd10 = new Keyframe(1, 0.1f);  // Keyframe at time=1 with value= 10%

            // Create a new AnimationCurve for each setting with the correct keyframes
            newLootBoxItemsSettings.itemChances = new AnimationCurve(keyStart90, keyEnd10);
            newLootBoxItemsSettings.attributeChances = new AnimationCurve(keyStart90, keyEnd10);
    
            itemsSettings.Add(newLootBoxItemsSettings);
        }
    }
}