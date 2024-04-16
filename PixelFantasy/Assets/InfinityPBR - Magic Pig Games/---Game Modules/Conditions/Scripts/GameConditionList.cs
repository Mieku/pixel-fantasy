using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static InfinityPBR.Modules.Timeboard;

/*
 * GAME CONDITION LIST
 *
 * This is for use at runtime, as a list of held Condition objects.
 *
 * There are helper methods attached to this which you may find useful.
 */

namespace InfinityPBR.Modules
{
    [HelpURL("https://infinitypbr.gitbook.io/infinity-pbr/game-modules/conditions")]
    [Serializable]
    public class GameConditionList : GameModulesList<GameCondition>, IHaveStartActions
    {
        // **********************************************************************
        // General things for all Game Module Lists
        // **********************************************************************
        
        public override string GameId() => Owner.GameId();
        public override string NoteSubject() => "Conditions";
        public GameConditionList Clone() => JsonUtility.FromJson<GameConditionList>(JsonUtility.ToJson(this));
        
        // **********************************************************************
        // Owner 
        // **********************************************************************

        public IHaveConditions Owner { get; private set; }
        public void SetOwner(IHaveConditions owner) => Owner = owner;
        private IHaveStats StatsOwner => (IHaveStats)Owner; // Conditions also has this StatsOwner property
        
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

            this.AddThisToBlackboard();
            foreach (var gameCondition in list)
            {
                gameCondition.SetParentList(this);
                gameCondition.Setup();
            }
        }
        
        // **********************************************************************
        // Affected Stats & Dirty
        // **********************************************************************

        public override void SetAffectedStatsDirty(IAmGameModuleObject gameModuleObject)
        {
            if (gameModuleObject is GameCondition gameCondition)
                Owner?.SetStatsDirty(gameCondition.DirectlyAffectsList());
        }
        
        private void SetAffectedStatsDirty(List<Stat> statList = null) 
            => StatsOwner?.SetStatsDirty(statList ?? DirectlyAffectsList());
        
        public List<Stat> DirectlyAffectsList(Stat stat = null) => ModificationLevels
            .SelectMany(x => x.targets)
            .Distinct()
            .ToList();

        public void SetDirty(string uid = null, bool dirty = true)
        {
            if (!string.IsNullOrWhiteSpace(uid))
            {
                Get(uid)?.SetDirty(dirty);
                return;
            }
            foreach (var gameCondition in list)
                gameCondition.SetDirty(dirty);
        }
        
        // **********************************************************************
        // Modification Levels
        // **********************************************************************

        public List<ModificationLevel> ModificationLevels => GetModificationLevels();
        private List<ModificationLevel> GetModificationLevels() => list.Select(x => x.ModificationLevel).ToList();
        
        // **********************************************************************
        // Standard methods for specific Scriptable Object types
        // **********************************************************************

        public GameCondition Get(Condition condition, bool addIfNull = false, bool distinct = false)
            => this.Get(condition.Uid(), addIfNull, distinct);
        
        // Note that Get() for Conditions is unique, as can take the IHaveStats source, which is the "Caster" or "giver"
        // of the condition to the object which now has the condition.
        public virtual GameCondition Get(Condition condition, bool addIfNull = false, IHaveStats source = null) => Get(condition.Uid(), addIfNull, source);
        public virtual GameCondition Get(GameCondition gameCondition, bool addIfNull = false, IHaveStats source = null) => Get(gameCondition.Uid(), addIfNull, source);
        public virtual GameCondition Get(string uid, bool addIfNull = false, IHaveStats source = null)
        {
            if (TryGet(uid, out var found))
                return found;

            return addIfNull ? Add(uid, source) : default;
        }
        
        public virtual bool TryGet(Condition condition, out GameCondition found) => TryGet(condition.Uid(), out found);
        public virtual bool TryGet(GameCondition gameCondition, out GameCondition found) => TryGet(gameCondition.Uid(), out found);
        public virtual bool TryGet(string uid, out GameCondition found)
        {
            found = Get(uid);
            return found != null;
        }

