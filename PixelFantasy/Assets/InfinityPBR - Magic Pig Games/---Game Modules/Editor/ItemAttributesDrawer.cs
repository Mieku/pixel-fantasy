using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static InfinityPBR.Modules.GameModuleUtilities;
using static InfinityPBR.Modules.InfinityEditorGameModules;

namespace InfinityPBR.Modules
{
    public class ItemAttributesDrawer : GameModulesDrawer
    {
        // -------------------------------
        // PROPERTIES AND VARIABLES -- UPDATE THESE FOR THIS TYPE
        // -------------------------------
        private ItemAttribute _modulesObject;
        const string ThisTypePlural = "Item Attributes";
        const string ThisType = "Item Attribute";
        private string ClassNamePlural => "ItemAttributes";
        private string ClassName => "ItemAttribute";
        private string DocsURL => "https://infinitypbr.gitbook.io/infinity-pbr/game-modules/items";
        private string DocsURLLabel => "Item Attributes";
        private string[] GameModuleObjectTypes(bool recompute = false) => GameModuleObjectTypes<ItemAttribute>(recompute);
        private ItemAttribute[] GameModuleObjects(bool recompute = false) => GameModuleObjects<ItemAttribute>(recompute);
        private ItemAttribute[] GameModuleObjectsOfType(string type, bool recompute = false) => GameModuleObjectsOfType<ItemAttribute>(type, recompute);
        // -------------------------------
        
        // -------------------------------
        // REQUIRED - NO UPDATE NEEDED
        // -------------------------------
        private Vector2 _scrollPosition;
        private int _fieldWidth;
        private DictionariesDrawer _dictionariesDrawer;
        private StatModificationLevelDrawer _statModificationLevelDrawer;
        private ItemAttributeDrawer _itemAttributeDrawer;
        private string[] _menuBarOptions;
        public bool drawnByGameModulesWindow = true;
        
        //private int _lootItemsIndex = 0;
        private List<LootItems> _cachedLootItems = new List<LootItems>();
        private List<string> _cachedLootItemsNames = new List<string>();
        private List<LootItems> _activeLootItems = new List<LootItems>();
        // -------------------------------

        // -------------------------------
        // METHODS -- UPDATE THESE FOR THIS TYPE
        // -------------------------------
        public void SetModulesObject(ItemAttribute modulesObject, bool drawnByWindow = true)
        {
            _modulesObject = modulesObject;
            _menuBarOptions = new[] { 
                "Settings", 
                "Stats Effects",
                "Dictionaries" 
            };
            _fieldWidth = 200;
            _dictionariesDrawer = new DictionariesDrawerEditor(_fieldWidth);
            _statModificationLevelDrawer = new StatModificationLevelDrawerEditor(_fieldWidth);
            drawnByGameModulesWindow = drawnByWindow;
        }
        
        protected void Cache()
        {
            GameModuleObjects<MasteryLevels>(true);
            GameModuleObjectNames<MasteryLevels>(true);
            GameModuleObjectTypes(true);
            GameModuleObjects(true);
            GameModuleObjectsOfType(_modulesObject.objectType, true);
            
            SetBool("statModificationLevelDrawer Force Cache", true);
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
            foreach (var itemObject in GameModuleObjects())
            {
                var tempName = itemObject.objectName;
                var tempType = itemObject.objectType;
                CheckName(_modulesObject);
                itemObject.CheckObjectType();
                var newCount = GameModuleObjects().Count(x => x.objectName == itemObject.objectName);
                if (newCount > 1)
                    Debug.LogError("There are " + newCount + " Item Objects named " + itemObject.objectName);
                if (tempName != itemObject.objectName || tempType != itemObject.objectType)
                {
                    UpdateProperties();
                    EditorUtility.SetDirty(itemObject);
                }
            }
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
            
            //SetEditorWindowWidth(); July 2 2023 -- I don't think this is needed anymore
            
            DrawLinkToDocs();

           
            BeginChangeCheck();
            // If this is a duplicate uid and the button is clicked to reset it, redo Initial Setup
            if (!drawnByGameModulesWindow && CheckUid(_modulesObject.Uid(), _modulesObject, _duplicateUid))
                _initialSetup = false;
            
            if (!drawnByGameModulesWindow)
            {
                LabelGrey("Item Attribute - " + _modulesObject.ObjectType);
                LabelBig(_modulesObject.ObjectName, 18, true);
            }


            ShowButtons();
            
            Undo.RecordObject(_modulesObject, "Undo Stats Changes");
            ShowSettings();
            
            Undo.RecordObject(_modulesObject, "Undo Stats Changes");
            ShowStats();

            Undo.RecordObject(_modulesObject, "Undo Dictionaries Changes");
            ShowDictionaries();
            
            CheckConflicts();
            
            EndChangeCheck(_modulesObject);
        }
        
