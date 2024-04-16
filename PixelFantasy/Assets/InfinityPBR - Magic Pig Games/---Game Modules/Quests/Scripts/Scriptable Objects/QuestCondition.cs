using System;
using UnityEngine;
using UnityEngine.Serialization;
using static InfinityPBR.Modules.MainBlackboard;

namespace InfinityPBR.Modules
{
    [HelpURL("https://infinitypbr.gitbook.io/infinity-pbr/game-modules/quests")]
    [Serializable]
    public abstract class QuestCondition : ModulesScriptableObject
    {
        [Header("Quest Condition Common")]
        public string description; // Can be internal for your notes, or you can expose it to players via the UI

        [FormerlySerializedAs("objectIdAsTopic")] [Header("Blackboard Note")] 
        public bool gameIdAsTopic; // If true, will expect an ObjectID as the "Topic"
        public string topic; // The "Topic" we will look up in the Blackboard
        public bool questNameQuestStepAsSubject; // If true, the "QuestName-QuestStepName" will be used as the subject
        public string subject; // The "Subject" we will look up in the Blackboard
        
        // Used in the Inspector
        [HideInInspector] public bool show;
        [HideInInspector] public bool customNote;
        [HideInInspector] public int toolbarIndex;
        [HideInInspector] public int menubarIndex;
        [HideInInspector] public bool hasBeenSetup;
        [HideInInspector] public bool drawnByGameModulesManager;
        
        protected BlackboardNote FoundNote(QuestStep questStep, IUseGameModules obj) 
            => FindBlackboardNote(questStep, obj);
        
        /*
         * saveCurrentValue means the system will automatically save the current value of the Topic/Subject used for
         * meeting the condition. When the Quest is created, the value will be saved in a special topic/subject on the
         * blackboard based on the Quest Game UID (unique to in-game GameQuest, so multiple GameQuests of the same type
         * can be active at once). The condition will be met only if the final value, inclusive of the saved value, is
         * met.
         *
         * Example, if the current value of "Gold" is 75, and the "Value" for completion is 100, then the actual value
         * will need to be 175.
         */
        public bool saveCurrentValue;
        // These will be populated by the specific QuestCondition types that use them,
        // such as BlackboardValueNumber.cs, which saves the original value of an int or float
        // required to compute the final value. They would only be used if saveCurrentValue is true.
        protected string _savedTopic;
        protected string _savedSubject;

        /*
         * This method finds the Blackboard note associated with this Quest Condition, including
         * looking up the ObjectID if required, or setting the subject of the Blackboard Note
         * based on the Quest name and Quest Step name.
         */
        protected BlackboardNote FindBlackboardNote(QuestStep questStep, IUseGameModules obj)
        {
            // Compute the actual Topic/Subject to search for
            var thisTopic = gameIdAsTopic ? questStep.GameId() : topic;
            var thisSubject = questNameQuestStepAsSubject ? $"{questStep.quest.name}-{questStep.name}" : subject;
           
            // Find the note. Return null if blackboard isn't found, or if the note itself isn't found.
            BlackboardNote foundNote = null;
            foundNote = blackboard?.TryGet(thisTopic, thisSubject, out foundNote) == true ? foundNote : default;
            return foundNote;
        }

        /*
         * Each version of QuestCondition must have it's own ConditionMet method, as it will be looking for
         * different things.
         *
         * The version here is the basic "find the blackboard note" version. Others may have more options, and
         * will override this.
         */
        public virtual bool ConditionMet<T>(QuestStep questStep, T obj)
        {
            // If the obj is a BlackboardNote, we can use that directly
            if (obj is BlackboardNote blackboardNote) 
                return ConditionMet(blackboardNote);

            // Grab the BlackboardNote, and return if we've met the condition. Will return false if 
            // the note can't be found. [Thanks Chat GPT for giving a shorter version]
            BlackboardNote foundNote = FoundNote(questStep, obj as IUseGameModules);
            
            return foundNote != null && ConditionMet(foundNote);
        }

        // All must have their own unique version of this. Some may also have a version that takes in
        // other types of objects, such as the GameConditionListContains.cs
        public abstract bool ConditionMet(BlackboardNote blackboardNote);

        /*
         * These will optionally be overridden by conditions that utilize them,
         * such as BlackboardValueNumber.cs
         */
        public virtual void SaveCurrentValue(GameQuest gameQuest, QuestStep questStep) { }
        public virtual float TrueValue() => default;
        public virtual void CompleteQuest() { }

        /*
         * These are all the enum values used in the various QuestConditions. It may be possible to write
         * your own custom QuestConditions, which could be useful if you have custom object types that you'd
         * like to use in the Quest module.
         */
        
        public enum QuestConditionMetric
        {
            BlackboardNote,
            Gametime,
            Explicit
        }

        public enum ValueComparison
        {
            GreaterThan,
            LessThan,
            EqualTo,
            NotEqualTo,
            GreaterThanOrEqualTo,
            LessThanOrEqualTo
        }
        
        public enum ValueContents
        {
            HasMoreThan,
            HasMoreThanOrEqualTo,
            HasLessThan,
            HasLessThanOrEqualTo,
            HasExactly,
            IsEmpty,
            IsNotEmpty
        }
        
        public enum GroupOfObjects
        {
            Any,
            Sum,
            Each
        }

        public enum GameModuleAspect
        {
            Item,
            Type
        }
    }
}