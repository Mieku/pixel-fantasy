using UnityEngine;
using UnityEditor;
using System.IO;

public class PrefabSaver : MonoBehaviour
{
    [MenuItem("Tools/Save Prefab Changes")]
    static void SavePrefabChanges()
    {
        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject == null)
        {
            Debug.LogError("No game object selected.");
            return;
        }

        GameObject prefabRoot = PrefabUtility.GetNearestPrefabInstanceRoot(selectedObject);
        if (prefabRoot == null)
        {
            Debug.LogError("Selected object is not part of a prefab instance.");
            return;
        }

        string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(prefabRoot);
        if (string.IsNullOrEmpty(prefabPath))
        {
            Debug.LogError("Could not determine the prefab path.");
            return;
        }

        PrefabUtility.SaveAsPrefabAssetAndConnect(prefabRoot, prefabPath, InteractionMode.UserAction);
        Debug.Log("Prefab changes saved to " + prefabPath);
    }
}