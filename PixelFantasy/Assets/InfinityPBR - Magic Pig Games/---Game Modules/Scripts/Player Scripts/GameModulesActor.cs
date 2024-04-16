using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static InfinityPBR.Modules.MainBlackboard; // Blackboard Module

/*
 * This is a player script that is fully set up with most of the Game Modules. You can inherit your player class from
 * this, and override any methods you'd like. Please note: This is not a Monobehaviour, and the Tick() or LateTick()
 * methods should be called from the Monobehaviour object which holds your player class.
 *
 * This class has an inventory and equipment, both GameItemObject lists, and methods to transfer values between them,
 * and add/remove to each, and so on. However, this class is not set up to work with the visual drag-and-drop
 * "Inventory System" which comes with the Game Modules. If you'd like to use that system, please inherit from
 * GameModulesInventoryActor instead.
 *
 * Game Modules have been made to be largely automatic. Once set up, many of their operations will happen without
 * additional code. In some cases, you may wish to override methods, or create your own classes or scripts to handle
 * unique use cases for your project. Many automatic operations can optionally be manually called, if you choose.
 *
 * For questions & support, visit the Discord, linked from www.InfinityPBR.com
 * For documentation and tutorial videos, visit the online documentation: https://infinitypbr.gitbook.io/infinity-pbr/
 */

namespace InfinityPBR.Modules
{
    [Serializable]
    public class GameModulesActorSavedData
    {
        public string stats;
        public string conditions;
        public string quests;
        public string dictionaries;
        public string inventory;
        public string equipment;
        public string gameId;
    }
    
    [Serializable]
    public class GameModulesActor : IUseGameModules, IHaveDictionaries, IHaveStats, IHaveConditions, IHaveQuests, IHaveStartActions
    {
        [Header("Game Modules Lists")]
        public GameStatList stats = new GameStatList();
        public GameConditionList conditions = new GameConditionList();
        public GameQuestList quests = new GameQuestList();
        public Dictionaries dictionaries = new Dictionaries();
        
        public GameItemObjectList inventoryItems = new GameItemObjectList(); // Items in the inventory
        public GameItemObjectList equippedItems = new GameItemObjectList(); // Items equipped on the actor
        public GameItemObject LastItemAdded => inventoryItems.Last();
        public GameItemObject LastEquipmentAdded => equippedItems.Last();
        
        protected List<ModificationLevel> _modificationLevels;

        public virtual IEnumerator StartActions()
        {
            yield return new WaitUntil(() => blackboard != null);
            SetOwner(this);

            yield return new WaitUntil(() => SaveAndLoad.saveAndLoad.IsLoaded);

            //RegisterQuestActorWithModulesHelper();
        }

        /*
        public void RegisterQuestActorWithModulesHelper()
        {
            if (quests.Count() > 0)
                ModulesHelper.Instance.RegisterQuestList(this);
        }

        public void UnregisterQuestActorWithModulesHelper()
        {
            if (quests.Count() == 0)
                ModulesHelper.Instance.UnregisterQuestList(this); 
        }
        */

        public GameItemObjectList InventoryAndEquipmentList()
        {
            var equipmentAndInventory = new GameItemObjectList();
            equipmentAndInventory.list.AddRange(inventoryItems.list);
            equipmentAndInventory.list.AddRange(equippedItems.list);
            return equipmentAndInventory;
        }


        // ------------------------------------------------------------------------------------------
        // IHaveGameId
        // ------------------------------------------------------------------------------------------
        
        [Header("Game ID")]
        [SerializeField] private string _gameId; // This will be created at runtime, but should never change after that.
        
        // GameId is a unique ID that is required to identify specific in-game objects. Generally, a GUID is created, 
        // but you can set this to a specific value if you'd like. It must be unique, so ensure that the value you 
        // select is unique, and specific to the object in question.
        public virtual string GameId(bool forceNew = false) =>
            string.IsNullOrWhiteSpace(_gameId) || forceNew
                ? GenerateNewGameId()
                : _gameId;
        
