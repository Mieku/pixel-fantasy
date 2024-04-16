using System.Linq;
using UnityEditor;
using UnityEngine;
using static InfinityPBR.InfinityEditor;
using static InfinityPBR.Modules.GameModuleUtilities;

namespace InfinityPBR.Modules
{
    public class EditorWindowQuestObject : EditorWindow
    {
        private int typeIndex;
        private string questType;
        private int fieldWidth = 200;
        private EditorWindow thisEditorWindow;
        
        public DictionariesDrawer dictionariesDrawer;
        private Vector2 scrollPosition;

        private Quest[] cachedQuestObjects;
        private Quest[] cachedQuestTypes;
        private string questTypeName;
        
        private string[] menuBarOptions = {
            "Dictionaries"
        };

        [MenuItem("Window/Game Modules/Quest Objects")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            EditorWindowQuestObject window = (EditorWindowQuestObject)GetWindow(typeof(EditorWindowQuestObject));
            window.Show();
        }

        private void Awake()
        {
            thisEditorWindow = this;
            titleContent = new GUIContent("Quest Objects");
            SetDictionaryDrawer();
            ResetCache();
        }

        public void Cache()
        {
            cachedQuestObjects = GameModuleObjects<Quest>(true);
        }

        public Quest[] CachedQuestObjects() => GameModuleObjects<Quest>();

        public Quest[] CachedQuestType(string newType)
        {
            if (cachedQuestTypes == null)
                cachedQuestTypes = GameModuleObjectsOfType<Quest>(newType);
            if (newType != questTypeName)
            {
                cachedQuestTypes = GameModuleObjectsOfType<Quest>(newType);
                questTypeName = newType;
            }

            return cachedQuestTypes;
        }

        private void OnEnable()
        {
            Undo.undoRedoPerformed += UndoWasDone;
        }

        private void AutoSelect()
        {
            if (!HasKey("Quest Object Selected")) return;

            var quest = GameModuleObjects<Quest>()
                .FirstOrDefault(x => x.objectName == GetString("Quest Object Selected"));

            if (quest == null) return;
            
            var objectTypes = GameModuleObjectTypes<Quest>();
            
            for (int i = 0; i < objectTypes.Length; i++)
            {
                if (objectTypes[i] != quest.objectType) continue;

                typeIndex = i;
                break;
            }

            DeleteKey("Quest Object Selected");
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= UndoWasDone;
        }

        private void UndoWasDone()
        {
            
        }

        private void OnFocus()
        {
            AutoSelect();
            ResetCache();
            Cache();
        }

        private void ResetCache()
        {
            if (string.IsNullOrWhiteSpace(questType)) return;
            Debug.Log($"Resetting cache for {GameModuleObjectTypes<Quest>()[typeIndex]}");
            if (dictionariesDrawer == null)
            {
                Debug.Log("dictionariesDrawer is null");
                SetDictionaryDrawer();
            }
            dictionariesDrawer.ResetCache("Quest", GameModuleObjectTypes<Quest>()[typeIndex]);

        }

        private void SetDictionaryDrawer()
        {
            Debug.Log("SetDictionaryDrawer");
            dictionariesDrawer = new DictionariesDrawerEditor(fieldWidth);
            Debug.Log($"DictionariesDrawerEditor created. Is null? {dictionariesDrawer == null}");
        }

        void OnGUI()
        {
            if (NoTypes()) return;

            CheckForNullDrawer();

            ShowHeader();
            Space();

            ShowButtons();
            Space();
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            ShowDictionaries();
            EditorGUILayout.EndScrollView();
            SetAllDirty();
        }

        private void SetAllDirty()
        {
            foreach (var questObject in GameModuleObjects<Quest>())
            {
                EditorUtility.SetDirty(questObject);
            }
        }

        private void CheckForNullDrawer()
        {
            if (dictionariesDrawer == null)
                SetDictionaryDrawer();
        }
        
        private int _menuBarIndex;
        private void ShowButtons() => _menuBarIndex = ToolbarMenuMain(menuBarOptions, _menuBarIndex);

        private void ShowDictionaries()
        {
            if (_menuBarIndex != 0) return;
            LabelSized("Dictionaries");

            Label("Create the structure for all Quest dictionaries here. Select individual Quest to set " +
                  "content values.", false, true);
            
            CachedQuestType(questType)[0].dictionaries.DrawStructure(dictionariesDrawer, "Quest", questType);
        }

        private void ShowHeader()
        {
            MessageBox("Manage the structure of all Quest Objects here. Select each Scriptable Object to set " +
                       "values for each object.", MessageType.Info);
            LabelSized("Quests - " + CachedQuestObjects().Where(x => x.objectType == questType).Count() + " " + questType + " objects");
            StartRow();
            typeIndex = Popup(typeIndex, GameModuleObjectTypes<Quest>(), 200);
            questType = GameModuleObjectTypes<Quest>()[typeIndex];
            Label("Select a Quest Type", 200);
            EndRow();
        }

        private bool NoTypes()
        {
            if (!GameModuleObjectTypes<Quest>().Any())
            {
                LabelSized("Quests");
                MessageBox("You have not created any Quests yet. You can do this by navigating to where " +
                           "you would like the Scriptable Object to live, then right-click, and select " +
                           "Create/Game Modules/Create/Quest/Quest.\n\nNote: The name of the parent directory will be " +
                           "set as the \"Object Type\" automatically.", MessageType.Info);
                return true;
            }

            return false;
        }
    }
}