        public virtual IEnumerable<GameCondition> GetAll(Condition condition) => GetAll(condition.Uid());
        public virtual IEnumerable<GameCondition> GetAll(GameCondition gameCondition) => GetAll(gameCondition.Uid());
        public virtual IEnumerable<GameCondition> GetAll(string uid) => list.Where(x => x.Uid() == uid);

        public virtual bool TryGetAll(Condition condition, out IEnumerable<GameCondition> found) => TryGetAll(condition.Uid(), out found);
        public virtual bool TryGetAll(GameCondition gameCondition, out IEnumerable<GameCondition> found) => TryGetAll(gameCondition.Uid(), out found);
        public virtual bool TryGetAll(string uid, out IEnumerable<GameCondition> found, IHaveStats source = null)
        {
            found = GetAll(uid);
            return found.Any();
        }

        
        public virtual GameCondition GetFromSource(IHaveStats source, Condition condition) => GetFromSource(source, condition.Uid());
        public virtual GameCondition GetFromSource(IHaveStats source, GameCondition gameCondition) => GetFromSource(source, gameCondition.Uid());
        public virtual GameCondition GetFromSource(IHaveStats source, string uid = null) 
            => uid == null 
                ? list.FirstOrDefault(x => x.Source == source) 
                : TryGetFromSource(source, out var found, uid) ? found : default;

        public virtual bool TryGetFromSource(IHaveStats source, Condition condition, out GameCondition found) => TryGetFromSource(source, out found, condition.Uid());
        public virtual bool TryGetFromSource(IHaveStats source, GameCondition gameCondition, out GameCondition found) => TryGetFromSource(source, out found, gameCondition.Uid());
        public virtual bool TryGetFromSource(IHaveStats source, out GameCondition found, string uid = null)
        {
            found = GetFromSource(source, uid);
            return found != null;
        }
        
        
        public virtual IEnumerable<GameCondition> GetAllFromSource(IHaveStats source, Condition condition) => GetAllFromSource(source, condition.Uid());
        public virtual IEnumerable<GameCondition> GetAllFromSource(IHaveStats source, GameCondition gameCondition) => GetAllFromSource(source, gameCondition.Uid());
        public virtual IEnumerable<GameCondition> GetAllFromSource(IHaveStats source, string uid = null) 
            => uid == null 
                ? list.Where(x => x.Source == source) 
                : GetAll(uid).Where(x => x.Source == source);

        public virtual bool TryGetAllFromSource(IHaveStats source, Condition condition, out IEnumerable<GameCondition> found) => TryGetAllFromSource(source, out found, condition.Uid());
        public virtual bool TryGetAllFromSource(IHaveStats source, GameCondition gameCondition, out IEnumerable<GameCondition> found) => TryGetAllFromSource(source, out found, gameCondition.Uid());
        public virtual bool TryGetAllFromSource(IHaveStats source, out IEnumerable<GameCondition> found, string uid = null)
        {
            found = GetAllFromSource(source, uid);
            return found.Any();
        }
        
        public virtual GameCondition GetDisplayName(string displayName) => list.FirstOrDefault(x => x.DisplayName == displayName);
        
        public virtual bool Contains(Condition condition) => this.Contains(condition.Uid());
        
        public virtual bool ContainsDisplayName(string displayName) 
            => list.Any(gameCondition => gameCondition.DisplayName == displayName);
        
        public virtual int CountPeriodic(string uid = null) => uid == null 
            ? list.Count(x => x.Periodic) 
            : list.Count(x => x.Uid() == uid && x.Periodic);
        
        public virtual int CountInfinite(string uid = null) => uid == null 
            ? list.Count(x => x.Infinite) 
            : list.Count(x => x.Uid() == uid && x.Infinite);
        
        public virtual int CountStack(string uid = null) => uid == null 
            ? list.Count(x => x.Stack) 
            : list.Count(x => x.Uid() == uid && x.Stack);
        
        public virtual int CountStackDistinct(string uid = null) => uid == null 
            ? list.Where(x => x.Stack).Distinct().Count()
            : list.Where(x => x.Uid() == uid && x.Stack).Distinct().Count();
        
        public virtual int CountLevel(int level, string uid = null) => uid == null 
            ? list.Count(x => x.Level == level) 
            : list.Count(x => x.Uid() == uid && x.Level == level);
        
