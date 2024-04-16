using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static InfinityPBR.Modules.GameModuleUtilities;

namespace InfinityPBR.Modules
{
    public class ItemObjectDrawer : GameModulesDrawer
    {
        // -------------------------------
        // PROPERTIES AND VARIABLES -- UPDATE THESE FOR THIS TYPE
        // -------------------------------
        private ItemObject _modulesObject;
        const string ThisTypePlural = "Item Objects";
        const string ThisType = "Item Object";
        private string ClassNamePlural => "ItemObjects";
        private string ClassName => "ItemObject";
        private string DocsURL => "https://infinitypbr.gitbook.io/infinity-pbr/game-modules/items";
        private string DocsURLLabel => "Item Objects";
        //private string[] GameModuleObjectTypes(bool recompute = false) => GameModuleObjectTypes<ItemObject>(recompute);
        private ItemObject[] GameModuleObjects(bool recompute = false) => GameModuleObjects<ItemObject>(recompute);
        private ItemObject[] GameModuleObjectsOfType(string type, bool recompute = false) => GameModuleObjectsOfType<ItemObject>(type, recompute);
        
        private string[] _gameModuleObjectTypes;
        private string[] CacheGameModuleObjectTypes() => _gameModuleObjectTypes = GameModuleObjectTypes<ItemObject>(true);
        
        private Dictionary<string, ItemObject[]> _gameModuleObjectsOfType;
        private Dictionary<string, ItemObject[]> CacheGameModuleObjectsOfType()
        {
            _gameModuleObjectsOfType = new Dictionary<string, ItemObject[]>();
            foreach(var type in _gameModuleObjectTypes)
                _gameModuleObjectsOfType.Add(type, GameModuleObjectsOfType<ItemObject>(type, true));

            return _gameModuleObjectsOfType;
        }
        
        // -------------------------------
        
        // -------------------------------
        // REQUIRED - NO UPDATE NEEDED
        // -------------------------------
        private Vector2 _scrollPosition;
        private int _fieldWidth;
        private DictionariesDrawer _dictionariesDrawer;
        private StatModificationLevelDrawer _statModificationLevelDrawer;
        private ItemAttributeDrawer _itemAttributeDrawer;
        private string[] _menuBarOptions;
        public bool drawnByGameModulesWindow = true;
        
        private int _lootItemsIndex;
        private List<LootItems> _cachedLootItems = new List<LootItems>();
        private List<string> _cachedLootItemsNames = new List<string>();
        private List<LootItems> _activeLootItems = new List<LootItems>();
        // -------------------------------

        // -------------------------------
        // METHODS -- UPDATE THESE FOR THIS TYPE
        // -------------------------------
        public void SetModulesObject(ItemObject modulesObject, bool drawnByWindow = true)
        {
            _modulesObject = modulesObject;
            _menuBarOptions = new[] { 
                "Item Attributes", 
                "Stats Effects",
                "Loot Items",
                "Inventory",
                "Dictionaries" 
            };
            _fieldWidth = 200;
            _dictionariesDrawer = new DictionariesDrawerEditor(_fieldWidth);
            _statModificationLevelDrawer = new StatModificationLevelDrawerEditor(_fieldWidth);
            drawnByGameModulesWindow = drawnByWindow;
        }
        
        protected void Cache()
        {
            GameModuleObjects<MasteryLevels>(true);
            GameModuleObjectNames<MasteryLevels>(true);
            
            CacheGameModuleObjectTypes();
            GameModuleObjects(true);
            CacheGameModuleObjectsOfType(); // Must be done after CacheGameModuleObjectTypes()

            SetBool("statModificationLevelDrawer Force Cache", true);
            
            _cachedLootItems.Clear();
            _activeLootItems.Clear();
            _cachedLootItemsNames.Clear();
            foreach (var lootItems in GameModuleObjects<LootItems>(true))
            {
                if (lootItems.itemObjects.Contains(_modulesObject))
                {
                    _activeLootItems.Add(lootItems);
                    continue;
                }

                _cachedLootItems.Add(lootItems);
                _cachedLootItemsNames.Add(lootItems.objectName);
            }
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
            foreach (var itemObject in GameModuleObjects())
            {
                var tempName = itemObject.objectName;
                var tempType = itemObject.objectType;
                CheckName(_modulesObject);
                itemObject.CheckObjectType();
                var newCount = GameModuleObjects().Count(x => x.objectName == itemObject.objectName);
                if (newCount > 1)
                    Debug.LogError("There are " + newCount + " Item Objects named " + itemObject.objectName);
                if (tempName != itemObject.objectName || tempType != itemObject.objectType)
                {
                    UpdateProperties();
                    EditorUtility.SetDirty(itemObject);
                }
            }
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
            
            //SetEditorWindowWidth(); July 2 2023 -- I don't think this is needed anymore
            
            DrawLinkToDocs();
            
            BeginChangeCheck();
            
            // If this is a duplicate uid and the button is clicked to reset it, redo Initial Setup
            if (!drawnByGameModulesWindow && CheckUid(_modulesObject.Uid(), _modulesObject, _duplicateUid))
                _initialSetup = false;
            
           

            if (!drawnByGameModulesWindow)
            {
                LabelGrey($"{ThisTypePlural} - " + _modulesObject.ObjectType);
                LabelBig(_modulesObject.ObjectName, 18, true);
            }
            

            Space();
            ShowMainOptions();
            
            Space();
            ShowButtons();
            
            
            Undo.RecordObject(_modulesObject, "Undo Item Attributes Changes");
            ShowItemAttributes();
            
            Undo.RecordObject(_modulesObject, "Undo Stats Changes");
            ShowStats();
            
            Undo.RecordObject(_modulesObject, "Undo Loot Items Changes");
            ShowLootItems();
            
            Undo.RecordObject(_modulesObject, "Undo Inventory Changes");
            ShowInventory();

            Undo.RecordObject(_modulesObject, "Undo Dictionaries Changes");
            ShowDictionaries();
            
            
            EndChangeCheck(_modulesObject);
            EditorUtility.SetDirty(_modulesObject);
        }
        
