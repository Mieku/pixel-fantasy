using UnityEngine;

public abstract class ScriptableObjectSingleton<T> : ScriptableObject where T : ScriptableObject
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance != null) return _instance;
            _instance = Resources.Load<T>(typeof(T).Name);

            if (_instance != null) return _instance;
            _instance = CreateInstance<T>();
            _instance.name = typeof(T).Name;

#if UNITY_EDITOR
            var folderPath = "Assets/Game Module Objects/Resources/";
            if (!System.IO.Directory.Exists(folderPath))
                System.IO.Directory.CreateDirectory(folderPath);

            UnityEditor.AssetDatabase.CreateAsset(_instance, folderPath + _instance.name + ".asset");
            UnityEditor.AssetDatabase.SaveAssets();
            // Run a virtual "Setup()" method
#endif
            return _instance;
        }
    }

    private void OnEnable() 
    {
        Setup();
    }

    protected virtual void Setup()
    {
        // Default implementation is empty
        // Inheritors can override this method to do their setup
    }
}