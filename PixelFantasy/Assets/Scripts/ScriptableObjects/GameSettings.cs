using System;
using Sirenix.OdinInspector;
using Systems.Appearance.Scripts;
using Systems.Needs.Scripts;
using Systems.Stats.Scripts;
using UnityEngine;

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

        [BoxGroup("Social"), ShowInInspector] public float BasePregnancyChance { get; private set; } = 50f;
        [BoxGroup("Social"), ShowInInspector, Tooltip("In Hours")] public float BaseSocialFrequency { get; private set; } = 4f;

        [BoxGroup("Mood"), ShowInInspector] public float MoodPositiveHourlyRate { get; private set; } = 12;
        [BoxGroup("Mood"), ShowInInspector] public float MoodNegativeHourlyRate { get; private set; } = -8;

        [BoxGroup("Work"), ShowInInspector] public float BaseWorkPerAction { get; private set; } = 1f;

        [BoxGroup("Experience"), ShowInInspector] public ExperienceSettings ExpSettings;
        
        public static T[] LoadAllSettings<T>() where T : ScriptableObject
        {
            return Resources.LoadAll<T>("Settings/Data");
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
    }
}
