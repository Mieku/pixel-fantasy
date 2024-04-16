using System.Linq;
using UnityEditor;
using UnityEngine;
using static InfinityPBR.InfinityEditor;
using static InfinityPBR.Modules.GameModuleUtilities;

namespace InfinityPBR.Modules
{
    public class EditorWindowConditions : EditorWindow
    {
        private int typeIndex;
        private string selectedType;
        private int fieldWidth = 200;
        private EditorWindow thisEditorWindow;
        
        public DictionariesDrawer dictionariesDrawer;
        private StatModificationLevelDrawer statModificationLevelDrawer;
        private ConditionDrawer conditionDrawer;
        private Vector2 scrollPosition;

        private Condition[] cachedConditionsOfType;
        private string _cachedTypeName = "";

        private string[] menuBarOptions = {
            "Main Settings",
            "Dictionaries"
        };

        [MenuItem("Window/Game Modules/Conditions")]
        static void Init()
        {
            
            // Get existing open window or if none, make a new one:
            EditorWindowConditions window = (EditorWindowConditions)GetWindow(typeof(EditorWindowConditions));
            window.Show();
        }

        private void Awake()
        {
            thisEditorWindow = this;
            SetDrawers();
            titleContent = new GUIContent("Conditions");
            DoChecks();
            ResetCache();
            GameModuleObjectTypes<Condition>(true);
        }

        public void Cache()
        {
            cachedConditionsOfType = GameModuleObjectsOfType<Condition>(selectedType);
        }

        private Condition[] CacheConditionsOfType()
        {
            if (cachedConditionsOfType == null)
                cachedConditionsOfType = GameModuleObjectsOfType<Condition>(selectedType);
            if (cachedConditionsOfType.Length == 0)
                cachedConditionsOfType = GameModuleObjectsOfType<Condition>(selectedType);
            if (_cachedTypeName != selectedType)
                cachedConditionsOfType = GameModuleObjectsOfType<Condition>(selectedType);
            
            _cachedTypeName = selectedType;
            return cachedConditionsOfType;
        }

        private void DoChecks()
        {
            
        }

        private void OnEnable() => Undo.undoRedoPerformed += UndoWasDone;
        private void OnDisable() => Undo.undoRedoPerformed -= UndoWasDone;

        private void UndoWasDone()
        {
            
        }

        private void OnFocus()
        {
            AutoSelect();
            ResetCache();
            Cache();
        }

        private void ResetCache()
        {
            if (string.IsNullOrWhiteSpace(selectedType)) return;
            
            Debug.Log($"Resetting cache for {GameModuleObjectTypes<Condition>()[typeIndex]}");
            CacheConditionsOfType();
            if (dictionariesDrawer == null)
                return;
            
            dictionariesDrawer.ResetCache("Conditions", GameModuleObjectTypes<Condition>()[typeIndex]);
            
            CacheConditionsOfType();
            GameModuleObjectTypes<Condition>(true);
        }

        private void AutoSelect()
        {
            if (!HasKey("Conditions Selected")) return;
            
            
            selectedType = GetString("Conditions Type Selected");
            
            var objTypes = GameModuleObjectTypes<Condition>();
            
            for (var i = 0; i < objTypes.Length; i++)
            {
                if (objTypes[i] != selectedType) continue;

                typeIndex = i;
                break;
            }

            DeleteKey("Conditions Selected");
        }

        private void SetDrawers(){
            dictionariesDrawer = new DictionariesDrawerEditor(fieldWidth);
            statModificationLevelDrawer = new StatModificationLevelDrawerEditor(fieldWidth);
            conditionDrawer = new ConditionDrawerEditor(fieldWidth);
        }

        void OnGUI()
        {
            if (NoTypes()) return;

            CheckForNullDrawer();

            ShowHeader();
            Space();

            ShowButtons();
            Space();
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            ShowMainSettings();
            //ShowStats();
            ShowDictionaries();
            //ShowPropertyCode();
            
            EditorGUILayout.EndScrollView();

            SetAllDirty();
        }
        
        private void SetAllDirty()
        {
            foreach (Condition itemObject in GameModuleObjects<Condition>(true))
            {
                EditorUtility.SetDirty(itemObject);
            }
        }

        private void CheckForNullDrawer()
        {
            if (dictionariesDrawer == null)
                SetDrawers();
        }
        
        private int _menuBarIndex;
        private void ShowButtons() => _menuBarIndex = ToolbarMenuMain(menuBarOptions, _menuBarIndex);
        /*
        private void ShowButtons()
        {
            StartRow();
            
            bool tempShowMain = GetBool("Conditions Window Display Main Settings");
            bool tempShowStats = GetBool("Conditions Window Display Stats");
            bool tempShowDictionaries = GetBool("Conditions Window Display Dictionaries");
            bool tempShowPropertyCode = GetBool("Conditions Window Display Property Code");

            ShowButtonOption("Main Settings");
            ShowButtonOption("Stats");
            ShowButtonOption("Dictionaries");
            ShowButtonOption("Property Code");
            
            if (!tempShowMain && GetBool("Conditions Window Display Main Settings"))
            {
                SetBool("Conditions Window Display Stats", false);
                SetBool("Conditions Window Display Dictionaries", false);
                SetBool("Conditions Window Display Property Code", false);
            }
            if (!tempShowStats && GetBool("Conditions Window Display Stats"))
            {
                SetBool("Conditions Window Display Main Settings", false);
                SetBool("Conditions Window Display Dictionaries", false);
                SetBool("Conditions Window Display Property Code", false);
            }
            if (!tempShowDictionaries && GetBool("Conditions Window Display Dictionaries"))
            {
                SetBool("Conditions Window Display Stats", false);
                SetBool("Conditions Window Display Main Settings", false);
                SetBool("Conditions Window Display Property Code", false);
            }
            if (!tempShowPropertyCode && GetBool("Conditions Window Display Property Code"))
            {
                SetBool("Conditions Window Display Stats", false);
                SetBool("Conditions Window Display Dictionaries", false);
                SetBool("Conditions Window Display Main Settings", false);
            }

            BackgroundColor(Color.white);
            EndRow();
        }
        */

