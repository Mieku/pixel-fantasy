using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static InfinityPBR.Modules.GameModuleUtilities;
using static InfinityPBR.Modules.Utilities;
using static InfinityPBR.InfinityEditor;
using Button = UnityEngine.UI.Button;
using static InfinityPBR.Modules.InfinityEditorGameModules;

namespace InfinityPBR.Modules
{
    public class StatsManager
    {
        // -------------------------------
        // UPDATE THESE FOR THIS TYPE
        // -------------------------------
        const string ThisType = "Stat";
        const string ThisTypePlural = "Stats";
        private string ClassNamePlural => "Stats";
        private string ClassName => "Stat";
        private string[] GameModuleObjectTypes(bool recompute = false) => GameModuleObjectTypes<Stat>(recompute);
        // -------------------------------
        
        
        private StatDrawer _drawer = ScriptableObject.CreateInstance<StatDrawer>();

        private Stat[] GameModuleObjects(bool recompute = false) => GameModuleObjects<Stat>(recompute);

        private Stat[] GameModuleObjectsOfType(string type, bool recompute = false) =>
            GameModuleObjectsOfType<Stat>(type, recompute);

        private string StatType => GetString("Stat Manager Stat Type");
        private string FinalDestination => LocationToSave<Stat>(NewObjectTypeIndex, SaveLocation, NewObjectType);

        string[] MainMenuButtonNames = { "Stats", "Types", "Create", "Code" };

        string[] MainMenuButtonToolTips =
        {
            "Manage individual Stat values.",
            "Update the settings and values that apply to all Stats of a specific type.", "Create new Stat objects."
        };

        private string MainMenuSelected => MainMenuButtonNames[MainMenuIndex];

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

        private bool GetShowDictionaries(string type) => EditorPrefs.GetBool($"Stat Search String {type}", false);

        private void SetShowDictionaries(string type, bool value) =>
            EditorPrefs.SetBool($"Stat Search String {type}", value);

        private const string StatManagerSaveLocationKey = "Stat Manager Save Location";
        private const string DefaultSaveLocation = "Game Module Objects/Stats/";

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

        private string SaveLocation
        {
            get => string.IsNullOrWhiteSpace(EditorPrefs.GetString(StatManagerSaveLocationKey, DefaultPathValue()))
                ? DefaultPathValue()
                : EditorPrefs.GetString(StatManagerSaveLocationKey, DefaultSaveLocation);
            set => SetString(StatManagerSaveLocationKey, value);
        }

        private string DefaultPathValue()
        {
            var statObjects = GameModuleObjects<Stat>();
            if (statObjects.Length == 0)
                return DefaultSaveLocation;

            var firstStat = statObjects[0];
            var path = AssetDatabase.GetAssetPath(firstStat);
            path = path.Replace($"{firstStat.ObjectType}/{firstStat.name}.asset", "");
            path = path.Replace("Assets/", "");

            return path;
        }

        private int _fieldWidth = 200;
        public DictionariesDrawer dictionariesDrawer;

        private void SetType(string newType) => SetString("Stat Manager Stat Type", newType);

        private void ResetCache()
        {
            if (!EditorPrefs.GetBool("Stats Manager Cache", false)) return;

            GameModuleObjectTypes<Stat>(true);
            GameModuleObjects<MasteryLevels>(true);
            SetBool("Stats Manager Cache", false);
            CheckObjectNamesAndTypes(GameModuleObjects());
            SetBool("statModificationLevelDrawer Force Cache", true);
        }

        private void SetDictionaryDrawer() => dictionariesDrawer = new DictionariesDrawerEditor(_fieldWidth);

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

            if (MainMenuSelected == "Stats")
                ManageStats();
            
            if (MainMenuSelected == "Code")
                ManageCode();

            SetAllDirty<Stat>();
        }

        private int _copiedCodeTimer = 0;
        private string _actorName = "actor";
        
