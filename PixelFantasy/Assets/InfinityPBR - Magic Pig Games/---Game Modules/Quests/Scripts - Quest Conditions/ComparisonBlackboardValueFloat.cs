using System;
using UnityEngine;

/*
 * December 27, 2022 -- Is this being used??
 */

namespace InfinityPBR.Modules
{
    [Serializable]
    public class ComparisonBlackboardValueFloat
    {
        public enum Comparison {EqualTo, NotEqualTo, GreaterThan, LessThan, GreaterThanOrEqualTo, LessThanOrEqualTo, True, False, Null, NotNull}

        // BLACKBOARD NOTE
        
        public Comparison comparison;
        public float compareValue;
        
        public bool useValueInt;
        
        public bool ConditionMet(QuestStep questStep)
        {
            /*
            if (!blackboard.TryGet(topic, subject, out BlackboardNote note))
                return false;

            var value = useValueInt ? note.valueInt : note.valueFloat;

            return Compare(value);
            */
            return false;
        }

        private bool Compare(in float value)
        {
            throw new NotImplementedException();
        }

        [HideInInspector] public bool valueFloatOn;
        public Comparison comparisonFloat = Comparison.GreaterThan;
        public float valueFloat;
        
        [HideInInspector] public bool valueIntOn;
        public Comparison comparisonInt = Comparison.GreaterThanOrEqualTo;
        public int valueInt;
        
        [HideInInspector] public bool valueStringOn;
        public Comparison comparisonString = Comparison.EqualTo;
        public string valueString; 
        
        [HideInInspector] public bool valueBoolOn;
        public Comparison comparisonBool = Comparison.True;
        public bool valueBool;
        
        /*
        [HideInInspector] public bool valueGameObjectOn = false;
        public Comparison comparisonGameObject = Comparison.NotNull;
        public GameObject valueGameObject;
        
        [HideInInspector] public bool valueGameStatOn = false;
        public Comparison comparisonGameStat = Comparison.EqualTo;
        public GameStat valueGameStat;
        
        [HideInInspector] public bool valueGameItemObjectOn = false;
        public Comparison comparisonGameItemObject = Comparison.EqualTo;
        public GameItemObject valueGameItemObject;
        
        [HideInInspector] public bool valueGameItemAttributeOn = false;
        public Comparison comparisonGameItemAttribute = Comparison.EqualTo;
        public GameItemAttribute valueGameItemAttribute;
        
        [HideInInspector] public bool valueGameConditionOn = false;
        public Comparison comparisonGameCondition = Comparison.EqualTo;
        public GameCondition valueGameCondition;
        
        [HideInInspector] public bool valueGameQuestOn = false;
        public Comparison comparisonGameQuest = Comparison.EqualTo;
        public GameQuest valueGameQuest;
        
        [HideInInspector] public bool valueGameLootBoxOn = false;
        public Comparison comparisonGameLootBox = Comparison.EqualTo;
        public GameLootBox valueGameLootBox;
        */
    }
}