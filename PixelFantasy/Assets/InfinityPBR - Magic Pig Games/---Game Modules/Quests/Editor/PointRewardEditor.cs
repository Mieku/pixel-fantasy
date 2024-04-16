using System;
using UnityEditor;

namespace InfinityPBR.Modules
{
    [CustomEditor(typeof(PointReward))]
    [CanEditMultipleObjects]
    [Serializable]
    public class PointRewardEditor : QuestRewardEditor
    {
        protected override QuestRewardDrawer CreateDrawer()
        {
            questRewardDrawer = CreateInstance<PointRewardDrawer>();
            return questRewardDrawer;
        }
    }
}