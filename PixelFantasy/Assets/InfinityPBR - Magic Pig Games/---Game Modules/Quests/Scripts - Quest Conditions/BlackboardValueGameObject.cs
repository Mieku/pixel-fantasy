using UnityEngine;

/*
 * This QuestCondition type looks for a Blackboard valueGameObject. The condition is set to be
 * met on whether the object is or is not the one we are looking for, essentially true or false.
 */

namespace InfinityPBR.Modules
{
    [CreateAssetMenu(fileName = "Blackboard Value GameObject Is...", menuName = "Game Modules/Quest Condition/Blackboard Value GameObject", order = 1)]
    public class BlackboardValueGameObject : QuestCondition
    {
        [Header("Blackboard Value GameObject Options")]
        public GameObject value;
        public bool valueIs = true;

        public override bool ConditionMet(BlackboardNote blackboardNote)
        {
            // Return true if the values match
            if (valueIs && blackboardNote.valueGameObject == value)
                return true;
            
            // Return true if the values do not match (opposite of the first!)
            if (!valueIs && blackboardNote.valueGameObject != value)
                return true;

            return false;
        }
    }
}