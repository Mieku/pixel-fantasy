using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static InfinityPBR.Modules.GameModuleUtilities;
using static InfinityPBR.InfinityEditor;
using static InfinityPBR.Modules.InfinityEditorGameModules;

namespace InfinityPBR.Modules
{
    public class QuestsManager
    {
        // -------------------------------
        // UPDATE THESE FOR THIS TYPE
        // -------------------------------
        private QuestsDrawer _drawer = ScriptableObject.CreateInstance<QuestsDrawer>();
        const string ThisType = "Quest";
        const string ThisTypePlural = "Quests";
        private string ClassNamePlural => "Quests";
        private string ClassName => "Quest";
        private string[] GameModuleObjectTypes(bool recompute = false) => GameModuleObjectTypes<Quest>(recompute);
        private Quest[] GameModuleObjects(bool recompute = false) => GameModuleObjects<Quest>(recompute);
        private Quest[] GameModuleObjectsOfType(string type, bool recompute = false) => GameModuleObjectsOfType<Quest>(type, recompute);
        private string FinalDestination => LocationToSave<Quest>(NewObjectTypeIndex, SaveLocation, NewObjectType);
        
        string[] MainMenuButtonNames = { ThisTypePlural, "Types", "Create" };
        string[] MainMenuButtonToolTips = { $"Manage individual {ThisType} values."
            , $"Update the settings and values that apply to all {ThisTypePlural} of a specific type."
            , $"Create new {ThisType} objects." };
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

            SetDictionaryDrawer();
            GameModuleObjectTypes<Quest>(true);
            GameModuleObjects<Quest>(true);
            SetBool($"{ThisType} Manager Cache", false);
            CheckObjectNamesAndTypes(GameModuleObjects());
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
            
            if (MainMenuSelected == "Quests")
                ManageObjects();
            
            SetAllDirty<Quest>();
        }

        private void ManageObjects()
        {
            DrawSearchBox();
            DrawResults();
        }

        private void DrawResults()
        {
            foreach(var moduleObject in SearchResults<Quest>(SearchTypeIndex, SearchString))
            {
                if (!moduleObject.showInManager) // NOTE ADD THIS TO MODULE OBJECT CLASSES
                    ShowOff(moduleObject);
                else
                    ShowOn(moduleObject);
                EditorUtility.SetDirty(moduleObject);
            }
        }

        private void ShowOff(Quest moduleObject)
        {
            StartRow();
            moduleObject.showInManager = OnOffButton(moduleObject.showInManager);
            ShowLinkButton(moduleObject);
            Label($"<b>{moduleObject.ObjectName}</b> <color=#777777><i>{moduleObject.ObjectType}</i></color>", 220, false, false, true);
            
            // More here
            ShowQuestSteps(moduleObject);
            ShowBlackboard(moduleObject);
            ShowAuto(moduleObject);
            ShowHidden(moduleObject);
            ShowEndTime(moduleObject);
            //ShowConditions(moduleObject);
            
            EndRow();
        }
        
        private void ShowConditions(Quest moduleObject)
        {
            var text = "";
            text = $"{text}<color=#999999>{(moduleObject.HasSuccessConditions ? symbolCheck : symbolX)} Success Cond.</color>";
            text = $"{text}<color=#999999>, {(moduleObject.HasFailConditions ? symbolCheck : symbolX)} Fail Cond.</color>";
            Label($"{text}", 150, false, false, true);
        }
        
        private void ShowHidden(Quest moduleObject)
        {
            var text = "";
            if (moduleObject.hidden)
                text = $"{text}<color=#999999>Hidden</color>";
            Label($"{text}", $"This Quest is {(moduleObject.hidden ? "" : "not ")} marked \"Hidden\". This flag " +
                             "may help when building your UI.", 50, false, false, true);
        }
        
        private void ShowEndTime(Quest moduleObject)
        {
            var text = "";
            if (moduleObject.hasEndTime)
                text = $"{text}<color=#999999>{symbolCheck} End Time</color>";
            Label($"{text}", $"This Quest {(moduleObject.hasEndTime ? "has an" : "does not have a")} end time. " +
                             "A Quest with an end time will automatically resolve itself, or simply disappear if " +
                             "it is not marked auto-succeed or auto-fail.", 70, false, false, true);
        }
        
