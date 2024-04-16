using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static InfinityPBR.Modules.LootBoxRepository;
using static InfinityPBR.Modules.Utilities;

/*
 * LOOT BOX MANAGER
 *
 * IMPORTANT: You can attach this script to your box (or other object that has a "Loot Box" like enemies etc), or if you
 * prefer to customize any of the code, create your own script which inherits from LootBoxManager.cs and then override
 * the appropriate methods. Note the Demo Scene does this.
 *
 * The methods below which are public virtual all have a "CUSTOM CODE ZONE" comment in them, identifying them as methods
 * which we think you may want to override.
 */

/*
 * DESCRIPTIONS OF METHODS THAT CAN BE OVERRIDDEN
 *
 * GetGeneratedLoot()
 * This starts the process of generating the loot, and will return the final List<Item>
 *
 * GenerateItems()
 * This is the main item generation method, called once for each item the script will attempt to generate
 *
 * GetRandomFloat()
 * Returns a random float, default from 0f to 1f. You may wish to augment or weigh the chances at this point.
 *
 * SpawnChance()
 * This is what decides whether the item is spawned or not, based on the selections and chance curve. Here you can
 * weigh the possibilities with external factors unique to your game. (Example. Player "Luck" may increase the chance
 * of a good item being spawned)
 *
 * HandleLoot()
 * This is called after the entire generation process is complete, allowing you to do something with the generated
 * List<Item>. What you do is entirely based on your project.
 * 
 */

namespace InfinityPBR.Modules
{
    public class OLDLootBoxManager : MonoBehaviour, IHaveLoot
    {
        
        [Header("Required")] 
        public string lootBoxUid;
        
        public LootBox LootBox => GetLootBox();
        private LootBox _lootBox;

        [Header("Options")] 
        public bool generateLootOnAwake = true;

        [Header("Generated Loot List")] 
        public GameItemObjectList generatedLoot;
        
        // Private
        private bool _generated;
        private IHaveLoot haveLootImplementation;

        private void Awake()
        {
            
        }

        public void Update()
        {
            if (!generateLootOnAwake) return;
            if (_generated) return;
            GenerateLoot();
        }

        private void ClearCurrentList()
        {
            generatedLoot.Clear();
        }
        
        public void GenerateLoot(bool overwrite = false)
        {
            if (!lootBoxRepository) return;
            if (!LootBox) return;
            if (!overwrite && generatedLoot.Count() > 0)
                return;

            _generated = true;
            
            ClearCurrentList();
            generatedLoot = GetGeneratedLoot();
  
            HandleLoot();
        }
        
        /*
         * ------------------------------------------------------------------------------------------
         * OVERRIDE THESE METHODS IF YOU WOULD LIKE TO CUSTOMIZE THE CODE!
         * ------------------------------------------------------------------------------------------
         */

        public virtual GameItemObjectList GetGeneratedLoot()
        {
            /*
            * CUSTOM CODE ZONE
            * Override this to customize it!
            */

            GameItemObjectList newList = new GameItemObjectList();
            newList.list.AddRange(LootBox.itemsSettings.SelectMany(GenerateItems).ToList());
            return newList;
        }

        public virtual IEnumerable<GameItemObject> GenerateItems(LootBoxItemsSettings itemsSetting)
        {
            /*
            * CUSTOM CODE ZONE
             * Override this to customize it!
            */
            
            List<GameItemObject> generatedItems = new List<GameItemObject>();
            for (int i = 0; i < itemsSetting.itemsToSpawn.Count; i++)
            {
                LootBoxItemToSpawn itemToSpawn = itemsSetting.itemsToSpawn[i];
                
                // Determine if we will generate this itemToSpawn -- if not, either return the list or continue to the next.
                float spawnChance = SpawnChance(i, itemsSetting);
                if (spawnChance < 1f && RandomFloat() > spawnChance) // CUSTOM CODE ZONE
                {
                    if (itemsSetting.stopAfterFirstFailure)
                        return generatedItems;
                    
                    continue;
                }
                
                // Determine the Item itself & create the object
                ItemObject newItem = itemsSetting.GenerateItemObject(itemToSpawn);
                GameItemObject newGameItemObject = new GameItemObject(newItem);

                // Add any Attributes
                Debug.Log("Getting Attributes");
                //newGameItemObject.attributes = GenerateAttributes(newItem, itemToSpawn);
                newGameItemObject.FullName(true);

                // Add the new item to the list
                generatedItems.Add(newGameItemObject);
            }
            
            return generatedItems;
        }

        public virtual float SpawnChance(int itemToSpawnIndex, LootBoxItemsSettings itemsSetting)
        {
            if (itemsSetting.itemsToSpawn[itemToSpawnIndex].force)
                return 1f;
            
            int forcedItems = itemsSetting.CountItemsToSpawn(true);
            
            float thisPosition = itemToSpawnIndex - forcedItems;
            float endPosition = itemsSetting.itemsToSpawn.Count - forcedItems;

            float percentInList = thisPosition / endPosition;


            return Mathf.Clamp(itemsSetting.ValueFromCurve(percentInList), 0f, 1f);
        }

        public virtual void HandleLoot()
        {
           /*
            * CUSTOM CODE ZONE
            *
            * Now, you have a list of Items. What are you going to do with it??
            *
            * This portion is entirely up to you and your project. In the demo, I'm using the Inventory module, to
            * put these items into the actual "Box" using that module. In some cases, not all of items we just
            * generated will even fit!
            *
            * But, your system may not be using the Inventory module, and if that is the case, then you'll want to
            * remove the code below, and write your own system to handle the List<Item> that was just populated.
            *
            * --THIS IS A METHOD YOU SHOULD OVERRIDE FROM YOUR OWN SCRIPT WHICH INHERITS FROM LootBoxManager!!!!!--
            */
        }

        /// <summary>
        /// This will return the selected _lootBox, or populate it from the lootBoxUid if it has not already been pouplated
        /// </summary>
        /// <returns></returns>
        private LootBox GetLootBox()
        {
            if (!_lootBox)
                return LinkLootBox();

            return _lootBox;
        }

        /// <summary>
        /// This wil link the _lootBox with the LootBox scriptable object via the control object
        /// </summary>
        /// <returns></returns>
        private LootBox LinkLootBox()
        {
            if (!HasUid()) return null;

            if (!lootBoxRepository)
            {
                Debug.LogError("Did you forget to add the LootBoxes Repository to the scene?");
                return null;
            }
            
            _lootBox = lootBoxRepository.GetByUid(lootBoxUid);
            return _lootBox;
        }
        
        /// <summary>
        /// Return false, and gives an editor error if the uid is not populated as that is required.
        /// </summary>
        /// <returns></returns>
        public bool HasUid()
        {
            if (!String.IsNullOrWhiteSpace(lootBoxUid)) return true;

            Debug.LogError("You need to assign the uid value for this Loot Box on " + gameObject.name);
            
            return false;
        }

        public string ObjectId()
        {
            throw new NotImplementedException();
        }

        public void Tick()
        {
            throw new NotImplementedException();
        }

        public void LateTick()
        {
            throw new NotImplementedException();
        }

        public string GetOwnerName()
        {
            throw new NotImplementedException();
        }

        public void SetOwner(object newOwner)
        {
            throw new NotImplementedException();
        }

        public string GameId(bool forceNew = false)
        {
            return haveLootImplementation.GameId(forceNew);
        }
    }
}
