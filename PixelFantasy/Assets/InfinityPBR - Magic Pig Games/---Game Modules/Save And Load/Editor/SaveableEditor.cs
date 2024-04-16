using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static InfinityPBR.Modules.GameModuleUtilities;
using static InfinityPBR.InfinityEditor;

namespace InfinityPBR.Modules
{
    [CustomEditor(typeof(Saveable))]
    [CanEditMultipleObjects]
    [Serializable]
    public class SaveableEditor : Editor
    {
        private Saveable Script;
        
        private List<ISaveable> _cachedSaveableComponents;
        
        private void OnEnable()
        {
            Script = (Saveable)target;
            _cachedSaveableComponents = null;
        }

        public override void OnInspectorGUI()
        {
            Header();
            DuplicateWarning();
            SaveIdDisplay();
            ShowLock();
            DisplaySaveableComponents();
        }

        private void DuplicateWarning()
        {
            var numberOfIds = CountSaveIdsInScene(Script.SaveId);
            if (numberOfIds > 1)
            {
                Space();
                Colors(Color.black, Color.red);
                StartVerticalBox();
                Label($"WARNING: There are {numberOfIds} Saveable objects in the scene with the same Save ID! This will " +
                      "cause problems when loading data, and often occurs when an object is copied in the Hierarchy, or " +
                      "brought in as a prefab.\n\nPlease update the Save ID on the NEW object. Do not change it on the " +
                      "original object.", false, true);
                EndVerticalBox();
                ResetColor();
                Space();
            }
        }

        private void ShowLock()
        {
            Space();
            ColorsIf(Script.locked, Color.black, Color.green, Color.white, Color.white);
            if (Button(Script.locked ? "Unlock" : "Lock", 200, 40))
                Script.locked = !Script.locked;
            ResetColor();
        }

        private void SaveIdDisplay()
        {
            var currentId = Script.SaveId;
            StartRow();
            Label($"Save ID: {symbolInfo}", "The Save ID is automatically generated but you can change it. Do not change " +
                                            "the value if your project already has save files, or this object will not load " +
                                            "next time those files are loaded.", 100);
            if (Script.locked)
            {
                Label($"{Script.SaveId}");
                EndRow();
                LabelGrey("Save ID is locked to prevent accidental changes.");
                return;
            }
            var newId = DelayedText($"{Script.SaveId}", 250);

            BackgroundColor(Color.black);
            if (Button("New", 40))
                newId = Guid.NewGuid().ToString();
            ResetColor();

            EndRow();

            if (string.IsNullOrWhiteSpace(newId))
                newId = currentId;

            if (currentId != newId)
                PromptUserForConfirmation(currentId, newId);
        }

        private void PromptUserForConfirmation(string currentId, string newId)
        {
            if (Dialog("Warning: About to Overwrite Save ID"
                    , "Do you really want to change the Save ID? This will " +
                      "break save files that already exist."))
            {
                EditorUtility.SetDirty(Script);
                Script.SetSaveId(newId);
                Script.locked = true;
                return;
            }
            
            Script.SetSaveId(currentId);
        }

        private void Header()
        {
            MessageBox("Important: Each Saveable object must have a unique Save ID in the scene. This value " +
                       "should never be changed once you start saving data, as it is how the system knows where data " +
                       "from the save file should be loaded.", MessageType.Warning);
            Space();
        }
        
        private void DisplaySaveableComponents()
        {
            var saveableComponents = GetSaveableComponents();

            if (saveableComponents.Count <= 0)
            {
                MessageBox($"There are no ISaveable components on this object. This object will not be saved!",
                    MessageType.Error);
                return;
            }

            Space();
            Header3("ISaveable Components");
        
            
            foreach(var component in saveableComponents)
            {
                string objectId;
                try
                {
                    objectId = string.IsNullOrWhiteSpace(component.SaveableObjectId()) 
                        ? "<color=#ff9999><b>Saveable Object ID is null, empty, or whitespace!</b></color>" 
                        : $"<color=#777777>SaveableObjectId: <color=#99ffff>{component.SaveableObjectId()}</color></color>";
                }
                catch (System.NotImplementedException)
                {
                    objectId = "<color=#ff9999><b>Saveable Object ID method not implemented!</b></color>";
                }
                
                Label($"{component.GetType().Name.SpacesBeforeCapitals()} {objectId}", false, true, true);
            }
        }
        
        private List<ISaveable> GetSaveableComponents() =>
            _cachedSaveableComponents ??= Script.gameObject.GetComponents<ISaveable>().ToList();
    }

}
