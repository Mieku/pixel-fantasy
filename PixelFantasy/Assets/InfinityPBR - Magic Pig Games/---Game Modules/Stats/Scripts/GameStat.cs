using System;
using System.Collections.Generic;
using UnityEngine;
using static InfinityPBR.Modules.MainBlackboard;
using static InfinityPBR.Modules.Timeboard;
using static InfinityPBR.Modules.Utilities;

/*
 * GAME STATS
 *
 * This is the in-game, runtime object to use, representing a Stat or a Skill made with the Stat module. It
 * is serializable, and will automatically reconnect the Stat scriptable objects with the unique IDs (uid) that
 * are saved, the first time they are called each game play.
 */

namespace InfinityPBR.Modules
{
    [HelpURL("https://infinitypbr.gitbook.io/infinity-pbr/game-modules/stats-and-skills")]
    [Serializable]
    public class StatSavedData
    {
        public string uid;
        public string gameId;
        public string objectName;
        public string objectType;
        public bool autoCompute;
        
        public float points;
        public float baseValue;
        public float baseProficiency;
        public string masteryLevelName;
        public int masteryLevelIndex;

        public GameStatList parentList;
        
        // Json strings
        public string jsonDictionary;
    }
    
    [Serializable]
    public class GameStat : IAmGameModuleObject, IHaveModificationLevels
    {
        // ************************************************************************************************
        // Connection to the parent object
        // ************************************************************************************************

        //private GameStatList _parentList;
        private Stat _parent;
        public Stat Parent
            => _parent 
                ? _parent 
                : _parent = GameModuleRepository.Instance.Get<Stat>(Uid());
        
        [SerializeField] private string _uid;  // SAVED
        public void SetUid(string value) => _uid = value;
        public string Uid() => _uid;
        
        public GameStatList ParentList { get; private set; }
        public void SetParentList(GameStatList value) => ParentList = value;
        public IHaveStats Owner => ParentList.Owner;

        public string ObjectName() => objectName;
        public string ObjectType() => objectType;
        public string objectName; // The name of this  // SAVED
        public string objectType; // The type (parent directory from hierarchy name) // SAVED
        public Dictionaries dictionaries = new Dictionaries("Unnamed");  // SAVED

        /*
         June 8, 2023 -- See note on similar code in GameModulesActor.cs
        // **********************************************************************
        // Save and Load
        // **********************************************************************

        public string SaveData()
        {
            var savedData = new StatSavedData
            {
                uid = Uid(),
                gameId = GameId(),
                objectName = objectName,
                objectType = objectType,
                autoCompute = autoCompute,
                points = _points,
                baseValue = _baseValue,
                baseProficiency = _baseProficiency,
                masteryLevelName = _masteryLevelName,
                masteryLevelIndex = _masteryLevelIndex,
                parentList = ParentList,
                jsonDictionary = dictionaries.SaveData()
            };
            
            Debug.Log($"GameStat: {savedData}");
            return JsonUtility.ToJson(savedData);
        }

        public void LoadData(string savedData, IHaveStats owner = null)
        {
            var loadedData = JsonUtility.FromJson<StatSavedData>(savedData);
            SetUid(loadedData.uid);
            _gameId = loadedData.gameId;
            objectName = loadedData.objectName;
            objectType = loadedData.objectType;
            autoCompute = loadedData.autoCompute;
            _points = loadedData.points;
            _baseValue = loadedData.baseValue;
            _baseProficiency = loadedData.baseProficiency;
            _masteryLevelName = loadedData.masteryLevelName;
            _masteryLevelIndex = loadedData.masteryLevelIndex;
            ParentList = loadedData.parentList;

            dictionaries.LoadData(loadedData.jsonDictionary, Parent);
        }
        */
        
        // **********************************************************************
        // GameId
        // **********************************************************************
        
        [SerializeField] private string _gameId;  // SAVED

        // Will create a new _gameId if one does not exist
        // NOTE: call forceNew true in Constructor. Also, be careful when cloning, as
        // in some cases you may wish to make a new GameId, but in other cases, cloning
        // will not want to make a new GameId.
        public virtual string GameId(bool forceNew = false) =>
            String.IsNullOrWhiteSpace(_gameId) || forceNew
                ? _gameId = Guid.NewGuid().ToString() 
                : _gameId;
        
        // ************************************************************************************************
        // Members unique for this type
        // ************************************************************************************************
        
