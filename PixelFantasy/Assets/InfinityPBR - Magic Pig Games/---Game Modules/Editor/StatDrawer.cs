using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static InfinityPBR.Modules.GameModuleUtilities;
using static InfinityPBR.Modules.Utilities;

namespace InfinityPBR.Modules
{
    public class StatDrawer : GameModulesDrawer
    {
        private Stat _modulesObject;
        private Vector2 _scrollPosition;
        private int _fieldWidth;
        private DictionariesDrawer _dictionariesDrawer;
        private StatModificationLevelDrawer _statModificationLevelDrawer;
        private string[] _menuBarOptions;

        public bool drawnByGameModulesWindow = true;

        public void SetModulesObject(Stat modulesObject, bool drawnByWindow = true)
        {
            _modulesObject = modulesObject;
            _menuBarOptions = new[] { "Main Settings", "Stat Effects", "Dictionaries" };
            _fieldWidth = 200;
            _dictionariesDrawer = new DictionariesDrawerEditor(_fieldWidth);
            _statModificationLevelDrawer = new StatModificationLevelDrawerEditor(_fieldWidth);
            drawnByGameModulesWindow = drawnByWindow;
        }

        private MasteryLevel ActiveMasteryLevel => _modulesObject.masteryLevels.levels[GetMasteryLevelIndex()];

        protected void DrawLinkToDocs()
        {
            if (drawnByGameModulesWindow) return;
            StartRow();

            BackgroundColor(Color.magenta);
            if (Button("Manage Stats"))
            {
                SetString("Game Modules Window Selected", "Stats");
                EditorWindow.GetWindow(typeof(EditorWindowGameModules)).Show();
            }
            ResetColor();
            LinkToDocs("https://infinitypbr.gitbook.io/infinity-pbr/game-modules/stats-and-skills", "Stats Docs");
            LinkToDocs("https://www.youtube.com/watch?v=4KZlCPboA5c&list=PLCK7vP-GxBCm8l-feq-aF5_dWnFda7cvQ", "Tutorials");
            LinkToDocs("https://discord.com/invite/cmZY2tH", "Discord");
            EndRow();
            BlackLine();
        }
        
        private void ShowButtons()
        {
            _menuBarOptions[1] = _modulesObject.canBeTrained ? "Mastery Levels" : "Stat Effects";
            _modulesObject.menubarIndex = ToolbarMenuMain(_menuBarOptions, _modulesObject.menubarIndex);
        }
        
        protected void Cache()
        {
            GameModuleObjects<MasteryLevels>(true);
            GameModuleObjectNames<MasteryLevels>(true);
            GameModuleObjectsOfType<Stat>(_modulesObject.objectType, true);
        }
        
        public void Validations()
        {
            CheckObjectTypes();
            RemoveMissingLinks();
        }

        private void UpdateObjectName()
        {
            if (_modulesObject.objectName == _modulesObject.name) return;
            
            _modulesObject.objectName = _modulesObject.name;
            UpdateProperties();
        }

        public void Draw()
        {
            if (_modulesObject == null) return;

            if (!InitialSetup()) return;
            
            UpdateObjectName();
            _modulesObject.RemoveMissingStats();
            
            DrawLinkToDocs();
            
            
            BeginChangeCheck();
            
            // If this is a duplicate uid and the button is clicked to reset it, redo Initial Setup
            if (!drawnByGameModulesWindow && CheckUid(_modulesObject.Uid(), _modulesObject, _duplicateUid))
                _initialSetup = false;
            
            DrawHeaderTitle();

            ShowButtons();
            
            //_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition); 
            Undo.RecordObject(_modulesObject, "Undo Main Settings Changes");
            ShowMainSettings();
            
            Undo.RecordObject(_modulesObject, "Undo Mastery Levels Changes");
            ShowMasteryLevels();
            ShowStats();
            
            Undo.RecordObject(_modulesObject, "Undo Dictionaries Changes");
            ShowDictionaries();
            ShowLinks();

            //EditorGUILayout.EndScrollView();

            EndChangeCheck(_modulesObject);
            EditorUtility.SetDirty(this);
        }

