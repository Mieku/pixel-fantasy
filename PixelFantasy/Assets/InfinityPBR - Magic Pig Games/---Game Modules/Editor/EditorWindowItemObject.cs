using System.Linq;
using UnityEditor;
using UnityEngine;
using static InfinityPBR.InfinityEditor;
using static InfinityPBR.Modules.GameModuleUtilities;

namespace InfinityPBR.Modules
{
    public class EditorWindowItemObject : EditorWindow
    {
        private int typeIndex;
        private string itemObjectType;
        private int fieldWidth = 200;
        private EditorWindow thisEditorWindow;
        
        public DictionariesDrawer dictionariesDrawer;
        private StatModificationLevelDrawer statModificationLevelDrawer;
        public ItemAttributeDrawer itemAttributeDrawer;
        private Vector2 scrollPosition;
        
        private ItemObject[] cachedItemObjectTypes;
        private string itemObjectTypeName;
        private string[] cachedItemObjecTypeNames;
        
        private bool PropertyCodeUpdateNeeded =>
            Utilities.GetItemObjectArray().Length > 0 && GetString("Item Object Hash") != Utilities.ItemObjectHash();

        private string[] menuBarOptions = {
            "Allowed Item Attributes",
            "Inventory",
            "Dictionaries"
        };
        
        [MenuItem("Window/Game Modules/Item Objects")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            EditorWindowItemObject window = (EditorWindowItemObject)GetWindow(typeof(EditorWindowItemObject));
            window.Show();
        }

        private void Awake()
        {
            thisEditorWindow = this;
            titleContent = new GUIContent("Item Objects");
            SetItemAttributeDrawer();
            SetDictionaryDrawer();
            ResetCache();
            //Cache();
        }

        public void Cache()
        {
            GameModuleObjects<ItemObject>(true);
            cachedItemObjecTypeNames = GameModuleObjectTypes<ItemObject>(true).ToArray();
        }

        public ItemObject[] CachedItemObjectsType(string newType)
        {
            if (cachedItemObjectTypes == null)
                cachedItemObjectTypes = GameModuleObjectsOfType<ItemObject>(newType);
            if (newType != itemObjectTypeName)
            {
                cachedItemObjectTypes = GameModuleObjectsOfType<ItemObject>(newType);
                itemObjectTypeName = newType;
            }

            return cachedItemObjectTypes;
        }
        
        public string[] CachedItemObjectTypeNames()
        {
            if (cachedItemObjecTypeNames == null)
                cachedItemObjecTypeNames = GameModuleObjectTypes<ItemObject>().ToArray();

            return cachedItemObjecTypeNames;
        }

        private void OnEnable()
        {
            Undo.undoRedoPerformed += UndoWasDone;
        }

        private void AutoSelect()
        {
            if (!HasKey("ItemObject Type Selected")) return;
            
            var selectedType = GetString("ItemObject Type Selected");
            
            var objTypes = GameModuleObjectTypes<ItemObject>();
            
            for (var i = 0; i < objTypes.Length; i++)
            {
                if (objTypes[i] != selectedType) continue;

                typeIndex = i;
                break;
            }
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= UndoWasDone;
        }

        private void UndoWasDone()
        {
            itemAttributeDrawer?.ResetCache();
        }

        private void OnFocus()
        {
            AutoSelect();
            itemAttributeDrawer?.ResetCache();
            ResetCache();
            Cache();
        }

        private void ResetCache()
        {
            if (string.IsNullOrWhiteSpace(itemObjectType)) return;
            Debug.Log($"Resetting cache for {CachedItemObjectTypeNames()[typeIndex]}");
            if (dictionariesDrawer == null)
                return;
            dictionariesDrawer.ResetCache("Item Object", CachedItemObjectTypeNames()[typeIndex]);

        }

        private void SetItemAttributeDrawer()
        {
            itemAttributeDrawer = new ItemAttributeDrawerEditor(fieldWidth);
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

            ShowButtons();
            Space();
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            ShowItemAttributes();
            Space();
            
            ShowInventory();
            Space();
            ShowDictionaries();
            
            EditorGUILayout.EndScrollView();
            SetAllDirty();
        }

        private void SetAllDirty()
        {
            foreach (var itemObject in GameModuleObjects<ItemObject>())
            {
                EditorUtility.SetDirty(itemObject);
            }
        }

        private void CheckForNullDrawer()
        {
            if (dictionariesDrawer == null)
                SetDictionaryDrawer();
            if (itemAttributeDrawer == null)
                SetItemAttributeDrawer();
        }
        
