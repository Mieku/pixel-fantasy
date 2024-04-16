using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static InfinityPBR.Modules.GameModuleUtilities;

namespace InfinityPBR.Modules
{
    public class LootBoxesDrawer : GameModulesDrawer
    {
        // -------------------------------
        // PROPERTIES AND VARIABLES -- UPDATE THESE FOR THIS TYPE
        // -------------------------------
        private LootBox _modulesObject;
        const string ThisTypePlural = "Loot Boxes";
        const string ThisType = "Loot Box";
        private string ClassNamePlural => "LootBoxes";
        private string ClassName => "LootBox";
        private string DocsURL => "https://infinitypbr.gitbook.io/infinity-pbr/game-modules/loot";
        private string DocsURLLabel => "Loot";
        private string[] GameModuleObjectTypes(bool recompute = false) => GameModuleObjectTypes<LootBox>(recompute);
        private LootBox[] GameModuleObjects(bool recompute = false) => GameModuleObjects<LootBox>(recompute);
        private LootBox[] GameModuleObjectsOfType(string type, bool recompute = false) => GameModuleObjectsOfType<LootBox>(type, recompute);
        // -------------------------------
        
        // -------------------------------
        // REQUIRED - NO UPDATE NEEDED
        // -------------------------------
        //private Vector2 _scrollPosition;
        private int _fieldWidth;
        private DictionariesDrawer _dictionariesDrawer;
        private StatModificationLevelDrawer _statModificationLevelDrawer;
        private ItemAttributeDrawer _itemAttributeDrawer;
        private ConditionDrawer _conditionDrawer;
        private string[] _menuBarOptions;
        public bool drawnByGameModulesWindow = true;
        // -------------------------------

        // -------------------------------
        // METHODS -- UPDATE THESE FOR THIS TYPE
        // -------------------------------
        public void SetModulesObject(LootBox modulesObject, bool drawnByWindow = true)
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
            
            
            Cache();
            generatedLoot = new GameItemObjectList();
            foreach (LootBoxItemsSettings itemsSettings in _modulesObject.itemsSettings)
            {
                foreach (LootBoxItemToSpawn itemToSpawn in itemsSettings.itemsToSpawn)
                {
                    itemToSpawn._dictionaryLoaded = false;
                    itemToSpawn.CheckListsForDeletions();
                }
            }
        }
        
        protected void Cache()
        {
            GameModuleObjects<MasteryLevels>(true);
            GameModuleObjectNames<MasteryLevels>(true);
            GameModuleObjectTypes(true);
            GameModuleObjects(true);
            GameModuleObjectsOfType(_modulesObject.objectType, true);
            CheckObjectTypes();
            
            CacheLootItems();
            CacheItemTypes();
            foreach (var itemsSettings in _modulesObject.itemsSettings)
                itemsSettings.CacheAttributeTypeSelection();
        }
        private void CacheItemTypes()
        {
            foreach (var lootBoxItemsSettings in _modulesObject.itemsSettings)
                lootBoxItemsSettings.SetAttributeTypes();
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

            if (!drawnByGameModulesWindow)
            {
                LabelGrey("Loot Box - " + _modulesObject.objectType);
                LabelBig(_modulesObject.objectName, 18, true);
            }

            BeginChangeCheck();

            // If this is a duplicate uid and the button is clicked to reset it, redo Initial Setup
            if (!drawnByGameModulesWindow && CheckUid(_modulesObject.Uid(), _modulesObject, _duplicateUid))
                _initialSetup = false;
            
           
            
            CheckForMissingLootBoxItems();
            _modulesObject.objectName = _modulesObject.name; // Set the lootBoxName variable to be the same as the object name
            
            if (!drawnByGameModulesWindow)
                DisplayHelp(); // Show the user helpful information
            
            if (!drawnByGameModulesWindow)
                Space();
            DisplayAddItems(); // Allow user to add new Loot Box Items to the system
            
            DisplayItems(); // Display all of the items

            Space();
            ShowTest();

            EndChangeCheck(_modulesObject);
            EditorUtility.SetDirty(_modulesObject);
        }
        
        private void CheckObjectTypes()
        {
            foreach (var lootBox in GameModuleObjects<LootBox>())
            {
                var tempName = lootBox.objectName;
                var tempType = lootBox.objectType;
                CheckName(_modulesObject);
                lootBox.CheckObjectType();
                if (tempName != lootBox.objectName || tempType != lootBox.objectType)
                {
                    UpdateProperties();
                    EditorUtility.SetDirty(lootBox);
                }
            }
        }
        
        // Used in the management of the inspector script
        //private int _selectedLootBoxItemsIndex = 0;
        private int _lootItemsIndex;
        private List<LootItems> _cachedLootItems = new List<LootItems>();
        private List<string> _cachedLootItemsNames = new List<string>();

        //private List<ItemObject> _testItems = new List<ItemObject>();
        public GameItemObjectList generatedLoot;
        
        private LootBoxItemsSettings _itemsSettings;
        private LootBoxItemToSpawn _itemToSpawn;
        private string _objectType;
        
        private void CacheLootItems()
        {
            _cachedLootItems.Clear();
            _cachedLootItemsNames.Clear();
            
            // Get all LootItems in the project, and cache them here
            foreach (var lootItems in GameModuleObjects<LootItems>(true))
            {
                _cachedLootItems.Add(lootItems);
                _cachedLootItemsNames.Add(lootItems.objectName);
            }
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
            _modulesObject.hasBeenSetup = true;
            return true;
        }

        private void ShowTest()
        {
            if (_modulesObject.itemsSettings.Count == 0) return;

            BackgroundColor(Color.magenta);
            if (Button("Test Generate Loot"))
                TestGenerateLoot();
            BackgroundColor(Color.white);

            if (generatedLoot == null) return;
            if (generatedLoot.Count() == 0) return;

            Space();
            foreach (GameItemObject itemObject in generatedLoot.list)
            {
                Label(itemObject.FullName());
            }

            Space();
            MessageBox("This is a sample list of what Item Objects may be created by the settings as they are. " +
                       "Keep in mind that depending on your inventory / display set up, if items can't fit into the " +
                       "physical Loot Box, they may be excluded from this list during run time.", MessageType.Info);
        }

