using UnityEngine;

/*
 * This QuestCondition type looks for a Blackboard valueBool that is true or false. The condition is set to be
 * met based on one of the two -- it can be met when the value is false, or true, your choice.
 */

namespace InfinityPBR.Modules
{
    [CreateAssetMenu(fileName = "Blackboard Value Bool Is...", menuName = "Game Modules/Quest Condition/Blackboard Value Bool", order = 1)]
    public class BlackboardValueBool : QuestCondition
    {
        [Header("Blackboard Value Bool Options")]
        public bool value;

        public override bool ConditionMet<T>(QuestStep questStep, T obj)
        {
            // If the obj is a BlackboardNote, we can use that directly
            if (obj is BlackboardNote note) 
                return ConditionMet(note);

            // Grab the BlackboardNote, and return if we've met the condition. Will return false if 
            // the note can't be found.
            var blackboardNote = FoundNote(questStep, obj as IUseGameModules);
            
            return blackboardNote != null && ConditionMet(blackboardNote);
        }

        // The value can be only true or false, so the condition is met if the value from the BlackboardNote
        // matches the value we are looking for.
        public override bool ConditionMet(BlackboardNote blackboardNote) =>
            blackboardNote.valueBool == value;
    }
}