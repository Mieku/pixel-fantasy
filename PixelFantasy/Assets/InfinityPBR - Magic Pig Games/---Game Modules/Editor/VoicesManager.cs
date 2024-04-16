using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static InfinityPBR.Modules.GameModuleUtilities;
using static InfinityPBR.InfinityEditor;
using static InfinityPBR.Modules.InfinityEditorGameModules;

namespace InfinityPBR.Modules
{
    public class VoicesManager
    {
        // -------------------------------
        // UPDATE THESE FOR THIS TYPE
        // -------------------------------
        private VoicesDrawer _drawer = ScriptableObject.CreateInstance<VoicesDrawer>();
        const string ThisType = "Voices";
        const string ThisTypePlural = "Voices";
        private string ClassNamePlural => "Voices";
        private string ClassName => "Voices";
        private string[] GameModuleObjectTypes(bool recompute = false) => GameModuleObjectTypes<Voices>(recompute);
        private Voices[] GameModuleObjects(bool recompute = false) => GameModuleObjects<Voices>(recompute);
        private Voices[] GameModuleObjectsOfType(string type, bool recompute = false) => GameModuleObjectsOfType<Voices>(type, recompute);
        private string FinalDestination => LocationToSave<Voices>(NewObjectTypeIndex, SaveLocation, NewObjectType);
        
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
        private readonly string _defaultSaveLocation = $"Game Module Objects/{ThisType}/";
       
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
            GameModuleObjectTypes<Voices>(true);
            GameModuleObjects<Voices>(true);
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
            
            if (MainMenuSelected == "Voices")
                ManageObjects();
            
            SetAllDirty<Voices>();
        }

        private void ManageObjects()
        {
            DrawSearchBox();
            DrawResults();
        }

        private void DrawResults()
        {
            foreach(var moduleObject in SearchResults<Voices>(SearchTypeIndex, SearchString))
            {
                if (!moduleObject.showInManager) // NOTE ADD THIS TO MODULE OBJECT CLASSES
                    ShowOff(moduleObject);
                else
                    ShowOn(moduleObject);
                EditorUtility.SetDirty(moduleObject);
            }
        }

        private void ShowOff(Voices moduleObject)
        {
            StartRow();
            moduleObject.showInManager = OnOffButton(moduleObject.showInManager);
            ShowLinkButton(moduleObject);
            Label($"<b>{moduleObject.ObjectName}</b> <color=#777777><i>{moduleObject.ObjectType}</i></color>", 250, false, false, true);
            
            // More here
            ShowVoices(moduleObject);
            ShowLines(moduleObject);
            ShowEmotions(moduleObject);
            
            EndRow();
        }

        private void ShowOn(Voices moduleObject)
        {
            StartVerticalBox();
            StartRow();
            moduleObject.showInManager = OnOffButton(moduleObject.showInManager);
            ShowLinkButton(moduleObject);
            if (!moduleObject.showInManager && KeyShift)
                CloseAllInManager<Voices>();
            
            Label($"<b>{moduleObject.ObjectName}</b> <color=#777777><i>{moduleObject.ObjectType}</i></color>", 250, false, false, true);
            // More here
            ShowVoices(moduleObject);
            ShowLines(moduleObject);
            ShowEmotions(moduleObject);
            
            EndRow();

            _drawer.SetModulesObject(moduleObject);
            _drawer.Draw();
            
            EndVerticalBox();
        }

        private void ShowEmotions(Voices moduleObject)
        {
            Label($"<color=#999999><b>{moduleObject.emotionNames.Count}</b></color> <color=#777777>Emotions</color>"
                , 80, false, false, true);
        }

        private void ShowLines(Voices moduleObject)
        {
            Label($"<color=#999999><b>{moduleObject.lineNames.Count}</b></color> <color=#777777>Lines</color>"
                , 80, false, false, true);
        }

        private void ShowVoices(Voices moduleObject)
        {
            Label($"<color=#999999><b>{moduleObject.voices.Count}</b></color> <color=#777777>Voices</color>"
                , 80, false, false, true);
        }


        private void DrawSearchBox()
        {
            var searchResults = SearchBoxWithType<Voices>(SearchTypeIndex, GameModuleObjectTypes());
            SearchTypeIndex = searchResults.Item1;
            SearchString = searchResults.Item2;
        }
        
        private void ManageTypes()
        {
            Label("<b><color=#99ffff>Voices</color></b> objects have <b><color=#99ffff>Dictionaries</color></b>, as do all <b><color=#99ffff>ModulesScriptableObject</color></b> objects, " +
                  "but they are not exposed via the Editor, or used as part of the Game Modules system.", false, true, true);
            
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
            //DrawDictionaries(type);
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
                    
                    var newObjectType = CreateAssets<Voices>(FinalDestination, NewObjectType, NewObjectTypeIndex, NewObjectsNameArray(NewObjectNames));
                    
                    if (isExistingType)
                    {
                        // Do something to keep them all aligned
                    }
                    
                    GameModuleObjectsOfType<Voices>(newObjectType, true);
                    NewObjectNames = ""; // Clear this value
                    MainMenuIndex = MainMenuButtonNames.ToList().IndexOf(ClassNamePlural);
                    EditorPrefs.SetBool($"{ClassNamePlural} Manager Cache", true);
                    SearchTypeIndex = GameModuleObjectTypes<Voices>(true).ToList().IndexOf(newObjectType);
                    GameModuleObjectsOfType<Voices>(newObjectType, true);
                    
                    /*
                    var isExistingType = NewObjectTypeIndex < moduleObjectTypes.Length;
                    if (!isExistingType && !string.IsNullOrWhiteSpace(NewObjectType))
                        isExistingType = GameModuleObjectTypes().Contains(NewObjectType);
                    var newObjectType = CreateAssets<Voices>(FinalDestination, NewObjectType, NewObjectTypeIndex, NewObjectsNameArray(NewObjectNames));
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