        private void DrawHeaderTitle()
        {
            if (drawnByGameModulesWindow) return;
            LabelGrey("Stat - " + _modulesObject.objectType);
            LabelBig(_modulesObject.objectName, 18, true);
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
            _modulesObject.CheckMasteryCount();
            SetNameOnLevels();
            
            if (_modulesObject.hasBeenSetup) return true;

            if (string.IsNullOrEmpty(_modulesObject.objectType))
            {
                #if UNITY_EDITOR
                var assetPath = AssetDatabase.GetAssetPath(_modulesObject);
                _modulesObject.objectType = Path.GetDirectoryName(assetPath);
                #endif
                if (string.IsNullOrEmpty(_modulesObject.objectType))
                {
                    Debug.Log("objectType is empty");
                    return false;
                }
            }
            if (_modulesObject.dictionaries.keyValues == null) return false;
            
            _modulesObject.Cache();
            Cache();
            
            // Match settings with other objects of this type which all share the same settings (i.e. all turned on, otherwise, stay off)
            MatchDictionary(GameModuleObjectsOfType<Stat>(_modulesObject.objectType));

            _modulesObject.hasBeenSetup = true;
            return true;
        }

        private void ShowLinks()
        {
            Space();
            LabelSized($"Links to other Stats {symbolInfo}", "The lists below, if any, will show the other " +
                                                                      "Stat objects that are affected by, or " +
                                                                      "affect this. This is useful for visualizing the " +
                                                                      "connections.\n\n\"Directly\" indicates the " +
                                                                      "Stat objects are directly affected by " +
                                                                      "this as targets, or directly affect this.", 300, 14, true);

            StartRow();
            StartVertical();
            ShowColumnHead("Directly Affected By", 300);
            foreach (Stat stat in _modulesObject.directlyAffectedBy)
                ShowOther(stat);
            EndVertical();
            
            StartVertical();
            ShowColumnHead("Directly Affects", 300);
            foreach (Stat stat in _modulesObject.directlyAffects)
                ShowOther(stat);
            EndVertical();
            EndRow();
            
            StartRow();
            StartVertical();
            ShowColumnHead("Affected By", 300);
            foreach (Stat stat in _modulesObject.allAffectedBy)
                ShowOther(stat);
            EndVertical();
            
            StartVertical();
            ShowColumnHead("Affects", 300);
            foreach (Stat stat in _modulesObject.allAffects)
                ShowOther(stat);
            EndVertical();
            EndRow();
        }

        private void ShowColumnHead(string label, int width = 150) => Label(label, width, true);

        private void ShowOther(Stat other, int width = 150) => Object(other, typeof(Stat), width);

        private void ShowMasteryButtons()
        {
            StartRow();
            
            foreach (var stat in GameModuleObjects<Stat>())
                stat.CheckMasteryCount();

            for (var i = 0; i < _modulesObject.masteryLevels.levels.Count; i++)
            {
                var masteryLevel = _modulesObject.masteryLevels.levels[i];
                var tempShowThis = masteryLevel.showThis;
                masteryLevel.showThis = ButtonCode(masteryLevel.name, masteryLevel.showThis) 
                                        || !masteryLevel.showThis && tempShowThis;

                if (!masteryLevel.showThis || tempShowThis == masteryLevel.showThis) continue;
                
                GUIUtility.keyboardControl = 0;
                foreach (var masteryLvl in _modulesObject.masteryLevels.levels)
                    masteryLvl.showThis = false;

                _modulesObject.masteryLevels.levels[i].showThis = true;
                _modulesObject.statModificationLevels[i].CacheModifiableStats(_modulesObject);
                _modulesObject.statModificationLevels[i].CacheSourceStats(GameModuleObjects<Stat>(true), _modulesObject);
            }

            BackgroundColor(Color.white);
            EndRow();
        }

        private void RemoveMissingLinks()
        {
            _modulesObject.allAffects.RemoveAll(x => x == null);
            _modulesObject.directlyAffects.RemoveAll(x => x == null);
            _modulesObject.allAffectedBy.RemoveAll(x => x == null);
            _modulesObject.directlyAffectedBy.RemoveAll(x => x == null);
        }

        private void CheckObjectTypes()
        {
            foreach (var stat in GameModuleObjects<Stat>())
            {
                var tempName = stat.objectName;
                var tempType = stat.objectType;
                CheckName(_modulesObject);
                stat.CheckObjectType();
                
                if (tempName == stat.objectName && tempType == stat.objectType) continue;
                
                UpdateProperties();
                EditorUtility.SetDirty(stat);
            }
        }
        
