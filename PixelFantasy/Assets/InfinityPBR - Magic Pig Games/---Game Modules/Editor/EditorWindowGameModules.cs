using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEditor;
using UnityEngine;
using static InfinityPBR.InfinityEditor;
using static InfinityPBR.Modules.PropertyCodeUtility;
using static InfinityPBR.Modules.GameModuleUtilities;

namespace InfinityPBR.Modules
{
    public class EditorWindowGameModules : EditorWindow
    {
        private int _fieldWidth = 200;
        private EditorWindow _thisEditorWindow;
        
        private Vector2 _scrollPosition;
        private const float ScrollbarWidth = 20f; // adjust to match the actual scrollbar width

        private Dictionary<string, string> _buttonSubtext = new Dictionary<string, string>();
        
        // Editors for each Game Modules Type
        private LookupTablesManager _lookupTablesManager;
        private VoicesManager _voicesManager;
        private LootItemsManager _lootItemsManager;
        private LootBoxesManager _lootBoxesManager;
        private QuestRewardsManager _questRewardsManager;
        private QuestConditionsManager _questConditionsManager;
        private QuestsManager _questsManager;
        private ConditionsManager _conditionsManager;
        private ItemObjectsManager _itemObjectsManager;
        private ItemAttributesManager _itemAttributesManager;
        private StatsManager _statsManager;
        private MasteryLevelsManager _masteryLevelsManager;

        private void InitializeEditorWindows()
        {
            _lookupTablesManager = new LookupTablesManager();
            _voicesManager = new VoicesManager();
            _lootItemsManager = new LootItemsManager();
            _lootBoxesManager = new LootBoxesManager();
            _questRewardsManager = new QuestRewardsManager();
            _questConditionsManager = new QuestConditionsManager();
            _questsManager = new QuestsManager();
            _conditionsManager = new ConditionsManager();
            _itemAttributesManager = new ItemAttributesManager();
            _itemObjectsManager = new ItemObjectsManager();
            _statsManager = new StatsManager();
            _masteryLevelsManager = new MasteryLevelsManager();
        }
        
        // Drawers for each Game Modules Type
        private DictionariesDrawer _dictionariesDrawer;
        private ItemAttributeDrawer _itemAttributeDrawer;
        private ConditionDrawer _conditionDrawer;
        private QuestsDrawer _questsDrawer;
        private QuestConditionDrawer _questConditionDrawer;
        private QuestRewardDrawer _questRewardDrawer;
        
        private void SetDictionaryDrawer() => _dictionariesDrawer = new DictionariesDrawerEditor(_fieldWidth);
        
        private static bool LoadGameModulesWindowAtStartup
        {
            get => EditorPrefs.GetBool("Show Game Modules Automatically", true);
            set => EditorPrefs.SetBool("Show Game Modules Automatically", value);
        }

        private static bool LoadedGameModulesWindowInSession = false;

        [InitializeOnLoadMethod]
        private static void LoadEditorOnStartup()
        {
            if (!LoadGameModulesWindowAtStartup || LoadedGameModulesWindowInSession) return;

            // We only want this to load automatically the first time the Editor loads
            if (SessionState.GetBool("AlreadyLoaded", false) == true)
                return;
            
            SessionState.SetBool("AlreadyLoaded", true);
            LoadedGameModulesWindowInSession = true;
            EditorApplication.delayCall += InitGameModules;
        }

        [MenuItem("Game Modules/⭐️Game Modules Manager Window", false, 1)]
        private static void InitGameModules() => ShowEditorWindow("Game Modules");

        [MenuItem("Game Modules/Stats", false, 2)]
        private static void InitStats() => ShowEditorWindow("Stats");
        
        [MenuItem("Game Modules/Mastery Levels ", false, 2)]
        private static void InitMasteryLevels() => ShowEditorWindow("Mastery Levels");
        
        [MenuItem("Game Modules/Item Objects", false, 2)]
        private static void InitItemObjects() => ShowEditorWindow("Item Objects");
        
        [MenuItem("Game Modules/Item Attributes", false, 2)]
        private static void InitItemAttributes() => ShowEditorWindow("Item Attributes");
        
