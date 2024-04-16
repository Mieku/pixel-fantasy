using UnityEngine;
using static InfinityPBR.Modules.MainBlackboard;

/*
 * This considers an int or float value. However, it has an extra section that others do not. Some quests may
 * require the player to do X "more" than they already have. In the demo scene, the quest is to "add 100 gold", to
 * whatever value exists. In these cases, the additional methods are used to save the current value of the
 * condition to meet, so that value can be deducted from the "current" value when the condition is being resolved.
 */

namespace InfinityPBR.Modules
{
    [CreateAssetMenu(fileName = "Blackboard Value Number Is...", menuName = "Game Modules/Quest Condition/Blackboard Value Number", order = 1)]
    public class BlackboardValueNumber : QuestCondition
    {
        [Header("Blackboard Value Number Options")]
        public bool useBlackboardValueInt; // If true, we will pull the valueInt, otherwise the valueFloat
        public ValueComparison valueComparison = ValueComparison.EqualTo;
        public float value;

        public override bool ConditionMet(BlackboardNote blackboardNote)
        {
            // Get the number we are comparing
            var number = useBlackboardValueInt ? blackboardNote.valueInt : blackboardNote.valueFloat;

            // Set it as an int
            if (useBlackboardValueInt)
                number = (int)number;

            // Return the value comparison
            if (valueComparison == ValueComparison.EqualTo)
                return number == TrueValue();
            
            if (valueComparison == ValueComparison.NotEqualTo)
                return number != TrueValue();
            
            if (valueComparison == ValueComparison.GreaterThan)
                return number > TrueValue();
            
            if (valueComparison == ValueComparison.LessThan)
                return number < TrueValue();

            if (valueComparison == ValueComparison.GreaterThanOrEqualTo)
                return number >= TrueValue();
            
            if (valueComparison == ValueComparison.LessThanOrEqualTo)
                return number <= TrueValue();
            
            return false; // This should never really trigger.
        }

        public override float TrueValue()
        {
            // Return value if we did not save the current value
            if (!saveCurrentValue) return value;

            // If we can't find the saved note in the blackboard, return value
            if (!blackboard.TryGet(_savedTopic, _savedSubject, out BlackboardNote savedNote))
                return value;

            // Return value plus the saved value (int or float)
            return value + (useBlackboardValueInt ? savedNote.valueInt : savedNote.valueFloat);
        }

        // This will save the current value and the special topic/subject
        public override void SaveCurrentValue(GameQuest gameQuest, QuestStep questStep)
        {
            if (!saveCurrentValue) return;

            // Save the new topic/subject
            _savedTopic = gameQuest.GameId();
            _savedSubject = questStep.name;
            
            // Set the current value. If we don't have a note, then set to 0
            var currentValue = 0f;
            if (blackboard.TryGet(topic, subject, out BlackboardNote savedNote))
                currentValue = useBlackboardValueInt ? savedNote.valueInt : savedNote.valueFloat;

            // Save the current value
            var newNote = new BlackboardNote(_savedTopic, _savedSubject);
            if (useBlackboardValueInt)
                newNote.valueInt = (int)currentValue;
            else
                newNote.valueFloat = currentValue;
            blackboard.AddNote(newNote);
        }

        // This will remove the blackboard notes that we saved while this was in play, as we do not need them anymore, 
        // and potentially there could be many hundreds of these throughout a game, clogging data.
        public override void CompleteQuest()
        {
            if (!saveCurrentValue) return;

            blackboard.RemoveNotes(_savedTopic, _savedSubject);
        }
    }
}