        private void ShowAuto(Quest moduleObject)
        {
            var text = "";
            var tooltip = "";
            if (moduleObject.autoSucceed && !moduleObject.autoFail)
            {
                if (!string.IsNullOrWhiteSpace(text))
                    text = $"{text}, ";
                text = $"{text}<color=#999999>{symbolRecycle} Succeed</color>";
                tooltip = "This will automatically succeed when all QuestSteps are complete. It will not automatically fail.";
            }
            if (moduleObject.autoFail && !moduleObject.autoSucceed)
            {
                if (!string.IsNullOrWhiteSpace(text))
                    text = $"{text}, ";
                text = $"{text}<color=#999999>{symbolRecycle} Fail</color>";
                tooltip = "This will automatically fail when at least one QuestStep has failed. It will not automatically succeed.";
            }
            if (moduleObject.autoFail && moduleObject.autoSucceed)
            {
                if (!string.IsNullOrWhiteSpace(text))
                    text = $"{text}, ";
                text = $"{text}<color=#999999>{symbolRecycle} Complete</color>";
                tooltip = "This will automatically succeed when all QuestSteps are complete or automatically fail when at least one QuestStep has failed";
            }
            Label($"{text}", tooltip, 80, false, false, true);
        }

        private void ShowBlackboard(Quest moduleObject)
        {
            var text = "";
            var tooltip = "";
            if (moduleObject.subscribeToBlackboard)
            {
                text = $"{text}<color=#999999>{symbolCheck} Blackboard</color>";
                tooltip = "This Quest will subscribe to the MainBlackboard.";
                if (moduleObject.queryEveryFrame)
                {
                    text = $"{text}<color=#999999> {symbolRecycle}</color>";
                    tooltip = $"{tooltip} It will query the Blackboard every frame.";
                }
            }
            Label($"{text}", tooltip, 100, false, false, true);
        }
        
        private void ShowQuestSteps(Quest moduleObject)
        {
            var text = "";
            var tooltip = "";
            text = $"{text}<color=#999999>{moduleObject.Steps} Step{(moduleObject.Steps != 1 ? "s": "")}</color>";
            tooltip = $"This Quest has {moduleObject.Steps} QuestSteps.";
            if (moduleObject.sequentialSteps)
            {
                text = $"{text} <color=#777777>Seq.</color>";
                tooltip = $"{tooltip} They will be resolved sequentially, meaning the first step must be completed before " +
                          "the second step will attempt to resolve, and so on.";
            }
            Label($"{text}", tooltip, 80, false, false, true);
        }

        private void ShowOn(Quest moduleObject)
        {
            StartVerticalBox();
            StartRow();
            moduleObject.showInManager = OnOffButton(moduleObject.showInManager);
            ShowLinkButton(moduleObject);
            if (!moduleObject.showInManager && KeyShift)
                CloseAllInManager<Quest>();
            
            Label($"<b>{moduleObject.ObjectName}</b> <color=#777777><i>{moduleObject.ObjectType}</i></color>", 250, false, false, true);
            // More here
            ShowQuestSteps(moduleObject);
            ShowBlackboard(moduleObject);
            ShowAuto(moduleObject);
            ShowHidden(moduleObject);
            ShowEndTime(moduleObject);
            
            EndRow();

            _drawer.SetModulesObject(moduleObject);
            _drawer.Draw();
            
            EndVerticalBox();
        }

        

        private void DrawSearchBox()
        {
            var searchResults = SearchBoxWithType<Quest>(SearchTypeIndex, GameModuleObjectTypes());
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
                    
                    var newObjectType = CreateAssets<Quest>(FinalDestination, NewObjectType, NewObjectTypeIndex, NewObjectsNameArray(NewObjectNames));
                    
                    if (isExistingType)
                    {
                        // Do something to keep them all aligned
                    }
                    
                    GameModuleObjectsOfType<Quest>(newObjectType, true);
                    NewObjectNames = ""; // Clear this value
                    MainMenuIndex = MainMenuButtonNames.ToList().IndexOf(ClassNamePlural);
                    EditorPrefs.SetBool($"{ClassNamePlural} Manager Cache", true);
                    SearchTypeIndex = GameModuleObjectTypes<Quest>(true).ToList().IndexOf(newObjectType);
                    GameModuleObjectsOfType<Quest>(newObjectType, true);
                    /*
                     
                    var isExistingType = NewObjectTypeIndex < moduleObjectTypes.Length;
                    if (!isExistingType && NewObjectType != null)
                        isExistingType = GameModuleObjectTypes().Contains(NewObjectType);
                    var newObjectType = CreateAssets<Quest>(FinalDestination, NewObjectType, NewObjectTypeIndex, NewObjectsNameArray(NewObjectNames));
                    if (isExistingType)
                    {
                        // Do something to keep them all aligned
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