        [MenuItem("Game Modules/Conditions ", false, 2)]
        private static void InitConditions() => ShowEditorWindow("Conditions");
        
        [MenuItem("Game Modules/Quests ", false, 2)]
        private static void InitQuests() => ShowEditorWindow("Quests");
        
        [MenuItem("Game Modules/Quest Conditions ", false, 2)]
        private static void InitQuestConditions() => ShowEditorWindow("Quest Conditions");
        
        [MenuItem("Game Modules/Quest Rewards ", false, 2)]
        private static void InitQuestRewards() => ShowEditorWindow("Quest Rewards");
        
        [MenuItem("Game Modules/Loot Boxes ", false, 2)]
        private static void InitLootBoxes() => ShowEditorWindow("Loot Boxes");
        
        [MenuItem("Game Modules/Loot Items ", false, 2)]
        private static void InitLootItems() => ShowEditorWindow("Loot Items");
        
        [MenuItem("Game Modules/Voices ", false, 2)]
        private static void InitVoices() => ShowEditorWindow("Voices");
        
        [MenuItem("Game Modules/Lookup Tables ", false, 2)]
        private static void InitLookupTables() => ShowEditorWindow("Lookup Tables");

        const string HighlightColorMuted = "<color=#889999><b>";
        const string CloseHighlightMuted = "</b></color>";
        
        private bool GameModulesShowFaq
        {
            get => EditorPrefs.GetBool("Game Modules Show FAQ", false);
            set => EditorPrefs.SetBool("Game Modules Show FAQ", value);
        }
        
        private bool GameModulesShowRateAndReview
        {
            get => EditorPrefs.GetBool("Game Modules Show Rate and Review", true);
            set => EditorPrefs.SetBool("Game Modules Show Rate and Review", value);
        }
        
        private bool GameModulesShowOptions
        {
            get => EditorPrefs.GetBool("Game Modules Show Options", false);
            set => EditorPrefs.SetBool("Game Modules Show Options", value);
        }
        
        private void PopulateButtonSubtext()
        {
            _buttonSubtext.Clear();
            _buttonSubtext.Add("Stats", 
                $"{HighlightColorMuted}Stats{CloseHighlightMuted}, {HighlightColorMuted}skills{CloseHighlightMuted}, {HighlightColorMuted}counters{CloseHighlightMuted} & more");
            _buttonSubtext.Add("Mastery Levels", 
                $"Skill advancement & effects on {HighlightColorMuted}Stats{CloseHighlightMuted}");
            _buttonSubtext.Add("Item Objects", 
                "Things to pick up, equip, or use");
            _buttonSubtext.Add("Item Attributes", 
                $"Modifiers for {HighlightColorMuted}Item Objects{CloseHighlightMuted} and anything else");
            _buttonSubtext.Add("Conditions", 
                $"Automatic {HighlightColorMuted}Stat{CloseHighlightMuted} modifications");
            _buttonSubtext.Add("Quests", 
                $"Something to do, with {HighlightColorMuted}Quest Rewards{CloseHighlightMuted}");
            _buttonSubtext.Add("Quest Conditions", 
                $"Success and fail conditions for {HighlightColorMuted}Quests{CloseHighlightMuted}");
            _buttonSubtext.Add("Quest Rewards",
                $"Good and bad rewards for {HighlightColorMuted}Quests{CloseHighlightMuted} and anything else");
            _buttonSubtext.Add("Loot Boxes", 
                "Dynamic, controllable, randomized Loot Boxes");
            _buttonSubtext.Add("Loot Items", 
                $"{HighlightColorMuted}Item Objects{CloseHighlightMuted} for {HighlightColorMuted}Loot{CloseHighlightMuted}, {HighlightColorMuted}Quest Rewards{CloseHighlightMuted}, and anything else");
            _buttonSubtext.Add("Voices", 
                "Easy audio player");
            _buttonSubtext.Add("Lookup Tables", 
                "Put a value in, get a value out");
            _buttonSubtext.Add("Property Code", 
                "Auto-generating Game Module properties");
            _buttonSubtext.Add("Object Reference", 
                "Cache non-serializable objects");
        }
        
