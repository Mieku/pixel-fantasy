//using static InfinityPBR.Modules.EditorUtilities;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static InfinityPBR.InfinityEditor;
using static InfinityPBR.Modules.GameModuleUtilities;
using Object = UnityEngine.Object;

namespace InfinityPBR.Modules
{
    public class DictionariesDrawerEditor : DictionariesDrawer
    {
        public EditorWindow thisEditorWindow;
        public Object ThisObject;
        private KeyValueDrawer _keyValueDrawer;
        private readonly int fieldWidth;
        private string _objectType;
        public string className = "Item Object";

        public string ObjectType => _objectType;

        public DictionariesDrawerEditor(int fieldWidth) => this.fieldWidth = fieldWidth;
        
        public void SetThisWindow(EditorWindow newEditorWindow) => thisEditorWindow = newEditorWindow;

        public void SetKeyValueDrawer()
        {
            if (_keyValueDrawer != null) return;
            _keyValueDrawer = new KeyValueDrawerEditor(fieldWidth);
        }

        public void Cache(string objectType)
        {
            
        }
        
        public void Draw(Dictionaries dictionaries, string newClassName, string objectName, string objectType)
        {
            SetKeyValueDrawer();

            // Ensure name is the "simple" type -- Feb 15, 2023 -- this is during debug etc, I can remove this later.
            foreach (var keyValueList in dictionaries.keyValues.SelectMany(keyValue => keyValue.values))
                keyValueList.typeName = CleanTypeName(keyValueList.typeName);

            Space();
            foreach (var keyValue in dictionaries.keyValues)
            {
                DrawKeyValue(keyValue);
                Space();
            }

            //return;
            // OLDER VERSION
            className = newClassName;
            SetKeyValueDrawer();
            
            if (_keyValueDrawer.AllDictionariesIsNull())
                DoCaching(objectType);

            if (EditorPrefs.GetBool("Show V4 Migration Options Dictionary", false))
            {
                Space();
                BackgroundColor(Color.magenta);
                StartVerticalBox();
                Label($"V4 Migration Options {symbolInfo}", "Dictionaries should migrate automatically to v4, which " +
                                                            "includes a new data structure. If they do not, you can Clear " +
                                                            "New Lists and then click the Migrate to v4 button to do it " +
                                                            "again.", true);
                if (Button("Clear New Lists"))
                {
                    foreach (var keyValue in dictionaries.keyValues)
                    {
                        keyValue.values.Clear();
                        keyValue.cachedLists.Clear();
                    }
                }

                if (Button("Migrate to v4"))
                {
                    foreach (var keyValue in dictionaries.keyValues)
                        keyValue.MigrateToVersion4(true);
                }

                SetBool("Show 3.x version of Dictionaries", Check($"Show 3.x version of Dictionaries {symbolInfo}",
                    "Click to see the old Dictionaries structure. You should " +
                    "use the new structure, but this will remain for a while to help with migration.",
                    GetBool("Show 3.x version of Dictionaries"), 150));

                EndVerticalBox();
                ResetColor();

                if (GetBool("Show 3.x version of Dictionaries"))
                    ShowCommonDictionary(dictionaries);
            }
        }

        private void DrawKeyValue(KeyValue keyValue)
        {
            //StartRow();
            keyValue.showValues = StartVerticalBoxSection(keyValue.showValues, $"{keyValue.key} [{keyValue.values.Count} types, {keyValue.TotalObjects} objects]");
            
            //if (keyValue.showValues)
             //   StartVerticalBox();
            
            
            //keyValue.showValues = OnOffButton(keyValue.showValues);
            //Label($"{keyValue.key} [{keyValue.values.Count} types, {keyValue.TotalObjects} objects]");
            //EndRow();

            if (keyValue.showValues)
            {
                foreach (var keyValueList in keyValue.values)
                {
                    ShowKeyValueList(keyValueList, keyValue);
                }

                ShowAddListType(keyValue);
            }
            EndVerticalBox();
        }

        private void ShowAddListType(KeyValue keyValue)
        {
            Space();
            StartRow();
            if (keyValue.UsingAllTypes)
            {
                LabelGrey("You are currently using all supported types! Wow! That's not expected!");
                EndRow();
                return;
            }

            BackgroundColor(Color.yellow);
            Label($"Add List Type {symbolInfo}", $"Each Key, such as {keyValue.key}, can have a one list of each " +
                                                 "type supported by the Game Modules Dictionaries. Each list can " +
                                                 "have any number of items in it.", 120);
            keyValue.selectedTypeString = keyValue.AvailableTypes[Popup(keyValue.SelectedIndex, keyValue.AvailableTypes, 120)];
            if (Button("Add", 50))
                keyValue.KeyValueList(keyValue.selectedTypeString, false);
            ResetColor();
            EndRow();
        }

        private void ShowKeyValueList(KeyValueList keyValueList, KeyValue keyValue)
        {
            Space();
            StartRow();
            BackgroundColor(Color.red);
            if (Button($"{symbolX}", 22))
            {
                keyValue.values.Remove(keyValueList);
                ExitGUI();
            }
            BackgroundColorIf(keyValueList.showValues, Color.white, Color.black);
            ContentColorIf(keyValueList.showValues, Color.white, Color.grey);

            if (Button($"{keyValueList.typeName}", 150))
                keyValueList.showValues = !keyValueList.showValues;
            ResetColor();
            ShowAddNewValue(keyValueList, keyValue);
            Label($"[{keyValueList.objects.Count} objects]", 80);
            EndRow();

            if (!keyValueList.showValues)
                return;

            var thisType = Type.GetType(keyValueList.typeName);
            ShowKeyValueListObjects(thisType, keyValueList, keyValue);
        }