        // Variables
        public bool autoCompute = true; // If true, will automatically recompute when value changes (isDirty = true).
        public float otherValue;
        public float otherProficiency;
        
        // Properties
        public float Points => GetPoints(); // Gets the current points assigned. Will not recompute final stat.
        public float BaseValue => GetBaseValue(); // Gets the current Base Value, will not recompute final stat
        public float BaseProficiency => GetBaseProficiency(); // Gets the current Base Proficiency, will not recompute final stat
        public float FinalValue => GetFinalValue(); // Gets the final, post-computation value. Will not recompute first.
        public float FinalProficiency => GetFinalProficiency(); // Gets the final, post-computation proficiency. Will not recompute first.

        public MasteryLevel MasteryLevel => GetMasteryLevel(); // returns the current Mastery Level on this object
        public ModificationLevel ModificationLevel => GetModificationLevel(); // returns the current Modification Level from Stat
        public int MasteryLevelIndex => GetMasteryLevelIndex(); // Returns the index of the current mastery level
        public string MasteryLevelName => GetMasteryLevelName(); // Returns the name of the active mastery level
        public int ParentModificationLevelsCount => Parent.ModificationLevelsCount; // Gets the number of modification levels from the parent object
        public List<Stat> DirectlyAffectsList(Stat stat = null) => ModificationLevel.targets;

        public bool Counter => !Parent.canBeModified && !Parent.canBeTrained; // "Counters" are stats that cannot be modified or trained.
        public bool Trainable => Parent.canBeTrained; // This can be trained
        public bool Modifiable => Parent.canBeModified; // This can be modified by other stats and objects
        public bool IsModified => Math.Abs(FinalStat() - (Points + BaseValue)) > 0.01; // This stat has been modified by something else
        public bool FinalStatGreaterThanPointsAndValue => FinalStat() > Points + BaseValue; // Something is modifying this stat to be greater than the sum of its parts
        public bool FinalStatLessThanPointsAndValue => FinalStat() < Points + BaseValue; // Something is modifying this stat to be less than the sum of its parts

        
        // This is the Stat which is used, optionally, when modifying THIS GameStat points. Example: "Experience" 
        // GameStat may be modified by the Stat "Learning", so when "100" points is added to Experience,
        // the final result may be "110" as an example.
        private Stat ModifyPointsByProficiency => Parent.modifyPointsByProficiency;

        // Private variables
        [SerializeField] [HideInInspector] private float _points;  // SAVED
        [SerializeField] [HideInInspector] private float _baseValue;  // SAVED
        [SerializeField] [HideInInspector] private float _baseProficiency;  // SAVED
        [SerializeField] [HideInInspector] private float _finalStat;
        [SerializeField] [HideInInspector] private float _finalValue;
        [SerializeField] [HideInInspector] private float _finalProficiency;
        private ModificationLevel _modificationLevel;
        private MasteryLevel _masteryLevel;
        [SerializeField] [HideInInspector] private string _masteryLevelName;  // SAVED
        [SerializeField] [HideInInspector] private int _masteryLevelIndex;  // SAVED
        private float lastComputedTime = -1;
        //[SerializeField] private IHaveStats owner;
        
        // ************************************************************************************************
        // Constructor
        // ************************************************************************************************

        public GameStat(Stat stat, GameStatList parentList, int newMasteryLevelIndex = 0)
        {
            _parent = stat; // v3.6 changed this
            SetParentList(parentList);
            SetUid(stat.Uid()); // v3.6 -- set the uid with this method now
            GameId(true);
            dictionaries = stat.dictionaries?.Clone();
            _points = stat.points;
            _baseValue = stat.baseValue;
            _baseProficiency = stat.baseProficiency;
            objectName = stat.objectName;
            objectType = stat.objectType;
            
            _masteryLevelIndex = newMasteryLevelIndex;
            if (!_parent.canBeTrained) return;
            if (stat.masteryLevels == null)
            {
                Debug.LogError($"There are no Mastery Levels assigned to trainable stat {stat.ObjectName}. Please " +
                               $"set up Mastery Levels in the Game Modules panel.");
                return;
            }
            _masteryLevel = AddToMasteryLevel(0);
            PostToBlackboard(); // Initial post on creation
        }

        public GameStat()
        {
            
        }

