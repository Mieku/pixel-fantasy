using System.IO;
using UnityEditor;
using UnityEngine;
using static InfinityPBR.Modules.GameModuleUtilities;

namespace InfinityPBR.Modules
{
    public class ObjectReferenceCreator : EditorWindow
    {
        [InitializeOnLoadMethod]
        private static void CreateObjectReferenceOnLoad() => EditorApplication.update += CheckForObjectReference;
        
        private static void CheckForObjectReference()
        {
            if (ObjectReference.Instance == null)
                CreateObjectReference();

            EditorApplication.update -= CheckForObjectReference;
        }

        public static void CreateObjectReference()
        {
            Debug.Log($"Creating new Object Reference File at {GameModulesGeneratedScriptPath}");
            if (!Directory.Exists(GameModulesGeneratedScriptPath))
                Directory.CreateDirectory(GameModulesGeneratedScriptPath);
                
            var objectReference = CreateInstance<ObjectReference>();
            AssetDatabase.CreateAsset(objectReference, $"{GameModulesGeneratedScriptPath}/ObjectReference.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            objectReference.PopulateMissingReferences();
        }
    }
}