        public virtual string GenerateNewGameId() => _gameId = Guid.NewGuid().ToString();

        // ------------------------------------------------------------------------------------------
        // IUseGameModules
        // ------------------------------------------------------------------------------------------
        
        /*
         * Tick() should be called from the class which holds your player data. This and other
         * GameModules classes are not Monobehaviours, so do not have Update() and LateUpdate().
         */
        public virtual void Tick()
        {
            quests.Tick(); // Note: Aug 4 2023 removed LateTick, moved this to ModulesHelper.
        }

        // You may have a variable for the name of the Actor, or some other way of utilizing "name". If so, you can
        // override this method in a derived class so it returns the value you prefer.
        public virtual string GetOwnerName() => GameId();
        
        // Owner, in this context, is this object as IHaveStats, or IHaveConditions etc. Those modules utilize the owner
        // for proper data management. Calling this method will call the SetOwner() method on all of the lists, which 
        // will subsequently call it on all of the items in the lists as well.
        public virtual void SetOwner(object newOwner)
        {
            stats.SetOwner((IHaveStats)newOwner);
            conditions.SetOwner((IHaveConditions)newOwner);
            quests.SetOwner((IHaveQuests)newOwner);
            inventoryItems.SetOwner((IHaveStats)newOwner);
            equippedItems.SetOwner((IHaveStats)newOwner);
        }

        // ------------------------------------------------------------------------------------------
        // IHaveDictionaries
        // ------------------------------------------------------------------------------------------

        /*
         * Dictionaries is a module that holds data in a serializable dictionary-like format (key/value pairs). There
         * are built in methods to make use of, and the dictionaries can hold GameModules runtime objects, automatically
         * linking them to their Scriptable Object parents at runtime. Please check out the Dictionaries class for
         * additional methods to make use of.
         */
        public virtual void AddDictionaryKey(string key) => dictionaries.AddNewKeyValue(key);
        public virtual KeyValue GetKeyValue(string key) => dictionaries.Key(key);
        public virtual bool HasKeyValue(string key) => dictionaries.HasKeyValue(key);
        public void CheckForMissingObjectReferences()
        {
            // Unused in this context.
        }

        // ------------------------------------------------------------------------------------------
        // IHaveStats
        // ------------------------------------------------------------------------------------------
        
        // This will NOT add the stat to the list if it is not already there. Set true if you'd like.
        /// <summary>
        /// Gets a GameStat by its uid. Returns null if not found. This will not add the GameStat if it is
        /// not found. If you'd like to Get the GameStat and add it if it is not found, use GetStat() instead.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="gameStat"></param>
        /// <param name="addIfNull"></param>
        /// <returns></returns>
        public virtual bool TryGetGameStat(string uid, out GameStat gameStat)
        {
            gameStat = GetStat(uid, false);
            return gameStat != null;
        }

        /// <summary>
        /// Gets a GameStat by the provided Stat. Returns null if not found. This will not add the GameStat if it is
        /// not found. If you'd like to Get the GameStat and add it if it is not found, use GetStat() instead.
        /// </summary>
        /// <param name="stat"></param>
        /// <param name="gameStat"></param>
        /// <returns></returns>
        public virtual bool TryGetGameStat(Stat stat, out GameStat gameStat) 
            => TryGetGameStat(stat.Uid(), out gameStat);

        /// <summary>
        /// Resets the _modificationLevels list, and optionally recomputes and caches the list.
        /// </summary>
        /// <param name="cache"></param>
        public virtual void ResetOtherLevels(bool cache = false)
        {
            _modificationLevels = null;
            
            if (cache) GetOtherLevels(true);
        }
        
