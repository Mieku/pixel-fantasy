using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using static InfinityPBR.Modules.GameModuleUtilities;

namespace InfinityPBR.Modules
{
    public class QuestConditionsDrawer : GameModulesDrawer
    {
        // -------------------------------
        // PROPERTIES AND VARIABLES -- UPDATE THESE FOR THIS TYPE
        // -------------------------------
        private QuestCondition _modulesObject;
        const string ThisTypePlural = "Quest Conditions";
        const string ThisType = "Quest Condition";
        private string ClassNamePlural => "QuestConditions";
        private string ClassName => "QuestCondition";
        private string DocsURL => "https://infinitypbr.gitbook.io/infinity-pbr/game-modules/quests/quest-conditions";
        private string DocsURLLabel => "Quest Conditions";
        private string[] GameModuleObjectTypes(bool recompute = false) => GameModuleObjectTypes<QuestCondition>(recompute);
        private QuestCondition[] GameModuleObjects(bool recompute = false) => GameModuleObjects<QuestCondition>(recompute);
        private QuestCondition[] GameModuleObjectsOfType(string type, bool recompute = false) => GameModuleObjectsOfType<QuestCondition>(type, recompute);
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
        private int MainPanelSelected => _modulesObject.menubarIndex;

        private List<QuestConditionDrawer> _questConditionDrawers = new List<QuestConditionDrawer>();
        private List<QuestRewardDrawer> _questRewardDrawers = new List<QuestRewardDrawer>();
        // -------------------------------

        // -------------------------------
        // METHODS -- UPDATE THESE FOR THIS TYPE
        // -------------------------------
        public void SetModulesObject(QuestCondition modulesObject, bool drawnByWindow = true)
        {
            _modulesObject = modulesObject;
            _menuBarOptions = new[] { 
                "Change this", 
                "Stats Effects",
                "Dictionaries" 
            };
            _fieldWidth = 200;
            _dictionariesDrawer = new DictionariesDrawerEditor(_fieldWidth);
            _conditionDrawer = new ConditionDrawerEditor(_fieldWidth);
            _statModificationLevelDrawer = new StatModificationLevelDrawerEditor(_fieldWidth);
            drawnByGameModulesWindow = drawnByWindow;
            
            GameModuleObjects<QuestCondition>(true);
            GameModuleObjects<QuestReward>(true);
            
            // Quest Condition Types
            var questConditionDrawerTypes = GetSubClassesOfQuestConditionDrawer();
            _questConditionDrawers = new List<QuestConditionDrawer>();
            foreach (var type in questConditionDrawerTypes)
            {
                var drawerInstance = CreateInstance(type) as QuestConditionDrawer;
                _questConditionDrawers.Add(drawerInstance);
            }
            
            // Quest Reward Types
            var questRewardDrawerTypes = GetSubClassesOfQuestRewardDrawer();
            _questRewardDrawers = new List<QuestRewardDrawer>();
            foreach (var type in questRewardDrawerTypes)
            {
                var drawerInstance = CreateInstance(type) as QuestRewardDrawer;
                _questRewardDrawers.Add(drawerInstance);
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
        
        //private void ShowButtons() => _modulesObject.menubarIndex = ToolbarMenuMain(_menuBarOptions, _modulesObject.menubarIndex);
        
        // -------------------------------

        //protected QuestConditionDrawer questConditionDrawer;
        
        // -------------------------------
        // DRAW METHODS -- UPDATE THESE FOR THIS TYPE
        // -------------------------------
        public void Draw()
        {
            if (_modulesObject == null) return;
            
            if (!InitialSetup()) return;
            
            BeginChangeCheck();
            
            // If this is a duplicate uid and the button is clicked to reset it, redo Initial Setup
            if (!drawnByGameModulesWindow && CheckUid(_modulesObject.Uid(), _modulesObject, _duplicateUid))
                _initialSetup = false;
            
        
          

            // Loop through available drawers to find one that can handle the object.
            foreach (var drawer in _questConditionDrawers) // This needs to be defined and filled appropriately.
            {
                if (drawer.CanHandle(_modulesObject))
                {
                    drawer.Draw(_modulesObject);
                    break;
                }
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

        private void CheckConflicts()
        {
            if (!_modulesObject) return;
            if (_modulesObject.dictionaries?.keyValues == null) return;
        }
    }
}