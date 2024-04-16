using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static InfinityPBR.Modules.GameModuleUtilities;
using static InfinityPBR.InfinityEditor;
using static InfinityPBR.Modules.InfinityEditorGameModules;

namespace InfinityPBR.Modules
{
    public class ItemAttributesManager
    {
        // -------------------------------
        // UPDATE THESE FOR THIS TYPE
        // -------------------------------
        private ItemAttributesDrawer _drawer = ScriptableObject.CreateInstance<ItemAttributesDrawer>();
        const string ThisType = "Item Attribute";
        const string ThisTypePlural = "Item Attributes";
        private string ClassNamePlural => "ItemAttributes";
        private string ClassName => "ItemAttribute";
        private string[] GameModuleObjectTypes(bool recompute = false) => GameModuleObjectTypes<ItemAttribute>(recompute);
        private ItemAttribute[] GameModuleObjects(bool recompute = false) => GameModuleObjects<ItemAttribute>(recompute);
        private ItemAttribute[] GameModuleObjectsOfType(string type, bool recompute = false) => GameModuleObjectsOfType<ItemAttribute>(type, recompute);
        private string FinalDestination => LocationToSave<ItemAttribute>(NewObjectTypeIndex, SaveLocation, NewObjectType);
        
        string[] MainMenuButtonNames = { "Item Attributes", "Types", "Create" };
        string[] MainMenuButtonToolTips = { "Manage individual Item Attribute values."
            , "Update the settings and values that apply to all Item Attributes of a specific type."
            , "Create new Item Attribute objects." };
        // -------------------------------
        
        // -------------------------------
        // REQUIRED PROPERTIES - NO UPDATE NEEDED
        // -------------------------------
        private string MainMenuSelected
        {
            get
            {
                if (MainMenuIndex <= 0)
                    MainMenuIndex = 0;
                return MainMenuButtonNames[MainMenuIndex];
            }
        }

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
                var obj = GameModuleObjectsOfType(type)[0];
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
            
            CheckObjectNamesAndTypes(GameModuleObjects());
            GameModuleObjectTypes<ItemAttribute>(true);
            GameModuleObjects<ItemAttribute>(true);
            SetBool($"{ThisType} Manager Cache", false);
        }
        
        private void CheckForNullDrawer()
        {
            if (_dictionariesDrawer == null)
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
            
            if (MainMenuSelected == "Item Attributes")
                ManageObjects();
            
            SetAllDirty<ItemAttribute>();
        }

        private void ManageObjects()
        {
            DrawSearchBox();
            DrawResults();
        }
        
        private readonly string[] _quickViewOptions = {
            "General Settings", "Name Settings", "Required Attributes"
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
            ResultsHeaderGeneralSettings();
            ResultsHeaderNameSettings();
            ResultsHeaderRequiredAttributes();
            EndRow();
        }

        private void ResultsHeaderGeneralSettings()
        {
            if (QuickViewSelection != "General Settings") return;
            
            Label("<b>Distinct</b>", 50, false, false, true);
            Label("<b>Replace</b>", 50, false, false, true);
            Label("<b>Affects Actor</b>", 120, false, false, true);
        }
        
        private void ResultsGeneralSettings(ItemAttribute moduleObject)
        {
            if (QuickViewSelection != "General Settings") return;

            var tempDistinct = moduleObject.distinct;
            var tempReplace = moduleObject.replaceOthers;
            var tempAffectsActor = moduleObject.affectsActor;
            
            moduleObject.distinct = LeftCheck("", moduleObject.distinct, 50);
            moduleObject.replaceOthers = LeftCheck("", moduleObject.replaceOthers, 50);
            moduleObject.affectsActor = LeftCheck("", moduleObject.affectsActor, 25);
            Label($"<color=#777777><i>Affects {(moduleObject.affectsActor ? "Actor" : "ItemObject")}</i></color>", 120, false, false, true);

            if (tempDistinct != moduleObject.distinct)
                GameModuleObjectsOfType(moduleObject.objectType).ToList().ForEach(x => x.distinct = moduleObject.distinct);
            
            if (tempReplace != moduleObject.replaceOthers)
                GameModuleObjectsOfType(moduleObject.objectType).ToList().ForEach(x => x.replaceOthers = moduleObject.replaceOthers);
            
            if (tempAffectsActor != moduleObject.affectsActor)
                if (KeyShift) GameModuleObjectsOfType(moduleObject.objectType).ToList().ForEach(x => x.affectsActor = moduleObject.affectsActor);
            
        }

