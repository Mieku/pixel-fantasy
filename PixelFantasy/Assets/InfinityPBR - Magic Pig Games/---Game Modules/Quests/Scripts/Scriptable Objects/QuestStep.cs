using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static InfinityPBR.Modules.MainBlackboard;

namespace InfinityPBR.Modules
{
    [Serializable]
    public class QuestStep
    {
        public string name = "Quest Step";
        public Quest quest; // What quest this is part of
        public string description;
        public bool canRevert = true; // If true, the condition can revert it's status. I.e. if the value required changes, it may go from "complete" to not complete.
        public QuestStepStatus status = QuestStepStatus.InProgress;
        public bool sendQuestEvents;

        public List<QuestReward> successRewards = new List<QuestReward>();
        public List<QuestReward> failureRewards = new List<QuestReward>();
        public List<CustomQuestReward> customSuccessRewards = new List<CustomQuestReward>(); 
        public List<CustomQuestReward> customFailureRewards = new List<CustomQuestReward>();

        public GameQuest ParentGameQuest{ get; set; }

        //public QuestCondition questCondition;

        [HideInInspector] public int toolbarIndex;
        
        /*
         * This will return true if any of the conditions require an Object ID. Only one
         * condition per QuestStep can require an Object Id. It can be pulled from the _owner,
         * set specifically in the Inspector of the Quest as a string (if you know the value ahead of time),
         * or can be set via code after the quest is created using SetObjectId()
         */
        public bool RequiresGameId()
        {
            if (successConditions.Any(x => x.gameIdAsTopic)) return true;
            if (failureConditions.Any(x => x.gameIdAsTopic)) return true;
            return false;
        }

        /*
         * When ObjectId is needed for the Blackboard Topic, generally it will be taken from _owner, as that is a
         * common source for the data being retrieved (the owner of the quest, like the player). Sometimes, that is not
         * the case. If _objectID is populated, then the system will use that value instead of the ObjectId of the _owner.
         */
        [SerializeField] private IHaveQuests _owner;
        [SerializeField] private string _objectId;

        /*
         * Note: QuestStepStatus is also used on Quest and GameQuest objects as well.
         */
        public enum QuestStepStatus
        {
            Started,
            InProgress,
            Succeeded,
            Failed
        }

        public bool hasTimeLimit;
        public float expirationTime = -1;
        
        public List<QuestCondition> successConditions = new List<QuestCondition>();
        public List<QuestCondition> failureConditions = new List<QuestCondition>();
        
        public string GameId()
        {
            // Return the object id if it is set
            if (!string.IsNullOrWhiteSpace(_objectId))
                return _objectId;
            
            // If we don't have an owner return default (nothing, really)
            if (_owner == null)
                return default;
            
            // Return the ObjectId of the owner
            return _owner.GameId();
        }
        public int gameIdOption; // Used in the inspector!
        public void SetGameId(string value) => _objectId = value;
        public void ClearGameId() => _objectId = "";
        public void SetQuest(Quest value) => quest = value;

        // Editor Script
        [HideInInspector] public bool show;

        //private bool _hasBlackboard; // Set automatically if we can find the static reference.
        private bool _setup;

        // Add a reward to this QuestStep. Useful for customizing in-game rewards, such as allowing a player to choose
        // a reward for a quest opportunity.
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
        
        public QuestStep Clone() => JsonUtility.FromJson<QuestStep>(JsonUtility.ToJson(this));

        /// <summary>
        /// Checks each QuestStep to see if we have succeeded in all of them, set that and return true. Otherwise, if we have
        /// failed in at least one, then we will set the quest to failed and return true. Otherwise, return false.
        /// </summary>
        /// <returns></returns>
        public bool CheckForCompletion()
        {
            // If we have succeeded or failed, and we can't revert, return true
            if (status != QuestStepStatus.InProgress && !canRevert) 
                return true;

            // If we have succeeded, return true
            if (CheckForSuccess()) 
                return true;
            
            // If we have failed, return true
            if (CheckForFailure())
                return true;
            
            // We have not completed the quest, return false
            return false;
        }

        public bool CheckForSuccess()
        {
            // If there are no success conditions, then this CAN NOT succeed. Generally this is because you're handling 
            // it very manually, or it's a time limit quest that only succeeds when time expires.
            if (successConditions.Count == 0) 
                return false;
            
            // If not all conditions have success, return false. All conditions must be met to success in the questStep
            if (!successConditions.All(x => x.ConditionMet(this, _owner)))
            {
                // Will return false. First, if it has already succeeded, then mark it as In Progress
                if (status == QuestStepStatus.Succeeded)
                    SetInProgress();
                return false;
            }

            // If status is already succeeded, it means we're rechecking, so we do not need to do the "Just completed"
            // actions, and can just return true;
            if (status == QuestStepStatus.Succeeded)
                return true;

            SetSucceeded();
            return true;
        }

        public bool CheckForFailure()
        {
            // If NO conditions have failed, return false. A single failure = the entire questStep is failed.
            if (!failureConditions.Any(x => x.ConditionMet(this, _owner))) 
                return false;
            
            // If status is already failed, it means we're rechecking, so we do not need to do the "Just failed"
            // actions, and can just return true;
            if (status == QuestStepStatus.Failed)
                return true;
            
            SetFailed();
            return true;
        }

        /// <summary>
        /// Sets the step to failed if we have not succeeded. Will check once before failing. If the quest has previously
        /// succeeded and we can't revert, it will not be set failed even if conditions have changed. Note, this will
        /// set the quest to failed even if no failure conditions have been met!
        /// </summary>
        public void SetFailedIfNotCompleted(bool forceCanNotRevert = false)
        {
            if (status == QuestStepStatus.Succeeded && !canRevert) return;
            if (CheckForSuccess()) return;

            SetFailed(forceCanNotRevert);
        }
        
