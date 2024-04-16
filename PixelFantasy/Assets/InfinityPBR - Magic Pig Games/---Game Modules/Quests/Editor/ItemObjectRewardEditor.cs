using System;
using UnityEditor;

namespace InfinityPBR.Modules
{
    [CustomEditor(typeof(ItemObjectReward))]
    [CanEditMultipleObjects]
    [Serializable]
    public class ItemObjectRewardEditor : QuestRewardEditor
    {
        protected override QuestRewardDrawer CreateDrawer()
        {
            questRewardDrawer = CreateInstance<ItemObjectRewardDrawer>();
            return questRewardDrawer;
        }
    }
}