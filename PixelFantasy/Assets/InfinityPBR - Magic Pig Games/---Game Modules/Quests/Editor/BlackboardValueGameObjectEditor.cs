using System;
using UnityEditor;

namespace InfinityPBR.Modules
{
    [CustomEditor(typeof(BlackboardValueGameObject))]
    [CanEditMultipleObjects]
    [Serializable]
    public class BlackboardValueGameObjectEditor : QuestConditionEditor
    {
        protected override QuestConditionDrawer CreateDrawer()
        {
            questConditionDrawer = CreateInstance<BlackboardValueGameObjectDrawer>();
            return questConditionDrawer;
        }
    }
}