        private void ShowDictionaries()
        {
            if (_modulesObject.menubarIndex != 2) return;
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
            Undo.RecordObject(Object, "Undo Value Changes");
            dictionaries.Draw(_dictionariesDrawer, "Stats", objectName, objectType);
        }

        private void ShowStats()
        {
            EnsureOneModificationLevel();

            if (_modulesObject.menubarIndex != 1) return;
            if (_modulesObject.canBeTrained) return;
            Space();

            StartVerticalBox();
            LabelBig("Stat Effects", 18, true);
            ShowMastery();
            EndVerticalBox();
        }

        private void EnsureOneModificationLevel() => _modulesObject.EnsureOneModificationLevel();
        
        private void ShowMasteryLevels()
        {
            if (_modulesObject.menubarIndex != 1) return;
            if (!_modulesObject.canBeTrained) return;
            Space();

            if (_modulesObject.masteryLevels == null)
            {
                MessageBox("There is no Mastery Levels assigned. Open the Mastery Levels management from the " +
                           "Game Modules window, and then create & " +
                           "set the Mastery Levels in the \"Types\" tab above.", MessageType.Warning);
                
                if (!Button("Manage Mastery Levels", 300)) return;
                SetString("Game Modules Window Selected", "Mastery Levels");
                EditorWindow.GetWindow(typeof(EditorWindowGameModules)).Show();
                return;
            }
            
            //NoMasteryLevelsAssigned();
            
            if (_modulesObject.masteryLevels.levels.Count == 0)
            {
                MessageBox("Mastery Levels object " + _modulesObject.masteryLevels.name + " does not have any " +
                           "levels created. Open the Mastery Levels management from the Game Modules window, and then " +
                           "set the Mastery Levels in the \"Types\" tab above.", MessageType.Warning);

                if (!Button("Manage Mastery Levels", 300)) return;
                
                SetString("Game Modules Window Selected", "Mastery Levels");
                EditorWindow.GetWindow(typeof(EditorWindowGameModules)).Show();
                
                //EditorPrefs.SetString("Game Modules Window Selected", "Mastery Levels");
                //var window = (EditorWindowGameModules)EditorWindow.GetWindow(typeof(EditorWindowGameModules));
                //window.Show();

                return;
            }
            
            StartVerticalBox();
            ShowMasteryLevelsHeader();
            ShowMasteryButtons();
            ShowMastery();
            EndVerticalBox();
        }
        
        private void NoMasteryLevelsAssigned()
        {
            ShowSelectionPopup(true);
            LabelGrey("* Changing this will affect all Stats of " + _modulesObject.objectType + " type.");
        }
        
        private void ShowSelectionPopup(bool firstTime = false)
        {
            var masteryLevels = GameModuleObjects<MasteryLevels>();
            var statsOfThisType = GameModuleObjectsOfType<Stat>(_modulesObject.objectType);

            var currentIndex = GetCurrentIndexMasteryLevel(masteryLevels, _modulesObject.masteryLevels);
            if (currentIndex < 0)
                currentIndex = 0;

            StartRow();
            var tempIndex = currentIndex;
            currentIndex = Popup(currentIndex, GameModuleObjectNames<MasteryLevels>(), 300);
            if (tempIndex != currentIndex)
                DoChecks(statsOfThisType);
            
            Label("Mastery Levels object*", 150);
            EndRow();
            
            foreach (var stat in statsOfThisType)
                stat.masteryLevels = masteryLevels[currentIndex];
        }
        
        private void DoChecks(Stat[] statsOfThisType)
        {
            foreach (var stat in statsOfThisType)
                stat.CheckMasteryCount();
        }
        
        private int GetCurrentIndexMasteryLevel(MasteryLevels[] masteryLevelsArray, MasteryLevels masteryLevels)
        {
            for (int i = 0; i < masteryLevelsArray.Length; i++)
            {
                if (masteryLevelsArray[i] == masteryLevels)
                    return i;
            }

            return -1;
        }

        private void ShowMoreDetails()
        {
            Space();
            StartVerticalBox();
            LabelBig("More Details", 18, true);
            Label("To Be completed...");
            EndVerticalBox();
        }

