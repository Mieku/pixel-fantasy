using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using static InfinityPBR.Modules.GameModuleUtilities;
using static InfinityPBR.InfinityEditor;
using Object = UnityEngine.Object;
using static InfinityPBR.Modules.InfinityEditorGameModules;

namespace InfinityPBR.Modules
{
    public class QuestRewardsManager
    {
        // -------------------------------
        // UPDATE THESE FOR THIS TYPE
        // -------------------------------
        private QuestRewardsDrawer _drawer = ScriptableObject.CreateInstance<QuestRewardsDrawer>();
        const string ThisType = "Quest Reward";
        const string ThisTypePlural = "Quest Rewards";
        private string ClassNamePlural => "QuestRewards";
        private string ClassName => "QuestReward";
        private string[] GameModuleObjectTypes(bool recompute = false) => GameModuleObjectTypes<QuestReward>(recompute);
        private QuestReward[] GameModuleObjects(bool recompute = false) => GameModuleObjects<QuestReward>(recompute);
        private QuestReward[] GameModuleObjectsOfType(string type, bool recompute = false) => GameModuleObjectsOfType<QuestReward>(type, recompute);
        private string FinalDestination => LocationToSave<QuestReward>(NewObjectTypeIndex, SaveLocation, NewObjectType);
        
        string[] MainMenuButtonNames = { ThisTypePlural, "Types", "Create" };
        string[] MainMenuButtonToolTips = { $"Manage individual {ThisType} values."
            , $"Update the settings and values that apply to all {ThisTypePlural} of a specific type."
            , $"Create new {ThisType} objects." };
        // -------------------------------
        
        // -------------------------------
        // REQUIRED PROPERTIES - NO UPDATE NEEDED
        // -------------------------------
        private List<QuestCondition> _questConditionClasses = new List<QuestCondition>();
        private List<QuestReward> _questRewardClasses = new List<QuestReward>();
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
        
        private int NewObjectQuestRewardIndex
        {
            get => EditorPrefs.GetInt($"New Object Quest Reward Index {ThisType}", 0);
            set => EditorPrefs.SetInt($"New Object Quest Reward Index {ThisType}", value);
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
            GameModuleObjectTypes<QuestReward>(true);
            GameModuleObjects<QuestReward>(true);
            SetBool($"{ThisType} Manager Cache", false);
            CheckObjectNamesAndTypes(GameModuleObjects());
            
            
            // Quest Condition Types
            var questConditionClassTypes = GetSubClassesOfQuestCondition();
            _questConditionClasses = new List<QuestCondition>();
            foreach (var type in questConditionClassTypes)
            {
                var drawerInstance = ScriptableObject.CreateInstance(type) as QuestCondition;
                _questConditionClasses.Add(drawerInstance);
            }
            
            // Quest Reward Types
            var questRewardClassTypes = GetSubClassesOfQuestReward();
            _questRewardClasses = new List<QuestReward>();
            foreach (var type in questRewardClassTypes)
            {
                var drawerInstance = ScriptableObject.CreateInstance(type) as QuestReward;
                _questRewardClasses.Add(drawerInstance);
            }

            // Save the type names for QuestConditionDrawers
            int i = 0;
            foreach (var drawer in _questConditionClasses)
            {
                EditorPrefs.SetString($"QuestConditionDrawerType{i}", drawer.GetType().ToString());
                i++;
            }

            EditorPrefs.SetInt("QuestConditionDrawerTypes", i);

            // Save the type names for QuestRewardDrawers
            i = 0;
            foreach (var drawer in _questRewardClasses)
            {
                EditorPrefs.SetString($"QuestRewardDrawerType{i}", drawer.GetType().ToString());
                i++;
            }
            
            EditorPrefs.SetInt("QuestRewardDrawerTypes", i);
            
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
            
            if (MainMenuSelected == "Quest Rewards")
                ManageObjects();
            
            SetAllDirty<QuestReward>();
        }

        private void ManageObjects()
        {
            DrawSearchBox();
            DrawResults();
        }

        private void DrawResults()
        {
            foreach(var moduleObject in SearchResults<QuestReward>(SearchTypeIndex, SearchString))
            {
                // UNIQUE FOR QUEST CONDITIONS & REWARDS
                EditorPrefs.SetBool($"Editor Show Quest Reward {moduleObject.name}", moduleObject.showInManager);
                if (!moduleObject.showInManager) // NOTE ADD THIS TO MODULE OBJECT CLASSES
                {
                    moduleObject.drawnByGameModulesManager = false; // UNIQUE FOR QUEST CONDITIONS & REWARDS
                    ShowOff(moduleObject);
                }
                else
                {
                    moduleObject.drawnByGameModulesManager = true; // UNIQUE FOR QUEST CONDITIONS & REWARDS
                    ShowOn(moduleObject);
                }
                EditorUtility.SetDirty(moduleObject);
            }
        }

        private void ShowObjType(QuestReward moduleObject)
        {
            var type = moduleObject.GetType().ToString().Replace("InfinityPBR.Modules.", "");
            Label($"<color=#777777><i>{type}</i></color>", 200, false, false, true);
        }

        private void ShowOff(QuestReward moduleObject)
        {
            StartRow();
            moduleObject.showInManager = OnOffButton(moduleObject.showInManager);
            ShowLinkButton(moduleObject);
            
            Label($"<b>{moduleObject.ObjectName}</b> <color=#777777><i>{moduleObject.ObjectType}</i></color>", 350, false, false, true);
            
            // More here
            ShowObjType(moduleObject);
            
            
            EndRow();
        }