        // ************************************************************************************************
        // Override Methods
        // ************************************************************************************************
        
        /// <summary>
        /// Returns a clone of this object
        /// </summary>
        /// <returns></returns>
        public GameStat Clone(bool forceNewGameId = false)
        {
            // Note: The clone will clone the Dictionaries object after, or it will continue to be linked directly to
            // the original object.
            var clonedObject = JsonUtility.FromJson<GameStat>(JsonUtility.ToJson(this));
            clonedObject.dictionaries = clonedObject.dictionaries.Clone();
            clonedObject.GameId(forceNewGameId);
            return clonedObject;
        }

        // SetDirty(true) will set this stat up for re-computation, false will do no action
        public void SetDirty(bool setDirty = true)
        {
            if (!setDirty) return;
            
            if (AlreadyComputedThisFrame()) return;
            
            if (ParentList == null) return; // If we don't have an ParentList, we can't check for other stats
            if (Owner == null) return; // If we don't have an owner, we can't check for other stats
            
            if (autoCompute)
                AddToQueue();
            
            // SetDirty for each of the GameStats that this one directly affects, only
            // if the owner has the GameStat
            foreach (var stat in Parent.directlyAffects)
            {
                if (!Owner.TryGetGameStat(stat.Uid(), out var otherStat))
                    continue;
                otherStat.SetDirty();
            }
        }
        
        private void AddToQueue()
        {
            if (ModulesHelper.Instance.recomputeStatQueue.Contains(this)) return;
            ModulesHelper.Instance.recomputeStatQueue.Enqueue(this); // Add to the queue.
            ModulesHelper.Instance.StartRecomputeQueue(); // Will start the queue if it hasn't already been started. Queue will delay one frame.
        }
        
        public List<ModificationLevel> GetModificationLevels() => new List<ModificationLevel> { ModificationLevel };

        // ************************************************************************************************
        // IHaveModificationLevels
        // ************************************************************************************************

        /// <summary>
        /// Adds a new Mastery Level, or changes the level index (upgrades / downgrades)
        /// </summary>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public MasteryLevel AddToMasteryLevel(int newValue)
        {
            SetMasteryLevel(_masteryLevelIndex += newValue);
            return GetMasteryLevel(true);
        }

        public int ModificationLevelsCount() => ParentModificationLevelsCount;
        
        /// <summary>
        /// Will return the current Mastery Level of this Stat
        /// </summary>
        /// <returns></returns>
        public MasteryLevel GetMasteryLevel(bool recompute = false) =>
            recompute || _masteryLevel == null 
                ? RecomputeActiveMasteryLevel() 
                : _masteryLevel;

        /// <summary>
        /// Will set the Mastery Level of this Stat
        /// </summary>
        /// <returns></returns>
        public MasteryLevel SetMasteryLevel(int index)
        {
            // Ensure the index is within the range for the Mastery Levels available
            index = Mathf.Clamp(index, 0, ModificationLevelsCount() - 1);

            // Set the values
            _masteryLevelIndex = index;
            _masteryLevel = Parent.masteryLevels.levels[index];
            _modificationLevel = Parent.statModificationLevels[index];
            _masteryLevelName = _masteryLevel.name;
            
            SetDirty(); // Stat may need to be recomputed now
            
            return _masteryLevel;
        }

        /// <summary>
        /// Will return the current Modification Level of this Stat
        /// </summary>
        /// <returns></returns>
        public ModificationLevel GetModificationLevel(bool recompute = false)
        {
            if (_modificationLevel == null || _masteryLevel == null || recompute) 
                RecomputeActiveMasteryLevel();
            return _modificationLevel;
        }

        /// <summary>
        /// Returns the current mastery level index
        /// </summary>
        /// <returns></returns>
        public int GetMasteryLevelIndex(bool recompute = false)
        {
            if (recompute)
                RecomputeActiveMasteryLevel();
            return _masteryLevelIndex;
        }
        
        /// <summary>
        /// Returns the current mastery level index
        /// </summary>
        /// <returns></returns>
        public string GetMasteryLevelName(bool recompute = false)
        {
            if (recompute)
                RecomputeActiveMasteryLevel();
            return _masteryLevelName;
        }
        
        /// <summary>
        /// Recomputes which mastery level is active
        /// </summary>
        public MasteryLevel RecomputeActiveMasteryLevel() => SetMasteryLevel(MasteryLevelIndex);
        
