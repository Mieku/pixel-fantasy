using System;
using UnityEditor;
using UnityEngine;
using static InfinityPBR.Modules.GameModuleUtilities;

/*
 * Add this to any object that has IGetSaved classes. This is required for the Save and Load system to work. Each
 * class on this object which is IGetSaved must have a unique SaveableObjectId which does not change at runtime.
 */

namespace InfinityPBR.Modules
{
    public class Saveable : MonoBehaviour
    {
        // The SaveId is a unique identifier that marks this specific object as the owner of some data in the 
        // saved game files. It should be populated automatically with the OnValidate() method, but there is a
        // context menu option to populate it as well.
        public string SaveId => _saveId;
        [SerializeField] private string _saveId = string.Empty;

        // Inspector
        [HideInInspector] [SerializeField] public bool locked = true;

        [ContextMenu("Create Save Id")] // Right click on the variable to set, in case it's empty
        private void CreateSaveId()
        {
            if (!string.IsNullOrWhiteSpace(_saveId)) return;
            
            _saveId = Guid.NewGuid().ToString();
        }

        public void SetSaveId(string value) => _saveId = value;

        // Used to automatically set the _saveId value
        private void OnValidate()
        {
#if UNITY_EDITOR
            CreateSaveId();
            CheckSaveIdForDuplicates();
#endif
        }

        private void CheckSaveIdForDuplicates()
        {
            var numberOfIds = CountSaveIdsInScene(SaveId);
            if (numberOfIds > 1)
            {
                Debug.LogWarning($"[Editor Log Only] There are {numberOfIds} objects with the Save ID {SaveId}. This is not allowed, and often " +
                               "happens when a Saveable object is copied in the scene. Please delete the SaveId on the " +
                               "new object, and a new random string will be created for you.\n\n" +
                               "Do not delete the SaveId on the old object, or you may lose save game data!!\n\n" +
                               "If this happens at runtime, it may be that two scenes have copies of the same object, in a " +
                               "singleton pattern, in which case, this warning was triggered right before one was destroyed.", gameObject);
            }
        }

        // This saves an SaveableData object with all IGetSaved objects, using the Uid() value as the key, and the 
        // Saved state return object as the value.
        public string SaveState()
        {
            var saveableData = new SaveableData();

            // For each Saveable object, save the object data as a json string with Newtonsoft.Json
            foreach (var iGetSavedObject in IGetSavedObjects)
                saveableData.AddKeyValue(iGetSavedObject.SaveableObjectId()
                    , JsonUtility.ToJson(iGetSavedObject.SaveState(), SaveAndLoad.saveAndLoad.prettyPrint));

            return JsonUtility.ToJson(saveableData, SaveAndLoad.saveAndLoad.prettyPrint);
        }

        // This is the opposite of SaveState, and will decode a json encoded string, which contains a SaveableData
        // object. Then we will send the value to each IGetSaved class on this gameObject
        public void LoadState(string jsonEncodedSavedState)
        {
            var saveableData = JsonUtility.FromJson<SaveableData>(jsonEncodedSavedState);

            SaveAndLoad.saveAndLoad.Write($"Loading State on {gameObject.name}. There are {saveableData.keyValues.Count} saveable objects in the saveableData file");
            
            foreach (var iGetSavedObject in IGetSavedObjects)
            {
                if (saveableData.TryGetValue(iGetSavedObject.SaveableObjectId(), out var value))
                    iGetSavedObject.LoadState(value);
                else
                    SaveAndLoad.saveAndLoad.Write($"<color=#00ffff>WARNING: </color>Unable to find SaveableObjectId \"{iGetSavedObject.SaveableObjectId()}\" on object {gameObject.name}");
            }
        }

        // Grabs all the IGetSaved classes on this gameObject
        private ISaveable[] IGetSavedObjects => GetComponents<ISaveable>();
    }
}