        private void TestGenerateLoot()
        {
            generatedLoot = _modulesObject.GenerateLoot();
        }

        private void CheckForMissingLootBoxItems()
        {
            for (int i = 0; i < _modulesObject.itemsSettings.Count; i++)
            {
                LootBoxItemsSettings itemsSettings = _modulesObject.itemsSettings[i];
                if (itemsSettings.lootItems == null)
                {
                    _modulesObject.itemsSettings.RemoveAt(i);
                    ExitGUI();
                }
            }
        }

        /// <summary>
        /// This will display helpful instructions for the user, if they toggle the help on.
        /// </summary>
        private void DisplayHelp()
        {
            SetBool("Show Help " + _modulesObject.name, LeftCheck("Show Instructions", GetBool("Show Help " + _modulesObject.name)));
            if (GetBool("Show Help " + _modulesObject.name))
            {
                Label("Loot Box", true);
                Label("A \"Loot Box\" can be attached to any object in your project. It is intended to create a randomly generated " +
                      "group of \"Item Objects\" (using the Items module), based on specific rules per Loot Box.\n\n" +
                      "First, add \"Loot Items\" objects, which can be created separately. Each one of these will hold a pre-set " +
                      "group of potential Item Objects. You can add any number of Loot Items to your Loot Box.\n\n" +
                      "For each Loot Items added, you can select any number of individual items to add to the final " +
                      "Loot Box. Each item can be \"forced\" to spawn, otherwise is can be considered a \"potential\", with the " +
                      "odds determined by the curve you can set per Loot Items object added.\n\n" +
                      "When populating the final contents of a Loot Box, the script will go top down, starting with the forced " +
                      "items in the first Loot Items group, then the potential items, before moving on to the next Loot " +
                      "Items group.", false, true);
                Space();
                MessageBox("Important: If you are using the \"Inventory\" module, or your own system with a limited amount " +
                           "of space in the final \"box\", it is possible that a forced item lower in the list will not be able to " +
                           "fit. Therefor, important items that absolutely must be included should always be at the top of the list.", MessageType.Warning);
                Space();
                Label(
                    "Visit the scripting docs at http://www.InfinityPBR.com or join us on Discord for additional support.", true, true);
            }
        }


        //-------------------------------------------------------------------------------------------------
        // DISPLAY METHODS
        //-------------------------------------------------------------------------------------------------
        
        /// <summary>
        /// This will display all of the Loot Box Item Settings attached.
        /// </summary>
        private void DisplayItems()
        {
            Space();
            for (int i = 0; i < _modulesObject.itemsSettings.Count; i++)
            {
                LootBoxItemsSettings itemsSettings = _modulesObject.itemsSettings[i];
                _itemsSettings = itemsSettings;
                DisplayItemSettings(i);
            }
        }

        /// <summary>
        /// This will display the actual settings for the specific Loot Box Items
        /// </summary>
        /// <param name="itemsSettings"></param>
        /// <param name="index"></param>
        private void DisplayItemSettings(int itemsSettingsIndex)
        {
            StartVerticalBox();
            DisplayItemSettingsHeader(itemsSettingsIndex);
            DisplayItemSettingsDetails(itemsSettingsIndex);
            EndVerticalBox();
        }

        /// <summary>
        /// Will show the details of the Settings
        /// </summary>
        /// <param name="_itemsSettings"></param>
        /// <param name="index"></param>
        private void DisplayItemSettingsDetails(int itemsSettingsIndex)
        {
            if (_itemsSettings == null) return;
            
            if (!_itemsSettings.show) return;
            //_itemsSettings.scrollPos = EditorGUILayout.BeginScrollView(_itemsSettings.scrollPos);
            StartVerticalBox();
            DisplayItems(itemsSettingsIndex);
            EndVerticalBox();
            //EditorGUILayout.EndScrollView();

            DisplaySpawnChances(itemsSettingsIndex);
            DisplayAttributeChances(itemsSettingsIndex);
            
        }

        /// <summary>
        /// This will show the chances that the prefixes or suffixes will show
        /// </summary>
        /// <param name="itemsSettings"></param>
        /// <param name="index"></param>
        private void DisplayAttributeChances(int index)
        {
            Space();
            // Don't display this section if no attributes are available.
            if (_itemsSettings.activeAttributeTypes.Count == 0)
            {
                LabelGrey("Add attribute types to enable attribute spawn chances.");
                return;
            }
            
            
            StartRow();
            Label("Attribute Chances", 200, true);
            
            EndRow();

            if (_itemsSettings.CountItemsToSpawn() + _itemsSettings.CountItemsToSpawn(true) == 0)
            {
                LabelGrey("No potential items are available.");
                return;
            }
            
            if (_itemsSettings.activeAttributeTypes.Count < 1)
            {
                LabelGrey("No Attributes are available");
                return;
            }
            
            StartRow();
            _itemsSettings.randomizeAttributeOrder = LeftCheck($"Randomize order {symbolInfo}", 
                "If true, the script will randomize the order in which Attributes are resolved. If false, " +
                "the order will be resolved \"top down\".",
                _itemsSettings.randomizeAttributeOrder, 200);
            
            _itemsSettings.stopAfterAttributeFailure = LeftCheck($"Stop after first failure {symbolInfo}", 
                "If true, the Attribute process will stop if any attempt to add and Attribute fails. Otherwise, " +
                "each Attribute will be resolved independently. Forced attributes will always be resolved. " +
                "If an Item Object can't use a particular Attribute, it will be skipped and will not be considered a " +
                "\"failed\" attempt.",
                _itemsSettings.stopAfterAttributeFailure, 200);
            EndRow();
            
            Undo.RecordObject(_modulesObject, "Update curve field");
            _itemsSettings.attributeChances = EditorGUILayout.CurveField(_itemsSettings.attributeChances, Color.green,  new Rect(new Vector2(0, 0), new Vector2(1, 1)), GUILayout.Width(500), GUILayout.Height(50));

            // Display each Item Attribute
            for (var i = 0; i < _itemsSettings.activeAttributeTypes.Count; i++)
            {
                var attributeType = _itemsSettings.activeAttributeTypes[i];
                DisplayAttributeType(attributeType, i);
            }
            
            if (_itemsSettings.randomizeAttributeOrder )
            {
                ContentColor(Color.yellow);
                Label("Random order is true. % shown is based on order shown.");
                ContentColor(Color.white);
            }
        }

