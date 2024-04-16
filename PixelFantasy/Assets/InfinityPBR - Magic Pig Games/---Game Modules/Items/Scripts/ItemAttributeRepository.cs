using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static InfinityPBR.Modules.Utilities;

namespace InfinityPBR.Modules
{
    public class ItemAttributeRepository : Repository<ItemAttribute>
    {
        // Static reference to this script, set in Awake, there can be only one
        public static ItemAttributeRepository itemAttributeRepository;
        
        private void Awake()
        {
            if (!itemAttributeRepository)
                itemAttributeRepository = this;
            else if (itemAttributeRepository != this)
                Destroy(gameObject);
        }
        
        // These are the unique lists etc used in the Editor script to easily show the data in the
        // Inspector when the Repository is selected.
        [Header("Auto populated")]
        public List<string> itemAttributeTypes = new List<string>();
        public Dictionary<string, List<ItemAttribute>> itemAttributesByType = new Dictionary<string, List<ItemAttribute>>();

        [Obsolete("Use GameModulesRepository.Instance.GetByType<T>(string objectType) instead. This method will be removed in a future version.")]
        public List<ItemAttribute> GetByType(string objectType)
        {
            Debug.LogWarning("Deprecated method GetByType is being used. Please switch to using GameModulesRepository.Instance.GetByType<T>(string objectType).");
            return GameModuleRepository.Instance.GetByObjectType<ItemAttribute>(objectType).ToList();
        }


        // Each Repository will need to have it's own PopulateList() method, as each will
        // do things different from the others.
        public override void PopulateList()
        {
#if UNITY_EDITOR
            scriptableObjects.Clear();
            itemAttributeTypes.Clear();
            itemAttributesByType.Clear();

            scriptableObjects = GetItemAttributeArray()
                .OrderBy(x => x.objectType)
                .ThenBy(x => x.objectName)
                .ToList();
            
            itemAttributeTypes = scriptableObjects
                .Select(x => x.objectType)
                .Distinct()
                .ToList();

            foreach (string objectType in itemAttributeTypes)
            {
                var objectsByObjectType = scriptableObjects
                    .Where(x => x.objectType == objectType)
                    .ToList();

                itemAttributesByType.Add(objectType, objectsByObjectType);
            }
#endif
        }
    }
}