        private void ShowOn(QuestReward moduleObject)
        {
            StartVerticalBox();
            StartRow();
            moduleObject.showInManager = OnOffButton(moduleObject.showInManager);
            ShowLinkButton(moduleObject);
            if (!moduleObject.showInManager && KeyShift)
                CloseAllInManager<QuestReward>();
            
            
            Label($"<b>{moduleObject.ObjectName}</b> <color=#777777><i>{moduleObject.ObjectType}</i></color>", 350, false, false, true);
            // More here
            ShowObjType(moduleObject);
            
            EndRow();

            _drawer.SetModulesObject(moduleObject);
            _drawer.Draw();
            
            EndVerticalBox();
        }

        

        private void DrawSearchBox()
        {
            var searchResults = SearchBoxWithType<QuestReward>(SearchTypeIndex, GameModuleObjectTypes());
            SearchTypeIndex = searchResults.Item1;
            SearchString = searchResults.Item2;
        }
        
        private void ManageTypes()
        {
            Label("<b><color=#99ffff>Quest Conditions</color></b> & <b><color=#99ffff>Quest Rewards</color></b> have <b><color=#99ffff>Dictionaries</color></b>, as do all <b><color=#99ffff>GameModulesScriptableObject</color></b> objects, " +
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
            Label($"<b>{type}</b> <i><color=#555555>{GameModuleObjectsOfType(type).Length} {ClassNamePlural}</color></i>", 350, false, false, true);
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
            Header2($"{ThisType} Type & Class"); // --- MODIFIED FOR QUEST CONDITIONS AND QUEST REWARDS ---
            Label(
                "<i><color=#ff9999><b>WARNING:</b> The class type can not be changed once created. Objects created with the wrong class type will need to be deleted prior to recreating them.</color></i>",
                false, true, true);
            StartRow(); // --- UNIQUE FOR QUEST CONDITIONS AND QUEST REWARDS ---
            ShowTypeSelector();
            ShowClassSelector(); // --- UNIQUE FOR QUEST CONDITIONS AND QUEST REWARDS ---
            EndRow(); // --- UNIQUE FOR QUEST CONDITIONS AND QUEST REWARDS ---
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

            
            
            if (ButtonWordWrap($"Create {NamesInArray(NewObjectNames)} {ClassNamePlural} of type {typeName}", 300, 50))
            {
                if (canCreate)
                {
                    // March 15, 2024
                    // IMPORTANT: Quest Condition & Quest Reward use a different method to create new objects, due to 
                    // the need to select a type of Quest Condition or Quest Reward. This is unique to these two classes.
                    
                    var isExistingType = NewObjectTypeIndex < moduleObjectTypes.Length;
                    if (!isExistingType && NewObjectType != null)
                        isExistingType = GameModuleObjectTypes().Contains(NewObjectType);
                    

                    var selectedTypeName = ClassTypes()[NewObjectQuestRewardIndex];
                    string assemblyQualifiedName = $"{selectedTypeName}, Assembly-CSharp";
                    Type objectType = Type.GetType(assemblyQualifiedName, true);

                    MethodInfo method = typeof(GameModuleUtilities).GetMethod("CreateAssets");
                    MethodInfo genericMethod = method.MakeGenericMethod(objectType);

                    var newObjectType = (string)genericMethod.Invoke(this, new object[] { FinalDestination, NewObjectType, NewObjectTypeIndex, NewObjectsNameArray(NewObjectNames) });
                    
                    
                    //var newObjectType = CreateAssets<QuestCondition>(FinalDestination, NewObjectType, NewObjectTypeIndex, NewObjectsNameArray(NewObjectNames));
                    if (isExistingType)
                    {
                        // Do something to keep them all aligned
                    }
                    NewObjectNames = "";
                    MainMenuIndex = MainMenuButtonNames.ToList().IndexOf(ClassNamePlural);
                    EditorPrefs.SetBool($"{ThisType} Manager Cache", true);
                    SearchTypeIndex = GameModuleObjectTypes(true).ToList().IndexOf(newObjectType);
                    ExitGUI();
                    
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

        private List<string> ClassTypes()
        {
            var typeList = new List<string>();
            var i = 0;
            while (i < EditorPrefs.GetInt("QuestRewardDrawerTypes"))
            {
                typeList.Add(EditorPrefs.GetString($"QuestRewardDrawerType{i}"));
                i++;
            }

            return typeList;
        }
        
        private void ShowClassSelector()
        {
            if (!EditorPrefs.HasKey("QuestRewardDrawerTypes"))
            {
                Label("No Quest Reward Types Found.");
                return;
            }

            var typeList = ClassTypes();

            if (typeList.Count == 0)
                return;
            
            var typeArray = typeList.ToArray();
            var displayList = new List<string>();
            foreach (var type in typeArray)
                displayList.Add(type.Replace("InfinityPBR.Modules.",""));
            
            if (NewObjectQuestRewardIndex > typeArray.Length)
                NewObjectQuestRewardIndex = 0;
            
            NewObjectQuestRewardIndex = Popup(NewObjectQuestRewardIndex, displayList.ToArray(), 250);
        }

        public static IEnumerable<Type> GetSubClassesOfQuestCondition()
        {
            var assembly = Assembly.GetAssembly(typeof(QuestCondition));
            var types = assembly.GetTypes();
            var subClasses = types.Where(t => t.IsSubclassOf(typeof(QuestCondition)) && !t.IsAbstract);
            return subClasses;
        }

        public static IEnumerable<Type> GetSubClassesOfQuestReward()
        {
            var assembly = Assembly.GetAssembly(typeof(QuestReward));
            var types = assembly.GetTypes();
            var subClasses = types.Where(t => t.IsSubclassOf(typeof(QuestReward)) && !t.IsAbstract);
            return subClasses;
        }
        // -------------------------------
    }
}