        private void ShowAddNewValue(KeyValueList keyValueList, KeyValue keyValue)
        {
            BackgroundColor(Color.yellow);

            if (HandleBasics(keyValueList, keyValue))
            {
                ResetColor();
                return;
                ExitGUI();
            }

            object newObject = null;
                
            if (keyValueList.typeName == "Stat") newObject = Object(null, typeof(Stat), fieldWidth);
            if (keyValueList.typeName == "ItemObject") newObject = Object(null, typeof(ItemObject), fieldWidth);
            if (keyValueList.typeName == "ItemAttribute") newObject = Object(null, typeof(ItemAttribute), fieldWidth);
            if (keyValueList.typeName == "Quest") newObject = Object(null, typeof(Quest), fieldWidth);
            if (keyValueList.typeName == "QuestCondition") newObject = Object(null, typeof(QuestCondition), fieldWidth);
            if (keyValueList.typeName == "QuestReward") newObject = Object(null, typeof(QuestReward), fieldWidth);
            if (keyValueList.typeName == "Condition") newObject = Object(null, typeof(Condition), fieldWidth);
            if (keyValueList.typeName == "LookupTable") newObject = Object(null, typeof(LookupTable), fieldWidth);
            if (keyValueList.typeName == "LootBox") newObject = Object(null, typeof(LootBox), fieldWidth);
            if (keyValueList.typeName == "LootItems") newObject = Object(null, typeof(LootItems), fieldWidth);
            if (keyValueList.typeName == "Texture2D") newObject = Object(null, typeof(Texture2D), fieldWidth);
            if (keyValueList.typeName == "Sprite") newObject = Object(null, typeof(Sprite), fieldWidth);
            if (keyValueList.typeName == "AnimationClip") newObject = Object(null, typeof(AnimationClip), fieldWidth);
            if (keyValueList.typeName == "AudioClip") newObject = Object(null, typeof(AudioClip), fieldWidth);
            if (keyValueList.typeName == "GameObject") newObject = Object(null, typeof(GameObject), fieldWidth);

            if (newObject != null)
            {
                if (keyValueList.typeName == "Stat") keyValueList.AddValue((Stat)newObject);
                if (keyValueList.typeName == "ItemObject") keyValueList.AddValue((ItemObject)newObject);
                if (keyValueList.typeName == "ItemAttribute") keyValueList.AddValue((ItemAttribute)newObject);
                if (keyValueList.typeName == "Quest") keyValueList.AddValue((Quest)newObject);
                if (keyValueList.typeName == "QuestCondition") keyValueList.AddValue((QuestCondition)newObject);
                if (keyValueList.typeName == "QuestReward") keyValueList.AddValue((QuestReward)newObject);
                if (keyValueList.typeName == "Condition") keyValueList.AddValue((Condition)newObject);
                if (keyValueList.typeName == "LookupTable") keyValueList.AddValue((LookupTable)newObject);
                if (keyValueList.typeName == "LootBox") keyValueList.AddValue((LootBox)newObject);
                if (keyValueList.typeName == "LootItems") keyValueList.AddValue((LootItems)newObject);
                if (keyValueList.typeName == "Texture2D") keyValueList.AddValue((Texture2D)newObject);
                if (keyValueList.typeName == "Sprite") keyValueList.AddValue((Sprite)newObject);
                if (keyValueList.typeName == "AnimationClip") keyValueList.AddValue((AnimationClip)newObject);
                if (keyValueList.typeName == "AudioClip") keyValueList.AddValue((AudioClip)newObject);
                if (keyValueList.typeName == "GameObject") keyValueList.AddValue((GameObject)newObject);
                
                DebugConsoleMessage($"New {keyValueList.typeName} added: {newObject}");
                ExitGUI();
            }
            
            ResetColor();
        }

        private bool HandleBasics(KeyValueList keyValueList, KeyValue keyValue)
        {
            if (keyValueList.typeName != "Int"
                && keyValueList.typeName != "String"
                && keyValueList.typeName != "Float"
                && keyValueList.typeName != "Bool"
                && keyValueList.typeName != "Vector2"
                && keyValueList.typeName != "Vector3"
                && keyValueList.typeName != "Vector4"
                && keyValueList.typeName != "Color")
                return false;
            
            if (keyValueList.typeName == "String")
            {
                keyValueList.newValueString = TextField(keyValueList.newValueString, 150);
                if (Button("Add", 50))
                    keyValueList.AddValue(keyValueList.newValueString);
                
                DebugConsoleMessage($"New {keyValueList.typeName} added: {keyValueList.objects.Last()}");
                return true;
            }
            

            if (Button("Add Value", 100))
            {
                if (keyValueList.typeName == "Int")
                    keyValueList.AddValue(0);
                if (keyValueList.typeName == "Float")
                    keyValueList.AddValue(0f);
                if (keyValueList.typeName == "Bool")
                    keyValueList.AddValue(false);
                if (keyValueList.typeName == "Vector2")
                    keyValueList.AddValue(new Vector2());
                if (keyValueList.typeName == "Vector3")
                    keyValueList.AddValue(new Vector3());
                if (keyValueList.typeName == "Vector4")
                    keyValueList.AddValue(new Vector4());
                if (keyValueList.typeName == "Color")
                    keyValueList.AddValue(new Color(0.5f, 0.5f, 0.5f, 1f));
                
                DebugConsoleMessage($"New {keyValueList.typeName} added: {keyValueList.objects.Last()}");
            }

            return true;
        }

