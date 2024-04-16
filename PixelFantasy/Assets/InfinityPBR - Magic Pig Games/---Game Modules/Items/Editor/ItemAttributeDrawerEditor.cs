using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static InfinityPBR.InfinityEditor;
using static InfinityPBR.Modules.GameModuleUtilities;

namespace InfinityPBR.Modules
{
    class ItemAttributeDrawerEditor : ItemAttributeDrawer
    {
        private int fieldWidth;
        private Vector2 _scrollPosition;
        
        public ItemAttributeDrawerEditor(int fieldWidth)
        {
            this.fieldWidth = fieldWidth;
        }
        
        public ItemObject CopyObject => GameModuleObjectByName<ItemObject>(GetString("Item Object Attributes"));
        
        // ---------------------------------------------------------------------------
        // CACHING CODE
        // ---------------------------------------------------------------------------

        private int _objectsOfThisType;
        public string _cachedItemObjectType;
        public string _cachedItemObjectName;

        public ItemAttribute[] cachedItemAttributeArray;
        
        //private List<string> allTypes = new List<string>();
        private Dictionary<string, int> mainUseCount;
        
        private List<int> itemObjectUseCounts;
        private Dictionary<string, List<ItemAttribute>> itemAttributesOfType;
        private Dictionary<string, Dictionary<string, int>> itemObjectCountCanUseItemAttribute;
        private Dictionary<string, ItemObject[]> itemObjectArrayDictionary;
        private Dictionary<string, ItemObject[]> itemObjectsThatCanTakeType;

        private bool _resetCache = true;
        
        public void ResetCache(bool forceTrue = false)
        {
            if (!_resetCache && !forceTrue)
                return;
            
            _resetCache = false;
            DebugConsoleMessage("Item Attributes Cache Reset");

            if (CopyObject == null)
            {
                EditorPrefs.DeleteKey("Item Object Attributes");
            }
            
            mainUseCount = new Dictionary<string, int>();
            //allTypes = GetItemAttributeTypes().ToList();
            GameModuleObjectTypes<ItemAttribute>(true);
            GameModuleObjectTypes<ItemObject>(true);

            if (string.IsNullOrWhiteSpace(_cachedItemObjectType))
                _cachedItemObjectType = GameModuleObjectTypes<ItemObject>()[0];
            
            _objectsOfThisType = GameModuleObjectsOfType<ItemObject>(_cachedItemObjectType).Length;
            var cachedItemObjectsOfType = GameModuleObjectsOfType<ItemObject>(_cachedItemObjectType);
            itemObjectUseCounts = new List<int>();
            itemAttributesOfType = new Dictionary<string, List<ItemAttribute>>();
            itemObjectCountCanUseItemAttribute = new Dictionary<string, Dictionary<string, int>>();
            itemObjectArrayDictionary = new Dictionary<string, ItemObject[]>();
            itemObjectsThatCanTakeType = new Dictionary<string, ItemObject[]>();
                
            for (var c = 0; c < GameModuleObjectTypes<ItemAttribute>().Length; c++)
            {
                var objectsThatCan = ItemObjectsOfTypeThatCanUseItemAttributeType(_cachedItemObjectType, GameModuleObjectTypes<ItemAttribute>()[c]).Length;
                itemObjectUseCounts.Add(objectsThatCan);
                    
                itemAttributesOfType.Add(GameModuleObjectTypes<ItemAttribute>()[c], GameModuleObjectsOfType<ItemAttribute>(GameModuleObjectTypes<ItemAttribute>()[c]).ToList());

                var newDictionary = new Dictionary<string, int>();
                foreach (var itemAttribute in itemAttributesOfType[GameModuleObjectTypes<ItemAttribute>()[c]])
                {
                    if (string.IsNullOrWhiteSpace(itemAttribute.objectName)) continue;
                    int newCount = cachedItemObjectsOfType.Count(x => x.CanUseAttribute(itemAttribute));
                    newDictionary.Add(itemAttribute.objectName, newCount);
                    itemObjectArrayDictionary.Add(itemAttribute.objectName, cachedItemObjectsOfType.Where(x => x.CanUseAttribute(itemAttribute)).ToArray());
                    
                }
                itemObjectCountCanUseItemAttribute.Add(GameModuleObjectTypes<ItemAttribute>()[c], newDictionary);
                itemObjectsThatCanTakeType.Add(GameModuleObjectTypes<ItemAttribute>()[c], cachedItemObjectsOfType.Where(x => x.CanUseAttributeType(GameModuleObjectTypes<ItemAttribute>()[c])).ToArray());
            }

            cachedItemAttributeArray = GameModuleObjects<ItemAttribute>();
        }

        private ItemAttribute[] CachedItemAttributeArray()
        {
            if (cachedItemAttributeArray == null)
                cachedItemAttributeArray = GameModuleObjects<ItemAttribute>();

            return cachedItemAttributeArray;
        }
        
        // ---------------------------------------------------------------------------
        // DRAWING INDIVIDUALLY
        // ---------------------------------------------------------------------------

