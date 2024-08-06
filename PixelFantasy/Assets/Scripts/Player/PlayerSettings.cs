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
        [DefaultValue(1)] public int AutoSaveFrequency = 1; // In days
        [DefaultValue(3)] public int MaxAutoSaves = 3; // Max number of autosaves to keep

        [DefaultValue(1f)] public float MasterVolume = 1f;
        [DefaultValue(1f)] public float MusicVolume = 1f;
        [DefaultValue(1f)] public float EffectsVolume = 1f;
        [DefaultValue(1f)] public float AmbientVolume = 1f;
    }

    public static class PlayerSettings
    {
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
        
        public static int AutoSaveFrequency
        {
            get => PlayerSettingsData.AutoSaveFrequency;
            set
            {
                PlayerSettingsData.AutoSaveFrequency = value;
                SavePlayerSettings();
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
        
        public static float MasterVolume
        {
            get => PlayerSettingsData.MasterVolume;
            set
            {
                if (!Mathf.Approximately(PlayerSettingsData.MasterVolume, value))
                {
                    PlayerSettingsData.MasterVolume = value;
                    SavePlayerSettings();
                }
            }
        }
        
        public static float MusicVolume
        {
            get => PlayerSettingsData.MusicVolume;
            set
            {
                if (!Mathf.Approximately(PlayerSettingsData.MusicVolume, value))
                {
                    PlayerSettingsData.MusicVolume = value;
                    SavePlayerSettings();
                }
            }
        }
        
        public static float EffectsVolume
        {
            get => PlayerSettingsData.EffectsVolume;
            set
            {
                if (!Mathf.Approximately(PlayerSettingsData.EffectsVolume, value))
                {
                    PlayerSettingsData.EffectsVolume = value;
                    SavePlayerSettings();
                }
            }
        }
        
        public static float AmbientVolume
        {
            get => PlayerSettingsData.AmbientVolume;
            set
            {
                if (!Mathf.Approximately(PlayerSettingsData.AmbientVolume, value))
                {
                    PlayerSettingsData.AmbientVolume = value;
                    SavePlayerSettings();
                }
            }
        }
    }
}