        private void ShowStats()
        {
            if (_modulesObject.menubarIndex != 1) return;
            Space();
            MessageBox("Item Objects can affect Stats. Often this is used when objects are \"equipped\" on a " +
                       "character. However, you can set this up however you'd like!\n\n" +
                       "IMPORTANT: Do not forget to set up the GetOtherLevels() method for your characters, so the " +
                       "system knows where to find the ItemObjectList that affects Stats! See the scripting docs linked " +
                       "from InfinityPBR.com for more details.");
            
            Space();
            StartVerticalBox();
            ShowStatsHeader();
            
            _statModificationLevelDrawer.Draw(_modulesObject.modificationLevel, null);
            
            EndVerticalBox();
        }
        
        private void ShowStatsHeader() => LabelBig("Stats",200, 18, true);
        
        public void ShowObjectsWindowButton()
        {
            if (!ButtonBig($"Manage {ThisTypePlural}", 300)) return;
            
            EditorPrefs.SetString("Game Modules Window Selected", ThisTypePlural);
            var window = (EditorWindowGameModules)EditorWindow.GetWindow(typeof(EditorWindowGameModules));
            window.Show();
        }
        
        private void ShowInventory()
        {
            if (_modulesObject.menubarIndex != 3) return;
            
            Space();
            MessageBox("[OPTIONAL] If you are using the Drag-and-Drop visual Inventory module, set the details " +
                       "here. The height and width refer to how many \"spots\" on the inventory grid this item will " +
                       "require.\n\n" +
                       "The Inventory Prefab " +
                       "is the object that is visible in the inventory panel, while the World Prefab is the prefab GameObject which " +
                       "is instantiated into the world when the item is dropped, or picked up from the world into the inventory.");
            
            Space();
            StartVerticalBox();
            ShowInventoryHeader();

            Space();
            StartRow();
            Label("Height", 50);
            _modulesObject.inventoryHeight = Int(_modulesObject.inventoryHeight, 50);
            Label("", 20);
            Label("Width", 50);
            _modulesObject.inventoryWidth = Int(_modulesObject.inventoryWidth, 50);
            EndRow();

            StartRow();
            Label("Inventory Prefab", 100);
            _modulesObject.prefabInventory = Object(_modulesObject.prefabInventory, typeof(GameObject), 240) as GameObject;
            EndRow();
            
            StartRow();
            Label("World Prefab", 100);
            _modulesObject.prefabWorld = Object(_modulesObject.prefabWorld, typeof(GameObject), 240) as GameObject;
            EndRow();

            EndVerticalBox();
        }
        
        private void ShowInventoryHeader()
        {
            LabelBig($"Inventory {symbolInfo}","If you are using the Inventory Module, you'll want to populate the values here, which are required to drive the " +
                                               "logic in the Inventory Module.",200, 18, true);
        }
        
        private void ShowLootItems()
        {
            if (_modulesObject.menubarIndex != 2) return;

            Space();
            MessageBox($"This shows which \"LootItems\" objects contain the {_modulesObject.objectName}. It may " +
                       "appear in as many LootItems as you would like. You can add this to other LootItems objects " +
                       "here, or when viewing the LootItems object itself.");
            
            Space();
            StartVerticalBox();
            ShowLootItemsHeader();

            Space();
            ShowAddLootItemsBox();

            Space();
            foreach (LootItems lootItems in _activeLootItems)
                ShowLootItemsObject(lootItems);

            EndVerticalBox();
        }
        
        private void ShowLootItemsObject(LootItems lootItems)
        {
            StartRow();
            BackgroundColor(Color.red);
            if (Button(symbolX, 25))
            {
                lootItems.itemObjects.RemoveAll(x => x == _modulesObject);
                Cache();
                ExitGUI();
            }
            BackgroundColor(Color.white);
            Label($"{lootItems.objectName} {symbolInfo}", $"[Internal description] {lootItems.description}", 200);
            Object(lootItems, typeof(LootItems), 150);
            EndRow();
        }
        