        private void ManageCode()
        {
            Header1("Code");
            GreyLine();
            Label("<color=#999999>I like using <b><color=#99ffff>Properties</color></b> in my code, so here is a nice way to copy a ton of property code which you " +
                  "can paste into your base <b>\"Character\"</b> class. The <b>\"Stat\"</b> versions of each will automatically " +
                  "add the Stat to the <b>GameStatList</b> on your actor if it doesn't already have it.\n\n" +
                  "Important: You should ensure you have an actor field which is of type <b><color=#99ffff>\"GameModulesActors\"</color></b>, " +
                  "<b><color=#99ffff>\"GameModulesInventoryActor\"</color></b>, or your own actor class which <b><color=#99ffff>inherits</color></b> from one of those. The code " +
                  "below expects there to be an <b><color=#99ffff>\"actor\"</color></b> field. <color=#777777><i>(You can select a custom name below.)</i></color>\n\nThe number of Properties per Stat will vary, " +
                  "depending on the kind of Stat it is, i.e. a \"Counter\" which can not be trained or modified, will" +
                  " only have Properties for the Stat object and \"Points\".</color>", false, true, true);
            GreyLine();
            StartRow();
            Label("Your Actor class name", 200);
            _actorName = TextField(_actorName, 100);
            EndRow();
            TextArea(StatsPropertyCode(), 100);
            StartRow();
            if (Button("Copy", 75))
            {
                _copiedCodeTimer = 100;
                EditorGUIUtility.systemCopyBuffer = StatsPropertyCode();
            }

            if (_copiedCodeTimer > 0)
            {
                Label("<color=#99ff99>Copied!</color>", 150, false, false, true);
                _copiedCodeTimer -= 1;
            }
            EndRow();
        }

        private string StatsPropertyCode()
        {
            var properties = "";
            foreach(var stat in GameModuleObjects<Stat>())
            {
                if (properties != "")
                    properties += "\n";
                // GameStat
                properties += $"public GameStat {stat.ObjectName.SafeString()}Stat => " +
                              $"{_actorName}.stats.Get(Properties.Stats.{stat.ObjectType.SafeString()}" +
                              $".{stat.ObjectName.SafeString()}, true);\n";
                // Points
                properties += $"public float {stat.ObjectName.SafeString()}Points => " +
                              $"{stat.ObjectName.SafeString()}Stat.Points;\n";
                if (stat.canBeTrained || stat.canBeModified) // Skip the rest if it's a Counter
                {
                    // Final Value
                    properties += $"public float {stat.ObjectName.SafeString()}Value => " +
                                  $"{stat.ObjectName.SafeString()}Stat.FinalValue;\n";
                    // Final Proficiency
                    properties += $"public float {stat.ObjectName.SafeString()}Proficiency => " +
                                  $"{stat.ObjectName.SafeString()}Stat.FinalProficiency;\n";
                }

                // Final Stat
                properties += $"public float {stat.ObjectName.SafeString()}(bool recompute = false) => " +
                              $"{stat.ObjectName.SafeString()}Stat.FinalStat(recompute);\n";
            }

            return properties;
        }
        
        /*
         * public float MaxHealth(bool recompute = false) => MaxHealthStat.FinalStat(recompute);
        public float Health => HealthStat.Points;
         */

        private void ManageStats()
        {
            DrawSearchBox();
            DrawResults();
        }

        private readonly string[] _quickViewOptions = {
            "Stat Type", "Start Values", "Min-Max Points", "Min-Max Base Value", "Min-Max Base Proficiency", "Min-Max Final Stat"
        };
        
        private string QuickViewSelection
        {
            get => EditorPrefs.GetString($"Quick View Selection {ThisType}", _quickViewOptions[0]);
            set => SetString($"Quick View Selection {ThisType}", value);
        }

        private int QuickViewIndex => _quickViewOptions.ToList().IndexOf(QuickViewSelection);

        private void DrawResultsHeader()
        {
            StartRow();
            ShowQuickView();
            ResultsHeaderStatType();
            ResultsHeaderStartValues();
            ResultsHeaderMinMaxValues();
            EndRow();
        }