        /// <summary>
        /// This draws the Item Attribute window for individual Item Objects
        /// </summary>
        /// <param name="itemAttributeDrawer"></param>
        /// <param name="itemObject"></param>
        /// <param name="structure"></param>
        public void Draw(ItemAttributeDrawer itemAttributeDrawer, ItemObject itemObject, bool structure = false)
        {
            ResetCache();
            /*
            if (_cachedItemObjectName != itemObject.objectName)
            {
                Debug.Log($"_cachedItemObjectName is {_cachedItemObjectName} and itemObject.objectName is {itemObject.objectName}");
                _cachedItemObjectName = itemObject.objectName;
                ResetCache(true);
            }
            */

            DrawAllowedAttributes(itemObject);
            DrawStartingAttributes(itemObject);
            DrawVariables(itemObject);
        }

        private void DrawVariables(ItemObject itemObject)
        {
            Space();
            
            SetBool(KeyVariables, StartVerticalBoxSection(GetBool(KeyVariables), $"Variables ({itemObject.variables.Count}) {symbolInfo}",
                14, true,
                $"Add custom float variables to {itemObject.objectName}. Optionally set the variable to " +
                "be a \"range\" of 0f --> 1f, and then you can add multiple ItemAttributes to be used when the " +
                "value of the variable is within specific ranges.\n\nThe effect of the ItemAttributes can be " +
                "modified by the variable value, or the result of a defined curve."));

            if (GetBool(KeyVariables))
            {
                Space();
                ShowCopyPasteVariables(itemObject);
                Space();
                ShowVariables(itemObject);
                ShowAddVariable(itemObject);
            }

            EndVerticalBox();
        }

        private void ShowCopyPasteVariables(ItemObject itemObject)
        {
            if (!itemObject.showCopyPaste)
                return;
            if (!HasKey("Item Object Attributes"))
                return;
            
            BackgroundColor(Color.cyan);
            StartVerticalBox();
            Label($"Paste \"{GetString("Item Object Attributes")}\" {symbolInfo}",
                "Copy and paste values from other ItemObjects. Paste will replace the entire " +
                "set of Variables, while Append will only add variables that are not already here, without removing " +
                "other variables.");

            StartRow();

            if (HasKey("Item Object Attributes"))
            {
                if (Button("Paste", 60))
                    itemObject.CopyVariables(CopyObject);

                if (Button("Append", 80))
                    itemObject.CopyVariables(CopyObject, true);
            }
            
            EndRow();
            EndVerticalBox();
            BackgroundColor(Color.white);
        }

        private void CopyButton(ItemObject itemObject)
        {
            if (Button($"Copy {itemObject.objectName}", 150))
                SetString("Item Object Attributes", itemObject.objectName);
        }

        private void ShowVariables(ItemObject itemObject)
        {
            foreach (var variable in itemObject.variables)
            {
                ShowVariable(variable, itemObject);
                Space();
            }
        }

        private void ShowVariable(ItemObjectVariable variable, ItemObject itemObject)
        {
            ShowNameRow(variable, itemObject);
            if (!variable.show)
            {
                EndVerticalBox();
                return;
            }

            ShowRangeRow(variable, itemObject);

            if (variable.useCurve)
                variable.curve = Curve(variable.curve, variable.RangeValue, 400, 30);

            ShowVariableAttributes(variable, itemObject);
            ShowSingleVariableActions(variable, itemObject);
            EndVerticalBox();
        }

        private void ShowSingleVariableActions(ItemObjectVariable variable, ItemObject itemObject)
        {
            if (!itemObject.showCopyPaste)
                return;
            
            StartRow();
            Label("Actions: ", 60, true);
            ShowPasteThisVariable(variable, itemObject);
            ShowCopyThisVariableToAllOfType(variable, itemObject);
            EndRow();
        }

        private void ShowCopyThisVariableToAllOfType(ItemObjectVariable variable, ItemObject itemObject)
        {
            var objectType = itemObject.objectType;
            
            BackgroundColor(Color.cyan);
            if (Button($"Copy to ItemObjects of type \"{objectType}\"", 250))
            {
                DebugConsoleMessage($"Copying {variable.name} to all ItemObjects of type {objectType}");
                foreach (var obj in GameModuleObjectsOfType<ItemObject>(objectType))
                {
                    if (obj == itemObject) continue; // Skip the object we are copying

                    if (!obj.HasVariableOfAttributeType(variable.ObjectType)
                        && !obj.HasVariableName(variable.name))
                    {
                        
                        foreach(var v in obj.variables)
                            Debug.Log($"Variable name is {v.name}");
                        obj.variables.Add(variable.Clone);
                    }
                    else
                        obj.VariableOfType(variable.ObjectType).CopyValuesFrom(variable);
                }
            }
            ResetColor();
            
            
        }

        //private bool _cachedTypesThatReplaceOthers;
        private string[] _typesThatReplaceOthers;
        private int _replaceOtherTypesIndex;

        private void CacheTypesThatReplaceOthers()
        {
            // All ItemAttribute types where any of the ItemAttributes have replaceOthers set true
            _typesThatReplaceOthers = GameModuleObjectTypes<ItemAttribute>()
                .Where(x => GameModuleObjectsOfType<ItemAttribute>(x)
                    .Any(x => x.replaceOthers))
                .OrderBy(x => x).ToArray();
            //_cachedTypesThatReplaceOthers = true;
        }
        