        private void DisplayAttributeType(string attributeType, int i)
        {
            StartRow();
            ShowAttributeChances(attributeType, i);
            ShowMoveUpButtonAttribute(attributeType, i);
            ShowMoveDownButtonAttribute(attributeType, i);
            ShowForceToggleAttribute(attributeType, i);
            Label($"{attributeType}", 150);
            EndRow();
        }
        
        /// <summary>
        /// Shows the toggle to force this item to spawn
        /// </summary>
        /// <param name="itemsSettings"></param>
        /// <param name="i"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void ShowForceToggleAttribute(string attributeType, int i)
        {
            bool forceStatus = _itemsSettings.attributeForce[i];
            _itemsSettings.attributeForce[i] = Check(_itemsSettings.attributeForce[i], 40);
            if (forceStatus != _itemsSettings.attributeForce[i])
            {
                _itemsSettings.attributeForce[i] = forceStatus;
                var toIndex = LastForcedAttributeIndex() + (_itemsSettings.attributeForce[i] ? 0 : 1);
                MoveAttributeTo(i, toIndex);
                _itemsSettings.attributeForce[toIndex] = !forceStatus;
            }
        }

        /// <summary>
        /// Shows the button that moves the item down
        /// </summary>
        /// <param name="itemsSettings"></param>
        /// <param name="i"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void ShowMoveDownButtonAttribute(string attributeType, int i)
        {
            if (_itemsSettings.attributeForce[i])
            {
                Label($"{symbolDash}", 25);
                return;
            }
            
            BackgroundColor(CanMoveAttributeDown(i) ? Color.white : Color.grey);
            ContentColor(CanMoveAttributeDown(i) ? Color.white : Color.black);
            if (Button(symbolArrowDown, 25) && CanMoveAttributeDown(i))
            {
                Undo.RecordObject(_modulesObject, "Move Attribute down");
                MoveAttribute(i, 1);
            }
            BackgroundColor(Color.white);
            ContentColor(Color.white);
        }

        /// <summary>
        /// Shows the button that moves the item up
        /// </summary>
        /// <param name="itemsSettings"></param>
        /// <param name="i"></param>
        private void ShowMoveUpButtonAttribute(string attributeType, int i)
        {
            if (_itemsSettings.attributeForce[i])
            {
                Label($"{symbolDash}", 25);
                return;
            }
            
            BackgroundColor(CanMoveAttributeUp(i) ? Color.white : Color.grey);
            ContentColor(CanMoveAttributeUp(i) ? Color.white : Color.black);
            if (Button(symbolArrowUp, 25) && CanMoveAttributeUp(i))
            {
                Undo.RecordObject(_modulesObject, "Move Attribute up");
                MoveAttribute(i, -1);
            }
            BackgroundColor(Color.white);
            ContentColor(Color.white);
        }

        /// <summary>
        /// Displays the chances a single item may spawn
        /// </summary>
        /// <param name="itemsSettings"></param>
        /// <param name="i"></param>
        private void ShowAttributeChances(string attributeType, int i)
        {
            var spawnChance = _itemsSettings.stopAfterAttributeFailure ? AttributeSpawnChanceIfStopAfterFirstFailure(i) : AttributeSpawnChance(i);
            ContentColor(spawnChance == 0 ? Color.red : Color.white);
            Label(Mathf.RoundToInt(spawnChance * 100) + "%", 35);
            ContentColor(Color.white);
        }
        
        private float SpawnChanceIfStopAfterFirstFailure(int index)
        {
            var chances = 1.0f;
            for (var i = 0; i <= index; i++)
                chances *= SpawnChance(i);

            return chances;
        }

        private float AttributeSpawnChanceIfStopAfterFirstFailure(int index)
        {
            var chances = 1.0f;
            for (var i = 0; i <= index; i++)
                chances *= AttributeSpawnChance(i);

            return chances;
        }

        /// <summary>
        /// This will show the chances that an item wil spawn
        /// </summary>
        /// <param name="itemsSettings"></param>
        /// <param name="index"></param>
        private void DisplaySpawnChances(int index)
        {
            Space();
            Label("Spawn Chances", true);
            if (_itemsSettings.CountItemsToSpawn() != 0)
            {
                _itemsSettings.stopAfterFirstFailure = LeftCheck($"Stop after first failure {symbolInfo}", 
                    "If true, potential items will not spawn if a previous " +
                    "attempt failed. Otherwise, script will attempt to spawn each potential " +
                    "item based on it's individual odds.",
                    _itemsSettings.stopAfterFirstFailure, 200);
            }
            if (_itemsSettings.CountItemsToSpawn() == 0)
            {
                LabelGrey("No potential items are available.");
                return;
            }
            
            
            Undo.RecordObject(_modulesObject, "Update curve field");
            _itemsSettings.itemChances = EditorGUILayout.CurveField(_itemsSettings.itemChances, Color.green, new Rect(new Vector2(0, 0), new Vector2(1, 1)), GUILayout.Width(500), GUILayout.Height(50));
        }

