using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static InfinityPBR.Modules.GameModuleUtilities;
using static InfinityPBR.InfinityEditor;
using static InfinityPBR.Modules.InfinityEditorGameModules;

namespace InfinityPBR.Modules
{
    public class ItemObjectsManager
    {
        // -------------------------------
        // UPDATE THESE FOR THIS TYPE
        // -------------------------------
        private ItemObjectDrawer _drawer = ScriptableObject.CreateInstance<ItemObjectDrawer>();
        const string ThisType = "Item Object";
        private string ClassNamePlural => "ItemObjects";
        private string ClassName => "ItemObject";
        //private string[] GameModuleObjectTypes(bool recompute = false) => GameModuleObjectTypes<ItemObject>(recompute);
        private string[] _gameModuleObjectTypes;
        private string[] CacheGameModuleObjectTypes() => _gameModuleObjectTypes = GameModuleObjectTypes<ItemObject>(true);
        
        private ItemObject[] GameModuleObjects(bool recompute = false) => GameModuleObjects<ItemObject>(recompute);
        //private ItemObject[] GameModuleObjectsOfType(string type, bool recompute = false) => GameModuleObjectsOfType<ItemObject>(type, recompute);
        private string FinalDestination => LocationToSave<ItemObject>(NewObjectTypeIndex, SaveLocation, NewObjectType);
        
        private Dictionary<string, ItemObject[]> _gameModuleObjectsOfType;
        private Dictionary<string, ItemObject[]> CacheGameModuleObjectsOfType()
        {
            _gameModuleObjectsOfType = new Dictionary<string, ItemObject[]>();
            foreach(var type in _gameModuleObjectTypes)
                _gameModuleObjectsOfType.Add(type, GameModuleObjectsOfType<ItemObject>(type, true));

            return _gameModuleObjectsOfType;
        }


        string[] MainMenuButtonNames = { "Item Objects", "Types", "Create" };
        string[] MainMenuButtonToolTips = { "Manage individual Item Object values."
            , "Update the settings and values that apply to all Item Objects of a specific type."
            , "Create new Item Object objects." };
        // -------------------------------
        
        // -------------------------------
        // REQUIRED PROPERTIES - NO UPDATE NEEDED
        // -------------------------------
        private string MainMenuSelected => MainMenuButtonNames[MainMenuIndex];
        private string GetObjectType => GetString($"{ThisType} Manager {ThisType} Type");
        private void SetObjectType(string newType) => SetString($"{ThisType} Manager {ThisType} Type", newType);
        private string NewObjectNames
        {
            get => EditorPrefs.GetString($"New Object Names {ThisType}", "");
            set => EditorPrefs.SetString($"New Object Names {ThisType}", value);
        }
        
        private string NewObjectType
        {
            get => EditorPrefs.GetString($"New Object Type {ThisType}", "");
            set => EditorPrefs.SetString($"New Object Type {ThisType}", value);
        }
        
        private int NewObjectTypeIndex
        {
            get => EditorPrefs.GetInt($"New Object Type Index {ThisType}", 0);
            set => EditorPrefs.SetInt($"New Object Type Index {ThisType}", value);
        }
        
        private int MainMenuIndex
        {
            get => EditorPrefs.GetInt($"Main Menu Index {ThisType}", 0);
            set => EditorPrefs.SetInt($"Main Menu Index {ThisType}", value);
        }
        
        private int SearchTypeIndex
        {
            get => EditorPrefs.GetInt($"Search Type Index {ThisType}", -1);
            set => EditorPrefs.SetInt($"Search Type Index {ThisType}", value);
        }
        
        private string SearchString
        {
            get => EditorPrefs.GetString($"Search String {ThisType}", "");
            set => EditorPrefs.SetString($"Search String {ThisType}", value);
        }
        
        private bool GetShowDictionaries(string type) => EditorPrefs.GetBool($"{ThisType} Search String {type}", false);
        private void SetShowDictionaries(string type, bool value) => EditorPrefs.SetBool($"{ThisType} Search String {type}", value);
        
