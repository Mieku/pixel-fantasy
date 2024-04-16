using System;
using UnityEditor;

namespace InfinityPBR.Modules
{
    [CustomEditor(typeof(BlackboardValueNumber))]
    [CanEditMultipleObjects]
    [Serializable]
    public class BlackboardValueNumberEditor : QuestConditionEditor
    {
        protected override QuestConditionDrawer CreateDrawer()
        {
            questConditionDrawer = CreateInstance<BlackboardValueNumberDrawer>();
            return questConditionDrawer;
        }
    }
}