        private static void ShowEditorWindow(string windowToShow)
        {
            SetString("Game Modules Window Selected", windowToShow);
            var window = (EditorWindowGameModules)GetWindow(typeof(EditorWindowGameModules));
            window.Show();
            window.Focus(); // Focus the window
        }

        private void Awake()
        {
            _thisEditorWindow = this;
            SetDictionaryDrawer();
            titleContent = new GUIContent("Game Modules");
            DoChecks();
            ResetCache();
            InitializeEditorWindows();
        }

        

        private void DoChecks()
        {
            
        }

        private void OnFocus()
        {
            InitializeEditorWindows();
            AutoSelect();
            ResetCache();
            PopulateButtonSubtext();
        }

        private void ResetCache()
        {
            EditorPrefs.SetBool("Stats Manager Cache", true);
            EditorPrefs.SetBool("Condition Manager Cache", true);
            EditorPrefs.SetBool("Item Object Manager Cache", true);
            EditorPrefs.SetBool("Item Attribute Manager Cache", true);
            EditorPrefs.SetBool("Mastery Levels Manager Cache", true);
            EditorPrefs.SetBool("Voices Manager Cache", true);
            EditorPrefs.SetBool("Loot Box Manager Cache", true);
            EditorPrefs.SetBool("Loot Items Manager Cache", true);
            EditorPrefs.SetBool("Quest Manager Cache", true);
            EditorPrefs.SetBool("Quest Condition Manager Cache", true);
            EditorPrefs.SetBool("Quest Reward Manager Cache", true);
            EditorPrefs.SetBool("Lookup Table Manager Cache", true);
            SessionState.SetString("Property Code Not Found", "");
        }
        
        private void AutoSelect()
        {
            if (!HasKey("Game Modules Window Selected")) return;

            var windowSelected = GetString("Game Modules Window Selected");
        }
        
        void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            ShowHeader();
            Space();
            
            DrawLookupTables();
            DrawVoices();
            DrawQuestRewards();
            DrawLootItems();
            DrawLootBoxes();
            DrawQuestConditions();
            DrawQuests();
            DrawStats();
            DrawItemObjects();
            DrawItemAttributes();
            DrawConditions();
            DrawMasteryLevels();
            DrawPropertyCode();
            DrawObjectReference();
            
            EditorGUILayout.EndScrollView();
        }

        void OnEnable()
        {
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
        }

        void OnDisable()
        {
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
        }

        public void OnBeforeAssemblyReload()
        {
            if (EditorPrefs.GetBool("Auto-Create Properties.cs", true))
            {
                CreatePropertyCode();
                //PopulateObjectReference();
                ExportEnumCode();
            }
            //Debug.Log("Before Assembly Reload");
        }
        

        public void OnAfterAssemblyReload()
        {
            //Debug.Log("After Assembly Reload");
        }

        private readonly string[] _moduleNames = {"Stats", "Mastery Levels", "Item Objects", "Item Attributes"
            , "Conditions", "Quests", "Quest Conditions", "Quest Rewards", "Loot Boxes", "Loot Items", "Voices"
            , "Lookup Tables", "Property Code", "Object Reference"};
        private int _selectedModule;
        private readonly float _minButtonWidth = 150;

        private string WindowSelected => HasKey("Game Modules Window Selected")
            ? GetString("Game Modules Window Selected")
            : "Game Modules";
        
        private void ShowHeader()
        {
            StartRow();
            BackgroundColor(Color.magenta);
            if (WindowSelected != "Game Modules"
                && Button($"{symbolCarrotLeft}", 25))
            {
                EditorPrefs.SetString("Game Modules Window Selected", "Game Modules");
                ExitGUI();
            }
            ResetColor();
            Header2(WindowSelected);
            ShowLinks();
            EndRow();

            if (WindowSelected != "Game Modules")
            {
                CacheMainWindow = true;
                return;
            }

            DoCacheMainWindow();
            ShowModuleButtons();
            Space();
            ShowWelcomeMessage();
            BlackLine();
            ShowOptions();
            BlackLine();
            ShowTips();
            BlackLine();
            ShowRateAndReview();
        }

