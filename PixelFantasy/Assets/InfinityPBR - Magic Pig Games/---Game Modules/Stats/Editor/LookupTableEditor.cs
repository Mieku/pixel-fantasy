using System;
using UnityEditor;

namespace InfinityPBR.Modules
{
    [CustomEditor(typeof(LookupTable))]
    [CanEditMultipleObjects]
    [Serializable]
    public class LookupTableEditor : Editor
    {
        
        private LookupTable _modulesObject;
        private LookupTablesDrawer _modulesDrawer;
        
        private void OnEnable()
        {
            _modulesObject = (LookupTable)target;
            _modulesDrawer = CreateInstance<LookupTablesDrawer>();
            _modulesDrawer.SetModulesObject(_modulesObject, false); // Pass the Modules object to Modules Drawer
        }
        
        public override void OnInspectorGUI() => _modulesDrawer.Draw();
    }
}