        private void ShowKeyValueListObjects(Type type, KeyValueList keyValueList, KeyValue keyValue)
        {
            var objectCount = keyValueList.objects.Count;
            var list = keyValueList.objects;
            //Label($"type is {keyValueList.typeName}");
            if (keyValueList.typeName == "Stat")
            {
                Label($"Stats: {keyValueList.objects.Count}");
            }
            for (var i = 0; i < keyValueList.objects.Count; i++)
            {
                StartRow();
                
                // Index & Field
                LabelGrey($"[{i}]", 25);
                ShowObjectField(type, keyValueList.objects[i], keyValueList, keyValue);
                
                // Move Up Button
                ColorsIf(CanMoveUp(i, objectCount)
                    , Color.white
                    , Color.black
                    , Color.black
                    , Color.grey);
                if (Button(symbolArrowUp, 25) && CanMoveUp(i, objectCount))
                    (list[i - 1], list[i]) = (list[i], list[i - 1]);
                
                // Move Down Button
                ColorsIf(CanMoveDown(i, list.Count)
                    , Color.white
                    , Color.black
                    , Color.black
                    , Color.grey);
                if (Button(symbolArrowDown, 25) && CanMoveDown(i, objectCount))
                    (list[i + 1], list[i]) = (list[i], list[i + 1]);
                ContentColor(Color.white);
                
                // Remove Button
                BackgroundColor(Color.red);
                if (Button(symbolX, 25))
                {
                    keyValueList.RemoveValue(keyValueList.objects[i]);
                    ExitGUI();
                }

                BackgroundColor(Color.white);
                EndRow();
            }
        }

