using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using static InfinityPBR.Modules.GameModuleUtilities;
using static InfinityPBR.Modules.EditorUtilities;
using static InfinityPBR.Modules.InfinityEditorGameModules;

namespace InfinityPBR.Modules
{
    public class QuestsDrawer : GameModulesDrawer
    {
        // -------------------------------
        // PROPERTIES AND VARIABLES -- UPDATE THESE FOR THIS TYPE
        // -------------------------------
        private Quest _modulesObject;
        const string ThisTypePlural = "Quests";
        const string ThisType = "Quest";
        private string ClassNamePlural => "Quests";
        private string ClassName => "Quest";
        private string DocsURL => "https://infinitypbr.gitbook.io/infinity-pbr/game-modules/quests";
        private string DocsURLLabel => "Quests";
        private string[] GameModuleObjectTypes(bool recompute = false) => GameModuleObjectTypes<Quest>(recompute);
        private Quest[] GameModuleObjects(bool recompute = false) => GameModuleObjects<Quest>(recompute);
        private Quest[] GameModuleObjectsOfType(string type, bool recompute = false) => GameModuleObjectsOfType<Quest>(type, recompute);
        // -------------------------------
        
        // -------------------------------
        // REQUIRED - NO UPDATE NEEDED
        // -------------------------------
        private Vector2 _scrollPosition;
        private int _fieldWidth;
        private DictionariesDrawer _dictionariesDrawer;
        private StatModificationLevelDrawer _statModificationLevelDrawer;
        private ItemAttributeDrawer _itemAttributeDrawer;
        private ConditionDrawer _conditionDrawer;
        private QuestConditionDrawer _questConditionDrawer;
        private QuestRewardDrawer _questRewardDrawer;

        private string[] _menuBarOptions;
        public bool drawnByGameModulesWindow = true;
        
        //private int _lootItemsIndex = 0;
        //private List<LootItems> _cachedLootItems = new List<LootItems>();
        //private List<string> _cachedLootItemsNames = new List<string>();
        //private List<LootItems> _activeLootItems = new List<LootItems>();
        
        // QUESTS MODULE
        private bool ViewingSuccessConditions(QuestStep questStep) => questStep.toolbarIndex == 0;
        private bool ViewingFailureConditions(QuestStep questStep) => questStep.toolbarIndex == 1;
        private bool ViewingSuccessRewards(QuestStep questStep) => questStep.toolbarIndex == 2;
        private bool ViewingFailureRewards(QuestStep questStep) => questStep.toolbarIndex == 3;
        private bool ViewingSuccessRewards() => _modulesObject.toolbarIndex == 1;
        private bool ViewingFailureRewards() => _modulesObject.toolbarIndex == 2;

        private List<QuestConditionDrawer> _questConditionDrawers = new List<QuestConditionDrawer>();
        private List<QuestRewardDrawer> _questRewardDrawers = new List<QuestRewardDrawer>();
        // -------------------------------

        // -------------------------------
        // METHODS -- UPDATE THESE FOR THIS TYPE
        // -------------------------------
        public void SetModulesObject(Quest modulesObject, bool drawnByWindow = true)
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
            
            var questConditionDrawerTypes = GetSubClassesOfQuestConditionDrawer();
            var questRewardDrawerTypes = GetSubClassesOfQuestRewardDrawer();
            if (questConditionDrawerTypes == null || questRewardDrawerTypes == null ||
                questConditionDrawerTypes.Count() != _questConditionDrawers.Count ||
                questRewardDrawerTypes.Count() != _questRewardDrawers.Count)
            {
                GameModuleObjects<QuestCondition>(true);
                GameModuleObjects<QuestReward>(true);
                
                // Quest Condition Types
                //var questConditionDrawerTypes = GetSubClassesOfQuestConditionDrawer();
                _questConditionDrawers = new List<QuestConditionDrawer>();
                foreach (var type in questConditionDrawerTypes)
                {
                    var drawerInstance = CreateInstance(type) as QuestConditionDrawer;
                    _questConditionDrawers.Add(drawerInstance);
                }
            
                // Quest Reward Types
                //var questRewardDrawerTypes = GetSubClassesOfQuestRewardDrawer();
                _questRewardDrawers = new List<QuestRewardDrawer>();
                foreach (var type in questRewardDrawerTypes)
                {
                    var drawerInstance = CreateInstance(type) as QuestRewardDrawer;
                    _questRewardDrawers.Add(drawerInstance);
                }
            }
            
            _dictionariesDrawer = new DictionariesDrawerEditor(_fieldWidth);
            _statModificationLevelDrawer = new StatModificationLevelDrawerEditor(_fieldWidth);

            _modulesObject.hasBeenSetup = false;
        }
        
        public static IEnumerable<Type> GetSubClassesOfQuestConditionDrawer()
        {
            var assembly = Assembly.GetAssembly(typeof(QuestConditionDrawer));
            var types = assembly.GetTypes();
            var subClasses = types.Where(t => t.IsSubclassOf(typeof(QuestConditionDrawer)) && !t.IsAbstract);
            return subClasses;
        }