        private void CheckConflicts()
        {
            if (!_modulesObject) return;
            if (_modulesObject.dictionaries?.keyValues == null) return;
            foreach (var keyValue in _modulesObject.dictionaries.keyValues)
            {
                if (!keyValue.valuesItemAttribute.Contains(_modulesObject)) continue;
                LogWarning("Item Attribute " + _modulesObject.objectName + " can't reference itself.");
                for (var index = 0; index < keyValue.valuesItemAttribute.Count; index++)
                {
                    if (keyValue.valuesItemAttribute[index] != _modulesObject)
                        continue;
                    
                    keyValue.valuesItemAttribute[index] = null;
                }
            }
        }
        
        private string KeyGeneral => $"Item Attribute General Settings {_modulesObject.objectName}";
        private string KeyNameSettings => $"Item Attribute Name Settings {_modulesObject.objectName}";
        private string KeyRequisiteSettings => $"Item Attribute Requisite Settings {_modulesObject.objectName}";
        
        private void ShowSettings()
        {
            if (_modulesObject.menubarIndex != 0) return;
            Space();
            
            // General Settings
            SetBool(KeyGeneral, StartVerticalBoxSection(GetBool(KeyGeneral), "General Settings"));
            ShowGeneral();
            EndVerticalBox();
            
            // Name Settings
            Space();
            SetBool(KeyNameSettings, StartVerticalBoxSection(GetBool(KeyNameSettings), "Name Settings"));
            ShowNameOptions();
            EndVerticalBox();
            
            // Requisite Settings
            Space();
            SetBool(KeyRequisiteSettings, StartVerticalBoxSection(GetBool(KeyRequisiteSettings), "Requisite Attributes"));
            ShowCopyPasteRequisiteAttributes();
            ShowRequisiteOptions();
            ShowRequisiteMessage();
            EndVerticalBox();
        }
        
        private void ShowRequisiteMessage()
        {
            if (!GetBool(KeyRequisiteSettings))
                return;
            
            Space();
            MessageBox("Required attributes do not cascade, i.e. " +
                       "if B requires C, and A requires B, A does not require C unless it is " +
                       "specified here.\n\n" +
                       "When a required attribute is added, if \"Remove others of type\" is true, other " +
                       "attributes of the same type will be removed. Attributes required by those will " +
                       "not be removed.\n\n" +
                       "An \"Add()\" or \"ReceiveTransfer()\" call will return null if any GameItemAttribute " +
                       "in the GameItemAttributeList is incompatible with the new addition, even if the " +
                       "new addition is required by another ItemAttribute in the list.", MessageType.Info);
        }
        
        private void ShowRequisiteOptions()
        {
            if (!GetBool(KeyRequisiteSettings))
                return;
            
            Space();
            StartRow();
            Label($"Required Attributes {symbolInfo}", "When this ItemAttribute is added to a GameItemAttributeList, " +
                                                       "the other ItemAttributes added here, will also be added if they " +
                                                       "are not already present. Example, an attribute \"Diamond\" may require " +
                                                       "the attribute \"Rare\".", 200, true);

            ShowAddRequiredBox();
            EndRow();
            ShowRequiredAttributes();

            Space();
            StartRow();
            Label($"Incompatible Attributes {symbolInfo}", "When this ItemAttribute is added to a GameItemAttributeList, " +
                                                           "if one of the ItemAttributes here is already present, then this will " +
                                                           "not be added. Example, \"Icy\" and \"of Fire\" could be incompatible in " +
                                                           "your project.", 200 , true);

            ShowAddIncompatibleBox();
            EndRow();
            ShowIncompatibleAttributes();
        }
        
        private void ShowIncompatibleAttributes()
        {
            foreach (var incompatibleAttribute in _modulesObject.incompatibleAttributes)
            {
                ShowIncompatibleAttribute(incompatibleAttribute);
            }

            Space();
        }

