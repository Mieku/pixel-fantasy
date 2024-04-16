using System;
using UnityEditor;

namespace InfinityPBR.Modules
{
    [CustomEditor(typeof(QuestQuestReward))]
    [CanEditMultipleObjects]
    [Serializable]
    public class QuestQuestRewardEditor : QuestRewardEditor
    {
        protected override QuestRewardDrawer CreateDrawer()
        {
            questRewardDrawer = CreateInstance<QuestQuestRewardDrawer>();
            return questRewardDrawer;
        }
    }
}