        private readonly string _objectManagerSaveLocationKey = $"{ThisType} Manager Save Location";
        private readonly string _defaultSaveLocation = $"Game Module Objects/{ThisType}s/";
       
        private string SaveLocation
        {
            get => string.IsNullOrWhiteSpace(EditorPrefs.GetString(_objectManagerSaveLocationKey, DefaultPathValue())) 
                ? DefaultPathValue() 
                : EditorPrefs.GetString(_objectManagerSaveLocationKey, _defaultSaveLocation);
            set => SetString(_objectManagerSaveLocationKey, value);
        }
        
        private readonly int _fieldWidth = 200;
        private DictionariesDrawer _dictionariesDrawer;
        private void SetDictionaryDrawer() => _dictionariesDrawer = new DictionariesDrawerEditor(_fieldWidth);
        private static void ShowLinkButton(ModulesScriptableObject moduleObject)
        {
            if (Button($"{symbolCircleArrow}", 25))
                EditorGUIUtility.PingObject(moduleObject);
        }
        private void ShowLinkToFolder(string type)
        {
            if (Button($"{symbolCircleArrow}", 25))
            {
                var obj = _gameModuleObjectsOfType[type][0];
                string objectPath = AssetDatabase.GetAssetPath(obj);
                string directoryPath = Path.GetDirectoryName(objectPath);

                var directory = AssetDatabase.LoadAssetAtPath<Object>(directoryPath);
                Selection.activeObject = directory;
                EditorGUIUtility.PingObject(directory);
            }
        }
        // -------------------------------

        // -------------------------------
        // REQUIRED METHODS - UPDATE ONLY FOR SPECIFICS
        // -------------------------------
        private string DefaultPathValue()
        {
            var moduleObjects = GameModuleObjects();
            if (moduleObjects.Length == 0) 
                return _defaultSaveLocation;
            
            var firstObject = moduleObjects[0];
            var path = AssetDatabase.GetAssetPath(firstObject);
            path = path.Replace($"{firstObject.ObjectType}/{firstObject.name}.asset", "");
            path = path.Replace("Assets/", "");
            
            return path;
        }
        
        private void ResetCache()
        {
            if (!EditorPrefs.GetBool($"{ThisType} Manager Cache", false)) return;
            
            GameModuleObjects<ItemObject>(true);
            GameModuleObjects<ItemAttribute>(true);
            GameModuleObjectTypes<ItemObject>(true);
            GameModuleObjectTypes<ItemAttribute>(true);
            CheckObjectNamesAndTypes(GameModuleObjects());
            CacheGameModuleObjectTypes();
            CacheGameModuleObjectsOfType(); // Must be done after CacheGameModuleObjectTypes()
            
            SetBool($"{ThisType} Manager Cache", false);
            DebugConsoleMessage("Item Objects Cache Reset");
        }
        
        private void CheckForNullDrawer()
        {
            if (_dictionariesDrawer != null) return;
            SetDictionaryDrawer();
        }
        // -------------------------------

        // -------------------------------
        // DRAW AND OTHER METHODS BELOW - UPDATE TYPES AS NEEDED
        // -------------------------------
        public void Draw(Rect area)
        {
            ResetCache();
            
            if (NoTypes()) return;

            CheckForNullDrawer();

            ShowMainButtons();
            
            BlackLine();

            
            if (MainMenuIndex >= MainMenuButtonNames.Length || MainMenuIndex < 0)
                MainMenuIndex = 0;
            
            if (MainMenuSelected == "Create")
                ManageCreateNew();

            if (MainMenuSelected == "Types")
                ManageTypes();

            if (MainMenuSelected == "Item Objects")
                ManageObjects();
            
            
            SetAllDirty<ItemObject>();
        }
        

        private void ManageObjects()
        {
            DrawSearchBox();
            DrawResults();
            
        }
        
        private readonly string[] _quickViewOptions = {
            "Starting Attributes", "Inventory System"
        };
        
        private string QuickViewSelection
        {
            get => EditorPrefs.GetString($"Quick View Selection {ThisType}", _quickViewOptions[0]);
            set => SetString($"Quick View Selection {ThisType}", value);
        }