        private void ShowAddLootItemsBox()
        {
            if (_cachedLootItems.Count == 0) return;
            
            if (_lootItemsIndex >= _cachedLootItems.Count)
                _lootItemsIndex = _cachedLootItems.Count - 1;
            BackgroundColor(Color.yellow);
            StartRow();

            _lootItemsIndex = Popup(_lootItemsIndex, _cachedLootItemsNames.ToArray(), 300);
            if (Button("Add", 70))
            {
                _cachedLootItems[_lootItemsIndex].itemObjects.Add(_modulesObject);
                Cache();
                ExitGUI();
            }
            
            EndRow();
            BackgroundColor(Color.white);
        }
        
        private void ShowLootItemsHeader()
        {
            LabelBig($"Loot Items {symbolInfo}","See which Loot Items contain this Item Object, and remove this from them, or add this to other " +
                                                "Loot Items. You can also add Item Objects to Loot Items by visiting the objects themselves.",200, 18, true);
        }
        
        private void ShowItemAttributes()
        {
            if (_modulesObject.menubarIndex != 0) return;

            SetItemAttributeDrawer();

            _itemAttributeDrawer.Draw(_itemAttributeDrawer, _modulesObject);
        }
        
        public void SetItemAttributeDrawer()
        {
            if (_itemAttributeDrawer != null) return;
            
            _itemAttributeDrawer = new ItemAttributeDrawerEditor(_fieldWidth);
        }

        private void ShowMainOptions()
        {
            StartVerticalBox();
            _modulesObject.questItem = LeftCheck($"Quest Item {symbolInfo}",
                "The \"questItem\" bool signifies that this object should be handled differently than others. " +
                "Some other systems, such as the Inventory module, may have different logic for questItem objects than " +
                "others. As an example, the Inventory module will not let players \"drop\" a questItem into the world, " +
                "and GameItemObjectList defaults to ensuring questItem objects are not removed. These logic can be " +
                "turns off.",
                _modulesObject.questItem);
            EndVerticalBox();
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
            
            Cache();
            
            SetNameOnLevels();
            if (_modulesObject.hasBeenSetup) return true;

            if (String.IsNullOrEmpty(_modulesObject.objectType)) return false;
            if (_modulesObject.dictionaries.keyValues == null) return false;

            // Match settings with other objects of this type which all share the same settings (i.e. all turned on, otherwise, stay off)
            var itemObjects = _gameModuleObjectsOfType[_modulesObject.ObjectType];//(_modulesObject.objectType);
            MatchDictionary(itemObjects);
            MatchItemAttributes(itemObjects);
            //MatchStats(itemObjects); // July 2 2023 I don't think we do this

            _modulesObject.hasBeenSetup = true;
            return true;
        }
        
        /*
        private void MatchStats(ItemObject[] itemObjects)
        {
            var itemObjectsOfThisType = itemObjects.Length - 1;
            Debug.Log("TO Do: Match Stat and Skills");
        }
        */

        private void MatchItemAttributes(ItemObject[] itemObjects)
        {
            var itemObjectsOfThisType = itemObjects.Length - 1;
            foreach (var itemAttributeType in GameModuleObjectTypes<ItemAttribute>())
            {
                if (ItemObjectsOfTypeThatCanUseItemAttributeType(_modulesObject.objectType, itemAttributeType).Length != itemObjectsOfThisType)
                {
                    continue;
                }

                _modulesObject.SetCanUseAttributeType(itemAttributeType, true);

                foreach (var itemAttribute in GameModuleObjectsOfType<ItemAttribute>(itemAttributeType))
                {
                    if (ItemObjectsOfTypeThatCanUseItemAttribute(_modulesObject.objectType, itemAttribute).Length !=
                        itemObjectsOfThisType)
                        continue;

                    _modulesObject.ToggleCanUseItemAttribute(itemAttribute, true);
                }
            }
        }

        private void SetNameOnLevels() => _modulesObject.modificationLevel.ownerName = _modulesObject.objectName;
        
        
        
        
        
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
        
        private void ShowDictionaries()
        {
            if (_modulesObject.menubarIndex != 4) return;
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
            //Undo.RecordObject(Object, "Undo Value Changes");
            dictionaries.Draw(_dictionariesDrawer, "Stats", objectName, objectType);
        }
        
        private void MatchDictionary(ItemObject[] itemObjects)
        {
            var itemObjectsOfThisType = itemObjects.Length - 1;
            var keyValues = itemObjects.Except(itemObjects.Where(x => x == _modulesObject).ToArray()).SelectMany(x => x.dictionaries.keyValues).Select(x => x.key).Distinct().ToList();
            
            foreach (var keyValue in keyValues)
            {
                if (string.IsNullOrWhiteSpace(keyValue)) continue;
                if (ObjectsOfTypeWithDictionaryKeyValue<ItemObject>(_modulesObject.objectType, keyValue).Length != itemObjectsOfThisType)
                    continue;

                var firstObjectThatIsNotThis = itemObjects.First(x => x != _modulesObject);

                //var newKeyValue = firstObjectThatIsNotThis.GetKeyValue(keyValue).Clone();
                var newKeyValue = new KeyValue
                {
                    key = keyValue
                };
                _modulesObject.dictionaries.keyValues.Add(newKeyValue);
            }
        }
    }
}