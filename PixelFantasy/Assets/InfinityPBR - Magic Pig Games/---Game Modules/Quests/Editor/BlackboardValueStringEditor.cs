using System;
using UnityEditor;

namespace InfinityPBR.Modules
{
    [CustomEditor(typeof(BlackboardValueString))]
    [CanEditMultipleObjects]
    [Serializable]
    public class BlackboardValueStringEditor : QuestConditionEditor
    {
        protected override QuestConditionDrawer CreateDrawer()
        {
            questConditionDrawer = CreateInstance<BlackboardValueStringDrawer>();
            return questConditionDrawer;
        }
    }
}