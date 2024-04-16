namespace InfinityPBR.Modules
{
    public class BlackboardValueBoolDrawer : QuestConditionDrawer
    {
        public override bool CanHandle(QuestCondition questCondition) 
            => questCondition is BlackboardValueBool;

        protected override void ShowSpecificData(QuestCondition questCondition) 
            => ShowData(questCondition as BlackboardValueBool);

        private void ShowData(BlackboardValueBool questCondition)
        {
            Label("Blackboard Value Bool", true);
            StartRow();
            Label($"Condition met if {symbolInfo}", "This condition will be met if the bool value " +
                                                    "of the Blackboard Note is " +
                                                    $"{questCondition.value}", 150);
            questCondition.value = Check(questCondition.value);
            EndRow();
        }
    }
}