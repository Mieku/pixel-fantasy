using System;
using InfinityPBR.Modules;

namespace InfinityPBR
{
    [Serializable]
    public class CustomQuestReward
    {
        public string name;
        public string uid;
        private QuestReward _questReward;

        public QuestReward Reward =>
            _questReward ??= GameModuleRepository.Instance.Get<QuestReward>(uid); // QuestRepository.questRepository.GetQuestRewardByUid(uid);

        public CustomQuestReward(QuestReward reward)
        {
            name = reward.objectName;
            uid = reward.Uid();
            _questReward = reward;
        }
    }
}