        private void ShowObjectField(Type type, KeyValueObject keyValueObject, KeyValueList keyValueList, KeyValue keyValue)
        {
            if (keyValueList.typeName == "String")
            {
                var valueString = keyValueObject.Object<string>();
                var newValue = keyValue.stringAsTextBox
                    ? TextArea(valueString, fieldWidth, 50)
                    : TextField(valueString, fieldWidth);
                if (valueString != newValue)
                    keyValueObject.SetObject<string>(newValue, "", typeof(string).ToString());
                return;
            }
            
            if (keyValueList.typeName == "Int")
            {
                var valueInt = keyValueObject.Object<int>();
                var newValue = Int(valueInt, 50);
                if (valueInt != newValue)
                    keyValueObject.SetObject<int>(newValue, "", typeof(int).ToString());
                return;
            }
            
            if (keyValueList.typeName == "Float")
            {
                var valueFloat = keyValueObject.Object<float>();
                var newValue = Float(valueFloat, 50);
                if (valueFloat != newValue)
                    keyValueObject.SetObject<float>(newValue, "", typeof(float).ToString());
                return;
            }
            
            if (keyValueList.typeName == "Bool")
            {
                var valueBool = keyValueObject.Object<bool>();
                var newValue = Check(valueBool, 25);
                if (valueBool != newValue)  
                    keyValueObject.SetObject<bool>(newValue, "", typeof(bool).ToString());
                return;
            }
            
            if (keyValueList.typeName == "Color")
            {
                var valueColor = keyValueObject.Object<Color>();
                var newValue = ColorField(valueColor, 150);
                if (valueColor != newValue)  
                    keyValueObject.SetObject<Color>(newValue, "", typeof(Color).ToString());
                return;
            }
            
            if (keyValueList.typeName == "Vector4")
            {
                var valueVector4 = keyValueObject.Object<Vector4>();
                var newValue = Vector4Field(valueVector4, 200);
                if (valueVector4 != newValue)  
                    keyValueObject.SetObject<Vector4>(newValue, "", typeof(Vector4).ToString());
                return;
            }
            
            if (keyValueList.typeName == "Vector3")
            {
                var valueVector3 = keyValueObject.Object<Vector3>();
                var newValue = Vector3Field(valueVector3, 150);
                if (valueVector3 != newValue)  
                    keyValueObject.SetObject<Vector3>(newValue, "", typeof(Vector3).ToString());
                return;
            }
            
            if (keyValueList.typeName == "Vector2")
            {
                var valueVector2 = keyValueObject.Object<Vector2>();
                var newValue = Vector2Field(valueVector2, 100);
                if (valueVector2 != newValue)  
                    keyValueObject.SetObject<Vector2>(newValue, "", typeof(Vector2).ToString());
                return;
            }

            if (keyValueList.typeName == "GameObject")
            {
                var valueGameObject = keyValueObject.Object<GameObject>();
                var newValue = Object(valueGameObject, typeof(GameObject), fieldWidth);
                if (valueGameObject != newValue)  
                    keyValueObject.SetObject<GameObject>(newValue, "", typeof(GameObject).ToString());
                return;
            }

            if (keyValueList.typeName == "Sprite")
            {
                var valueObject = keyValueObject.Object<Sprite>();
                if (valueObject != null)
                {
                    var tex = AssetPreview.GetAssetPreview(valueObject);
                    Texture2D scaledTex = ScaleTexture(tex, 50);
                    if (scaledTex != null)  
                        GUILayout.Box(scaledTex, GUILayout.Width(scaledTex.width), GUILayout.Height(scaledTex.height));
                }
                var newValue = Object(valueObject, typeof(Sprite), fieldWidth);
                if (valueObject != newValue)  
                    keyValueObject.SetObject<Sprite>(newValue, "", typeof(Sprite).ToString());
                return;
            }
            
            if (keyValueList.typeName == "Texture2D")
            {
                var valueObject = keyValueObject.Object<Texture2D>();
                if (valueObject != null)
                {
                    var tex = AssetPreview.GetAssetPreview(valueObject);
                    Texture2D scaledTex = ScaleTexture(tex, 50);
                    if (scaledTex != null)
                        GUILayout.Box(scaledTex, GUILayout.Width(scaledTex.width), GUILayout.Height(scaledTex.height));
                }
                var newValue = Object(valueObject, typeof(Texture2D), fieldWidth);
                if (valueObject != newValue)
                {
                    keyValueObject.SetObject<Texture2D>(newValue, "", typeof(Texture2D).ToString());
                }
                return;
            }
            
            if (keyValueList.typeName == "AudioClip")
            {
                var valueObject = keyValueObject.Object<AudioClip>();
                var newValue = Object(valueObject, typeof(AudioClip), fieldWidth);
                if (valueObject != newValue)  
                    keyValueObject.SetObject<AudioClip>(newValue, "", typeof(AudioClip).ToString());
                return;
            }
            
            if (keyValueList.typeName == "AnimationClip")
            {
                var valueObject = keyValueObject.Object<AnimationClip>();
                var newValue = Object(valueObject, typeof(AnimationClip), fieldWidth);
                if (valueObject != newValue)  
                    keyValueObject.SetObject<AnimationClip>(newValue, "", typeof(AnimationClip).ToString());
                return;
            }
            
            if (keyValueList.typeName == "Stat")
            {
                var valueObject = keyValueObject.Object<Stat>();
                var newValue = Object(valueObject, typeof(Stat), fieldWidth);
                if (valueObject != newValue)  
                    keyValueObject.SetObject<Stat>(newValue, "", typeof(Stat).ToString());
                return;
            }
            
            if (keyValueList.typeName == "Condition")
            {
                var valueObject = keyValueObject.Object<Condition>();
                var newValue = Object(valueObject, typeof(Condition), fieldWidth);
                if (valueObject != newValue)  
                    keyValueObject.SetObject<Condition>(newValue, "", typeof(Condition).ToString());
                return;
            }
            
            if (keyValueList.typeName == "Quest")
            {
                var valueObject = keyValueObject.Object<Quest>();
                var newValue = Object(valueObject, typeof(Quest), fieldWidth);
                if (valueObject != newValue)  
                    keyValueObject.SetObject<Quest>(newValue, "", typeof(Quest).ToString());
                return;
            }
            
            if (keyValueList.typeName == "QuestCondition")
            {
                var valueObject = keyValueObject.Object<QuestCondition>();
                var newValue = Object(valueObject, typeof(QuestCondition), fieldWidth);
                if (valueObject != newValue)  
                    keyValueObject.SetObject<QuestCondition>(newValue, "", typeof(QuestCondition).ToString());
                return;
            }
            
            if (keyValueList.typeName == "QuestReward")
            {
                var valueObject = keyValueObject.Object<QuestReward>();
                var newValue = Object(valueObject, typeof(QuestReward), fieldWidth);
                if (valueObject != newValue)  
                    keyValueObject.SetObject<QuestReward>(newValue, "", typeof(QuestReward).ToString());
                return;
            }
            
            if (keyValueList.typeName == "ItemObject")
            {
                var valueObject = keyValueObject.Object<ItemObject>();
                var newValue = Object(valueObject, typeof(ItemObject), fieldWidth);
                if (valueObject != newValue)  
                    keyValueObject.SetObject<ItemObject>(newValue, "", typeof(ItemObject).ToString());
                return;
            }
            
            if (keyValueList.typeName == "ItemAttribute")
            {
                var valueObject = keyValueObject.Object<ItemAttribute>();
                var newValue = Object(valueObject, typeof(ItemAttribute), fieldWidth);
                if (valueObject != newValue)  
                    keyValueObject.SetObject<ItemAttribute>(newValue, "", typeof(ItemAttribute).ToString());
                return;
            }
            
            if (keyValueList.typeName == "LookupTable")
            {
                var valueObject = keyValueObject.Object<LookupTable>();
                var newValue = Object(valueObject, typeof(LookupTable), fieldWidth); 
                
                if (valueObject != newValue)
                {
                    Debug.Log($"New LookupTable added: {newValue.name}");
                    keyValueObject.SetObject<LookupTable>(newValue, "", typeof(LookupTable).ToString());
                }
                return;
            }
            
            if (keyValueList.typeName == "LootBox")
            {
                var valueObject = keyValueObject.Object<LootBox>();
                var newValue = Object(valueObject, typeof(LootBox), fieldWidth);
                if (valueObject != newValue)  
                    keyValueObject.SetObject<LootBox>(newValue, "", typeof(LootBox).ToString());
                return;
            }
            
            if (keyValueList.typeName == "LootItems")
            {
                var valueObject = keyValueObject.Object<LootItems>();
                var newValue = Object(valueObject, typeof(LootItems), fieldWidth);
                if (valueObject != newValue)  
                    keyValueObject.SetObject<LootItems>(newValue, "", typeof(LootItems).ToString());
            }
        }

