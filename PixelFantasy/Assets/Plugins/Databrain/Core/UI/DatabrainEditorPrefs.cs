#if UNITY_EDITOR
using UnityEditor;

namespace Databrain.UI
{
    public class DatabrainEditorPrefs : EditorWindow
    {
        [MenuItem("Tools/Databrain/Debug/Delete EditorPrefs")]
        static void DeleteAllPlayerPrefs()
        {
            EditorPrefs.DeleteAll();
        }
    }
}
#endif