        // ************************************************************************************************
        // METHODS UNIQUE TO THIS TYPE
        // ************************************************************************************************

        
        // FinalStat() will return the final stat without computing the values. Pass in otherLevels, and set recompute to true to 
        // recompute the final stat value before returning it. Set recomputeTree to true to also recompute all stats which affect
        // this one, otherwise those values will not be updated first.
        public float FinalStat(bool recompute = false, bool recomputeTree = false)
        {
            if (recompute)
                _finalStat = ComputeFinalStats(recomputeTree);

            return _finalStat;
        }

        // Recomputes the value. It will recompute the Tree, and cache this frame as well, so that
        // we don't end up recomputing it (or other values) multiple times in the same frame.
        public void Recompute()
        {
            ComputeFinalStats();
        }

        /// <summary>
        /// Add points to the points value on this Stat. Can add negative points to reduce final value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="proficiencyModifier"></param>
        /// <param name="useProficiencyModifier"></param>
        /// <returns></returns>
        public float AddPoints(float value, bool useProficiencyModifier = true)
        {
            _points += useProficiencyModifier ? ValueModifiedByProficiencyModifier(value) : value;
            _points = MinMaxPoints(_points); // Ensure we are within the min/max range set in the Inspector
            SetDirty(); 
            
            PostToBlackboard();
            return GetPoints();
        }

        /// <summary>
        /// Sets the point value for this GameStat to a specific value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="useProficiencyModifier"></param>
        /// <param name="setDirty"></param>
        /// <returns></returns>
        public float SetPoints(float value, bool useProficiencyModifier = true, bool setDirty = true)
        {
            _points = useProficiencyModifier ? ValueModifiedByProficiencyModifier(value) : value;
            _points = MinMaxPoints(_points); // Ensure we are within the min/max range set in the Inspector
            if (setDirty)
                SetDirty(); 
            
            PostToBlackboard();
            return GetPoints();
        }

        /// <summary>
        /// Set Points to a new value only if it's within the min/max range provided. Otherwise, return the current value.
        /// Note: This will still keep the value within the min/max range set in the Inspector. This version simply adds
        /// an additional layer of min/max protection.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="useProficiencyModifier"></param>
        /// <param name="setDirty"></param>
        /// <returns></returns>
        public float SetPoints(float value, float min, float max, bool useProficiencyModifier = true,
            bool setDirty = true) => value < min || value > max ? _points : SetPoints(value, useProficiencyModifier, setDirty);

        // If we are modifying the points by proficiency of another stat, this will return the new value after
        // modification.
        private float ValueModifiedByProficiencyModifier(float value) 
            => ModifyPointsByProficiency 
                ? ValueModifiedByOtherProficiency(value, ModifyPointsByProficiency.Uid()) 
                : value;

        // Will modify the value by the proficiency of the other GameStat based on it's uid
        //private float ValueModifiedByOtherProficiency(float value, string otherUid) 
        //    => value * (1 + FinalProficiencyOfOtherGameStat(otherUid));
        private float ValueModifiedByOtherProficiency(float value, string otherUid) 
            => value * (1 + FinalProficiencyOfOtherGameStat(otherUid));

        // Will return the final proficiency of another GameStat owned by the same IHaveStats owner. Returns
        // 0f if there is no owner, or if the owner does not have the GameStat being queries.
        private float FinalProficiencyOfOtherGameStat(string uid)
        {
            if (Owner == null) return default;
            return !Owner.TryGetGameStat(uid, out var gameStat) 
                ? default 
                : gameStat._finalProficiency;
        }

        /// <summary>
        /// Add points to the base value on this Stat. Optionally include another GameStat
        /// which will modify the add by the proficiency value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="proficiencyModifier"></param>
        /// <returns></returns>
        public float AddBaseValue(float value, GameStat proficiencyModifier = null)
        {
            // Note: Min/Max is computed during ComputeFinalStat, as this value may be modified by the
            // other modification levels
            _baseValue += proficiencyModifier == null 
                ? value 
                : ValueModifiedByOtherProficiency(value, proficiencyModifier.Uid());
            SetDirty();
            return GetBaseValue();
        }
        