        /// <summary>
        /// Gets the other ModificationLevels which affect stats from the other GameObject lists on this object, such
        /// as the conditions, quests, and equipped items.
        /// </summary>
        /// <param name="cache"></param>
        /// <returns></returns>
        public virtual List<ModificationLevel> GetOtherLevels(bool cache = false)
        {
            if (_modificationLevels != null && !cache)
            {
                return _modificationLevels;
            }

            var otherModificationLevels = new List<ModificationLevel>();
            
            otherModificationLevels.AddRange(conditions.ModificationLevels);
            otherModificationLevels.AddRange(quests.ModificationLevels);
            otherModificationLevels.AddRange(equippedItems.ModificationLevels);

            _modificationLevels = otherModificationLevels;
            return otherModificationLevels;
        }

        /// <summary>
        /// Provided a list of Stat objects, if this stats list contains those, they will be SetDirty(true). This is
        /// used in the automatic re-computation of stat Final Values in automatic operations between Modules.
        ///
        /// This will also Reset the cache of the "Other Levels" used in the GetOtherLevels() method.
        /// </summary>
        /// <param name="statList"></param>
        public virtual void SetStatsDirty(List<Stat> statList)
        {
            ResetOtherLevels(); // Set this to cache next time it is required
            foreach (var stat in statList)
            {
                if (!TryGetGameStat(stat, out var gameStat))
                {
                    continue;
                }
                gameStat.SetDirty();
            }
        }
        
        // ---- ADDITIONAL HELPER METHODS ----
        /// <summary>
        /// Returns true if a stat with the provided uid is found in the GameStatList
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public virtual bool HasStat(string uid) => stats.Contains(uid);
        
        /// <summary>
        /// Returns true if a stat with the provided Stat object is found in the GameStatList
        /// </summary>
        /// <param name="stat"></param>
        /// <returns></returns>
        public virtual bool HasStat(Stat stat) => HasStat(stat.Uid());

        /// <summary>
        /// Adds a vanilla GameStat of the stat by the provided uid to the GameStatList.
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public virtual GameStat AddStat(string uid) => stats.Add(uid);
        
        /// <summary>
        /// Adds a vanilla GameStat of the provided Stat object to the GameStatList.
        /// </summary>
        /// <param name="stat"></param>
        /// <returns></returns>
        public virtual GameStat AddStat(Stat stat) => AddStat(stat.Uid());

        /// <summary>
        /// Removes one or all GameStats by the provided uid from the GameStatList.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="removeAll"></param>
        public virtual void RemoveStat(string uid, bool removeAll = false)
        {
            if (removeAll)
                stats.RemoveAll(uid);
            else
                stats.Remove(uid);
        }
        
        /// <summary>
        /// Removes one or all GameStats by the provided Stat object from the GameStatList.
        /// </summary>
        /// <param name="stat"></param>
        /// <param name="removeAll"></param>
        public virtual void RemoveStat(Stat stat, bool removeAll = false) => RemoveStat(stat.Uid(), removeAll);

        /// <summary>
        /// Removes a GameStat from GameStatList based on it's unique runtime GameId. This is used to remove a
        /// specific GameStat, especially when the list contains multiple GameStats of the same Stat, but with different
        /// values.
        /// </summary>
        /// <param name="gameId"></param>
        public virtual void RemoveStatByGameId(string gameId) => stats.RemoveGameId(gameId);

        /// <summary>
        /// Get a GameStat by the provided uid. If addIfNull is true, it will add a vanilla GameStat and return it.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="addIfNull"></param>
        /// <returns></returns>
        public virtual GameStat GetStat(string uid, bool addIfNull = true) => stats.Get(uid, addIfNull);

        public IEnumerator SetStats()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get a GameStat by the provided Stat object. If addIfNull is true, it will add a vanilla GameStat and return it.
        /// </summary>
        /// <param name="stat"></param>
        /// <param name="addIfNull"></param>
        /// <returns></returns>
        public virtual GameStat GetStat(Stat stat, bool addIfNull = true) => GetStat(stat.Uid(), addIfNull);
        
