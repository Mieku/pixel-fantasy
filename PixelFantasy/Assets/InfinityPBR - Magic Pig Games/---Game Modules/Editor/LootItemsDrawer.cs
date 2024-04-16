using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static InfinityPBR.Modules.GameModuleUtilities;

namespace InfinityPBR.Modules
{
    public class LootItemsDrawer : GameModulesDrawer
    {
        // -------------------------------
        // PROPERTIES AND VARIABLES -- UPDATE THESE FOR THIS TYPE
        // -------------------------------
        private LootItems _modulesObject;
        const string ThisTypePlural = "Loot Items";
        const string ThisType = "Loot Items";
        private string ClassNamePlural => "LootItems";
        private string ClassName => "LootItems";
        private string DocsURL => "https://infinitypbr.gitbook.io/infinity-pbr/game-modules/loot";
        private string DocsURLLabel => "Loot Items";
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
        public void SetModulesObject(LootItems modulesObject, bool drawnByWindow = true)
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
            
            CacheItemTypeArray();
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

            if (!drawnByGameModulesWindow)
            {
                LabelGrey("Loot Items");
                LabelBig(_modulesObject.objectName, 18, true);
            }

            BeginChangeCheck();
            
            BeginChangeCheck();
            
            // If this is a duplicate uid and the button is clicked to reset it, redo Initial Setup
            if (!drawnByGameModulesWindow && CheckUid(_modulesObject.Uid(), _modulesObject, _duplicateUid))
                _initialSetup = false;
            
           

            Space();
            ShowDescription();

            Space();
            ShowAddBox();

            Space();
            Undo.RecordObject(_modulesObject, "Undo Item Attributes Changes");
            ShowList();

            EndChangeCheck(_modulesObject);
            EditorUtility.SetDirty(_modulesObject);
        }

        private void ShowDescription()
        {
            StartRow();
            _modulesObject.displayDescription = OnOffButton(_modulesObject.displayDescription);
            Label($"Optional internal description {symbolInfo}", "Use this space to remind yourself how you plan " +
                                                                 "on using this Loot Items. It will be visible when you " +
                                                                 "are setting up Loot Boxes.", 300);
            EndRow();

            if (!_modulesObject.displayDescription) return;
            
            _modulesObject.description = TextArea(_modulesObject.description, 300, 50);
        }

        private void ShowAddBox()
        {
            BackgroundColor(Color.yellow);
            StartVerticalBox();
            StartRow();

            Label("Add Item", 70, true);
            var tempTypeIndex = _modulesObject.typeIndex;
            _modulesObject.typeIndex = Popup(_modulesObject.typeIndex, _modulesObject.availableTypes, 150);
            if (tempTypeIndex != _modulesObject.typeIndex) 
                CacheItemObjectArray();
            
            if (_modulesObject.availableItemNames.Length > 0)
            {
                _modulesObject.itemIndex = Popup(_modulesObject.itemIndex, _modulesObject.availableItemNames, 150);
                if (Button("Add", 70))
                {
                    _modulesObject.itemObjects.Add(GetObject<ItemObject>(_modulesObject.availableItems[_modulesObject.itemIndex]));
                    CacheItemObjectArray();
                }
                if (Button("Add All", 70))
                {
                    foreach(string uid in _modulesObject.availableItems)
                        _modulesObject.itemObjects.Add(GetObject<ItemObject>(uid));
                    CacheItemObjectArray();
                }
            }
            else
            {
                LabelGrey("There are no available Item Objects of this type.");
            }

            EndRow();
            EndVerticalBox();
            BackgroundColor(Color.white);
        }

        private void CacheItemObjectArray()
        {
            _modulesObject.availableItems = GameModuleObjectsOfType<ItemObject>(_modulesObject.availableTypes[_modulesObject.typeIndex])
                .Except(_modulesObject.itemObjects)
                .Select(x => x.Uid())
                .ToArray();
            
            _modulesObject.availableItemNames = GameModuleObjectsOfType<ItemObject>(_modulesObject.availableTypes[_modulesObject.typeIndex])
                .Except(_modulesObject.itemObjects)
                .Select(x => x.objectName)
                .ToArray();
        }

        private void CacheItemTypeArray()
        {
            _modulesObject.availableTypes = GameModuleObjectTypes<ItemObject>();
            if (_modulesObject.typeIndex >= _modulesObject.availableTypes.Length)
                _modulesObject.typeIndex = _modulesObject.availableTypes.Length - 1;
        }

        private void ShowList()
        {
            foreach (ItemObject itemObject in _modulesObject.itemObjects)
            {
                ShowItemObject(itemObject);
            }
        }

        private void ShowItemObject(ItemObject itemObject)
        {
            StartRow();

            BackgroundColor(Color.red);
            if (Button(symbolX, 25))
            {
                _modulesObject.itemObjects.RemoveAll(x => x == itemObject);
                CacheItemObjectArray();
                ExitGUI();
            }
            ResetColor();
            Label($"{itemObject.objectName}", 150);
            Object(itemObject, typeof(ItemObject), 150);
            BackgroundColor(Color.white);
            
            EndRow();
        }

        private bool _duplicateUid;
        private bool _initialSetup;
        private bool InitialSetup()
        {
            if (_modulesObject == null) return false;
            if (_initialSetup) return true;
            _initialSetup = true;
            _duplicateUid = ThisIsDuplicateUid(_modulesObject.Uid(), _modulesObject);
            CheckObjectTypes();
            
            if (_modulesObject.hasBeenSetup) return true;
            
            CacheItemTypeArray();
            CacheItemObjectArray();
            CheckForMissingItems();
            _modulesObject.hasBeenSetup = true;
            return true;
        }
        
        private void CheckObjectTypes()
        {
            foreach (var moduleObject in GameModuleObjects())
            {
                var tempName = moduleObject.objectName;
                var tempType = moduleObject.objectType;
                CheckName(_modulesObject);
                moduleObject.CheckObjectType();
                
                if (tempName == moduleObject.objectName && tempType == moduleObject.objectType) continue;
                
                UpdateProperties();
                EditorUtility.SetDirty(moduleObject);
            }
        }
        
        private void CheckForMissingItems()
        {
            _modulesObject.itemObjects.RemoveAll(x => x == null);
        }
    }
}