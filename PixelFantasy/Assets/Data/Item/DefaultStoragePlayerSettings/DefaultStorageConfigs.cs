using System.Collections.Generic;
using Managers;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;

namespace Data.Item.DefaultStoragePlayerSettings
{
    [CreateAssetMenu(fileName = "DefaultStorageSettings", menuName = "Settings/DefaultStorageSettings", order = 0)] 
    public class DefaultStorageConfigs : SerializedScriptableObject
    {
        [OdinSerialize] private StorageConfigs _defaultConfigs;

        public StorageConfigs StorageConfigs => _defaultConfigs;
        private Dictionary<EItemCategory, List<AllowedStorageEntry>> Options => _defaultConfigs.StorageOptions.Options;
        
        [Button("UpdateListOfOptions")]
        private void UpdateListOfOptions() 
        {
            var upToDateItems = Librarian.Instance.GetAllItemSettings();
            foreach (var potentialItem in upToDateItems)
            {
                if (potentialItem.CanBeStored) 
                {
                    if (!Options.ContainsKey(potentialItem.Category))
                    {
                        Options.Add(potentialItem.Category, new List<AllowedStorageEntry>());
                    }
                    
                    var currentValues = Options[potentialItem.Category];
                    List<ItemSettings> allCurrentItemSettings = new List<ItemSettings>();
                    foreach (var currentEntry in currentValues)
                    {
                        allCurrentItemSettings.Add(currentEntry.Item);
                    }

                    if (!allCurrentItemSettings.Contains(potentialItem))
                    {
                        bool autoAllowed = !_defaultConfigs.StorageOptions.AreAllInCategoryNotAllowed(potentialItem.Category);
                        var newEntry = new AllowedStorageEntry(potentialItem, autoAllowed);
                        Options[newEntry.Category].Add(newEntry);
                    }
                }
            }
        } 
    }
}