        /// <summary>
        /// Recompute the value of a GameStat based on the uid provided, along with all Stats that are affected by it.
        /// </summary>
        /// <param name="uid"></param>
        public virtual void RecomputeStat(string uid) => GetStat(uid).SetDirty();

        /// <summary>
        /// Recompute the value of a GameStat based on the Stat object provided, along with all Stats that are affected by it.
        /// </summary>
        /// <param name="stat"></param>
        public virtual void RecomputeStat(Stat stat) => RecomputeStat(stat.Uid());

        /// <summary>
        /// Recompute the value of a GameStat based on the GameId provided, along with all Stats that are affected by it. If
        /// the GameId is not found, nothing will happen.
        /// </summary>
        /// <param name="gameId"></param>
        public virtual void RecomputeStatByGameId(string gameId)
        {
            if (stats.TryGetGameId(gameId, out var gameStat))
                gameStat.SetDirty();
        }
        
        /// <summary>
        /// Returns the number of stats which can be modified
        /// </summary>
        /// <returns></returns>
        public virtual int CountStatsCanBeModified() => stats.list.Count(x => x.Parent.canBeModified);
        
        /// <summary>
        /// Returns the number of stats which can be trained
        /// </summary>
        /// <returns></returns>
        public virtual int CountStatsCanBeTrained() => stats.list.Count(x => x.Parent.canBeTrained);
        
        /// <summary>
        /// Returns the number of stats which can be trained
        /// </summary>
        /// <returns></returns>
        public virtual int CountStatsCanBeTrainedAndModified() => stats.list.Count(x => x.Parent.canBeTrained && x.Parent.canBeModified);
        
        /// <summary>
        /// Returns the number of stats which can be trained
        /// </summary>
        /// <returns></returns>
        public virtual int CountStatsCounters() => stats.list.Count(x => !x.Parent.canBeTrained && !x.Parent.canBeModified);
        
        // ------------------------------------------------------------------------------------------
        // IHaveConditions
        // ------------------------------------------------------------------------------------------
        
        /// <summary>
        /// Used by the Conditions module, will add points to a specific stat if the stat exists in the GameStatList.
        /// </summary>
        /// <param name="statUid"></param>
        /// <param name="pointValue"></param>
        public virtual void ConditionPointEffect(string statUid, float pointValue)
        {
            if (!stats.TryGet(statUid, out var gameStat)) return;

            gameStat.AddPoints(pointValue);
        }

        /* // June 8 2023, ConditionPointEffect() is now called from DoInstantActions() on the GameCondition. This
         // method is no longer required.
        public void DoInstantActions(GameCondition gameCondition)
        {
            throw new NotImplementedException();
        } */

        /// <summary>
        /// Pass a complete GameCondition to the GameConditionList, and set stats dirty, then do any Instant Actions.
        /// </summary>
        /// <param name="gameCondition"></param>
        /// <returns></returns>
        public virtual GameCondition ReceiveCondition(GameCondition gameCondition)
        {
            var transferred = conditions.ReceiveTransfer(gameCondition);
            SetStatsDirty(transferred.ModificationLevel.targets);
            transferred.DoInstantActions();
            return transferred;
        }
        
        /// <summary>
        /// Creates a new GameCondition of the provided Condition and transfers it to the GameConditionList, setting
        /// stats dirty and performing any Instant Actions.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public virtual GameCondition AddCondition(Condition condition, IHaveStats source = null) 
            => AddCondition(condition.Uid(), source);

        /// <summary>
        /// Creates a new GameCondition of the provided Condition uid and transfers it to the GameConditionList, setting
        /// stats dirty and performing any Instant Actions.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public virtual GameCondition AddCondition(string uid, IHaveStats source = null) 
            => ReceiveCondition(new GameCondition(GameModuleRepository.Instance.Get<Condition>(uid), conditions, source));

        /*
         June 8, 2023 -- this appears to be not needed. It was not being used, and GameConditionList does not have a
         method which calls this.
        // GameConditionsList will call this to ensure stats affected by Conditions are re-computed. In this context,
        // the stat will NOT be added if it does not exist.
        public virtual void SetStatDirty(string uid, bool dirty = true) => GetStat(uid, false)?.SetDirty();
        */
        
