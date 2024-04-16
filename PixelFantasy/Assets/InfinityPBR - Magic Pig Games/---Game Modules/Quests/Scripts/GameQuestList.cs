using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static InfinityPBR.Modules.QuestEventBoard;

namespace InfinityPBR.Modules
{
    [HelpURL("https://infinitypbr.gitbook.io/infinity-pbr/game-modules/quests")]
    [Serializable]
    public class GameQuestList : GameModulesList<GameQuest>, IHaveStartActions
    {
        // **********************************************************************
        // General things for all Game Module Lists
        // **********************************************************************
        
        public override string GameId() => Owner.GameId();
        public override string NoteSubject() => "Quests";
        public GameQuestList Clone() => JsonUtility.FromJson<GameQuestList>(JsonUtility.ToJson(this));
        
        // **********************************************************************
        // Owner 
        // **********************************************************************
        
        public IHaveQuests Owner { get; private set; }
        public virtual void SetOwner(IHaveQuests value)
        {
            Owner = value;
            foreach (var quest in list)
                quest.SetOwner(value);
        }
        public IHaveStats StatsOwner => (IHaveStats)Owner; // Quests also has this StatsOwner property
        
        
        public virtual void RegisterQuestActorWithModulesHelper()
        {
            if (list.Count > 0)
                ModulesHelper.Instance.RegisterQuestList(this);
        }

        public virtual void UnregisterQuestActorWithModulesHelper()
        {
            if (list.Count == 0)
                ModulesHelper.Instance.UnregisterQuestList(this); 
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

            // Each quest must subscribe to the blackboard, and perhaps do other things in their own
            // StartActions() method
            foreach (var quest in list)
                quest.StartActions();

            this.AddThisToBlackboard(false);
            RegisterQuestActorWithModulesHelper();
        }
        
        // **********************************************************************
        // Affected Stats & Dirty
        // **********************************************************************

        public void SetAffectedStatsDirty(List<Stat> statList = null) 
            => StatsOwner?.SetStatsDirty(statList ?? DirectlyAffectsList());
        
        public override void SetAffectedStatsDirty(IAmGameModuleObject gameModuleObject)
        {
            if (gameModuleObject is GameQuest gameQuest)
                gameQuest.SetDirty();
        }
        
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
            list.ForEach(x => modificationLevels.AddRange(x.ModificationLevels));
            return modificationLevels;
        }

        // **********************************************************************
        // Standard methods for specific Scriptable Object types
        // **********************************************************************

        public virtual GameQuest Get(Quest quest, bool addIfNull = false, bool distinct = false)
            => this.Get(quest.Uid(), addIfNull, distinct);
        
        public virtual GameQuest GetInProgress(string uid) 
            => list.FirstOrDefault(x => x.Uid() == uid && x.status == QuestStep.QuestStepStatus.InProgress);
        public virtual GameQuest GetSucceeded(string uid) 
            => list.FirstOrDefault(x => x.Uid() == uid && x.status == QuestStep.QuestStepStatus.Succeeded);
        public virtual GameQuest GetFailed(string uid) 
            => list.FirstOrDefault(x => x.Uid() == uid && x.status == QuestStep.QuestStepStatus.Failed);
        public virtual GameQuest GetCompleted(string uid) 
            => list.FirstOrDefault(x => x.Uid() == uid && x.status != QuestStep.QuestStepStatus.InProgress);

        public virtual bool TryGetInProgress(string uid, out GameQuest found)
        {
            found = GetInProgress(uid);
            return found != null;
        }

        public virtual bool TryGetSucceeded(string uid, out GameQuest found)
        {
            found = GetInProgress(uid);
            return found != null;
        }
        
        public virtual bool TryGetFailed(string uid, out GameQuest found)
        {
            found = GetFailed(uid);
            return found != null;
        }
        
        public virtual bool TryGetCompleted(string uid, out GameQuest found)
        {
            found = GetCompleted(uid);
            return found != null;
        }

        public virtual IEnumerable<GameQuest> QuestsModifyingStats => list.Where(x => x.ModifiesStats);
        