        private static bool CacheMainWindow;
        private static int StatsCount;
        private static int MasteryLevelsCount;
        private static int ItemObjectsCount;
        private static int ItemAttributesCount;
        private static int ConditionsCount;
        private static int QuestsCount;
        private static int QuestConditionsCount;
        private static int QuestRewardsCount;
        private static int LootBoxesCount;
        private static int LootItemsCount;
        private static int VoicesCount;
        private static int LookupTablesCount;

        private void DoCacheMainWindow()
        {
            if (!CacheMainWindow) return;
            CacheMainWindow = false;

            StatsCount = GameModuleObjects<Stat>().Length;
            MasteryLevelsCount = GameModuleObjects<MasteryLevels>().Length;
            ItemObjectsCount = GameModuleObjects<ItemObject>().Length;
            ItemAttributesCount = GameModuleObjects<ItemAttribute>().Length;
            ConditionsCount = GameModuleObjects<Condition>().Length;
            QuestsCount = GameModuleObjects<Quest>().Length;
            QuestConditionsCount = GameModuleObjects<QuestCondition>().Length;
            QuestRewardsCount = GameModuleObjects<QuestReward>().Length;
            LootBoxesCount = GameModuleObjects<LootBox>().Length;
            LootItemsCount = GameModuleObjects<LootItems>().Length;
            VoicesCount = GameModuleObjects<Voices>().Length;
            LookupTablesCount = GameModuleObjects<LookupTable>().Length;
        }

        private void ShowRateAndReview()
        {
            StartRow();
            GameModulesShowRateAndReview = OnOffButton(GameModulesShowRateAndReview);
            Header2("Please rate & review, and join the Discord");
            EndRow();
            
            if (!GameModulesShowRateAndReview) return;

            Space();
            Label("If you like <b>Game Modules</b>, <color=#99ffff>please provide a <b>5-star rating</b> and positive " +
                  "review</color> on the Asset Store. It really helps to let other users know <b>Game Modules</b> is a " +
                  "quality tool!", ContentWidth, false, true, true);
            Space();
            Label("If you don't feel this is worthy of a 5-star rating and review, please <color=#99ffff>come to the <b>Discord</b></color> and let me know what you think could be " +
                  "improved. Your feedback will help me make <b>Game Modules</b> better for everyone!", ContentWidth, false, true, true);
            Space();
            
            StartRow();
            LinkToDocs("https://assetstore.unity.com/packages/slug/246034?aid=1100lxWw&pubref=GMRR", "Leave a Review");
            LinkToDocs("https://discord.com/invite/cmZY2tH", "Join the Discord");
            EndRow();
        }

        private void ShowTips()
        {
            
            StartRow();
            GameModulesShowFaq = OnOffButton(GameModulesShowFaq);
            Header2("FAQs & Tips");
            EndRow();

            if (!GameModulesShowFaq) return;
            
            Space();
            Header3("<color=#99ffff>Hold \"shift\" to change all of type</color>", true);
            Label("Many of the informational buttons and options when viewing lists of Game Modules objects allow you " +
                  "to update the value without opening the object itself. In some cases, <color=#99ffff>holding \"Shift\" while you " +
                  "select the new option will also update all the other objects of the same ObjectType.</color>", ContentWidth, false, true, true);
            
            Space();
            Header3("<color=#99ffff>Change the ObjectType</color> of a Game Modules object", true);
            Label("The \"ObjectType\" of a Game Modules object (Stat, Item Object, Condition, etc) is the name of " +
                  "the project folder that the object is in. To change the ObjectType, <color=#99ffff>move the object to the " +
                  "folder with the ObjectType name</color>. You can create a new folder to create a new ObjectType -- ObjectTypes will show in the system " +
                  "only if there is at least one object in that folder.", ContentWidth, false, true, true);

            Space();
            Header3("<color=#99ffff>Change the ObjectName</color> of a Game Modules Object", true);
            Label("The \"ObjectName\" of a Game Modules object (Stat, Item Object, Condition, etc) is the name of " +
                  "the ScriptableObject. To change the ObjectName, <color=#99ffff>rename the ScriptableObject in your project view</color>. " +
                  "<i>Keep in mind Game Modules objects must have distinct names.</i>", ContentWidth, false, true, true);

            
            Space();
            Header3("<color=#99ffff>Delete a Game Modules object</color>", true);
            Label("Each object is a ScriptableObject found in your project. To delete an object, just <color=#99ffff>delete the " +
                  "ScriptableObject</color>. <i>Please backup before making deletions!</i>", ContentWidth, false, true, true);

            Space();
        }

