using System.Collections.Generic;
using System.Linq;
using InfinityPBR.Modules.Inventory;
using UnityEngine;
using static InfinityPBR.Modules.GameModuleUtilities;

namespace InfinityPBR.Modules
{
    [System.Serializable]
    public class GameModuleRepository : ScriptableObjectSingleton<GameModuleRepository>
    {
        public List<RepositoryList> repositories = new List<RepositoryList>();
        public Dictionary<string, Dictionary<string, ModulesScriptableObject>> dictionary = new Dictionary<string, Dictionary<string, ModulesScriptableObject>>();
        public Dictionary<string, List<ModulesScriptableObject>> dictionaryByType = new Dictionary<string, List<ModulesScriptableObject>>();
        public Dictionary<string, List<ModulesScriptableObject>> dictionaryAllByModuleType = new Dictionary<string, List<ModulesScriptableObject>>();
        public Dictionary<string, List<string>> dictionaryObjectTypes = new Dictionary<string, List<string>>();

        // Stats
        public List<Stat> statModifiable = new List<Stat>();
        public List<Stat> statTrainable = new List<Stat>();


        /// <summary>
        /// Returns the item with the given uid. If it is not found, it will return null.
        /// </summary>
        /// <param name="uidOrName"></param>
        /// <returns></returns>
        public T Get<T>(string uidOrName) where T : ModulesScriptableObject
        {
            if (string.IsNullOrWhiteSpace(uidOrName))
            {
                Debug.LogError("GameModulesRepository: Attempting to get an object with null or empty uid or name string. That is not allowed");
                return default;
            }

            var typeName = typeof(T).Name;
            
            // First try to get the item from the Dictionary
            if (TryGetValue<T>(uidOrName, typeName, out var found)) return found;

            // If we couldn't find it, try to add it, and return that.
            if (TryAddItem<T>(uidOrName, typeName, out var newEntry)) return newEntry;
            
            Debug.LogWarning($"GameModulesRepository Warning: Could not find an item of type {typeName} " +
                             $"with uid or name {uidOrName}. Comment out this Debug.Log if you do not want to be warned.");
            return default;
        }

        public List<T> GetAll<T>() where T : ModulesScriptableObject
        {
            var typeName = typeof(T).Name;
    
            // Check if we've already fetched this type and return it if found
            if (dictionaryAllByModuleType.TryGetValue(typeName, out var typeEntries))
                return typeEntries.Cast<T>().ToList();
        
            var results = new List<ModulesScriptableObject>();
            foreach (var repo in repositories.Where(repo => repo.typeName == typeName))
                results.AddRange(repo.items.Select(x => x.gameModulesObject));

            // Cache results in the dictionary
            var resultList = results.OfType<T>().ToList();
            if(resultList.Count > 0)
                dictionaryAllByModuleType[typeName] = resultList.Cast<ModulesScriptableObject>().ToList();

            return resultList;
        }

        public List<string> GetObjectTypes<T>() where T : ModulesScriptableObject
        {
            var typeName = typeof(T).Name;
            if (dictionaryObjectTypes.TryGetValue(typeName, out var objectTypes))
                return objectTypes;

            Debug.LogError($"There was no key for {typeName} in the dictionaryObjectTypes. This should not happen.");
            return null;
        }

        
        public List<T> GetByObjectType<T>(string objectType) where T : ModulesScriptableObject
        {
            string key = $"{typeof(T).Name}/{objectType}";

            // Check if we've already fetched this type and return it if found
            if (dictionaryByType.TryGetValue(key, out var typeEntries))
                return typeEntries.OfType<T>().ToList();

            // If we couldn't find it, fetch it from the repositories, populate the dictionary, and return it
            var fetchedItems = repositories
                .Where(repo => repo.typeName == typeof(T).Name)
                .SelectMany(repo => repo.items)
                .Where(item => item.gameModulesObject.objectType == objectType)
                .Select(item => item.gameModulesObject)
                .OfType<T>()
                .ToList();

            if (fetchedItems.Count > 0)
                dictionaryByType[key] = fetchedItems.Cast<ModulesScriptableObject>().ToList();
        
            return fetchedItems;
        }
        
        private bool TryGetValue<T>(string uidOrName, string typeName, out T found) where T : ModulesScriptableObject
        {
            if (dictionary.ContainsKey(typeName))
            {
                if (dictionary[typeName].TryGetValue(uidOrName, out ModulesScriptableObject value) && value is T)
                {
                    found = (T)value;
                    return true;
                }
            }
            found = null;
            return false;
        }

        private bool TryAddItem<T>(string uidOrName, string typeName, out T newEntry) where T : ModulesScriptableObject
        {
            // Grab the Repository List of this type
            var repositoryList = GetRepositoryList(typeName);
            if (repositoryList == null)
            {
                Debug.LogError($"GameModulesRepository Error: No Repository List found with type {typeName}");
                newEntry = null;
                return false;
            }
            
            // Grab the repositoryItems of this type
            var repositoryItems = GetRepositoryItems(repositoryList, uidOrName);
            if (repositoryItems == null)
            {
                Debug.LogError($"GameModulesRepository Error: No Repository Item found with type {typeName} and uid or name {uidOrName}");
                newEntry = null;
                return false;
            }
            
            // Ensure the first Dictionary is set
            if (!dictionary.ContainsKey(typeName))
                dictionary[typeName] = new Dictionary<string, ModulesScriptableObject>();
            
            // Add entries to the Dictionary for both the uid and name
            dictionary[typeName][repositoryItems.uid] = repositoryItems.gameModulesObject;
            dictionary[typeName][repositoryItems.name] = repositoryItems.gameModulesObject;
            
            // Return the value and true
            newEntry = (T)repositoryItems.gameModulesObject;
            return true;
        }
        