        private void ShowIncompatibleAttribute(ItemAttribute attribute)
        {
            StartRow();
            if (XButton())
            {
                _modulesObject.incompatibleAttributes.Remove(attribute);
                ExitGUI();
            }
            
            Object(attribute, typeof(ItemAttribute), 200);
            if (attribute.distinct)
                ContentColor(Color.cyan);
            Label($"{attribute.objectType}{(attribute.distinct ? $": Has no effect on existing \"{attribute.objectType}\" attributes." : "")}", false, true);
            ResetColor();
            
            EndRow();
        }
        
        private void ShowAddIncompatibleBox()
        {
            var newObject = ObjectSelectBox<ItemAttribute>("Add: ", 50, 150, 220);
            if (newObject == null) return;
            if (newObject == _modulesObject) return;

            if (_modulesObject.requiredAttributes.Any(x => x.itemAttribute == newObject))
            {
                Debug.Log($"{newObject.objectName} is a Required attribute. Please remove it before " +
                          "adding it as incompatible.");
                return;
            }

            _modulesObject.incompatibleAttributes.AddDistinct(newObject);
        }
        
        private void ShowRequiredAttributes()
        {
            foreach (var requiredAttribute in _modulesObject.requiredAttributes)
            {
                ShowRequiredAttribute(requiredAttribute);
            }

            Space();
        }
        
        private void ShowRequiredAttribute(RequiredItemAttribute requiredAttribute)
        {
            var attribute = requiredAttribute.itemAttribute;
                
            StartRow();
            if (XButton())
            {
                _modulesObject.requiredAttributes.Remove(requiredAttribute);
                ExitGUI();
            }

            Object(attribute, typeof(ItemAttribute), 200);
            if (attribute.distinct)
                ContentColor(Color.cyan);
            var overwriteMessage = attribute.replaceOthers
                ? $": Will replace other \"{attribute.objectType}\" attributes when \"{_modulesObject.objectName}\" is added."
                : $": Will not be added if other \"{attribute.objectType}\" attributes are present.";
            Label($"{attribute.objectType}{(attribute.distinct ? overwriteMessage: "")}", false, true);
            ResetColor();
            
            EndRow();
        }

        private void ShowAddRequiredBox()
        {
            var newObject = ObjectSelectBox<ItemAttribute>("Add: ", 50, 150, 220);
            if (newObject == null) return;
            if (newObject == _modulesObject) return;
            if (_modulesObject.incompatibleAttributes.Any(x => x == newObject))
            {
                Debug.Log($"{newObject.objectName} is already in the Incompatible list. Please remove " +
                          "it before adding it here.");
                return;
            }
            
            // Can be only one! (maybe)
            var newType = newObject.objectType;
            if (_modulesObject.requiredAttributes.Any(x => x.itemAttribute.objectType == newType && x.onePerType))
            {
                Debug.Log($"Could not add ItemAttribute {newObject.objectName} because there is already a ItemAttribute " +
                          $"of type {newObject.objectType} in the list, with the \"Remove others of type\" toggle true.");
                return;
            }

            var newRequiredAttribute = new RequiredItemAttribute
            {
                itemAttribute = newObject
            };

            if (_modulesObject.objectType ==  newType)
                newRequiredAttribute.onePerType = false;
            
            if (_modulesObject.requiredAttributes.Any(x => x.itemAttribute.objectType == newType))
                newRequiredAttribute.onePerType = false;
            
            _modulesObject.requiredAttributes.AddDistinct(newRequiredAttribute);
        }
        
        private string requisiteAttributeKey = "Item Attribute Requisite Attributes";
        public ItemAttribute CopyItemAttribute => GameModuleObjectByName<ItemAttribute>(GetString(requisiteAttributeKey));
        
        private void ShowCopyPasteRequisiteAttributes()
        {
            if (!GetBool(KeyRequisiteSettings))
                return;
            
            Space();
            BackgroundColor(Color.cyan);
            StartVerticalBox();
            Label($"Copy/Paste \"{GetString(requisiteAttributeKey)}\" {symbolInfo}", "Copy and paste the requisite ItemAttributes from " +
                "another object.");
            
            StartRow();
            if (Button("Copy", 50))
                SetString(requisiteAttributeKey, _modulesObject.objectName);

            Label("", 20);
            
            if (HasKey(requisiteAttributeKey))
            {
                if (Button("Paste", 60))
                    _modulesObject.CopyRequisiteAttributes(CopyItemAttribute);
            }
            
            EndRow();
            EndVerticalBox();
            BackgroundColor(Color.white);
        }
        