        private void ResultsHeaderMinMaxValues()
        {
            if (QuickViewSelection != "Min-Max Points" 
                && QuickViewSelection != "Min-Max Base Value"
                && QuickViewSelection != "Min-Max Base Proficiency"
                && QuickViewSelection != "Min-Max Final Stat") return;

            Label("", 50);
            Label("<b>Min</b>", 50, false, false, true);
            Label("<b>Value</b>", 50, false, false, true);
            Label("<b>Max</b>", 50, false, false, true);
            Label("<b>Value</b>", 50, false, false, true);
            
            if (QuickViewSelection == "Min-Max Final Stat")
                Label("<b>Round</b>", 50, false, false, true);
        }
        
        private void ShowMinMaxValues(Stat stat)
        {
            if (QuickViewSelection != "Min-Max Points" 
                && QuickViewSelection != "Min-Max Base Value"
                && QuickViewSelection != "Min-Max Base Proficiency"
                && QuickViewSelection != "Min-Max Final Stat") return;

            var tempMinPointsType = stat.minPointsType;
            var tempMaxPointsType = stat.maxPointsType;
            var tempMinBaseValueType = stat.minBaseValueType;
            var tempMaxBaseValueType = stat.maxBaseValueType;
            var tempMinBaseProficiencyType = stat.minBaseProficiencyType;
            var tempMaxBaseProficiencyType = stat.maxBaseProficiencyType;
            var tempMinFinalType = stat.minFinalType;
            var tempMaxFinalType = stat.maxFinalType;
            var tempRounding = stat.roundingMethod;
            
            if (QuickViewSelection == "Min-Max Points")
            {
                Label("<color=#999999><i>Points</i></color>", 50, false, false, true);
                stat.minPointsType = Popup(stat.minPointsType, stat.minMaxOptions, 50);
                if (stat.minPointsType == 0)
                    Label("", 50);
                else if (stat.minPointsType == 1)
                    stat.minPoints = Float(stat.minPoints, 50);
                else
                    stat.minPointsStat = Object(stat.minPointsStat, typeof(Stat), 50) as Stat;
                stat.maxPointsType = Popup(stat.maxPointsType, stat.minMaxOptions, 50);
                if (stat.maxPointsType == 0)
                    Label("", 50);
                else if (stat.maxPointsType == 1)
                    stat.maxPoints = Float(stat.maxPoints, 50);
                else
                    stat.maxPointsStat = Object(stat.maxPointsStat, typeof(Stat), 50) as Stat;
            }


            if (QuickViewSelection == "Min-Max Base Value")
            {
                Label("<color=#999999><i>Value</i></color>", 50, false, false, true);
                stat.minBaseValueType = Popup(stat.minBaseValueType, stat.minMaxOptions, 50);
                if (stat.minBaseValueType == 0)
                    Label("", 50);
                else if (stat.minBaseValueType == 1)
                    stat.minBaseValue = Float(stat.minBaseValue, 50);
                else
                    stat.minBaseValueStat = Object(stat.minBaseValueStat, typeof(Stat), 50) as Stat;
                stat.maxBaseValueType = Popup(stat.maxBaseValueType, stat.minMaxOptions, 50);
                if (stat.maxBaseValueType == 0)
                    Label("", 50);
                else if (stat.maxBaseValueType == 1)
                    stat.maxBaseValue = Float(stat.maxBaseValue, 50);
                else
                    stat.maxBaseValueStat = Object(stat.maxBaseValueStat, typeof(Stat), 50) as Stat;
            }

            if (QuickViewSelection == "Min-Max Base Proficiency")
            {
                Label("<color=#999999><i>Prof.</i></color>", 50, false, false, true);
                stat.minBaseProficiencyType = Popup(stat.minBaseProficiencyType, stat.minMaxOptions, 50);
                if (stat.minBaseProficiencyType == 0)
                    Label("", 50);
                else if (stat.minBaseProficiencyType == 1)
                    stat.minBaseProficiency = Float(stat.minBaseProficiency, 50);
                else
                    stat.minBaseProficiencyStat = Object(stat.minBaseProficiencyStat, typeof(Stat), 50) as Stat;
                stat.maxBaseProficiencyType = Popup(stat.maxBaseProficiencyType, stat.minMaxOptions, 50);
                if (stat.maxBaseProficiencyType == 0)
                    Label("", 50);
                else if (stat.maxBaseProficiencyType == 1)
                    stat.maxBaseProficiency = Float(stat.maxBaseProficiency, 50);
                else
                    stat.maxBaseProficiencyStat = Object(stat.maxBaseProficiencyStat, typeof(Stat), 50) as Stat;
            }
            
            if (QuickViewSelection == "Min-Max Final Stat")
            {
                Label("<color=#999999><i>Final</i></color>", 50, false, false, true);
                stat.minFinalType = Popup(stat.minFinalType, stat.minMaxOptions, 50);
                if (stat.minFinalType == 0)
                    Label("", 50);
                else if (stat.minFinalType == 1)
                    stat.minFinal = Float(stat.minFinal, 50);
                else
                    stat.minFinalStat = Object(stat.minFinalStat, typeof(Stat), 50) as Stat;
                stat.maxFinalType = Popup(stat.maxFinalType, stat.minMaxOptions, 50);
                if (stat.maxFinalType == 0)
                    Label("", 50);
                else if (stat.maxFinalType == 1)
                    stat.maxFinal = Float(stat.maxFinal, 50);
                else
                    stat.maxFinalStat = Object(stat.maxFinalStat, typeof(Stat), 50) as Stat;
                stat.roundingMethod = (Rounding)EnumPopup(stat.roundingMethod, 50);
                if (stat.roundingMethod == Rounding.Round)
                    stat.decimals = Int(stat.decimals, 20);
            }

            if (KeyShift)
            {
                if (tempMinPointsType != stat.minPointsType)
                    GameModuleObjectsOfType(stat.objectType).ToList().ForEach(statOfType => statOfType.minPointsType = stat.minPointsType);
                if (tempMaxPointsType != stat.maxPointsType)
                    GameModuleObjectsOfType(stat.objectType).ToList().ForEach(statOfType => statOfType.maxPointsType = stat.maxPointsType);
                if (tempMinBaseValueType != stat.minBaseValueType)
                    GameModuleObjectsOfType(stat.objectType).ToList().ForEach(statOfType => statOfType.minBaseValueType = stat.minBaseValueType);
                if (tempMaxBaseValueType != stat.maxBaseValueType)
                    GameModuleObjectsOfType(stat.objectType).ToList().ForEach(statOfType => statOfType.maxBaseValueType = stat.maxBaseValueType);
                if (tempMinBaseProficiencyType != stat.minBaseProficiencyType)
                    GameModuleObjectsOfType(stat.objectType).ToList().ForEach(statOfType => statOfType.minBaseProficiencyType = stat.minBaseProficiencyType);
                if (tempMaxBaseProficiencyType != stat.maxBaseProficiencyType)
                    GameModuleObjectsOfType(stat.objectType).ToList().ForEach(statOfType => statOfType.maxBaseProficiencyType = stat.maxBaseProficiencyType);
                if (tempMinFinalType != stat.minFinalType)
                    GameModuleObjectsOfType(stat.objectType).ToList().ForEach(statOfType => statOfType.minFinalType = stat.minFinalType);
                if (tempMaxFinalType != stat.maxFinalType)
                    GameModuleObjectsOfType(stat.objectType).ToList().ForEach(statOfType => statOfType.maxFinalType = stat.maxFinalType);
                if (tempRounding != stat.roundingMethod)
                    GameModuleObjectsOfType(stat.objectType).ToList().ForEach(statOfType => statOfType.roundingMethod = stat.roundingMethod);
            }
        }
        
