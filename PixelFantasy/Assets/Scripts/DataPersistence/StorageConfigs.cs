using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Managers;
using Newtonsoft.Json;
using ScriptableObjects;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
public class StorageConfigs : ICopyPasteConfig
{
    [field: SerializeField]
    public EConfigType ConfigType { get; protected set; }
        
    // Priority for storage
    [EnumPaging]
    public EUsePriority UsePriority;
        
    // Durability Range
    [MinMaxSlider(0, 100, true)]
    public Vector2Int DurabilityRange;
    [JsonIgnore] public int DurabilityMin => DurabilityRange.x;
    [JsonIgnore] public int DurabilityMax => DurabilityRange.y;

    // Quality Range
    [MinMaxSlider(0, 4, true)]
    public Vector2Int QualityRange;
        
    [JsonIgnore] public EItemQuality QualityMin => (EItemQuality)QualityRange.x;
    [JsonIgnore] public EItemQuality QualityMax => (EItemQuality)QualityRange.y;

    // Allowed Items
    [OdinSerialize] public AllowedStorageOptions StorageOptions = new AllowedStorageOptions();

    /// <summary>
    /// Updates settings to be the same
    /// </summary>
    public void PasteConfigs(ICopyPasteConfig otherConfigs)
    {
        if (ConfigType == otherConfigs.ConfigType)
        {
            var storageConfigs = (StorageConfigs) otherConfigs;
            ConfigType = storageConfigs.ConfigType;
            UsePriority = storageConfigs.UsePriority;
            DurabilityRange = storageConfigs.DurabilityRange;
            QualityRange = storageConfigs.QualityRange;
            StorageOptions = new AllowedStorageOptions(storageConfigs.StorageOptions);
        }
    }

    public void PasteConfigs(DefaultStorageConfigs defaultSettings)
    {
        PasteConfigs(defaultSettings.StorageConfigs);
    }

    public bool IsItemTypeAllowed(ItemSettings itemSettings)
    {
        return StorageOptions.IsItemTypeAllowed(itemSettings);
    }

    public bool IsItemValidToStore(ItemData itemData)
    {
        if (!IsItemTypeAllowed(itemData.Settings)) return false;

        var durability = itemData.DurabilityPercent;
        if (durability < DurabilityMin / 100f || durability > DurabilityMax / 100f) return false;

        var quality = (int)itemData.Quality;
        if (quality < (int)QualityMin || quality > (int)QualityMax) return false;

        return true;
    }
}
    

public enum EUsePriority
{
    [Description("Ignore")] Ignore = 0,
    [Description("Low")] Low = 1,
    [Description("Normal")] Normal = 2,
    [Description("Preferred")] Preferred = 3,
    [Description("Critical")] Critical = 4
}

[Serializable]
public class AllowedStorageOptions
{
    [OdinSerialize] public Dictionary<EItemCategory, List<AllowedStorageEntry>> Options =
        new Dictionary<EItemCategory, List<AllowedStorageEntry>>();

    public AllowedStorageOptions()
    {
        Options = new Dictionary<EItemCategory, List<AllowedStorageEntry>>();
    }

    public AllowedStorageOptions(AllowedStorageOptions optionsToClone)
    {
        Options = new Dictionary<EItemCategory, List<AllowedStorageEntry>>();
        foreach (var optionToClone in optionsToClone.Options)
        {
            List<AllowedStorageEntry> clonedEntries = new List<AllowedStorageEntry>();
            foreach (var entry in optionToClone.Value)
            {
                AllowedStorageEntry clonedEntry = new AllowedStorageEntry();
                clonedEntry.Init(entry.Item, entry.IsAllowed);
                clonedEntries.Add(clonedEntry);
            }
            Options.Add(optionToClone.Key, clonedEntries);
        }
    }

    public List<AllowedStorageEntry> SearchByName(string search)
    {
        return Options
            .SelectMany(pair => pair.Value)
            .Where(entry => entry.Item.ItemName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
            .ToList();
    }

    public bool IsItemTypeAllowed(ItemSettings itemSettings)
    {
        var cat = GetAllItemsByCategory(itemSettings.Category);
        var itemEntry = cat.Find(entry => entry.Item == itemSettings);
            
        if (itemEntry == null) return false;

        return itemEntry.IsAllowed;
    }
        
    [Button("UpdateListOfOptions")]
    private void UpdateListOfOptions()
    {
        var upToDateItems = GameSettings.Instance.LoadAllItemSettings();
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
                    bool autoAllowed = !AreAllInCategoryNotAllowed(potentialItem.Category);
                    var newEntry = new AllowedStorageEntry();
                    newEntry.Init(potentialItem, autoAllowed);
                    Options[newEntry.Category].Add(newEntry);
                }
            }
        }
    } 

    [Button("Update Allowed By Category")]
    public void UpdateAllowedByCategory(EItemCategory category, bool isAllowed)
    {
        var catOptions = GetAllItemsByCategory(category);
        foreach (var entry in catOptions)
        {
            entry.IsAllowed = isAllowed;
        }
    }

    [Button("Apply to all options")]
    public void ApplyToAllOptions(bool isAllowed)
    {
        var allCategories = Enum.GetValues(typeof(EItemCategory));
        foreach (var catObj in allCategories)
        {
            EItemCategory cat = (EItemCategory)catObj;
            UpdateAllowedByCategory(cat, isAllowed);
        }
    }

    public bool AreAllInCategoryAllowed(EItemCategory category)
    {
        var catOptions = GetAllItemsByCategory(category);
        foreach (var option in catOptions)
        {
            if (!option.IsAllowed)
            {
                return false;
            }
        }

        return true;
    }

    public bool AreAllInCategoryNotAllowed(EItemCategory category)
    {
        var catOptions = GetAllItemsByCategory(category);
        foreach (var option in catOptions)
        {
            if (option.IsAllowed)
            {
                return false;
            }
        }

        return true;
    }

    public List<AllowedStorageEntry> GetAllItemsByCategory(EItemCategory category)
    {
        if (!Options.ContainsKey(category))
        {
            Options.Add(category, new List<AllowedStorageEntry>());
        }

        return Options[category];
    }
}
    
[Serializable]
public class AllowedStorageEntry
{
    public string ItemSettingsID;
    public bool IsAllowed;
    [JsonIgnore] public EItemCategory Category => Item.Category;
    [JsonIgnore] public ItemSettings Item => GameSettings.Instance.LoadItemSettings(ItemSettingsID);
    
    public void Init(ItemSettings itemSettings, bool isAllowed = true)
    {
        ItemSettingsID = itemSettings.name;
        IsAllowed = isAllowed;
    }
}