        /// <summary>
        /// Add points to the base proficiency on this Stat. Optionally include another GameStat
        /// which will modify the add by the proficiency value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="proficiencyModifier"></param>
        /// <returns></returns>
        public float AddBaseProficiency(float value, GameStat proficiencyModifier = null)
        {
            // Note: Min/Max is computed during ComputeFinalStat, as this value may be modified by the
            // other modification levels
            _baseProficiency += proficiencyModifier == null 
                ? value 
                : ValueModifiedByOtherProficiency(value, proficiencyModifier.Uid());
            SetDirty();
            return GetBaseProficiency();
        }
        
        /// <summary>
        /// Return the effect this has on another Stat
        /// </summary>
        /// <param name="targetUid"></param>
        /// <returns></returns>
        public (float, float) GetEffectOn(string targetUid) 
            => Parent.statModificationLevels[_masteryLevelIndex]
                .GetEffectOn(targetUid, GetPoints(), FinalProficiency, Owner);
        
        private float GetPoints() => _points;
        private float GetBaseValue() => _baseValue;
        private float GetBaseProficiency() => _baseProficiency;
        private float GetFinalValue() => _finalValue;
        private float GetFinalProficiency() => _finalProficiency;

        // Computes the value and proficiency to add from the otherLevels passed into this system. Other levels can 
        // optionally include ItemObjects, Conditions, etc. That should be handled by the developer using this system.
        private (float, float) ComputeOtherValueAndProficiency(List<ModificationLevel> otherLevels = null)
        {
            if (otherLevels == null) return (0f, 0f); // Return 0s if there are not otherLevels

            var value = 0f;
            var proficiency = 0f;
            
            // For each other level, add the value and proficiency
            foreach (var modificationLevel in otherLevels)
            {
                // Points and Proficiency are always 0 as these are from non-Stat objects!
                var modificationResult = modificationLevel.GetEffectOn(Uid(), 0, 0, Owner);
                value += modificationResult.Item1; // It's always Value then Proficiency
                proficiency += modificationResult.Item2;
            }
            return (value, proficiency);
        }
        
        // Computes the other required Stat, in the proper order. the computeList value from the Parent
        // is ordered properly.
        private void ComputeRequiredStats(List<ModificationLevel> otherLevels = null)
        {
            foreach (var stat in Parent.computeList)
            {
                if (!Owner.TryGetGameStat(stat.Uid(), out var gameStat))
                    continue;
                gameStat.ComputeFinalStats( false);
            }
        }

        // This will compute the final stat for a single Stat -- and all of those that affect it! This is the major 
        // heavy lifting of the Stat module. By default, we will recompute the full stat tree, and cache the frame to
        // avoid computing other stats multiple times during this frame.
        protected virtual float ComputeFinalStats(bool recomputeTree = true)
        {
            // If we have already computed the final stat this frame, then we do not need to do it again. Return the _finalStat
            if (AlreadyComputedThisFrame())
                return _finalStat;

            SetLastComputeTime(); // Set the frame time, so we don't end up doing this twice.
            
            // If we are recomputing the entire tree, i.e. all the Stat that feed into this, then do it here.
            if (recomputeTree)
            {
                ComputeRequiredStats(Owner.GetOtherLevels());
            }

            // Compute the value/proficiency gains from all the Stat objects that affect this.
            var (valueAdd, proficiencyAdd) = Owner != null ? ComputeStatEffects() : (0f, 0f);
            
            if (Owner != null)
            {
                // Compute the value/proficiency from the owner.GetOtherLevels() passed in. These are meant to be ItemObjects, ItemAttributes
                // and potentially other things that affect Stat, but are kept outside of the Stat ecosystem.
                (otherValue, otherProficiency) = ComputeOtherValueAndProficiency(Owner.GetOtherLevels());
            }
            else
            {
                Debug.LogWarning($"Owner was null on GameStat {ObjectName()}. Skipping ComputeOtherValueAndProficiency()");
            }
            
            // Final value is Points + Base Value + Other Value + Stat Effects Value Add, and is then clamped by the
            // Min/Max options, if set.
            _finalValue = GetPoints() + GetBaseValue() + otherValue + valueAdd;
            _finalValue = MinMaxValue(_finalValue);
            
            // Final proficiency is Base Proficiency + Other Proficiency + Stat Effects Proficiency Add, and is then
            // clamped by the Min/Max options, if set.
            _finalProficiency = GetBaseProficiency() + otherProficiency + proficiencyAdd;
            _finalProficiency = MinMaxProficiency(_finalProficiency);
            
            // Final stat is the Final Value * (1 + Final Proficiency).  This way if proficiency is 0, the value is simply
            // not modified. If the proficiency is > 0, the value is higher, and if the proficiency is < 0, the value is
            // reduced. Proficiency = 0 is a "normal" proficiency, one might say.
            _finalStat = _finalValue * (1 + _finalProficiency);
            
            // Do any min/max and rounding operations on the final stat
            _finalStat = MinMaxFinalStat(_finalStat);
            _finalStat = RoundThis(_finalStat, Parent.decimals, Parent.roundingMethod);
            
            ResetOtherValueAndProficiency();
            PostToBlackboard();
           
            // RemoveFromQueue();
            return _finalStat;
        }