        private void ShowVariableAttributes(ItemObjectVariable variable, ItemObject itemObject)
        {
            if (variable.variableAttributes.Count == 0)
            {
                ShowAddVariableAttributes(variable, itemObject);
                return;
            }

            StartRow();
            Label("", 25);
            Label("", 25);
            Label("", 25);
            Label("ItemAttribute", 150, true);
            Label($"RangeValue {symbolInfo}", "The ItemAttribute will be active when the value of the variable is greater than " +
                                              "this, but less than the next highest value. There must always be at least two ItemAttributes in the " +
                                              "list, with one at RangeValue of 0.", 100, true);
            EndRow();

            for (var index = 0; index < variable.variableAttributes.Count; index++)
            {
                var attribute = variable.variableAttributes[index];
                ShowVariableAttribute(attribute, variable, itemObject, index);
            }
        }

        private void ShowVariableAttribute(ItemObjectVariableAttribute attribute, ItemObjectVariable variable,
            ItemObject itemObject, int i)
        {
            StartRow();
            if (XButton())
            {
                if (variable.variableAttributes.Count > 2)
                {
                    variable.variableAttributes.RemoveAll(x => x == attribute);
                    ExitGUI();
                }
                else
                {
                    if (Dialog($"Remove all {attribute.ItemAttribute.objectType} Attributes?",
                            "At least two ItemAttributes are required. Do you want to delete both?"))
                    {
                        variable.variableAttributes = new List<ItemObjectVariableAttribute>();
                        ExitGUI();
                    }
                }
            }

            var attributeCount = variable.variableAttributes.Count;
            // Move Up Button
            ColorsIf(CanMoveUp(i, attributeCount)
                , Color.white
                , Color.black
                , Color.black
                , Color.grey);
            if (Button(symbolArrowUp, 25) && CanMoveUp(i, attributeCount))
            {
                var value1 = variable.variableAttributes[i - 1].value;
                var value2 = variable.variableAttributes[i].value;
                
                (variable.variableAttributes[i - 1], variable.variableAttributes[i]) = (variable.variableAttributes[i],
                    variable.variableAttributes[i - 1]);

                variable.variableAttributes[i - 1].value = value1;
                variable.variableAttributes[i].value = value2;
            }
                
            // Move Down Button
            ColorsIf(CanMoveDown(i, variable.variableAttributes.Count)
                , Color.white
                , Color.black
                , Color.black
                , Color.grey);
            if (Button(symbolArrowDown, 25) && CanMoveDown(i, attributeCount))
            {
                var value1 = variable.variableAttributes[i + 1].value;
                var value2 = variable.variableAttributes[i].value;
                
                (variable.variableAttributes[i + 1], variable.variableAttributes[i]) = (variable.variableAttributes[i],
                    variable.variableAttributes[i + 1]);
                
                variable.variableAttributes[i + 1].value = value1;
                variable.variableAttributes[i].value = value2;
            }
            
            // Name of ItemAttribute
            ResetColor();
            ContentColor(variable.ActiveAttribute == attribute.ItemAttribute ? Color.green : Color.white);
            Label($"{attribute.ItemAttribute.objectName}", 150);
            ResetColor();
            // RangeValue
            if (attribute.value == 0)
            {
                Label("0", 25);
                LabelGrey("* One value must always be 0");
            }
            else
            {
                var tempValue = attribute.value;
                var newValue = DelayedFloat(attribute.value, 100);
                if (tempValue != newValue)
                {
                    attribute.value = newValue;
                    variable.variableAttributes = variable.variableAttributes.OrderBy(x => x.value).ToList();
                }
            }
            EndRow();
        }

        private void ShowAddVariableAttributes(ItemObjectVariable variable, ItemObject itemObject)
        {
            CacheTypesThatReplaceOthers();
            
            BackgroundColor(Color.cyan);
            StartRow();
            Label($"Add Variable Attributes {symbolInfo}", "ItemAttributes can be added to a GameItemObject based on the " +
                                                           $"RangeValue of the {variable.name} variable. Select an ItemAttribute type which " +
                                                           "has \"Replace Others\" set true. A minimum of two attributes are required.", 150);
            if (GameModuleObjects<ItemAttribute>().Length == 0)
            {
                Label("<i><color=#999999>No ItemAttributes found</color></i>", 150, false, false, true);
            }
            else
            {
                _replaceOtherTypesIndex = Popup(_replaceOtherTypesIndex, _typesThatReplaceOthers, 150);
                if (Button("Add", 50))
                {
                    var selectedType = _typesThatReplaceOthers[_replaceOtherTypesIndex];
                    var attributesOfType = GameModuleObjectsOfType<ItemAttribute>(selectedType);
                    var startValue = 0f;
                    var interval = 1f / (attributesOfType.Length);
                
                    foreach (var attribute in attributesOfType)
                    {
                        variable.AddAttribute(attribute, startValue);
                        startValue += interval;
                        itemObject.ForceCanUseAttribute(attribute);
                    }
                
                    // Remove from Starting Item Attributes
                    if (itemObject.HasStartingItemAttributeType(selectedType))
                        Debug.LogWarning($"Important: At least one Starting Item Attribute was type {selectedType}, and was " +
                                         "removed from the Starting Item Attribute list.");
                    itemObject.startingItemAttributes.RemoveAll(x => x.objectType == selectedType);
                }
            }
            EndRow();
            ResetColor();
        }