        public virtual IEnumerable<GameQuest> QuestsInProgress => list.Where(x => x.status == QuestStep.QuestStepStatus.InProgress);
        public virtual IEnumerable<GameQuest> QuestsSucceeded => list.Where(x => x.status == QuestStep.QuestStepStatus.Succeeded);
        public virtual IEnumerable<GameQuest> QuestsFailed => list.Where(x => x.status == QuestStep.QuestStepStatus.Failed);
        public virtual IEnumerable<GameQuest> QuestsCompleted => list.Where(x => x.status != QuestStep.QuestStepStatus.InProgress);

        public virtual bool Contains(Quest quest) => this.Contains(quest.Uid());
        
        public virtual bool ContainsQuestsInProgress(Quest quest) => ContainsQuestsInProgress(quest.Uid());
        public virtual bool ContainsQuestsInProgress(GameQuest gameQuest) => ContainsQuestsInProgress(gameQuest.Uid());
        public virtual bool ContainsQuestsInProgress(string uid = null) => uid == null
            ? QuestsInProgress.Any()
            : QuestsInProgress.Any(x => x.Uid() == uid);
        
        public virtual bool ContainsQuestsSucceeded(Quest quest) => ContainsQuestsSucceeded(quest.Uid());
        public virtual bool ContainsQuestsSucceeded(GameQuest gameQuest) => ContainsQuestsSucceeded(gameQuest.Uid());
        public virtual bool ContainsQuestsSucceeded(string uid = null) => uid == null
            ? QuestsSucceeded.Any()
            : QuestsSucceeded.Any(x => x.Uid() == uid);
        
        public virtual bool ContainsQuestsFailed(Quest quest) => ContainsQuestsFailed(quest.Uid());
        public virtual bool ContainsQuestsFailed(GameQuest gameQuest) => ContainsQuestsFailed(gameQuest.Uid());
        public virtual bool ContainsQuestsFailed(string uid = null) => uid == null
            ? QuestsFailed.Any()
            : QuestsFailed.Any(x => x.Uid() == uid);
        
        public virtual bool ContainsQuestsCompleted(Quest quest) => ContainsQuestsCompleted(quest.Uid());
        public virtual bool ContainsQuestsCompleted(GameQuest gameQuest) => ContainsQuestsCompleted(gameQuest.Uid());
        public virtual bool ContainsQuestsCompleted(string uid = null) => uid == null
            ? QuestsCompleted.Any()
            : QuestsCompleted.Any(x => x.Uid() == uid);

        public virtual bool ContainsQuestsWhichModifyStats() => CountModifyingStats() > 0;
        
        public virtual int CountQuestsInProgress(Quest quest) => CountQuestsInProgress(quest.Uid());
        public virtual int CountQuestsInProgress(GameQuest gameQuest) => CountQuestsInProgress(gameQuest.Uid());
        public virtual int CountQuestsInProgress(string uid = null) => uid == null
            ? QuestsInProgress.Count()
            : QuestsInProgress.Count(x => x.Uid() == uid);
        
        public virtual int CountQuestsSucceeded(Quest quest) => CountQuestsSucceeded(quest.Uid());
        public virtual int CountQuestsSucceeded(GameQuest gameQuest) => CountQuestsSucceeded(gameQuest.Uid());
        public virtual int CountQuestsSucceeded(string uid = null) => uid == null
            ? QuestsSucceeded.Count()
            : QuestsSucceeded.Count(x => x.Uid() == uid);
        
        public virtual int CountQuestsFailed(Quest quest) => CountQuestsFailed(quest.Uid());
        public virtual int CountQuestsFailed(GameQuest gameQuest) => CountQuestsFailed(gameQuest.Uid());
        public virtual int CountQuestsFailed(string uid = null) => uid == null
            ? QuestsFailed.Count()
            : QuestsFailed.Count(x => x.Uid() == uid);
        
