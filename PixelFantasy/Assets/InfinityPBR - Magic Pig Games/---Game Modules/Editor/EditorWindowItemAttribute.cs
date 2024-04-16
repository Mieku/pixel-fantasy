using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static InfinityPBR.Modules.GameModuleUtilities;
using static InfinityPBR.InfinityEditor;
using static InfinityPBR.Modules.InfinityEditorGameModules;

namespace InfinityPBR.Modules
{
    public class EditorWindowItemAttribute : EditorWindow
    {
        private int typeIndex;
        private string itemAttributeType;
        private int fieldWidth = 200;
        private EditorWindow thisEditorWindow;
        
        public DictionariesDrawer dictionariesDrawer;
        private StatModificationLevelDrawer statModificationLevelDrawer;
        private Vector2 scrollPosition;
        
        private ItemAttribute[] cachedItemAttributeArrayOfType;
        private string cachedtype;

        private static List<ItemAttribute> selectedAttributes;

        private string[] menuBarOptions = {
            //$"Stats",
            "Dictionaries"
        };
        
        [MenuItem("Window/Game Modules/Item Attributes")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            var window = (EditorWindowItemAttribute)GetWindow(typeof(EditorWindowItemAttribute));
            window.Show();
        }
        
        private void OnSelectionChanged()
        {
            if (!CanDoModulesWindowSelection) return;
            
            if (Selection.assetGUIDs.Length != 1)
                return;

            TryToSelectInspector();
        }

        public void Cache()
        {
            GameModuleObjects<ItemAttribute>(true);
            GameModuleObjectTypes<ItemAttribute>(true);
        }
        
        private void Awake()
        {
            thisEditorWindow = this;
            SetDictionaryDrawer();
            titleContent = new GUIContent("Item Attributes");
            ResetCache();
        }

        private void OnEnable()
        {
            Undo.undoRedoPerformed += UndoWasDone;
            //Selection.selectionChanged += OnSelectionChanged;
        }
        
        private void OnDisable()
        {
            Undo.undoRedoPerformed -= UndoWasDone;
            //Selection.selectionChanged -= OnSelectionChanged;
        }
        
        

        private void AutoSelect()
        {
            if (!HasKey("ItemAttribute Type Selected")) return;

            var selectedType = GetString("ItemAttribute Type Selected");
            //Debug.Log($"selected typ is {selectedType}");
            //var itemAttribute = GameModuleObjects<ItemAttribute>()
            //    .FirstOrDefault(x => x.objectName == GetString("ItemAttribute Type Selected"));

            //if (itemAttribute == null) return;

            var attributeTypes = GameModuleObjectTypes<ItemAttribute>();
            
            for (var i = 0; i < attributeTypes.Length; i++)
            {
                if (attributeTypes[i] != selectedType) continue;

                typeIndex = i;
                break;
            }

            //DeleteKey("ItemAttribute Type Selected");
        }

        

        private void UndoWasDone()
        {
            //_itemAttributeDrawer.ResetCache();
        }

        private void OnFocus()
        {
            AutoSelect();
            ResetCache();
            Cache();
        }

        private void ResetCache()
        {
            if (string.IsNullOrWhiteSpace(itemAttributeType)) return;
            if (dictionariesDrawer == null) return;
            Debug.Log($"Resetting cache for {GameModuleObjectTypes<ItemAttribute>()[typeIndex]}");
            dictionariesDrawer?.ResetCache("Item Attribute", GameModuleObjectTypes<ItemAttribute>()[typeIndex]);
        }

        private void SetItemAttributeDrawer()
        {
            //_itemAttributeDrawer = new ItemAttributeDrawerEditor(fieldWidth);
        }
        
        private void SetDictionaryDrawer()
        {
            dictionariesDrawer = new DictionariesDrawerEditor(fieldWidth);
            statModificationLevelDrawer = new StatModificationLevelDrawerEditor(fieldWidth);
        }

        void OnGUI()
        {
            if (NoTypes()) return;

            CheckForNullDrawer();
            
            ShowHeader();
            Space();

            if (selectedAttributes != null)
            {
                LabelSized($"Ther eare {selectedAttributes.Count} selected ");
            }

            ShowButtons();
            Space();
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            //ShowStats();
            //Space();
            ShowDictionaries();
            //Space();
            //ShowPropertyCode();
            EditorGUILayout.EndScrollView();

            SetAllDirty();
        }
        
        private void SetAllDirty()
        {
            foreach (var itemObject in GameModuleObjects<ItemAttribute>(true))
            {
                EditorUtility.SetDirty(itemObject);
            }
        }

