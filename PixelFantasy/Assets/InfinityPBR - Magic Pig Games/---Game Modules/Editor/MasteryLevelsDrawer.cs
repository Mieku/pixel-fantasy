using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static InfinityPBR.Modules.GameModuleUtilities;

namespace InfinityPBR.Modules
{
    public class MasteryLevelsDrawer : GameModulesDrawer
    {
        // -------------------------------
        // PROPERTIES AND VARIABLES -- UPDATE THESE FOR THIS TYPE
        // -------------------------------
        private MasteryLevels _modulesObject;
        const string ThisTypePlural = "Mastery Levels";
        const string ThisType = "Mastery Levels";
        private string ClassNamePlural => "MasteryLevels";
        private string ClassName => "MasteryLevels";
        private string DocsURL => "https://infinitypbr.gitbook.io/infinity-pbr/game-modules/stats-and-skills/mastery-levels";
        private string DocsURLLabel => "Mastery Levels";
        private string[] GameModuleObjectTypes(bool recompute = false) => GameModuleObjectTypes<MasteryLevels>(recompute);
        private MasteryLevels[] GameModuleObjects(bool recompute = false) => GameModuleObjects<MasteryLevels>(recompute);
        private MasteryLevels[] GameModuleObjectsOfType(string type, bool recompute = false) => GameModuleObjectsOfType<MasteryLevels>(type, recompute);
        // -------------------------------
        
        // -------------------------------
        // REQUIRED - NO UPDATE NEEDED
        // -------------------------------
        private Vector2 _scrollPosition;
        private int _fieldWidth;
        private DictionariesDrawer _dictionariesDrawer;
        private StatModificationLevelDrawer _statModificationLevelDrawer;
        private ItemAttributeDrawer _itemAttributeDrawer;
        private ConditionDrawer _conditionDrawer;
        private string[] _menuBarOptions;
        public bool drawnByGameModulesWindow = true;
        
        //private int _lootItemsIndex = 0;
        private List<LootItems> _cachedLootItems = new List<LootItems>();
        private List<string> _cachedLootItemsNames = new List<string>();
        private List<LootItems> _activeLootItems = new List<LootItems>();
        // -------------------------------

        // -------------------------------
        // METHODS -- UPDATE THESE FOR THIS TYPE
        // -------------------------------
        public void SetModulesObject(MasteryLevels modulesObject, bool drawnByWindow = true)
        {
            _modulesObject = modulesObject;
            _menuBarOptions = new[] { 
                "Levels Structure",
                "Dictionaries" 
            };
            _fieldWidth = 200;
            _dictionariesDrawer = new DictionariesDrawerEditor(_fieldWidth);
            _conditionDrawer = new ConditionDrawerEditor(_fieldWidth);
            _statModificationLevelDrawer = new StatModificationLevelDrawerEditor(_fieldWidth);
            drawnByGameModulesWindow = drawnByWindow;
        }
        
        protected void Cache()
        {
            GameModuleObjects<MasteryLevels>(true);
            GameModuleObjectNames<MasteryLevels>(true);
            GameModuleObjectTypes(true);
            GameModuleObjects(true);
            GameModuleObjectsOfType(_modulesObject.objectType, true);
            //CheckObjectTypes();
        }

        // -------------------------------

        // -------------------------------
        // REQUIRED METHODS -- NO UPDATES NEEDED
        // -------------------------------
        protected void DrawLinkToDocs()
        {
            if (drawnByGameModulesWindow) return;
            StartRow();

            BackgroundColor(Color.magenta);
            if (Button($"Manage {ClassNamePlural}"))
            {
                SetString("Game Modules Window Selected", ThisTypePlural);
                EditorWindow.GetWindow(typeof(EditorWindowGameModules)).Show();
            }
            ResetColor();
            LinkToDocs(DocsURL, $"{ThisTypePlural} Docs");
            LinkToDocs("https://www.youtube.com/watch?v=4KZlCPboA5c&list=PLCK7vP-GxBCm8l-feq-aF5_dWnFda7cvQ", "Tutorials");
            LinkToDocs("https://discord.com/invite/cmZY2tH", "Discord");
            EndRow();
            BlackLine();
        }
        