        private void ResultsHeaderNameSettings()
        {
            if (QuickViewSelection != "Name Settings") return;
            
            Label("<b>Human Name</b>", 150, false, false, true);
            Label("<b>Name Order</b>", 100, false, false, true);
        }
        
        private void ResultsNameSettings(ItemAttribute moduleObject)
        {
            if (QuickViewSelection != "Name Settings") return;
            
            var tempNameOrder = moduleObject.nameOrder;
            moduleObject.humanName = TextField(moduleObject.humanName, 150);
            moduleObject.nameOrder = DelayedInt(moduleObject.nameOrder, 20);

            if (tempNameOrder != moduleObject.nameOrder)
                if (KeyShift) GameModuleObjectsOfType(moduleObject.objectType).ToList().ForEach(x => x.nameOrder = moduleObject.nameOrder);
        }
        
        private void ResultsHeaderRequiredAttributes()
        {
            if (QuickViewSelection != "Required Attributes") return;
            
            Label("<b>Required Attributes</b>", 250, false, false, true);
        }
        
        private void ResultsRequiredAttributes(ItemAttribute moduleObject)
        {
            if (QuickViewSelection != "Required Attributes") return;

            var text = "";
            foreach (var attribute in moduleObject.requiredAttributes)
            {
                if (!string.IsNullOrWhiteSpace(text))
                    text = $"{text}, ";
                text = $"{text}{attribute.itemAttribute.ObjectName}";
            }
            
            Label($"<color=#999999>{text}</color>", 250, false, false, true);
        }

        private void DrawResults()
        {
            DrawResultsHeader();
            GreyLine(1, 0);
            foreach(var moduleObject in SearchResults<ItemAttribute>(SearchTypeIndex, SearchString))
            {
                if (!moduleObject.showInManager) // NOTE ADD THIS TO MODULE OBJECT CLASSES
                {
                    Undo.RecordObject(moduleObject, "Undo Value change");
                    ShowOff(moduleObject);
                }
                else
                    ShowOn(moduleObject);
                EditorUtility.SetDirty(moduleObject);
            }
        }
        
        private void ShowOff(ItemAttribute moduleObject)
        {
            StartRow();
            moduleObject.showInManager = OnOffButton(moduleObject.showInManager);
            ShowLinkButton(moduleObject);
            Label($"<b>{moduleObject.ObjectName}</b> <color=#777777><i>{moduleObject.ObjectType}</i></color>", 250, false, false, true);
            ResultsGeneralSettings(moduleObject);
            ResultsNameSettings(moduleObject);
            ResultsRequiredAttributes(moduleObject);
            // More here
            
            EndRow();
        }

        private void ShowOn(ItemAttribute moduleObject)
        {
            StartVerticalBox();
            StartRow();
            moduleObject.showInManager = OnOffButton(moduleObject.showInManager);
            ShowLinkButton(moduleObject);
            if (!moduleObject.showInManager && KeyShift)
                CloseAllInManager<ItemAttribute>();
            
            Label($"<b>{moduleObject.ObjectName}</b> <color=#777777><i>{moduleObject.ObjectType}</i></color>", 250, false, false, true);
            Undo.RecordObject(moduleObject, "Undo Value change");
            ResultsGeneralSettings(moduleObject);
            Undo.RecordObject(moduleObject, "Undo Value change");
            ResultsNameSettings(moduleObject);
            ResultsRequiredAttributes(moduleObject);
            EndRow();

            _drawer.SetModulesObject(moduleObject);
            _drawer.Draw();
            
            EndVerticalBox();
        }

        private void DrawSearchBox()
        {
            var searchResults = SearchBoxWithType<ItemAttribute>(SearchTypeIndex, GameModuleObjectTypes());
            SearchTypeIndex = searchResults.Item1;
            SearchString = searchResults.Item2;
        }
        
        private void ManageTypes()
        {
            Label($"Changes here affect all <b><color=#99ffff>{ThisType}</color></b> objets of the same type. All {ThisType} objects of the same " +
                  "type must have the same <b><color=#99ffff>Dictionaries</color></b> structure, although each can have " +
                  "unique values for each Dictionary entry.", false, true, true);
            
            BlackLine();

            foreach(var type in GameModuleObjectTypes())
                DrawTypeRow(type);
        }

