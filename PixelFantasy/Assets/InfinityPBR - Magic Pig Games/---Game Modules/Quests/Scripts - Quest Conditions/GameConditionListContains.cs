using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*
 * This Quest Condition searches for a specific Condition in a GameConditionList.
 */

namespace InfinityPBR.Modules
{
    [CreateAssetMenu(fileName = "Game Condition List Contains...", menuName = "Game Modules/Quest Condition/Game Condition List Contains", order = 1)]
    public class GameConditionListContains : QuestCondition
    {
        [Header("Game Condition List Options")]
        public ValueContents valueContents = ValueContents.HasMoreThan;
        public GroupOfObjects groupOfObjects = GroupOfObjects.Sum;
        public int quantity;
        public GameModuleAspect gameModuleAspect = GameModuleAspect.Item;
        public List<Condition> value;
        public List<string> types;
        
        public override bool ConditionMet<T>(QuestStep questStep, T obj)
        {
            // If the obj passed in is a GameConditionList, we can skip to the check on whether
            // the condition is met. [This is not an expected behavior, would only happen if
            // the developer is using the QuestCondition in another way...which is fine!]
            if (obj is GameConditionList list)
                return ConditionMet(list);
           
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
        public override bool ConditionMet(BlackboardNote blackboardNote) => ConditionMet(blackboardNote.valueGameConditionList);

        // Compute whether the condition has been met.
        private bool ConditionMet(GameConditionList list)
        {
            // Return the value comparison
            if (valueContents == ValueContents.IsEmpty)
                return list.Count() == 0;
            
            if (valueContents == ValueContents.IsNotEmpty)
                return list.Count() != 0;

            if (gameModuleAspect == GameModuleAspect.Item)
                return ConditionMetBasedOnItems(list);
            
            if (gameModuleAspect == GameModuleAspect.Type)
                return ConditionMetBasedOnTypes(list);
            
            return false; // This should never really trigger.
        }

        private bool ConditionMetNoItems(GameConditionList list)
        {
            if (valueContents == ValueContents.HasExactly)
                return value.Count == quantity;

            if (valueContents == ValueContents.HasLessThan)
                return value.Count < quantity;
                
            if (valueContents == ValueContents.HasLessThanOrEqualTo)
                return value.Count <= quantity;

            if (valueContents == ValueContents.HasMoreThan)
                return value.Count > quantity;
                
            if (valueContents == ValueContents.HasMoreThanOrEqualTo)
                return value.Count >= quantity;

            return false;
        }

        private bool ConditionMetBasedOnTypes(GameConditionList list)
        {
            if (types.Count == 0)
                return ConditionMetNoItems(list);

            if (groupOfObjects == GroupOfObjects.Sum)
            {
                var sum = types.Sum(type => list.list.Count(item => item.ObjectType() == type));
                
                if (valueContents == ValueContents.HasExactly) return sum == quantity;
                if (valueContents == ValueContents.HasLessThan) return sum < quantity;
                if (valueContents == ValueContents.HasLessThanOrEqualTo) return sum <= quantity;
                if (valueContents == ValueContents.HasMoreThan) return sum > quantity;
                if (valueContents == ValueContents.HasMoreThanOrEqualTo) return sum >= quantity;
            }
            else if (groupOfObjects == GroupOfObjects.Any)
            {
                if (valueContents == ValueContents.HasExactly)
                    return types.Any(type => list.list.Count(item => item.ObjectType() == type) == quantity);

                if (valueContents == ValueContents.HasLessThan)
                    return types.Any(type => list.list.Count(item => item.ObjectType() == type) < quantity);
                
                if (valueContents == ValueContents.HasLessThanOrEqualTo)
                    return types.Any(type => list.list.Count(item => item.ObjectType() == type) <= quantity);

                if (valueContents == ValueContents.HasMoreThan)
                    return types.Any(type => list.list.Count(item => item.ObjectType() == type) > quantity);
                
                if (valueContents == ValueContents.HasMoreThanOrEqualTo)
                    return types.Any(type => list.list.Count(item => item.ObjectType() == type) >= quantity);
            }
            else if (groupOfObjects == GroupOfObjects.Each)
            {
                if (valueContents == ValueContents.HasExactly)
                    return types.All(type => list.list.Count(item => item.ObjectType() == type) == quantity);

                if (valueContents == ValueContents.HasLessThan)
                    return types.All(type => list.list.Count(item => item.ObjectType() == type) < quantity);

                if (valueContents == ValueContents.HasLessThanOrEqualTo)
                    return types.All(type => list.list.Count(item => item.ObjectType() == type) <= quantity);

                if (valueContents == ValueContents.HasMoreThan)
                    return types.All(type => list.list.Count(item => item.ObjectType() == type) > quantity);

                if (valueContents == ValueContents.HasMoreThanOrEqualTo)
                    return types.All(type => list.list.Count(item => item.ObjectType() == type) >= quantity);
            }


            return false;
        }

        private bool ConditionMetBasedOnItems(GameConditionList list)
        {
            if (value.Count == 0)
                return ConditionMetNoItems(list);

            if (groupOfObjects == GroupOfObjects.Sum)
            {
                var sum = value.Sum(condition => list.Count(condition.Uid()));
                
                if (valueContents == ValueContents.HasExactly) return sum == quantity;
                if (valueContents == ValueContents.HasLessThan) return sum < quantity;
                if (valueContents == ValueContents.HasLessThanOrEqualTo) return sum <= quantity;
                if (valueContents == ValueContents.HasMoreThan) return sum > quantity;
                if (valueContents == ValueContents.HasMoreThanOrEqualTo) return sum >= quantity;
            }
            else if (groupOfObjects == GroupOfObjects.Any)
            {
                if (valueContents == ValueContents.HasExactly)
                    return value.Any(condition => list.Count(condition.Uid()) == quantity);

                if (valueContents == ValueContents.HasLessThan)
                    return value.Any(condition => list.Count(condition.Uid()) < quantity);
                
                if (valueContents == ValueContents.HasLessThanOrEqualTo)
                    return value.Any(condition => list.Count(condition.Uid()) <= quantity);

                if (valueContents == ValueContents.HasMoreThan)
                    return value.Any(condition => list.Count(condition.Uid()) > quantity);
                
                if (valueContents == ValueContents.HasMoreThanOrEqualTo)
                    return value.Any(condition => list.Count(condition.Uid()) >= quantity);
            }
            else if (groupOfObjects == GroupOfObjects.Each)
            {
                if (valueContents == ValueContents.HasExactly)
                    return value.All(condition => list.Count(condition.Uid()) == quantity);

                if (valueContents == ValueContents.HasLessThan)
                    return value.All(condition => list.Count(condition.Uid()) < quantity);

                if (valueContents == ValueContents.HasLessThanOrEqualTo)
                    return value.All(condition => list.Count(condition.Uid()) <= quantity);

                if (valueContents == ValueContents.HasMoreThan)
                    return value.All(condition => list.Count(condition.Uid()) > quantity);

                if (valueContents == ValueContents.HasMoreThanOrEqualTo)
                    return value.All(condition => list.Count(condition.Uid()) >= quantity);
            }

            return false;
        }
    }
}