        private void ShowButtons() => _modulesObject.menubarIndex = ToolbarMenuMain(_menuBarOptions, _modulesObject.menubarIndex);
        
        public void Validations()
        {
            //CheckObjectTypes();
        }

        private void UpdateObjectName()
        {
            if (_modulesObject.objectName == _modulesObject.name) return;
            
            _modulesObject.objectName = _modulesObject.name;
            UpdateProperties();
        }
        // -------------------------------

        // -------------------------------
        // DRAW METHODS -- UPDATE THESE FOR THIS TYPE
        // -------------------------------
        public void Draw()
        {
            if (_modulesObject == null) return;

            if (!InitialSetup()) return;

            DrawLinkToDocs();
            
            BeginChangeCheck();
            
            // If this is a duplicate uid and the button is clicked to reset it, redo Initial Setup
            if (!drawnByGameModulesWindow && CheckUid(_modulesObject.Uid(), _modulesObject, _duplicateUid))
                _initialSetup = false;
            
           

            if (!drawnByGameModulesWindow)
            {
                LabelGrey("Mastery Levels - " + _modulesObject.objectType);
                LabelBig(_modulesObject.objectName, 18, true);
            }
            

            ResetCache();
            UpdateStatsAndSkills();
            
            //StartVerticalBox();
            ShowMasteryBody();
            //EndVerticalBox();
            
            EditorUtility.SetDirty(this);
        }
        
        private bool _duplicateUid;
        private bool _initialSetup;
        private bool InitialSetup()
        {
            if (_initialSetup) return true;
            _initialSetup = true;
            _duplicateUid = ThisIsDuplicateUid(_modulesObject.Uid(), _modulesObject);
            
            _initialSetup = true;
            CheckObjectTypes();
            
            return true;
        }
        
        private void CheckObjectTypes()
        {
            foreach (var obj in GameModuleObjects<MasteryLevels>(true))
            {
                var tempName = obj.objectName;
                var tempType = obj.objectType;
                CheckName(_modulesObject);
                obj.CheckObjectType();
                if (tempName != obj.objectName || tempType != obj.objectType)
                {
                    UpdateProperties();
                    EditorUtility.SetDirty(obj);
                }
            }
        }

        private void ResetCache()
        {
            if (!EditorPrefs.GetBool("Mastery Levels Manager Cache", false)) return;
            SetBool("Mastery Levels Manager Cache", false);

            foreach(var masteryLevel in GameModuleObjects<MasteryLevels>())
            {
#if UNITY_EDITOR
                var assetPath = AssetDatabase.GetAssetPath(masteryLevel);
                var directoryName = Path.GetDirectoryName(assetPath);
                var lastDirectory = Path.GetFileName(directoryName);
                masteryLevel.objectType = lastDirectory;
#endif
            }
            GameModuleObjectTypes<MasteryLevels>(true);
            GameModuleObjects<MasteryLevels>(true);
            CacheStatTypes();
            CacheMasteryLevelNames(true);
            _dictionariesDrawer?.ResetCache("Mastery Levels", CacheMasteryLevelNames()[SelectedMasteryLevelsIndex]);
            CheckObjectNamesAndTypes(GameModuleObjects());
        }
        
        private void CacheStatTypes() => GameModuleObjectTypes<Stat>(true);
        
        private int SelectedMasteryLevelsIndex
        {
            get => SessionState.GetInt("Selected Mastery Levels Index", 0);
            set => SessionState.SetInt("Selected Mastery Levels Index", value);
        }