        private RepositoryItems GetRepositoryItems(RepositoryList repositoryList, string uidOrName) 
            => repositoryList.items.Where(item 
                    => item.uid == uidOrName || item.name == uidOrName)
                .FirstOrDefault(item => item.uid != null && item.name != null);

        private RepositoryList GetRepositoryList(string typeName) 
            => repositories.FirstOrDefault(repositoryList => repositoryList.typeName == typeName);

        // ***********************************************************************************************************
        // ITEM OBJECTS
        // ***********************************************************************************************************
        
        /*
         * Displayable Inventory methods store the GameItemObjectList objects which are intended to be used in the
         * Inventory module. This Dictionary allows objects to reference the correct list at runtime, only needing to
         * store the gameId string that they are attached to.
         */
        private Dictionary<string, IHaveInventory> _displayableInventories = new Dictionary<string, IHaveInventory>();
        
        public void RegisterDisplayableInventory(IHaveInventory value) => _displayableInventories[value.GameId()] = value;

        public GameItemObjectList DisplayableInventory(string gameId) => _displayableInventories[gameId].DisplayableInventory();

        public Spots DisplayableSpots(string gameId) => _displayableInventories[gameId].DisplayableSpots();
        
        // ***********************************************************************************************************
        // ON VALIDATE AND AUTO SETUP
        // ***********************************************************************************************************

        protected override void Setup()
        {
#if UNITY_EDITOR
            PopulateAll();
#endif
        }
        
        // We populate the list automatically OnValidate(), so it's all just magical!
        protected virtual void OnValidate()
        {
#if UNITY_EDITOR
            PopulateAll();
#endif
        }

        public void PopulateAll()
        {
            #if UNITY_EDITOR
            Populate<Stat>();
            Populate<ItemObject>();
            Populate<ItemAttribute>();
            Populate<Condition>();
            Populate<MasteryLevels>();
            Populate<Quest>();
            Populate<QuestCondition>();
            Populate<QuestReward>();
            Populate<LootBox>();
            Populate<LootItems>();
            Populate<Voices>();
            Populate<LookupTable>();
            
            if (ObjectReference.Instance != null)
            {
                // August 5 2023 -- this turns out to cause some odd SendMessage warning, which seems to be a 
                // Unity thing. So, removing it. Really, there should be no missing references, so likely this
                // is not needed, adn the button is available in the Game Modules Manager.
                //Debug.Log("Populate missing references");
                //ObjectReference.Instance.PopulateMissingReferences();
            }
            
            PopulateSpecialStats();
            #endif
        }

        private void Populate<T>() where T : ModulesScriptableObject
        {
#if UNITY_EDITOR
            var repositoryList = AddToRepositoryList<T>();
            var objs = GameModuleObjects<T>(true);
            
            // Remove any that are in the list but not in the objects (i.e. they have been deleted)
            foreach (var repositoryItem in repositoryList.items
                         .Where(repositoryItem => !objs.Contains(repositoryItem.gameModulesObject)))
                repositoryList.items.Remove(repositoryItem);
            
            // Add all objects
            foreach (var obj in objs)
                AddToRepositoryItems<T>(repositoryList, obj);
            
            // Update object type cache
            var typeName = typeof(T).Name;
            if (!dictionaryObjectTypes.ContainsKey(typeName))
                dictionaryObjectTypes[typeName] = new List<string>();
            else
                dictionaryObjectTypes[typeName].Clear(); // Clear the list if it already exists
            dictionaryObjectTypes[typeName].AddRange(objs.Select(o => o.objectType).Distinct());
#endif
        }
        
        public void PopulateSpecialStats()
        {
            statModifiable = GetAll<Stat>()
                .Where(x => x.canBeModified)
                .ToList();
            
            statTrainable = GetAll<Stat>()
                .Where(x => x.canBeTrained)
                .ToList();
        }
        
        public RepositoryList AddToRepositoryList<T>() where T : ModulesScriptableObject
        {
            var typeName = typeof(T).Name;
            var repositoryList = GetRepositoryList(typeName);
            if (repositoryList != null) return repositoryList;
            
            repositoryList = new RepositoryList {typeName = typeName};
            repositories.Add(repositoryList);

            return repositoryList;
        }

        public RepositoryItems AddToRepositoryItems<T>(RepositoryList repositoryList,
            ModulesScriptableObject gameModulesObject) where T : ModulesScriptableObject
        {
            var repositoryItems = GetRepositoryItems(repositoryList, gameModulesObject.ObjectName);
            if (repositoryItems != null) return repositoryItems;
            
            repositoryItems = new RepositoryItems
            {
                uid = gameModulesObject.Uid(),
                name = gameModulesObject.ObjectName,
                gameModulesObject = gameModulesObject,
                objectType =  gameModulesObject.ObjectType
            };
            repositoryList.items.Add(repositoryItems);

            return repositoryItems;
        }
    }
}