        public void DrawStructure(Dictionaries dictionaries, string newClassName, string objectType)
        {
            SetKeyValueDrawer();
            if (_keyValueDrawer.AllDictionariesIsNull())
                DoCaching(objectType);
            
            className = newClassName;
            if (className == "Quest")
                DrawQuestStructure(dictionaries, objectType);
            
            if (className == "Item Object")
                DrawItemObjectStructure(dictionaries, objectType);
            
            if (className == "Item Attribute")
                DrawItemAttributeStructure(dictionaries, objectType);
            
            if (className == "Stats")
                DrawStatsStructure(dictionaries, objectType);
            
            if (className == "Condition")
                DrawConditionsStructure(dictionaries, objectType);
            
            if (className == "Mastery Levels")
                DrawMasteryLevelsStructure(dictionaries, objectType);
        }
        
        void ShowCommonDictionary(Dictionaries dictionaries)
        {
            //dictionaries.CheckForNullLists();
            if (dictionaries == null)
            {
                Debug.LogWarning("Dictionary is null");
                return;
            }
            if (dictionaries.keyValues == null)
            {
                Debug.LogWarning("Dictionary.keyValues is null");
                return;
            }

            foreach (var keyValue in dictionaries.keyValues)
            {
                 
            }

            for (int i = 0; i < dictionaries.keyValues.Count; i++)
            {
                KeyValue keyValue = dictionaries.keyValues[i];
                Space();
                StartVerticalBox();
                StartRow();
                
                /*
                AllValueColors(keyValue.showAllValueTypes);
                if (Button(keyValue.showAllValueTypes ? symbolDash : symbolBulletClosed, 25))
                {
                    keyValue.showAllValueTypes = !keyValue.showAllValueTypes;
                }
                BackgroundColor(Color.white);
                ContentColor(Color.white);
                */

                
                Label(keyValue.key, 180);

                StartVertical();

                // DRAW KEY VALUES
                keyValue.Draw(_keyValueDrawer);
                
                EndVerticalBox();
               
                EndRow();
                EndVerticalBox();
                
            }
        }


        void ShowCommonDictionaryStructure(Dictionaries dictionaries, string objectType, Dictionaries[] thisObjectDictionaries, Action content)
        {
            if (string.IsNullOrWhiteSpace(ObjectType) || ObjectType != objectType || _keyValueDrawer.AllDictionariesIsNull())
                DoCaching(objectType);

            BackgroundColor(Color.yellow);
            StartVerticalBox();
            Label("Add Dictionary Key (v4)");
            StartRow();
            
            SetString(className + " Add Dictionary Key", TextField(GetString(className + " Add Dictionary Key"), 150));
            if (Button("Add to all " + objectType, 200))
            {
                content?.Invoke(); // THIS IS THE CUSTOM CODE
                DoCaching(className);
                SetAllDirty();
            }
            EndRow();
            EndVerticalBox();
            BackgroundColor(Color.white);
            Space();

            for (var i = 0; i < dictionaries.keyValues.Count; i++)
            {
                var keyValue = dictionaries.keyValues[i];

                StartRow();
                
                BackgroundColor(Color.white);
                ContentColor(Color.white);
                
                if (Button(symbolArrowUp, 25))
                {
                    if (className == "Item Object")
                    {
                        foreach (var itemObject in GameModuleObjectsOfType<ItemObject>(ObjectType))
                            MoveItem(itemObject.dictionaries.keyValues, i, -1);
                    }
                    if (className == "Item Attribute")
                    {
                        foreach (var itemAttribute in GameModuleObjectsOfType<ItemAttribute>(ObjectType))
                            MoveItem(itemAttribute.dictionaries.keyValues, i, -1);
                    }
                    if (className == "Stat")
                    {
                        foreach (var stat in GameModuleObjectsOfType<Stat>(ObjectType))
                            MoveItem(stat.dictionaries.keyValues, i, -1);
                    }
                    
                    if (className == "Condition")
                    {
                        foreach (var conditions in GameModuleObjectsOfType<Condition>(ObjectType))
                            MoveItem(conditions.dictionaries.keyValues, i, -1);
                    }
                    if (className == "Mastery Level")
                    {
                        foreach (var masteryLevels in GameModuleObjectsOfType<MasteryLevels>(ObjectType))
                            MoveItem(masteryLevels.dictionaries.keyValues, i, -1);
                    }
                }
                
                if (Button(symbolArrowDown, 25))
                {
                    Debug.Log($"className is {className}");
                    if (className == "Item Object")
                    {
                        foreach (var itemObject in GameModuleObjectsOfType<ItemObject>(ObjectType))
                            MoveItem(itemObject.dictionaries.keyValues, i, 1);
                    }
                    if (className == "Item Attribute")
                    {
                        foreach (var itemAttribute in GameModuleObjectsOfType<ItemAttribute>(ObjectType))
                            MoveItem(itemAttribute.dictionaries.keyValues, i, 1);
                    }
                    if (className == "Stat")
                    {
                        foreach (var stat in GameModuleObjectsOfType<Stat>(ObjectType))
                            MoveItem(stat.dictionaries.keyValues, i, 1);
                    }
                    
                    if (className == "Condition")
                    {
                        foreach (var condition in GameModuleObjectsOfType<Condition>(ObjectType))
                            MoveItem(condition.dictionaries.keyValues, i, 1);
                    }
                    if (className == "Mastery Level")
                    {
                        foreach (var masteryLevels in GameModuleObjectsOfType<MasteryLevels>(ObjectType))
                            MoveItem(masteryLevels.dictionaries.keyValues, i, 1);
                    }
                }
                
                var tempKeyValue = keyValue.key;
                keyValue.key = DelayedText(keyValue.key, 180);
                if (tempKeyValue != keyValue.key)
                {
                    if (dictionaries.keyValues.Count(x => x.key == keyValue.key) > 1)
                    {
                        Debug.LogWarning("There is already a key named " + keyValue.key);
                        keyValue.key = tempKeyValue;
                    }
                    else
                    {
                        foreach (Dictionaries d in thisObjectDictionaries)
                        {
                            d.keyValues[i].key = keyValue.key;
                        }
                    }
                }
                
                BackgroundColor(Color.red);
                if (Button(symbolX, 25))
                {
                    Undo.RecordObjects(GameModuleObjects<Stat>().Cast<Object>().ToArray(), "Undo Delete" );
                    Undo.RecordObjects(GameModuleObjects<ItemObject>().Cast<Object>().ToArray(), "Undo Delete" );
                    Undo.RecordObjects(GameModuleObjects<ItemAttribute>().Cast<Object>().ToArray(), "Undo Delete" );
                    Undo.RecordObjects(GameModuleObjects<Condition>().Cast<Object>().ToArray(), "Undo Delete" );
                    Undo.RecordObjects(GameModuleObjects<MasteryLevels>().Cast<Object>().ToArray(), "Undo Delete" );
                    foreach (var d in thisObjectDictionaries)
                    {
                        d.keyValues.RemoveAt(i);
                    }
                    ExitGUI();
                }
                ResetColor();
                
                var tempStringAsTextBox = keyValue.stringAsTextBox;
                keyValue.stringAsTextBox = LeftCheck("Show String as Text Box", keyValue.stringAsTextBox, 200);
                if (tempStringAsTextBox != keyValue.stringAsTextBox)
                {
                    foreach (Dictionaries d in thisObjectDictionaries)
                        d.keyValues[i].stringAsTextBox = keyValue.stringAsTextBox;
                }
                
                EndRow();
            }
        }

