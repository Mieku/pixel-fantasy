using System;
using System.Collections.Generic;
using System.Linq;
using InfinityPBR.Modules.Inventory;
using UnityEngine;
using static InfinityPBR.Modules.Utilities;

namespace InfinityPBR.Modules
{
    public class ItemObjectRepository : Repository<ItemObject>
    {
        // Static reference to this script, set in Awake, there can be only one
        public static ItemObjectRepository itemObjectRepository;

        /*
         * Displayable Inventory methods store the GameItemObjectList objects which are intended to be used in the
         * Inventory module. This Dictionary allows objects to reference the correct list at runtime, only needing to
         * store the gameId string that they are attached to.
         */
        private Dictionary<string, IHaveInventory> _displayableInventories = new Dictionary<string, IHaveInventory>();

        [Obsolete("This method is deprecated, please use the corresponding method from GameModulesRepository.Instance instead.", false)]
        public void RegisterDisplayableInventory(IHaveInventory value)
        {
            Debug.LogWarning("RegisterDisplayableInventory is obsolete. Please use the corresponding method from GameModulesRepository.Instance instead.");
            _displayableInventories[value.GameId()] = value;
        }

        [Obsolete("This method is deprecated, please use the corresponding method from GameModulesRepository.Instance instead.", false)]
        public GameItemObjectList DisplayableInventory(string gameId) => GameModuleRepository.Instance.DisplayableInventory(gameId);
        //{
            //Debug.LogWarning("DisplayableInventory is obsolete. Please use the corresponding method from GameModulesRepository.Instance instead.");
            //return _displayableInventories[gameId].DisplayableInventory();
       // }

        [Obsolete(
            "This method is deprecated, please use the corresponding method from GameModulesRepository.Instance instead.",
            false)]
        public Spots DisplayableSpots(string gameId) => GameModuleRepository.Instance.DisplayableSpots(gameId);
        //{
        //    Debug.LogWarning("DisplayableSpots is obsolete. Please use the corresponding method from GameModulesRepository.Instance instead.");
       //     return _displayableInventories[gameId].DisplayableSpots();
        //}


        private void Awake()
        {
            if (!itemObjectRepository)
                itemObjectRepository = this;
            else if (itemObjectRepository != this)
                Destroy(gameObject);
        }
        
        // These are the unique lists etc used in the Editor script to easily show the data in the
        // Inspector when the Repository is selected.
        [Header("Auto populated")] 
        public List<string> itemObjectTypes = new List<string>();
        public Dictionary<string, List<ItemObject>> itemObjectsByType = new Dictionary<string, List<ItemObject>>();
        
        // Each Repository will need to have it's own PopulateList() method, as each will
        // do things different from the others.
        public override void PopulateList()
        {
#if UNITY_EDITOR
            scriptableObjects.Clear();
            itemObjectTypes.Clear();
            itemObjectsByType.Clear();

            scriptableObjects = GetItemObjectArray()
                .OrderBy(x => x.objectType)
                .ThenBy(x => x.objectName)
                .ToList();
            
            itemObjectTypes = scriptableObjects
                .Select(x => x.objectType)
                .Distinct()
                .ToList();

            foreach (string objectType in itemObjectTypes)
            {
                var objectsByObjectType = scriptableObjects
                    .Where(x => x.objectType == objectType)
                    .ToList();

                itemObjectsByType.Add(objectType, objectsByObjectType);
            }
#endif
        }
    }
}
