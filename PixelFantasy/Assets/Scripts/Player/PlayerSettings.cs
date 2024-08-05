using System;
using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Player
{
    [Serializable]
    public class PlayerSettingsData
    {
        // Note: All values need to have a default value
        [DefaultValue(true)] public bool AutoSaveEnabled { get; set; }
        [DefaultValue(30f)] public float AutoSaveFrequency { get; set; } // In minutes
        [DefaultValue(3)] public int MaxAutoSaves { get; set; } // Max number of autosaves to keep
    }

    public static class PlayerSettings
    {
        public static event Action<bool> OnAutoSaveEnabledChanged;
        public static event Action<float> OnAutoSaveFrequencyChanged;

        private static readonly string PlayerSettingsFilePath = Path.Combine(Application.persistentDataPath + "/PlayerSettings/", "playerSettings.json");
        private static PlayerSettingsData _playerSettingsDataCache;

        private static PlayerSettingsData PlayerSettingsData
        {
            get
            {
                if (_playerSettingsDataCache == null)
                {
                    LoadPlayerSettings();
                }
                return _playerSettingsDataCache;
            }
        }

        private static void LoadPlayerSettings()
        {
            if (File.Exists(PlayerSettingsFilePath))
            {
                try
                {
                    var settings = new JsonSerializerSettings
                    {
                        DefaultValueHandling = DefaultValueHandling.Populate,
                        ObjectCreationHandling = ObjectCreationHandling.Replace
                    };

                    string json = File.ReadAllText(PlayerSettingsFilePath);
                    _playerSettingsDataCache = JsonConvert.DeserializeObject<PlayerSettingsData>(json, settings);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to load player settings: {ex.Message}");
                    _playerSettingsDataCache = new PlayerSettingsData(); // Load default settings on failure
                }
            }
            else
            {
                _playerSettingsDataCache = new PlayerSettingsData(); // Create default settings if no file exists
            }
        }

        private static void SavePlayerSettings()
        {
            if (!Directory.Exists(Application.persistentDataPath + "/PlayerSettings/"))
            {
                Directory.CreateDirectory(Application.persistentDataPath + "/PlayerSettings/");
            }

            try
            {
                string json = JsonConvert.SerializeObject(PlayerSettingsData, Formatting.Indented);
                File.WriteAllText(PlayerSettingsFilePath, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save player settings: {ex.Message}");
            }
        }

        public static bool AutoSaveEnabled
        {
            get => PlayerSettingsData.AutoSaveEnabled;
            set
            {
                if (PlayerSettingsData.AutoSaveEnabled != value)
                {
                    PlayerSettingsData.AutoSaveEnabled = value;
                    SavePlayerSettings();
                    OnAutoSaveEnabledChanged?.Invoke(value);
                }
            }
        }

        public static float AutoSaveFrequency
        {
            get => PlayerSettingsData.AutoSaveFrequency;
            set
            {
                if (Math.Abs(PlayerSettingsData.AutoSaveFrequency - value) > 0.01f)
                {
                    PlayerSettingsData.AutoSaveFrequency = value;
                    SavePlayerSettings();
                    OnAutoSaveFrequencyChanged?.Invoke(value);
                }
            }
        }

        public static int MaxAutoSaves
        {
            get => PlayerSettingsData.MaxAutoSaves;
            set
            {
                if (PlayerSettingsData.MaxAutoSaves != value)
                {
                    PlayerSettingsData.MaxAutoSaves = value;
                    SavePlayerSettings();
                }
            }
        }
    }
}