        private void ResultsHeaderStartValues()
        {
            if (QuickViewSelection != "Start Values") return;

            Label("<b>Points</b>", 50, false, false, true);
            Label("<b>Value</b>", 50, false, false, true);
            Label("<b>Prof.</b>", 50, false, false, true);
            Label("<b>Points Mod</b>", 120, false, false, true);
        }

        private void ResultsHeaderStatType()
        {
            if (QuickViewSelection != "Stat Type") return;
            
            Label("<b>Can be Trained</b>", 100, false, false, true);
            Label("<b>Can be Modified</b>", 100, false, false, true);
            Label("<b>Stat Type</b>", 150, false, false, true);
        }

        private void ShowQuickView()
        {
            if (!_quickViewOptions.ToList().Contains(QuickViewSelection))
                QuickViewSelection = _quickViewOptions[0];
            QuickViewSelection = _quickViewOptions[Popup(QuickViewIndex, _quickViewOptions, 250)];
            Label("", 25);
            Label("", 25);
        }

        private void DrawResults()
        {
            DrawResultsHeader();
            GreyLine(1, 0);
            foreach(var moduleObject in SearchResults<Stat>(SearchTypeIndex, SearchString))
            {
                if (moduleObject == null) continue;
                if (!moduleObject.showInManager)
                {
                    Undo.RecordObject(moduleObject, "Undo Value change");
                    ShowOff(moduleObject);
                }
                else
                    ShowOn(moduleObject);
                EditorUtility.SetDirty(moduleObject);
            }
        }
        
