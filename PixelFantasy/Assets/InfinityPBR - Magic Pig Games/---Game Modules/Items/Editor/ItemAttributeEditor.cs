using System;
using UnityEditor;

namespace InfinityPBR.Modules
{
    [CustomEditor(typeof(ItemAttribute))]
    [CanEditMultipleObjects]
    [Serializable]
    public class ItemAttributeEditor : Editor
    {
        private ItemAttribute _modulesObject;
        private ItemAttributesDrawer _modulesDrawer;
        
        private void OnEnable()
        {
            _modulesObject = (ItemAttribute)target;
            _modulesDrawer = CreateInstance<ItemAttributesDrawer>();
            _modulesDrawer.SetModulesObject(_modulesObject, false); // Pass the Modules object to Modules Drawer
        }
        
        public override void OnInspectorGUI()
        {
            _modulesDrawer.Draw();
        }
    }
}