        private int GetMasteryLevelIndex()
        {
            if (!_modulesObject.canBeTrained) return 0;
            
            for (var i = 0; i < _modulesObject.masteryLevels.levels.Count; i++)
            {
                if (!_modulesObject.masteryLevels.levels[i].showThis) 
                    continue;
                
                return i;
            }

            _modulesObject.masteryLevels.levels[0].showThis = true;
            return 0;
        }

        private void ShowMasteryLevelsHeader() => Label($"{_modulesObject.objectName} - <b>{ActiveMasteryLevel.name}</b>", false, false, true);

        public void ShowMastery() => _statModificationLevelDrawer.Draw(_modulesObject.statModificationLevels[GetMasteryLevelIndex()], _modulesObject);

        private void ShowMainSettings()
        {
            if (_modulesObject.menubarIndex != 0) return;
            Space();
            StartVerticalBox();
            //LabelBig("Main Settings", 18, true);
            ShowMainSettingsBody();
            EndVerticalBox();
        }

        private void ShowMainSettingsBody()
        {
            //ShowCanBeTrained();
            //ShowCanBeModified();
            ShowStatKind();

            Space();
            StartVerticalBox();
            DrawStatsHeader();
            DrawStatsPoints();
            DrawStatsBaseValue();
            DrawStatsBaseProficiency();
            DrawStatsFinalStat();
            EndVerticalBox();
            
            StartVerticalBox();
            DrawBlackboardSettings();
            DrawBlackboardPoints();
            DrawBlackboardBaseValue();
            DrawBlackboardBaseProficiency();
            DrawBlackboardFinalValue();
            EndVerticalBox();
        }

        private void ShowStatKind()
        {
            StartRow();
            // Trained
            BackgroundColor(_modulesObject.canBeTrained ? Color.green : Color.black);
            ContentColor(_modulesObject.canBeTrained ? Color.white : Color.grey);
            if (Button("Can be Trained", 120, 40))
            {
                Undo.RecordObject(_modulesObject, "Undo Can Be Trained Setting");
                _modulesObject.canBeTrained = !_modulesObject.canBeTrained;
            }
            BackgroundColor(Color.white);
            ContentColor(Color.white);
            
            // Modified
            BackgroundColor(_modulesObject.canBeModified ? Color.green : Color.black);
            ContentColor(_modulesObject.canBeModified ? Color.white : Color.grey);
            if (Button("Can be Modified", 120, 40))
            {
                Undo.RecordObject(_modulesObject, "Undo Can Be Modified Setting");
                _modulesObject.canBeModified = !_modulesObject.canBeModified;
            }
            BackgroundColor(Color.white);
            ContentColor(Color.white);
            
            // Text details
            if (_modulesObject.canBeTrained && _modulesObject.canBeModified)
            {
                Label($"<b>Skill (Modifiable)</b>\n{_modulesObject.ObjectName} can be trained, and improve over time. " +
                      "Use \"Mastery Levels\" tab to manage the effects it has on other Stats at each Mastery Level " +
                      $". It is available for other objects to modify.{ModificationLevelWarning()}", false, true, true);
            }
            if (_modulesObject.canBeTrained && !_modulesObject.canBeModified)
            {
                Label($"<b>Skill</b>\n{_modulesObject.ObjectName} can be trained, and improve over time. " +
                      "Use \"Mastery Levels\" tab to manage the effects it has on other Stats at each Mastery Level " +
                      $". Other objects can NOT modify this.{ModificationLevelWarning()}", false, true, true);
            }
            if (!_modulesObject.canBeTrained && _modulesObject.canBeModified)
            {
                Label($"<b>Stat</b>\n{_modulesObject.ObjectName} can be modified by other objects, but can not be " +
                      "trained over time. Use \"Stat Effects\" tab to set immutable affects on other Stats.", false, true, true);
            }
            if (!_modulesObject.canBeTrained && !_modulesObject.canBeModified)
            {
                Label($"<b>Counter</b>\n{_modulesObject.ObjectName} acts as a counter, using the \"Points\" value. This " +
                      "is useful for things like \"Health\" or \"Experience\".", false, true, true);
            }
            
            EndRow();
        }

        private string ModificationLevelWarning()
        {
            if (!_modulesObject.canBeTrained) return "";
            if (_modulesObject.masteryLevels != null) return "";
            
            return $"\n\n<color=#ff5555><b>Warning:</b> Mastery Levels are not set. This means that this object will not " +
                   $"have any effect on other objects. Use the \"Mastery Levels\" tab to set this up.</color>";
        }

