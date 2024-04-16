namespace InfinityPBR.Modules
{
    public class BlackboardValueStringDrawer : QuestConditionDrawer
    {
        public override bool CanHandle(QuestCondition questCondition) 
            => questCondition is BlackboardValueString;
        
        protected override void ShowSpecificData(QuestCondition questCondition) 
            => ShowData(questCondition as BlackboardValueString);

        private void ShowData(BlackboardValueString questCondition)
        {
            Label("Blackboard Value String", true);
            StartRow();
            Label($"String {symbolInfo}", "This is the string used in the comparison.", 120);
            questCondition.value = TextField(questCondition.value, 200);
            EndRow();
            
            StartRow();
            Label($"Value is {symbolInfo}", "This condition will be met if the string value " +
                                            "of the Blackboard Note, when compared to the GameObject value provided here is " +
                                            $"{questCondition.value}", 120);
            questCondition.valueIs = Check(questCondition.valueIs);
            EndRow();
        }
    }
}