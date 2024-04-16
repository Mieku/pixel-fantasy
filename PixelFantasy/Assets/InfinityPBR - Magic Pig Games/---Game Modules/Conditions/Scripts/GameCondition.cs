using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
//using static InfinityPBR.Modules.ConditionsRepository;
using static InfinityPBR.Modules.Timeboard;

/*
 * GAME CONDITIONS
 *
 * This is the in-game, runtime object to use, representing a Condition made with the Conditions module. It
 * is serializable, and will automatically reconnect the Condition scriptable objects with the unique IDs (uid) that
 * are saved, the first time they are called each game play.
 */

namespace InfinityPBR.Modules
{
    [HelpURL("https://infinitypbr.gitbook.io/infinity-pbr/game-modules/conditions")]
    [Serializable]
    public class GameCondition : IAmGameModuleObject, IHaveModificationLevels
    {
        public float Now => timeboard.gametime.Now();
        
        private Condition _parent;
        public Condition Parent() 
            => _parent 
                ? _parent 
                : _parent = GameModuleRepository.Instance.Get<Condition>(Uid());
        
        [SerializeField] private string _uid;
        public void SetUid(string value) => _uid = value;
        public string Uid() => _uid;
        
        public string ObjectName() => objectName;
        public string ObjectType() => objectType;
        public string objectName; // The name of this
        public string objectType; // The type (parent directory from hierarchy name)
        public Dictionaries dictionaries = new Dictionaries("Unnamed");

        public IHaveStats Source => _source;
        private IHaveStats _source;

        // **********************************************************************
        // GameId
        // **********************************************************************
        
        [SerializeField] private string _gameId;

        // Will create a new _gameId if one does not exist
        // NOTE: call forceNew true in Constructor. Also, be careful when cloning, as
        // in some cases you may wish to make a new GameId, but in other cases, cloning
        // will not want to make a new GameId.
        public virtual string GameId(bool forceNew = false) =>
            string.IsNullOrWhiteSpace(_gameId) || forceNew
                ? _gameId = Guid.NewGuid().ToString() 
                : _gameId;
        
        public float endTime; // Time when the GameCondition will delete itself
        public float nextEffect; // Next time when the periodic effects will trigger

        // These lists hold the actual effects, per Stat affected. Point, Value, & Proficiency effect. It is possible to have
        // multiple GameConditionEffect objects which affect the same Stat.
        public List<StatModification> effectsPeriodic = new List<StatModification>();
        public ModificationLevel ModificationLevel => GetModificationLevel(); // Will hold all of the constant effects
        
        public List<ModificationLevel> ModificationLevels => GetModificationLevels(); // returns the current Modification Level from the Quest
        
        private GameConditionList _parentList;
        public GameConditionList ParentList => _parentList;
        public void SetParentList(GameConditionList value) => _parentList = value;
        
        public bool autoCompute = true; // If true, will automatically recompute when value changes (isDirty = true).
        public bool isDirty; // If true, it means we may need to recompute this -- value may have changed, or value of an influencer changed.
        
        public string DisplayName => Parent().DisplayName;
        public string Description => Parent().description;
        public bool Stack => Parent().Stack;
        public bool Instant => Parent().Instant;
        public bool Periodic => Parent().Periodic;
        public bool Infinite => Parent().Infinite;
        public int Level => Parent().Level;
        public IHaveConditions Owner => ParentList.Owner;
        public Condition ExpirationCondition => Parent().ExpirationCondition;
        public ExpirationConditionHandler ExpirationConditionHandler => Parent().ExpirationConditionHandler;
        
        public bool Expired() => endTime <= Now && !Infinite; // Expires if end time is less than the game time, and not infinite
        public bool WillBeExpired(float gameTime) => endTime <= gameTime && !Infinite;
        public bool PeriodicReady() => nextEffect <= Now && !Instant;
        public bool WillBePeriodicReady(float gameTime) => nextEffect <= gameTime && !Instant;
        
        public float TimeActive() => Now - _startTime;

        //public string uid => GetUid(); // Will return the uid, which can be serialized and used to look up the Scriptable Object
        
        //private Condition condition;
        [SerializeField] [HideInInspector] private ModificationLevel _modificationLevel = new ModificationLevel();
        [SerializeField] [HideInInspector] private float _startTime;
        [SerializeField] [HideInInspector] private float _totalTime;

        // ------------------------------------------------------------------------------------------
        // PUBLIC METHODS
        // ------------------------------------------------------------------------------------------
        
        public void SetDirty(bool newValue = true)
        {
            isDirty = newValue;
            
            if (!newValue) return; // Do not set isDirty to false for other values!

            if (StatsOwner == null)
            {
                return;
            }
            StatsOwner?.SetStatsDirty(Parent().statModificationLevels[0].targets);
        }
        
        private IHaveStats StatsOwner => (IHaveStats)Owner;
        
