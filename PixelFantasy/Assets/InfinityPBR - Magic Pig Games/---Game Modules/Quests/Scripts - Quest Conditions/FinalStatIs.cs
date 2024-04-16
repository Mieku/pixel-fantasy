using System;
using UnityEngine;

namespace InfinityPBR.Modules
{
    [CreateAssetMenu(fileName = "Final Stat Is...", menuName = "Game Modules/Quest Condition/Final Stat Is", order = 1)]
    public class FinalStatIs : QuestCondition
    {
        [Header("Final Stat Options")] 
        public Stat stat;
        public ValueComparison valueComparison = ValueComparison.EqualTo;
        public float value;
        
        public override bool ConditionMet<T>(QuestStep questStep, T obj)
        {
            // If the obj passed in is a GameStat, we can skip to the check on whether
            // the condition is met. [This is not an expected behavior, would only happen if
            // the developer is using the QuestCondition in another way...which is fine!]
            if (obj is GameStat gameStat)
                return ConditionMet(gameStat);
            
            // If the obj is a BlackboardNote, we can use that directly
            if (obj is BlackboardNote note) 
                return ConditionMet(note);
            
            // Grab the BlackboardNote, and return if we've met the condition. Will return false if 
            // the note can't be found.
            var foundNote = FoundNote(questStep, obj as IUseGameModules);
            return foundNote != null && ConditionMet(foundNote);
        }
        
        // This QuestCondition does not have to be based on the Blackboard, but could be (and will be if the
        // Quest module is being used).
        public override bool ConditionMet(BlackboardNote blackboardNote) => ConditionMet(blackboardNote.valueGameStat);
        
       private bool ConditionMet(GameStat gameStat)
        {
            // Get the final stat
            var finalStat = gameStat.FinalStat();

            // Return the value comparison
            if (valueComparison == ValueComparison.EqualTo)
                return Math.Abs(finalStat - value) < 0.01;
            
            if (valueComparison == ValueComparison.NotEqualTo)
                return Math.Abs(finalStat - value) > 0.01;
            
            if (valueComparison == ValueComparison.GreaterThan)
                return finalStat > value;
            
            if (valueComparison == ValueComparison.LessThan)
                return finalStat < value;

            if (valueComparison == ValueComparison.GreaterThanOrEqualTo)
                return finalStat >= value;
            
            if (valueComparison == ValueComparison.LessThanOrEqualTo)
                return finalStat <= value;
            
            return false; // This should never really trigger.
        }
    }
}