        // ---- ADDITIONAL HELPER METHODS ----
        /// <summary>
        /// Get a GameCondition by the provided uid.
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public virtual GameCondition GetCondition(string uid) => conditions.Get(uid);
        
        /// <summary>
        /// Get a GameCondition by the provided Condition object.
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public virtual GameCondition GetCondition(Condition condition) => GetCondition(condition.Uid());

        /// <summary>
        /// Get a list of all conditions by the provided uid.
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public virtual IEnumerable<GameCondition> GetAllConditions(string uid) => conditions.GetAll(uid);
        
        /// <summary>
        /// Get a list of all conditions by the provided Condition object.
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public virtual IEnumerable<GameCondition> GetAllConditions(Condition condition) => GetAllConditions(condition.Uid());

        /// <summary>
        /// Returns true if the provided uid is in the GameConditionList.
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public virtual bool HasCondition(string uid) => conditions.Contains(uid);
        
        /// <summary>
        /// Returns true if the provided Condition object is in the GameConditionList.
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public virtual bool HasCondition(Condition condition) => conditions.Contains(condition.Uid());

        // This will give a condition to another IHaveConditions object, passing in this as IHaveStats source
        /// <summary>
        /// Give a condition to another IHaveConditions object, passing in this as IHaveStats source
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="newOwner"></param>
        public virtual void GiveCondition(string uid, IHaveConditions newOwner) => newOwner.AddCondition(uid, this);
        
        /// <summary>
        /// Give a condition to another IHaveConditions object, passing in this as IHaveStats source
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="newOwner"></param>
        public virtual void GiveCondition(Condition condition, IHaveConditions newOwner) =>
            GiveCondition(condition.Uid(), newOwner);

        /// <summary>
        /// Remove a condition by the provided uid. 
        /// </summary>
        /// <param name="uid"></param>
        
        public virtual void RemoveCondition(string uid) => conditions.Remove(uid);
        
        /// <summary>
        /// Remove a condition by the provided Condition.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="removeAll"></param>
        public virtual void RemoveCondition(Condition condition) => RemoveCondition(condition.Uid());
        
        /// <summary>
        /// Remove all of a condition by the provided Condition.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="removeAll"></param>
        public virtual void RemoveAllOfCondition(Condition condition) => RemoveAllOfCondition(condition.Uid());
        
        /// <summary>
        /// Remove all of a condition by the provided uid.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="removeAll"></param>
        public virtual void RemoveAllOfCondition(string uid, bool removeAll = true) => conditions.RemoveAll(uid);

        /// <summary>
        /// Removes a specific GameCondition based on the unique runtime gameId
        /// </summary>
        /// <param name="gameId"></param>
        public virtual void RemoveConditionByGameId(string gameId) => conditions.RemoveExact(gameId);

        /// <summary>
        /// Returns true if the GameConditionList contains a GameCondition with the provided uid
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="gameCondition"></param>
        /// <returns></returns>
        public virtual bool TryGetGameCondition(string uid, out GameCondition gameCondition)
        {
            gameCondition = GetCondition(uid);
            return gameCondition != null;
        }

        /// <summary>
        /// Returns true if the GameConditionList contains a GameCondition with the provided Condition object
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="gameCondition"></param>
        /// <returns></returns>
        public virtual bool TryGetGameCondition(Condition condition, out GameCondition gameCondition) 
            => TryGetGameCondition(condition.Uid(), out gameCondition);

        /// <summary>
        /// Sets a condition dirty, along with all stats the condition affects
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="dirty"></param>
        public virtual void SetConditionDirty(string uid, bool dirty = true) => conditions.SetDirty(uid, dirty);
        