        /// <summary>
        /// This will display all the individual items that may spawn
        /// </summary>
        /// <param name="itemsSettings"></param>
        /// <param name="index"></param>
        private void DisplayItems(int itemsSettingsIndex)
        {
            DisplayItemsToSpawnHeader();
            DisplayItemsToSpawn(itemsSettingsIndex);
        }

        /// <summary>
        /// Displays the header area above the items to spawn
        /// </summary>
        /// <param name="itemsSettings"></param>
        private void DisplayItemsToSpawnHeader()
        {
            StartRow();
            BackgroundColor(Color.green);
            Label($"{symbolInfo}", "Press \"Add\" to add items to the potential spawn list. You can " +
                                   "then set whether the item, and any Attributes are random or set to a specific " +
                                   "value. Unless \"Force\" is checked, the chance is not always 100% that any " +
                                   "given Item Object will spawn.", 35);
            if (Button("Add", 54))
            {
                Undo.RecordObject(_modulesObject, "Add new item");
                AddNewItem();
                Cache();
            }
            BackgroundColor(Color.white);
            Label("Force", 40, true);

            var toolTip = "Select which items, if any, may spawn. If multiple are selected, the spawn will " +
                          "choose a random object from the selections. There must be at least one Item Object.";
            var toolTipAttribute =
                "Select which attributes may potentially be added to spawned items. If an attribute " +
                "is not allowed to be added to a spawned item, it will not be added.";
            Label($"Item Obj. {symbolInfo}", toolTip, 100, true);
            
            // Show header for each active attribute type
            for (var index = 0; index < _itemsSettings.activeAttributeTypes.Count; index++)
            {
                var activeAllowedAttributeType = _itemsSettings.activeAttributeTypes[index];
                if (XButton())
                {
                    if (_itemsSettings.attributeForce[index])
                    {
                        var lastBool = _itemsSettings.attributeForce
                            .Select((value, i) => new { Value = value, Index = i })
                            .Where(x => x.Value)
                            .Select(x => x.Index)
                            .LastOrDefault();

                        _itemsSettings.attributeForce.RemoveAt(lastBool);
                    }
                    
                    _itemsSettings.activeAttributeTypes.RemoveAt(index);
                    Cache();
                    ExitGUI();
                }
                Label($"{activeAllowedAttributeType} {symbolInfo}", toolTipAttribute, 72, true);
            }

            // Show option to add new attribute type
            BackgroundColor(Color.yellow);
            if (_itemsSettings.AvailableAllowedAttributeTypes.Length > 0)
            {
                if (_itemsSettings.newAttributeTypeIndex >= _itemsSettings.cachedAvailableAllowedAttributeTypes.Length)
                    _itemsSettings.newAttributeTypeIndex = 0;
                
                _itemsSettings.newAttributeTypeIndex = Popup(_itemsSettings.newAttributeTypeIndex, _itemsSettings.cachedAvailableAllowedAttributeTypes, 150);
                if (Button("Add", 40))
                {
                    _itemsSettings.activeAttributeTypes.Add(_itemsSettings.cachedAvailableAllowedAttributeTypes[_itemsSettings.newAttributeTypeIndex]);
                    Cache();
                }
            }
            else
            {
                LabelGrey("No attributes types available.");
            }
            ResetColor();

            EndRow();
        }

        /// <summary>
        /// This will display the individual items to spawn
        /// </summary>
        /// <param name="itemsSettings"></param>
        private void DisplayItemsToSpawn(int itemsSettingsIndex)
        {
            // Goes through each row in the ItemsSettings, visually, each row in the item list
            for (int i = 0; i < _itemsSettings.itemsToSpawn.Count; i++)
            {
                LootBoxItemToSpawn itemToSpawn = _itemsSettings.itemsToSpawn[i];
                _itemToSpawn = itemToSpawn;
                StartRow();
                ShowItemChances(i);
                ShowMoveUpButton(i);
                ShowMoveDownButton(i);
                ShowForceToggle(i);
                
                // Selection of the main ItemObject to be spawned
                ShowItemSelections(i, itemsSettingsIndex);
                
                // For each active attribute type, show the attribute selection, for potential attributes
                foreach(var activeAllowedAttributeType in _itemsSettings.activeAttributeTypes)
                {
                    ShowAttributeSelections(activeAllowedAttributeType, i, itemsSettingsIndex);
                }
                
                ShowDeleteButton(i);
                EndRow();
            }
        }

        void ToggleSelectAll(LootBoxItemToSpawn itemToSpawn, LootBoxItemsSettings itemsSettings)
        {
            itemToSpawn.itemObjects.Clear();
            foreach (ItemObject itemObject in itemsSettings.lootItems.itemObjects) 
                itemToSpawn.itemObjects.Add(itemObject);
        }
        
        void ToggleItemSelected(object itemName)
        {
            string names = (string) itemName;
            var values = names.Split(',').ToList();

            int.TryParse(values[0], out var itemsSettingsIndex);
            var itemsSettings = _modulesObject.itemsSettings[itemsSettingsIndex];
            int.TryParse(values[1], out var itemToSpawnIndex);
            var itemToSpawn = itemsSettings.itemsToSpawn[itemToSpawnIndex];
            var objectName = values[2];
            
            if (objectName == "All")
            {
                ToggleSelectAll(itemToSpawn, itemsSettings);
                return;
            }

            if (objectName == "First")
            {
                itemToSpawn.itemObjects.Clear();
                itemToSpawn.itemObjects.Add(itemsSettings.lootItems.itemObjects[0]);
                return;
            }
            

            ItemObject itemObject = itemsSettings.lootItems.itemObjects.FirstOrDefault(x => x.objectName == objectName);
            
            if (itemToSpawn.itemObjects.Contains(itemObject))
            {
                if (itemToSpawn.ItemsSelectedCount == 1)
                {
                    Debug.LogWarning("You must have at least one Item Object selected.");
                    return;
                }
                itemToSpawn.itemObjects.RemoveAll(x => x == itemObject);
                return;
            }

            itemToSpawn.itemObjects.Add(itemObject);
        }
        
