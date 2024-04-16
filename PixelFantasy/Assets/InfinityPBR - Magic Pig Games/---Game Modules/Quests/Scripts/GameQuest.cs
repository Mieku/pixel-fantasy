using System;
using System.Collections.Generic;
using System.Linq;
using InfinityPBR.Modules;
using UnityEngine;
using static InfinityPBR.Modules.MainBlackboard;
using static InfinityPBR.Modules.QuestEventBoard;
using static InfinityPBR.Modules.Timeboard;

/*
 * GAME QUEST
 *
 * This is the in-game, runtime object to use for Quests. It will automatically cache the scriptable object from
 * the Quests repository.
 */

namespace InfinityPBR
{
    [HelpURL("https://infinitypbr.gitbook.io/infinity-pbr/game-modules/quests")]
    [Serializable]
    public class GameQuest : IAmGameModuleObject, IFollowBlackboard
    {
        public GameQuestList ParentList { get; private set; }
        public void SetParentList(GameQuestList value) => ParentList = value;
        public IHaveQuests Owner => ParentList == null ? _owner : ParentList.Owner;
        public void SetOwner(IHaveQuests value) => _owner = value;
        private IHaveQuests _owner;

        public IHaveStats StatsOwner => (IHaveStats)Owner;
        
        // ************************************************************************************************
        // Connection to the parent object
        // ************************************************************************************************
        
        private Quest _parent;
        public Quest Parent() 
            => _parent 
                ? _parent 
                : _parent = GameModuleRepository.Instance.Get<Quest>(Uid());
        
        [SerializeField] private string _uid;
        public void SetUid(string value) => _uid = value;
        public string Uid() => _uid;
        
        public string ObjectName() => objectName;
        public string ObjectType() => objectType;
        public string objectName; // The name of this
        public string objectType; // The type (parent directory from hierarchy name)
        public Dictionaries dictionaries = new Dictionaries("Unnamed");
        
        // **********************************************************************
        // GameId
        // **********************************************************************
        
        [SerializeField] private string _gameId;

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
        
        public QuestStep.QuestStepStatus status = QuestStep.QuestStepStatus.InProgress;
        
        public List<QuestStep> questSteps = new List<QuestStep>();
        public List<CustomQuestReward> customSuccessRewards = new List<CustomQuestReward>(); 
        public List<CustomQuestReward> customFailureRewards = new List<CustomQuestReward>();

        public List<QuestStep> QuestStepsInProgress =>
            questSteps.Where(x => x.status == QuestStep.QuestStepStatus.InProgress).ToList();
        public List<QuestStep> QuestStepsSucceeded =>
            questSteps.Where(x => x.status == QuestStep.QuestStepStatus.Succeeded).ToList();
        public List<QuestStep> QuestStepsFailed =>
            questSteps.Where(x => x.status == QuestStep.QuestStepStatus.Failed).ToList();
        
        public bool ModifiesStats => GetModificationLevel().AffectsList().Any();
        
        
        
        public ModificationLevel ModificationLevel => GetModificationLevel(); // returns the current Modification Level from the ItemObject
        public List<ModificationLevel> ModificationLevels => GetModificationLevels(); // returns the current Modification Level from the Quest
        
        private ModificationLevel _modificationLevel;
        public GameQuestList gameQuestList;


        public float startTime = -1; // Optional. The start time for when this quest began.
        public float endTime = -1; // Optional. The time which this quest expires
        
        // Properties && Settings
        public int activeStep;
        public bool SequentialSteps => Parent().sequentialSteps;
        public bool Hidden => Parent().hidden;
        public bool failed; // Set true if the quest has failed. Not all quests can fail.
        public int Steps => questSteps.Count; // Number of steps in this quest
        public int SucceededSteps => questSteps.Count(step => step.status == QuestStep.QuestStepStatus.Succeeded); // Number of completed steps
        public int FailedSteps => questSteps.Count(step => step.status == QuestStep.QuestStepStatus.Failed); // Number of failed steps
        public int StepsLeft => Steps - SucceededSteps; // Number of steps left to complete
        public bool IsComplete => StepsLeft == 0; // True if all steps are completed
        public float Now() => timeboard.gametime.Now();
        
