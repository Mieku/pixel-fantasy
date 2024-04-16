using UnityEditor;
using UnityEngine;

namespace InfinityPBR.Modules
{
    public class QuestQuestRewardDrawer : QuestRewardDrawer
    {
        public override bool CanHandle(QuestReward questReward) 
            => questReward is QuestQuestReward;

        protected override void ShowSpecificData(QuestReward questReward) 
            => ShowData(questReward as QuestQuestReward);

        private void ShowData(QuestQuestReward questReward)
        {
        
            Label("(Quest) Quest Rewards", true);
            MessageBox("Add \"Quest\" quest rewards here, and the recipient will gain each quest in this list.");

            ShowHeader(questReward);
            Undo.RecordObject(questReward, "Undo Quest Change");
            ShowQuests(questReward);
            Undo.RecordObject(questReward, "Undo Quest Add");
            ShowAddQuest(questReward);
        }

        private void ShowAddQuest(QuestQuestReward questReward)
        {
            BackgroundColor(Color.yellow);
            StartRow();
            Label("Add Quest", 150);
            var newQuest = Object(null, typeof(Quest), 200) as Quest;
            EndRow();
            ResetColor();
            if (newQuest == null) return;

            if (questReward.quests.Contains(newQuest)) return;
            
            questReward.quests.Add(newQuest);
        }

        private void ShowQuests(QuestQuestReward questReward)
        {
            if (questReward.quests.Count == 0) return;
            
            foreach (var quest in questReward.quests)
            {
                StartRow();
                if (XButton())
                {
                    questReward.quests.Remove(quest);
                    ExitGUI();
                }

                Object(quest, typeof(Quest), 150);
                
                EndRow();
            }
        }

        private void ShowHeader(QuestQuestReward questReward)
        {
            if (questReward.quests.Count == 0) return;
            
            StartRow();
            Label("", 25);
            Label("Quest", 150);
            
            EndRow();
        }
    }
}