        private void CheckForNullDrawer()
        {
            if (dictionariesDrawer == null)
                SetDictionaryDrawer();
        }

        private int _menuBarIndex;
        private void ShowButtons() => _menuBarIndex = ToolbarMenuMain(menuBarOptions, _menuBarIndex);

        private bool ButtonCode(string label, bool current)
        {
            BackgroundColor(current ? Color.green : Color.black);
            if (Button(label))
                current = !current;
            return current;
        }
        
        private void ShowPropertyCode()
        {
            if (!GetBool("Item Attributes Window Display Property Code"))
                return;
            LabelSized("Property Code");

            Label("The Property code script can be exported from the Property Code window, " +
                  "which is also in Window/Game Modules/Property Code.", false, true);

            if (Button("Open Property Code window"))
            {
                EditorWindowPropertyCode window = (EditorWindowPropertyCode)GetWindow(typeof(EditorWindowPropertyCode));
                window.Show();
            }
        }

        private void ShowDictionaries()
        {
            if (_menuBarIndex != 0) return;
            LabelSized("Dictionaries");

            Label("Create the structure for all Item Attributes dictionaries here. Select individual Item Attributes to set " +
                  "content values.", false, true);
            
            GameModuleObjectsOfType<ItemAttribute>(itemAttributeType)[0].dictionaries.DrawStructure(dictionariesDrawer, "Item Attribute", itemAttributeType);
        }

        private void ShowStats()
        {
            if (_menuBarIndex != 0) return;
            LabelSized("Stats");

            foreach (var itemAttribute in GameModuleObjectsOfType<ItemAttribute>(itemAttributeType))
            {
                if (ShowStatValues(itemAttribute)) continue;
                StartRow();
                Label(itemAttribute.objectName, 100, true);
                LabelGrey($"{itemAttribute.objectName} does not have any Stat effects");
                EndRow();
            }
        }

        private bool ShowStatValues(ItemAttribute itemAttribute)
        {
            if (itemAttribute.modificationLevel.targets.Count == 0) return false;
            
            Label(itemAttribute.objectName, true);
            statModificationLevelDrawer.DrawSimple(itemAttribute.modificationLevel, null);
            Space();
            return true;
        }

        private void ShowValueAndProficiency(ItemAttribute itemAttribute, Stat source, bool isBase, bool isPerSkillPoint)
        {
            StartVertical();
            Label(isBase ? isPerSkillPoint ? "Per Skill Point" : "Base" : source.objectName, 100, true);
            StartRow();
            LabelSized("Value", 50, 8);
            LabelSized("Proficiency", 50, 8);
            EndRow();
            
            foreach (Stat target in itemAttribute.modificationLevel.targets)
            {
                StartRow();
                StatModification mod = itemAttribute.modificationLevel.GetModification(null, target, true);
                itemAttribute.modificationLevel.SetValue(null, target,  Float(mod.value, 50), true);
                itemAttribute.modificationLevel.SetProficiency(null, target,  Float(mod.proficiency, 50), true);
                EndRow();
            }

            
            EndVerticalBox();
        }

        private void ShowHeader()
        {
            MessageBox("Manage the structure of all Item Attributes here. Select each Scriptable Object to set " +
                       "values for each attribute object.", MessageType.Info);
            LabelSized("Item Attributes - " + GameModuleObjects<ItemAttribute>().Where(x => x.objectType == itemAttributeType).Count() + " " + itemAttributeType + " attributes");
            StartRow();
            typeIndex = Popup(typeIndex, GameModuleObjectTypes<ItemAttribute>(), 200);
            itemAttributeType = GameModuleObjectTypes<ItemAttribute>()[typeIndex];
            Label("Select a Item Attribute Type", 200);
            EndRow();
         
            
                /*
                 * List the structure options here. Update the DRAW functions to hide the structural options or show based on a bool.
                 * If showing structure, should updating the content here update for all item objects?  Probably.s
                 */
        }

        private bool NoTypes()
        {
            if (!GameModuleObjectTypes<ItemAttribute>().Any())
            {
                LabelSized("Item Attributes");
                MessageBox("You have not created any Item Attributes yet. You can do this by navigating to where " +
                           "you would like the Scriptable Object to live, then right-click, and select " +
                           "Create/Game Modules/Create/Item Attribute.\n\nNote: The name of the parent directory will be " +
                           "set as the \"Object Type\" automatically.", MessageType.Info);
                return true;
            }

            return false;
        }
    }
}
