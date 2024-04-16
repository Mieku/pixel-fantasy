using UnityEngine;
using static InfinityPBR.Modules.Utilities;

namespace InfinityPBR.Modules
{
    public class PointRewardDrawer : QuestRewardDrawer
    {
        public override bool CanHandle(QuestReward questReward) 
            => questReward is PointReward;

        protected override void ShowSpecificData(QuestReward questReward) 
            => ShowData(questReward as PointReward);

        private void ShowData(PointReward questReward)
        {
            Label("Point Rewards", true);
            MessageBox("Add point rewards here. Each Reward object can hold a List of rewards, with additional " +
                       "options.");
            ShowPointRewards(questReward);
            ShowAddStatPoint(questReward);
        }

        private void ShowAddStatPoint(PointReward questReward)
        {
            BackgroundColor(Color.yellow);
            if (Button("Add", 50))
                questReward.statPoints.Add(new StatPoints());
            ResetColor();
        }

        private void ShowPointRewards(PointReward questReward)
        {
            if (questReward.statPoints.Count == 0) return;

            StartRow();
            Label("", 25);
            Label($"Stat {symbolInfo}", "The stat that will be affected.", 100);
            Label($"Points {symbolInfo}", "A random value between min and max will be added. Can be negative to remove points.", 80);
            Label($"Add if null {symbolInfo}", "If true, the stat will be added if it does not exist.", 80);
            Label($"Use Prof. Mod {symbolInfo}", "If true false, the points will be added at face value. Otherwise, the points may be modified " +
                                                            "if the Stat has a proficiency modifier attached to its Point value.", 100);
            Label($"Round {symbolInfo}", "Choose how to round the final value.", 60);
            Label($"Decimals {symbolInfo}", "If rounding, round to this many decimal places", 70);
            EndRow();
            
            foreach (var reward in questReward.statPoints)
            {
                StartRow();
                BackgroundColor(Color.red);
                if (Button($"{symbolX}", 25))
                {
                    questReward.statPoints.RemoveAll(x => x == reward);
                    ExitGUI();
                }
                ResetColor();
                reward.stat = Object(reward.stat, typeof(Stat), 100) as Stat;
                reward.pointsMin = Float(reward.pointsMin, 39);
                reward.pointsMax = Float(reward.pointsMax, 39);
                reward.addIfNull = Check(reward.addIfNull, 80);
                reward.useProficiencyModifier = Check(reward.useProficiencyModifier, 100);
                reward.roundingMethod = (Rounding) EnumPopup(reward.roundingMethod, 60);
                if (reward.roundingMethod == Rounding.Round)
                    reward.decimals = Int(reward.decimals, 70);
                EndRow();

                if (reward.pointsMin > reward.pointsMax)
                    reward.pointsMax = reward.pointsMin;
                if (reward.pointsMax < reward.pointsMin)
                    reward.pointsMin = reward.pointsMax;
            }
        }
    }
}