        public virtual int CountQuestsCompleted(Quest quest) => CountQuestsCompleted(quest.Uid());
        public virtual int CountQuestsCompleted(GameQuest gameQuest) => CountQuestsCompleted(gameQuest.Uid());
        public virtual int CountQuestsCompleted(string uid = null) => uid == null
            ? QuestsCompleted.Count()
            : QuestsCompleted.Count(x => x.Uid() == uid);
        
        public virtual int CountModifyingStats(Quest quest) => CountModifyingStats(quest.Uid());
        public virtual int CountModifyingStats(GameQuest gameQuest) => CountModifyingStats(gameQuest.Uid());
        public virtual int CountModifyingStats(string uid = null) => uid == null
            ? QuestsModifyingStats.Count()
            : QuestsModifyingStats.Count(x => x.Uid() == uid);

        public virtual void Remove(Quest quest) => this.Remove(quest.Uid());
        public virtual void RemoveAll(Quest quest) => this.RemoveAll(quest.Uid());

        public virtual GameQuest Add(Quest quest, bool distinct = true) =>
            Add(quest.Uid(), distinct);
        
        public virtual GameQuest Add(GameQuest gameQuest, bool distinct = true) =>
            Add(gameQuest.Parent().Uid(), distinct);
        
        public override GameQuest Add(string uid, bool distinct = true)
        {
            if (distinct && this.Contains(uid)) return this.Get(uid);
            list.Add(new GameQuest(GameModuleRepository.Instance.Get<Quest>(uid), this, Now));

            SetAffectedStatsDirty(this.Last());

            RegisterQuestActorWithModulesHelper();
            
            SendEvent(this.Last(), QuestStep.QuestStepStatus.Started);
            
            this.AddThisToBlackboard();
            return this.Last();
        }
        
        // **********************************************************************
        // Transfering
        // **********************************************************************
        
        public virtual GameQuest ReceiveTransfer(GameQuest gameQuest, bool distinct = true, bool makeClone = true)
        {
            if (gameQuest == null) return default;
            if (distinct && this.Contains(gameQuest.Uid())) return null;
            
            var transferObject = makeClone ? gameQuest.Clone() : gameQuest;
            transferObject.gameQuestList = this;
            transferObject.SetOwner(Owner);
            transferObject.SetParentList(this);
            
            list.Add(transferObject);
            
            SetAffectedStatsDirty(this.Last());

            RegisterQuestActorWithModulesHelper();
            
            this.AddThisToBlackboard();
            return this.Last();
        }

        public virtual GameQuest TransferTo(GameQuestList transferTo, GameQuest transferObject, bool distinct = false)
        {
            var objectAtDestination = transferTo.ReceiveTransfer(transferObject, distinct);
            
            // If distinct = true, transfer may fail if an object like this is already present.
            if (objectAtDestination == null)
                return null;

            SetAffectedStatsDirty(transferObject.DirectlyAffects());
            this.RemoveGameId(transferObject.GameId());
            
            this.AddThisToBlackboard();
            return objectAtDestination;
        }
        
        // **********************************************************************
        // Additional Methods, Variables, and Properties for this type
        // **********************************************************************
        
        public virtual bool ModifiesStats => list.Any(x => x.ModifiesStats);

        // **********************************************************************
        // Custom Methods for this List type
        // **********************************************************************

        public virtual void SendEvent(GameQuest gameQuest, QuestStep.QuestStepStatus status) => questEventBoard.AddEvent(gameQuest, status);
        
        // **********************************************************************
        // Override Methods
        // **********************************************************************
        
        public virtual void Tick() => TickActions();

        public virtual void TickActions()
        {
            foreach (var item in list)
                item.TickActions();
        }

        
        
        
        // June 4, 2023 -- These are not used right now. Perhaps in the future. Not sure if we will have individual objects of this type post to the blackboard
        // Handles force posting for individual objects in the list
        public bool forcePostToBlackboard; // If true, objects will post to blackboard even if they are individual set to not post
        public bool forceNotifyOnPost; // If true, when objects post, they will be forced to notify followers of the blackboard
    }
}