        // March 15 2024 -- do the others if you want, no big deal.
        private string _setConsoleString = "Show Console Messages in Edit Mode";
        public bool ShowConsoleMessages => EditorPrefs.GetBool(_setConsoleString, false);

        private string _loadWindowAtStartupString = "Load Game Modules Window at Startup";
        public bool ShowWindowAtStartup => EditorPrefs.GetBool(_loadWindowAtStartupString, false);
        
        private string _autoCreatePropertiesString = "Auto-Create Properties.cs";
        public bool ShowAutoCreateProperties => EditorPrefs.GetBool(_autoCreatePropertiesString, true);
        
        private string _showDictionariesMigrationString = "Display V4 Dictionary Migration Info";
        public bool ShowDictionariesMigration => EditorPrefs.GetBool(_showDictionariesMigrationString, false);
        
        private void ShowOptions()
        {
            StartRow();
            GameModulesShowOptions = OnOffButton(GameModulesShowOptions);
            Header2("Options");
            EndRow();
            
            if (!GameModulesShowOptions) return;
            
            ShowOption(_loadWindowAtStartupString,
                "When true, the Game Modules window will be opened when the project is loaded. I suggest leaving " +
                "this on, for ease of use.",
                _loadWindowAtStartupString,
                ShowWindowAtStartup);
            
            ShowOption(_autoCreatePropertiesString,
                $"The Properties.cs class is generated automatically by Game Modules during " +
                $"OnBeforeAssemblyReload(), unless this is toggled off. You can manually create the class on the " +
                $"\"Property Code\" tab.",
                _autoCreatePropertiesString,
                ShowAutoCreateProperties);
            
            ShowOption(_showDictionariesMigrationString,
                "Game Modules 4 is a major update with breaking changes. I was able to add a migration option " +
                "for the older Dictionaries structure to the new, allowing for a one-click migration, with " +
                "relatively simple cleanup from there. Toggle this on to see the options (in pink) when viewing " +
                "Dictionaries.",
                _showDictionariesMigrationString,
                ShowDictionariesMigration);

            ShowOption(_setConsoleString,
                "When true, the console will print messages as you edit objects, which will help pin point " +
                "where in the code the operations are happening. Use this for debugging, and sending me (the dev) info " +
                "about what may be broken in Discord.",
                _setConsoleString,
                ShowConsoleMessages);
        }

        private void ShowOption(string label, string tooltip, string prefsString, bool prefsValue)
        {
            StartRow();
            var cacheValue = prefsValue;
            SetBool(prefsString
                , LeftCheck($"{label} {symbolInfo}",
                    $"{tooltip}",
                    prefsValue));
            var newValue = EditorPrefs.GetBool(prefsString, false);
            
            if (cacheValue != newValue)
            {
                DebugConsoleMessage($"Showing Console Messages option is now {newValue}.");
            }
            EndRow();
        }
        

        private void ShowWelcomeMessage()
        {
            Header2("Welcome to Game Modules!");
            Label("Select a module to get started. Click the <color=#99ffff>\"Docs\"</color> link to " +
                  "view our extensive documentation, <color=#99ffff>\"Tutorials\"</color> to view our YouTube tutorials, and " +
                  "<color=#99ffff>join the Discord</color> to chat with us and other users.", ContentWidth, false, true, true);
        }

        private static int WindowWidth => (int)EditorGUIUtility.currentViewWidth;
        private static int ContentWidth => WindowWidth - 0 - ScrollBarWidth;
        private static int ScrollBarWidth => 20; // Change this in the future if we can figure out how to actively check for the scroll bar.
        