        public virtual int CountLevelGreaterThan(int level, string uid = null) => uid == null 
            ? list.Count(x => x.Level > level) 
            : list.Count(x => x.Uid() == uid && x.Level > level);
        
        public virtual int CountLevelLessThan(int level, string uid = null) => uid == null 
            ? list.Count(x => x.Level < level) 
            : list.Count(x => x.Uid() == uid && x.Level < level);
        
        public virtual int CountHasExpirationCondition(string uid = null) => uid == null 
            ? list.Count(x => x.ExpirationCondition != null) 
            : list.Count(x => x.Uid() == uid && x.ExpirationCondition != null);

        public virtual int CountExpiringBeforeRealTime(float timeToAdd, string uid = null) => uid == null 
            ? list.Count(x => x.endTime < timeboard.gametime.LaterRealTime(timeToAdd)) 
            : list.Count(x => x.Uid() == uid && x.endTime < timeboard.gametime.LaterRealTime(timeToAdd));
        
        public virtual int CountExpiringAfterRealTime(float timeToAdd, string uid = null) => uid == null 
            ? list.Count(x => x.endTime > timeboard.gametime.LaterRealTime(timeToAdd)) 
            : list.Count(x => x.Uid() == uid && x.endTime > timeboard.gametime.LaterRealTime(timeToAdd));

        public virtual int CountExpiringBefore(float seconds = 0f, float minutes = 0f, float hours = 0f, float days = 0f,
            float weeks = 0f, float months = 0f, float seasons = 0f, float years = 0f, string uid = null) => uid == null
            ? list.Count(x =>
                x.endTime < timeboard.gametime.Later(seconds, minutes, hours, days, weeks, months, seasons, years))
            : list.Count(x => 
                x.Uid() == uid && x.endTime < timeboard.gametime.Later(seconds, minutes, hours, days, weeks, months, seasons, years));
        
        public virtual int CountExpiringAfter(float seconds = 0f, float minutes = 0f, float hours = 0f, float days = 0f,
            float weeks = 0f, float months = 0f, float seasons = 0f, float years = 0f, string uid = null) => uid == null
            ? list.Count(x =>
                x.endTime > timeboard.gametime.Later(seconds, minutes, hours, days, weeks, months, seasons, years))
            : list.Count(x => 
                x.Uid() == uid && x.endTime > timeboard.gametime.Later(seconds, minutes, hours, days, weeks, months, seasons, years));
        
        public virtual int CountExpiringBefore(TimeSpan timeSpan, string uid = null) => uid == null
            ? list.Count(x => x.endTime < timeboard.gametime.Later(timeSpan))
            : list.Count(x => x.Uid() == uid && x.endTime < timeboard.gametime.Later(timeSpan));
        
        public virtual int CountExpiringAfter(TimeSpan timeSpan, string uid = null) => uid == null
            ? list.Count(x => x.endTime > timeboard.gametime.Later(timeSpan))
            : list.Count(x => x.Uid() == uid && x.endTime > timeboard.gametime.Later(timeSpan));

        public virtual GameCondition Add(string uid, IHaveStats source = null)
        {
            var gameCondition = new GameCondition(GameModuleRepository.Instance.Get<Condition>(uid), this, source);
            if (!gameCondition.Stack && ContainsDisplayName(gameCondition.DisplayName))
                return UpdateCondition(gameCondition);
            
            return Add(gameCondition, source);
        }
        
        public virtual GameCondition Add(Condition condition, IHaveStats source = null)
        {
            var gameCondition = new GameCondition(condition, this, source);
            if (!gameCondition.Stack && ContainsDisplayName(gameCondition.DisplayName))
                return UpdateCondition(gameCondition);
            
            return Add(gameCondition, source);
        }

        public virtual GameCondition Add(GameCondition gameCondition, IHaveStats source = null)
        {
            list.Add(gameCondition);
            gameCondition.SetParentList(this);
            gameCondition.SetDirty();
            gameCondition.Setup();
            this.AddThisToBlackboard();
            if (gameCondition.Instant) return default;
            return list[^1];
        }
        
        public virtual void Remove(Condition condition) => this.Remove(condition.Uid());
        public virtual void RemoveAll(Condition condition) => this.RemoveAll(condition.Uid());
        
