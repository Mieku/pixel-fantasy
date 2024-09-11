using System;
using System.Collections.Generic;
using System.Linq;
using AI;
using DataPersistence;
using Sirenix.OdinInspector;
using Systems.Appearance.Scripts;
using Systems.Mood.Scripts;
using Systems.Needs.Scripts;
using Systems.Stats.Scripts;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "Settings/Game Settings", order = 0)]
    public class GameSettings : ScriptableObject
    {
        private static GameSettings _instance;

        public static GameSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<GameSettings>($"Settings/GameSettings");
                }
                return _instance;
            }
        }

        private List<Command> _commandsCache;
        private List<ItemSettings> _itemSettingsCache;
        private List<ConstructionSettings> _constructionSettingsCache;
        private List<FurnitureSettings> _furnitureSettingsCache;
        private List<DyeSettings> _dyeSettingsCache;
        private List<TaskAction> _taskActionsCache;
        private List<ZoneSettings> _zoneSettingsCache;
        
        public void RefreshCaches()
        {
            _commandsCache = LoadAllCommands();
            _itemSettingsCache = LoadAllItemSettings();
            _constructionSettingsCache = LoadAllConstructionSettings();
            _furnitureSettingsCache = LoadAllFurnitureSettings();
            _dyeSettingsCache = LoadAllDyeSettings();
            _zoneSettingsCache = LoadAllZoneSettings();
        }

        [BoxGroup("DEBUG"), ShowInInspector] public bool FastActions { get; private set; } = true;
        
        [BoxGroup("Social"), ShowInInspector] public float BasePregnancyChance { get; private set; } = 50f;
        [BoxGroup("Social"), ShowInInspector, Tooltip("In Hours")] public float BaseSocialFrequency { get; private set; } = 4f;

        [BoxGroup("Mood"), ShowInInspector] public float MoodPositiveHourlyRate { get; private set; } = 12;
        [BoxGroup("Mood"), ShowInInspector] public float MoodNegativeHourlyRate { get; private set; } = -8;

        [BoxGroup("Work"), ShowInInspector] public float BaseWorkPerAction { get; private set; } = 1f;
        [BoxGroup("Work"), ShowInInspector] public float MaxZoomDisplayWork { get; private set; } = 9f;

        [BoxGroup("Experience"), ShowInInspector] public ExperienceSettings ExpSettings;

        [BoxGroup("Schedule"), ShowInInspector] public Color AnythingScheduleColour;
        [BoxGroup("Schedule"), ShowInInspector] public Color SleepScheduleColour;
        [BoxGroup("Schedule"), ShowInInspector] public Color WorkScheduleColour;
        [BoxGroup("Schedule"), ShowInInspector] public Color RecreationScheduleColour;

        [BoxGroup("Visibility"), ShowInInspector] public float MinGoodVisibility { get; private set; }  = 0.7f;
        [BoxGroup("Visibility"), ShowInInspector] public float MinLowVisibility { get; private set; }  = 0.3f;
        [BoxGroup("Visibility"), ShowInInspector] public float LowVisibilityStatMod { get; private set; }  = -0.25f;
        [BoxGroup("Visibility"), ShowInInspector] public float BlindVisibilityStatMod { get; private set; }  = -0.7f;

        [BoxGroup("Input"), ShowInInspector] public InputActionAsset InputActions;
        
        [BoxGroup("Camera"), ShowInInspector] public float SlowCameraSpeed { get; private set; } = 5f;
        [BoxGroup("Camera"), ShowInInspector] public float NormalCameraSpeed { get; private set; } = 8f;
        [BoxGroup("Camera"), ShowInInspector] public float FastCameraSpeed { get; private set; } = 12f;

        [BoxGroup("Colours"), ShowInInspector] public Color HighlightColour { get; private set; } = new Color(0.996f, 0.784f, 0.141f, 1.000f);
        [BoxGroup("Colours"), ShowInInspector] public Color IssueColour { get; private set; } = new Color(0.961f, 0.333f, 0.365f, 1.000f);
        [BoxGroup("Colours"), ShowInInspector] public Color SelectOutlineColour { get; private set; } = Color.white;
        [BoxGroup("Colours"), ShowInInspector] public Color HoverOutlineColour { get; private set; } = new Color(0.580f, 0.992f, 1.0f, 1.000f);
        [BoxGroup("Colours"), ShowInInspector] public Color PassedUsePosColour { get; private set; } = new Color(0.580f, 0.992f, 1.0f, 1.000f);
        [BoxGroup("Colours"), ShowInInspector] public Color FailedUsePosColour { get; private set; } = new Color(0.961f, 0.333f, 0.365f, 1.000f);
        
        
        [BoxGroup("Settings", true, true, 1)] public SettingsCategories PlayerBuildCategories;
        
        public List<Settings> LoadAllSettings()
        {
            return Resources.LoadAll<Settings>("Settings").Where(s => s != null).ToList();
        }
        
        public RaceSettings LoadRaceSettings(string raceName)
        {
            return Resources.Load<RaceSettings>($"Settings/Races/{raceName}");
        }

        public NeedSettings LoadNeedSettings(string needSettingsID)
        {
            return Resources.Load<NeedSettings>($"Settings/Needs/{needSettingsID}");
        }

        public SkillSettings LoadSkillSettings(string skillSettingsID)
        {
            return Resources.Load<SkillSettings>($"Settings/Skills/{skillSettingsID}");
        }

        public History LoadHistorySettings(string historyID)
        {
            return Resources.Load<History>($"Settings/Histories/{historyID}");
        }

        public Trait LoadTraitSettings(string traitID)
        {
            return Resources.Load<Trait>($"Settings/Traits/{traitID}");
        }

        public EmotionalBreakdownSettings LoadEmotionalBreakdownSettings(string settingsID)
        {
            return Resources.Load<EmotionalBreakdownSettings>($"Settings/Emotional Breakdowns/{settingsID}");
        }

        public EmotionSettings LoadEmotionSettings(string settingsID)
        {
            return Resources.Load<EmotionSettings>($"Settings/Emotions/{settingsID}");
        }
        
        public List<ItemSettings> LoadAllItemSettings()
        {
            return Resources.LoadAll<ItemSettings>("Settings/Items").Where(s => s != null).ToList();
        }

        public ItemSettings LoadItemSettings(string settingsID)
        {
            if (_itemSettingsCache == null || _itemSettingsCache.Count == 0)
            {
                _itemSettingsCache = LoadAllItemSettings();
            }

            var result = _itemSettingsCache.Find(settings => settings.SettingsID == settingsID);
            return result;
        }
        
        private List<FurnitureSettings> LoadAllFurnitureSettings()
        {
            return Resources.LoadAll<FurnitureSettings>("Settings/Furniture").Where(s => s != null).ToList();
        }

        public FurnitureSettings LoadFurnitureSettings(string settingsID)
        {
            if (_furnitureSettingsCache == null || _furnitureSettingsCache.Count == 0)
            {
                _furnitureSettingsCache = LoadAllFurnitureSettings();
            }

            var result = _furnitureSettingsCache.Find(settings => settings.SettingsID == settingsID);
            return result;
        }
        
        private List<ZoneSettings> LoadAllZoneSettings()
        {
            return Resources.LoadAll<ZoneSettings>("Settings/Zones").Where(s => s != null).ToList();
        }

        public ZoneSettings LoadZoneSettings(string settingsID)
        {
            if (_zoneSettingsCache == null || _zoneSettingsCache.Count == 0)
            {
                _zoneSettingsCache = LoadAllZoneSettings();
            }

            var result = _zoneSettingsCache.Find(settings => settings.SettingsID == settingsID);
            return result;
        }
        
        private List<ConstructionSettings> LoadAllConstructionSettings()
        {
            return Resources.LoadAll<ConstructionSettings>("Settings/Structure").Where(s => s != null).ToList();
        }

        public ConstructionSettings LoadConstructionSettings(string settingsID)
        {
            if (_constructionSettingsCache == null || _constructionSettingsCache.Count == 0)
            {
                _constructionSettingsCache = LoadAllConstructionSettings();
            }

            var result = _constructionSettingsCache.Find(settings => settings.SettingsID == settingsID);
            return result;
        }
        
        private List<DyeSettings> LoadAllDyeSettings()
        {
            return Resources.LoadAll<DyeSettings>("Settings/Dye").Where(s => s != null).ToList();
        }

        public DyeSettings LoadDyeSettings(string settingsID)
        {
            if (_dyeSettingsCache == null || _dyeSettingsCache.Count == 0)
            {
                _dyeSettingsCache = LoadAllDyeSettings();
            }

            var result = _dyeSettingsCache.Find(settings => settings.SettingsID == settingsID);
            return result;
        }

        public TileBase LoadTileBase(string tilebaseName)
        {
            TileBase tileBase = Resources.Load<TileBase>($"Tiles/{tilebaseName}");
            return tileBase;
        }
        
        // Commands
        private List<Command> LoadAllCommands()
        {
            return Resources.LoadAll<Command>("Settings/Commands").Where(c => c != null).ToList();
        }

        public Command LoadCommand(string commandID)
        {
            if (_commandsCache == null || _commandsCache.Count == 0)
            {
                _commandsCache = LoadAllCommands();
            }

            var result = _commandsCache.Find(command => command.CommandID == commandID);
            return result;
        }
    }
}
