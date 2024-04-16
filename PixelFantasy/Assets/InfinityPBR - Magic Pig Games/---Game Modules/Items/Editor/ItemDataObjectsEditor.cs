using System;
using UnityEditor;

/*
 * This script will create the inspector extension to easily populate and manage the "Item Data Objects" objects.
 */

namespace InfinityPBR.Modules
{
    [CustomEditor(typeof(ItemDataObjects))]
    [CanEditMultipleObjects]
    [Serializable]
    public class ItemDataObjectsEditor : InfinityEditor
    {
        
        private ItemDataObjects Script;
        
        protected virtual void OnEnable()
        {
            Script = target as ItemDataObjects;
            GetAll();
        }
        
        /// <summary>
        /// This is where the inspector is drawn
        /// </summary>
        public override void OnInspectorGUI()
        {
            if (!Script) return;

            
            MessageBox("The objects list will be populated for you, and should always include all object " +
                       "scriptable objects in your project. Just make sure you inspect the object (as you are doing now) " +
                       "after creating new object, so that the List is updated.", MessageType.Info);
            
            DrawDefaultInspector();
        }

        private void GetAll()
        {
            Script.itemObjects.Clear();
            Script.itemAttributes.Clear();
            
            string[] guids1 = AssetDatabase.FindAssets("t:ItemObject", null);
            foreach (string guid1 in guids1)
            {
                var newObject = AssetDatabase.LoadAssetAtPath<ItemObject>(AssetDatabase.GUIDToAssetPath(guid1));
                Script.itemObjects.Add(newObject);
            }
            
            string[] guids2 = AssetDatabase.FindAssets("t:ItemAttribute", null);
            foreach (string guid2 in guids2)
            {
                var newObject = AssetDatabase.LoadAssetAtPath<ItemAttribute>(AssetDatabase.GUIDToAssetPath(guid2));
                Script.itemAttributes.Add(newObject);
            }
            EditorUtility.SetDirty(Script); // Make sure changes can be saved
        }
    }
}