        private void ShowDictionaries()
        {
            if (_modulesObject.menubarIndex != 1) return;
            StartVerticalBox();
            LabelSized("Dictionaries", 18, true);

            Label("Create the structure for all of the Mastery Levels dictionaries here. Select individual Mastery Levels to set " +
                  "content values.", false, true);

            _modulesObject.dictionaries.DrawStructure(_dictionariesDrawer, "Mastery Levels", masteryLevelsType);

            EndVerticalBox();
            Space();
        }

        private void ShowHeader()
        {
            var namesArray = CacheMasteryLevelNames();
            StartRow();
            ShowLinkButton(GameModuleObjects<MasteryLevels>()[SelectedMasteryLevelsIndex]);
            SelectedMasteryLevelsIndex = Popup(SelectedMasteryLevelsIndex, namesArray, 200);
            masteryLevelsType = namesArray[SelectedMasteryLevelsIndex];
            Label("Select a Mastery Levels Object", 200);
            EndRow();

            if (tempTypeIndex != SelectedMasteryLevelsIndex)
            {
                tempTypeIndex = SelectedMasteryLevelsIndex;
                
            }
        }

        private string[] masteryLevelNames;
        private string masteryLevelsType;
        private int tempTypeIndex = -1;
        

        private static void ShowLinkButton(ModulesScriptableObject moduleObject)
        {
            if (Button($"{symbolCircleArrow}", 25))
                EditorGUIUtility.PingObject(moduleObject);
            
        }
        private string[] CacheMasteryLevelNames(bool force = false)
        {
            if (masteryLevelNames == null
                || masteryLevelNames.Length == 0
                || force)
                masteryLevelNames = GameModuleObjectNames<MasteryLevels>(true);
            return masteryLevelNames;
        }
        
        private bool NoTypes()
        {
            if (!GameModuleObjectTypes<Stat>().Any())
            {
                LabelSized("Stats");
                MessageBox("You have not created any Stats yet. You can do this by navigating to where " +
                           "you would like the Scriptable Object to live, then right-click, and select " +
                           "Create/Game Modules/Create/Stats.\n\nNote: The name of the parent directory will be " +
                           "set as the \"Object Type\" automatically.", MessageType.Info);
                return true;
            }

            return false;
        }
        
        // ---------------------------------------------------------------------------------------------------------
        // MASTERY CODE
        // ---------------------------------------------------------------------------------------------------------

        public void ShowMasteryBody()
        {
            //if (!drawnByGameModulesWindow)
            //    LabelSized(_modulesObject.objectName, 18, true);
            Label("Create individual levels, and order them with the to down, <b>least proficient on top</b> and " +
                  "<b>most proficient on bottom</b>.", false, true, true);
            ShowLevels();
            ShowAddLevelButton();
            Space();
            BlackLine();
            Space();
            ShowMasteryManagemmentButtons();
            Space();
            ShowMasteryManagementDictionary();
        }

        private void ShowMasteryManagementDictionary()
        {
            for (var index = 0; index < _modulesObject.levels.Count; index++)
            {
                var level = _modulesObject.levels[index];
                if (_levelIndex != index) continue;

                ShowDictionariesHeader();
                Undo.RecordObject(_modulesObject, "Undo Value Changes");
                level.dictionaries.Draw(_dictionariesDrawer, "Mastery Level", level.name, _modulesObject.name);
            }
        }

        private int _levelIndex;
        private void ShowMasteryManagemmentButtons()
        {
            var levelNames = _modulesObject.levels.Select(x => x.name).ToArray();

            if (levelNames.Length == 0) return;
            _levelIndex = ToolbarMenu(levelNames, _levelIndex);
        }
        