        private void ShowNameOptions()
        {
            if (!GetBool(KeyNameSettings))
                return;

            if (_modulesObject.nameOrder == 0)
                _modulesObject.nameOrder = 1;
            
            StartRow();
            var toolTip = "This is a string which will be added into the full name of any object that has this " +
                          "attribute attached to it. Leave it blank if you do not want it to be included in the full " +
                          "name.\n\nHold shift to apply the \"Use name\" and \"Clear\" buttons to all Item Attributes of " +
                          $"type {_modulesObject.objectType}.";
            Label($"Human Name {symbolInfo}", toolTip, 100);
            _modulesObject.humanName = TextField(_modulesObject.humanName, 200);
            if (Button("Use name", 100))
            {
                _modulesObject.humanName = _modulesObject.objectName;
                if (KeyShift)
                {
                    foreach (var itemAttribute in AttributesOfThisType)
                        itemAttribute.humanName = itemAttribute.objectName;
                }
            }
            if (Button("Clear", 100))
            {
                _modulesObject.humanName = "";
                if (KeyShift)
                {
                    foreach (var itemAttribute in AttributesOfThisType)
                        itemAttribute.humanName = "";
                }
            }
            EndRow();
            
            StartRow();
            toolTip = "This value determines where the Human Name will appear, in relation to the Object Name. The " +
                      "Object Name will always be at position 0, so a negative value here will display before the " +
                      "Object Name, and a positive value will display after.\n\n" +
                      "Each Item Attribute type should have its own value, or unpredictable results may occur. If you " +
                      "are unsure of how many attributes you may have in your game, you may wish to choose values with " +
                      "larger gaps between them, which may be filled later by newly added attributes.";
            Label($"Name Order {symbolInfo}", toolTip, 100);
            _modulesObject.nameOrder = Int(_modulesObject.nameOrder, 50);
            if (Button("Apply to all of this type", 250))
            {
                ApplyNameOrderToAllType();
            }
            EndRow();

            Space();
            // Thanks Pryankster at Immobilehome games!
            MessageBox("For those writing in English, a common ordering of adjectives is:\n\n" +
                       "Quantity / Opinion / Size / Age / Shape / Color / Origin or Material / Qualifier [Noun]\n\n" +
                       "Visit https://www.grammarly.com/blog/adjective-order/ to read more about it!", MessageType.Info);
        }
        
        private void ApplyNameOrderToAllType()
        {
            foreach (var itemAttribute in AttributesOfThisType)
                itemAttribute.nameOrder = _modulesObject.nameOrder;
        }
        
        private void ShowGeneral()
        {
            if (!GetBool(KeyGeneral))
                return;

            ShowDistinctAndReplace();
            ShowAffectOption();
        }
        
        private void ShowAffectOption()
        {
            StartRow();
            Label($"Affects Actor {symbolInfo}", "When true, the stat effects will be applied to the actor, or other " +
                                                 "IHaveStats object which owns the GameItemObjectList which holds the ItemObject" +
                                                 " that contains this ItemAttribute (get it?).\n\n" +
                                                 "When false, the stat effects will be applied to the ItemObject itself prior " +
                                                 "to those effects impacting the Actor.", 150);
            var tempOption = _modulesObject.affectsActor;
            _modulesObject.affectsActor = Check(_modulesObject.affectsActor, 50);
            if (KeyShift)
            {
                Label($"[Holding Shift] Changes will affect all \"{_modulesObject.objectType}\" attributes");
                if (tempOption != _modulesObject.affectsActor)
                {
                    foreach (var itemAttribute in AttributesOfThisType)
                        itemAttribute.affectsActor = _modulesObject.affectsActor;
                }
            }
            else
            {
                Label($"<color=#666666><i>Hold \"shift\" to affect all \"{_modulesObject.objectType}\" attributes</i></color>", false, false, true);
            }
            EndRow();
        }

        private ItemAttribute[] AttributesOfThisType =>
            GameModuleObjectsOfType<ItemAttribute>(_modulesObject.objectType);