        private void RemoveFromQueue()
        {
            // Jan 8, 2023 -- I was going to do this, but to do so would require creating new Queue<> so...probably
            // not much more efficient, if not less efficient.
            //if (!statsRepository.recomputeQueue.Contains(this)) return;
        }

        public void PostToBlackboard()
        {
            if (!Parent.postPointsToBlackboard && !ParentList.forcePostToBlackboard
                                               && !Parent.postFinalValueToBlackboard &&
                                               !ParentList.forcePostToBlackboard
                                               && !Parent.postFinalProficiencyToBlackboard &&
                                               !ParentList.forcePostToBlackboard
                                               && !Parent.postFinalStatToBlackboard &&
                                               !ParentList.forcePostToBlackboard)
                return;
                            
                            
            if (blackboard == null)
            {
                Debug.LogWarning($"Stat {objectName} is attempting to post to the blackboard, but " +
                                 "MainBlackboard is null. Did you forget to add the prefab to the scene?");
                return;
            }
            
            if (Owner == null)
            {
                Debug.LogWarning($"Stat {objectName} is attempting to post to the blackboard, but the " +
                                 $"owner is null.");
                return;
            }

            // For each one, if we are forcing or the parent is set to post, then post. Notify if we are forcing a
            // notification or the parent is set to notify.
            if (Parent.postPointsToBlackboard || ParentList.forcePostToBlackboard)
                blackboard.UpdateNote(Owner.GameId(), $"{objectName}-Points", Points
                    , true, ParentList.forceNotifyOnPost || Parent.notifyPointsToBlackboard);
            
            if (Parent.postFinalValueToBlackboard || ParentList.forcePostToBlackboard)
                blackboard.UpdateNote(Owner.GameId(), $"{objectName}-FinalValue", FinalValue
                    , true, ParentList.forceNotifyOnPost || Parent.notifyFinalValueToBlackboard);
            
            if (Parent.postFinalProficiencyToBlackboard || ParentList.forcePostToBlackboard)
                blackboard.UpdateNote(Owner.GameId(), $"{objectName}-FinalProficiency", FinalProficiency
                    , true, ParentList.forceNotifyOnPost || Parent.notifyFinalProficiencyToBlackboard);
            
            if (Parent.postFinalStatToBlackboard || ParentList.forcePostToBlackboard)
                blackboard.UpdateNote(Owner.GameId(), $"{objectName}-FinalStat", FinalStat()
                    , true, ParentList.forceNotifyOnPost || Parent.notifyFinalStatToBlackboard);
        }

        private (float, float) ComputeStatEffects()
        {
            var valueAdd = 0f;
            var proficiencyAdd = 0f;
            
            foreach (var stat in Parent.computeList)
            {
                // Continue if the owner does not have the Stat required
                if (!Owner.TryGetGameStat(stat.Uid(), out var gameStat)) continue;
                
                var affectedByOtherStat = gameStat.GetEffectOn(Uid());
                valueAdd += affectedByOtherStat.Item1;
                proficiencyAdd += affectedByOtherStat.Item2;
            }

            return (valueAdd, proficiencyAdd);
        }
        
        // ************************************************************************************************
        // OPERATIONAL METHODS / TOOLS
        // ************************************************************************************************

        // To avoid computing the same stat multiple times per frame, generally we will cache the
        // lastComputeTime to Time.time, and check AlreadyComputedThisFrame() before recomputing a stat.
        private bool AlreadyComputedThisFrame() => Math.Abs(lastComputedTime - timeboard.FrameTime) < 0.001f;
        private void SetLastComputeTime() => lastComputedTime = timeboard.FrameTime;

