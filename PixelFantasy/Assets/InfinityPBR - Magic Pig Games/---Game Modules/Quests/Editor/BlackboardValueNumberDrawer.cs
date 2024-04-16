namespace InfinityPBR.Modules
{
    public class BlackboardValueNumberDrawer : QuestConditionDrawer
    {
        public override bool CanHandle(QuestCondition questCondition) 
            => questCondition is BlackboardValueNumber;
        
        protected override void ShowSpecificData(QuestCondition questCondition) 
            => ShowData(questCondition as BlackboardValueNumber);

        private void ShowData(BlackboardValueNumber questCondition)
        {
            StartRow();
            Label("Number type", 120);
            var barOptions = new[] {"int", "float"};
            var toolbarOption = Toolbar(barOptions, questCondition.useBlackboardValueInt ? 0 : 1, 200);
            questCondition.useBlackboardValueInt = toolbarOption == 0;
            EndRow();
            
            StartRow();
            Label($"Value Comparison {symbolInfo}", "This is the comparison used to determine if the Quest Condition has been met.", 120);
            questCondition.valueComparison = (QuestCondition.ValueComparison) EnumPopup(questCondition.valueComparison, 200);
            EndRow();

            StartRow();
            Label($"Value {symbolInfo}", "The value used in the comparison.", 120);
            if (questCondition.useBlackboardValueInt)
                questCondition.value = Int((int)questCondition.value, 200);
            else
                questCondition.value = Float(questCondition.value, 200);
            EndRow();

            Space();
            StartRow();
            Label($"Save start value {symbolInfo}", "If true, the system will save the existing value of the Topic/Subject, and " +
                                                    "success means a combination of the existing value and \"Value\". This can be " +
                                                    "helpful when the goal is to add an additional value to what is already existing, such " +
                                                    "as \"Kill 10 more goblins\" when the player has already killed 15. Otherwise, the " +
                                                    "condition will be met right away, as they have already killed more than the required amount.\n\n" +
                                                    "In that example, the player will need to kill 25 total -- the 15 they already killed and " +
                                                    "10 more.", 120);
            questCondition.saveCurrentValue = Check(questCondition.saveCurrentValue);
            EndRow();

            if (!questCondition.saveCurrentValue) return;
        }
    }
}