using System;
using UnityEditor;

/*
 * This script will display an easy way to populate the data in the Loot Box Items object. The goal is to populate a
 * group of Item objects, Prefix and Suffix objects that are "potential" additions to a Loot Box, when the loot box is
 * populated. In general, I think this should be used for groups of similar Item objects, where you want a specific
 * style or level or quality of item to be provided to the player, but want some randomization among that group.
 *
 * I'm sure this can be used in more novel ways, however!
 */

namespace InfinityPBR.Modules.Loot
{
    [CustomEditor(typeof(LootItems))]
    [CanEditMultipleObjects]
    [Serializable]
    public class LootItemsEditor : Editor
    {
        private LootItems _modulesObject;
        private LootItemsDrawer _modulesDrawer;
        
        private void OnEnable()
        {
            _modulesObject = (LootItems)target;
            _modulesDrawer = CreateInstance<LootItemsDrawer>();
            _modulesDrawer.SetModulesObject(_modulesObject, false); // Pass the Modules object to Modules Drawer
        }
        
        public override void OnInspectorGUI() => _modulesDrawer.Draw();
        
        /*
        private LootItems _modulesObject;
        
        protected override void Header()
        {
            BeginChangeCheck();
            Validations();
            ShowHeader(_modulesObject, _modulesObject.Uid());
            EndChangeCheck();
        }

        private void Validations()
        {
            _modulesObject.objectName = _modulesObject.name;
        }

        // This is "Awake"
        protected override void Setup()
        {
            _modulesObject = (LootItems) target;
            if (!_modulesObject) return;
            CacheItemTypeArray();
        }

        /// <summary>
        /// This is where the inspector is drawn.
        /// </summary>
        protected override void Draw()
        {
            if (!InitialSetup()) return;
            
            LabelGrey("Loot Items");
            LabelBig(_modulesObject.objectName, 18, true);

            BeginChangeCheck();

            Space();
            ShowDescription();

            Space();
            ShowAddBox();

            Space();
            Undo.RecordObject(_modulesObject, "Undo Item Attributes Changes");
            ShowList();

            Footer();

            EndChangeCheck();
            EditorUtility.SetDirty(_modulesObject);
        }

        private void ShowDescription()
        {
            Label($"Optional internal description {symbolInfo}", "Use this space to remind yourself how you plan " +
                                                                 "on using this Loot Items. It will be visible when you " +
                                                                 "are setting up Loot Boxes.", 300);
            _modulesObject.description = TextArea(_modulesObject.description, 300, 50);
        }

        private void ShowAddBox()
        {
            BackgroundColor(Color.yellow);
            StartVerticalBox();
            StartRow();

            Label("Add Item", 70, true);
            var tempTypeIndex = _modulesObject.typeIndex;
            _modulesObject.typeIndex = Popup(_modulesObject.typeIndex, _modulesObject.availableTypes, 150);
            if (tempTypeIndex != _modulesObject.typeIndex) 
                CacheItemObjectArray();
            
            if (_modulesObject.availableItemNames.Length > 0)
            {
                _modulesObject.itemIndex = Popup(_modulesObject.itemIndex, _modulesObject.availableItemNames, 150);
                if (Button("Add", 70))
                {
                    _modulesObject.itemObjects.Add(GetObject<ItemObject>(_modulesObject.availableItems[_modulesObject.itemIndex]));
                    CacheItemObjectArray();
                }
                if (Button("Add All", 70))
                {
                    foreach(string uid in _modulesObject.availableItems)
                        _modulesObject.itemObjects.Add(GetObject<ItemObject>(uid));
                    CacheItemObjectArray();
                }
            }
            else
            {
                LabelGrey("There are no available Item Objects of this type.");
            }

            EndRow();
            EndVerticalBox();
            BackgroundColor(Color.white);
        }

        private void CacheItemObjectArray()
        {
            _modulesObject.availableItems = GameModuleObjectsOfType<ItemObject>(_modulesObject.availableTypes[_modulesObject.typeIndex])
                .Except(_modulesObject.itemObjects)
                .Select(x => x.Uid())
                .ToArray();
            
            _modulesObject.availableItemNames = GameModuleObjectsOfType<ItemObject>(_modulesObject.availableTypes[_modulesObject.typeIndex])
                .Except(_modulesObject.itemObjects)
                .Select(x => x.objectName)
                .ToArray();
        }

        private void CacheItemTypeArray()
        {
            _modulesObject.availableTypes = GameModuleObjectTypes<ItemObject>();
            if (_modulesObject.typeIndex >= _modulesObject.availableTypes.Length)
                _modulesObject.typeIndex = _modulesObject.availableTypes.Length - 1;
        }

        private void ShowList()
        {
            foreach (ItemObject itemObject in _modulesObject.itemObjects)
            {
                ShowItemObject(itemObject);
            }
        }

        private void ShowItemObject(ItemObject itemObject)
        {
            StartRow();

            BackgroundColor(Color.red);
            if (Button(symbolX, 25))
            {
                _modulesObject.itemObjects.RemoveAll(x => x == itemObject);
                CacheItemObjectArray();
                ExitGUI();
            }
            ResetColor();
            Label($"{itemObject.objectName}", 150);
            Object(itemObject, typeof(ItemObject), 150, false);
            BackgroundColor(Color.white);
            
            EndRow();
        }

        private bool InitialSetup()
        {
            if (_modulesObject.hasBeenSetup) return true;
            
            CacheItemTypeArray();
            CacheItemObjectArray();
            CheckForMissingItems();
            _modulesObject.hasBeenSetup = true;
            return true;
        }
        
        private void Footer()
        {
            ShowFooter(Script);
            if (GetBool("Show full inspector " + _modulesObject.objectName))
                DrawDefaultInspector();
        }

        private void CheckForMissingItems()
        {
            _modulesObject.itemObjects.RemoveAll(x => x == null);
        }*/
    }
}