        /// <summary>
        /// Sets a condition dirty, along with all stats the condition affects
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="dirty"></param>
        public virtual void SetConditionDirty(Condition condition, bool dirty = true) =>
            SetConditionDirty(condition.Uid(), dirty);

        // ------------------------------------------------------------------------------------------
        // IHaveQuests
        // ------------------------------------------------------------------------------------------

        // No required methods

        // ---- ADDITIONAL HELPER METHODS ----
        /// <summary>
        /// Adds a vanilla Quest to the GameQuestList
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public virtual GameQuest AddQuest(string uid)
        {
            var newQuest = quests.Add(uid);
            // Register with the Modules Helper, so Tick() will be called on GameQuestList!
            //RegisterQuestActorWithModulesHelper();
            return newQuest;
        }

        /// <summary>
        /// Adds a vanilla Quest to the GameQuestList
        /// </summary>
        /// <param name="quest"></param>
        /// <returns></returns>
        public virtual GameQuest AddQuest(Quest quest) => AddQuest(quest.Uid());
        
        /// <summary>
        /// Get a GameQuest by the provided uid.
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public virtual GameQuest GetQuest(string uid) => quests.Get(uid);
        
        /// <summary>
        /// Get a GameQuest by the provided Quest object.
        /// </summary>
        /// <param name="quest"></param>
        /// <returns></returns>
        public virtual GameQuest GetQuest(Quest quest) => GetQuest(quest.Uid());

        /// <summary>
        /// Returns an array of all the quests with the "In Progress" status
        /// </summary>
        /// <returns></returns>
        public virtual GameQuest[] GetQuestsInProgress() =>
            quests.list.Where(x => x.status == QuestStep.QuestStepStatus.InProgress).ToArray();
        
        /// <summary>
        /// Returns an array of all the quests with the "Succeeded" status
        /// </summary>
        /// <returns></returns>
        public virtual GameQuest[] GetQuestsSucceeded() =>
            quests.list.Where(x => x.status == QuestStep.QuestStepStatus.Succeeded).ToArray();
        
        /// <summary>
        /// Returns an array of all the quests with the "Failed" status
        /// </summary>
        /// <returns></returns>
        public virtual GameQuest[] GetQuestsFailed() =>
            quests.list.Where(x => x.status == QuestStep.QuestStepStatus.Failed).ToArray();
        
        /// <summary>
        /// Returns an array of all the quests with the "Complete" status, which includes all that are not InProgress.
        /// </summary>
        /// <returns></returns>
        public virtual GameQuest[] GetQuestsComplete() =>
            quests.list.Where(x => x.status != QuestStep.QuestStepStatus.InProgress).ToArray();

        /// <summary>
        /// Set a quest to the "Succeeded" status
        /// </summary>
        /// <param name="uid"></param>
        public virtual void SetQuestSucceeded(string uid)
        {
            var gameQuest = GetQuest(uid);
            gameQuest.SetStepsSucceeded(true);
            gameQuest.CheckForSuccess();
        }
        
        /// <summary>
        /// Set a quest to the "Failed" status
        /// </summary>
        /// <param name="uid"></param>
        public virtual void SetQuestFailed(string uid)
        {
            var gameQuest = GetQuest(uid);
            gameQuest.SetStepsFailed(true);
            gameQuest.CheckForFailure();
        }

        /// <summary>
        /// Returns the number of quests in the InProgress status
        /// </summary>
        public virtual int CountQuestInProgress => quests.list.Count(x => x.status == QuestStep.QuestStepStatus.InProgress);
        
        /// <summary>
        /// Returns the number of quests in the Succeeded status
        /// </summary>
        public virtual int CountQuestSucceeded => quests.list.Count(x => x.status == QuestStep.QuestStepStatus.Succeeded);
        
        /// <summary>
        /// Returns the number of quests in the Failed status
        /// </summary>
        public virtual int CountQuestFailed => quests.list.Count(x => x.status == QuestStep.QuestStepStatus.Failed);
        