        private void ShowStatType(Stat stat)
        {
            if (QuickViewSelection != "Stat Type") return;

            var tempTrainable = stat.canBeTrained;
            var tempModifiable = stat.canBeModified;
            
            ColorsIf(stat.canBeTrained, Color.white, Color.black, Color.white, Color.grey);
            stat.canBeTrained = ButtonToggle(stat.canBeTrained, "Trainable", 100);
            ColorsIf(stat.canBeModified, Color.white, Color.black, Color.white, Color.grey);
            stat.canBeModified = ButtonToggle(stat.canBeModified, "Modifiable", 100);
            ResetColor();
            Label($"<color=#777777><i>{StatKind(stat)}</i></color>", 100, false, false, true);

            if (KeyShift)
            {
                if (tempTrainable != stat.canBeTrained)
                    GameModuleObjectsOfType(stat.objectType).ToList().ForEach(statOfType => statOfType.canBeTrained = stat.canBeTrained);
                if (tempModifiable != stat.canBeModified)
                    GameModuleObjectsOfType(stat.objectType).ToList().ForEach(statOfType => statOfType.canBeModified = stat.canBeModified);
            }

            ShowMasteryLevelsWarning(stat);
        }

        private void ShowMasteryLevelsWarning(Stat stat)
        {
            if (!stat.canBeTrained) return;
            if (stat.masteryLevels != null) return;
            ContentColor(Color.red);
            Label($"<b>{symbolImportant}</b>", "Mastery Levels are not set!", 100, false, false, true);
            ResetColor();
        }

        private void ShowStartValues(Stat stat)
        {
            if (QuickViewSelection != "Start Values") return;

            stat.points = Float(stat.points, 50);
            stat.baseValue = Float(stat.baseValue, 50);
            stat.baseProficiency = Float(stat.baseProficiency, 50);
            stat.modifyPointsByProficiency = Object(stat.modifyPointsByProficiency, typeof(Stat), 120) as Stat;
        }

        private void ShowOff(Stat stat)
        {
            StartRow();
            stat.showInManager = OnOffButton(stat.showInManager);
            ShowLinkButton(stat);
            Label($"<b>{stat.ObjectName}</b> <color=#777777><i>{stat.ObjectType}</i></color>", 250, false, false, true);
            ShowStatType(stat);
            ShowStartValues(stat);
            ShowMinMaxValues(stat);
            EndRow();
        }

        private void ShowOn(Stat stat)
        {
            StartVerticalBox();
            StartRow();
            stat.showInManager = OnOffButton(stat.showInManager);
            ShowLinkButton(stat);
            if (!stat.showInManager && KeyShift)
                CloseAllInManager<Stat>();
            
            Label($"<b>{stat.ObjectName}</b> <color=#777777><i>{stat.ObjectType}</i></color>", 250, false, false, true);
            EndRow();

            _drawer.SetModulesObject(stat);
            _drawer.Draw();
            
            EndVerticalBox();
        }

