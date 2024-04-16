using System;
using UnityEditor;

namespace InfinityPBR.Modules
{
    [CustomEditor(typeof(ConditionReward))]
    [CanEditMultipleObjects]
    [Serializable]
    public class ConditionRewardEditor : QuestRewardEditor
    {
        protected override QuestRewardDrawer CreateDrawer()
        {
            questRewardDrawer = CreateInstance<ConditionRewardDrawer>();
            return questRewardDrawer;
        }
    }
}