        /// <summary>
        /// Returns the number of quests in the Complete status, which includes all that are not InProgress.
        /// </summary>
        public virtual int CountQuestComplete => quests.list.Count(x => x.status != QuestStep.QuestStepStatus.InProgress);
        
        // ------------------------------------------------------------------------------------------
        // Inventory and Equipment GameItemObjectLists
        // ------------------------------------------------------------------------------------------
        
        // Note: This returns true. In many games, not all actors can equip all items. You can override this in the
        // actor script which inherits this to customize the functionality.
        public virtual bool CanEquipItem(GameItemObject item) => true;
        
        /// <summary>
        /// Will add a clone of a GameItemObject to the "inventory" list.
        /// </summary>
        /// <param name="item"></param>
        public virtual void AddItemToInventoryChangeThis(GameItemObject item) => inventoryItems.ReceiveTransfer(item);
        
        /// <summary>
        /// Move a GameItemObject from the inventory list to the equipment list
        /// </summary>
        /// <param name="item"></param>
        public virtual void EquipItem(GameItemObject item) =>  inventoryItems.TransferTo(equippedItems, item);
        
        /// <summary>
        /// Move a GameItemObject from the equipment list to the inventory list
        /// </summary>
        /// <param name="item"></param>
        public virtual void UnEquipItem(GameItemObject item) =>  equippedItems.TransferTo(inventoryItems, item);
        
        /// <summary>
        /// Remove a GameItemObject from the inventory list
        /// </summary>
        /// <param name="item"></param>
        public virtual void RemoveFromInventory(GameItemObject item) => inventoryItems.RemoveGameId(item.GameId());
        
        /// <summary>
        /// Remove a GameItemObject from the equipment list
        /// </summary>
        /// <param name="item"></param>
        public virtual void RemoveFromEquipment(GameItemObject item) => equippedItems.RemoveGameId(item.GameId());
        
        /// <summary>
        /// Add a vanilla GameItemObject to the inventory list based on the uid provided.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="distinct"></param>
        public virtual void AddItemToInventory(string uid, bool distinct = true) => inventoryItems.Add(uid, distinct);
        
        /// <summary>
        /// Add a vanilla GameItemObject to the equipment list based on the ItemObject provided.
        /// </summary>
        /// <param name="itemObject"></param>
        /// <param name="distinct"></param>
        public virtual void AddItemToInventory(ItemObject itemObject, bool distinct = true) 
            => AddItemToInventory(itemObject.Uid(), distinct);
        
        /// <summary>
        /// Add a vanilla GameItemObject to the equipment list based on the GameItemObject provided.
        /// </summary>
        /// <param name="gameItemObject"></param>
        /// <param name="distinct"></param>
        public virtual void AddItemToInventory(GameItemObject gameItemObject, bool distinct = true) 
            => AddItemToInventory(gameItemObject.Uid(), distinct);
        
        /// <summary>
        /// Add a vanilla GameItemObject to the equipment list based on the uid provided.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="distinct"></param>
        public virtual void AddItemToEquipment(string uid, bool distinct = true) => equippedItems.Add(uid, distinct);
        
        /// <summary>
        /// Add a vanilla GameItemObject to the equipment list based on the ItemObject provided.
        /// </summary>
        /// <param name="itemObject"></param>
        /// <param name="distinct"></param>
        public virtual void AddItemToEquipment(ItemObject itemObject, bool distinct = true) 
            => AddItemToEquipment(itemObject.Uid(), distinct);
        
        /// <summary>
        /// Add a vanilla GameItemObject to the equipment list based on the GameItemObject provided.
        /// </summary>
        /// <param name="gameItemObject"></param>
        /// <param name="distinct"></param>
        public virtual void AddItemToEquipment(GameItemObject gameItemObject, bool distinct = true) 
            => AddItemToEquipment(gameItemObject.Uid(), distinct);
        