        private void ShowDictionariesHeader()
        {
            MasteryLevel activeLevel = _modulesObject.levels[0];
            for (var index = 0; index < _modulesObject.levels.Count; index++)
            {
                var masteryLevel = _modulesObject.levels[index];
                if (_levelIndex == index)
                {
                    activeLevel = masteryLevel;
                }
            }

            StartRow();
            LabelSized("Dictionaries", 18, true);
            BackgroundColor(Color.yellow);
            if (Button("Copy", 75))
            {
                Debug.Log("Copy: " + activeLevel.name);
                Undo.RecordObject(_modulesObject, "Undo Copy Settings");
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
                    MasteryLevel copyItemObject = _modulesObject.levels
                        .FirstOrDefault(x => x.name == GetString("Mastery Levels Dictionaries"));
                    if (copyItemObject != null)
                    {
                        Undo.RecordObject(_modulesObject, "Undo Paste Settings");
                        activeLevel.dictionaries.ReplaceContentWith(copyItemObject.dictionaries);
                    }
                }
            }
            EndRow();
            BackgroundColor(Color.white);
        }

        private void ShowLevels()
        {
            for (var i = 0; i < _modulesObject.levels.Count; i++)
            {
                var masteryLevel = _modulesObject.levels[i];

                StartRow();
                
                ColorsIf(CanMoveUp(i, _modulesObject.levels.Count)
                    , Color.white
                    , Color.black
                    , Color.black
                    , Color.grey);
                if (Button("↑", 25))
                {
                    Undo.RecordObject(_modulesObject, "Undo Move");
                    MoveItem(_modulesObject.levels, i, -1);
                }
                ResetColor();

                ColorsIf(CanMoveDown(i, _modulesObject.levels.Count)
                    , Color.white
                    , Color.black
                    , Color.black
                    , Color.grey);
                if (Button("↓", 25))
                {
                    Undo.RecordObject(_modulesObject, "Undo Move");
                    MoveItem(_modulesObject.levels, i, 1);
                }
                ResetColor();
                
                Undo.RecordObject(_modulesObject, "Undo Text Change");
                masteryLevel.name = TextField(masteryLevel.name, 300);
                
                if (XButton())
                {
                    Undo.RecordObject(_modulesObject, "Undo Delete");
                    _modulesObject.levels.RemoveAt(i);
                    updateStatsAndSkills = true;
                    ExitGUI();
                }

                EndRow();
            }
        }

        private void ShowAddLevelButton()
        {
            StartRow();
            Label("Add new Mastery Level", 200, true);
            BackgroundColor(Color.yellow);
            SetString("Add Mastery Level Name", TextField(GetString("Add Mastery Level Name"), 200));
            if (Button("Add", 50))
            {
                if (String.IsNullOrWhiteSpace(GetString("Add Mastery Level Name")))
                {
                    EndRow();
                    Debug.LogWarning("Name can not be empty.");
                    return;
                }
                if (_modulesObject.levels.FirstOrDefault(x => x.name == GetString("Add Mastery Level Name")) != null)
                {
                    EndRow();
                    Debug.LogWarning("There is already a level named " + GetString("Add Mastery Level Name"));
                    return;
                }

                Undo.RecordObject(_modulesObject, "Undo Add Level");
                AddNewMasteryLevel();
            }
            BackgroundColor(Color.white);
            EndRow();
        }

        private bool updateStatsAndSkills;
        
        private void AddNewMasteryLevel()
        {
            MasteryLevel newMasteryLevel = new MasteryLevel();
            newMasteryLevel.name = GetString("Add Mastery Level Name");
            newMasteryLevel.dictionaries = _modulesObject.dictionaries.Clone();
            _modulesObject.levels.Add(newMasteryLevel);
            updateStatsAndSkills = true;
            
        }

        private void UpdateStatsAndSkills()
        {
            if (!updateStatsAndSkills) return;
            updateStatsAndSkills = false;
            
            foreach (var stat in GameModuleObjects<Stat>(true)
                .Where(x => x.masteryLevels == _modulesObject))
            {
                stat.CheckMasteryCount();
            }
        }
    }
}