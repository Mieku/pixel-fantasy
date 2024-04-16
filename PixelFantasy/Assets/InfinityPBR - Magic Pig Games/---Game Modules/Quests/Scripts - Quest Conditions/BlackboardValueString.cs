using UnityEngine;
using static InfinityPBR.Modules.MainBlackboard;

namespace InfinityPBR.Modules
{
    [CreateAssetMenu(fileName = "Blackboard Value String Is...", menuName = "Game Modules/Quest Condition/Blackboard Value String", order = 1)]
    public class BlackboardValueString : QuestCondition
    {
        [Header("Blackboard Value String Options")] 
        public string value;
        public bool valueIs = true;

        public override bool ConditionMet(BlackboardNote blackboardNote)
        {
            // Ensure the blackboard is in the scene and available
            if (blackboard == null)
            {
                Debug.Log("The static reference to the Blackboard was null. Did you forget to add it to the scene?");
                return false;
            }

            // Get the blackboard note.
            if (!blackboard.TryGet(topic, subject, out BlackboardNote note))
                return false;

            // Return true if the values match
            if (valueIs && note.valueString == value)
                return true;
            
            // Return true if the values do not match
            if (!valueIs && note.valueString != value)
                return true;

            return false;
        }
    }
}