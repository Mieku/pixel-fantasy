using System.Collections.Generic;
using Managers;
using ScriptableObjects;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "DefaultStorageSettings", menuName = "Settings/DefaultStorageSettings", order = 0)] 
public class DefaultStorageConfigs : SerializedScriptableObject
{
    [OdinSerialize] private StorageConfigs _defaultConfigs;

    public StorageConfigs StorageConfigs => _defaultConfigs;
    private Dictionary<EItemCategory, List<AllowedStorageEntry>> Options => _defaultConfigs.StorageOptions.Options;

    [Button("Clear Options")]
    private void ClearOptions()
    {
        _defaultConfigs.StorageOptions.Options.Clear();
    }
        
    [Button("UpdateListOfOptions")]
    private void UpdateListOfOptions()
    {
        var upToDateItems = GameSettings.LoadAllSettings<ItemSettings>();
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
