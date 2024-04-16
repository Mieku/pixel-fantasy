using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static InfinityPBR.Modules.Utilities;

/*
 * v3.6 Update: This one is more tricky! Prior to this update, ItemObject and ItemAttribute objects
 * were both included in the ItemsRepository. After, we will have an ItemObjectRepository, and an
 * ItemAttributeRepository.
 *
 * I plan on leaving this in the project, as removing it will break things quite a bit, and advising folks
 * to migrate their code over, since the class names will be new.
 */

namespace InfinityPBR.Modules
{
    public class ItemsRepository : Repository<ItemObject>
    {
        // Static reference to this script, set in Awake, there can be only one
        public static ItemsRepository itemsRepository;
        
        private void Awake()
        {
            if (!itemsRepository)
                itemsRepository = this;
            else if (itemsRepository != this)
                Destroy(gameObject);
        }
        
        public ItemObject GetItemObjectByUid(string uid) => ItemObjectByUid(uid);
        public ItemAttribute GetItemAttributeByUid(string uid) => ItemAttributeByUid(uid);
        public GameItemObject CreateGameItemObject(string uid) => CreateGameItemObjectByUid(uid);
        
        [Header("Auto populated")] 
        public List<ItemObject> itemObjects = new List<ItemObject>();
        public List<ItemAttribute> itemAttributes = new List<ItemAttribute>();
        public List<string> itemObjectTypes = new List<string>();
        public List<string> itemAttributeTypes = new List<string>();
        public Dictionary<string, List<ItemObject>> itemObjectsByType = new Dictionary<string, List<ItemObject>>();
        public Dictionary<string, List<ItemAttribute>> itemAttributesByType = new Dictionary<string, List<ItemAttribute>>();

        private readonly Dictionary<string, ItemObject> itemObjectsByUid = new Dictionary<string, ItemObject>();
        private readonly Dictionary<string, ItemAttribute> itemAttributesByUid = new Dictionary<string, ItemAttribute>();

        // ------------------------------------------------------------------------------------------
        // PUBLIC METHODS
        // ------------------------------------------------------------------------------------------

        
        
        // ------------------------------------------------------------------------------------------
        // PRIVATE METHODS
        // ------------------------------------------------------------------------------------------


        private GameItemObject CreateGameItemObjectByUid(string uid)
        {
            return new GameItemObject(GetItemObjectByUid(uid));
        }

        
        private void PopulateDictionaries()
        {
            itemObjectsByType.Clear();
            itemAttributesByType.Clear();
            foreach (string itemType in itemObjectTypes)
                itemObjectsByType.Add(itemType, itemObjects.Where(x => x.objectType == itemType).ToList());
            
            foreach (string itemType in itemAttributeTypes)
                itemAttributesByType.Add(itemType, itemAttributes.Where(x => x.objectType == itemType).ToList());
            
        }

        private ItemObject ItemObjectByUid(string uid)
        {
            Debug.Log("v3.6 separates the ItemsRepository into ItemObjectRepository and ItemAttributeRepository. " +
                      "Please update your code to query those. ItemsRepository will be removed in a future update. " +
                      "Thank you!");
            if (itemObjectsByUid.TryGetValue(uid, out var found)) return found;
            
            if (TryAddItemObjectByUid(uid, out var newEntry)) return newEntry;

            Debug.LogWarning($"Warning: Could not find an ItemObject object with uid {uid}. Comment out this Debug.Log if you do not want to be warned.");
            return default;
        }
        
        private ItemAttribute ItemAttributeByUid(string uid)
        {
            Debug.Log("v3.6 separates the ItemsRepository into ItemObjectRepository and ItemAttributeRepository. " +
                      "Please update your code to query those. ItemsRepository will be removed in a future update. " +
                      "Thank you!");
            if (itemAttributesByUid.TryGetValue(uid, out var found)) return found;
            
            if (TryAddItemAttributeByUid(uid, out var newEntry)) return newEntry;

            Debug.LogWarning($"Warning: Could not find an ItemAttribute object with uid {uid}. Comment out this Debug.Log if you do not want to be warned.");
            return default;
        }

        private bool TryAddItemObjectByUid(string uid, out ItemObject newEntry)
        {
            bool found = false;
            foreach(ItemObject itemObject in itemObjects)
            {
                if (itemObject.Uid() != uid) continue;
                itemObjectsByUid.Add(uid, itemObject);
                found = true;
                break;
            }
            
            if (!found)
            {
                newEntry = null;
                return false;
            }
            
            newEntry = itemObjectsByUid[uid];
            return newEntry != null;
        }

        private bool TryAddItemAttributeByUid(string uid, out ItemAttribute newEntry)
        {
            bool found = false;
            foreach(ItemAttribute itemAttribute in itemAttributes)
            {
                if (itemAttribute.Uid() != uid) continue;
                itemAttributesByUid.Add(uid, itemAttribute);
                found = true;
                break;
            }
            
            if (!found)
            {
                newEntry = null;
                return false;
            }
            
            newEntry = itemAttributesByUid[uid];
            return newEntry != null;
        }
        
        public override void PopulateList()
        {
#if UNITY_EDITOR
            itemAttributes.Clear();
            itemObjects.Clear();
            itemAttributeTypes.Clear();
            itemObjectTypes.Clear();
            itemObjectsByType.Clear();
            itemAttributesByType.Clear();
            
            itemObjects = GetItemObjectArray()
                .OrderBy(x => x.objectType)
                .ThenBy(x => x.objectName)
                .ToList();
            
            itemAttributes = GetItemAttributeArray()
                .OrderBy(x => x.objectType)
                .ThenBy(x => x.objectName)
                .ToList();

            itemObjectTypes = itemObjects
                .Select(x => x.objectType)
                .Distinct()
                .ToList();
            
            itemAttributeTypes = itemAttributes
                .Select(x => x.objectType)
                .Distinct()
                .ToList();

            foreach (string objectType in itemObjectTypes)
            {
                var objectsByObjectType = itemObjects
                    .Where(x => x.objectType == objectType)
                    .ToList();

                itemObjectsByType.Add(objectType, objectsByObjectType);
            }
            
            foreach (string objectType in itemAttributeTypes)
            {
                var objectsByObjectType = itemAttributes
                    .Where(x => x.objectType == objectType)
                    .ToList();

                itemAttributesByType.Add(objectType, objectsByObjectType);
            }
#endif
        }

        
    }
}