        private void DrawTypeRow(string type)
        {
            if (GetShowDictionaries(type))
                StartVerticalBox();
            StartRow();
            ShowLinkToFolder(type);
            Label($"<b>{type}</b> <i><color=#555555>{GameModuleObjectsOfType(type).Length} {ClassNamePlural}</color></i>", 250, false, false, true);
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

            GameModuleObjectsOfType(type)[0].dictionaries.DrawStructure(_dictionariesDrawer, ThisType, type);
            Space();
        }

        private void ShowMainButtons() 
            => MainMenuIndex = ToolbarMenuMainTall(MainMenuButtonNames, MainMenuButtonToolTips
                , MainMenuIndex, 50, 18);

        private bool NoTypes()
        {
            if (!GameModuleObjectTypes().Any())
            {
                ManageCreateNew();
                return true;
            }

            if (string.IsNullOrWhiteSpace(GetObjectType))
                SetObjectType(GameModuleObjectTypes(true)[0]);
                    
            return false;
        }
        // -------------------------------

        // -------------------------------
        // CREATE NEW AND REQUIRED METHODS - UPDATE TYPES AS NEEDED
        // -------------------------------
        private void ManageCreateNew()
        {
            var moduleObjectTypes = GameModuleObjectTypes();
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
                    var isExistingType = NewObjectTypeIndex < moduleObjectTypes.Length;
                    if (!isExistingType && NewObjectType != null)
                        isExistingType = GameModuleObjectTypes().Contains(NewObjectType);
                    
                    var newObjectType = CreateAssets<ItemAttribute>(FinalDestination, NewObjectType, NewObjectTypeIndex, NewObjectsNameArray(NewObjectNames));
                    
                    if (isExistingType)
                    {
                        var distinct = GameModuleObjectsOfType(newObjectType)[0].distinct;
                        var replaces = GameModuleObjectsOfType(newObjectType)[0].replaceOthers;
                        var affectsActor = GameModuleObjectsOfType(newObjectType)[0].affectsActor;
                        foreach (var itemAttribute in GameModuleObjectsOfType(newObjectType, true))
                        {
                            itemAttribute.replaceOthers = replaces;
                            itemAttribute.distinct = distinct;
                            itemAttribute.affectsActor = affectsActor;
                        }
                    }
                    
                    GameModuleObjectsOfType<ItemAttribute>(newObjectType, true);
                    NewObjectNames = ""; // Clear this value
                    MainMenuIndex = MainMenuButtonNames.ToList().IndexOf(ClassNamePlural);
                    EditorPrefs.SetBool($"{ClassNamePlural} Manager Cache", true);
                    SearchTypeIndex = GameModuleObjectTypes<ItemAttribute>(true).ToList().IndexOf(newObjectType);
                    GameModuleObjectsOfType<ItemAttribute>(newObjectType, true);
                    
                    /*
                    var isExistingType = NewObjectTypeIndex < moduleObjectTypes.Length;
                    if (!isExistingType && NewObjectType != null)
                        isExistingType = GameModuleObjectTypes().Contains(NewObjectType);
                    var newObjectType = CreateAssets<ItemAttribute>(FinalDestination, NewObjectType, NewObjectTypeIndex, NewObjectsNameArray(NewObjectNames));
                    if (isExistingType)
                    {
                        var distinct = GameModuleObjectsOfType(newObjectType)[0].distinct;
                        var replaces = GameModuleObjectsOfType(newObjectType)[0].replaceOthers;
                        var affectsActor = GameModuleObjectsOfType(newObjectType)[0].affectsActor;
                        foreach (var itemAttribute in GameModuleObjectsOfType(newObjectType, true))
                        {
                            itemAttribute.replaceOthers = replaces;
                            itemAttribute.distinct = distinct;
                            itemAttribute.affectsActor = affectsActor;
                        }
                    }
                    NewObjectNames = "";
                    MainMenuIndex = MainMenuButtonNames.ToList().IndexOf(ClassNamePlural);
                    EditorPrefs.SetBool($"{ThisType} Manager Cache", true);
                    SearchTypeIndex = GameModuleObjectTypes(true).ToList().IndexOf(newObjectType);
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
            var moduleObjectTypes = GameModuleObjectTypes();
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