        public static IEnumerable<Type> GetSubClassesOfQuestRewardDrawer()
        {
            var assembly = Assembly.GetAssembly(typeof(QuestRewardDrawer));
            var types = assembly.GetTypes();
            var subClasses = types.Where(t => t.IsSubclassOf(typeof(QuestRewardDrawer)) && !t.IsAbstract);
            return subClasses;
        }
        
        protected void Cache()
        {
            GameModuleObjects<MasteryLevels>(true);
            GameModuleObjectNames<MasteryLevels>(true);
            GameModuleObjectTypes(true);
            GameModuleObjects(true);
            GameModuleObjectsOfType(_modulesObject.objectType, true);
            GameModuleObjects<QuestCondition>(true);
            GameModuleObjects<QuestReward>(true);
            UpdateList();
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
            //CheckObjectTypes();
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
                LabelGrey("Quest - " + _modulesObject.objectType);
                LabelBig(_modulesObject.objectName, 18, true);
            }
            
            BeginChangeCheck();
            
            // If this is a duplicate uid and the button is clicked to reset it, redo Initial Setup
            if (!drawnByGameModulesWindow && CheckUid(_modulesObject.Uid(), _modulesObject, _duplicateUid))
                _initialSetup = false;
           
            
            ShowButtons();
            
            Undo.RecordObject(_modulesObject, "Undo Quest Changes");
            ShowQuest();
            
            Undo.RecordObject(_modulesObject, "Undo Quest Steps Changes");
            ShowQuestSteps();

            Undo.RecordObject(_modulesObject, "Undo Stats Changes");
            ShowStatsAndSkills();

            Undo.RecordObject(_modulesObject, "Undo Dictionaries Changes");
            ShowDictionaries();

            CheckConflicts();

