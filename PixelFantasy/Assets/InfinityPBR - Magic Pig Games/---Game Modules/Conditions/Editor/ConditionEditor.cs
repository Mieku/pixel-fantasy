using System;
using UnityEditor;

namespace InfinityPBR.Modules
{
    [CustomEditor(typeof(Condition))]
    [CanEditMultipleObjects]
    [Serializable]
    public class ConditionEditor : Editor
    {
        
        private Condition _modulesObject;
        private ConditionsDrawer _modulesDrawer;
        
        private void OnEnable()
        {
            _modulesObject = (Condition)target;
            _modulesDrawer = CreateInstance<ConditionsDrawer>();
            _modulesDrawer.SetModulesObject(_modulesObject, false); // Pass the Modules object to Modules Drawer
            EditorPrefs.SetBool("statModificationLevelDrawer Force Cache", true);
        }
        
        public override void OnInspectorGUI() => _modulesDrawer.Draw();
    }
}