        /*
         December 19, 2021
         Why is this here? Well, it's cool. The idea that you can use {{Code}} in a string and parse it to other values.
         However, it's also awkward... that is, there's no "Right" way to parse. Do you want to flip negative to a positive?
         (I.e. "Causes 15 damage" vs "Causes -15 damage").  Do you want it to include the data from the recepient?
         
         There are just...too many ways of going about it. So this is something I may bring back later, but I need to think
         more about how to do it RIGHT.
         *
         * 
        // PARSED DESCRIPTION
        public string ParseDescription()
        {
            var description = Condition.description;
            if (string.IsNullOrWhiteSpace(description)) return description;
            Debug.Log("Parse Start: " + description);
        
            if (!description.Contains("{{")) return description;

            int loopCheck = 0; // set this so you don't make an infinite loop!
            // Loop through so long as the descriptionToParse still has some {{Key}} values left
            while (description.Contains("{{"))
            {
                var start = description.IndexOf("{{") + 2;
                var key = description.Substring(start, description.IndexOf("}}") - start);
            
                Debug.Log("key: " + key);

                string replaceThis = "{{" + key + "}}";
                Debug.Log("Replace: " + replaceThis);
                // Replace the key with a value computed in ParseValue()
                description = description.Replace(replaceThis, ParseValue(key));

                // This protects from an infinite loop.
                loopCheck++;
                if (loopCheck > 10)
                    return description;
            }
        
            Debug.Log("Parse End: " + description);
        
            return description;
        }
        
        
        
        public string ParseValue(string key, bool removeNegative = true)
        {
            Debug.LogWarning("Redo this");
            
            Debug.Log("Searching for key " + key);

            var gametime = owner.GetGametime();

            if (key == "Duration")
            {
                if (gametime == null) return "Missing Gametime";
                
                var time = _totalTime;
                var minutes = Mathf.Floor(time / gametime.secondsPerGameMinute);
                var partial = (time % gametime.secondsPerGameMinute) / gametime.secondsPerGameMinute;
                var final = (float) Math.Round( minutes + partial, 2, MidpointRounding.AwayFromZero);
                return $"{final}";
            }
            
            
            // Search for key in the constant values
            foreach (StatModification modification in ModificationLevel.modifications)
            {
                if (modification.target.objectName != key && modification.target.uid != key) continue;

                var final = modification.value;
                if (removeNegative) final = Mathf.Abs(final);
                return $"{final}";
            }
            
            return "Parse End Err";
        }
        */

        // ------------------------------------------------------------------------------------------
        // CONSTRUCTOR
        // ------------------------------------------------------------------------------------------

        public GameCondition(Condition condition, GameConditionList parentList, IHaveStats source, int masteryLevelIndex = 0)
        {
            // Set basic details
            dictionaries = condition.dictionaries?.Clone(); // Clone the dictionary from the condition
            SetUid(condition.Uid());
            objectName = condition.objectName; // Set name
            objectType = condition.objectType; // Set type

            _source = source; // Set the source of this condition

            GameId();
            SetParentList(parentList);

            SetupGameTimeValues(source); // Set the time values for this
            SetupGameConditionValues(source); // Set the final impact values for this
            SetDirty();
        }
        
        // This method will start coroutines that track periodic effects, and the end time of the condition.
        public void Setup()
        {
            ModulesHelper.Instance.SetupGameCondition(this);
        }

        public IEnumerator CheckEndTime()
        {
            // Do nothing until the end time is reached.
            while (endTime > Now)
            {
                yield return null;
            }
            
            ParentList.ExpirationActions(this);
        }

        public void ExpireNow() => endTime = Now;

        public IEnumerator CheckPeriodicTime()
        {
            // Keep doing this until endTime is reached or forever if infinite
            while (endTime > Now || Parent().Infinite)
            {
                // If time for next effect has been reached, do the action, and update nextEffect time
                if (nextEffect <= Now)
                {
                    PeriodicActions();
                    nextEffect += Parent().Period; // Update the next effect by adding Period (time length)
                }

                yield return null;
            }
        }

        private void SetupGameTimeValues(IHaveStats source)
        {
            // If this is instant, set the time to current, so it will expire right away.
            if (Parent().Instant)
            {
                endTime = Now;
                return;
            }
            
            _totalTime = Parent().Time(source);
            endTime = Now + _totalTime; // Add the final Time based on the source Stat values

            if (!Parent().Periodic) return; // If this is not periodic, then we are done.
            nextEffect = Now; // Set to now, so that the first effect will be next frame
        }

        private void SetupGameConditionValues(IHaveStats source)
        {
            // Point Effects
            foreach (var effect in Parent().pointEffects)
            {
                if (effect.statAffected == null) continue;
                AddToPeriodic(effect, source);
            }
            
            // Value / Proficiency Effects
            foreach (var target in Parent().ModificationLevel.targets)
            {
                AddToConstant(Parent().ModificationLevel, target, source);
            }
        }