        private void DrawBlackboardFinalValue()
        {
            StartRow();
            Label("Final Stat", 150);
            Undo.RecordObject(_modulesObject, "Undo Value change");
            BackgroundColor(_modulesObject.notifyFinalStatToBlackboard ? Color.grey : Color.white);
            _modulesObject.postFinalStatToBlackboard = Check(_modulesObject.postFinalStatToBlackboard, 50);
            ResetColor();
            _modulesObject.notifyFinalStatToBlackboard = Check(_modulesObject.notifyFinalStatToBlackboard, 50);
            if (_modulesObject.notifyFinalStatToBlackboard && !_modulesObject.postFinalStatToBlackboard)
                _modulesObject.postFinalStatToBlackboard = true;
            ContentColor(_modulesObject.postFinalStatToBlackboard ? Color.white : Color.grey);
            Label($"Subject: {_modulesObject.objectName}-FinalStat");
            ResetColor();
            EndRow();
        }

        private void DrawBlackboardBaseProficiency()
        {
            StartRow();
            Label("Final Proficiency", 150);
            Undo.RecordObject(_modulesObject, "Undo Value change");
            BackgroundColor(_modulesObject.notifyFinalProficiencyToBlackboard ? Color.grey : Color.white);
            _modulesObject.postFinalProficiencyToBlackboard = Check(_modulesObject.postFinalProficiencyToBlackboard, 50);
            ResetColor();
            _modulesObject.notifyFinalProficiencyToBlackboard = Check(_modulesObject.notifyFinalProficiencyToBlackboard, 50);
            if (_modulesObject.notifyFinalProficiencyToBlackboard && !_modulesObject.postFinalProficiencyToBlackboard)
                _modulesObject.postFinalProficiencyToBlackboard = true;
            ContentColor(_modulesObject.postFinalProficiencyToBlackboard ? Color.white : Color.grey);
            Label($"Subject: {_modulesObject.objectName}-FinalProficiency");
            ResetColor();
            EndRow();
        }

        private void DrawBlackboardBaseValue()
        {
            StartRow();
            Label("Final Value", 150);
            Undo.RecordObject(_modulesObject, "Undo Value change");
            BackgroundColor(_modulesObject.notifyFinalValueToBlackboard ? Color.grey : Color.white);
            _modulesObject.postFinalValueToBlackboard = Check(_modulesObject.postFinalValueToBlackboard, 50);
            ResetColor();
            _modulesObject.notifyFinalValueToBlackboard = Check(_modulesObject.notifyFinalValueToBlackboard, 50);
            if (_modulesObject.notifyFinalValueToBlackboard && !_modulesObject.postFinalValueToBlackboard)
                _modulesObject.postFinalValueToBlackboard = true;
            ContentColor(_modulesObject.postFinalValueToBlackboard ? Color.white : Color.grey);
            Label($"Subject: {_modulesObject.objectName}-FinalValue");
            ResetColor();
            EndRow();
        }

        private void DrawBlackboardPoints()
        {
            StartRow();
            Label("Points", 150);
            Undo.RecordObject(_modulesObject, "Undo Value change");
            BackgroundColor(_modulesObject.notifyPointsToBlackboard ? Color.grey : Color.white);
            _modulesObject.postPointsToBlackboard = Check(_modulesObject.postPointsToBlackboard, 50);
            ResetColor();
            _modulesObject.notifyPointsToBlackboard = Check(_modulesObject.notifyPointsToBlackboard, 50);
            if (_modulesObject.notifyPointsToBlackboard && !_modulesObject.postPointsToBlackboard)
                _modulesObject.postPointsToBlackboard = true;
            ContentColor(_modulesObject.postPointsToBlackboard ? Color.white : Color.grey);
            Label($"Subject: {_modulesObject.objectName}-Points");
            ResetColor();
            EndRow();
        }

