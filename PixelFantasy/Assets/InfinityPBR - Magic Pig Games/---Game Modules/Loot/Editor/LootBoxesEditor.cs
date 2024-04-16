using System;
using UnityEditor;

/*
 * This script will create the inspector extension to easily populate and manage the "Loot Box" objects.
 */

namespace InfinityPBR.Modules.Loot
{
    [CustomEditor(typeof(LootBoxes))]
    [CanEditMultipleObjects]
    [Serializable]
    public class LootBoxesEditor : InfinityEditor
    {
        
        private LootBoxes Script;
        
        protected virtual void OnEnable()
        {
            Script = target as LootBoxes;
            GetAllLootBoxes();
        }
        
        /// <summary>
        /// This is where the inspector is drawn
        /// </summary>
        public override void OnInspectorGUI()
        {
            if (!Script) return;

            
            MessageBox("The \"Boxes\" list will be populated for you, and should always include all LootBox" +
                       "scriptable objects in your project. Just make sure you inspect the object (as you are doing now) " +
                       "after creating new LootBox objects, so that the List is updated.", MessageType.Info);
            
            DrawDefaultInspector();
        }

        private void GetAllLootBoxes()
        {
            Script.boxes.Clear();
            string[] guids1 = AssetDatabase.FindAssets("t:LootBox", null);

            foreach (string guid1 in guids1)
            {
                LootBox newBox = AssetDatabase.LoadAssetAtPath<LootBox>(AssetDatabase.GUIDToAssetPath(guid1));
                Script.boxes.Add(newBox);
            }
            EditorUtility.SetDirty(Script); // Make sure changes can be saved
        }
    }
}