        private void SetAllDirty()
        {
            foreach (var itemObject in GameModuleObjects<ItemObject>(true))
                EditorUtility.SetDirty(itemObject);
            
            foreach (var itemAttribute in GameModuleObjects<ItemAttribute>(true))
                EditorUtility.SetDirty(itemAttribute);
            
            foreach (var condition in GameModuleObjects<Condition>(true))
                EditorUtility.SetDirty(condition);
            
            foreach (var statsAndSkills in GameModuleObjects<Stat>(true))
                EditorUtility.SetDirty(statsAndSkills);
        }

        private void DoCaching(string objectType)
        {
            
            //if (string.IsNullOrWhiteSpace(objectType)) return;
            if (_keyValueDrawer == null) return;
            _objectType = objectType;
            Cache(ObjectType);
            if (className == "Item Object")
            {
                if (GameModuleObjectDictionaries<ItemObject>(ObjectType) != null)
                {
                    if (GameModuleObjectDictionaries<ItemObject>(ObjectType) != null)
                        _keyValueDrawer.SetAllDictionaries(GameModuleObjectDictionaries<ItemObject>(ObjectType));
                }
            }
                    
            if (className == "Item Attribute")
            {
                if (GameModuleObjectDictionaries<ItemAttribute>(ObjectType) != null)
                {
                    if (GameModuleObjectDictionaries<ItemAttribute>(ObjectType) != null)
                        _keyValueDrawer.SetAllDictionaries(GameModuleObjectDictionaries<ItemAttribute>(ObjectType));
                }
            }
                    
            if (className == "Stat")
            {
                if (GameModuleObjectDictionaries<Stat>(ObjectType) != null)
                {
                    if (GameModuleObjectDictionaries<Stat>(ObjectType) != null)
                        _keyValueDrawer.SetAllDictionaries(GameModuleObjectDictionaries<Stat>(ObjectType));
                }
            }
            
            if (className == "Condition")
            {
                if (GameModuleObjectDictionaries<Condition>(ObjectType) != null)
                {
                    if (GameModuleObjectDictionaries<Condition>(ObjectType) != null)
                        _keyValueDrawer.SetAllDictionaries(GameModuleObjectDictionaries<Condition>(ObjectType));
                }
            }
                    
            if (className == "Mastery Level")
            {
                if (GameModuleObjectDictionaries<MasteryLevels>(ObjectType) != null)
                {
                    if (GameModuleObjectDictionaries<MasteryLevels>(ObjectType) != null)
                        _keyValueDrawer.SetAllDictionaries(GameModuleObjectDictionaries<MasteryLevels>(ObjectType));
                }
            }
        }
        
        public virtual KeyValue TryGetKeyValue(string keyName, Dictionaries dictionaries)
        {
            return dictionaries.keyValues.FirstOrDefault(x => x.key == keyName);
        }
        
