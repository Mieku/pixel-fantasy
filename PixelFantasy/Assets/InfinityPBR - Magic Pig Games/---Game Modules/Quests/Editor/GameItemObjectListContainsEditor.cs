using System;
using UnityEditor;

namespace InfinityPBR.Modules
{
    [CustomEditor(typeof(GameItemObjectListContains))]
    [CanEditMultipleObjects]
    [Serializable]
    public class GameItemObjectListContainsEditor : QuestConditionEditor
    {
        protected override QuestConditionDrawer CreateDrawer()
        {
            questConditionDrawer = CreateInstance<GameItemObjectListContainsDrawer>();
            return questConditionDrawer;
            //return ScriptableObject.CreateInstance<GameItemObjectListContainsDrawer>();
        }
    }
}