        private int QuickViewIndex => _quickViewOptions.ToList().IndexOf(QuickViewSelection);

        private void ShowQuickView()
        {
            if (!_quickViewOptions.ToList().Contains(QuickViewSelection))
                QuickViewSelection = _quickViewOptions[0];
            QuickViewSelection = _quickViewOptions[Popup(QuickViewIndex, _quickViewOptions, 250)];
            Label("", 25);
            Label("", 25);
        }
        
        private void DrawResultsHeader()
        {
            StartRow();
            ShowQuickView();
            ResultsHeaderStartingAttribute();
            ResultsHeaderInventorySystem();
            EndRow();
        }

        private void ResultsHeaderInventorySystem()
        {
            if (QuickViewSelection != "Inventory System") return;
            
            Label("<b>Height</b>", 50, false, false, true);
            Label("<b>Width</b>", 50, false, false, true);
            Label("<b>Inventory Prefab</b>", 100, false, false, true);
            Label("<b>World Prefab</b>", 100, false, false, true);
        }
        
        private void ResultsInventorySystem(ItemObject moduleObject)
        {
            if (QuickViewSelection != "Inventory System") return;

            moduleObject.inventoryHeight = Int(moduleObject.inventoryHeight, 50);
            moduleObject.inventoryWidth = Int(moduleObject.inventoryWidth, 50);
            moduleObject.prefabInventory = Object(moduleObject.prefabInventory, typeof(GameObject), 100) as GameObject;
            moduleObject.prefabWorld = Object(moduleObject.prefabWorld, typeof(GameObject), 100) as GameObject;
        }

        private void ResultsHeaderStartingAttribute()
        {
            if (QuickViewSelection != "Starting Attributes") return;
            
            Label("<b>Starting & Variable Attributes</b>", 200, false, false, true);
        }
        
        private void ResultsStartingAttribute(ItemObject moduleObject)
        {
            if (QuickViewSelection != "Starting Attributes") return;
            AttributeString(moduleObject);
        }

        private void DrawResults()
        {
            DrawResultsHeader();
            GreyLine(1, 0);
            foreach(var moduleObject in SearchResults<ItemObject>(SearchTypeIndex, SearchString))
            {
                if (!moduleObject.showInManager) // NOTE ADD THIS TO MODULE OBJECT CLASSES
                {
                    //Undo.RecordObject(moduleObject, "Undo Value change");
                    ShowOff(moduleObject);
                }
                else
                    ShowOn(moduleObject);
                EditorUtility.SetDirty(moduleObject);
            }
        }

        private string QuestItemLabel(ItemObject moduleObject) => moduleObject.questItem ? " <i><b><color=#7777ff>Quest Item</color></b></i>" : "";
        private void ShowOff(ItemObject moduleObject)
        {
            StartRow();
            moduleObject.showInManager = OnOffButton(moduleObject.showInManager);
            ShowLinkButton(moduleObject);
            Label($"<b>{moduleObject.ObjectName}</b> <color=#777777><i>{moduleObject.ObjectType}</i></color>{QuestItemLabel(moduleObject)}", 250, false, false, true);
            // More here
            ResultsStartingAttribute(moduleObject);
            ResultsInventorySystem(moduleObject);
            EndRow();
        }

        private void AttributeString(ItemObject moduleObject)
        {
            var attributeList = GetAttributeList(moduleObject);
            var attributeString = "";
            foreach(var startingAttribute in attributeList)
            {
                if (!string.IsNullOrWhiteSpace(attributeString))
                    attributeString = $"{attributeString}, ";
                attributeString = $"{attributeString}{startingAttribute}";
            }

            Label($"{attributeString}", 300, false, false, true);
        }

        private List<string> GetAttributeList(ItemObject moduleObject)
        {
            var attributeList = new List<string>();
            foreach(var startingAttribute in moduleObject.startingItemAttributes)
            {
                attributeList.Add($"<color=#999999><b>{startingAttribute.ObjectName}</b></color>");
            }
            foreach(var variable in moduleObject.variables)
            {
                if (variable.variableAttributes.Count == 0) continue;
                var startingAttribute = variable.ActiveAttribute;
                attributeList.Add($"<color=#999999><b>{startingAttribute.ObjectName}</b></color>");
            }

            return attributeList;
        }

