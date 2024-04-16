using System;
using UnityEditor;

namespace InfinityPBR.Modules
{
    [CustomEditor(typeof(GameStatListContains))]
    [CanEditMultipleObjects]
    [Serializable]
    public class GameStatListContainsEditor : QuestConditionEditor
    {
        protected override QuestConditionDrawer CreateDrawer()
        {
            questConditionDrawer = CreateInstance<GameStatListContainsDrawer>();
            return questConditionDrawer;
        }
    }
}