        // Returns all QuestPointReward objects from all QuestReward objects in a single list
        //public List<QuestPointReward> QuestPointRewards => questRewards.SelectMany(reward => reward.pointRewards).ToList();
        
        // Returns all LootBox objects from all QuestReward objects in a single list
        //public List<LootBox> LootBoxRewards => questRewards.SelectMany(reward => reward.lootBoxes).ToList();
        
        // ************************************************************************************************
        // Constructor
        // ************************************************************************************************
        
        public GameQuest(Quest parent, GameQuestList parentList = null, float startTimeValue = -1)
        {
            _parent = parent; // Set the Item Object

            if (!parent) return; // Return if we do not have an ItemObject attached yet
            
            dictionaries = parent.dictionaries.Clone(); // Clone the dictionaries object, so we don't overwrite our Scriptable Object data!
            SetUid(parent.Uid()); // v3.6 -- set the uid with this method now, replacing "_uid = ..."
            objectName = parent.objectName; // Set the name
            objectType = parent.objectType; // Set the type

            SetParentList(parentList);

            startTime = startTimeValue;
            if (_parent.hasEndTime)
                endTime = startTimeValue + _parent.timeLimit;

            parent.questSteps.ForEach(step => questSteps.Add(step.Clone()));
            questSteps.ForEach(x => x.ParentGameQuest = this);
            //parent.successRewards.ForEach(reward => successRewards.Add(reward.Clone()));
            //parent.failureRewards.ForEach(reward => failureRewards.Add(reward.Clone()));
            
            // Create the GameUid
            GameId();
            
            SaveConditionValues(); // Save any values that we need to save
            QuestStepTicks(); // Do this in case conditions are already met
            
            SendQuestEvent(status);
            
            // Actions below require subscribing to the blackboard
            if (!parent.subscribeToBlackboard) return;
            
            SubscribeToBlackboard(); // Make sure we subscribe
        }

        // ************************************************************************************************
        // Override Methods
        // ************************************************************************************************

        public void StartActions()
        {
            SubscribeToBlackboard(); // Make sure we subscribe
        }

        private void RelinkData()
        {
            if (didRelink) return;

            // Foreach quest step, find the parent quest step and relink the success and failure conditions
            foreach (var step in questSteps)
            {
                step.quest = Parent();
                var parentStep = Parent().questSteps.FirstOrDefault(x => x.name == step.name);
                if (parentStep == null)
                {
                    Debug.LogError($"Could not find the parent quest step for {objectName}");
                    continue;
                }
                step.successConditions = parentStep.successConditions;
                step.failureConditions = parentStep.failureConditions;
            }
            
            /*
            foreach (var reward in customSuccessRewards)
            {
                Debug.Log("Handle relinks for custom rewards");
            }
            */

            didRelink = true;
        }

        // public void Tick() => TickActions();
        //public void LateTick() => TickActions();

        private bool didRelink;
        
        public void TickActions()
        {
            RelinkData();
            // We only do things if the Quest is InProgress, otherwise, this job is done!
            if (status != QuestStep.QuestStepStatus.InProgress)
                return;
            
            // Check for time expiration. If we have reached it, we will skip doing the QuestStepTicks()
            if (!CheckExpiration())
            {
                // Only do this if we are checking every frame.
                if (Parent().queryEveryFrame)
                    QuestStepTicks(); // This is where status may change based on QuestStep changes
            }

            // Here we check for success first, and then failure.
            if (Parent().autoSucceed) CheckForSuccess();
            if (Parent().autoFail) CheckForFailure();
        }

        public void SetDirty() => SetAffectedStatsDirty();
        
        // ************************************************************************************************
        // Custom Methods for this Type
        // ************************************************************************************************