        private void AddToConstant(ModificationLevel modificationLevel, Stat target, IHaveStats source)
        {
            float valueEffect;
            float proficiencyEffect;
            (valueEffect, proficiencyEffect) = modificationLevel.GetEffectOn(target.Uid(), 0, 0, source);
            var newStatModification = new StatModification(null, target, valueEffect, proficiencyEffect)
                {
                    isBase = true
                };
            _modificationLevel.modifications.Add(newStatModification);
            if (_modificationLevel.targets.Contains(target)) return;
            _modificationLevel.targets.Add(target);
        }

        private void AddToPeriodic(ConditionPointEffect effect, IHaveStats sourceOfCondition)
        {
            var newStatModification = new StatModification(null, effect.statAffected, effect.ComputeEffect(sourceOfCondition), 0);
            effectsPeriodic.Add(newStatModification);
        }

        /// <summary>
        /// Manually set the value of a periodic effect between a min and max float value. Will return the new value if successful, or -1 if not, meaning
        /// we were unable to find the effect to change.
        /// </summary>
        /// <param name="statAffected"></param>
        /// <param name="minPoints"></param>
        /// <param name="maxPoints"></param>
        /// <returns></returns>
        public float SetPeriodicEffectValue(Stat statAffected, float minPoints, float maxPoints)
        {
            var newValue = UnityEngine.Random.Range(minPoints, maxPoints);
            foreach(var effect in effectsPeriodic)
            {
                if (effect.target != statAffected) continue;
                Debug.Log($"Found effect {effect.target.ObjectName}");
                effect.value = newValue;
                Debug.Log($"value is now {effect.value}");
                return newValue;
            }

            return -1; // We did not find a matching effect
        }
        
        public void DoInstantActions()
        {
            if (!Instant && (Periodic || effectsPeriodic.Count <= 0)) return;
            PointActions();
        }

        // ------------------------------------------------------------------------------------------
        // PRIVATE METHODS
        // ------------------------------------------------------------------------------------------

        /// <summary>
        /// Returns the unique id for this
        /// </summary>
        /// <returns></returns>
        private string GetUid() => Uid();
        
        /// <summary>
        /// Returns a clone of this object
        /// </summary>
        /// <returns></returns>
        public GameCondition Clone()
        {
            // Note: The clone will clone the Dictionaries object after, or it will continue to be linked directly to
            // the original object.
            var clonedObject = JsonUtility.FromJson<GameCondition>(JsonUtility.ToJson(this));
            clonedObject.dictionaries = clonedObject.dictionaries.Clone();
            return clonedObject;
        }
        
        // Not used in this context.
        public List<Stat> DirectlyAffectedBy(Stat stat = null) => default;
        
        public List<Stat> DirectlyAffects(Stat stat = null) =>
            ModificationLevels.SelectMany(x => x.targets).Distinct().ToList();
        
        public List<Stat> DirectlyAffectedByList(Stat stat = null)
        {
            throw new NotImplementedException();
        }

        public List<Stat> DirectlyAffectsList(Stat stat = null) => ModificationLevel.targets;

        public List<ModificationLevel> GetModificationLevels() => new() { ModificationLevel };

        public ModificationLevel GetModificationLevel() => Parent().ModificationLevel;

        public void PeriodicActions(bool ignorePeriodic = false)
        {
            if (!Parent().Periodic && !ignorePeriodic) return;
            
            // Assign the point affects
            PointActions();
        }

        public virtual void PointActions()
        {
            if (Owner == null)
            {
                Debug.LogError("Owner is null, cannot apply point effects.");
                return;
            }
            foreach (var statModification in effectsPeriodic)
                Owner.ConditionPointEffect(statModification.targetUid, statModification.value);
        }

        public MasteryLevel GetMasteryLevel(bool recompute = false)
        {
            throw new NotImplementedException();
        }

        public MasteryLevel SetMasteryLevel(int index)
        {
            throw new NotImplementedException();
        }

        public int GetMasteryLevelIndex(bool recompute = false)
        {
            throw new NotImplementedException();
        }

        public string GetMasteryLevelName(bool recompute = false)
        {
            throw new NotImplementedException();
        }

        public MasteryLevel AddToMasteryLevel(int value)
        {
            throw new NotImplementedException();
        }

        public MasteryLevel RecomputeActiveMasteryLevel()
        {
            throw new NotImplementedException();
        }

        public ModificationLevel GetModificationLevel(bool recompute = false)
        {
            throw new NotImplementedException();
        }

        public int ModificationLevelsCount()
        {
            throw new NotImplementedException();
        }
        
        public virtual void AddDictionaryKey(string key) => dictionaries.AddNewKeyValue(key);

        public virtual KeyValue GetKeyValue(string key) => dictionaries.Key(key);

        public virtual bool HasKeyValue(string key) => dictionaries.HasKeyValue(key);
    }
}