        private void ShowModuleButtons()
        {
            var windowRightMargin = 10;
            
            var buttonsPerRow = Mathf.FloorToInt((WindowWidth - windowRightMargin) / _minButtonWidth);
            buttonsPerRow = buttonsPerRow == 0 ? 1 : buttonsPerRow;
            var buttonWidth = (WindowWidth - windowRightMargin) / buttonsPerRow;

            float buttonsInRow = 0;
            StartRow();
            foreach (var gameModule in _moduleNames)
            {
                if (buttonsInRow >= buttonsPerRow)
                {
                    EndRow();
                    StartRow();
                    buttonsInRow = 0; // Reset the total button width
                }

                var headerColor = HightlightNoItems(gameModule) ? "#99ffff" : "#ffffff";
                var buttonLabel = $"<size=16><b><color={headerColor}>{gameModule}</color></b></size>\n" +
                                  $"<size=10><i><color=#777777>{_buttonSubtext[gameModule]}</color></i></size>";
                
                buttonsInRow += 1;
                
                if (!ButtonWordWrap(buttonLabel, (int)buttonWidth, 65, true))
                {
                    //ResetColor();
                    continue;
                }
                //ResetColor();

                ResetCache();
                EditorPrefs.SetString("Game Modules Window Selected", gameModule);
                ExitGUI();
            }
            EndRow();
        }

        // July 29 2023 -- was thinking of somehow highlighting buttons where there are no items. However, I'm not sure
        // that adds any real value, as it could be annoying if a user never plans on using a module type, but keeps
        // getting their eyes drawn to it.
        private bool HightlightNoItems(string gameModule)
        {
            return false;
        }

        private void ShowLinks()
        {
            var tempWindowSelected = WindowSelected;
            var docsLink = WindowSelected switch
            {
                "Stats" => "https://infinitypbr.gitbook.io/infinity-pbr/game-modules/stats-and-skills",
                "Mastery Levels" => "https://infinitypbr.gitbook.io/infinity-pbr/game-modules/stats-and-skills/mastery-levels",
                "Loot Boxes" => "https://infinitypbr.gitbook.io/infinity-pbr/game-modules/loot",
                "Loot Items" => "https://infinitypbr.gitbook.io/infinity-pbr/game-modules/loot",
                "Item Objects" => "https://infinitypbr.gitbook.io/infinity-pbr/game-modules/items",
                "Item Attributes" => "https://infinitypbr.gitbook.io/infinity-pbr/game-modules/items",
                "Conditions" => "https://infinitypbr.gitbook.io/infinity-pbr/game-modules/conditions",
                "Quests" => "https://infinitypbr.gitbook.io/infinity-pbr/game-modules/quests",
                "Quest Conditions" => "https://infinitypbr.gitbook.io/infinity-pbr/game-modules/quests/quest-conditions",
                "Quest Rewards" => "https://infinitypbr.gitbook.io/infinity-pbr/game-modules/quests/quest-rewards",
                "Voices" => "https://infinitypbr.gitbook.io/infinity-pbr/game-modules/voices",
                "Lookup Tables" => "https://infinitypbr.gitbook.io/infinity-pbr/game-modules/lookup-table",
                "Property Code" => "https://infinitypbr.gitbook.io/infinity-pbr/game-modules/property-code",
                "Object Reference" => "https://infinitypbr.gitbook.io/infinity-pbr/game-modules/dictionaries/object-reference",
                _ => "https://infinitypbr.gitbook.io/infinity-pbr/"
            };
            LinkToDocs(docsLink, $"{WindowSelected} Docs");
            LinkToDocs("https://www.youtube.com/watch?v=4KZlCPboA5c&list=PLCK7vP-GxBCm8l-feq-aF5_dWnFda7cvQ", "Tutorials");
            LinkToDocs("https://discord.com/invite/cmZY2tH", "Discord");

            if (WindowSelected != tempWindowSelected)
            {
                ResetCache();
            }
        }
        
