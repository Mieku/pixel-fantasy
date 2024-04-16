#if UNITY_EDITOR
using InfinityPBR;
using UnityEditor;
using UnityEngine;
using InfinityPBR.Modules; // replace with your correct namespace

[CustomEditor(typeof(GameModuleRepository))]
public class GameModuleRepositoryEditor : InfinityEditor
{
    [InitializeOnLoadMethod]
    private static void EnsureScriptableObjectExists()
    {
        var repository = GameModuleRepository.Instance;

        if (repository != null) return;
        repository = ScriptableObject.CreateInstance<GameModuleRepository>();

        var folderPath = "Assets/Game Module Objects/Resources/";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets/Game Module Objects", "Resources");
        }

        AssetDatabase.CreateAsset(repository, folderPath + "GameModuleRepository.asset");
        AssetDatabase.SaveAssets();
        
        var newRepository = GameModuleRepository.Instance;
        if (newRepository != null) return;
        newRepository.PopulateAll();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var repository = (GameModuleRepository)target;
        // Custom inspector code goes here
    }
    
}
#endif