        private void ShowOn(ItemObject moduleObject)
        {
            StartVerticalBox();
            StartRow();
            moduleObject.showInManager = OnOffButton(moduleObject.showInManager);
            ShowLinkButton(moduleObject);
            if (!moduleObject.showInManager && KeyShift)
                CloseAllInManager<ItemObject>();
            
            Label($"<b>{moduleObject.ObjectName}</b> <color=#777777><i>{moduleObject.ObjectType}</i></color>{QuestItemLabel(moduleObject)}", 250, false, false, true);
            Undo.RecordObject(moduleObject, "Undo Value change");
            ResultsStartingAttribute(moduleObject);
            Undo.RecordObject(moduleObject, "Undo Value change");
            ResultsInventorySystem(moduleObject);
            EndRow();

            _drawer.SetModulesObject(moduleObject);
            _drawer.Draw();
            
            EndVerticalBox();
        }

        private void DrawSearchBox()
        {
            var searchResults = SearchBoxWithType<ItemObject>(SearchTypeIndex, _gameModuleObjectTypes);
            SearchTypeIndex = searchResults.Item1;
            SearchString = searchResults.Item2;
        }
        
        private void ManageTypes()
        {
            Label($"Changes here affect all <b><color=#99ffff>{ThisType}</color></b> objets of the same type. All {ThisType} objects of the same " +
                  "type must have the same <b><color=#99ffff>Dictionaries</color></b> structure, although each can have " +
                  "unique values for each Dictionary entry.", false, true, true);
            
            BlackLine();

            foreach(var type in _gameModuleObjectTypes)
                DrawTypeRow(type);
        }

        private void DrawTypeRow(string type)
        {
            if (GetShowDictionaries(type))
                StartVerticalBox();
            StartRow();
            ShowLinkToFolder(type);
            Label($"<b>{type}</b> <i><color=#555555>{_gameModuleObjectsOfType[type].Length} {ClassNamePlural}</color></i>", 250, false, false, true);
            DrawDictionaries(type);
            EndRow();
            
            DrawDictionaryDetails(type);
            if (GetShowDictionaries(type))
                EndVerticalBox();
        }

        private void DrawDictionaries(string type)
        {
            ColorsIf(GetShowDictionaries(type)
                , Color.green
                , Color.white
                , Color.white
                , Color.white);
            if (Button("Dictionaries Structure",150))
            {
                SetShowDictionaries(type, !GetShowDictionaries(type));
                ExitGUI();
            }
            ResetColor();
        }

        private void DrawDictionaryDetails(string type)
        {
            if (!GetShowDictionaries(type)) return;

            _gameModuleObjectsOfType[type][0].dictionaries.DrawStructure(_dictionariesDrawer, ThisType, type);
            Space();
        }

        private void ShowMainButtons() 
            => MainMenuIndex = ToolbarMenuMainTall(MainMenuButtonNames, MainMenuButtonToolTips
                , MainMenuIndex, 50, 18);
        
        private bool NoTypes()
        {
            if (!_gameModuleObjectTypes.Any())
            {
                ManageCreateNew();
                return true;
            }

            
            
            if (string.IsNullOrWhiteSpace(GetObjectType))
                SetObjectType(CacheGameModuleObjectTypes()[0]);
                    
            return false;
        }
        // -------------------------------