        private void DrawObjectReference()
        {
            if (WindowSelected != "Object Reference") return;
            BlackLine();
            Header1("Object Reference");
            Label("The <color=#99ffff><b>ObjectReference</b></color> Scriptable Object is auto-created and stores references " +
                  "to objects which can't be serialized, but are referenced in your <color=#99ffff><b>Dictionaries</b></color>.", false, true, true);
            Space();
            Label("Visit the <b>Object Reference Docs</b> linked above for more info.", false, true, true);

            StartRow();
            if (Button("View ObjectReference", 150))
            {
                PingObjectReference();
            }

            if (Button("Check for Missing References", 200))
            {
                var objectReference = GetObjectInProject<ScriptableObject>("ObjectReference") as ObjectReference;
                if (objectReference != null) objectReference.PopulateMissingReferences();
                //PingObjectReference();
            }
            EndRow();
        }

        private void PingObjectReference()
        {
            var foundObjectToPing = PingObjectNamed<ScriptableObject>("ObjectReference");
            if (foundObjectToPing)
                return;
            
            ObjectReferenceCreator.CreateObjectReference();
        }


        private void DrawPropertyCode()
        {
            if (WindowSelected != "Property Code") return;
            BlackLine();
            Header1("Property Code");
            Label("The <color=#99ffff><b>Properties.cs</b></color> class is generated on assembly reload, and contains " +
                  "helpful properties leading to all of the Game Modules <color=#99ffff><b>Scriptable Objects</b></color> " +
                  "and <color=#99ffff><b>Uid()</b></color> values.", false, true, true);
            Space();
            Label("Visit the <b>Property Code Docs</b> linked above for examples of how this useful script can be used.", false, true, true);

            StartRow();
            if (Button("View Properties.cs", 150))
            {
                PingProperties();
            }

            if (Button("Create Now", 150))
            {
                CreatePropertyCode();
                PingProperties();
            }
            EndRow();

            Label(SessionState.GetString("Property Code Not Found", ""), false, false, true);
        }

        private void PingProperties()
        {
            var foundObjectToPing = PingObjectNamed<MonoScript>("Properties");
            if (foundObjectToPing) return;
            
            SessionState.SetString("Property Code Not Found",
                "Unable to locate <b><color=#99ffff>Properties.cs</color></b>. Perhaps it has not yet been exported.");
        }

        private void DrawLookupTables()
        {
            if (WindowSelected != "Lookup Tables") return;
            _lookupTablesManager.Draw(new Rect(0, 0, position.width, position.height));
        }
        
        private void DrawVoices()
        {
            if (WindowSelected != "Voices") return;
            _voicesManager.Draw(new Rect(0, 0, position.width, position.height));
        }
        
        private void DrawLootItems()
        {
            if (WindowSelected != "Loot Items") return;
            _lootItemsManager.Draw(new Rect(0, 0, position.width, position.height));
        }
        
        private void DrawLootBoxes()
        {
            if (WindowSelected != "Loot Boxes") return;
            _lootBoxesManager.Draw(new Rect(0, 0, position.width, position.height));
        }

        private void DrawQuestRewards()
        {
            if (WindowSelected != "Quest Rewards") return;
            _questRewardsManager.Draw(new Rect(0, 0, position.width, position.height));
        }

        private void DrawQuestConditions()
        {
            if (WindowSelected != "Quest Conditions") return;
            _questConditionsManager.Draw(new Rect(0, 0, position.width, position.height));
        }
        
        private void DrawQuests()
        {
            if (WindowSelected != "Quests") return;
            _questsManager.Draw(new Rect(0, 0, position.width, position.height));
        }
        
        private void DrawStats()
        {
            if (WindowSelected != "Stats") return;
            _statsManager.Draw(new Rect(0, 0, position.width, position.height));
        }
        
        private void DrawItemObjects()
        {
            if (WindowSelected != "Item Objects") return;
            _itemObjectsManager.Draw(new Rect(0, 0, position.width, position.height));
        }

        private void DrawItemAttributes()
        {
            if (WindowSelected != "Item Attributes") return;
            _itemAttributesManager.Draw(new Rect(0, 0, position.width, position.height));
        }
        
        private void DrawConditions()
        {
            if (WindowSelected != "Conditions") return;
            _conditionsManager.Draw(new Rect(0, 0, position.width, position.height));
        }

        private void DrawMasteryLevels()
        {
            if (WindowSelected != "Mastery Levels") return;
            _masteryLevelsManager.Draw(new Rect(0, 0, position.width, position.height));
        }
    }
}