        private void AllValueColors(bool isTrue)
        {
            BackgroundColor(isTrue ? Color.white : Color.black);
            ContentColor(isTrue ? Color.white : Color.grey);
        }
        
        
        
        // ----------------------------------------------------------------------------------------
        // ITEM OBJECT
        // ----------------------------------------------------------------------------------------
        public void DrawItemObject(Dictionaries dictionaries, string objectName, string objectType)
        {
            className = "Item Object";
            ShowCommonDictionary(dictionaries); 
        }
        
        public void DrawItemObjectStructure(Dictionaries dictionaries, string objectType)
        {
            ShowCommonDictionaryStructure(dictionaries, objectType, GameModuleObjectDictionaries<ItemObject>(objectType),
                () =>
                {
                    AddDictionaryKeyItemObjectStructure(GetString(className + " Add Dictionary Key"), objectType);
                });
            
            
        }

        public void AddDictionaryKeyItemObject(Dictionaries dictionaries, string keyName, string objectName, string objectType, bool addToAllOfType = false)
        {
            if (addToAllOfType)
            {
                foreach (var itemObject in GameModuleObjects<ItemObject>().Where(x => x.objectType == objectType))
                    itemObject.AddDictionaryKeyToSo(keyName);

                return;
            }

            if (TryGetKeyValue(keyName, dictionaries) != null)
            {
                LogWarning(objectName + " dictionary already has key " + keyName);
                return;
            }
            
            var newKeyValue = new KeyValue();
            newKeyValue.key = keyName;
            dictionaries.keyValues.Add(newKeyValue);
        }
        
        public void AddDictionaryKeyItemObjectStructure(string keyName, string objectType)
        {
            foreach (var itemObject in GameModuleObjectsOfType<ItemObject>(ObjectType))
                itemObject.AddDictionaryKeyToSo(keyName);
        }

        public void AddDictionaryKeyItemAttributeStructure(string keyName, string objectType)
        {
            Debug.Log("AND HERE");
            foreach (ItemAttribute itemAttribute in GameModuleObjectsOfType<ItemAttribute>(ObjectType))
                itemAttribute.AddDictionaryKeyToSo(keyName);
        }
        
        
        // ----------------------------------------------------------------------------------------
        // ITEM ATTRIBUTE
        // ----------------------------------------------------------------------------------------
        public void DrawItemAttribute(Dictionaries dictionaries, string objectName, string objectType)
        {
            className = "Item Attribute";
            ShowCommonDictionary(dictionaries);
        }

        public void ResetCache(string newClassName, string objectType)
        {
            className = newClassName;
            DoCaching(objectType);
        }

        public void DrawItemAttributeStructure(Dictionaries dictionaries, string objectType)
        {
            ShowCommonDictionaryStructure(dictionaries, objectType, GameModuleObjectDictionaries<ItemAttribute>(objectType),
                () => { AddDictionaryKeyItemAttributeStructure(dictionaries, GetString(className + " Add Dictionary Key"), objectType); });
        }
        
        public void AddDictionaryKeyItemAttribute(Dictionaries dictionaries, string keyName, string objectName, string objectType, bool addToAllOfType = false)
        {
            if (addToAllOfType)
            {
                foreach (ItemAttribute itemAttribute in GameModuleObjects<ItemAttribute>().Where(x => x.objectType == objectType))
                    itemAttribute.AddDictionaryKeyToSo(keyName);

                return;
            }

            if (TryGetKeyValue(keyName, dictionaries) != null)
            {
                LogWarning(objectName + " dictionary already has key " + keyName);
                return;
            }
            
            KeyValue newKeyValue = new KeyValue();
            newKeyValue.key = keyName;
            dictionaries.keyValues.Add(newKeyValue);
        }
        
        public void AddDictionaryKeyItemAttributeStructure(Dictionaries dictionaries, string keyName, string objectType)
        {
            foreach (ItemAttribute itemAttribute in GameModuleObjects<ItemAttribute>().Where(x => x.objectType == objectType))
                itemAttribute.AddDictionaryKeyToSo(keyName);
        }
        
        // ----------------------------------------------------------------------------------------
        // QUEST
        // ----------------------------------------------------------------------------------------
        public void DrawQuest(Dictionaries dictionaries, string objectName, string objectType)
        {
            className = "Quest";
            ShowCommonDictionary(dictionaries);
        }

        public void DrawQuestStructure(Dictionaries dictionaries, string objectType)
        {
            ShowCommonDictionaryStructure(dictionaries, objectType, GameModuleObjectDictionaries<Quest>(objectType),
                () => { AddDictionaryKeyQuestStructure(dictionaries, GetString(className + " Add Dictionary Key"), objectType); });
        }

        public void AddDictionaryKeyQuest(Dictionaries dictionaries, string keyName, string objectName, string objectType, bool addToAllOfType = false)
        {
            if (addToAllOfType)
            {
                foreach (Quest quest in GameModuleObjects<Quest>().Where(x => x.objectType == objectType))
                    quest.AddDictionaryKeyToSo(keyName);

                return;
            }

            if (TryGetKeyValue(keyName, dictionaries) != null)
            {
                LogWarning(objectName + " dictionary already has key " + keyName);
                return;
            }
            
            KeyValue newKeyValue = new KeyValue();
            newKeyValue.key = keyName;
            dictionaries.keyValues.Add(newKeyValue);
        }
        