        private void ShowRangeRow(ItemObjectVariable variable, ItemObject itemObject)
        {
            StartRow();
            Label("Min", 30);
            var tempMin = variable.min;
            variable.min = Float(variable.min, 50);
            if (tempMin != variable.min)
            {
                if (variable.min > variable.max)
                {
                    Debug.Log("Min must be less than max.");
                    variable.min = tempMin;
                }
                else
                    variable.SetToRangeValue(variable.RangeValue);
            }
            Label("", 20);
            Label("Max", 30);
            var tempMax = variable.max;
            variable.max = Float(variable.max, 50);
            if (tempMax != variable.max)
            {
                if (variable.max < variable.min)
                {
                    Debug.Log("Max must be greater than min.");
                    variable.max = tempMax;
                }
                else
                    variable.SetToRangeValue(variable.RangeValue);
            }
            Label("", 20);
            Label("Value", 50);
            var tempValue = variable.value;
            variable.value = SliderFloat(variable.value, variable.min, variable.max, 150);
            if (tempValue != variable.value)
                variable.SetValue(variable.value, true);
            Label($"RangeValue = {variable.RangeValue} {symbolInfo}", $"While the value {variable.value} is between min {variable.min} and " +
                                                                      $"max {variable.max}, the RangeValue will be either a normalized value between " +
                                                                      "0.0 and 1.0, or the evaluation result of an curve, if a curve is being used.");
            EndRow();
        }

        private void ShowNameRow(ItemObjectVariable variable, ItemObject itemObject)
        {
            StartVerticalBox();
            StartRow();
            variable.show = ButtonOpenClose(variable.show);
            
            if (!variable.show)
            {
                Label($"{variable.name}");
                EndRow();
                return;
            }
            
            if (XButton())
            {
                itemObject.variables.RemoveAll(x => x.name == variable.name);
                ExitGUI();
            }

            var tempName = variable.name;
            var newName = DelayedText(variable.name, 150);
            if (tempName != newName)
            {
                if (string.IsNullOrWhiteSpace(newName))
                    Debug.Log("Name can not be empty.");
                else if (itemObject.HasVariableName(newName))
                    Debug.Log($"There is already a variable named {newName}");
                else
                    variable.name = newName;
            }

            variable.useCurve = LeftCheck($"Use Curve {symbolInfo}", "When true, the RangeValue will be the evaluation result of " +
                                                                     "an animation curve. Otherwise the RangeValue will be a normalized " +
                                                                     "representation of the value between 0 and 1.", variable.useCurve, 110);

            //ShowPasteThisVariable(variable, itemObject);
            EndRow();
        }

        private void ShowPasteThisVariable(ItemObjectVariable variable, ItemObject itemObject)
        {
            if (!HasKey("Item Object Attributes"))
                return;

            if (!CopyObject.HasVariableOfAttributeType(variable.ObjectType))
                return;
            
            BackgroundColor(Color.cyan);
            if (Button($"Paste from \"{GetString("Item Object Attributes")}\"", 200))
                variable.CopyValuesFrom(CopyObject.VariableOfType(variable.ObjectType));
            ResetColor();
        }

        private string VariableName => GetString("Add Variable");
        
        private void ShowAddVariable(ItemObject itemObject)
        {
            BackgroundColor(Color.yellow);
            StartVerticalBox();
            StartRow();
            
            Label("Add Variables: ", 150);
            SetString("Add Variable", TextField(VariableName, 150));
            if (Button("Add", 50))
            {
                if (itemObject.variables.All(x => x.name != VariableName))
                {
                    itemObject.variables.Add(new ItemObjectVariable
                    {
                        name = VariableName,
                        min = 0,
                        max = 1,
                        value = 1,
                        rangeValue = 1
                    });
                }
                else
                    Debug.Log($"There is already a variable named {VariableName}.");
            }

            EndRow();
            EndVerticalBox();
            ResetColor();
        }

        private void DrawStartingAttributes(ItemObject itemObject)
        {
            Space();
            
            SetBool(KeyStarting, StartVerticalBoxSection(GetBool(KeyStarting), $"Starting Item Attributes ({itemObject.startingItemAttributes.Count}) {symbolInfo}",
                14, true,
                "These attributes will be added automatically whenever a GameItemObject is created out " +
                    "of this ItemObject. Often this can be useful for descriptive attributes, such as \"Rarity\" or " +
                    "\"Quality\"."));
            

            if (GetBool(KeyStarting))
            {
                Space();
                ShowCopyPasteStartingAttributes(itemObject);
                Space();
                ShowStartingAttributes(itemObject);
                Space();
                ShowAddStartingAttribute(itemObject);
            }

            

            EndVerticalBox();
        }

        private void ShowAddStartingAttribute(ItemObject itemObject)
        {
            BackgroundColor(Color.yellow);
            StartVerticalBox();
            StartRow();
            
            Label("Add Starting Attribute: ", 150);
            ItemAttribute newAttribute = null;
            newAttribute = Object(null, typeof(ItemAttribute), 150) as ItemAttribute;
            if (newAttribute != null)
                AddStartingAttribute(itemObject, newAttribute);

            EndRow();
            EndVerticalBox();
            ResetColor();
        }

