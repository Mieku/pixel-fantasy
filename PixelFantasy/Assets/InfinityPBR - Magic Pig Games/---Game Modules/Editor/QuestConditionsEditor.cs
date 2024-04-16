using System;
using UnityEditor;

namespace InfinityPBR.Modules
{
    [CanEditMultipleObjects]
    [Serializable]
    public class QuestConditionsEditor : Editor
    {
        private QuestCondition _modulesObject;
        private QuestConditionsDrawer _modulesDrawer;
        
        private void OnEnable()
        {
            _modulesObject = (QuestCondition)target;
            _modulesDrawer = CreateInstance<QuestConditionsDrawer>();
            _modulesDrawer.SetModulesObject(_modulesObject, false); // Pass the Modules object to Modules Drawer
        }
        
        public override void OnInspectorGUI() => _modulesDrawer.Draw();
    }
}