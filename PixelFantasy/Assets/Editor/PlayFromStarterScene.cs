using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public class PlayFromStarterScene
{
    private const string PreviousSceneKey = "PlayFromStarterScene.PreviousScenePath";

    static PlayFromStarterScene()
    {
        // Hook into the play mode state change event
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            // Save the currently open scene path before entering play mode
            string currentScenePath = SceneManager.GetActiveScene().path;
            EditorPrefs.SetString(PreviousSceneKey, currentScenePath);

            // Open Scene 0 (assuming it's the first scene in the build settings)
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(SceneUtility.GetScenePathByBuildIndex(0));
            }
        }

        if (state == PlayModeStateChange.EnteredEditMode)
        {
            // Reopen the previous scene after play mode ends
            if (EditorPrefs.HasKey(PreviousSceneKey))
            {
                string previousScenePath = EditorPrefs.GetString(PreviousSceneKey);
                if (!string.IsNullOrEmpty(previousScenePath))
                {
                    EditorSceneManager.OpenScene(previousScenePath);
                }
                EditorPrefs.DeleteKey(PreviousSceneKey); // Clear the stored path after using it
            }
        }
    }
}