        private void AddStartingAttribute(ItemObject itemObject, ItemAttribute newAttribute)
        {
            // Check to make sure there aren't any conflicting starting attributes
            if (newAttribute.distinct && itemObject.HasStartingItemAttributeType(newAttribute.objectType))
            {
                Debug.Log("There is already a starting attribute of type " +
                          $"\"{newAttribute.objectType}\". Please remove that before " +
                          $"adding {newAttribute.objectName}.");
                return;
            }
            
            // Check to make sure there aren't any conflicting Varibles
            if (itemObject.variables
                .Any(x => x.variableAttributes
                    .Any(y => y.ItemAttribute.objectType == newAttribute.objectType)))
            {
                Debug.Log("There is already a variable making use of type " +
                          $"\"{newAttribute.objectType}\". Please remove that before " +
                          $"adding {newAttribute.objectName}.");
                return;
            }
            
            // If we don't have it already then add it
            if (!itemObject.HasStartingItemAttribute(newAttribute))
                itemObject.startingItemAttributes.Add(newAttribute);

            // Make sure the itemObject can use the newAttribute
            itemObject.ForceCanUseAttribute(newAttribute);
        }

        private void ShowStartingAttributes(ItemObject itemObject)
        {
            foreach (var attribute in itemObject.startingItemAttributes)
            {
                StartRow();
                BackgroundColor(Color.red);
                if (Button(symbolX, 25))
                {
                    itemObject.startingItemAttributes.Remove(attribute);
                    ExitGUI();
                }
                ResetColor();
                Object(attribute, typeof(ItemAttribute), 200);

                if (attribute.distinct)
                    ContentColor(Color.cyan);
                Label($"{attribute.objectType}{(attribute.distinct ? "*": "")}", 200);
                ResetColor();
                EndRow();
            }
        }

        private void DrawAllowedAttributes(ItemObject itemObject)
        {
            Space();
            ShowCopyPasteAll(itemObject);
            Space();

            SetBool(KeyAllowed, StartVerticalBoxSection(GetBool(KeyAllowed), $"Allowed Item Attributes {symbolInfo}",
                14, true,
                "Not all Item Objects may be able to use all Item Attributes. The system will ensure that " +
                "only attributes which are allowed are added to items. Select \"Item Attribute Types\", and then " +
                $"individual Item Attributes to allow those to be used with this {itemObject.objectName}. You can " +
                "also copy the settings, and then paste them on other objects to speed things up."));

            if (GetBool(KeyAllowed))
            {
                Label(itemObject.allowedItemAttributeObjectTypes.Count + " types & " + itemObject.allowedItemAttributes.Count + " possible attributes");
                Space();
                ShowCopyPasteAllowedAttributes(itemObject);
                Space();
                ShowItemAttributeTypes(itemObject);
                ShowItemAttributeAttributes(itemObject);
            }

            EndVerticalBox();
        }

        private void ShowCopyPasteAll(ItemObject itemObject)
        {
            StartRow();
            itemObject.showCopyPaste = Check(itemObject.showCopyPaste, 20);
            Label("Show Copy/Paste Options");
            EndRow();

            if (!itemObject.showCopyPaste)
                return;
            
            BackgroundColor(Color.cyan);
            StartRow();
            CopyButton(itemObject);
            if (HasKey("Item Object Attributes")
                && Button($"Paste all values from \"{GetString("Item Object Attributes")}\"", 250))
            {
                itemObject.CopyAllowedItemAttributes(CopyObject);
                itemObject.CopyStartingAttributes(CopyObject);
                itemObject.CopyVariables(CopyObject);
            }
            ResetColor();
            EndRow();
        }
        
        private string KeyAllowed => "Show Allowed Attributes";
        private string KeyStarting => "Show Starting Attributes";
        private string KeyVariables => "Show Variables";
        
        private void ShowHeaderVariables(ItemObject itemObject)
        {
            StartRow();
            ButtonOpenClose(KeyVariables);
            LabelSized($"Variables ({itemObject.variables.Count}) {symbolInfo}", $"Add custom float variables to {itemObject.objectName}. Optionally set the variable to " +
                       "be a \"range\" of 0f --> 1f, and then you can add multiple ItemAttributes to be used when the " +
                       "value of the variable is within specific ranges.\n\nThe effect of the ItemAttributes can be " +
                       "modified by the variable value, or the result of a defined curve.", 18, true);
            
            EndRow();
        }
        
        private void ShowHeaderStartingAttributes(ItemObject itemObject)
        {
            StartRow();
            ButtonOpenClose(KeyStarting);
            LabelSized($"Starting Item Attributes ({itemObject.startingItemAttributes.Count}) {symbolInfo}", "These attributes will be added automatically whenever a GameItemObject is created out " +
                "of this ItemObject. Often this can be useful for descriptive attributes, such as \"Rarity\" or " +
                "\"Quality\".", 18, true);
            
            EndRow();
        }
        
        private void ShowCopyPasteAllowedAttributes(ItemObject itemObject)
        {
            if (!itemObject.showCopyPaste)
                return;
            
            BackgroundColor(Color.cyan);
            StartVerticalBox();
            Label($"Paste \"{GetString("Item Object Attributes")}\" {symbolInfo}", "Copy and paste or append the allowed ItemAttributes from " +
                "another object. Copy will replace the values entirely, while append will add any true values while keeping existing true values.");
            
            StartRow();
            
            if (HasKey("Item Object Attributes"))
            {
                if (Button("Paste", 60))
                    itemObject.CopyAllowedItemAttributes(CopyObject);

                if (Button("Append", 80))
                    itemObject.CopyAllowedItemAttributes(CopyObject,null, true);
            }
            
            EndRow();
            EndVerticalBox();
            BackgroundColor(Color.white);
        }