        private void DrawBlackboardSettings()
        {
            StartRow();
            Label($"Blackboard Settings {symbolInfo}", "Stats can be automatically posted as notes to the " +
                                                       "MainBlackboard. You can opt to have notifications sent out " +
                                                       "when the stat is posted or not. Each time the FinalStat() value " +
                                                       "changes, the note will be updated, sending out a notification " +
                                                       "if you toggle that option on.", 150, true);
            Label("Post", 50, true);
            Label("Notify", 50, true);
            Label($"BlackboardNote Subject {symbolInfo}", "If Post is true, the \"Topic\" of the Blackboard Note will " +
                                                          "be the GameId() of the object which owns this GameStat. The subject, " +
                                                          "will be based on the name of the Stat and the value being reported " +
                                                          "in the BlackboardNote.");
            
            EndRow();
        }

        private void DrawStatsFinalStat()
        {
            ShowMinMax();
            Space();
            ShowRounding();
        }

        private void DrawStatsBaseProficiency()
        {
            ShowBaseValue("Base Proficiency", () =>
            {
                _modulesObject.baseProficiency = Float(_modulesObject.baseProficiency, 100);
                
                Undo.RecordObject(_modulesObject, "Undo Value change");
                _modulesObject.minBaseProficiencyType = Popup(_modulesObject.minBaseProficiencyType, _modulesObject.minMaxOptions, 60);

                switch (_modulesObject.minBaseProficiencyType)
                {
                    case 0:
                        Label("", 60);
                        break;
                    case 1:
                        Undo.RecordObject(_modulesObject, "Undo Value change");
                        _modulesObject.minBaseProficiency = Float(_modulesObject.minBaseProficiency, 60);
                        break;
                    default:
                        Undo.RecordObject(_modulesObject, "Undo Value change");
                        _modulesObject.minBaseProficiencyStat = Object(_modulesObject.minBaseProficiencyStat, typeof(Stat), 60) as Stat;
                        break;
                }
                
                Undo.RecordObject(_modulesObject, "Undo Value change");
                _modulesObject.maxBaseProficiencyType = Popup(_modulesObject.maxBaseProficiencyType, _modulesObject.minMaxOptions, 60);

                switch (_modulesObject.maxBaseProficiencyType)
                {
                    case 0:
                        Label("", 60);
                        break;
                    case 1:
                        Undo.RecordObject(_modulesObject, "Undo Value change");
                        _modulesObject.maxBaseProficiency = Float(_modulesObject.maxBaseProficiency, 60);
                        break;
                    default:
                        Undo.RecordObject(_modulesObject, "Undo Value change");
                        _modulesObject.maxBaseProficiencyStat = Object(_modulesObject.maxBaseProficiencyStat, typeof(Stat), 60) as Stat;
                        break;
                }
                
            });
        }

        private void DrawStatsBaseValue()
        {
            ShowBaseValue("Base Value", () =>
            {
                _modulesObject.baseValue = Float(_modulesObject.baseValue, 100);
                
                Undo.RecordObject(_modulesObject, "Undo Value change");
                _modulesObject.minBaseValueType = Popup(_modulesObject.minBaseValueType, _modulesObject.minMaxOptions, 60);

                switch (_modulesObject.minBaseValueType)
                {
                    case 0:
                        Label("", 60);
                        break;
                    case 1:
                        Undo.RecordObject(_modulesObject, "Undo Value change");
                        _modulesObject.minBaseValue = Float(_modulesObject.minBaseValue, 60);
                        break;
                    default:
                        Undo.RecordObject(_modulesObject, "Undo Value change");
                        _modulesObject.minBaseValueStat = Object(_modulesObject.minBaseValueStat, typeof(Stat), 60) as Stat;
                        break;
                }
                
                Undo.RecordObject(_modulesObject, "Undo Value change");
                _modulesObject.maxBaseValueType = Popup(_modulesObject.maxBaseValueType, _modulesObject.minMaxOptions, 60);

                switch (_modulesObject.maxBaseValueType)
                {
                    case 0:
                        Label("", 60);
                        break;
                    case 1:
                        Undo.RecordObject(_modulesObject, "Undo Value change");
                        _modulesObject.maxBaseValue = Float(_modulesObject.maxBaseValue, 60);
                        break;
                    default:
                        Undo.RecordObject(_modulesObject, "Undo Value change");
                        _modulesObject.maxBaseValueStat = Object(_modulesObject.maxBaseValueStat, typeof(Stat), 60) as Stat;
                        break;
                }
                
            });
        }