        private void DrawSearchBox()
        {
            var searchResults = SearchBoxWithType<Stat>(SearchTypeIndex, GameModuleObjectTypes<Stat>());
            SearchTypeIndex = searchResults.Item1;
            SearchString = searchResults.Item2;
        }

        private void ManageTypes()
        {
            Label("Changes here affect all <b><color=#99ffff>Stat</color></b> objets of the same type. All Stat objects of the same " +
                       "type must have the same <b><color=#99ffff>Mastery Levels</color></b> and <b><color=#99ffff>Dictionaries</color></b> structure, although each can have " +
                       "unique values for each Mastery Level and Dictionary entry.", false, true, true);
            
            BlackLine();

            foreach(var type in GameModuleObjectTypes<Stat>())
                DrawTypeRow(type);
        }

        private void DrawTypeRow(string type)
        {
            if (GetShowDictionaries(type))
                StartVerticalBox();
            StartRow();
            ShowLinkToFolder(type);
            Label($"<b>{type}</b> <i><color=#555555>{GameModuleObjectsOfType<Stat>(type).Length} Stats</color></i>", 250, false, false, true);
            DrawMasteryLevel(type);
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
            
            GameModuleObjectsOfType<Stat>(type)[0].dictionaries.DrawStructure(dictionariesDrawer, "Stats", type);
            Space();
        }

        private void DrawMasteryLevel(string type)
        {
            if (GameModuleObjects<MasteryLevels>().Length == 0)
            {
                Label("<color=#555555><i>No Mastery Levels found.</i></color>", 200, false, false, true);
                return;
            }

            MasteryLevelPopup(type);
        }
        
        private void MasteryLevelPopup(string type)
        {
            var masteryLevels = GameModuleObjects<MasteryLevels>();
            var statsOfThisType = GameModuleObjectsOfType<Stat>(type);
            if (statsOfThisType.Length  == 0)
            {
                Label($"<color=#ff5555><i>No Stats of type {type} found!</i></color>", 200, false, false, true);
                return;
            }
            var firstStat = statsOfThisType[0];
            
            var currentIndex = GetCurrentIndexMasteryLevel(masteryLevels, firstStat.masteryLevels);
            if (currentIndex < 0)
            {
                foreach (var stat in statsOfThisType)
                    stat.masteryLevels = masteryLevels[0];
                currentIndex = 0;
            }
            
            var tempIndex = currentIndex;
            currentIndex = Popup(currentIndex, GameModuleObjectNames<MasteryLevels>(), 200);
            if (tempIndex == currentIndex)
                return;
            
            if (Dialog("Change Mastery Level", "Are you sure you want to change the Mastery Level for all Stats of type " +
                                             $"{type} from {masteryLevels[tempIndex].name} to {masteryLevels[currentIndex].name}? This may " +
                                             "destroy any existing data on those Stats."))
            {
                //DoChecks(); // July 1, 2023 -- Not sure if we need this here now. We may. Leaving it in case there are errors.
                foreach (var stat in statsOfThisType)
                    stat.masteryLevels = masteryLevels[currentIndex];
            }
        }

        private void ShowMainButtons() 
            => MainMenuIndex = ToolbarMenuMainTall(MainMenuButtonNames, MainMenuButtonToolTips
                , MainMenuIndex, 50, 18);
        
        /*
        private void SetAllDirty(bool recompute = false) // December 16 2023 -- Changed this to false
        {
            foreach (var itemObject in GameModuleObjects<Stat>(recompute)) 
                EditorUtility.SetDirty(itemObject);
        }
        */

        private void CheckForNullDrawer()
        {
            if (dictionariesDrawer == null)
                SetDictionaryDrawer();
        }