        public void AddCustomRewardSuccess(QuestReward questReward) 
            => customSuccessRewards.Add(new CustomQuestReward(questReward));

        public void AddCustomRewardFailure(QuestReward questReward)
            => customFailureRewards.Add(new CustomQuestReward(questReward));

        public void RemoveCustomRewardSuccess(QuestReward questReward)
            => customSuccessRewards.Remove(customSuccessRewards.First(x 
                => x.uid == questReward.Uid()));

        public void RemoveCustomRewardFailure(QuestReward questReward)
            => customFailureRewards.Remove(customFailureRewards.First(x 
                => x.uid == questReward.Uid()));
        
        // Not used in this context.
        public List<Stat> DirectlyAffectedBy(Stat stat = null) => default;
        
        public List<Stat> DirectlyAffects(Stat stat = null) =>
            ModificationLevels.SelectMany(x => x.targets).Distinct().ToList();

        // Not used in this context. Use ModificationLevel instead, as each GameQuest only has one.
        public List<Stat> DirectlyAffectedByList(Stat stat = null)
        {
            throw new NotImplementedException();
        }

        public List<Stat> DirectlyAffectsList() => ModificationLevel.targets;

        public List<ModificationLevel> GetModificationLevels() => new() { ModificationLevel };

        public ModificationLevel GetModificationLevel()
        {
            return status switch
            {
                QuestStep.QuestStepStatus.InProgress => Parent().modificationLevelInProgress,
                QuestStep.QuestStepStatus.Failed => Parent().modificationLevelFailed,
                QuestStep.QuestStepStatus.Succeeded => Parent().modificationLevelSucceeded,
                _ => new ModificationLevel()
            };
        }



        private void SetAffectedStatsDirty(List<Stat> statList = null) => StatsOwner?.SetStatsDirty(statList ?? DirectlyAffectsList());

        private bool CheckExpiration()
        {
            if (!Parent().hasEndTime) return false; // This has no end time
            if (endTime > Now()) return false; // We have not yet reached the end time
            
            // We've reached the end time. Options are to simply expire, or to auto succeed or fail.
            if (Parent().succeedOnExpiration)
            {
                SetStepsSucceeded(true);
                return true;
            }

            if (Parent().failOnExpiration)
            {
                SetStepsFailed(true);
                return true;
            }
            
            // We will remove on expiration, without succeeding or failing...simply removing.
            RemoveQuestOperations();
            gameQuestList.RemoveGameId(GameId());
            return true;
        }

        public void SetStepsSucceeded(bool forceCanNotRevert = false)
        {
            foreach (var step in questSteps)
                 step.SetSucceeded(forceCanNotRevert);
        }
        
        public void SetStepsFailed(bool forceCanNotRevert = false)
        {
            foreach (var step in questSteps)
                step.SetFailed(forceCanNotRevert);
        }

        public void QuestStepTicks()
        {
            if (status != QuestStep.QuestStepStatus.InProgress) return;
            
            if (!Parent().sequentialSteps)
            {
                questSteps.ForEach(step => DoQuestStepTick(step));
                return;
            }

            foreach (var step in questSteps)
            {
                // If the step has succeeded or failed, move on to the next. If it 
                // canRevert, then we need to check regardless.
                if (step.status != QuestStep.QuestStepStatus.InProgress
                    && !step.canRevert)
                    continue;

                // Check the next step, and break, so that only this step is checked
                DoQuestStepTick(step);
                break;
            }
        }

        /*
         * This is the method which has a QuestStep do its Tick() and keeps track of the status of the step.
         * If the step changes, a questStep event is sent out.
         */
        private void DoQuestStepTick(QuestStep questStep)
        {
            var tempStatus = questStep.status;
            questStep.Tick(Owner, Now());
            
            // Do not continue if the QuestStep doesn't send events
            if (!questStep.sendQuestEvents)
                return;
            
            // Do not continue if the status did not change
            if (tempStatus == questStep.status)
                return;
            
            // Send the event!
            SendQuestStepEvent(questStep.status, questStep);
        }