        private void ShowDistinctAndReplace()
        {
            var tempDistinct = _modulesObject.distinct;
            var tempReplace = _modulesObject.replaceOthers;
            
            StartRow();
            Label($"Distinct {symbolInfo}", "When true, each GameItemAttributeList will " +
                                            $"only contain one GameItemAttribute of \"{_modulesObject.objectType}\" " +
                                            "type.", 150);
            _modulesObject.distinct = Check(_modulesObject.distinct, 50);
            LabelGrey($"* Affects all \"{_modulesObject.objectType}\" attributes");
            EndRow();
            
            Colors(_modulesObject.distinct ? Color.white : Color.grey);
            StartRow();
            Label($"Replace Others {symbolInfo}", "When Distinct is true, and this is also true, other GameItemAttributes " +
                                                  $"of type \"{_modulesObject.objectType}\" will be replaced when this " +
                                                  "is added to a GameItemAttributeList.", 150);
            if (_modulesObject.distinct)
                _modulesObject.replaceOthers = Check(_modulesObject.replaceOthers, 50);
            else
                Check(_modulesObject.replaceOthers, 50);
            LabelGrey($"* Affects all \"{_modulesObject.objectType}\" attributes");
            EndRow();
            ResetColor();

            if (tempDistinct == _modulesObject.distinct 
                && tempReplace == _modulesObject.replaceOthers) return;
            
            foreach (var itemAttribute in AttributesOfThisType)
            {
                itemAttribute.distinct = _modulesObject.distinct;
                itemAttribute.replaceOthers = _modulesObject.replaceOthers;
            }
        }
        
        private void ShowStats()
        {
            if (_modulesObject.menubarIndex != 1) return;
            Space();
            MessageBox("Item Objects can affect Stats. Often this is used when objects are \"equipped\" on a " +
                       "character. However, you can set this up however you'd like!\n\n" +
                       "IMPORTANT: Do not forget to set up the GetOtherLevels() method for your characters, so the " +
                       "system knows where to find the ItemObjectList that affects Stats! See the scripting docs linked " +
                       "from InfinityPBR.com for more details.");
            
            Space();
            StartVerticalBox();
            ShowStatsHeader();
            
            _statModificationLevelDrawer.Draw(_modulesObject.modificationLevel, null);
            
            EndVerticalBox();
        }
        
        private void ShowStatsHeader() => LabelBig("Stats",200, 18, true);
        
        public void ShowObjectsWindowButton()
        {
            if (!ButtonBig($"Manage {ThisTypePlural}", 300)) return;
            
            EditorPrefs.SetString("Game Modules Window Selected", ThisTypePlural);
            var window = (EditorWindowGameModules)EditorWindow.GetWindow(typeof(EditorWindowGameModules));
            window.Show();
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
            
            Cache();
            
            SetNameOnLevels();
            if (_modulesObject.hasBeenSetup) return true;

            if (string.IsNullOrEmpty(_modulesObject.objectType)) return false;
            if (_modulesObject.dictionaries.keyValues == null) return false;

            // Match settings with other objects of this type which all share the same settings (i.e. all turned on, otherwise, stay off)
            MatchDictionary(AttributesOfThisType);

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
        
        private void SetNameOnLevels() => _modulesObject.modificationLevel.ownerName = _modulesObject.objectName;

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
        
        
        private void MatchDictionary(ItemAttribute[] itemAttributes)
        {
            itemAttributes = itemAttributes.Where(x => x != _modulesObject).ToArray();
            
            var keyValues = itemAttributes
                .Except(itemAttributes
                    .Where(x => x == _modulesObject)
                    .ToArray()).SelectMany(x => x.dictionaries.keyValues)
                .Select(x => x.key)
                .Distinct()
                .ToList();
            
            foreach (var keyValue in keyValues)
            {
                if (string.IsNullOrWhiteSpace(keyValue)) continue;
                if (ObjectsOfTypeWithDictionaryKeyValue<ItemAttribute>(_modulesObject.objectType, keyValue).Length != itemAttributes.Length)
                    continue;

                //var newKeyValue = itemAttributes[0].GetKeyValue(keyValue).Clone();
                var newKeyValue = new KeyValue
                {
                    key = keyValue
                };
                _modulesObject.dictionaries.keyValues.Add(newKeyValue);
            }
        }
    }
}