        /// <summary>
        /// Transfers a complete GameItemObject into the inventory. If you are trying to transfer from the equipment
        /// list to the inventory list, use UnequipItem() instead.
        /// </summary>
        /// <param name="gameItemObject"></param>
        /// <param name="distinct"></param>
        /// <param name="makeClone"></param>
        /// <param name="ignoreQuestItem"></param>
        /// <returns></returns>
        public virtual GameItemObject TransferItemToInventory(GameItemObject gameItemObject, bool distinct = false
            , bool makeClone = true, bool ignoreQuestItem = false)
            => inventoryItems.ReceiveTransfer(gameItemObject, distinct, makeClone, ignoreQuestItem);
        
        /// <summary>
        /// Transfers a complete GameItemObject into the equipment. If you are trying to transfer from the inventory
        /// list to the equipment list, use EquipItem() instead.
        /// </summary>
        /// <param name="gameItemObject"></param>
        /// <param name="distinct"></param>
        /// <param name="makeClone"></param>
        /// <param name="ignoreQuestItem"></param>
        /// <returns></returns>
        public virtual GameItemObject TransferItemToEquipment(GameItemObject gameItemObject, bool distinct = false
            , bool makeClone = true, bool ignoreQuestItem = false)
            => equippedItems.ReceiveTransfer(gameItemObject, distinct, makeClone, ignoreQuestItem);

        /// <summary>
        /// Will attempt to get the first GameItemObject of a specific objectType from the inventory list.
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="gameItemObject"></param>
        /// <returns></returns>
        public virtual bool TryGetInventoryType(string objectType, out GameItemObject gameItemObject)
        {
            gameItemObject = inventoryItems.list.FirstOrDefault(x => x.objectType == objectType);
            return gameItemObject != null;
        }
        
        /// <summary>
        /// Will attempt to get the first GameItemObject of a specific objectType from the equipment list.
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="gameItemObject"></param>
        /// <returns></returns>
        public virtual bool TryGetEquippedType(string objectType, out GameItemObject gameItemObject)
        {
            gameItemObject = equippedItems.list.FirstOrDefault(x => x.objectType == objectType);
            return gameItemObject != null;
        }
        
        /*
         June 8, 2023 -- This section is not complete, as I am not sure if it is really needed. As seen in GameData.cs
         (in the Party Based RPG demo game), saving works fine just by saving a list of Actors which inherit from
         GameModulesActor. The only potential benefits would be if the serialization depth limit is reached, in which
         case saving each GameModuleActor as a string, and each GameModuleList in those as strings, would be helpful.
         
         Otherwise, it just complicates things. I'm going to leave this code here though, in case I change my mind or
         another use case comes up.
        // ------------------------------------------------------------------------------------------
        // Save and Load
        // ------------------------------------------------------------------------------------------

        /// <summary>
        /// Compiles the Game Module Lists into json encoded strings, and returns a single json encoded string,
        /// which you can save as a string in your main data class. This is not required, but may help in situations
        /// where the serialization depth limit is reached.
        /// </summary>
        /// <returns></returns>
        public virtual string SaveData()
        {
            var savedData = new GameModulesActorSavedData()
            {
                stats = stats.SaveData(),
                conditions = conditions.SaveData(),
                quests = quests.SaveData(),
                dictionaries = dictionaries.SaveData(),
                inventory = inventoryItems.SaveData(),
                equipment = equippedItems.SaveData(),
            };

            return JsonUtility.ToJson(savedData);
        }

        /// <summary>
        /// Decompile the json encoded string, and load the data into the Game Module Lists, each of which will
        /// receive its own json encoded string to decompile.
        /// </summary>
        /// <param name="savedData"></param>
        public virtual void LoadData(string savedData)
        {
            Debug.Log("Loading GameModulesActor data...");
            var loadedData = JsonUtility.FromJson<GameModulesActorSavedData>(savedData);
            stats.LoadData(loadedData.stats, this);
            // Conditions
            // Quests
            // Dictionaries
            dictionaries.LoadData(loadedData.dictionaries);
        }
        */
    }
}
