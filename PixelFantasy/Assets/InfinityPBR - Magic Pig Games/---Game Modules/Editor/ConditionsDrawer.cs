using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static InfinityPBR.Modules.GameModuleUtilities;

namespace InfinityPBR.Modules
{
    public class ConditionsDrawer : GameModulesDrawer
    {
        // -------------------------------
        // PROPERTIES AND VARIABLES -- UPDATE THESE FOR THIS TYPE
        // -------------------------------
        private Condition _modulesObject;
        const string ThisTypePlural = "Conditions";
        const string ThisType = "Condition";
        private string ClassNamePlural => "Conditions";
        private string ClassName => "Condition";
        private string DocsURL => "https://infinitypbr.gitbook.io/infinity-pbr/game-modules/conditions";
        private string DocsURLLabel => "Conditions";
        private string[] GameModuleObjectTypes(bool recompute = false) => GameModuleObjectTypes<Condition>(recompute);
        private Condition[] GameModuleObjects(bool recompute = false) => GameModuleObjects<Condition>(recompute);
        private Condition[] GameModuleObjectsOfType(string type, bool recompute = false) => GameModuleObjectsOfType<Condition>(type, recompute);
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
        public void SetModulesObject(Condition modulesObject, bool drawnByWindow = true)
        {
            _modulesObject = modulesObject;
            _menuBarOptions = new[] { 
                "Settings", 
                "Stats Effects",
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
            CheckObjectTypes();
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
            CheckObjectTypes();
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
                LabelGrey("Condition - " + _modulesObject.objectType);
                LabelBig(_modulesObject.objectName, 18, true);
            }

            
            ShowButtons();
            
            Undo.RecordObject(_modulesObject, "Undo Main Settings Changes");
            ShowMainSettings();
            
            Undo.RecordObject(_modulesObject, "Undo Mastery Levels Changes");
            ShowStats();
            
            Undo.RecordObject(_modulesObject, "Undo Dictionaries Changes");
            ShowDictionaries();
            
            EndChangeCheck(_modulesObject);
            if (this == null)
                return;
            EditorUtility.SetDirty(this);
        }
        
        

        public void ShowWindowButton()
        {
            if (ButtonBig("Manage Conditions", 300))
            {
                SetString("Conditions Selected", _modulesObject.Uid());
                SetString("Conditions Type Selected", _modulesObject.ObjectType);
                EditorWindowConditions window = (EditorWindowConditions)EditorWindow.GetWindow(typeof(EditorWindowConditions));
                window.Show();
            }
        }
        
        private void CheckObjectTypes()
        {
            foreach (var condition in GameModuleObjects<Condition>(true))
            {
                var tempName = condition.objectName;
                var tempType = condition.objectType;
                CheckName(_modulesObject);
                condition.CheckObjectType();
                if (tempName != condition.objectName || tempType != condition.objectType)
                {
                    UpdateProperties();
                    EditorUtility.SetDirty(condition);
                }
            }
        }

        private void ShowDictionaries()
        {
            if (_modulesObject.menubarIndex != 2) return;
            Space();
            StartVerticalBox();
            ShowDictionariesHeader();
            ShowDictionary(_modulesObject.dictionaries, _modulesObject.objectName, _modulesObject.objectType, _modulesObject);
            EndVerticalBox();
        }
         
        private void ShowDictionariesHeader()
        {
            
            StartRow();
            LabelBig("Dictionaries",200, 18, true);
            EndRow();
            BackgroundColor(Color.white);
        }

        public void ShowDictionary<T>(Dictionaries dictionaries, string objectName, string objectType, T Object) where T : ModulesScriptableObject
        {
            Undo.RecordObject(Object, "Undo Value Changes");
            dictionaries.Draw(_dictionariesDrawer, "Conditions", objectName, objectType);
        }

