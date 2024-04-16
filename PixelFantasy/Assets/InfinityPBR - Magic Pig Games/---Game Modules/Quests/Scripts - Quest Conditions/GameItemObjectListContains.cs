using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InfinityPBR.Modules
{
    [CreateAssetMenu(fileName = "Game Item Object List Contains...", menuName = "Game Modules/Quest Condition/Game Item Object List Contains", order = 1)]
    public class GameItemObjectListContains : QuestCondition
    {
        [Header("Game Item Object List Options")]
        public ValueContents valueContents = ValueContents.HasMoreThan;
        public GroupOfObjects groupOfObjects = GroupOfObjects.Sum;
        public int quantity = -1;
        public GameModuleAspect gameModuleAspect = GameModuleAspect.Item;
        public List<ItemObject> value;
        public List<string> types;
        
        public override bool ConditionMet<T>(QuestStep questStep, T obj)
        {
            // If the obj passed in is a GameItemObjectList, we can skip to the check on whether
            // the condition is met. [This is not an expected behavior, would only happen if
            // the developer is using the QuestCondition in another way...which is fine!]
            if (obj is GameItemObjectList list)
                return ConditionMet(list);
            
            // If the obj is a BlackboardNote, we can use that directly
            if (obj is BlackboardNote note) 
                return ConditionMet(note);
            
            // Grab the BlackboardNote, and return if we've met the condition. Will return false if 
            // the note can't be found. [Thanks Chat GPT for giving a shorter version]
            BlackboardNote foundNote = FoundNote(questStep, obj as IUseGameModules);
            return foundNote != null && ConditionMet(foundNote);
        }
        
        // This QuestCondition does not have to be based on the Blackboard, but could be (and will be if the
        // Quest module is being used).
        public override bool ConditionMet(BlackboardNote blackboardNote) => ConditionMet(blackboardNote.valueGameItemObjectList);
        
        private bool ConditionMet(GameItemObjectList list)
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
        
        private bool ConditionMetNoItems(GameItemObjectList list)
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

        private bool ConditionMetBasedOnTypes(GameItemObjectList list)
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

        private bool ConditionMetBasedOnItems(GameItemObjectList list)
        {
            if (value.Count == 0)
                return ConditionMetNoItems(list);

            if (groupOfObjects == GroupOfObjects.Sum)
            {
                var sum = value.Sum(itemObject => list.Count(itemObject.Uid()));
                
                if (valueContents == ValueContents.HasExactly) return sum == quantity;
                if (valueContents == ValueContents.HasLessThan) return sum < quantity;
                if (valueContents == ValueContents.HasLessThanOrEqualTo) return sum <= quantity;
                if (valueContents == ValueContents.HasMoreThan) return sum > quantity;
                if (valueContents == ValueContents.HasMoreThanOrEqualTo) return sum >= quantity;
            }
            else if (groupOfObjects == GroupOfObjects.Any)
            {
                if (valueContents == ValueContents.HasExactly)
                    return value.Any(itemObject => list.Count(itemObject.Uid()) == quantity);

                if (valueContents == ValueContents.HasLessThan)
                    return value.Any(itemObject => list.Count(itemObject.Uid()) < quantity);
                
                if (valueContents == ValueContents.HasLessThanOrEqualTo)
                    return value.Any(itemObject => list.Count(itemObject.Uid()) <= quantity);

                if (valueContents == ValueContents.HasMoreThan)
                    return value.Any(itemObject => list.Count(itemObject.Uid()) > quantity);
                
                if (valueContents == ValueContents.HasMoreThanOrEqualTo)
                    return value.Any(itemObject => list.Count(itemObject.Uid()) >= quantity);
            }
            else if (groupOfObjects == GroupOfObjects.Each)
            {
                if (valueContents == ValueContents.HasExactly)
                    return value.All(itemObject => list.Count(itemObject.Uid()) == quantity);

                if (valueContents == ValueContents.HasLessThan)
                    return value.All(itemObject => list.Count(itemObject.Uid()) < quantity);

                if (valueContents == ValueContents.HasLessThanOrEqualTo)
                    return value.All(itemObject => list.Count(itemObject.Uid()) <= quantity);

                if (valueContents == ValueContents.HasMoreThan)
                    return value.All(itemObject => list.Count(itemObject.Uid()) > quantity);

                if (valueContents == ValueContents.HasMoreThanOrEqualTo)
                    return value.All(itemObject => list.Count(itemObject.Uid()) >= quantity);
            }

            return false;
        }
    }
}