using System;
using UnityEditor;

namespace InfinityPBR.Modules
{
    [CustomEditor(typeof(BlackboardValueBool))]
    [CanEditMultipleObjects]
    [Serializable]
    public class BlackboardValueBoolEditor : QuestConditionEditor
    {
        protected override QuestConditionDrawer CreateDrawer()
        {
            questConditionDrawer = CreateInstance<BlackboardValueBoolDrawer>();
            return questConditionDrawer;
        }
    }
}