            EndChangeCheck(_modulesObject);
            EditorUtility.SetDirty(_modulesObject);
        }

        private void CheckConflicts()
        {
            if (!_modulesObject) return;
            if (_modulesObject.dictionaries?.keyValues == null) return;
        }

        private void ShowDictionaries()
        {
            if (MainPanelSelected != 2) return;
            Space();
            StartVerticalBox();
            ShowDictionariesHeader();
            ShowDictionary(_modulesObject.dictionaries, _modulesObject.objectName, _modulesObject.objectType, _modulesObject);
            EndVerticalBox();
        }
        
        private void ShowStatsAndSkills()
        {
            if (MainPanelSelected != 1) return;
            
            MessageBox("Each quest can have an ongoing impact on the stats of the owner. You " +
                       "can set different impacts based on the status of the Quest, to add ongoing changes " +
                       "while a quest is in progress, or ongoing buffs or penalties if the quest has " +
                       "succeeded or failed.", MessageType.Info);

            var cachedOption = _modulesObject.questStatsState;
            var barOptions = new[] {"In Progress", "Succeeded", "Failed"};
            var toolbarOption = ToolbarMenu(barOptions, _modulesObject.questStatsState, 12, true);
            _modulesObject.questStatsState = toolbarOption;

            switch (_modulesObject.questStatsState)
            {
                case 0 : ShowModifications(_modulesObject.modificationLevelInProgress, "In Progress", cachedOption);
                    break;
                case 1 : ShowModifications(_modulesObject.modificationLevelSucceeded, "Succeeded", cachedOption);
                    break;
                case 2 : ShowModifications(_modulesObject.modificationLevelFailed, "Failed", cachedOption);
                    break;
            }
        }

        private void ShowModifications(ModificationLevel modificationLevel, string header, int cachedOption)
        {
            if (_modulesObject.questStatsState != cachedOption)
                SetBool("statModificationLevelDrawer Force Cache", true);

            Space();
            StartVerticalBox();
            ShowHeader(header);
            _statModificationLevelDrawer.Draw(modificationLevel, null);
            EndVerticalBox();
        }
        
        private void ShowQuest()
        {
            if (MainPanelSelected != 0) return;
            Space();
            _modulesObject.showQuestMain = StartVerticalBoxSection(_modulesObject.showQuestMain, "Quest Settings & Rewards");
            
            if (_modulesObject.showQuestMain)
            {
                Space();
                ShowMainButtons();
                ShowSettings();
                ShowQuestSuccessConditions();
                ShowQuestFailureConditions();
            }
            EndVerticalBox();
        }
        
        private void ShowMainButtons()
        {
            var startValue = _modulesObject.toolbarIndex;
            var barOptions = new[] {
                "Settings", 
                $"Success Rewards ({_modulesObject.successRewards.Count})",
                $"Failure Rewards ({_modulesObject.failureRewards.Count})"
            };
            var toolbarOption = ToolbarMenu(barOptions, startValue, 12, true);
            _modulesObject.toolbarIndex = toolbarOption;
        }

        private void ShowQuestSuccessConditions()
        {
            if (_modulesObject.toolbarIndex != 1) return;
            MessageBox("When the quest succeeds, automatic rewards may occur, including point effects on " +
                       "stats, other effects on stats, conditions being added or removed, quests being added or " +
                       "automatically succeeding or failing regardless of their own conditions, and items being added.\n\n" +
                       "Use the \"Stats\" panel to add constant effects on stats, similar to Item Objects being equipped.");
            _modulesObject.successRewards.ForEach(x => ShowReward(null, x));

            Space();
            ShowAddReward();
        }
        
        private void ShowQuestFailureConditions()
        {
            if (_modulesObject.toolbarIndex != 2) return;
            MessageBox("When the quest fails, automatic \"rewards\" (which perhaps could be negative rewards) may occur, including point effects on " +
                       "stats, other effects on stats, conditions being added or removed, quests being added or " +
                       "automatically succeeding or failing regardless of their own conditions, and items being added.\n\n" +
                       "Use the \"Stats\" panel to add constant effects on stats, similar to Item Objects being equipped.");
            _modulesObject.failureRewards.ForEach(x => ShowReward(null, x));
            
            Space();
            ShowAddReward();
        }

        private void ShowSettings()
        {
            if (_modulesObject.toolbarIndex != 0) return;
            var extraMessage = "";

            // SUBSCRIBE TO BLACKBOARD
            _modulesObject.subscribeToBlackboard = LeftCheck($"Subscribe to Blackboard {symbolInfo}",
                "If true, GameQuest objects created " +
                "from this Quest will subscribe to the Blackboard. This is " +
                "required for quests to auto-check their status using the " +
                "Blackboard Note topic/subject.", _modulesObject.subscribeToBlackboard);
            
            // QUERY EVERY FRAME
            ContentColor(_modulesObject.subscribeToBlackboard ? Color.white : Color.grey);
            extraMessage = _modulesObject.subscribeToBlackboard ? "" : "[Requires Subscribe to Blackboard]";
            _modulesObject.queryEveryFrame = LeftCheck($"Query Every Frame {symbolInfo} {extraMessage}",
                "If true, this GameQuest will query the Blackboard every frame to check " +
                "success and failure. If you have many quests, this may not be as optimized as you'd like. " +
                "When false, the checks will only occur when the Blackboard Notes & Events are triggered.", 
                _modulesObject.queryEveryFrame);
            ResetColor();

            // AUTO SUCCEED
            
            ContentColor(_modulesObject.subscribeToBlackboard ? Color.white : Color.grey);
            extraMessage = _modulesObject.subscribeToBlackboard ? "" : "[Requires Subscribe to Blackboard]";
            _modulesObject.autoSucceed = LeftCheck($"Auto Succeed {symbolInfo} {extraMessage}",
                "If true, the system will automatically check to see if the " +
                "quest has been completed, and run the completion code at that point. " +
                "Set this false, if you want to control when the quest is checked for " +
                "completion, such as when a quest is \"turned in\" to an NPC.", _modulesObject.autoSucceed);
            ResetColor();
            
            // AUTO FAIL
            ContentColor(_modulesObject.HasFailConditions && _modulesObject.subscribeToBlackboard ? Color.white : Color.grey);
            extraMessage = _modulesObject.subscribeToBlackboard ? "" : "[Requires Subscribe to Blackboard]";
            extraMessage = _modulesObject.HasFailConditions ? $"{extraMessage} " : $"{extraMessage} [No Fail Conditions Set]";
            _modulesObject.autoFail = LeftCheck($"Auto Fail {symbolInfo} {extraMessage}",
                "If true, the system will automatically check to see if the " +
                "quest has failed, and run the failure code at that point. " +
                "Set this false, if you want to control when the quest is checked for " +
                "failure.", _modulesObject.autoFail);
            ResetColor();

            // SEQUENTIAL STEPS
            ContentColor(_modulesObject.Steps > 1 ? Color.white : Color.grey);
            if (_modulesObject.Steps == 0) extraMessage = "[No Steps Set]";
            else if (_modulesObject.Steps == 1) extraMessage = "[Only One Step]";
            _modulesObject.sequentialSteps = LeftCheck($"Sequential Steps {symbolInfo} {extraMessage}","If true, steps must be completed " +
                                                                               "sequentially. Otherwise, each step can be completed " +
                                                                               "out of order.", _modulesObject.sequentialSteps);
            ResetColor();

            // HIDDEN
            _modulesObject.hidden = LeftCheck($"Hidden {symbolInfo}","This is intended to mark some quests as hidden from " +
                                                            "players. It is up to you to determine how to utilize this " +
                                                            "data.", _modulesObject.hidden);
            
            // TIME LIMIT
            _modulesObject.hasEndTime = LeftCheck($"Has End Time {symbolInfo}",
                "If true, this Quest will automatically expire " +
                "once the time limit has been reached. It will not succeed or fail unless " +
                "one of those options is selected, in which case it will remove itself from the GameQuestList. If this has an end time, you must " +
                "remember to pass in the Gametime value, whether you are using the Gametime " +
                "module or your own time system.\n\nQuestStep: Succeed or Fail due to the expiration will ignore other conditions and set " +
                "success and failure. All steps will also be marked canRevert = false." +
                "\n\nThe Quest itself will only succeed or fail automatically if the Auto Succeed or Auto Fail is true. Otherwise, " +
                "you will need to handle completing the quest as you'd like.", _modulesObject.hasEndTime);
            if (_modulesObject.hasEndTime)
            {
                StartRow();
                Label("Time Limit:", 90);
                _modulesObject.timeLimit = Float(_modulesObject.timeLimit, 50);
                LabelGrey("In-game minutes", 150);
                EndRow();
                
                StartRow();
                Label("On Expiration:", 90);
                var barOptions = new[] {"Remove", "Succeed", "Fail"};
                var toolbarOption = 0;
                if (_modulesObject.succeedOnExpiration) toolbarOption = 1;
                if (_modulesObject.failOnExpiration) toolbarOption = 2;
                var selectedOption = ToolbarOptions(barOptions, toolbarOption, false, 12, false, 200);

                _modulesObject.succeedOnExpiration = selectedOption == 1;
                _modulesObject.failOnExpiration = selectedOption == 2;
                EndRow();
            }
        }

        private void ShowAddQuestStep()
        {
            if (!_modulesObject.showQuestSteps) return;
            BackgroundColor(Color.yellow);
            if (Button("Add Quest Step", 120))
                _modulesObject.questSteps.Add(new QuestStep(_modulesObject));
            ResetColor();
        }

        private int MainPanelSelected => _modulesObject.menubarIndex;
        
        private void ShowQuestSteps()
        {
            if (MainPanelSelected != 0) return;
            
            Space();
            _modulesObject.showQuestSteps = StartVerticalBoxSection(_modulesObject.showQuestSteps, "Quest Steps");

            if (!_modulesObject.showQuestSteps)
            {
                EndVerticalBox();
                return;
            }

            Space();
            
            _modulesObject.questSteps.ForEach(questStep =>
            {
                BackgroundColorIf(questStep.show, Color.white, Color.black);
                StartVerticalBox();
                ShowQuestStep(questStep);
                if (questStep.show)
                {
                    Space();
                    ShowCompletionButtons(questStep);
                    ShowSuccessConditions(questStep);
                    ShowFailureConditions(questStep);
                    ShowSuccessRewards(questStep);
                    ShowFailureRewards(questStep);

                    Space();
                    ShowAddCondition(questStep);
                    ShowAddReward(questStep);
                }
                EndVerticalBox();
                Space();
                ResetColor();
            });
            
            ShowAddQuestStep();
            EndVerticalBox();
        }

        private void ShowSuccessConditions(QuestStep questStep)
        {
            if (questStep.toolbarIndex != 0) return;
            MessageBox($"You have {questStep.successConditions.Count} success conditions set. This QuestStep will " +
                       "be marked \"Succeeded\" when all conditions are met, so long as no failure conditions have " +
                       "previously been met.");
            questStep.successConditions.ForEach(x => ShowCondition(questStep, x));
        }

        private void ShowFailureConditions(QuestStep questStep)
        {
            if (questStep.toolbarIndex != 1) return;
            MessageBox($"You have {questStep.failureConditions.Count} failure conditions set. This QuestStep will " +
                       "be marked \"Failed\" when one or more of these conditions are met. It only takes one failed " +
                       "condition to fail the entire step!");
            questStep.failureConditions.ForEach(x => ShowCondition(questStep, x));
        }
        
        private void ShowSuccessRewards(QuestStep questStep)
        {
            if (questStep.toolbarIndex != 2) return;
            MessageBox("When this step succeeds, automatic rewards may occur, including point effects on " +
                       "stats, other effects on stats, conditions being added or removed, quests being added or " +
                       "automatically succeeding or failing regardless of their own conditions, and items being added.\n\n" +
                       $"If the quest \"Can Revert\" (currently {questStep.canRevert}), these rewards may occur multiple " +
                       "times!");
            questStep.successRewards.ForEach(x => ShowReward(questStep, x));
        }
        
        private void ShowFailureRewards(QuestStep questStep)
        {
            if (questStep.toolbarIndex != 3) return;
            MessageBox("When this step fails, automatic rewards may occur, including point effects on " +
                       "stats, other effects on stats, conditions being added or removed, quests being added or " +
                       "automatically succeeding or failing regardless of their own conditions, and items being added.\n\n" +
                       $"If the quest \"Can Revert\" (currently {questStep.canRevert}), these rewards may occur multiple " +
                       "times!");
            //questStep.failureConditions.ForEach(x => ShowCondition(questStep, x));
        }

        private QuestCondition[] resultsQuestConditions;
        
        
        //private string SearchStringQuestCondition => GetString("Quest Condition Search");
       
        
        private QuestReward[] resultsQuestRewards;
        private QuestReward[] cachedQuestRewards;
        
        private QuestReward[] CacheQuestRewards() => cachedQuestRewards;


        private void ShowSearch()
        {
            var tempString = SearchString<QuestCondition>();
            var newValue = SearchField<QuestCondition>();
            
            if (newValue != tempString)
                UpdateList();
        }
        
        private void ShowSearchReward()
        {
            var tempString = SearchString<QuestReward>();
            var newValue = SearchField<QuestReward>();

            if (newValue != tempString)
                UpdateListRewards();
        }

        private void UpdateList()
        {
            // If no search is provided, return
            if (string.IsNullOrWhiteSpace(SearchString<QuestCondition>()))
            {
                resultsQuestConditions = GameModuleObjects<QuestCondition>()
                    .Where(x => !string.IsNullOrWhiteSpace(x.objectName))
                    .OrderBy(x => x.objectName)
                    .ToArray();
                return;
            }
            
            // Do search string
            resultsQuestConditions = GameModuleObjects<QuestCondition>()
                .Where(x => !string.IsNullOrWhiteSpace(x.objectName))
                .Where(x => x.objectName.IndexOf(SearchString<QuestCondition>(), StringComparison.OrdinalIgnoreCase) >= 0 
                            || x.objectType.IndexOf(SearchString<QuestCondition>(), StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(x => x.objectName)
                .ToArray();
            
        }
        
        private void UpdateListRewards()
        {
            // If no search is provided, return
            if (string.IsNullOrWhiteSpace(SearchString<QuestReward>()))
            {
                resultsQuestRewards = GameModuleObjects<QuestReward>()
                    .Where(x => !string.IsNullOrWhiteSpace(x.objectName))
                    .OrderBy(x => x.objectName)
                    .ToArray();
                return;
            }
            
            // Do search string
            resultsQuestRewards = GameModuleObjects<QuestReward>()
                .Where(x => !string.IsNullOrWhiteSpace(x.objectName))
                .Where(x => x.objectName
                    .IndexOf(SearchString<QuestReward>(), StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(x => x.objectName)
                .ToArray();
            
        }

        private string[] ResultsQuestConditionsNames() 
            => resultsQuestConditions.Select(x => x.name).ToArray();
        private string[] ResultsQuestRewardNames() 
            => resultsQuestRewards.Select(x => x.name).ToArray();

        private string QuestStepsConditionsKey => "Quest Step Condition Index";
        private string QuestStepsRewardsKey => "Quest Step Reward Index";

        private QuestCondition SelectedQuestCondition()
        {
            if (resultsQuestConditions == null) return null;
            if (resultsQuestConditions.Length == 0) return null;
            if (resultsQuestConditions.Length < GetInt(QuestStepsConditionsKey))
                SetInt(QuestStepsConditionsKey, resultsQuestConditions.Length - 1);
            
            return resultsQuestConditions[GetInt(QuestStepsConditionsKey)];
        }
        
        private QuestReward SelectedQuestReward()
        {
            if (resultsQuestRewards == null) return null;
            if (resultsQuestRewards.Length == 0) return null;
            if (resultsQuestRewards.Length < GetInt(QuestStepsRewardsKey))
                SetInt(QuestStepsRewardsKey, resultsQuestRewards.Length - 1);
            
            return resultsQuestRewards[GetInt(QuestStepsRewardsKey)];
        }

        private bool QuestStepContainsSelectedCondition(QuestStep questStep) 
            => questStep.successConditions.Contains(SelectedQuestCondition()) 
               || questStep.failureConditions.Contains(SelectedQuestCondition());
        private bool QuestStepContainsSelectedReward(QuestStep questStep) 
            => questStep.successRewards.Contains(SelectedQuestReward()) 
               || questStep.failureRewards.Contains(SelectedQuestReward());
        private bool QuestContainsSelectedReward() 
            => _modulesObject.successRewards.Contains(SelectedQuestReward()) 
               || _modulesObject.failureRewards.Contains(SelectedQuestReward());

        private void ShowAddCondition(QuestStep questStep)
        {
            if (questStep.toolbarIndex > 1) return;
            BackgroundColor(Color.yellow);
            StartVerticalBox();
            Label($"Add Condition {symbolInfo}", "Add success or failure conditions. Each Quest Step will only succeed or " +
                                                 "fail if all of the conditions added here are successful.", 150, true);
            ShowSearch();

            StartRow();
            PopupSetInt(QuestStepsConditionsKey, ResultsQuestConditionsNames(), 275);
            
            var hasCondition = QuestStepContainsSelectedCondition(questStep);
            BackgroundColor(hasCondition ? Color.grey : Color.yellow);
            if (Button("Add Condition", 100) && !hasCondition)
            {
                if (questStep.toolbarIndex == 0) // Success Conditions
                    questStep.successConditions.Add(SelectedQuestCondition());
                else if (questStep.toolbarIndex == 1) // Failure Conditions
                    questStep.failureConditions.Add(SelectedQuestCondition());
                Cache();
            }
            EndRow();
            
            EndVerticalBox();
            
            ResetColor();
        }
        
        private void ShowAddReward(QuestStep questStep = null)
        {
            if (questStep?.toolbarIndex < 2) return;
            BackgroundColor(Color.yellow);
            StartVerticalBox();
            var tooltip = "Add success or failure rewards. Rewards will be handled by the owner " +
                          "of the Quest at the time that the Quest Step completes or fails. Important: If " +
                          "the Quest Step can revert, the rewards may be given multiple times.\n\n" +
                          "You can create your own Quest Reward types to handle rewards that are more specific " +
                          "to your project. See the online docs and videos for examples.";
            if (questStep == null)
            {
                tooltip = "Add success or failure rewards. Rewards will be handled by the owner " +
                          "of the Quest at the time that the Quest completes or fails and is \"Turned in\".\n\n" +
                          "You can create your own Quest Reward types to handle rewards that are more specific " +
                          "to your project. See the online docs and videos for examples.";
            }
            Label($"Add Reward {symbolInfo}", tooltip, 150, true);
            ShowSearchReward();

            StartRow();
            PopupSetInt(QuestStepsRewardsKey, ResultsQuestRewardNames(), 275);
            
            var hasReward = questStep == null ? QuestContainsSelectedReward() : QuestStepContainsSelectedReward(questStep);
            BackgroundColor(hasReward ? Color.grey : Color.yellow);
            if (Button("Add Reward", 100) && !hasReward)
            {
                if (questStep == null)
                {
                    if (_modulesObject.toolbarIndex == 1)
                        _modulesObject.successRewards.Add(SelectedQuestReward());
                    else if (_modulesObject.toolbarIndex == 2)
                        _modulesObject.failureRewards.Add(SelectedQuestReward());
                }
                else
                {
                    if (questStep.toolbarIndex == 2) // Success Rewards
                        questStep.successRewards.Add(SelectedQuestReward());
                    else if (questStep.toolbarIndex == 3) // Failure Rewards
                        questStep.failureRewards.Add(SelectedQuestReward());
                }
                
                Cache();
            }
            EndRow();
            
            EndVerticalBox();
            
            ResetColor();
        }

        private void ShowCondition(QuestStep questStep, QuestCondition questCondition)
        {   
            StartRow();
            BackgroundColor(Color.red);
            if (Button($"{symbolX}", 25))
            {
                questStep.successConditions.RemoveAll(x => x == questCondition);
                questStep.failureConditions.RemoveAll(x => x == questCondition);
                ExitGUI();
            }
            ResetColor();
            Undo.RecordObject(questCondition, "Undo Condition Changes");
            foreach (var drawer in _questConditionDrawers)
            {
                if (drawer.CanHandle(questCondition))
                {
                    drawer.Draw(questCondition);
                    break;
                }
            }
            //questConditionDrawer.Draw(questCondition);
            EndRow();
        }

        private void ShowReward(QuestStep questStep = null, QuestReward questReward = null)
        {   
            StartRow();
            BackgroundColor(Color.red);
            if (Button($"{symbolX}", 25))
            {
                if (questStep == null)
                {
                    if (ViewingSuccessRewards())
                        _modulesObject.successRewards.RemoveAll(x => x == questReward);
                    if (ViewingFailureRewards())
                        _modulesObject.failureRewards.RemoveAll(x => x == questReward);
                }
                else
                {
                    if (ViewingSuccessRewards(questStep))
                        questStep.successRewards.RemoveAll(x => x == questReward);
                    if (ViewingFailureRewards(questStep))
                        questStep.failureRewards.RemoveAll(x => x == questReward);
                }
                ExitGUI();
            }
            ResetColor();
            Undo.RecordObject(questReward, "Undo Reward Changes");
            foreach (var drawer in _questRewardDrawers)
            {
                if (drawer.CanHandle(questReward))
                {
                    drawer.Draw(questReward);
                    break;
                }
            }
            //questRewardDrawer.Draw(questReward);
            EndRow();
        }

        private void ShowCompletionButtons(QuestStep questStep)
        {
            var startValue = questStep.toolbarIndex;
            var barOptions = new[] {
                $"Success Conditions ({questStep.successConditions.Count})",
                $"Failure Conditions ({questStep.failureConditions.Count})", 
                $"Success Rewards ({questStep.successRewards.Count})",
                $"Failure Rewards ({questStep.failureRewards.Count})"
            };
            var toolbarOption = ToolbarMenu(barOptions, startValue, 12, true);
            
            questStep.toolbarIndex = toolbarOption;
        }

        private void ShowQuestStep(QuestStep questStep)
        {
            StartRow();
            BackgroundColorIf(questStep.show, Color.green, Color.grey);
            //if (Button($"{(questStep.show ? "•" : "-")}", 25))
            //    questStep.show = !questStep.show;

            questStep.show = OnOffButton(questStep.show);
            
            
            ResetColor();
            ShowStepUpDownButtons(questStep);
            
            if (questStep.show)
                DoShowingStep(questStep);
            else
            {
                DoNotShowingStep(questStep);
                return;
            }

            DoStepEventOptions(questStep);
            DoGameId(questStep);
            //DoTimeLimit(questStep); // Dec 29, 2022 -- moved this to the quest itself. Maybe bring it back later?

            DoStatus(questStep);
            
            DoStepDescription(questStep);
            
        }

        private void DoGameId(QuestStep questStep)
        {
            if (!questStep.RequiresGameId()) return;

            var tempOption = questStep.gameIdOption;
            StartRow();
            Label($"Game ID Source {symbolInfo}", "Choose the source of the required Game ID. It can be " +
                                                   "pulled automatically from the owner of the Quest, set manually via code, or " +
                                                   "set as a specific string here.", 120);
            var barOptions = new[] {"Use Owner", "Set Manually", "Set Now"};
            var toolbarOption = ToolbarOptions(barOptions, questStep.gameIdOption, false, 12, false, 300);
            questStep.gameIdOption = toolbarOption;
            EndRow();
            
            // Save the current string if we are changing modes. It has to be empty, but we want to be able to bring it back!
            if (questStep.gameIdOption != 2 && tempOption != questStep.gameIdOption)
                SaveAndClearGameId(questStep);

            if (questStep.gameIdOption == 0)
                MessageBox("The Game ID will be pulled automatically from the owner of this Quest.");

            if (questStep.gameIdOption == 1)
                MessageBox("You will need to set the Game ID value via code, once you know what it will be, " +
                           "perhaps when the GameQuest is created at runtime. The name of this QuestStep is required, using " +
                           "the code below. Note, if you change the name of the QuestStep, the code will need to be updated!" +
                           $"\n\nGameQuest.SetGameId(\"{questStep.name}\", gameId); // Replace \"gameId\" with the " +
                           "Game ID you'd like to use");

            if (questStep.gameIdOption == 2)
            {
                // If the GameId string is empty, and we've saved one before, set that now, bringing it back.
                if (String.IsNullOrEmpty(questStep.GameId()) &&
                    HasKey($"{_modulesObject.Uid()} step {questStep.name} saved Game ID"))
                    questStep.SetGameId(GetString($"{_modulesObject.Uid()} step {questStep.name} saved Game ID"));
                
                MessageBox("Set the Game ID here. Make sure the object which sends data to the Blackboard has" +
                           " the same Game ID!");
               
                // Let the user set the Game Id
                StartRow();
                Label("Game ID: ", 120);
                questStep.SetGameId(TextField(questStep.GameId(), 200));
                EndRow();
            }
        }

        // Save the current string in EditorPrefs, then clear it.
        private void SaveAndClearGameId(QuestStep questStep)
        {
            SetString($"{_modulesObject.Uid()} step {questStep.name} saved Game ID", questStep.GameId());
            questStep.ClearGameId();
        }

        private void DoStatus(QuestStep questStep)
        {
            // Note: This is currently just set here. Should we expose it for folks to set it themselves?
            // If you're reading this, and have a use case, let me know in the Discord!
            questStep.status = QuestStep.QuestStepStatus.InProgress;
        }

        // Dec 29, 2022 -- Moved time limit to the Quest itself, not the individual steps
        private void DoTimeLimit(QuestStep questStep)
        {
            //Label("TO DO: Time Limit Options");
        }

        private void DoStepEventOptions(QuestStep questStep)
        {
            questStep.sendQuestEvents = LeftCheck($"Send Quest Event {symbolInfo}", "If true, the QuestStep will send events as the status " +
                "changes. The QuestEvent object will have this QuestStep as it's QuestStep value -- if that value is null, it means the Quest " +
                "itself sent the QuestEvent. Your scripts will need to determine if they care, and what to do when QuestEvents they care about " +
                "are received.\n\nImportant: If this canRevert, then events may be sent multiple times for the same status. Your script can handle " +
                "this as needed, as sometimes this is desired, sometimes it may not be.", questStep.sendQuestEvents);
        }

        private void DoStepDescription(QuestStep questStep)
        {
            Label($"Description {symbolInfo}", "Optional. Description can be used in your project, if you " +
                                               "have a need for a description on each step of the quest.");
            questStep.description = TextArea(questStep.description, 250, 50);
        }

        private void DoNotShowingStep(QuestStep questStep)
        {
            ContentColor(_modulesObject.sequentialSteps ? Color.white : Color.grey);
            Label($"[{QuestStepIndex(questStep)}]", 30);
            ResetColor();
            Label($"{questStep.name}", 150);
            if (!ShowNoSuccessError(questStep))
            {
                Label("", 10);
                ShowCanRevert(questStep);
            }
            EndRow();
        }

        private void DoShowingStep(QuestStep questStep)
        {
            //StartRow();
            ShowRemoveQuestStep(questStep);
            ContentColor(_modulesObject.sequentialSteps ? Color.white : Color.grey);
            Label($"[{_modulesObject.questSteps.IndexOf(questStep)}] ", 30);
            ResetColor();
            var tempName = questStep.name;
            questStep.name = DelayedText(questStep.name, 200);
            if (String.IsNullOrWhiteSpace(questStep.name))
                questStep.name = tempName;

            ShowNoSuccessError(questStep);
            EndRow();
                
            StartRow();
            ShowCanRevert(questStep);
            EndRow();
        }

        private void ShowCanRevert(QuestStep questStep)
        {
            questStep.canRevert = LeftCheck($"Can Revert {symbolInfo}", "If true, the step will be checked for completion" +
                                                                        " even after it has initially been completed, to " +
                                                                        "ensure the conditions continue to be met. Otherwise, once " +
                                                                        "the step has succeeded, it will always be set to succeeded.", questStep.canRevert);
        }

        private bool ShowNoSuccessError(QuestStep questStep)
        {
            if (questStep.successConditions.Count > 0) return false;

            ContentColor(Color.red);
            Label("No success conditions have been defined!", true);
            ResetColor();
            return true;
        }

        private void ShowRemoveQuestStep(QuestStep questStep)
        {
            if (_modulesObject.Steps <= 1) return;
            BackgroundColor(Color.red);
            if (Button($"{symbolX}", 25))
            {
                _modulesObject.questSteps.Remove(questStep);
                ExitGUI();
            }
            ResetColor();
        }

        private int QuestStepIndex(QuestStep questStep) => _modulesObject.questSteps.IndexOf(questStep);
        
        private void ShowStepUpDownButtons(QuestStep questStep)
        {
            ColorsIf(CanMoveUp(questStep), Color.grey, Color.black, Color.white, Color.gray);
            if (Button($"{symbolArrowUp}", 25))
                Move(questStep, -1);
            ColorsIf(CanMoveDown(questStep), Color.grey, Color.black, Color.white, Color.gray);
            if (Button($"{symbolArrowDown}", 25))
                Move(questStep, 1);
            ResetColor();
        }

        private void Move(QuestStep questStep, int moveValue)
        {
            MoveItem(_modulesObject.questSteps, QuestStepIndex(questStep), moveValue);
            ExitGUI();
        }

        private bool CanMoveUp(QuestStep questStep) => QuestStepIndex(questStep) > 0;
        private bool CanMoveDown(QuestStep questStep) => QuestStepIndex(questStep) < _modulesObject.questSteps.Count - 1;

        private void ShowDictionariesHeader()
        {
            StartRow();
            ShowHeader("Dictionaries");
            BackgroundColor(Color.cyan);
            if (Button("Copy", 75))
            {
                Undo.RecordObject(_modulesObject, "Undo Copy Settings");
                SetString("Quest Dictionaries", _modulesObject.objectName);
            }

            // REPLACE CONTENT
            if (HasKey("Quest Dictionaries"))
            {
                BackgroundColor(Color.cyan);
                if (Button("Replace all values with " + GetString("Quest Dictionaries") + " Content",
                    "This will replace the current content with the copied content.",
                    300))
                {
                    var copyQuest = GameModuleObjects<Quest>().ToList()
                        .FirstOrDefault(x => x.objectName == GetString("Quest Dictionaries"));
                    if (copyQuest != null)
                    {
                        // Oct 8 2021 - Removed this option because Undo wasn't working, and that's a big problem
                        // since this would replace ALL the data. No bueno.
                        /*
                        if (KeyShift)
                        {
                            Undo.RecordObject(ThisObject, "Undo Paste Settings");
                            ReplaceContent(copyItemObject.dictionary);
                            ExitGUI();
                        }
                        */
                        Undo.RecordObject(_modulesObject, "Undo Paste Settings");
                        _modulesObject.dictionaries.ReplaceContentWith(copyQuest.dictionaries);
                    }
                }
                //Label("\"Shift-click\" to apply to all of type " + ThisObject.objectType);
            }
            EndRow();
            BackgroundColor(Color.white);
        }
        
        private void ShowHeader(string value) => LabelBig(value, 200, 18, true);
        
        public void ShowDictionary<T>(Dictionaries dictionaries, string objectName, string objectType, T Object) where T : ModulesScriptableObject
        {
            Undo.RecordObject(Object, "Undo Value Changes");
            dictionaries.Draw(_dictionariesDrawer, "Quest", objectName, objectType);
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
            SetNameOnLevels();
            EnsureOneQuestStep();
            if (_modulesObject.hasBeenSetup) return true;

            if (String.IsNullOrEmpty(_modulesObject.objectType)) return false;
            if (_modulesObject.dictionaries.keyValues == null) return false;
            
            Cache();
            UpdateList();
            UpdateListRewards();

            // Match settings with other objects of this type which all share the same settings (i.e. all turned on, otherwise, stay off)
            var quest = GameModuleObjectsOfType<Quest>(_modulesObject.objectType);
            MatchDictionary(quest);
            MatchStatAndSkills(quest);

            _modulesObject.hasBeenSetup = true;
            return true;
        }
        
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

        private void EnsureOneQuestStep()
        {
            if (_modulesObject.questSteps.Count > 0) return;
            _modulesObject.questSteps.Add(new QuestStep(_modulesObject));
        }
        
        private void SetNameOnLevels()
        {
            _modulesObject.modificationLevel.ownerName = _modulesObject.objectName;
        }

        private void MatchStatAndSkills(Quest[] quests)
        {
            var questObjectsOfThisType = quests.Length - 1;
            // Debug.Log("TO Do: Match Stat and Skills"); // Jun 27, 2023 -- I can't remember this TODO, I think we don't do this, because
            // the stat effects can be different for each quest object of this type. It's not like match dictionary.
        }

        private void MatchDictionary(Quest[] quests)
        {
            var questsOfThisType = quests.Length - 1;
            var keyValues = quests.Except(quests.Where(x => x == _modulesObject).ToArray()).SelectMany(x => x.dictionaries.keyValues).Select(x => x.key).Distinct().ToList();
            
            foreach (var keyValue in keyValues)
            {
                if (string.IsNullOrWhiteSpace(keyValue)) continue;
                if (ObjectsOfTypeWithDictionaryKeyValue<Quest>(_modulesObject.objectType, keyValue).Length != questsOfThisType)
                    continue;

                var firstObjectThatIsNotThis = quests.First(x => x != _modulesObject);

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