        // -------------------------------
        // CREATE NEW AND REQUIRED METHODS - UPDATE TYPES AS NEEDED
        // -------------------------------
        private void ManageCreateNew()
        {
            var moduleObjectTypes = _gameModuleObjectTypes;
            Header1($"Create {ClassNamePlural}");

            // Module Object names
            Space();
            Header2($"{ThisType} Names");
            Label($"Enter the names of the new <b><color=#99ffff>{ThisType}</color></b> objects you want to create. Separate each name with a comma.", false, true, true);
            NewObjectNames = TextArea(NewObjectNames);
            
            // Module Object Type
            Space();
            Header2($"{ThisType} Type");
            ShowTypeSelector();
            if (NewObjectTypeIndex == moduleObjectTypes.Length || GameModuleObjects().Length == 0)
            {
                StartRow();
                Label($"{ThisType} Type {symbolInfo}",
                    $"Each {ThisType} created will be placed in a directory called this, and this will be " +
                    $"the \"ObjectType\" of the {ThisType}. You can move {ThisType} objects to other types later if you'd like.", 150);
                NewObjectType = TextField(NewObjectType, 200);
                NewObjectType = TrimAndRemoveInvalidChars(NewObjectType);
                if (!TypeNameIsAllowed(NewObjectType))
                    Label("<color=#ff5555><i>Invalid type name!</i></color>", 200, false, false, true);
                EndRow();
                
                ShowSaveLocation();
            }
            
            var typeName = "[Choose new type]";
            if (NewObjectTypeIndex < moduleObjectTypes.Length)
                typeName = moduleObjectTypes[NewObjectTypeIndex];
            else if (!string.IsNullOrWhiteSpace(NewObjectType))
                typeName = NewObjectType;
            
            // Create button
            Space();
            var canCreate = TypeNameIsAllowed(typeName) && NamesInArray(NewObjectNames) > 0
                                                        && !(string.IsNullOrWhiteSpace(NewObjectType) &&
                                                             NewObjectTypeIndex == moduleObjectTypes.Length);
            ColorsIf(canCreate, Color.white, Color.black, Color.white, Color.grey);

            
            
            if (Button($"Create {NamesInArray(NewObjectNames)} {ClassNamePlural} of type {typeName}", 300, 50))
            {
                if (canCreate)
                {
                    var newObjectType = CreateAssets<ItemObject>(FinalDestination, NewObjectType, NewObjectTypeIndex, NewObjectsNameArray(NewObjectNames));
                    GameModuleObjectsOfType<ItemObject>(newObjectType, true);
                    NewObjectNames = ""; // Clear this value
                    MainMenuIndex = MainMenuButtonNames.ToList().IndexOf(ClassNamePlural);
                    EditorPrefs.SetBool($"{ClassNamePlural} Manager Cache", true);
                    SearchTypeIndex = GameModuleObjectTypes<ItemObject>(true).ToList().IndexOf(newObjectType);
                    GameModuleObjectsOfType<ItemObject>(newObjectType, true);
                    ExitGUI();
                    /*
                    var newObjectType = CreateAssets<ItemObject>(FinalDestination, NewObjectType, NewObjectTypeIndex, NewObjectsNameArray(NewObjectNames));
                    NewObjectNames = "";
                    MainMenuIndex = MainMenuButtonNames.ToList().IndexOf(ClassNamePlural);
                    EditorPrefs.SetBool($"{ThisType} Manager Cache", true);
                    
                    SearchTypeIndex = CacheGameModuleObjectTypes().ToList().IndexOf(newObjectType);
                    ExitGUI();
                    */
                }
            }
        }

        private void ShowSaveLocation()
        {
            StartRow();
            if (Button($"{symbolRecycle}", 25))
                SaveLocation = _defaultSaveLocation;
            if (Button("Choose Location", $"{ThisType} objects are saved in directories based on their type. The \"ObjectType\" " +
                                        $"is always the name of the directory the {ThisType} is in. Choose which directory should " +
                                        $"hold your new {ThisType} object types.", 150))
            {
                var relativePath = GetRelativePathFromUserSelection();
                if (relativePath != null)
                    SaveLocation = relativePath;
            }
            Label($"{SaveLocation}");
            EndRow();
        }
        
        private void ShowTypeSelector()
        {
            var moduleObjectTypes = _gameModuleObjectTypes;
            if (moduleObjectTypes.Length == 0)
                return;
            
            var typesList = moduleObjectTypes.ToList(); 
            typesList.Add("New Type...");
            if (NewObjectTypeIndex > typesList.Count)
                NewObjectTypeIndex = 0;
            
            NewObjectTypeIndex = Popup(NewObjectTypeIndex, typesList.ToArray(), 200);
        }
        // -------------------------------
    }
}