        private void DrawStatsPoints()
        {
            ShowBaseValue("Points", () =>
            {
                Undo.RecordObject(_modulesObject, "Undo Value change");
                _modulesObject.points = Float(_modulesObject.points, 100);

                Undo.RecordObject(_modulesObject, "Undo Value change");
                _modulesObject.minPointsType = Popup(_modulesObject.minPointsType, _modulesObject.minMaxOptions, 60);

                switch (_modulesObject.minPointsType)
                {
                    case 0:
                        Label("", 60);
                        break;
                    case 1:
                        Undo.RecordObject(_modulesObject, "Undo Value change");
                        _modulesObject.minPoints = Float(_modulesObject.minPoints, 60);
                        break;
                    default:
                        Undo.RecordObject(_modulesObject, "Undo Value change");
                        _modulesObject.minPointsStat = Object(_modulesObject.minPointsStat, typeof(Stat), 60) as Stat;
                        break;
                }
                
                Undo.RecordObject(_modulesObject, "Undo Value change");
                _modulesObject.maxPointsType = Popup(_modulesObject.maxPointsType, _modulesObject.minMaxOptions, 60);

                switch (_modulesObject.maxPointsType)
                {
                    case 0:
                        Label("", 60);
                        break;
                    case 1:
                        Undo.RecordObject(_modulesObject, "Undo Value change");
                        _modulesObject.maxPoints = Float(_modulesObject.maxPoints, 60);
                        break;
                    default:
                        Undo.RecordObject(_modulesObject, "Undo Value change");
                        _modulesObject.maxPointsStat = Object(_modulesObject.maxPointsStat, typeof(Stat), 60) as Stat;
                        break;
                }
                
                _modulesObject.modifyPointsByProficiency = Object(_modulesObject.modifyPointsByProficiency, typeof(Stat), 200) as Stat;
            });
        }

        private void DrawStatsHeader()
        {
            StartRow();
            Label($"Settings {symbolInfo}", "\"Points\" is intended to be player modifiable, where players can change " +
                                            "this value actively throughout the game, and generally permanently.\n\n" +
                                            "\"Base Value\" is the starting value for this Stats, and \"Base " +
                                            "Proficiency\" is the starting proficiency. Remember that 0 = 100% proficiency.\n\n" +
                                            "You can also choose to set a minimum and maximum value for the final stat, " +
                                            "and choose how to round, if at all, the final stat.", 150, true);
            Label($"Values {symbolInfo}", "These are the starting values for the settings.", 100, true);
            Label($"Minimum {symbolInfo}", "Force a minimum value.", 123, true);
            Label($"Maximum {symbolInfo}", "Force a maximum value.", 123, true);
            Label($"Modified by this proficiency {symbolInfo}", "When adding or subtracting points, the amount " +
                                                                "added can be modified by the final proficiency " +
                                                                "of this Stat object. Call \"AddPoints(false)\" " +
                                                                "to ignore this feature when adding points.", 200, true);
            EndRow();
        }

        private void ShowBaseValue(string label, Action content)
        {
            StartRow();
            
            Label(label, 150);
            Undo.RecordObject(_modulesObject, "Undo Value change");
            content?.Invoke();
            EndRow();
        }

        private void ShowRounding()
        {
            StartRow();
            Label("Rounding", 150);
            _modulesObject.roundingMethod = (Rounding) EnumPopup(_modulesObject.roundingMethod, 60);
            if (_modulesObject.roundingMethod == Rounding.Round)
            {
                Label("Decimal Places", 100);
                _modulesObject.decimals = Int(_modulesObject.decimals, 20);
            }
            EndRow();
        }