        public virtual void RemoveAllFromSource(IHaveStats source, Condition condition) => RemoveAllFromSource(source, condition.Uid());
        public virtual void RemoveAllFromSource(IHaveStats source, GameCondition gameCondition) => RemoveAllFromSource(source, gameCondition.Uid());
        public virtual void RemoveAllFromSource(IHaveStats source, string conditionUid = null)
        {
            if (conditionUid == null)
                list.RemoveAll(x => x.Source == source);
            else
                list.RemoveAll(x => x.Uid() == conditionUid && x.Source == source);
        }
        
        public virtual void ExpireAllFromSource(IHaveStats source, Condition condition) => ExpireAllFromSource(source, condition.Uid());
        public virtual void ExpireAllFromSource(IHaveStats source, GameCondition gameCondition) => ExpireAllFromSource(source, gameCondition.Uid());
        public virtual void ExpireAllFromSource(IHaveStats source, string conditionUid = null)
        {
            foreach(var condition in GetAllFromSource(source, conditionUid))
                condition.ExpireNow();
        }

        
        // **********************************************************************
        // Transfering
        // **********************************************************************

        public virtual GameCondition ReceiveTransfer(GameCondition gameCondition, bool makeClone = true)
        {
            if (gameCondition == null) return default;
            
            if (!gameCondition.Stack && ContainsDisplayName(gameCondition.DisplayName))
            {
                return UpdateCondition(gameCondition);
            }

            var clone = makeClone ? gameCondition.Clone() : gameCondition;
            clone.SetParentList(this);
            list.Add(clone);
            clone.SetDirty();
            clone.Setup();
            this.AddThisToBlackboard();
            return clone;
        }

        public virtual GameCondition TransferTo(GameConditionList transferTo, GameCondition transferObject, object newOwner = null, bool distinct = false)
        {
            transferObject.SetDirty();
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
        
        public virtual void ExpirationActions(GameCondition gameCondition)
        {
            AddExpirationCondition(gameCondition);
            this.RemoveExact(gameCondition);
            this.AddThisToBlackboard();
        }

        protected virtual void AddExpirationCondition(GameCondition gameCondition)
        {
            if (!gameCondition.ExpirationCondition) return;
            
            // If we have a custom handler, use it, then return
            if (gameCondition.ExpirationConditionHandler != null)
            {
                gameCondition.ExpirationConditionHandler.HandleExpiration(Owner, gameCondition.ExpirationCondition,
                    gameCondition.Source);
                return;
            }

            // No custom handler, add the expiration condition
            Add(gameCondition.ExpirationCondition.Uid());
        }

        protected virtual void PeriodicActions()
        {
            // Check each gameCondition in the list
            foreach (GameCondition gameCondition in list)
            {
                if (!gameCondition.PeriodicReady()) continue; // Skip if we aren't ready to do the action
                gameCondition.PeriodicActions(); // Do the action
            }
        }

        // Will replace details if the level of the new one is higher, and will always update the end time.
        protected virtual GameCondition UpdateCondition(GameCondition gameCondition)
        {
            var existingCondition = GetDisplayName(gameCondition.DisplayName);
            var greaterTime = Mathf.Max(existingCondition.endTime, gameCondition.endTime);
            if (gameCondition.Level <= existingCondition.Level) // Level is not greater, so only update time
            {
                existingCondition.endTime = greaterTime; // Set time to make sure it's the greater of the two
                return existingCondition;
            }
            
            this.Remove(existingCondition); // Remove the old
            list.Add(gameCondition); // Add the new
            gameCondition.endTime = greaterTime; // Set time to make sure it's the greater of the two
            Debug.Log("UPDATE CONDITION SHOULD SET DIRT");
            gameCondition.SetDirty();
            this.AddThisToBlackboard();
            return gameCondition;
        }
        
        
        


        
        // June 4, 2023 -- These are not used right now. Perhaps in the future. Not sure if we will have individual objects of this type post to the blackboard
        // Handles force posting for individual objects in the list
        public bool forcePostToBlackboard; // If true, objects will post to blackboard even if they are individual set to not post
        public bool forceNotifyOnPost; // If true, when objects post, they will be forced to notify followers of the blackboard
    }
}