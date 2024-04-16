using System;
using UnityEditor;

namespace InfinityPBR.Modules
{
    [CustomEditor(typeof(MasteryLevels))]
    [CanEditMultipleObjects]
    [Serializable]
    public class MasteryLevelsEditor : Editor
    {
        
        private MasteryLevels _modulesObject;
        private MasteryLevelsDrawer _modulesDrawer;
        
        private void OnEnable()
        {
            _modulesObject = (MasteryLevels)target;
            _modulesDrawer = CreateInstance<MasteryLevelsDrawer>();
            _modulesDrawer.SetModulesObject(_modulesObject, false); // Pass the Modules object to Modules Drawer
        }
        
        public override void OnInspectorGUI() => _modulesDrawer.Draw();
        
        /*
        private MasteryLevels ThisObject;
        
        private Vector2 _scrollPosition;

        private int fieldWidth = 200;
        private DictionariesDrawer dictionariesDrawer;

        protected override void Setup()
        {
            ThisObject = (MasteryLevels) target;
            dictionariesDrawer = new DictionariesDrawerEditor(fieldWidth);
        }

        protected override void Draw()
        {
            LabelGrey("Mastery Levels");
            Validations();
            
            LabelBig(ThisObject.objectName, 18, true);

            if (ThisObject.levels.Count == 0)
            {
                Label("You have not yet added any mastery levels. Click \"Manage Mastery Levels\" above to add " +
                      "levels to this Mastery Levels object.", false, true);
                return;
            }

            // October 11 2021 -- Rather than duplicating only a small amount of functionality, I'm moving all of it to 
            // the main window, which is just as easier, if not easier, to manage the data.
            Label("Manage this using the \"Manage Mastery Levels\" window by clicking above, and then selecting this " +
                  "Mastery Levels object from the drop down.", false, true);
            return;
            
        }

        protected override void Header()
        {
            Validations();
            ShowMasteryLevelsWindowButton();
            ShowHeader(ThisObject, ThisObject.Uid());
        }
        
        public void ShowMasteryLevelsWindowButton()
        {
            if (!ButtonBig("Manage Mastery Levels", 300, 24, 18, false)) return;
            
            EditorPrefs.SetString("Game Modules Window Selected", "Mastery Levels");
            var window = (EditorWindowGameModules)EditorWindow.GetWindow(typeof(EditorWindowGameModules));
            window.Show();
        }
        
        private void Footer()
        {
            ShowFooter(Script);
            if (GetBool("Show full inspector " + Script.name))
                DrawDefaultInspector();
        }
        
        private void ShowButtons()
        {
            StartRow();

            for (int i = 0; i < ThisObject.levels.Count; i++)
            {
                MasteryLevel masteryLevel = ThisObject.levels[i];
                bool tempShowThis = masteryLevel.showThis;
                masteryLevel.showThis = ButtonCode(masteryLevel.name, masteryLevel.showThis);
                
                if (!masteryLevel.showThis && tempShowThis)
                    masteryLevel.showThis = true;
                
                if (masteryLevel.showThis && tempShowThis != masteryLevel.showThis)
                {
                    GUIUtility.keyboardControl = 0;
                    foreach (MasteryLevel ml in ThisObject.levels
                        .Except(ThisObject.levels
                            .Where(x => x == masteryLevel)).ToArray())
                    {
                        ml.showThis = false;
                    }
                }
            }

            BackgroundColor(Color.white);
            EndRow();
        }
        
        private void Validations()
        {
            CheckName(ThisObject);
            CheckObjectTypes();
        }
        
        private void CheckObjectTypes()
        {
            foreach (var masteryLevelsObject in GameModuleObjects<MasteryLevels>())
            {
                var tempName = masteryLevelsObject.objectName;
                CheckName(masteryLevelsObject);

                if (tempName == masteryLevelsObject.objectName) return;
                UpdateProperties();
            }
        }

        private void ShowDictionaries()
        {
            if (ThisObject.levels.All(x => x.showThis == false)) return;

            MasteryLevel thisMasteryLevel = ThisObject.levels.First(x => x.showThis == true);
            Dictionaries thisDictionaries = thisMasteryLevel.dictionaries;
            
            Space();
            StartVerticalBox();
            ShowDictionariesHeader();
            ShowDictionary(thisDictionaries, thisMasteryLevel.name, ThisObject.objectName, ThisObject);
            EndVerticalBox();
        }
         
        private void ShowDictionariesHeader()
        {
            MasteryLevel activeLevel = ThisObject.levels[0];
            foreach (MasteryLevel masteryLevel in ThisObject.levels)
            {
                if (masteryLevel.showThis)
                {
                    activeLevel = masteryLevel;
                }
            }
            
            StartRow();
            LabelBig("Dictionaries", 18, true);
            BackgroundColor(Color.yellow);
            if (Button("Copy", 75))
            {
               Undo.RecordObject(ThisObject, "Undo Copy Settings");
                SetString("Mastery Levels Dictionaries", activeLevel.name);
            }

            // REPLACE CONTENT
            if (HasKey("Mastery Levels Dictionaries"))
            {
                BackgroundColor(Color.yellow);
                if (Button("Replace all values with " + GetString("Mastery Levels Dictionaries") + " Content",
                    "This will replace the current content with the copied content.",
                    300))
                {
                    MasteryLevel copyItemObject = ThisObject.levels
                        .FirstOrDefault(x => x.name == GetString("Mastery Levels Dictionaries"));
                    if (copyItemObject != null)
                    {
                        Undo.RecordObject(ThisObject, "Undo Paste Settings");
                        activeLevel.dictionaries.ReplaceContentWith(copyItemObject.dictionaries);
                    }
                }
            }
            EndRow();
            BackgroundColor(Color.white);
        }

        public void ShowDictionary<T>(Dictionaries dictionaries, string masteryLevelName, string objectName, T Object) where T : ModulesScriptableObject
        {
            Undo.RecordObject(Object, "Undo Value Changes");
            dictionaries.Draw(dictionariesDrawer, "Mastery Levels", masteryLevelName, objectName);
        }
*/
    }
}