        public bool CheckForFailure()
        {
            // If nothing has failed, then we haven't failed.
            if (FailedSteps == 0) return false;

            return CompleteQuestFail();
        }

        public bool CheckForSuccess()
        {
            // If there are still steps left to complete then we have not completed this quest.
            // NOTE: This is auto-completion, so once it is complete, it will no longer be checked. This means
            // that if the QuestSteps "canRevert", that option only applies prior to the entire Quest being
            // completed. Once it is completed, QuestSteps will not be checked, so they will not be able to revert.
            if (StepsLeft != 0)
                return false;

            return CompleteQuestSuccess();
        }

        // Call this to instantly fail the quest!
        public bool CompleteQuestFail()
        {
            if (status == QuestStep.QuestStepStatus.Failed)
                return true;
            
            Debug.Log("Quest Failed! (Send the QuestEvent & do other completion actions)");
            SetStatus(QuestStep.QuestStepStatus.Failed); // Set the status
            RemoveQuestOperations();
            GiveRewards(Parent().failureRewards); // Rewards set on the Scriptable Object Quest
            GiveCustomRewards(customFailureRewards); // Rewards added to this GameQuest
            SetDirty();
            
            return true;
        }

        // Call this to instantly succeed the quest!
        public bool CompleteQuestSuccess()
        {
            if (status == QuestStep.QuestStepStatus.Succeeded)
                return true;

            Debug.Log("Quest Succeeded! (Send the QuestEvent & do other completion actions)");
            SetStatus(QuestStep.QuestStepStatus.Succeeded); // Set the status
            RemoveQuestOperations();
            GiveRewards(Parent().successRewards); // Rewards set on the Scriptable Object Quest
            GiveCustomRewards(customSuccessRewards); // Rewards added to this GameQuest
            SetDirty();
            
            return true;
        }

        // Give each reward in the provided list to the Owner
        private void GiveRewards(List<QuestReward> questRewards) 
            => questRewards.ForEach(x => x.GiveReward(Owner));

        private void GiveCustomRewards(List<CustomQuestReward> customQuestRewards)
        {
            foreach (var reward in customQuestRewards)
                reward.Reward.GiveReward(Owner);
        }

        // Called from the QuestStep when it is completed, will pass in either the success or failure rewards.
        public void GiveQuestStepRewards(List<QuestReward> questRewards, List<CustomQuestReward> customQuestRewards)
        {
            questRewards.ForEach(x => x.GiveReward(Owner));
            foreach (var reward in customQuestRewards)
                reward.Reward.GiveReward(Owner);
        }

        private void RemoveQuestOperations()
        {
            // Unsubscribe from the blackboard
            if (Parent().subscribeToBlackboard)
                UnsubscribeFromBlackboard();

            // Run the agnostic actions for steps. These are more like bookkeeping, rather than 
            // rewards/punishments.
            foreach (var step in questSteps)
                step.CompleteQuest();
        }

        /// <summary>
        /// Set the status of this quest. Optionally send an event if the status has changed.
        /// </summary>
        /// <param name="newStatus"></param>
        /// <param name="sendEventIfChanged"></param>
        public void SetStatus(QuestStep.QuestStepStatus newStatus, bool sendEventIfChanged = true)
        {
            if (newStatus == status) return;
            
            // Set affected stats for the current Status
            gameQuestList?.SetAffectedStatsDirty(this);
            
            // Set the new status, and set affected stats for that
            status = newStatus; // Set the new status
            gameQuestList?.SetAffectedStatsDirty(this);
            
            // If we aren't sending events we are done; return
            if (!sendEventIfChanged)
                return;
            
            SendQuestEvent(status); // Send the event notifying of the new status
        }

        /*
         * This is where quest events are sent. Any object that is subscribed will receive the QuestEvent, and should
         * determine for itself whether the event is one that it cares about. The status options are the same as
         * in QuestStep.QuestStepStatus.
         */
        private void SendQuestEvent(QuestStep.QuestStepStatus eventStatus) 
            => questEventBoard.AddEvent(QuestEvent.Create(this, eventStatus));
        
