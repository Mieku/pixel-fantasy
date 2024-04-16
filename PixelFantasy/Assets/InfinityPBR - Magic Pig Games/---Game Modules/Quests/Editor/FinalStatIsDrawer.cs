namespace InfinityPBR.Modules
{
    public class FinalStatIsDrawer : QuestConditionDrawer
    {
        public override bool CanHandle(QuestCondition questCondition) 
            => questCondition is FinalStatIs;
        
        protected override void ShowSpecificData(QuestCondition questCondition) 
            => ShowData(questCondition as FinalStatIs);

        private void ShowData(FinalStatIs questCondition)
        {
            Label("Final Stat", true);
            
            StartRow();
            Label($"Stat {symbolInfo}", "Will be looking for GameStat version of this Stat in the Blackboard Note.", 120);
            questCondition.stat = Object(questCondition.stat, typeof(Stat), 200) as Stat;
            EndRow();
            
            StartRow();
            Label($"Value Comparison {symbolInfo}", "This is the comparison used to determine if the Quest Condition has been met.", 120);
            questCondition.valueComparison = (QuestCondition.ValueComparison) EnumPopup(questCondition.valueComparison, 200);
            EndRow();

            StartRow();
            Label($"Value {symbolInfo}", "The value used in the comparison.", 120);
            questCondition.value = Float(questCondition.value, 200);
            EndRow();
        }
    }
}