        private void ShowMinMax()
        {
            if (_modulesObject.minFinal >= _modulesObject.maxFinal)
                _modulesObject.maxFinal = _modulesObject.minFinal + 0.1f;
            StartRow();
            Label("Final Stat", 150);
            Label("", 100);
            
            Undo.RecordObject(_modulesObject, "Undo Value change");
            _modulesObject.minFinalType = Popup(_modulesObject.minFinalType, _modulesObject.minMaxOptions, 60);

            switch (_modulesObject.minFinalType)
            {
                case 0:
                    Label("", 60);
                    break;
                case 1:
                    Undo.RecordObject(_modulesObject, "Undo Value change");
                    _modulesObject.minFinal = Float(_modulesObject.minFinal, 60);
                    break;
                default:
                    Undo.RecordObject(_modulesObject, "Undo Value change");
                    _modulesObject.minFinalStat = Object(_modulesObject.minFinalStat, typeof(Stat), 60) as Stat;
                    break;
            }
                
            Undo.RecordObject(_modulesObject, "Undo Value change");
            _modulesObject.maxFinalType = Popup(_modulesObject.maxFinalType, _modulesObject.minMaxOptions, 60);

            switch (_modulesObject.maxFinalType)
            {
                case 0:
                    Label("", 60);
                    break;
                case 1:
                    Undo.RecordObject(_modulesObject, "Undo Value change");
                    _modulesObject.maxFinal = Float(_modulesObject.maxFinal, 60);
                    break;
                default:
                    Undo.RecordObject(_modulesObject, "Undo Value change");
                    _modulesObject.maxFinalStat = Object(_modulesObject.maxFinalStat, typeof(Stat), 60) as Stat;
                    break;
            }
            
            EndRow();
        }

        private void ShowCanBeModified()
        {
            StartRow();
            BackgroundColor(_modulesObject.canBeModified ? Color.green : Color.black);
            ContentColor(_modulesObject.canBeModified ? Color.white : Color.grey);
            if (Button("Can be Modified", 200, 40))
            {
                Undo.RecordObject(_modulesObject, "Undo Can Be Modified Setting");
                _modulesObject.canBeModified = !_modulesObject.canBeModified;
            }
            BackgroundColor(Color.white);
            ContentColor(Color.white);
            if (_modulesObject.canBeModified)
            {
                Label($"{_modulesObject.ObjectName} can be modified by other Stats, as well as Items, Conditions, " +
                      $"Quests, etc. {_modulesObject.ObjectName} will show as available for modification to " +
                      "other objects.", false, true);
            }
            else
            {
                Label($"{_modulesObject.ObjectName} can not be modified. It will not be available to other Stats, Items, " +
                      "or other objects which otherwise can modify Stats.", false, true);
            }
            
            EndRow();
        }
        
        private void ShowCanBeTrained()
        {
            StartRow();
            BackgroundColor(_modulesObject.canBeTrained ? Color.green : Color.black);
            ContentColor(_modulesObject.canBeTrained ? Color.white : Color.grey);
            if (Button("Can be Trained", 200, 40))
            {
                Undo.RecordObject(_modulesObject, "Undo Can Be Trained Setting");
                _modulesObject.canBeTrained = !_modulesObject.canBeTrained;
            }
            BackgroundColor(Color.white);
            ContentColor(Color.white);
            if (_modulesObject.canBeTrained)
            {
                Label($"{_modulesObject.ObjectName} can be trained, and improve over time. The \"Mastery Levels\" tab will " +
                      $"let you manage the effects {_modulesObject.ObjectName} has on other Stats at each Mastery Level " +
                      "in it's trainable path.", false, true);
            }
            else
            {
                Label($"Players can not train {_modulesObject.ObjectName}. The \"Stat Effects\" tab allows " +
                      $"{_modulesObject.ObjectName} to affect other stats. Any effects will be immutable.", false, true);
            }
            
            EndRow();
        }

        private void SetNameOnLevels()
        {
            foreach (var level in _modulesObject.statModificationLevels)
            {
                if (level == null) continue;
                level.ownerName = _modulesObject.objectName;
            }
        }
        
        private void MatchDictionary(Stat[] otherObjects)
        {
            var otherObjectsOfThisType = otherObjects.Length - 1;
            var keyValues = otherObjects
                .Except(otherObjects
                    .Where(x => x == _modulesObject)
                    .ToArray())
                .SelectMany(x => x.dictionaries.keyValues)
                .Select(x => x.key)
                .Distinct()
                .ToList();
            
            foreach (var keyValue in keyValues)
            {
                if (string.IsNullOrWhiteSpace(keyValue)) continue;
                if (ObjectsOfTypeWithDictionaryKeyValue<Stat>(_modulesObject.objectType, keyValue).Length != otherObjectsOfThisType)
                    continue;

                var newKeyValue = otherObjects[0].GetKeyValue(keyValue)?.Clone();
                if (newKeyValue != null)
                    _modulesObject.dictionaries.keyValues.Add(newKeyValue);
            }
        }
    }
}