        private void ShowStats()
        {
            EnsureOneModificationLevel();

            if (_modulesObject.menubarIndex != 1) return;
            Space();

            if (_modulesObject.Instant)
            {
                MessageBox($"The condition {_modulesObject.objectName} is currently set to Instant. Values set " +
                           "here would be instantly removed.", MessageType.Info);
                return;
            }

            StartVerticalBox();
            LabelBig($"{_modulesObject.objectName} - Stats", 18, true);
            ShowMastery();
            EndVerticalBox();
        }

        private void EnsureOneModificationLevel() => _modulesObject.EnsureOneModificationLevel();

        public void ShowMastery() => _statModificationLevelDrawer.Draw(_modulesObject.statModificationLevels[0], null);

        private string KeySettings => "ConditionSettings";
        private string KeyConditionTime => "ConditionTime";
        private string KeyEffectsOnPoints => "ConditionEffectsOnPoints";
        
        private void ShowMainSettings()
        {
            if (_modulesObject.menubarIndex != 0) return;
            Space();

            Undo.RecordObject(_modulesObject, "Undo Condition Changes");
            _conditionDrawer.Draw(_modulesObject);
        }

        private void ShowMainSettingsHeader() => LabelBig("Main Settings", 18, true);

        private bool _duplicateUid;
        private bool _initialSetup;
        private bool InitialSetup()
        {
            if (_modulesObject == null) return false;
            if (_initialSetup) return true;
            _initialSetup = true;
            _duplicateUid = ThisIsDuplicateUid(_modulesObject.Uid(), _modulesObject);
            CheckObjectTypes();
            _modulesObject.EnsureOneModificationLevel();
            SetNameOnLevels();
            
            if (_modulesObject.hasBeenSetup) return true;

            if (String.IsNullOrEmpty(_modulesObject.objectType)) return false;
            if (_modulesObject.dictionaries.keyValues == null) return false;

            // Match settings with other objects of this type which all share the same settings (i.e. all turned on, otherwise, stay off)
            var otherObjects = CacheConditionsArray(_modulesObject.objectType);
            MatchDictionary(otherObjects);

            _modulesObject.hasBeenSetup = true;
            return true;
        }
        
        private void SetNameOnLevels()
        {
            foreach (var level in _modulesObject.statModificationLevels)
            {
                if (level == null) continue;
                level.ownerName = _modulesObject.objectName;
            }
        }
        
        private string _conditionsArrayType;
        private Condition[] cachedConditionsArray;
        
        private Condition[] CacheConditionsArray(string statsType)
        {
            if (cachedConditionsArray == null)
                cachedConditionsArray = GameModuleObjectsOfType<Condition>(statsType);
            if (cachedConditionsArray.Length == 0)
                cachedConditionsArray = GameModuleObjectsOfType<Condition>(statsType);
            if (_conditionsArrayType != statsType)
            {
                cachedConditionsArray = GameModuleObjectsOfType<Condition>(statsType);
                _conditionsArrayType = statsType;
            }

            return cachedConditionsArray;
        }
        
        private void MatchDictionary(Condition[] otherObjects)
        {
            var otherObjectsOfThisType = otherObjects.Length - 1;
            var keyValues = otherObjects
                .Except(otherObjects
                    .Where(x => x == _modulesObject)
                    .ToArray())
                .SelectMany(x => x.dictionaries.keyValues)
                .Select(x => x.key)
                .Distinct()
                .ToList();
            
            foreach (var keyValue in keyValues)
            {
                if (string.IsNullOrWhiteSpace(keyValue)) continue;
                if (ObjectsOfTypeWithDictionaryKeyValue<Stat>(_modulesObject.objectType, keyValue).Length != otherObjectsOfThisType)
                    continue;

                //var newKeyValue = otherObjects[0].GetKeyValue(keyValue)?.Clone();
                var newKeyValue = new KeyValue
                {
                    key = keyValue
                };
                if (newKeyValue != null)
                    _modulesObject.dictionaries.keyValues.Add(newKeyValue);
            }
        }
    }
}