        private void ShowButtonOption(string label)
        {
            SetBool("Conditions Window Display " + label,
                ButtonCode(label, GetBool("Conditions Window Display " + label)));
        }
        
        private bool ButtonCode(string label, bool current)
        {
            BackgroundColor(current ? Color.green : Color.black);
            if (Button(label))
                current = !current;
            return current;
        }
        
        private void ShowPropertyCode()
        {
            if (!GetBool("Conditions Window Display Property Code"))
                return;
            LabelSized("Property Code");

            Label("The Property code script can be exported from the Property Code window, " +
                  "which is also in Window/Game Modules/Property Code.", false, true);

            if (Button("Open Property Code window"))
            {
                EditorWindowPropertyCode window = (EditorWindowPropertyCode)GetWindow(typeof(EditorWindowPropertyCode));
                window.Show();
            }
        }

        private void ShowDictionaries()
        {
            if (_menuBarIndex != 1) return;
            
            LabelSized("Dictionaries");

            Label("Create the structure for all Conditions dictionaries here. Select individual Conditions to set " +
                  "content values.", false, true);

            CacheConditionsOfType()[0].dictionaries.DrawStructure(dictionariesDrawer, "Conditions", selectedType);
            Space();
        }

        private void ShowHeader()
        {
            MessageBox("Manage the structure of all Conditions here. Select each Scriptable Object to set " +
                       "values for each attribute object.", MessageType.Info);
            LabelSized("Conditions - " + CacheConditionsOfType().Length + " " + selectedType + " objects");
            StartRow();
            if (typeIndex >= GameModuleObjectTypes<Condition>().Length)
                typeIndex = GameModuleObjectTypes<Condition>().Length - 1;
            typeIndex = Popup(typeIndex, GameModuleObjectTypes<Condition>(), 200);
            selectedType = GameModuleObjectTypes<Condition>()[typeIndex];
            Label("Select a Conditions Type", 200);
            EndRow();
         
            
                /*
                 * List the structure options here. Update the DRAW functions to hide the structural options or show based on a bool.
                 * If showing structure, should updating the content here update for all item objects?  Probably.s
                 */
        }

        private bool NoTypes()
        {
            if (!GameModuleObjectTypes<Condition>().Any())
            {
                LabelSized("Conditions");
                MessageBox("You have not created any Conditions yet. You can do this by navigating to where " +
                           "you would like the Scriptable Object to live, then right-click, and select " +
                           "Create/Game Modules/Create/Condition.\n\nNote: The name of the parent directory will be " +
                           "set as the \"Object Type\" automatically.", MessageType.Info);
                return true;
            }

            return false;
        }
        
        // ---------------------------------------------------------------------------------------------------------
        // MAIN SETTINGS
        // ---------------------------------------------------------------------------------------------------------
        
        private void ShowMainSettings()
        {
            if (_menuBarIndex != 0) return;
            Space();
            ShowMainSettingsBody();
            Space();
        }

        public void ShowMainSettingsBody()
        {
            Header1("Main Settings");

            Space();
            MessageBox("Manage additional options when viewing each Condition", MessageType.Info);
            Space();
            
            foreach (Condition condition in CacheConditionsOfType())
            {
                if (condition._showInEditor) Space();
                StartVerticalBox();
                StartRow();
                BackgroundColor(condition._showInEditor ? Color.green : Color.black);
                
                if (Button(condition.objectName, 200))
                    condition._showInEditor = !condition._showInEditor;
                
                BackgroundColor(Color.white);
                EndRow();
                if (condition._showInEditor) conditionDrawer.Draw(condition);
                EndVerticalBox();
            }
        }

        // ---------------------------------------------------------------------------------------------------------
        // STAT / MASTERY CODE
        // ---------------------------------------------------------------------------------------------------------
        
        private void ShowStats()
        {
            if (!GetBool("Conditions Window Display Stats"))
                return;
            LabelSized("Stats");

            foreach (Condition condition in cachedConditionsOfType)
            {
                if (ShowStatValues(condition)) continue;
                StartRow();
                Label(condition.objectName, 100, true);
                LabelGrey($"{condition.objectName} does not have any Stat effects");
                EndRow();
            }
        }
        
        private bool ShowStatValues(Condition condition)
        {
            if (condition.statModificationLevels[0].targets.Count == 0) return false;
            
            Label(condition.objectName, true);
            statModificationLevelDrawer.DrawSimple(condition.statModificationLevels[0], null);
            Space();
            return true;
        }
    }
}