        private void ShowCopyPasteStartingAttributes(ItemObject itemObject)
        {
            if (!itemObject.showCopyPaste)
                return;
            if (!HasKey("Item Object Attributes"))
                return;
            
            BackgroundColor(Color.cyan);
            StartVerticalBox();
            Label($"Paste \"{GetString("Item Object Attributes")}\" {symbolInfo}", "Copy and paste values from other ItemObjects. Paste will replace the entire " +
                                              "list, while Append will only add what is not in this list already, without removing " +
                                              "other values.\n\nThe \"Append (One per objectType)\" option will ensure that only " +
                                              "one ItemAttribute per type is appended. As an example, for a type of \"Rarity\" if the " +
                                              "ItemObject already has \"Rare\", then another one called \"Common\" will not be appended.");
            
            StartRow();

            if (HasKey("Item Object Attributes"))
            {
                if (Button("Paste", 60))
                    itemObject.CopyStartingAttributes(CopyObject);

                if (Button("Append", 80))
                    itemObject.CopyStartingAttributes(CopyObject, true);
                
                if (Button("Append (One per objectType)", 200))
                    itemObject.CopyStartingAttributes(CopyObject, true, true);
            }
            
            EndRow();
            EndVerticalBox();
            BackgroundColor(Color.white);
        }
        
        private void ShowItemAttributesHeader(ItemObject itemObject)
        {
            StartRow();
            ButtonOpenClose(KeyAllowed);
            LabelSized($"Allowed Item Attributes {symbolInfo}", "Not all Item Objects may be able to use all Item Attributes. The system will ensure that " +
                                                                "only attributes which are allowed are added to items. Select \"Item Attribute Types\", and then " +
                                                                $"individual Item Attributes to allow those to be used with this {itemObject.objectName}. You can " +
                                                                "also copy the settings, and then paste them on other objects to speed things up.", 18, true);
            EndRow();
            Label(itemObject.allowedItemAttributeObjectTypes.Count + " types & " + itemObject.allowedItemAttributes.Count + " possible attributes");
        }
        
        private void ShowItemAttributeTypes(ItemObject itemObject)
        {
            itemObject.showItemAttributeTypes = ButtonSectionBig(itemObject.showItemAttributeTypes,
                $"Item Attribute Types ({GameModuleObjectTypes<ItemAttribute>().Length})", 220);
            if (!itemObject.showItemAttributeTypes) return;

            foreach (var itemAttributeType in GameModuleObjectTypes<ItemAttribute>())
            {
                StartRow();
                BackgroundColor(itemObject.CanUseAttributeType(itemAttributeType) ? Color.white : Color.black);
                ContentColor(itemObject.CanUseAttributeType(itemAttributeType) ? Color.white : Color.grey);
                
                if (Button(itemAttributeType, fieldWidth))
                {
                    if (itemObject.CanUseAttributeType(itemAttributeType))
                    {
                        if (itemObject.HasStartingItemAttributeType(itemAttributeType))
                            Debug.Log($"{itemAttributeType} is represented in Starting Item Attributes. Please remove it " +
                                      "from that list before toggling this off.");
                        else if (itemObject.HasVariableOfAttributeType(itemAttributeType))
                            Debug.Log($"A variable is using the type {itemAttributeType}. Please remove it " +
                                      "from before toggling this off.");
                        else
                            itemObject.ToggleCanUseAttributeType(itemAttributeType);
                    }
                    else
                        itemObject.ToggleCanUseAttributeType(itemAttributeType);
                    
                    UpdateEditorWindow();
                }
                
                BackgroundColor(Color.white);
                ContentColor(Color.white);

                if (HasKey("Item Object Attributes") && itemObject.showCopyPaste)
                {
                    BackgroundColor(Color.cyan);
                    if (Button("Paste", 60))
                        itemObject.CopyAllowedItemAttributes(CopyObject, itemAttributeType);
                    if (Button("Append", 80))
                        itemObject.CopyAllowedItemAttributes(CopyObject, itemAttributeType, true);
                    ResetColor();
                }
                EndRow();
            }

        }

        private void ShowItemAttributeAttributes(ItemObject itemObject)
        {
            Space();
            itemObject.showItemAttributesAttributes = ButtonSectionBig(itemObject.showItemAttributesAttributes,
                "Available Item Attributes", 220);
            if (!itemObject.showItemAttributesAttributes) return;

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            StartRow();
            
            foreach (string itemAttributeObjectType in itemObject.allowedItemAttributeObjectTypes)
            {
                StartVertical();
                Label(itemAttributeObjectType, fieldWidth, true);
                
                var ItemAttributesThatCanBeHad = CachedItemAttributeArray().
                    Where(x => x.objectType == itemAttributeObjectType).
                    Where(x => itemObject.CanUseAttributeType(x));
                
                foreach (var itemAttribute in ItemAttributesThatCanBeHad)
                {
                    BackgroundColor(itemObject.CanUseAttribute(itemAttribute) ? Color.white : Color.black);
                    ContentColor(itemObject.CanUseAttribute(itemAttribute) ? Color.white : Color.grey);
                    if (Button(itemAttribute.objectName, fieldWidth))
                    {
                        if (itemObject.CanUseAttribute(itemAttribute))
                        {
                            if (itemObject.HasStartingItemAttribute(itemAttribute))
                                Debug.Log($"{itemAttribute.objectName} is a Starting Item Attribute. Please remove it " +
                                      "from that list before toggling this off.");
                            else if (itemObject.HasVariableOfAttributeType(itemAttribute))
                                Debug.Log($"A variable is using the type {itemAttribute.objectType}. Please remove it " +
                                          "from before toggling this off.");
                            else
                                itemObject.ToggleCanUseItemAttribute(itemAttribute);
                        }
                        else
                            itemObject.ToggleCanUseItemAttribute(itemAttribute);
                        UpdateEditorWindow();
                    }
                    BackgroundColor(Color.white);
                    ContentColor(Color.white);
                }
                
                EndVerticalBox();
            }

            EndRow();
            EditorGUILayout.EndScrollView();
        }

