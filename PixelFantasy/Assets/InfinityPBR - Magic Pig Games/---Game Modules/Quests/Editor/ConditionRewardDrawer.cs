using UnityEditor;
using UnityEngine;

namespace InfinityPBR.Modules
{
    public class ConditionRewardDrawer : QuestRewardDrawer
    {
        public override bool CanHandle(QuestReward questReward) 
            => questReward is ConditionReward;

        protected override void ShowSpecificData(QuestReward questReward) 
            => ShowData(questReward as ConditionReward);

        private void ShowData(ConditionReward questReward)
        {
        
            Label("Condition Rewards", true);
            MessageBox("Add condition rewards here, and the recipient will gain each condition in this list.");

            ShowHeader(questReward);
            Undo.RecordObject(questReward, "Undo Condition Change");
            ShowConditions(questReward);
            Undo.RecordObject(questReward, "Undo Condition Add");
            ShowAddConditions(questReward);
        }

        private void ShowAddConditions(ConditionReward questReward)
        {
            BackgroundColor(Color.yellow);
            StartRow();
            Label("Add Condition", 150);
            var newCondition = Object(null, typeof(Condition), 200) as Condition;
            EndRow();
            ResetColor();
            if (newCondition == null) return;

            if (questReward.conditions.Contains(newCondition)) return;
            
            questReward.conditions.Add(newCondition);
        }

        private void ShowConditions(ConditionReward questReward)
        {
            if (questReward.conditions.Count == 0) return;
            
            foreach (var condition in questReward.conditions)
            {
                StartRow();
                if (XButton())
                {
                    questReward.conditions.Remove(condition);
                    ExitGUI();
                }

                Object(condition, typeof(Condition), 150);
                
                EndRow();
            }
        }

        private void ShowHeader(ConditionReward questReward)
        {
            if (questReward.conditions.Count == 0) return;
            
            StartRow();
            Label("", 25);
            Label("Condition", 150);
            
            EndRow();
        }
    }
}