        // This version sends an event but with the QuestStep attached, meaning it is a QuestStep event, not the full Quest.
        private void SendQuestStepEvent(QuestStep.QuestStepStatus eventStatus, QuestStep questStep) 
            => questEventBoard.AddEvent(QuestEvent.Create(this, eventStatus, questStep));

        /// <summary>
        /// Returns the unique id for this
        /// </summary>
        /// <returns></returns>
        private string GetUid() => Uid();

        public void CreateUid()
        {
            // Not used in this context.
        }

        /// <summary>
        /// Returns the unique in-game id for this
        /// </summary>
        /// <returns></returns>
        private string GetGameUid() => GameId();

        /// <summary>
        /// Adds time to the expiration time of this GameQuest
        /// </summary>
        /// <param name="valueAdd"></param>
        /// <returns></returns>
        public float ExtendEndTime(float valueAdd) => endTime += valueAdd;
        
        /// <summary>
        /// Returns a clone of this object
        /// </summary>
        /// <returns></returns>
        public GameQuest Clone()
        {
            // Note: The clone will clone the Dictionaries object after, or it will continue to be linked directly to
            // the original object.
            var clonedQuest = JsonUtility.FromJson<GameQuest>(JsonUtility.ToJson(this));
            clonedQuest.dictionaries = clonedQuest.dictionaries.Clone();
            return clonedQuest;
        }
        
        

        // This asks the conditions on all the QuestSteps to save their current value if that
        // option is toggled on for them.
        private void SaveConditionValues()
        {
            foreach (var step in questSteps)
            {
                foreach (var condition in step.successConditions)
                    SaveValues(condition, step);
                foreach (var condition in step.failureConditions)
                    SaveValues(condition, step);
            }
        }

        // This is where we ask the condition to save it's current value, if that option is 
        // toggled on.
        private void SaveValues(QuestCondition condition, QuestStep questStep)
        {
            if (!condition.saveCurrentValue) return;

            condition.SaveCurrentValue(this, questStep);
        }

        // ------------------------------------------------------------------------------------------
        // BLACKBOARD EVENTS
        // ------------------------------------------------------------------------------------------
        
        private void SubscribeToBlackboard()
        {
            if (!Parent().subscribeToBlackboard) return;
            
            UnsubscribeFromBlackboard();
            blackboard.Subscribe(this);
        }
        
        private void UnsubscribeFromBlackboard() => blackboard.Unsubscribe(this);
        
        /*
         * If "Query Every Frame" is false, then this is the only way Quest Steps can be automatically completed. The
         * updated Note is received, and then we run through QuestStepTicks, which would be called every frame if
         * Query Every Frame was set true. 
         */
        public void ReceiveChange(BlackboardNote blackboardNote)
        {
            // December 26, 2022 -- Turns out, when a note is received we don't have to do anything special, since 
            // the code already checks every frame (if query every frame is true), and the same code works regardless --
            // as it just checks the Blackboard.
            
            // Let each questStep receive the change
            //foreach (var questStep in questSteps)
            //    questStep.ReceiveChange(blackboardNote);

            // Run through the checks for each
            QuestStepTicks();
        }

        public void ReceiveEvent(BlackboardEvent blackboardEvent)
        {
            /*
             * December 26, 2022 -- I think we should add this. However, QuestStepTicks doesn't really work with this
             * Events would be like "Killed a Goblin", which may not be something we track, and could be really anything
             * ... so events are useful.
             *
             * When one is received, we should check QuestSteps that have conditions which look for Blackboard Events. This
             * may be something new we need to code for.
             */
        }
        
        public virtual void AddDictionaryKey(string key) => dictionaries.AddNewKeyValue(key);

        public virtual KeyValue GetKeyValue(string key) => dictionaries.Key(key);

        public virtual bool HasKeyValue(string key) => dictionaries.HasKeyValue(key);
    }
}