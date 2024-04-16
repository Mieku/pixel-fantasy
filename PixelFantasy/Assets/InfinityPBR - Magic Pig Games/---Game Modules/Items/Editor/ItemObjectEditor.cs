using System;
using UnityEditor;

namespace InfinityPBR.Modules
{
    [CustomEditor(typeof(ItemObject))]
    [CanEditMultipleObjects]
    [Serializable]
    public class ItemObjectEditor : Editor
    {
        private ItemObject _modulesObject;
        private ItemObjectDrawer _modulesDrawer;
        
        private void OnEnable()
        {
            _modulesObject = (ItemObject)target;
            _modulesDrawer = CreateInstance<ItemObjectDrawer>();
            _modulesDrawer.SetModulesObject(_modulesObject, false); // Pass the Modules object to Modules Drawer
        }
        
        public override void OnInspectorGUI() => _modulesDrawer.Draw();
        /*
        private ItemObject _modulesObject;

        private ItemAttributeDrawer _itemAttributeDrawer;

        private float _width;
        private int _columnWidth = 190;
        private Vector2 _scrollPosition;
        
        private int _fieldWidth = 200;
        private DictionariesDrawer _dictionariesDrawer;
        private StatModificationLevelDrawer _statModificationLevelDrawer;

        private int _lootItemsIndex = 0;
        private List<LootItems> _cachedLootItems = new List<LootItems>();
        private List<string> _cachedLootItemsNames = new List<string>();
        private List<LootItems> _activeLootItems = new List<LootItems>();
        
        private string[] menuBarOptions = new[] {
            $"Item Attributes", 
            $"Stats Effects",
            $"Loot Items",
            $"Inventory",
            $"Dictionaries"
        };
        
        protected override void DrawLinkToDocs() => LinkToDocs("https://infinitypbr.gitbook.io/infinity-pbr/game-modules/items");
        
        private void ShowButtons() => _modulesObject.menubarIndex =  ToolbarMenuMain(menuBarOptions, _modulesObject.menubarIndex);

        protected override void Cache()
        {
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
        
        public void SetItemAttributeDrawer()
        {
            if (_itemAttributeDrawer != null) return;
            
            _itemAttributeDrawer = new ItemAttributeDrawerEditor(_fieldWidth);
        }
        
        protected override void Setup()
        {
            _modulesObject = (ItemObject) target;
            _dictionariesDrawer = new DictionariesDrawerEditor(_fieldWidth);
            _statModificationLevelDrawer = new StatModificationLevelDrawerEditor(_fieldWidth);
            Cache();
        }

        protected override void Header()
        {
            BeginChangeCheck();
            Validations();
            ShowObjectsWindowButton(_modulesObject);
            ShowHeader(_modulesObject, _modulesObject.Uid());
            EndChangeCheck();
        }
        
        protected override void OnProjectWindowItemOnGUI(string guid, Rect selectionRect) => DoOnProjectWindowItemOnGUI(guid, selectionRect, _modulesObject);

        protected override void Draw()
        {
            if (!InitialSetup()) return;
            
            SetEditorWindowWidth();

            LabelGrey("Item Object - " + Script.objectType);
            LabelBig(Script.objectName, 18, true);

            BeginChangeCheck();

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
            
            Footer();
            EndChangeCheck();
            EditorUtility.SetDirty(_modulesObject);
        }

        private void ShowMainOptions()
        {
            StartVerticalBox();
            _modulesObject.questItem = LeftCheck($"Quest Item {symbolInfo}",
                $"The \"questItem\" bool signifies that this object should be handled differently than others. " +
                $"Some other systems, such as the Inventory module, may have different logic for questItem objects than " +
                $"others. As an example, the Inventory module will not let players \"drop\" a questItem into the world, " +
                $"and GameItemObjectList defaults to ensuring questItem objects are not removed. These logic can be " +
                $"turns off.",
                _modulesObject.questItem);
            EndVerticalBox();
        }

        private void SetEditorWindowWidth()
        {
            var thisWidth = EditorGUILayout.GetControlRect().width;
            if (thisWidth < 2f) return;
            _width = thisWidth;
        }

        private void ShowItemAttributes()
        {
            if (_modulesObject.menubarIndex != 0) return;

            SetItemAttributeDrawer();

            _itemAttributeDrawer.Draw(_itemAttributeDrawer, _modulesObject);
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
        
        private void ShowStats()
        {
            if (_modulesObject.menubarIndex != 1) return;
            Space();
            MessageBox($"Item Objects can affect Stats. Often this is used when objects are \"equipped\" on a " +
                       $"character. However, you can set this up however you'd like!\n\n" +
                       $"IMPORTANT: Do not forget to set up the GetOtherLevels() method for your characters, so the " +
                       $"system knows where to find the ItemObjectList that affects Stats! See the scripting docs linked " +
                       $"from InfinityPBR.com for more details.");
            
            Space();
            StartVerticalBox();
            ShowStatsHeader();
            
            _statModificationLevelDrawer.Draw(_modulesObject.modificationLevel, null);
            
            EndVerticalBox();
        }
        
        private void ShowInventory()
        {
            if (_modulesObject.menubarIndex != 3) return;
            
            Space();
            MessageBox($"[OPTIONAL] If you are using the Drag-and-Drop visual Inventory module, set the details " +
                       "here. The height and width refer to how many \"spots\" on the inventory grid this item will " +
                       "require.\n\n" +
                       "The Inventory Prefab " +
                       $"is the object that is visible in the inventory panel, while the World Prefab is the prefab GameObject which " +
                       $"is instantiated into the world when the item is dropped, or picked up from the world into the inventory.");
            
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
            _modulesObject.prefabInventory = Object(_modulesObject.prefabInventory, typeof(GameObject), 240, false) as GameObject;
            EndRow();
            
            StartRow();
            Label("World Prefab", 100);
            _modulesObject.prefabWorld = Object(_modulesObject.prefabWorld, typeof(GameObject), 240, false) as GameObject;
            EndRow();

            EndVerticalBox();
        }
        
        private void ShowLootItems()
        {
            if (_modulesObject.menubarIndex != 2) return;

            Space();
            MessageBox($"This shows which \"LootItems\" objects contain the {_modulesObject.objectName}. It may " +
                       $"appear in as many LootItems as you would like. You can add this to other LootItems objects " +
                       $"here, or when viewing the LootItems object itself.");
            
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
            Object(lootItems, typeof(LootItems), 150, false);
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
            if (Button($"Add", 70))
            {
                _cachedLootItems[_lootItemsIndex].itemObjects.Add(_modulesObject);
                Cache();
                ExitGUI();
            }
            
            EndRow();
            BackgroundColor(Color.white);
        }

        private void ShowDictionariesHeader()
        {
            StartRow();
            LabelBig("Dictionaries",200, 18, true);
            BackgroundColor(Color.yellow);
            if (Button("Copy", 75))
            {
                Undo.RecordObject(_modulesObject, "Undo Copy Settings");
                SetString("Item Dictionaries", _modulesObject.objectName);
            }

            // REPLACE CONTENT
            if (HasKey("Item Dictionaries"))
            {
                BackgroundColor(Color.yellow);
                if (Button("Replace all values with " + GetString("Item Dictionaries") + " Content",
                    "This will replace the current content with the copied content.",
                    300))
                {
                    var copyItemObject = GameModuleObjects<ItemObject>().ToList()
                        .FirstOrDefault(x => x.objectName == GetString("Item Dictionaries"));
                    if (copyItemObject != null)
                    {
                        Undo.RecordObject(_modulesObject, "Undo Paste Settings");
                        _modulesObject.dictionaries.ReplaceContentWith(copyItemObject.dictionaries);
                    }
                }
            }
            EndRow();
            BackgroundColor(Color.white);
        }
        
        private void ShowStatsHeader() => LabelBig("Stats",200, 18, true);
        
        private void ShowLootItemsHeader()
        {
            LabelBig($"Loot Items {symbolInfo}",$"See which Loot Items contain this Item Object, and remove this from them, or add this to other " +
                                                $"Loot Items. You can also add Item Objects to Loot Items by visiting the objects themselves.",200, 18, true);
        }
        
        private void ShowInventoryHeader()
        {
            LabelBig($"Inventory {symbolInfo}",$"If you are using the Inventory Module, you'll want to populate the values here, which are required to drive the " +
                                               $"logic in the Inventory Module.",200, 18, true);
        }

        public void ShowDictionary<T>(Dictionaries dictionaries, string objectName, string objectType, T Object) where T : ModulesScriptableObject
        {
            Undo.RecordObject(Object, "Undo Value Changes");
            dictionaries.Draw(_dictionariesDrawer, "Item Object", objectName, objectType);
        }

        private void Footer()
        {
            ShowFooter(Script);
            if (GetBool("Show full inspector " + Script.name))
                DrawDefaultInspector();
        }

        private void Validations()
        {
            foreach (var itemObject in GameModuleObjects<ItemObject>())
            {
                var tempName = itemObject.objectName;
                var tempType = itemObject.objectType;
                CheckName(_modulesObject);
                itemObject.CheckObjectType();
                var newCount = GameModuleObjects<ItemObject>().Count(x => x.objectName == itemObject.objectName);
                if (newCount > 1)
                    Debug.LogError("There are " + newCount + " Item Objects named " + itemObject.objectName);
                if (tempName != itemObject.objectName || tempType != itemObject.objectType)
                {
                    UpdateProperties();
                    EditorUtility.SetDirty(itemObject);
                }
            }
        }

        private bool InitialSetup()
        {
            SetNameOnLevels();
            if (_modulesObject.hasBeenSetup) return true;

            if (String.IsNullOrEmpty(_modulesObject.objectType)) return false;
            if (_modulesObject.dictionaries.keyValues == null) return false;

            // Match settings with other objects of this type which all share the same settings (i.e. all turned on, otherwise, stay off)
            var itemObjects = GameModuleObjectsOfType<ItemObject>(_modulesObject.objectType);
            MatchDictionary(itemObjects);
            MatchItemAttributes(itemObjects);
            MatchStatAndSkills(itemObjects);

            _modulesObject.hasBeenSetup = true;
            return true;
        }
        
        private void SetNameOnLevels() => _modulesObject.modificationLevel.ownerName = _modulesObject.objectName;

        private void MatchStatAndSkills(ItemObject[] itemObjects)
        {
            var itemObjectsOfThisType = itemObjects.Length - 1;
            Debug.Log("TO Do: Match Stat and Skills");
        }

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
     */   
    }
}