        private void UpdateEditorWindow()
        {
            _cachedItemObjectType = "";
        }

        // ---------------------------------------------------------------------------
        // STRUCTURE ONLY (Editor Window)
        // ---------------------------------------------------------------------------
        
        /// <summary>
        /// This draws the Item Attribute window for the entire stucture, with a different view.
        /// </summary>
        /// <param name="itemAttributeDrawer"></param>
        public void DrawStructure(ItemAttributeDrawer itemAttributeDrawer, string itemObjectType)
        {

            if (_cachedItemObjectType != itemObjectType)
            {
                _cachedItemObjectType = itemObjectType;
                ResetCache();
            }

            Space();
            BackgroundColor(new Color(.6f, 1f, 0.6f, 1f));
            StartVerticalBox();
            ResetColor();
            ShowItemAttributeTypesStructure(itemObjectType);
            EndVerticalBox();
            
            ShowItemAttributeAttributesStructure(itemObjectType);
        }
        
        private void ShowItemAttributeAttributesStructure(string itemObjectType)
        {
            for (int t = 0; t < GameModuleObjectTypes<ItemAttribute>().Length; t++)
            {
                var itemAttributeType = GameModuleObjectTypes<ItemAttribute>()[t];

                if (!EditorPrefs.GetBool(KeyString(itemAttributeType)))
                    continue;
                
                var useCount = itemObjectUseCounts[t];
                if (useCount != _objectsOfThisType) continue;
                
                Space();
                StartVerticalBox();
                Header3(itemAttributeType);

                foreach (var itemAttribute in itemAttributesOfType[itemAttributeType])
                {
                    var useCountOfThisAttribute = itemObjectCountCanUseItemAttribute[itemAttributeType][itemAttribute.objectName];
                    var startingAttributeCount =
                        ItemObjectsOfTypeWithStartingItemAttributeType(itemObjectType, itemAttribute.objectType).Length;
                    var variableAttributeCount =
                        ItemObjectsOfTypeWithItemAttributeTypeVariable(itemObjectType, itemAttribute.objectType).Length;
                    
                    StartRow();
                    Label(itemAttribute.objectName, fieldWidth);
                    
                    BackgroundColor(useCountOfThisAttribute == 0 
                                    || startingAttributeCount > 0 
                                    || variableAttributeCount > 0 ? Color.black : Color.white);
                    ContentColor(useCountOfThisAttribute == 0 ? Color.black : (startingAttributeCount > 0 
                                                                               || variableAttributeCount > 0 ? Color.grey : Color.white));
                    if (Button("Remove", 75))
                    {
                        // Check starting
                        if (startingAttributeCount > 0)
                        {
                            Debug.Log($"Can't remove. There are {startingAttributeCount} {itemObjectType} " +
                                      $"with starting attributes of type {itemAttribute.objectType}.");
                        }
                        else if(variableAttributeCount > 0)
                        {
                            Debug.Log($"Can't remove. There are {variableAttributeCount} {itemObjectType} " +
                                      $"with Item Attribute Variables of type {itemAttribute.objectType}.");
                        }
                        else if (useCountOfThisAttribute > 0)
                        {
                            ToggleAttributeForAllObjectsOfType(itemAttribute, itemObjectType, false);
                            UpdateEditorWindow();
                        }
                    }
                
                    BackgroundColor(useCountOfThisAttribute == _objectsOfThisType ? Color.black : Color.white);
                    ContentColor(useCountOfThisAttribute == _objectsOfThisType ? Color.black : Color.white);
                    if (Button("Add", 75))
                    {
                        if (useCountOfThisAttribute != _objectsOfThisType)
                        {
                            ToggleAttributeForAllObjectsOfType(itemAttribute, itemObjectType, true);
                            UpdateEditorWindow();
                        }
                    }
                    BackgroundColor(Color.white);
                    ContentColor(Color.white);
                
                    
                    ContentColor(useCountOfThisAttribute > 0
                        ? useCountOfThisAttribute == _objectsOfThisType ? Color.white : Color.yellow : Color.grey);
                    string useCountString = useCountOfThisAttribute > 0
                        ? useCountOfThisAttribute == _objectsOfThisType ? "all" : useCountOfThisAttribute.ToString() : "none";
                    string tooltip = "";
                    if (useCountOfThisAttribute > 0 && useCountOfThisAttribute < _objectsOfThisType)
                    {
                        
                        //ItemObject[] itemObjects = ItemObjectsOfTypeThatHaveItemAttribute(itemObjectType, itemAttribute);
                        ItemObject[] itemObjects = itemObjectArrayDictionary[itemAttribute.objectName];
                        for (int i = 0; i < itemObjects.Length; i++)
                        {
                            if (i > 0) tooltip = $"{tooltip}, ";
                            tooltip = $"{tooltip}{itemObjects[i].objectName}";
                        }
                    }
                    
                    var startingString = "";
                    var variableString = "";
                    if (startingAttributeCount > 0)
                        startingString = $"[Starting Attributes: {startingAttributeCount}]";
                    if (variableAttributeCount > 0)
                        variableString = $"[Variable Attributes: {variableAttributeCount}]";
                
                    Label($"[Active in {useCountString}] {startingString} {variableString}", tooltip);
                    BackgroundColor(Color.white);
                    ContentColor(Color.white);
                    
                    EndRow();
                    
                }

                EndVerticalBox();
            }
        }
        