        void ToggleSelectAllAttributes(LootBoxItemToSpawn itemToSpawn, string objectType)
        {
            var availableItemAttributes = itemToSpawn.availableItemAttributes.FirstOrDefault(x => x.objectType == objectType);
            availableItemAttributes.attributes.Clear();
            
            foreach (var itemAttribute in GameModuleObjectsOfType<ItemAttribute>(objectType))
                availableItemAttributes.attributes.Add(itemAttribute);
        }
        
        void ToggleItemAttributeSelected(object itemName)
        {
            string names = (string) itemName;
            var values = names.Split(',').ToList();

            int.TryParse(values[0], out var itemsSettingsIndex);
            var itemsSettings = _modulesObject.itemsSettings[itemsSettingsIndex];
            int.TryParse(values[1], out var itemToSpawnIndex);
            var itemToSpawn = itemsSettings.itemsToSpawn[itemToSpawnIndex];
            var objectType = values[2];
            var objectName = values[3];
            
            if (objectName == "All")
            {
                ToggleSelectAllAttributes(itemToSpawn, objectType);
                return;
            }

            if (objectName == "None")
            {
                itemToSpawn.availableItemAttributes
                    .FirstOrDefault(x => x.objectType == objectType)!
                    .attributes
                    .Clear();
                return;
            }

            var availableItemAttributesList =
                itemToSpawn
                    .availableItemAttributes
                    .FirstOrDefault(x => x.objectType == objectType);
            
            if (availableItemAttributesList == null)
            {
                Debug.LogError($"Could not find availableItemAttributesList for {objectType}");
                return;
            }
            
            // If the list contains the attribute, remove it
            if (availableItemAttributesList.attributes.FirstOrDefault(x => x.ObjectName == objectName))
            {
                availableItemAttributesList.attributes.RemoveAll(x => x.ObjectName == objectName);
                return;
            }
            
            // Otherwise add it
            availableItemAttributesList.attributes
                .Add(GameModuleObjectsOfType<ItemAttribute>(objectType)
                .FirstOrDefault(x => x.name == objectName));
        }

