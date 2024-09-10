using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ScriptableObjects
{
    [Serializable]
    public class SettingsCategories 
    {
        [ListDrawerSettings(DraggableItems = true)]
        public List<SettingsCategory> Categories = new List<SettingsCategory>();

        public List<T> GetAllSettingsByCategory<T> (ESettingsCategory requestedCategory)
        {
            foreach (var category in Categories)
            {
                if (category.Category == requestedCategory)
                {
                    return category.Settings.OfType<T>().ToList();
                }
            }

            Debug.LogError($"Unable to find category {requestedCategory}");
            return null;
        }

        [Button("Find All Missing Settings")]
        private void FindAllMissingSettings()
        {
            var allSettings = GameSettings.Instance.LoadAllSettings();
            foreach (var settings in allSettings)
            {
                if (!DoesSettingExist(settings))
                {
                    Categories.Find(c => c.Category == ESettingsCategory.Unknown).AddSetting(settings);
                }
            }
        }

        [Button("Reorder All Alphabetically")]
        private void ReorderAllAlphabetically()
        {
            foreach (var category in Categories)
            {
                category.ReorderAlphabetically();
            }
        }

        private bool DoesSettingExist(Settings settings)
        {
            foreach (var category in Categories)
            {
                if(category.Settings.Contains(settings))
                {
                    return true;
                }
            }

            return false;
        }
    }
    
    [Serializable]
    public class SettingsCategory
    {
        public ESettingsCategory Category;

        [ListDrawerSettings(DraggableItems = true), ShowInInspector]
        public List<Settings> Settings = new List<Settings>();

        public void AddSetting(Settings settings)
        {
            Settings.Add(settings);
        }

        public void ReorderAlphabetically()
        {
            // Reorder Alphabetically by the 'name' property of Settings
            Settings.Sort((x, y) => string.Compare(x.SettingsID, y.SettingsID, StringComparison.Ordinal));
        }
    }

    public enum ESettingsCategory
    {
        Unknown = 0,
        Dyes = 1,
        Structure_Walls = 10,
        Structure_Floors = 11,
        Structure_Doors = 12,
        Furniture_Storage = 20,
        Furniture_Decorations = 21,
        Furniture_Crafting = 22,
        Furniture_Lighting = 23,
        Furniture_Lifestyle = 24,
    }
}
