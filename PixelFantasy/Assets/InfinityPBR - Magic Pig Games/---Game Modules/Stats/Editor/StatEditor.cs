using System;
using UnityEditor;

namespace InfinityPBR.Modules
{
    [CustomEditor(typeof(Stat))]
    [CanEditMultipleObjects]
    [Serializable]
    public class StatEditor : Editor
    {
        private Stat _modulesObject;
        private StatDrawer _modulesDrawer;
        
        private void OnEnable()
        {
            _modulesObject = (Stat)target;
            _modulesDrawer = CreateInstance<StatDrawer>();
            _modulesDrawer.SetModulesObject(_modulesObject, false); // Pass the Stat object to StatDrawer
            EditorPrefs.SetBool("statModificationLevelDrawer Force Cache", true);
        }
        
        public override void OnInspectorGUI() => _modulesDrawer.Draw();
    }
}
