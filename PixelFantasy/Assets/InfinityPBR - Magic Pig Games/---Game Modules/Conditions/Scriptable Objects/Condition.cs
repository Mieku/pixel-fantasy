using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static InfinityPBR.Modules.Utilities;

/*
 * A condition is a singular thing that can potentially do multiple things. The common factor is the name and the
 * length of time the condition is set for. Time is optional, in case you want to manage conditions differently -- how
 * you utilize the conditions, and handle their expiration, is up to you, but with Game Modules 4, it can be entirely
 * automated.
 */

namespace InfinityPBR.Modules
{
    [HelpURL("https://infinitypbr.gitbook.io/infinity-pbr/game-modules/conditions")]
    [Serializable]
    [CreateAssetMenu(fileName = "Condition", menuName = "Game Modules/Create/Condition", order = 1)]
    public class Condition : ModulesScriptableObject
    {
        public string description;
        
        public List<ConditionTime> conditionTimes = new List<ConditionTime>();
        public List<ConditionPointEffect> pointEffects = new List<ConditionPointEffect>();
        
        public bool Periodic => _periodic;
        public float Period => _period;
        public bool Infinite => _infinite;
        public bool Instant => _instant;
        public bool Stack => _stack;
        public int Level => _level;
        public string DisplayName => _displayName;
        public Condition ExpirationCondition => _expirationCondition;
        public ExpirationConditionHandler ExpirationConditionHandler => _expirationConditionHandler;
        public float Time(IHaveStats owner = null) => ComputeTime(owner);
        public ModificationLevel ModificationLevel => statModificationLevels[0];
        
        public List<ModificationLevel> statModificationLevels = new List<ModificationLevel>();
        public int ModificationLevelsCount => statModificationLevels.Count;
        
        [HideInInspector] public MasteryLevels masteryLevels; // The Mastery Levels object we are using on this Stat

        [SerializeField] [HideInInspector] public string _displayName;
        [SerializeField] [HideInInspector] private bool _infinite;
        [SerializeField] [HideInInspector] private bool _instant;
        [SerializeField] [HideInInspector] private bool _periodic;
        [SerializeField] [HideInInspector] private float _period;
        [SerializeField] [HideInInspector] private bool _stack;
        [SerializeField] [HideInInspector] private int _level;
        [SerializeField] [HideInInspector] private Condition _expirationCondition;
        [SerializeField] [HideInInspector] private ExpirationConditionHandler _expirationConditionHandler;
        
        // Editor/Inspector
        [HideInInspector] public bool showDictionaries;
        [HideInInspector] public bool showMastery;
        [HideInInspector] public bool showMainSettings;
        [HideInInspector] public bool hasBeenSetup;
        
        private ItemObject[] cachedItemObjectArray;
        private ItemAttribute[] cachedItemAttributeArray;
        private Stat[] cachedStatsArray;
        private string _statsArrayType;
        private Stat[] cachedStatsTypeArray;
        [HideInInspector] public bool _showConditionTimes;
        [HideInInspector] public bool _showPointEffects;
        [HideInInspector] public bool _showRemovals;
        [HideInInspector] public bool _showInEditor;
        [HideInInspector] public int menubarIndex;

        /// <summary>
        /// Will return the total time (in game minutes) based on the list of ConditionTime values. If time = 0f, then
        /// this is an "instant" condition, and will be applied, then removed immediately.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="gametime"></param>
        /// <returns></returns>
        private float ComputeTime(IHaveStats owner = null) 
            => conditionTimes.Sum(conditionTime => conditionTime.Time(owner));

        /*
        public void AddDictionaryKey(string key)
        {
            if (GetKeyValue(key) != null)
                return;

            KeyValue newKeyValue = new KeyValue {key = key};
            dictionaries.keyValues.Add(newKeyValue);
        }
        */
        
        public void Caching()
        {
            cachedItemObjectArray = ItemObjectArray();
            cachedItemAttributeArray = ItemAttributeArray();
            cachedStatsArray = StatsArray();
        }
        
        /// <summary>
        /// Ensures that we always have at least one Modification Level
        /// </summary>
        public void EnsureOneModificationLevel()
        {
            if (statModificationLevels.Count == 0)
                statModificationLevels.Add(new ModificationLevel());
        }

        public void SetInfinite(bool newValue) => _infinite = newValue;
        public void SetInstant(bool newValue) => _instant = newValue;
        public void SetPeriodic(bool newValue) => _periodic = newValue;
        public void SetPeriod(float newValue) => _period = newValue;
        public void SetStack(bool newValue) => _stack = newValue;
        public void SetLevel(int newValue) => _level = newValue;
        public void SetDisplayName(string newValue) => _displayName = newValue;
        public void SetExpirationCondition(Condition newCondition) => _expirationCondition = newCondition;

        public void SetExpirationConditionHandler(ExpirationConditionHandler newHandler) => _expirationConditionHandler = newHandler;
    }
}