        private bool NoTypes()
        {
            if (!GameModuleObjectTypes<Stat>().Any())
            {
                ManageCreateNew();
                return true;
            }

            if (string.IsNullOrWhiteSpace(StatType))
                SetType(GameModuleObjectTypes<Stat>(true)[0]);
                    
            return false;
        }
        
        
        private void ManageCreateNew()
        {
            var statTypes = GameModuleObjectTypes<Stat>();
            Header1("Create Stats");

            // Stat names
            Space();
            Header2("Stat Names");
            Label("Enter the names of the new <b><color=#99ffff>Stat</color></b> objects you want to create. Separate each name with a comma.", false, true, true);
            NewObjectNames = TextArea(NewObjectNames);
            
            // Stat Type
            Space();
            Header2("Stat Type");
            ShowTypeSelector();
            if (NewObjectTypeIndex == statTypes.Length || GameModuleObjects().Length == 0)
            {
                StartRow();
                Label($"Stat Type {symbolInfo}",
                    "Each stat created will be placed in a directory called this, and this will be " +
                    "the \"ObjectType\" of the stat. You can move stats to other types later if you'd like.", 150);

                NewObjectType = TextField(NewObjectType, 200);
                NewObjectType = TrimAndRemoveInvalidChars(NewObjectType);
                
                if (!TypeNameIsAllowed(NewObjectType))
                    Label("<color=#ff5555><i>Invalid type name!</i></color>", 200, false, false, true);
                EndRow();
                
                ShowSaveLocation();
            }
            
            var typeName = "[Choose new type]";
            if (NewObjectTypeIndex < statTypes.Length)
                typeName = statTypes[NewObjectTypeIndex];
            else if (!string.IsNullOrWhiteSpace(NewObjectType))
                typeName = NewObjectType;
            
            // Create button
            Space();

            var canCreate = TypeNameIsAllowed(typeName) && NamesInArray(NewObjectNames) > 0
                                                        && !(string.IsNullOrWhiteSpace(NewObjectType) &&
                                                             NewObjectTypeIndex == statTypes.Length);
            ColorsIf(canCreate, Color.white, Color.black, Color.white, Color.grey);

            if (Button($"Create {NamesInArray(NewObjectNames)} Stats of type {typeName}", 300, 50))
            {
                if (canCreate)
                {
                    var newObjectType = CreateAssets<Stat>(FinalDestination, NewObjectType, NewObjectTypeIndex, NewObjectsNameArray(NewObjectNames));
                    GameModuleObjectsOfType<Stat>(newObjectType, true);
                    NewObjectNames = ""; // Clear this value
                    MainMenuIndex = MainMenuButtonNames.ToList().IndexOf("Stats");
                    EditorPrefs.SetBool("Stats Manager Cache", true);
                    SearchTypeIndex = GameModuleObjectTypes<Stat>(true).ToList().IndexOf(newObjectType);
                    GameModuleObjectsOfType<Stat>(newObjectType, true);
                    ExitGUI();
                }
            }
        }

        private void ShowSaveLocation()
        {
            StartRow();
            if (Button($"{symbolRecycle}", 25))
                SaveLocation = DefaultSaveLocation;
            if (Button("Choose Location", "Stat objects are saved in directories based on their type. The \"ObjectType\" " +
                                          "is always the name of the directory the Stat is in. Choose which directory should " +
                                          "hold your new Stat object types.", 150))
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
            var statTypes = GameModuleObjectTypes<Stat>();
            if (statTypes.Length == 0)
                return;
            
            var statTypesList = statTypes.ToList(); 
            statTypesList.Add("New Type...");
            if (NewObjectTypeIndex > statTypesList.Count)
                NewObjectTypeIndex = 0;
            
            NewObjectTypeIndex = Popup(NewObjectTypeIndex, statTypesList.ToArray(), 200);
        }

        private int GetCurrentIndexMasteryLevel(MasteryLevels[] masteryLevelsArray, MasteryLevels masteryLevels)
        {
            for (var i = 0; i < masteryLevelsArray.Length; i++)
            {
                if (masteryLevelsArray[i] == masteryLevels)
                    return i;
            }

            return -1;
        }
    }
}