       private string KeyString(string itemAttributeType) => $"ItemObjectManagerItemAttributes_{itemAttributeType}";
        
        private void ShowItemAttributeTypesStructure(string itemObjectType)
        {

            Header2("Item Attribute Types");
            
            for (var t = 0; t < GameModuleObjectTypes<ItemAttribute>().Length; t++)
            {
                string itemAttributeType = GameModuleObjectTypes<ItemAttribute>()[t];
                var useCount = itemObjectUseCounts[t];
                
                var startingAttributeCount =
                    ItemObjectsOfTypeWithStartingItemAttributeType(itemObjectType, itemAttributeType).Length;
                var variableAttributeCount =
                    ItemObjectsOfTypeWithItemAttributeTypeVariable(itemObjectType, itemAttributeType).Length;
                
                
                StartRow();

                if (!HasKey(KeyString(itemAttributeType)))
                    EditorPrefs.SetBool(KeyString(itemAttributeType), true);

                if (useCount == 0)
                    Label("", 25);
                else 
                    ButtonOpenClose(KeyString(itemAttributeType));
                
                //EditorPrefs.SetBool(KeyString(itemAttributeType), ButtonToggleEye(EditorPrefs.GetBool(KeyString(itemAttributeType)), 25));
                Label(itemAttributeType, fieldWidth);
                
                BackgroundColor(useCount == 0 
                                || startingAttributeCount > 0 
                                || variableAttributeCount > 0 ? Color.black : Color.white);
                ContentColor(useCount == 0 ? Color.black : (startingAttributeCount > 0 
                                                            || variableAttributeCount > 0 ? Color.grey : Color.white));
                if (Button("Remove", 75))
                {
                    if (startingAttributeCount > 0)
                    {
                        Debug.Log($"Can't remove. There are {startingAttributeCount} {itemObjectType} " +
                                  $"with starting attributes of type {itemAttributeType}.");
                    }
                    else if(variableAttributeCount > 0)
                    {
                        Debug.Log($"Can't remove. There are {variableAttributeCount} {itemObjectType} " +
                                  $"with Item Attribute Variables of type {itemAttributeType}.");
                    }
                    else if (useCount > 0)
                    {
                        ToggleAttributeTypeForAllObjectsOfType(itemAttributeType, itemObjectType, false);
                        UpdateEditorWindow();
                    }
                }
                
                BackgroundColor(useCount == _objectsOfThisType ? Color.black : Color.white);
                ContentColor(useCount == _objectsOfThisType ? Color.black : Color.white);
                if (Button("Add", 75))
                {
                    if (useCount != _objectsOfThisType)
                    {
                        ToggleAttributeTypeForAllObjectsOfType(itemAttributeType, itemObjectType, true);
                        UpdateEditorWindow();
                    }
                }
                BackgroundColor(Color.white);
                ContentColor(Color.white);
                
                ContentColor(useCount > 0
                    ? useCount == _objectsOfThisType ? Color.white : Color.yellow : Color.grey);
                string useCountString = useCount > 0
                    ? useCount == _objectsOfThisType ? "all" : useCount.ToString() : "none";
                string tooltip = "";
                if (useCount > 0 && useCount < _objectsOfThisType)
                {
                    ItemObject[] itemObjects = itemObjectsThatCanTakeType[itemAttributeType];
                    for (int i = 0; i < itemObjects.Length; i++)
                    {
                        if (i > 0) tooltip = $"{tooltip}, ";
                        tooltip = $"{tooltip}{itemObjects[i].objectName}";
                    }
                }

                var startingString = "";
                var variableString = "";
                if (startingAttributeCount > 0)
                    startingString = $"[Starting Attributes: {startingAttributeCount}]";
                if (variableAttributeCount > 0)
                    variableString = $"[Variable Attributes: {variableAttributeCount}]";
                
                Label($"[Active in {useCountString}] {startingString} {variableString}", tooltip);
                
                ContentColor(Color.white);
                
                EndRow();
            }

        }
        
        private void ToggleAttributeTypeForAllObjectsOfType(string itemAttributeType, string itemObjectType, bool canUse)
        {
            foreach (var itemObject in GameModuleObjectsOfType<ItemObject>(itemObjectType))
            {
                Undo.RecordObject(itemObject, "Undo Attribute Changes");
                itemObject.SetCanUseAttributeType(itemAttributeType, canUse);
            }
        }
        
        private void ToggleAttributeForAllObjectsOfType(ItemAttribute itemAttribute, string itemObjectType, bool canUse)
        {
            foreach (var itemObject in GameModuleObjectsOfType<ItemObject>(itemObjectType))
            {
                Undo.RecordObject(itemObject, "Undo Attribute Changes");
                itemObject.ToggleCanUseItemAttribute(itemAttribute, canUse);
            }
        }
        
    }
}