        /// <summary>
        /// Sets the step to failed if we have not succeeded. Will check once before failing. If the quest has previously
        /// succeeded and we can't revert, it will not be set failed even if conditions have changed. Note, this will
        /// set the quest to failed even if no failure conditions have been met!
        /// </summary>
        public void SetSucceededIfNotCompleted(bool forceCanNotRevert = false)
        {
            if (status == QuestStepStatus.Failed && !canRevert) return;
            if (CheckForFailure()) return;

            SetSucceeded(forceCanNotRevert);
        }

        /// <summary>
        /// Sets the step to failed, and optionally will set it so that it can not revert back to InProgress status.
        /// </summary>
        /// <param name="forceCanNotRevert"></param>
        public void SetFailed(bool forceCanNotRevert = false)
        {
            // If we have already failed, do nothing
            if (status == QuestStepStatus.Failed) return;

            // Set the status to failed
            status = QuestStepStatus.Failed;
            
            // Give rewards
            if (ParentGameQuest != null)
                ParentGameQuest.GiveQuestStepRewards(failureRewards, customFailureRewards);
            
            // If we aren't forcing it to not revert, then do nothing else
            if (!forceCanNotRevert) return;
            
            // Set this so that it can not revert back from Failed status
            SetCanRevert(false);
        }

        /// <summary>
        /// Sets the step to succeeded, and optionally will set it so that it can not revert back to InProgress status.
        /// </summary>
        /// <param name="forceCanNotRevert"></param>
        public void SetSucceeded(bool forceCanNotRevert = false)
        {
            // If we have already succeeded, do nothing
            if (status == QuestStepStatus.Succeeded) return;
            
            // Set the status to succeeded
            status = QuestStepStatus.Succeeded;
            
            // Give rewards
            if (ParentGameQuest != null)
                ParentGameQuest.GiveQuestStepRewards(successRewards, customSuccessRewards);

            // If we aren't forcing it to not revert, then do nothing else
            if (!forceCanNotRevert) return;
            
            // Set this so that it can not revert back from Succeeded status
            SetCanRevert(false);
        }

        /// <summary>
        /// Forces the step to be able to revert or not based on the value provided. Defaults to true.
        /// </summary>
        /// <param name="value"></param>
        public void SetCanRevert(bool value = true) => canRevert = value;
        
        /// <summary>
        /// Sets the step to in progress. If we have already succeeded or failed, and we can't revert, do nothing.
        /// </summary>
        public void SetInProgress()
        {
            // If we are already in progress, do nothing
            if (status == QuestStepStatus.InProgress) return;

            // If we can't revert, we can't set it to in progress
            if (!canRevert) return;
            
            // Set the status to in progress
            status = QuestStepStatus.InProgress;
        }

        public void Tick(IHaveQuests owner = null, float gametimeNow = -1f) => TickActions(owner);

        public void LateTick(IHaveQuests owner = null, float gametimeNow = -1f) => TickActions(owner);

        private void TickActions(IHaveQuests owner)
        {
            if (!Setup()) return; // Do the initial setup and reference checks
            _owner = owner;
            
            // Check for completion (success or failure) and return out if we have completed. If canRevert is true,
            // then we may return false even if we had returned true before.
            if (CheckForCompletion())
                return;
        }

        private bool Setup()
        {
            if (_setup) return true;

            //if (blackboard != null) _hasBlackboard = true;
            
            _setup = true;
            return true;
        }

        public void RemoveCondition(QuestCondition questCondition)
        {
            RemoveSuccessCondition(questCondition);
            RemoveFailureCondition(questCondition);
        }
        
        public void RemoveSuccessCondition(QuestCondition questCondition) 
            => successConditions.RemoveAll(x => x == questCondition);
        
        public void RemoveFailureCondition(QuestCondition questCondition) 
            => failureConditions.RemoveAll(x => x == questCondition);

        public void AddSuccessCondition(QuestCondition questCondition)
        {
            if (successConditions.Contains(questCondition)) return;
            successConditions.Add(questCondition);
        }
        
        public void AddFailureCondition(QuestCondition questCondition)
        {
            if (failureConditions.Contains(questCondition)) return;
            failureConditions.Add(questCondition);
        }
        
        /*
         * ReceiveChange() requires the Quest "Subscribe to Blackboard". When a note is updated, or an event occurs,
         * this will be engaged. Each Quest Condition will be checked only if the Topic/Subject matches what they
         * are concerned about.
         */
        public void ReceiveChange(BlackboardNote blackboardNote)
        {
            // Skip this if we've completed or failed, but can't revert.
            if (status != QuestStepStatus.InProgress && !canRevert) return;

            Debug.Log($"Received change from {blackboardNote.topic} / {blackboardNote.subject} ... {blackboardNote.valueString}");
            
            foreach (var condition in successConditions)
            {
                // Skip if topic or subject do not match the Blackboard Note
                if (condition.topic != blackboardNote.topic ||
                    condition.subject != blackboardNote.subject) continue;
            }
        }

        public void CompleteQuest()
        {
            foreach (var condition in successConditions)
                condition.CompleteQuest();
            foreach (var condition in failureConditions)
                condition.CompleteQuest();
        }
        
        //****************
        // CONSTRUCTOR
        //****************

        public QuestStep(Quest newQuest)
        {
            quest = newQuest;
        }
    }
}