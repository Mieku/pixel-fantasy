using System;
using UnityEditor;

namespace InfinityPBR.Modules
{
    [CustomEditor(typeof(GameConditionListContains))]
    [CanEditMultipleObjects]
    [Serializable]
    public class GameConditionListContainsEditor : QuestConditionEditor
    {
        protected override QuestConditionDrawer CreateDrawer()
        {
            questConditionDrawer = CreateInstance<GameConditionListContainsDrawer>();
            return questConditionDrawer;
        }
    }
}