        // Returns the final stat after performing min/max actions
        private float MinMaxFinalStat(float finalStat)
        {
            if (Parent.HasMinFinal)
                finalStat = ClampMin(finalStat, Parent.minFinalType, Parent.minFinal, Parent.minFinalStat);
            
            if (Parent.HasMaxFinal)
                finalStat = ClampMax(finalStat, Parent.maxFinalType, Parent.maxFinal, Parent.maxFinalStat);
            
            return finalStat;
        }
        
        private float MinMaxValue(float finalValue)
        {
            if (Parent.HasMinBaseValue)
                finalValue = ClampMin(finalValue, Parent.minBaseValueType, Parent.minBaseValue, Parent.minBaseValueStat);
            
            if (Parent.HasMaxBaseValue)
                finalValue = ClampMax(finalValue, Parent.maxBaseValueType, Parent.maxBaseValue, Parent.maxBaseValueStat);
            
            return finalValue;
        }
        
        private float MinMaxProficiency(float finalProficiency)
        {
            if (Parent.HasMinBaseProficiency)
                finalProficiency = ClampMin(finalProficiency, Parent.minBaseProficiencyType, Parent.minBaseProficiency, Parent.minBaseProficiencyStat);
            
            if (Parent.HasMaxBaseProficiency)
                finalProficiency = ClampMax(finalProficiency, Parent.maxBaseProficiencyType, Parent.maxBaseProficiency, Parent.maxBaseProficiencyStat);
            
            return finalProficiency;
        }
        
        private float MinMaxPoints(float finalPoints)
        {
            if (Parent.HasMinPoints)
                finalPoints = ClampMin(finalPoints, Parent.minPointsType, Parent.minPoints, Parent.minPointsStat);
            
            if (Parent.HasMaxPoints)
                finalPoints = ClampMax(finalPoints, Parent.maxPointsType, Parent.maxPoints, Parent.maxPointsStat);
            
            return finalPoints;
        }

        private float ClampMax(float finalAmount, int pointsType, float pointsLimit, Stat stat = null)
        {
            if (pointsType == 1)
                return Mathf.Min(finalAmount, pointsLimit);

            if (stat == null) return finalAmount;

            if (ParentList == null)
            {
                Debug.Log($"ParentList of {objectName} is null. Did you forget to set ParentList in StartActions()?");
                return finalAmount;
            }

            if (Owner == null)
            {
                Debug.Log($"Owner is null of {objectName}. Will not clamp value.");
                return finalAmount;
            }

            if (!Owner.TryGetGameStat(stat.Uid(), out var foundOther))
                return finalAmount;
            
            finalAmount = pointsType switch
            {
                2 => Mathf.Min(finalAmount, foundOther.Points),
                3 => Mathf.Min(finalAmount, foundOther.BaseValue),
                4 => Mathf.Min(finalAmount, foundOther.BaseProficiency),
                5 => Mathf.Min(finalAmount, foundOther.FinalStat(true, true)),
                _ => finalAmount
            };

            return finalAmount;
        }
        
        private float ClampMin(float finalAmount, int pointsType, float pointsLimit, Stat stat = null)
        {
            if (pointsType == 1)
                return Mathf.Max(finalAmount, pointsLimit);
            
            if (stat == null) return finalAmount;


            if (!Owner.TryGetGameStat(stat.Uid(), out var foundOther))
                return finalAmount;

            finalAmount = pointsType switch
            {
                2 => Mathf.Max(finalAmount, foundOther.Points),
                3 => Mathf.Max(finalAmount, foundOther.BaseValue),
                4 => Mathf.Max(finalAmount, foundOther.BaseProficiency),
                5 => Mathf.Max(finalAmount, foundOther.FinalStat(true, true)),
                _ => finalAmount
            };

            return finalAmount;
        }

        /// <summary>
        ///  Resets the "other" values, getting ready for the next computation
        /// </summary>
        public void ResetOtherValueAndProficiency()
        {
            otherValue = 0; 
            otherProficiency = 0;
        }

        public string ReportOwner() => Owner.GetOwnerName();
        
        public virtual void AddDictionaryKey(string key) => dictionaries.AddNewKeyValue(key);

        public virtual KeyValue GetKeyValue(string key) => dictionaries.Key(key);

        public virtual bool HasKeyValue(string key) => dictionaries.HasKeyValue(key);
    }
}