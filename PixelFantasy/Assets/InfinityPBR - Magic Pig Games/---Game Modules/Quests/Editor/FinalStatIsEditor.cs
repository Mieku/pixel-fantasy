using System;
using UnityEditor;

namespace InfinityPBR.Modules
{
    [CustomEditor(typeof(FinalStatIs))]
    [CanEditMultipleObjects]
    [Serializable]
    public class FinalStatIsEditor : QuestConditionEditor
    {
        protected override QuestConditionDrawer CreateDrawer()
        {
            questConditionDrawer = CreateInstance<FinalStatIsDrawer>();
            return questConditionDrawer;
        }
    }
}