        /// <summary>
        /// Displays the dropdowns for Items, Prefix, Item, and Suffix
        /// </summary>
        /// <param name="itemsSettings"></param>
        /// <param name="i"></param>
        /// <param name="itemToSpawn"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void ShowItemSelections(int itemToSpawnIndex, int itemsSettingsIndex)
        {
            var itemsSettings = _modulesObject.itemsSettings[itemsSettingsIndex];
            var itemToSpawn = itemsSettings.itemsToSpawn[itemToSpawnIndex];

            // Check if itemObjects list is empty or first item is null and add all items if needed
            IfNullOrEmptySelectAll(itemToSpawn, itemsSettings);

            var countSelected = itemToSpawn.ItemsSelectedCount;
            var label = countSelected == 1 ? itemToSpawn.itemObjects[0].objectName : $"Rand. of {countSelected}";
            
            // Show a button with the above label. If the button is clicked, show a context menu (GenericMenu).
            if (Button(label, 100))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Select All"), false, ToggleItemSelected, $"{itemsSettingsIndex},{itemToSpawnIndex},All");
                menu.AddItem(new GUIContent("Select First"), false, ToggleItemSelected, $"{itemsSettingsIndex},{itemToSpawnIndex},First");
                menu.AddSeparator("");

                // Add an option to the menu for each item in lootItems. 
                // If the item is already in itemObjects list of itemToSpawn, this option is checked in the menu.
                foreach (ItemObject itemObject in itemsSettings.lootItems.itemObjects)
                {
                    menu.AddItem(new GUIContent($"{itemObject.objectType} - {itemObject.objectName}"),
                        itemToSpawn.itemObjects.Contains(itemObject), ToggleItemSelected,
                        $"{itemsSettingsIndex},{itemToSpawnIndex},{itemObject.objectName}");
                }

                menu.ShowAsContext();
            }
        }

        // If itemObjects list is null, empty or the first item is null, clear the list and add all items from lootItems
        private void IfNullOrEmptySelectAll(LootBoxItemToSpawn itemToSpawn, LootBoxItemsSettings itemsSettings)
        {
            if (itemToSpawn.itemObjects != null 
                && itemToSpawn.itemObjects.Count != 0 
                && itemToSpawn.itemObjects[0] != null) return;
            
            itemToSpawn.itemObjects.Clear();
            itemToSpawn.itemObjects.AddRange(itemsSettings.lootItems.itemObjects);
        }

        private void ShowAttributeSelections(string attributeType, int itemToSpawnIndex, int itemsSettingsIndex)
        {
            // Make sure the list exists on the ItemToSpawn
            var thisAttributeList = _itemToSpawn
                .availableItemAttributes
                .FirstOrDefault(x => x.objectType == attributeType);
            
            // If the list doesn't exist on this itemToSpawn, create it
            if (thisAttributeList == null)
            {
                var newAttributeList = new AvailableItemAttributes
                {
                    objectType = attributeType,
                    attributes = new List<ItemAttribute>()
                };
                newAttributeList.attributes.AddRange(GameModuleObjectsOfType<ItemAttribute>(attributeType));
                _itemToSpawn.availableItemAttributes.Add(newAttributeList);
                thisAttributeList = _itemToSpawn.availableItemAttributes.Last();
            }

            // Ensure the list is created
            thisAttributeList.attributes ??= new List<ItemAttribute>();

            var countSelected = thisAttributeList.attributes.Count;
            var label = countSelected == 0 ? "None" : countSelected == 1 ? thisAttributeList.attributes[0].objectName : $"Rand. of {countSelected}";

            if (Button(label, 100))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Select All"), false, ToggleItemAttributeSelected, $"{itemsSettingsIndex},{itemToSpawnIndex},{attributeType},All");
                menu.AddItem(new GUIContent("Select None"), false, ToggleItemAttributeSelected, $"{itemsSettingsIndex},{itemToSpawnIndex},{attributeType},None");
                menu.AddSeparator("");
                
                //foreach (ItemAttribute itemAttribute in _itemsSettings.itemAttributes(attributeType))
                var allAttributesOfThisType = GameModuleObjectsOfType<ItemAttribute>(attributeType);
                foreach(ItemAttribute itemAttribute in allAttributesOfThisType)
                {
                    // Ensure the list is created
                    _itemToSpawn.availableItemAttributes ??= new List<AvailableItemAttributes>();

                    menu.AddItem(new GUIContent($"{itemAttribute.objectName}"), 
                        _itemToSpawn.availableItemAttributes
                            .FirstOrDefault(x => x.objectType == itemAttribute.objectType)!
                            .attributes
                            .Contains(itemAttribute), ToggleItemAttributeSelected, $"{itemsSettingsIndex},{itemToSpawnIndex},{itemAttribute.objectType},{itemAttribute.objectName}");
                }

                menu.ShowAsContext();
            }
        }

        /// <summary>
        /// Displays the button to delete an item from the list
        /// </summary>
        /// <param name="itemsSettings"></param>
        /// <param name="i"></param>
        private void ShowDeleteButton(int i)
        {
            BackgroundColor(Color.red);
            if (Button(symbolX, 25))
            {
                Undo.RecordObject(_modulesObject, "Delete Item To Spawn");
                _itemsSettings.itemsToSpawn.RemoveAt(i);
                EditorUtility.SetDirty(_modulesObject);
                ExitGUI();
            }
            BackgroundColor(Color.white);
        }

        /// <summary>
        /// Shows the toggle to force this item to spawn
        /// </summary>
        /// <param name="itemsSettings"></param>
        /// <param name="i"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void ShowForceToggle(int i)
        {
            bool forceStatus = _itemToSpawn.force;
            _itemToSpawn.force = Check(_itemToSpawn.force, 40);
            if (forceStatus != _itemToSpawn.force)
            {
                _itemToSpawn.force = forceStatus;
                MoveItemToSpawnTo(i, LastForcedToSpawnIndex() + (_itemToSpawn.force ? 0 : 1));
                _itemToSpawn.force = !forceStatus;
            }
        }

        /// <summary>
        /// Shows the button that moves the item down
        /// </summary>
        /// <param name="itemsSettings"></param>
        /// <param name="i"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void ShowMoveDownButton(int i)
        {
            if (_itemsSettings.itemsToSpawn[i].force)
            {
                Label($"{symbolDash}", 25);
                return;
            }
            BackgroundColor(CanMoveItemDown(i) ? Color.white : Color.grey);
            ContentColor(CanMoveItemDown(i) ? Color.white : Color.black);
            if (Button(symbolArrowDown, 25) && CanMoveItemDown(i))
            {
                Undo.RecordObject(_modulesObject, "Move Item to Spawn down");
                MoveItemToSpawn(i, 1);
            }
            BackgroundColor(Color.white);
            ContentColor(Color.white);
        }

        /// <summary>
        /// Shows the button that moves the item up
        /// </summary>
        /// <param name="itemsSettings"></param>
        /// <param name="i"></param>
        private void ShowMoveUpButton(int i)
        {
            if (_itemsSettings.itemsToSpawn[i].force)
            {
                Label($"{symbolDash}", 25);
                return;
            }
            BackgroundColor(CanMoveItemUp(i) ? Color.white : Color.grey);
            ContentColor(CanMoveItemUp(i) ? Color.white : Color.black);
            if (Button(symbolArrowUp, 25) && CanMoveItemUp(i))
            {
                Undo.RecordObject(_modulesObject, "Move Item to Spawn up");
                MoveItemToSpawn(i, -1);
            }
            BackgroundColor(Color.white);
            ContentColor(Color.white);
        }

        /// <summary>
        /// Displays the chances a single item may spawn
        /// </summary>
        /// <param name="itemsSettings"></param>
        /// <param name="i"></param>
        private void ShowItemChances(int i)
        {
            // Chances
            float spawnChance = _itemsSettings.stopAfterFirstFailure ? SpawnChanceIfStopAfterFirstFailure(i) : SpawnChance(i);
            ContentColor(spawnChance == 0 ? Color.red : Color.white);
            Label(Mathf.RoundToInt(spawnChance * 100) + "%", 35);
            ContentColor(Color.white);
        }
        
        /// <summary>
        /// Gives the chances for spwaning. NOTE: if you customize the code with an override, this will not change!
        /// </summary>
        /// <param name="itemToSpawnIndex"></param>
        /// <param name="itemsSettings"></param>
        /// <returns></returns>
        public float SpawnChance(int itemToSpawnIndex)
        {
            if (_itemsSettings.itemsToSpawn[itemToSpawnIndex].force)
                return 1f;

            if (_itemsSettings.itemsToSpawn.Count == 1)
                return Mathf.Clamp(_itemsSettings.ValueFromCurve(0), 0f, 1f);

            int forcedItems = _itemsSettings.CountItemsToSpawn(true);
            
            float thisPosition = itemToSpawnIndex - forcedItems;
            float endPosition = _itemsSettings.itemsToSpawn.Count - forcedItems;
            
            if (endPosition <= 0)
                return Mathf.Clamp(_itemsSettings.ValueFromCurve(0), 0f, 1f);

            var percentInList = thisPosition / endPosition;
            
            return Mathf.Clamp(_itemsSettings.ValueFromCurve(percentInList), 0f, 1f);
        }
        
        public float AttributeSpawnChance(int attributeOrderIndex)
        {
            if (_itemsSettings.attributeForce[attributeOrderIndex])
                return 1f;
            
            var forcedAttributes = _itemsSettings.CountAttributes(true);
            var thisPosition = attributeOrderIndex - forcedAttributes;
            var endPosition = _itemsSettings.activeAttributeTypes.Count - forcedAttributes - 1;
            var percentInList = endPosition == 0 ? 0 : (float)thisPosition / endPosition;
            
            return Mathf.Clamp(_itemsSettings.ValueFromAttributeCurve(percentInList), 0f, 1f);
        }

        /// <summary>
        /// Adds a new item to the list
        /// </summary>
        /// <param name="itemsSettings"></param>
        private void AddNewItem()
        {
            var newItem = new LootBoxItemToSpawn
            {
                lootItems = _itemsSettings.lootItems //, itemIndex = 0, randomItem = true, 
            };
            _itemsSettings.itemsToSpawn.Add(newItem);
            _itemsSettings.attributeForce.Add(false);
            
        }

        /// <summary>
        /// Returns an array of item names available in the Loot Box Items object
        /// </summary>
        /// <param name="lootItems"></param>
        /// <returns></returns>
        private String[] ItemsNames(LootItems lootItems)
        {
            int i = 1;
            
            var allItems = new string[lootItems.itemObjects.Count + 1];
            foreach (ItemObject itemObject in lootItems.itemObjects)
            {
                allItems[i] = itemObject.objectName;
                i++;
            }
            
            return allItems;
        }

        /// <summary>
        /// Displays the header of the settings area.
        /// </summary>
        /// <param name="_itemsSettings"></param>
        /// <param name="index"></param>
        private void DisplayItemSettingsHeader(int index)
        {
            if (_itemsSettings == null) return;
            StartRow();
            
            // On Off Button
            _itemsSettings.show = OnOffButton(_itemsSettings.show);
            
            // Move Up button
            BackgroundColor(CanMoveUp(index) ? Color.white : Color.grey);
            ContentColor(CanMoveUp(index) ? Color.white : Color.black);
            if (Button(symbolArrowUp, 25) && CanMoveUp(index))
            {
                Undo.RecordObject(_modulesObject, "Move Loot Box Item up");
                MoveItem(index, -1);
            }
            BackgroundColor(Color.white);
            ContentColor(Color.white);
            
            // Move Down Button
            BackgroundColor(CanMoveDown(index) ? Color.white : Color.grey);
            ContentColor(CanMoveDown(index) ? Color.white : Color.black);
            if (Button(symbolArrowDown, 25) && CanMoveDown(index))
            {
                Undo.RecordObject(_modulesObject, "Move Loot Box Item down");
                MoveItem(index, 1);
            }
            BackgroundColor(Color.white);
            ContentColor(Color.white);
            
            // Title / Toggle Button
            BackgroundColor(_itemsSettings.show ? Color.white : Color.grey);
            Label(_itemsSettings.lootItems.name,200, true);
            BackgroundColor(Color.white);

            // Delete Button
            BackgroundColor(Color.red);
            if (Button(symbolX, 25))
            {
                Undo.RecordObject(_modulesObject, "Delete Loot Box Item");
                _modulesObject.itemsSettings.RemoveAt(index);
                EditorUtility.SetDirty(_modulesObject);
                ExitGUI();
            }
            BackgroundColor(Color.white);

            ShowItemBreakdown(index);

            EndRow();
        }

        /// <summary>
        /// This will show the breakdown of the items currently in the Loot Box Item Settings, which contains a single
        /// Loot Box Item along with the settings for spawning.
        /// </summary>
        /// <param name="itemsSettings"></param>
        /// <param name="index"></param>
        private void ShowItemBreakdown(int index)
        {
            ContentColor(_itemsSettings.CountItemsToSpawn(true) > 0 ? Color.white : Color.grey);
            Label(_itemsSettings.CountItemsToSpawn(true) + " forced item" +
                  (_itemsSettings.CountItemsToSpawn(true) == 1 ? "" : "s"), 85);
            
            ContentColor(_itemsSettings.CountItemsToSpawn(true) > 0 ? _itemsSettings.CountItemsToSpawn() > 0 ? Color.white : Color.grey : Color.grey);
            Label(" + ", 15); 
            
            ContentColor(_itemsSettings.CountItemsToSpawn() > 0 ? Color.white : Color.grey);
            Label(_itemsSettings.CountItemsToSpawn() + " potential item" + 
                  (_itemsSettings.CountItemsToSpawn() == 1 ? "" : "s"), 100);
            ContentColor(Color.white);
        }

        private int copiedUid;
        
        /// <summary>
        /// Displays the unique ID for this object
        /// </summary>
        private void DisplayUid()
        {
            copiedUid = Mathf.Clamp(copiedUid - 1, 0, copiedUid);

            StartRow();
            BackgroundColor(copiedUid > 0 ? Color.green : Color.white);
            if (Button(copiedUid > 0 ? "Copied!" : "Copy Uid", 80))
            {
                EditorGUIUtility.systemCopyBuffer = _modulesObject.Uid();
                copiedUid = 100;
            }
            BackgroundColor(Color.white);
            LabelGrey("uid: " + _modulesObject.Uid());
            EndRow();
        }

        /// <summary>
        /// Displays the box where users can add Loot Items to the Loot Box
        /// </summary>
        private void DisplayAddItems()
        {
            if (_cachedLootItemsNames.Count == 0) return;
            
            BackgroundColor(Color.yellow);
            StartVerticalBox();
            
            Label("Add <b><color=#77ffff>Loot Items</color></b> to this <b><color=#77ffff>Loot Box</color></b>", false, true, true);
            
            StartRow();
            _lootItemsIndex = Popup(_lootItemsIndex, _cachedLootItemsNames.ToArray(), 200);
            if (Button("Add", 200))
            {
                Undo.RecordObject(_modulesObject, "Undo");
                AddLootItems(_cachedLootItems[_lootItemsIndex]);
                Cache();
            }
            EndRow();

            LootItems lootItems = _cachedLootItems[_lootItemsIndex];
            if (!String.IsNullOrWhiteSpace(lootItems.description))
            {
                Label(lootItems.description);
            }
            
            EndVerticalBox();
            BackgroundColor(Color.white);
        }
        
        //-------------------------------------------------------------------------------------------------
        // OTHER METHODS
        //-------------------------------------------------------------------------------------------------

        /// <summary>
        /// Returns true if this can move down
        /// </summary>
        /// <param name="itemsSettings"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private bool CanMoveDown(int index)
        {
            if (index + 1 >= _modulesObject.itemsSettings.Count) return false;
            return true;
        }

        /// <summary>
        /// Returns true if this can move up
        /// </summary>
        /// <param name="itemsSettings"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private bool CanMoveUp(int index)
        {
            if (index - 1 < 0) return false;
            return true;
        }
        
        /// <summary>
        /// Returns true if the item can move down
        /// </summary>
        /// <param name="itemsSettings"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private bool CanMoveItemDown(int index)
        {
            if (index + 1 >= _itemsSettings.itemsToSpawn.Count) return false;

            return true;
        }
        
        private bool CanMoveAttributeDown(int index)
        {
            if (index + 1 >= _itemsSettings.activeAttributeTypes.Count) return false;

            return true;
        }

        /// <summary>
        /// Returns true if this item can move up
        /// </summary>
        /// <param name="itemsSettings"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private bool CanMoveItemUp(int index)
        {
            if (index - 1 <= LastForcedToSpawnIndex()) return false;

            return true;
        }
        
        private bool CanMoveAttributeUp(int index)
        {
            if (index - 1 <= LastForcedAttributeIndex()) return false;

            return true;
        }
        
        /// <summary>
        /// Adds a loot box item
        /// </summary>
        /// <param name="lootItems"></param>
        private void AddLootItems(LootItems lootItems)
        {
            _modulesObject.AddLootItems(lootItems);
        }
        
        /// <summary>
        /// Moves the item to spawn to a new location in the List(), up or down
        /// </summary>
        /// <param name="itemsSettings"></param>
        /// <param name="index"></param>
        /// <param name="moveValue"></param>
        private void MoveItemToSpawn(int index, int moveValue)
        {
            if (index + moveValue < 0) return;
            if (index + moveValue >= _itemsSettings.itemsToSpawn.Count) return;

            MoveItemToSpawnTo(index, index + moveValue);
        }
        
        private void MoveAttribute(int index, int moveValue)
        {
            if (index + moveValue < 0) return;
            if (index + moveValue >= _itemsSettings.attributeTypes.Count) return;

            MoveAttributeTo(index, index + moveValue);
        }

        /// <summary>
        /// Move the item to spawn to a new location in the List() as specified
        /// </summary>
        /// <param name="itemsSettings"></param>
        /// <param name="fromIndex"></param>
        /// <param name="toIndex"></param>
        private void MoveItemToSpawnTo(int fromIndex, int toIndex)
        {
            if (toIndex < 0) return;
            if (toIndex >= _itemsSettings.itemsToSpawn.Count) return;
            
            LootBoxItemToSpawn itemToSpawn = _itemsSettings.itemsToSpawn[fromIndex];
            _itemsSettings.itemsToSpawn.RemoveAt(fromIndex);
            _itemsSettings.itemsToSpawn.Insert(toIndex, itemToSpawn);
        }
        
        private void MoveAttributeTo(int fromIndex, int toIndex)
        {
            if (toIndex < 0) return;
            if (toIndex >= _itemsSettings.activeAttributeTypes.Count) return;
            
            string attributeType = _itemsSettings.activeAttributeTypes[fromIndex];
            bool attributeForce = _itemsSettings.attributeForce[fromIndex];
            
            _itemsSettings.activeAttributeTypes.RemoveAt(fromIndex);
            _itemsSettings.activeAttributeTypes.Insert(toIndex, attributeType);
            
            _itemsSettings.attributeForce.RemoveAt(fromIndex);
            _itemsSettings.attributeForce.Insert(toIndex, attributeForce);
        }
        
        /// <summary>
        /// Moves the item to a new location in the list, by moveValue
        /// </summary>
        /// <param name="index"></param>
        /// <param name="moveValue"></param>
        private void MoveItem(int index, int moveValue)
        {
            if (index + moveValue < 0) return;
            if (index + moveValue >= _modulesObject.itemsSettings.Count) return;

            MoveItemTo(index, index + moveValue);
        }

        /// <summary>
        /// Moves the item to a new location in the List() as specified
        /// </summary>
        /// <param name="fromIndex"></param>
        /// <param name="toIndex"></param>
        private void MoveItemTo(int fromIndex, int toIndex)
        {
            if (toIndex < 0) return;
            if (toIndex >= _modulesObject.itemsSettings.Count) return;
            
            LootBoxItemsSettings item = _modulesObject.itemsSettings[fromIndex];
            _modulesObject.itemsSettings.RemoveAt(fromIndex);
            _modulesObject.itemsSettings.Insert(toIndex, item);
        }

        /// <summary>
        /// Returns the index of the last forced item in the List()
        /// </summary>
        /// <param name="itemsSettings"></param>
        /// <returns></returns>
        private int LastForcedToSpawnIndex()
        {
            int lastForcedIndex = -1;
            for (int i = 0; i < _itemsSettings.itemsToSpawn.Count; i++)
            {
                if (!_itemsSettings.itemsToSpawn[i].force)
                    return lastForcedIndex;
                
                lastForcedIndex = i;
            }

            return lastForcedIndex;
        }
        
        private int LastForcedAttributeIndex()
        {
            int lastForcedIndex = -1;
            for (int i = 0; i < _itemsSettings.attributeForce.Count; i++)
            {
                if (!_itemsSettings.attributeForce[i])
                    return lastForcedIndex;
                
                lastForcedIndex = i;
            }

            return lastForcedIndex;
        }
    }
}