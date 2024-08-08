using System;
using System.Collections.Generic;
using System.Linq;
using AI;
using Sirenix.OdinInspector;
using Systems.Appearance.Scripts;
using Systems.Mood.Scripts;
using Systems.Needs.Scripts;
using Systems.Stats.Scripts;
using UnityEngine;
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

        [BoxGroup("DEBUG"), ShowInInspector] public bool FastActions { get; private set; } = true;
        
        [BoxGroup("Social"), ShowInInspector] public float BasePregnancyChance { get; private set; } = 50f;
        [BoxGroup("Social"), ShowInInspector, Tooltip("In Hours")] public float BaseSocialFrequency { get; private set; } = 4f;

        [BoxGroup("Mood"), ShowInInspector] public float MoodPositiveHourlyRate { get; private set; } = 12;
        [BoxGroup("Mood"), ShowInInspector] public float MoodNegativeHourlyRate { get; private set; } = -8;

        [BoxGroup("Work"), ShowInInspector] public float BaseWorkPerAction { get; private set; } = 1f;

        [BoxGroup("Experience"), ShowInInspector] public ExperienceSettings ExpSettings;

        [BoxGroup("Schedule"), ShowInInspector] public Color AnythingScheduleColour;
        [BoxGroup("Schedule"), ShowInInspector] public Color SleepScheduleColour;
        [BoxGroup("Schedule"), ShowInInspector] public Color WorkScheduleColour;
        [BoxGroup("Schedule"), ShowInInspector] public Color RecreationScheduleColour;
        
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

        private List<ItemSettings> _itemSettingsCache = null;
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

            var result = _itemSettingsCache.Find(settings => settings.name == settingsID);
            return result;
        }
        
        private List<FurnitureSettings> _furnitureSettingsCache = null;
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

            var result = _furnitureSettingsCache.Find(settings => settings.name == settingsID);
            return result;
        }

        private List<ZoneSettings> _zoneSettingsCache = null;
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

            var result = _zoneSettingsCache.Find(settings => settings.name == settingsID);
            return result;
        }
        
        private List<ConstructionSettings> _constructionSettingsCache = null;
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

            var result = _constructionSettingsCache.Find(settings => settings.name == settingsID);
            return result;
        }
        
        private List<DyeSettings> _dyeSettingsCache = null;
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

            var result = _dyeSettingsCache.Find(settings => settings.name == settingsID);
            return result;
        }

        public TileBase LoadTileBase(string tilebaseName)
        {
            TileBase tileBase = Resources.Load<TileBase>($"Tiles/{tilebaseName}");
            return tileBase;
        }
        
        // Task Actions
        private List<TaskAction> _taskActionsCache = null;
        private List<TaskAction> LoadAllTaskActions()
        {
            return Resources.LoadAll<TaskAction>("Settings/Actions").Where(s => s != null).ToList();
        }

        public TaskAction LoadTaskAction(string taskActionName)
        {
            if (_taskActionsCache == null || _taskActionsCache.Count == 0)
            {
                _taskActionsCache = LoadAllTaskActions();
            }

            var result = _taskActionsCache.Find(settings => settings.name == taskActionName);
            return result;
        }
    }
}
