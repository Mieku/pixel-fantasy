using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InfinityPBR.Modules
{
    [Serializable]
    public class GameStatListSavedData
    {
        public bool postListToBlackboard;
        public bool notifyOnPost;
        public string noteSubject;

        public List<string> gameStats = new List<string>();
    }
    
    [HelpURL("https://infinitypbr.gitbook.io/infinity-pbr/game-modules/stats-and-skills")]
    [Serializable]
    public class GameStatList : GameModulesList<GameStat>, IHaveStartActions
    {
        // **********************************************************************
        // General things for all Game Module Lists
        // **********************************************************************
        
        public override string GameId() => Owner.GameId();
        public override string NoteSubject() => "Stats";
        public GameStatList Clone() => JsonUtility.FromJson<GameStatList>(JsonUtility.ToJson(this));
        
        // **********************************************************************
        // Owner 
        // **********************************************************************
        
        public IHaveStats Owner { get; private set; }
        public void SetOwner(IHaveStats value)
        {
            Owner = value;
            UpdateParentListReferenceInChildren(); // Update the Stats in the list
        }

        // **********************************************************************
        // Start Actions
        //
        // These are actions that must be required when the application loads. At Start() on your manager script (often
        // the script which holds this object), start this coroutine. It will wait until the blackboard is present and
        // available.
        // **********************************************************************
        
        public virtual IEnumerator StartActions()
        {
            // Wait for blackboard but also to ensure the Owner has been populated.
            while (MainBlackboard.blackboard == null || Owner == null)
                yield return null;

            UpdateParentListReferenceInChildren();
            this.AddThisToBlackboard();
        }
        
        // This is called by the StartActions() coroutine. It will update the parent list reference in all children, 
        // ensuring the Stats know which parent they have.
        public virtual void UpdateParentListReferenceInChildren()
        {
            foreach (var gameStat in list)
                gameStat.SetParentList(this);
        }
        
        // **********************************************************************
        // Affected Stats & Dirty
        // **********************************************************************
        
        public override void SetAffectedStatsDirty(IAmGameModuleObject gameModuleObject)
        {
            if (gameModuleObject is GameStat gameStat)
                Owner?.SetStatsDirty(gameStat.DirectlyAffectsList());
        }
        
        public void SetAffectedStatsDirty(List<Stat> statList = null) 
            => Owner?.SetStatsDirty(statList ?? DirectlyAffectsList());

        public List<Stat> DirectlyAffectsList(Stat stat = null) => ModificationLevels
            .SelectMany(x => x.targets)
            .Distinct()
            .ToList();

        public void SetDirty() => SetAffectedStatsDirty();
        
        // **********************************************************************
        // Modification Levels
        // **********************************************************************
        
        public virtual List<ModificationLevel> ModificationLevels => GetModificationLevels();
        protected virtual List<ModificationLevel> GetModificationLevels()
        {
            var modificationLevels = new List<ModificationLevel>();
            list.ForEach(x => modificationLevels.Add(x.ModificationLevel));
            return modificationLevels;
        }
        
        // **********************************************************************
        // Standard methods for specific Scriptable Object types
        // **********************************************************************
        
        public virtual GameStat Get(Stat stat, bool addIfNull = false, bool distinct = false)
            => this.Get(stat.Uid(), addIfNull, distinct);
        
        public virtual IEnumerable<GameStat> GetAllCounters() => list.Where(s => s.Counter);

        public virtual IEnumerable<GameStat> GetAllTrainable() => list.Where(s => s.Trainable);

        public virtual IEnumerable<GameStat> GetAllModifiable() => list.Where(s => s.Modifiable);

        public virtual IEnumerable<GameStat> GetAllTrainableAndModifiable() => list.Where(s => s.Trainable && s.Modifiable);

        public virtual IEnumerable<GameStat> GetAllIsModified() => list.Where(s => s.IsModified);

        public virtual IEnumerable<GameStat> GetAllWhereFinalStatGreaterThanPointsAndValue() => list.Where(s => s.FinalStatGreaterThanPointsAndValue);

        public virtual IEnumerable<GameStat> GetAllWhereFinalStatLessThanPointsAndValue() => list.Where(s => s.FinalStatLessThanPointsAndValue);

        public virtual IEnumerable<GameStat> GetAllFinalStatGreaterThan(float value) => list.Where(s => s.FinalValue > value);

        public virtual IEnumerable<GameStat> GetAllFinalStatLessThan(float value) => list.Where(s => s.FinalValue < value);

        public virtual IEnumerable<GameStat> GetAllFinalStatIs(float value) => list.Where(s => Math.Abs(s.FinalValue - value) < 0.01);

        public virtual IEnumerable<GameStat> GetAllPointsGreaterThan(float value) => list.Where(s => s.Points > value);

        public virtual IEnumerable<GameStat> GetAllPointsLessThan(float value) => list.Where(s => s.Points < value);

        public virtual IEnumerable<GameStat> GetAllPointsIs(float value) => list.Where(s => Math.Abs(s.Points - value) < 0.01);

        public virtual IEnumerable<GameStat> GetAllProficiencyGreaterThan(float value) => list.Where(s => s.FinalProficiency > value);

        public virtual IEnumerable<GameStat> GetAllProficiencyLessThan(float value) => list.Where(s => s.FinalProficiency < value);

        public virtual IEnumerable<GameStat> GetAllProficiencyIs(float value) => list.Where(s => Math.Abs(s.FinalProficiency - value) < 0.01);

        public virtual IEnumerable<GameStat> GetAllValueGreaterThan(float value, bool includePoints = true) => list.Where(s => (includePoints ? s.Points : 0) + s.BaseValue > value);

        public virtual IEnumerable<GameStat> GetAllValueLessThan(float value, bool includePoints = true) => list.Where(s => (includePoints ? s.Points : 0) + s.BaseValue < value);

        public virtual IEnumerable<GameStat> GetAllValueIs(float value, bool includePoints = true) => list.Where(s => Math.Abs((includePoints ? s.Points : 0) + s.BaseValue - value) < 0.01);

        
        public virtual bool Contains(Stat stat) => this.Contains(stat.Uid());
        
        public virtual void Remove(Stat stat) => this.Remove(stat.Uid());
        
        public virtual void RemoveAll(Stat stat) => this.RemoveAll(stat.Uid());
        
        public virtual GameStat Add(Stat stat, bool distinct = true) => Add(stat.Uid(), distinct);
        public override GameStat Add(string uid, bool distinct = true) => Add(uid, distinct);
        public virtual GameStat Add(GameStat gameStat, bool distinct = true) => Add(gameStat.Parent.Uid(), distinct);
        public virtual GameStat Add(string uid, bool distinct = true, bool setDirty = true)
        {
            if (distinct && this.Contains(uid)) return this.Get(uid);

            list.Add(new GameStat(GameModuleRepository.Instance.Get<Stat>(uid), this));
            list[^1].SetDirty(setDirty);
            this.AddThisToBlackboard();
            return list[^1];
        }

        /// <summary>
        /// Returns a <string, float> Dictionary with the FinalStat for each GameStat in the list. The key is either the
        /// ObjectName of the stat or the Uid, and we can optionally recompute all values prior to returning the list.
        /// </summary>
        /// <param name="keyIsObjectName"></param>
        /// <param name="recompute"></param>
        /// <param name="recomputeTree"></param>
        /// <returns></returns>
        public virtual Dictionary<string, float> GetAllFinalStats(bool keyIsObjectName = true, bool recompute = false, bool recomputeTree = false)
        {
            var newDictionary = new Dictionary<string, float>();
            foreach (var gameStat in list)
                newDictionary.Add(keyIsObjectName ? gameStat.ObjectName() : gameStat.Uid(), gameStat.FinalStat(recompute, recomputeTree));

            return newDictionary;
        }

        /// <summary>
        /// Returns a <string, float> Dictionary with the Points for each GameStat in the list. The key is either the
        /// ObjectName of the stat or the Uid. If countersOnly is false, all stats will be returned, otherwise only
        /// those which are "Counters" will be returned (i.e. those which cannot be trained or modified).
        /// </summary>
        /// <param name="keyIsObjectName"></param>
        /// <param name="countersOnly"></param>
        /// <param name="includeBaseValue"></param>
        /// <returns></returns>
        public virtual Dictionary<string, float> GetAllPoints(bool keyIsObjectName = true, bool countersOnly = true, bool includeBaseValue = false)
        {
            var newDictionary = new Dictionary<string, float>();
            foreach (var gameStat in list)
            {
                if (countersOnly && gameStat.Counter != true) continue; // Skip anything that can be trained or modified
                newDictionary.Add(keyIsObjectName 
                    ? gameStat.ObjectName() 
                    : gameStat.Uid(),gameStat.Points + (includeBaseValue ? gameStat.BaseValue : 0));
            }

            return newDictionary;
        }
        
        /// <summary>
        /// Returns all GameStats that are Counters, as a <string, GameStat> Dictionary. The key is either the
        /// ObjectName of the stat or the Uid.
        /// </summary>
        /// <param name="keyIsObjectName"></param>
        /// <returns></returns>
        public virtual Dictionary<string, GameStat> GetAllCounterStats(bool keyIsObjectName = true)
        {
            var newDictionary = new Dictionary<string, GameStat>();
            foreach (var gameStat in list)
            {
                if (!gameStat.Counter) continue;
                newDictionary.Add(keyIsObjectName ? gameStat.ObjectName() : gameStat.Uid(), gameStat);
            }

            return newDictionary;
        }

        /// <summary>
        /// Returns all GameStats that are trainable, as a <string, GameStat> Dictionary. The key is either the
        /// ObjectName of the stat or the Uid.
        /// </summary>
        /// <param name="keyIsObjectName"></param>
        /// <returns></returns>
        public virtual Dictionary<string, GameStat> GetAllTrainableStats(bool keyIsObjectName = true)
        {
            var newDictionary = new Dictionary<string, GameStat>();
            foreach (var gameStat in list)
            {
                if (!gameStat.Trainable) continue;
                newDictionary.Add(keyIsObjectName ? gameStat.ObjectName() : gameStat.Uid(), gameStat);
            }

            return newDictionary;
        }
        
        /// <summary>
        /// Returns all GameStats that are Modifiable, as a <string, GameStat> Dictionary. The key is either the
        /// ObjectName of the stat or the Uid.
        /// </summary>
        /// <param name="keyIsObjectName"></param>
        /// <returns></returns>
        public virtual Dictionary<string, GameStat> GetAllModifiableStats(bool keyIsObjectName = true)
        {
            var newDictionary = new Dictionary<string, GameStat>();
            foreach (var gameStat in list)
            {
                if (!gameStat.Modifiable) continue;
                newDictionary.Add(keyIsObjectName ? gameStat.ObjectName() : gameStat.Uid(), gameStat);
            }

            return newDictionary;
        }
        
        /// <summary>
        /// Returns all GameStats that are Modifiable, as a <string, GameStat> Dictionary. The key is either the
        /// ObjectName of the stat or the Uid.
        /// </summary>
        /// <param name="keyIsObjectName"></param>
        /// <returns></returns>
        public virtual Dictionary<string, GameStat> GetAllTrainableAndModifiableStats(bool keyIsObjectName = true)
        {
            var newDictionary = new Dictionary<string, GameStat>();
            foreach (var gameStat in list)
            {
                if (!gameStat.Modifiable || !gameStat.Trainable) continue;
                newDictionary.Add(keyIsObjectName ? gameStat.ObjectName() : gameStat.Uid(), gameStat);
            }

            return newDictionary;
        }

        /// <summary>
        /// Returns all GameStats that are currently modified, as a <string, GameStat> Dictionary. The key is either the
        /// ObjectName of the stat or the Uid.
        /// </summary>
        /// <param name="keyIsObjectName"></param>
        /// <param name="includeAll">If true, all that are modified (i.e. greater or less than Points + BaseValue) will be included</param>
        /// <param name="greaterThan">If includeAll is false, this handles whether we include only those greater or less than the Points + BaseValue</param>
        /// <returns></returns>
        public virtual Dictionary<string, GameStat> GetAllModifiedStats(bool keyIsObjectName = true, bool includeAll = true, bool greaterThan = true)
        {
            var newDictionary = new Dictionary<string, GameStat>();
            foreach (var gameStat in list)
            {
                if (!gameStat.IsModified) continue; // Skip if not modified
                if (!includeAll & greaterThan && !gameStat.FinalStatGreaterThanPointsAndValue) continue; // Skip if not greater than
                if (!includeAll & !greaterThan && !gameStat.FinalStatLessThanPointsAndValue) continue; // Skip if not less than

                newDictionary.Add(keyIsObjectName ? gameStat.ObjectName() : gameStat.Uid(), gameStat);
            }

            return newDictionary;
        }

        // **********************************************************************
        // Transfering
        // **********************************************************************
        
        public virtual GameStat ReceiveTransfer(GameStat gameStat, bool distinct = true, bool makeClone = true, bool setDirty = true)
        {
            if (gameStat == null) return default;
            
            // If we already have and object of this type in the list, and they must be distinct, return the object we already have.
            if (distinct && this.Contains(gameStat.Uid())) 
                return this.Get(gameStat.Uid());
            
            var clone = makeClone ? gameStat.Clone() : gameStat; // Make a clone, or use the object being transferred
            //clone.SetOwner(Owner); // Set the owner of the new object to this
            clone.SetParentList(this);
            list.Add(clone); // Add it to the list
            list[^1].SetDirty(setDirty); // Make sure it recomputes FinalValue
            this.AddThisToBlackboard();
            return clone;
        }

        public virtual object TransferTo(GameStatList transferTo, GameStat transferObject, bool distinct = false, bool setDirty = true)
        {
            transferObject.SetDirty(setDirty);
            var objectAtDestination = transferTo.ReceiveTransfer(transferObject, distinct);
            
            // If distinct = true, transfer may fail if an object like this is already present.
            if (objectAtDestination == null)
                return null;
            
            this.RemoveExact(transferObject);
            this.AddThisToBlackboard();
            return objectAtDestination;
        }
        
        // **********************************************************************
        // Additional Methods, Variables, and Properties for this type
        // **********************************************************************
        
        // Handles force posting for individual objects in the list
        public bool forcePostToBlackboard; // If true, objects will post to blackboard even if they are individual set to not post
        public bool forceNotifyOnPost; // If true, when objects post, they will be forced to notify followers of the blackboard

        public virtual void PostStatsToBlackboard()
        {
            foreach (var stat in list)
                stat.PostToBlackboard();
        }
    }
}