        public void AddDictionaryKeyQuestStructure(Dictionaries dictionaries, string keyName, string objectType)
        {
            foreach (Quest quest in GameModuleObjects<Quest>().Where(x => x.objectType == objectType))
                quest.AddDictionaryKeyToSo(keyName);
        }

        // ----------------------------------------------------------------------------------------
        // CONDITION
        // ----------------------------------------------------------------------------------------
        public void DrawCondition(Dictionaries dictionaries, string objectName, string objectType)
        {
            className = "Conditions";
            ShowCommonDictionary(dictionaries);
        }

        public void DrawConditionStructure(Dictionaries dictionaries, string objectType)
        {
            ShowCommonDictionaryStructure(dictionaries, objectType, GameModuleObjectDictionaries<Condition>(objectType),
                () => { AddDictionaryKeyConditionsStructure(dictionaries, GetString(className + " Add Dictionary Key"), objectType); });
        }

        public void AddDictionaryKeyCondition(Dictionaries dictionaries, string keyName, string objectName, string objectType, bool addToAllOfType = false)
        {
            if (addToAllOfType)
            {
                foreach (Condition condition in GameModuleObjects<Condition>().Where(x => x.objectType == objectType))
                    condition.AddDictionaryKeyToSo(keyName);

                return;
            }

            if (TryGetKeyValue(keyName, dictionaries) != null)
            {
                LogWarning(objectName + " dictionary already has key " + keyName);
                return;
            }
            
            KeyValue newKeyValue = new KeyValue();
            newKeyValue.key = keyName;
            dictionaries.keyValues.Add(newKeyValue);
        }
        
        public void AddDictionaryKeyConditionStructure(Dictionaries dictionaries, string keyName, string objectType)
        {
            foreach (Condition condition in GameModuleObjects<Condition>().Where(x => x.objectType == objectType))
                condition.AddDictionaryKeyToSo(keyName);
        }

        // ----------------------------------------------------------------------------------------
        // Stats
        // ----------------------------------------------------------------------------------------
        public void DrawStatsAndSkills(Dictionaries dictionaries, string objectName, string objectType)
        {
            //className = "Stats";
            ShowCommonDictionary(dictionaries);
        }
        
        public void DrawMasteryLevels(Dictionaries dictionaries, string masteryLevelName, string objectName)
        {
            //className = "Stats";
            ShowCommonDictionary(dictionaries);
        }
        
        public void DrawStatsStructure(Dictionaries dictionaries, string objectType)
        {
            ShowCommonDictionaryStructure(dictionaries, objectType, GameModuleObjectDictionaries<Stat>(objectType),
                () =>
                {
                    AddDictionaryKeyStatsStructure(dictionaries, GetString(className + " Add Dictionary Key"),
                        objectType);
                });
        }
        
        public void DrawConditionsStructure(Dictionaries dictionaries, string objectType)
        {
            ShowCommonDictionaryStructure(dictionaries, objectType, GameModuleObjectDictionaries<Condition>(objectType),
                () =>
                {
                    AddDictionaryKeyConditionsStructure(dictionaries, GetString(className + " Add Dictionary Key"),
                        objectType);
                });
        }

        public void DrawMasteryLevelsStructure(Dictionaries dictionaries, string objectType)
        {
            ShowCommonDictionaryStructure(dictionaries, objectType, GameModuleObjectDictionaries<MasteryLevels>(objectType),
                () =>
                {
                    AddDictionaryKeyMasteryLevelsStructure(dictionaries, GetString(className + " Add Dictionary Key"),
                        objectType);
                });
        }

        public void AddDictionaryKeyStatsAndSkills(Dictionaries dictionaries, string keyName, string objectName, string objectType, bool addToAllOfType = false)
        {
            if (addToAllOfType)
            {
                foreach (Stat statsAndSkills in GameModuleObjects<Stat>().Where(x => x.objectType == objectType))
                    statsAndSkills.AddDictionaryKeyToSo(keyName);

                return;
            }

            if (TryGetKeyValue(keyName, dictionaries) != null)
            {
                LogWarning(objectName + " dictionary already has key " + keyName);
                return;
            }
            
            KeyValue newKeyValue = new KeyValue();
            newKeyValue.key = keyName;
            dictionaries.keyValues.Add(newKeyValue);
        }
        
        public void AddDictionaryKeyStatsStructure(Dictionaries dictionaries, string keyName, string objectType)
        {
            foreach (Stat stat in GameModuleObjects<Stat>().Where(x => x.objectType == objectType))
                stat.AddDictionaryKeyToSo(keyName);
        }
        
        public void AddDictionaryKeyConditionsStructure(Dictionaries dictionaries, string keyName, string objectType)
        {
            foreach (Condition condition in GameModuleObjects<Condition>().Where(x => x.objectType == objectType))
                condition.AddDictionaryKeyToSo(keyName);
        }
        
        public void AddDictionaryKeyMasteryLevelsStructure(Dictionaries dictionaries, string keyName, string objectType)
        {
            Debug.Log($"AddDictionaryKeyMasteryLevelsStructure {keyName} {objectType}");
            foreach (Dictionaries d in GameModuleObjectDictionaries<MasteryLevels>(objectType))
            {
                if (d.HasKeyValue(keyName))
                {
                    Debug.Log("Dictionary already has key " + keyName);
                    continue;
                }

                Debug.Log($"Add new key {keyName} to {d.name}");
                d.AddNewKeyValue(keyName);
            }
        }
    }
}