        private void ShowPropertyCode()
        {
            if (!GetBool("Item Objects Window Display Property Code"))
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
        
        private int _menuBarIndex;
        private void ShowButtons() => _menuBarIndex = ToolbarMenuMain(menuBarOptions, _menuBarIndex);

        private bool ButtonCode(string label, bool current)
        {
            BackgroundColor(current ? Color.green : Color.black);
            if (Button(label))
                current = !current;
            return current;
        }

        private void ShowItemAttributes()
        {
            if (_menuBarIndex != 0) return;
            Header1("Allowed Item Attributes");

            Label("Manage Item Attribute availability for all Item Objects here. Select individual" +
                  "Item Objects to add options unique Item Attribute settings. Individual Item Attributes will only show" +
                  $"if all Item Objects of type {itemObjectType} can use the Item Attribute type.", false, true);

            itemAttributeDrawer.DrawStructure(itemAttributeDrawer, itemObjectType);
        }
        
        private void ShowDictionaries()
        {
            if (_menuBarIndex != 2) return;
            LabelSized("Dictionaries");

            Label("Create the structure for all Item Object dictionaries here. Select individual Item Objects to set " +
                  "content values.", false, true);

            CachedItemObjectsType(itemObjectType)[0].dictionaries.DrawStructure(dictionariesDrawer, "Item Object", itemObjectType);
        }

        private void ShowInventory()
        {
            if (_menuBarIndex != 1) return;
            LabelSized("Inventory");

            StartRow();
            Label("", 150);
            Label("Height", 70, true);
            Label("Width", 70, true);
            Label("Inventory Prefab", 150, true);
            Label("World Prefab", 150, true);
            EndRow();
            
            foreach (ItemObject itemObject in CachedItemObjectsType(itemObjectType))
            {
                StartRow();
                Label(itemObject.objectName, 150);
                itemObject.inventoryHeight = Int(itemObject.inventoryHeight, 70);
                itemObject.inventoryWidth = Int(itemObject.inventoryWidth, 70);
                itemObject.prefabInventory = Object(itemObject.prefabInventory, typeof(GameObject), 150) as GameObject;
                itemObject.prefabWorld = Object(itemObject.prefabWorld, typeof(GameObject), 150) as GameObject;
                EndRow();
            }
        }
        
        private void ShowStats()
        {
            if (_menuBarIndex != 1) return;
            LabelSized("Stats");

            foreach (ItemObject itemObject in CachedItemObjectsType(itemObjectType))
            {
                if (ShowStatValues(itemObject)) continue;
                StartRow();
                Label(itemObject.objectName, 100, true);
                LabelGrey($"{itemObject.objectName} does not have any Stat effects");
                EndRow();
            }
        }
        
        private bool ShowStatValues(ItemObject itemObject)
        {
            if (itemObject.modificationLevel.targets.Count == 0) return false;
            
            Label(itemObject.objectName, true);
            statModificationLevelDrawer.DrawSimple(itemObject.modificationLevel, null);
            Space();
            return true;
        }

        /*
        private bool ShowStatValues(ItemObject itemObject)
        {
            if (itemObject.modificationLevel.targets.Count == 0) return false;
            
            StartVerticalBox();
            StartRow();
            StartVertical();
            Label(itemObject.objectName, 100, true);
            Label("", 100);
            foreach (Stat target in itemObject.modificationLevel.targets)
            {
                Label(target.name, 100);
            }
            EndVertical();

            ShowValueAndProficiency(itemObject, null, true, false);
            foreach (Stat source in itemObject.modificationLevel.sources)
            {
                ShowValueAndProficiency(itemObject, source, false, false);
            }

            EndRow();
            EndVerticalBox();
            return true;
        }
        */

        private void ShowValueAndProficiency(ItemObject itemObject, Stat source, bool isBase, bool isPerSkillPoint)
        {
            StartVertical();
            Label(isBase ? isPerSkillPoint ? "Per Skill Point" : "Base" : source.objectName, 100, true);
            StartRow();
            LabelSized("Value", 50, 8);
            LabelSized("Proficiency", 50, 8);
            EndRow();
            
            foreach (Stat target in itemObject.modificationLevel.targets)
            {
                StartRow();
                StatModification mod = itemObject.modificationLevel.GetModification(null, target, true);
                itemObject.modificationLevel.SetValue(null, target,  Float(mod.value, 50), true);
                itemObject.modificationLevel.SetProficiency(null, target,  Float(mod.proficiency, 50), true);
                EndRow();
            }

            
            EndVerticalBox();
        }

        private void ShowHeader()
        {
            MessageBox("Manage the structure of all Item Objects here. Select each Scriptable Object to set " +
                       "values for each object.", MessageType.Info);
            LabelSized("Item Objects - " + GameModuleObjects<ItemObject>().Where(x => x.objectType == itemObjectType).Count() + " " + itemObjectType + " objects");
            StartRow();
            typeIndex = Popup(typeIndex, CachedItemObjectTypeNames(), 200);
            itemObjectType = CachedItemObjectTypeNames()[typeIndex];
            Label("Select a Item Object Type", 200);
            EndRow();
         
            
                /*
                 * List the structure options here. Update the DRAW functions to hide the structural options or show based on a bool.
                 * If showing structure, should updating the content here update for all item objects?  Probably.s
                 */
        }

        private bool NoTypes()
        {
            if (!CachedItemObjectTypeNames().Any())
            {
                LabelSized("Item Objects");
                MessageBox("You have not created any Item Objects yet. You can do this by navigating to where " +
                           "you would like the Scriptable Object to live, then right-click, and select " +
                           "Create/Game Modules/Create/Item Object.\n\nNote: The name of the parent directory will be " +
                           "set as the \"Object Type\" automatically.", MessageType.Info);
                return true;
            }

            return false;
        }
    }
}
