using System;
using System.Linq;
using UnityEngine;
using static InfinityPBR.Modules.EditorUtilities;
using static InfinityPBR.Modules.GameModuleUtilities;

namespace InfinityPBR.Modules
{ 
    class KeyValueDrawerEditor : KeyValueDrawer
    {
        private readonly int fieldWidth;
        public Dictionaries[] allDictionariesStructure;

        public ItemObject copyPasteItemObject => GetCopyPasteItemObject();

        private ItemObject GetCopyPasteItemObject()
        {
            if (!HasKey("Item Dictionaries"))
                return null;
            _copyPasteItemObject = GameModuleObjects<ItemObject>()
                .ToList()
                .FirstOrDefault(x => x.objectName == GetString("Item Dictionaries"));
            return _copyPasteItemObject;
        }

        private ItemObject _copyPasteItemObject;
        private Stat copyPasteStat;
        
         
        
        public KeyValueDrawerEditor(int fieldWidth)
        {
            this.fieldWidth = fieldWidth;
        }
        
        public void Draw(KeyValue keyValue, bool structureOnly = false)
        {
            
        }
        
        
        
        
        
        public void Draw_v3(KeyValue keyValue, bool structureOnly = false)
        {
            /*if (allDictionariesStructure == null)
            {
                Debug.LogWarning("allDictionariesStructure was null");
                return;
            } */

            //Debug.Log("Redo");
            
            DrawFloat(keyValue, structureOnly);
            DrawString(keyValue, structureOnly);
            DrawInt(keyValue, structureOnly);
            DrawBool(keyValue, structureOnly);
            
            DrawAnimation(keyValue, structureOnly);
            DrawTexture2D(keyValue, structureOnly);
            DrawSprite(keyValue, structureOnly);
            DrawAudioClip(keyValue, structureOnly);
            DrawPrefab(keyValue, structureOnly);
            DrawColor(keyValue, structureOnly);
            DrawVector3(keyValue, structureOnly);
            DrawVector2(keyValue, structureOnly);
            
            DrawStat(keyValue, structureOnly);
            DrawItemObject(keyValue, structureOnly);
            DrawItemObjectType(keyValue, structureOnly);
            DrawItemAttribute(keyValue, structureOnly);
            DrawItemAttributeType(keyValue, structureOnly);
            DrawCondition(keyValue, structureOnly);
            DrawConditionType(keyValue, structureOnly);
            
            
        }
        
        public void SetAllDictionaries(Dictionaries[] allDictionaries)
        {
            allDictionariesStructure = allDictionaries;
        }

        public bool AllDictionariesIsNull()
        {
            return allDictionariesStructure == null;
        }

        private bool CanMoveUp(int index, int total)
        {
            if (index == 0) return false;

            return true;
        }
        
        private bool CanMoveDown(int index, int total)
        {
            if (index == total - 1) return false;

            return true;
        }

        public void DrawFloat(KeyValue keyValue, bool structureOnly = false)
        {
            keyValue.showFloat = ShowDictionaryValue(keyValue, keyValue.showFloat, "Float", structureOnly, 
                () => { StartVertical();
                    Label("Float", true);
                    for (int i = 0; i < keyValue.valuesFloat.Count; i++)
                    {
                        StartRow();
                        LabelGrey($"[{i}]", 25);
                        keyValue.valuesFloat[i] = Float(keyValue.valuesFloat[i], fieldWidth);

                        var list = keyValue.valuesFloat;
                        BackgroundColor(CanMoveUp(i, list.Count) ? Color.white : Color.black);
                        ContentColor(CanMoveUp(i, list.Count) ? Color.black : Color.grey);
                        if (Button(symbolArrowUp, 25))
                        {
                            if (CanMoveUp(i, list.Count))
                            {
                                var temp = list[i - 1];
                            list[i - 1] = list[i];
                            list[i] = temp;
                            }
                        }
                        BackgroundColor(CanMoveDown(i, list.Count) ? Color.white : Color.black);
                        ContentColor(CanMoveDown(i, list.Count) ? Color.black : Color.grey);
                        if (Button(symbolArrowDown, 25))
                        {if (CanMoveDown(i, list.Count))
                            {
                                var temp = list[i + 1];
                                list[i + 1] = list[i];
                                list[i] = temp;
                            }}
                        ContentColor(Color.white);
                        
                        BackgroundColor(Color.red);
                        if (i != 0 && Button(symbolX, 25))
                        {
                            keyValue.valuesFloat.RemoveAt(i);
                            ExitGUI();
                        }

                        BackgroundColor(Color.green);
                        if (i == keyValue.valuesFloat.Count - 1 && Button("+", 25))
                            keyValue.valuesFloat.Add(default);
                        
                        BackgroundColor(Color.white);
                        EndRow();
                    }
                    EndVertical(); },
                () => {  if (copyPasteItemObject)
                    keyValue.valuesFloat = copyPasteItemObject.dictionaries.Key(keyValue.key).valuesFloat; });
            if (!structureOnly) return;
           // Debug.Log($"allDictionariesStructure count {allDictionariesStructure.Length}");
            foreach (Dictionaries d in allDictionariesStructure)
            {
                // BUG? If there's a bug, then we could fix it here by adding hte key/value if it's missing.
                d.Key(keyValue.key).showFloat = keyValue.showFloat;
            }
        }

        
        public void DrawInt(KeyValue keyValue, bool structureOnly = false)
        {
            
            keyValue.showInt = ShowDictionaryValue(keyValue, keyValue.showInt, "Int", structureOnly,
                () =>
                {
                    StartVertical();
                    Label("Int", true);
                    for (int i = 0; i < keyValue.valuesInt.Count; i++)
                    {
                        StartRow();
                        LabelGrey($"[{i}]", 25);
                        keyValue.valuesInt[i] = Int(keyValue.valuesInt[i], fieldWidth);
                        
                        var list = keyValue.valuesInt;
                        BackgroundColor(CanMoveUp(i, list.Count) ? Color.white : Color.black);
                        ContentColor(CanMoveUp(i, list.Count) ? Color.black : Color.grey);
                        if (Button(symbolArrowUp, 25))
                        {
                            if (CanMoveUp(i, list.Count))
                            {
                                var temp = list[i - 1];
                            list[i - 1] = list[i];
                            list[i] = temp;
                            }
                        }
                        BackgroundColor(CanMoveDown(i, list.Count) ? Color.white : Color.black);
                        ContentColor(CanMoveDown(i, list.Count) ? Color.black : Color.grey);
                        if (Button(symbolArrowDown, 25))
                        {if (CanMoveDown(i, list.Count))
                            {
                                var temp = list[i + 1];
                                list[i + 1] = list[i];
                                list[i] = temp;
                            }}
                        ContentColor(Color.white);
                        BackgroundColor(Color.red);
                        if (i != 0 && Button(symbolX, 25))
                        {
                            keyValue.valuesInt.RemoveAt(i);
                            ExitGUI();
                        }

                        BackgroundColor(Color.green);
                        if (i == keyValue.valuesInt.Count - 1 && Button("+", 25))
                            keyValue.valuesInt.Add(default);
                        
                        BackgroundColor(Color.white);
                        EndRow();
                    }
                    EndVertical();
                },
                
                () => {  if (copyPasteItemObject)
                    keyValue.valuesInt = copyPasteItemObject.dictionaries.Key(keyValue.key).valuesInt; });
            if (!structureOnly) return;
            foreach (Dictionaries d in allDictionariesStructure)
                d.Key(keyValue.key).showInt = keyValue.showInt;
        }

        public void DrawBool(KeyValue keyValue, bool structureOnly = false)
        {
            keyValue.showBool = ShowDictionaryValue(keyValue, keyValue.showBool, "Bool", structureOnly, 
                () => { StartVertical();
                    Label("Bool", true);
                    for (int i = 0; i < keyValue.valuesBool.Count; i++)
                    {
                        StartRow();
                        LabelGrey($"[{i}]", 25);
                        keyValue.valuesBool[i] = Check(keyValue.valuesBool[i], fieldWidth);
                        
                        var list = keyValue.valuesBool;
                        BackgroundColor(CanMoveUp(i, list.Count) ? Color.white : Color.black);
                        ContentColor(CanMoveUp(i, list.Count) ? Color.black : Color.grey);
                        if (Button(symbolArrowUp, 25))
                        {
                            if (CanMoveUp(i, list.Count))
                            {
                                var temp = list[i - 1];
                            list[i - 1] = list[i];
                            list[i] = temp;
                            }
                        }
                        BackgroundColor(CanMoveDown(i, list.Count) ? Color.white : Color.black);
                        ContentColor(CanMoveDown(i, list.Count) ? Color.black : Color.grey);
                        if (Button(symbolArrowDown, 25))
                        {if (CanMoveDown(i, list.Count))
                            {
                                var temp = list[i + 1];
                                list[i + 1] = list[i];
                                list[i] = temp;
                            }}
                        ContentColor(Color.white);
                        BackgroundColor(Color.red);
                        if (i != 0 && Button(symbolX, 25))
                        {
                            keyValue.valuesBool.RemoveAt(i);
                            ExitGUI();
                        }

                        BackgroundColor(Color.green);
                        if (i == keyValue.valuesBool.Count - 1 && Button("+", 25))
                            keyValue.valuesBool.Add(default);
                        
                        BackgroundColor(Color.white);
                        EndRow();
                    }
                    EndVertical(); },
                () => {  if (copyPasteItemObject)
                    keyValue.valuesBool = copyPasteItemObject.dictionaries.Key(keyValue.key).valuesBool; });
            if (!structureOnly) return;
            foreach (Dictionaries d in allDictionariesStructure)
            {
                d.Key(keyValue.key).showBool = keyValue.showBool;
            }
        }
        
        public void DrawString(KeyValue keyValue, bool structureOnly = false)
        {
            keyValue.showString = ShowDictionaryValue(keyValue, keyValue.showString, "String", structureOnly, 
                () =>
                {
                    StartVertical();
                    Label("String", true);
                    for (int i = 0; i < keyValue.valuesString.Count; i++)
                    {
                        StartRow();
                        LabelGrey($"[{i}]", 25);
                        if (keyValue.stringAsTextBox)
                            keyValue.valuesString[i] = TextArea(keyValue.valuesString[i], fieldWidth, 50);
                        else
                            keyValue.valuesString[i] = TextField(keyValue.valuesString[i], fieldWidth);

                        var list = keyValue.valuesString;
                        BackgroundColor(CanMoveUp(i, list.Count) ? Color.white : Color.black);
                        ContentColor(CanMoveUp(i, list.Count) ? Color.black : Color.grey);
                        if (Button(symbolArrowUp, 25))
                        {
                            if (CanMoveUp(i, list.Count))
                            {
                                var temp = list[i - 1];
                            list[i - 1] = list[i];
                            list[i] = temp;
                            }
                        }
                        BackgroundColor(CanMoveDown(i, list.Count) ? Color.white : Color.black);
                        ContentColor(CanMoveDown(i, list.Count) ? Color.black : Color.grey);
                        if (Button(symbolArrowDown, 25))
                        {if (CanMoveDown(i, list.Count))
                            {
                                var temp = list[i + 1];
                                list[i + 1] = list[i];
                                list[i] = temp;
                            }}
                        ContentColor(Color.white);
                        
                        BackgroundColor(Color.red);
                        if (i != 0 && Button(symbolX, 25))
                        {
                            keyValue.valuesString.RemoveAt(i);
                            ExitGUI();
                        }

                        BackgroundColor(Color.green);
                        if (i == keyValue.valuesString.Count - 1 && Button("+", 25))
                            keyValue.valuesString.Add(default);
                        
                        BackgroundColor(Color.white);
                        EndRow();
                    }
                    EndVertical();
                },
                () => {  if (copyPasteItemObject)
                    keyValue.valuesString = copyPasteItemObject.dictionaries.Key(keyValue.key).valuesString; }, 
                () =>
                {
                    if (keyValue.showAllValueTypes)
                        keyValue.stringAsTextBox = LeftCheck("Display as Textbox", keyValue.stringAsTextBox, 150);
                });
            if (!structureOnly) return;
            foreach (Dictionaries d in allDictionariesStructure)
            {
                d.Key(keyValue.key).showString = keyValue.showString;
                d.Key(keyValue.key).stringAsTextBox = keyValue.stringAsTextBox;
            }
        }

        public void DrawAnimation(KeyValue keyValue, bool structureOnly = false)
        {
            keyValue.showAnimationClip = ShowDictionaryValue(keyValue, keyValue.showAnimationClip, "AnimationClip", structureOnly, 
                () => { StartVertical();
                    Label("AnimationClip", true);
                    for (int i = 0; i < keyValue.valuesAnimationClip.Count; i++)
                    {
                        StartRow();
                        LabelGrey($"[{i}]", 25);
                        keyValue.valuesAnimationClip[i] = Object(keyValue.valuesAnimationClip[i], typeof(AnimationClip), fieldWidth) as AnimationClip;
                        
                        var list = keyValue.valuesAnimationClip;
                        BackgroundColor(CanMoveUp(i, list.Count) ? Color.white : Color.black);
                        ContentColor(CanMoveUp(i, list.Count) ? Color.black : Color.grey);
                        if (Button(symbolArrowUp, 25))
                        {
                            if (CanMoveUp(i, list.Count))
                            {
                                var temp = list[i - 1];
                            list[i - 1] = list[i];
                            list[i] = temp;
                            }
                        }
                        BackgroundColor(CanMoveDown(i, list.Count) ? Color.white : Color.black);
                        ContentColor(CanMoveDown(i, list.Count) ? Color.black : Color.grey);
                        if (Button(symbolArrowDown, 25))
                        {if (CanMoveDown(i, list.Count))
                            {
                                var temp = list[i + 1];
                                list[i + 1] = list[i];
                                list[i] = temp;
                            }}
                        ContentColor(Color.white);
                        
                        BackgroundColor(Color.red);
                        if (i != 0 && Button(symbolX, 25))
                        {
                            keyValue.valuesAnimationClip.RemoveAt(i);
                            ExitGUI();
                        }

                        BackgroundColor(Color.green);
                        if (i == keyValue.valuesAnimationClip.Count - 1 && Button("+", 25))
                            keyValue.valuesAnimationClip.Add(default);
                        
                        BackgroundColor(Color.white);
                        EndRow();
                    }
                    BackgroundColor(Color.yellow);
                    AnimationClip objectToAdd = null;
                    StartRow();
                    objectToAdd = Object(objectToAdd, typeof(AnimationClip), 150) as AnimationClip;
                    LabelGrey("Drag/Drop single or multiple objects");
                    EndRow();
                    if (objectToAdd != null)
                    {
                        if (keyValue.valuesAnimationClip[0] == null)
                            keyValue.valuesAnimationClip[0] = objectToAdd;
                        else
                            keyValue.valuesAnimationClip.Add(objectToAdd);
                    }
                    BackgroundColor(Color.white);
                    
                    EndVertical(); },
                () => {  if (copyPasteItemObject)
                    keyValue.valuesAnimationClip = copyPasteItemObject.dictionaries.Key(keyValue.key).valuesAnimationClip; });
            if (!structureOnly) return;
            foreach (Dictionaries d in allDictionariesStructure)
                d.Key(keyValue.key).showAnimationClip = keyValue.showAnimationClip;
        }

        public void DrawTexture2D(KeyValue keyValue, bool structureOnly = false)
        {
            keyValue.showTexture2D = ShowDictionaryValue(keyValue, keyValue.showTexture2D, "Texture2D", structureOnly, 
                () => { StartVertical();
                    Label("Texture2D", true);
                    for (int i = 0; i < keyValue.valuesTexture2D.Count; i++)
                    {
                        StartRow();
                        LabelGrey($"[{i}]", 25);
                        keyValue.valuesTexture2D[i] = Object(keyValue.valuesTexture2D[i], typeof(Texture2D), fieldWidth) as Texture2D;
                        
                        var list = keyValue.valuesTexture2D;
                        BackgroundColor(CanMoveUp(i, list.Count) ? Color.white : Color.black);
                        ContentColor(CanMoveUp(i, list.Count) ? Color.black : Color.grey);
                        if (Button(symbolArrowUp, 25))
                        {
                            if (CanMoveUp(i, list.Count))
                            {
                                var temp = list[i - 1];
                            list[i - 1] = list[i];
                            list[i] = temp;
                            }
                        }
                        BackgroundColor(CanMoveDown(i, list.Count) ? Color.white : Color.black);
                        ContentColor(CanMoveDown(i, list.Count) ? Color.black : Color.grey);
                        if (Button(symbolArrowDown, 25))
                        {if (CanMoveDown(i, list.Count))
                            {
                                var temp = list[i + 1];
                                list[i + 1] = list[i];
                                list[i] = temp;
                            }}
                        ContentColor(Color.white);
                        
                        BackgroundColor(Color.red);
                        if (i != 0 && Button(symbolX, 25))
                        {
                            keyValue.valuesTexture2D.RemoveAt(i);
                            ExitGUI();
                        }

                        BackgroundColor(Color.green);
                        if (i == keyValue.valuesTexture2D.Count - 1 && Button("+", 25))
                            keyValue.valuesTexture2D.Add(default);
                        
                        BackgroundColor(Color.white);
                        EndRow();
                    }

                    BackgroundColor(Color.yellow);
                    Texture2D objectToAdd = null;
                    StartRow();
                    objectToAdd = Object(objectToAdd, typeof(Texture2D), 150) as Texture2D;
                    LabelGrey("Drag/Drop single or multiple objects");
                    EndRow();
                    if (objectToAdd != null)
                    {
                        if (keyValue.valuesTexture2D[0] == null)
                            keyValue.valuesTexture2D[0] = objectToAdd;
                        else
                            keyValue.valuesTexture2D.Add(objectToAdd);
                    }
                    BackgroundColor(Color.white);
                    
                    EndVertical(); },
                () => {  if (copyPasteItemObject)
                    keyValue.valuesTexture2D = copyPasteItemObject.dictionaries.Key(keyValue.key).valuesTexture2D; });
            if (!structureOnly) return;
            foreach (Dictionaries d in allDictionariesStructure)
                d.Key(keyValue.key).showTexture2D = keyValue.showTexture2D;
        }

        public void DrawSprite(KeyValue keyValue, bool structureOnly = false)
        {
            keyValue.showSprite = ShowDictionaryValue(keyValue, keyValue.showSprite, "Sprite", structureOnly, 
                () => { StartVertical();
                    Label("Sprite", true);
                    for (int i = 0; i < keyValue.valuesSprite.Count; i++)
                    {
                        StartRow();
                        LabelGrey($"[{i}]", 25);
                        keyValue.valuesSprite[i] = Object(keyValue.valuesSprite[i], typeof(Sprite), fieldWidth) as Sprite;
                       
                        var list = keyValue.valuesSprite;
                        BackgroundColor(CanMoveUp(i, list.Count) ? Color.white : Color.black);
                        ContentColor(CanMoveUp(i, list.Count) ? Color.black : Color.grey);
                        if (Button(symbolArrowUp, 25))
                        {
                            if (CanMoveUp(i, list.Count))
                            {
                                var temp = list[i - 1];
                            list[i - 1] = list[i];
                            list[i] = temp;
                            }
                        }
                        BackgroundColor(CanMoveDown(i, list.Count) ? Color.white : Color.black);
                        ContentColor(CanMoveDown(i, list.Count) ? Color.black : Color.grey);
                        if (Button(symbolArrowDown, 25))
                        {if (CanMoveDown(i, list.Count))
                            {
                                var temp = list[i + 1];
                                list[i + 1] = list[i];
                                list[i] = temp;
                            }}
                        ContentColor(Color.white);
                        
                        BackgroundColor(Color.red);
                        if (i != 0 && Button(symbolX, 25))
                        {
                            keyValue.valuesSprite.RemoveAt(i);
                            ExitGUI();
                        }

                        BackgroundColor(Color.green);
                        if (i == keyValue.valuesSprite.Count - 1 && Button("+", 25))
                            keyValue.valuesSprite.Add(default);
                        
                        BackgroundColor(Color.white);
                        EndRow();
                    }
                    
                    BackgroundColor(Color.yellow);
                    Sprite objectToAdd = null;
                    StartRow();
                    objectToAdd = Object(objectToAdd, typeof(Sprite), 150) as Sprite;
                    LabelGrey("Drag/Drop single or multiple objects");
                    EndRow();
                    if (objectToAdd != null)
                    {
                        if (keyValue.valuesSprite[0] == null)
                            keyValue.valuesSprite[0] = objectToAdd;
                        else
                            keyValue.valuesSprite.Add(objectToAdd);
                    }
                    BackgroundColor(Color.white);
                    
                    EndVertical(); },
                () => {  if (copyPasteItemObject)
                    keyValue.valuesSprite = copyPasteItemObject.dictionaries.Key(keyValue.key).valuesSprite; });
            if (!structureOnly) return;
            foreach (Dictionaries d in allDictionariesStructure)
                d.Key(keyValue.key).showSprite = keyValue.showSprite;
        }

        public void DrawAudioClip(KeyValue keyValue, bool structureOnly = false)
        {
            keyValue.showAudioClip = ShowDictionaryValue(keyValue, keyValue.showAudioClip, "AudioClip", structureOnly, 
                () => { StartVertical();
                    Label("AudioClip", true);
                    for (int i = 0; i < keyValue.valuesAudioClip.Count; i++)
                    {
                        StartRow();
                        LabelGrey($"[{i}]", 25);
                        keyValue.valuesAudioClip[i] = Object(keyValue.valuesAudioClip[i], typeof(AudioClip), fieldWidth) as AudioClip;
                        var list = keyValue.valuesAudioClip;
                        BackgroundColor(CanMoveUp(i, list.Count) ? Color.white : Color.black);
                        ContentColor(CanMoveUp(i, list.Count) ? Color.black : Color.grey);
                        if (Button(symbolArrowUp, 25))
                        {
                            if (CanMoveUp(i, list.Count))
                            {
                                var temp = list[i - 1];
                            list[i - 1] = list[i];
                            list[i] = temp;
                            }
                        }
                        BackgroundColor(CanMoveDown(i, list.Count) ? Color.white : Color.black);
                        ContentColor(CanMoveDown(i, list.Count) ? Color.black : Color.grey);
                        if (Button(symbolArrowDown, 25))
                        {if (CanMoveDown(i, list.Count))
                            {
                                var temp = list[i + 1];
                                list[i + 1] = list[i];
                                list[i] = temp;
                            }}
                        ContentColor(Color.white);
                        BackgroundColor(Color.red);
                        if (i != 0 && Button(symbolX, 25))
                        {
                            keyValue.valuesAudioClip.RemoveAt(i);
                            ExitGUI();
                        }

                        BackgroundColor(Color.green);
                        if (i == keyValue.valuesAudioClip.Count - 1 && Button("+", 25))
                            keyValue.valuesAudioClip.Add(default);
                        
                        BackgroundColor(Color.white);
                        EndRow();
                    }
                    
                    BackgroundColor(Color.yellow);
                    AudioClip objectToAdd = null;
                    StartRow();
                    objectToAdd = Object(objectToAdd, typeof(AudioClip), 150) as AudioClip;
                    LabelGrey("Drag/Drop single or multiple objects");
                    EndRow();
                    if (objectToAdd != null)
                    {
                        if (keyValue.valuesAudioClip[0] == null)
                            keyValue.valuesAudioClip[0] = objectToAdd;
                        else
                            keyValue.valuesAudioClip.Add(objectToAdd);
                    }
                    BackgroundColor(Color.white);
                    
                    
                    EndVertical(); },
                () => {  if (copyPasteItemObject)
                    keyValue.valuesAudioClip = copyPasteItemObject.dictionaries.Key(keyValue.key).valuesAudioClip; });
            if (!structureOnly) return;
            foreach (Dictionaries d in allDictionariesStructure)
                d.Key(keyValue.key).showAudioClip = keyValue.showAudioClip;
        }

        public void DrawPrefab(KeyValue keyValue, bool structureOnly = false)
        {
            keyValue.showPrefab = ShowDictionaryValue(keyValue, keyValue.showPrefab, "Prefab", structureOnly, 
                () => { StartVertical();
                    Label("Prefab", true);
                    for (int i = 0; i < keyValue.valuesPrefab.Count; i++)
                    {
                        StartRow();
                        LabelGrey($"[{i}]", 25);
                        keyValue.valuesPrefab[i] = Object(keyValue.valuesPrefab[i], typeof(GameObject), fieldWidth) as GameObject;
                        
                        var list = keyValue.valuesPrefab;
                        BackgroundColor(CanMoveUp(i, list.Count) ? Color.white : Color.black);
                        ContentColor(CanMoveUp(i, list.Count) ? Color.black : Color.grey);
                        if (Button(symbolArrowUp, 25))
                        {
                            if (CanMoveUp(i, list.Count))
                            {
                                var temp = list[i - 1];
                            list[i - 1] = list[i];
                            list[i] = temp;
                            }
                        }
                        BackgroundColor(CanMoveDown(i, list.Count) ? Color.white : Color.black);
                        ContentColor(CanMoveDown(i, list.Count) ? Color.black : Color.grey);
                        if (Button(symbolArrowDown, 25))
                        {if (CanMoveDown(i, list.Count))
                            {
                                var temp = list[i + 1];
                                list[i + 1] = list[i];
                                list[i] = temp;
                            }}
                        ContentColor(Color.white);
                        
                        BackgroundColor(Color.red);
                        if (i != 0 && Button(symbolX, 25))
                        {
                            keyValue.valuesPrefab.RemoveAt(i);
                            ExitGUI();
                        }

                        BackgroundColor(Color.green);
                        if (i == keyValue.valuesPrefab.Count - 1 && Button("+", 25))
                            keyValue.valuesPrefab.Add(default);
                        
                        BackgroundColor(Color.white);
                        EndRow();
                    }
                    
                    BackgroundColor(Color.yellow);
                    GameObject objectToAdd = null;
                    StartRow();
                    objectToAdd = Object(objectToAdd, typeof(GameObject), 150) as GameObject;
                    LabelGrey("Drag/Drop single or multiple objects");
                    EndRow();
                    if (objectToAdd != null)
                    {
                        if (keyValue.valuesPrefab[0] == null)
                            keyValue.valuesPrefab[0] = objectToAdd;
                        else
                            keyValue.valuesPrefab.Add(objectToAdd);
                    }
                    BackgroundColor(Color.white);
                    
                    EndVertical(); },
                () => {  if (copyPasteItemObject)
                    keyValue.valuesPrefab = copyPasteItemObject.dictionaries.Key(keyValue.key).valuesPrefab; });
            if (!structureOnly) return;
            foreach (Dictionaries d in allDictionariesStructure)
                d.Key(keyValue.key).showPrefab = keyValue.showPrefab;
        }

        public void DrawColor(KeyValue keyValue, bool structureOnly = false)
        {
            keyValue.showColor = ShowDictionaryValue(keyValue, keyValue.showColor, "Color", structureOnly, 
                () => { StartVertical();
                    Label("Color", true);
                    for (int i = 0; i < keyValue.valuesColor.Count; i++)
                    {
                        StartRow();
                        LabelGrey($"[{i}]", 25);
                        keyValue.valuesColor[i] = ColorField(keyValue.valuesColor[i], fieldWidth);
                        
                        var list = keyValue.valuesColor;
                        BackgroundColor(CanMoveUp(i, list.Count) ? Color.white : Color.black);
                        ContentColor(CanMoveUp(i, list.Count) ? Color.black : Color.grey);
                        if (Button(symbolArrowUp, 25))
                        {
                            if (CanMoveUp(i, list.Count))
                            {
                                var temp = list[i - 1];
                            list[i - 1] = list[i];
                            list[i] = temp;
                            }
                        }
                        BackgroundColor(CanMoveDown(i, list.Count) ? Color.white : Color.black);
                        ContentColor(CanMoveDown(i, list.Count) ? Color.black : Color.grey);
                        if (Button(symbolArrowDown, 25))
                        {if (CanMoveDown(i, list.Count))
                            {
                                var temp = list[i + 1];
                                list[i + 1] = list[i];
                                list[i] = temp;
                            }}
                        ContentColor(Color.white);
                        
                        BackgroundColor(Color.red);
                        if (i != 0 && Button(symbolX, 25))
                        {
                            keyValue.valuesColor.RemoveAt(i);
                            ExitGUI();
                        }

                        BackgroundColor(Color.green);
                        if (i == keyValue.valuesColor.Count - 1 && Button("+", 25))
                            keyValue.valuesColor.Add(default);
                        
                        BackgroundColor(Color.white);
                        EndRow();
                    }
                    EndVertical(); },
                () => {  if (copyPasteItemObject)
                    keyValue.valuesColor = copyPasteItemObject.dictionaries.Key(keyValue.key).valuesColor; });
            if (!structureOnly) return;
            foreach (Dictionaries d in allDictionariesStructure)
                d.Key(keyValue.key).showColor = keyValue.showColor;
        }

        public void DrawVector3(KeyValue keyValue, bool structureOnly = false)
        {
            keyValue.showVector3 = ShowDictionaryValue(keyValue, keyValue.showVector3, "Vector3", structureOnly, 
                () => { StartVertical();
                    Label("Vector3", true);
                    for (int i = 0; i < keyValue.valuesVector3.Count; i++)
                    {
                        StartRow();
                        LabelGrey($"[{i}]", 25);
                        keyValue.valuesVector3[i] = Vector3Field(keyValue.valuesVector3[i], fieldWidth);
                        
                        var list = keyValue.valuesVector3;
                        BackgroundColor(CanMoveUp(i, list.Count) ? Color.white : Color.black);
                        ContentColor(CanMoveUp(i, list.Count) ? Color.black : Color.grey);
                        if (Button(symbolArrowUp, 25))
                        {
                            if (CanMoveUp(i, list.Count))
                            {
                                var temp = list[i - 1];
                            list[i - 1] = list[i];
                            list[i] = temp;
                            }
                        }
                        BackgroundColor(CanMoveDown(i, list.Count) ? Color.white : Color.black);
                        ContentColor(CanMoveDown(i, list.Count) ? Color.black : Color.grey);
                        if (Button(symbolArrowDown, 25))
                        {if (CanMoveDown(i, list.Count))
                            {
                                var temp = list[i + 1];
                                list[i + 1] = list[i];
                                list[i] = temp;
                            }}
                        ContentColor(Color.white);
                        
                        BackgroundColor(Color.red);
                        if (i != 0 && Button(symbolX, 25))
                        {
                            keyValue.valuesVector3.RemoveAt(i);
                            ExitGUI();
                        }

                        BackgroundColor(Color.green);
                        if (i == keyValue.valuesVector3.Count - 1 && Button("+", 25))
                            keyValue.valuesVector3.Add(default);
                        
                        BackgroundColor(Color.white);
                        EndRow();
                    }
                    EndVertical(); },
                () => {  if (copyPasteItemObject)
                    keyValue.valuesVector3 = copyPasteItemObject.dictionaries.Key(keyValue.key).valuesVector3; });
            if (!structureOnly) return;
            foreach (Dictionaries d in allDictionariesStructure)
                d.Key(keyValue.key).showVector3 = keyValue.showVector3;
        }

        public void DrawVector2(KeyValue keyValue, bool structureOnly = false)
        {
            keyValue.showVector2 = ShowDictionaryValue(keyValue, keyValue.showVector2, "Vector2", structureOnly, 
                () => { StartVertical();
                    Label("Vector2", true);
                    for (int i = 0; i < keyValue.valuesVector2.Count; i++)
                    {
                        StartRow();
                        LabelGrey($"[{i}]", 25);
                        keyValue.valuesVector2[i] = Vector2Field(keyValue.valuesVector2[i], fieldWidth);
                        
                        var list = keyValue.valuesVector2;
                        BackgroundColor(CanMoveUp(i, list.Count) ? Color.white : Color.black);
                        ContentColor(CanMoveUp(i, list.Count) ? Color.black : Color.grey);
                        if (Button(symbolArrowUp, 25))
                        {
                            if (CanMoveUp(i, list.Count))
                            {
                                var temp = list[i - 1];
                            list[i - 1] = list[i];
                            list[i] = temp;
                            }
                        }
                        BackgroundColor(CanMoveDown(i, list.Count) ? Color.white : Color.black);
                        ContentColor(CanMoveDown(i, list.Count) ? Color.black : Color.grey);
                        if (Button(symbolArrowDown, 25))
                        {if (CanMoveDown(i, list.Count))
                            {
                                var temp = list[i + 1];
                                list[i + 1] = list[i];
                                list[i] = temp;
                            }}
                        ContentColor(Color.white);
                        
                        BackgroundColor(Color.red);
                        if (i != 0 && Button(symbolX, 25))
                        {
                            keyValue.valuesVector2.RemoveAt(i);
                            ExitGUI();
                        }

                        BackgroundColor(Color.green);
                        if (i == keyValue.valuesVector2.Count - 1 && Button("+", 25))
                            keyValue.valuesVector2.Add(default);
                        
                        BackgroundColor(Color.white);
                        EndRow();
                    }
                    EndVertical(); },
                () => {  if (copyPasteItemObject)
                    keyValue.valuesVector2 = copyPasteItemObject.dictionaries.Key(keyValue.key).valuesVector2; });
            if (!structureOnly) return;
            foreach (Dictionaries d in allDictionariesStructure)
                d.Key(keyValue.key).showVector2 = keyValue.showVector2;
        }

        public void DrawStat(KeyValue keyValue, bool structureOnly = false)
        {
            keyValue.showStat = ShowDictionaryValue(keyValue, keyValue.showStat, "Stat", structureOnly, 
                () => { StartVertical();
                    Label("Stat", true);
                    for (int i = 0; i < keyValue.valuesStat.Count; i++)
                    {
                        StartRow();
                        LabelGrey($"[{i}]", 25);
                        keyValue.valuesStat[i] = Object(keyValue.valuesStat[i], typeof(Stat), fieldWidth) as Stat;
                        if (keyValue.valuesStat[i])
                            keyValue.SetValue(i, keyValue.valuesStat[i]);
                        
                        var list = keyValue.valuesStat;
                        BackgroundColor(CanMoveUp(i, list.Count) ? Color.white : Color.black);
                        ContentColor(CanMoveUp(i, list.Count) ? Color.black : Color.grey);
                        if (Button(symbolArrowUp, 25))
                        {
                            if (CanMoveUp(i, list.Count))
                            {
                                var temp = list[i - 1];
                                list[i - 1] = list[i];
                                list[i] = temp;
                            }
                        }
                        BackgroundColor(CanMoveDown(i, list.Count) ? Color.white : Color.black);
                        ContentColor(CanMoveDown(i, list.Count) ? Color.black : Color.grey);
                        if (Button(symbolArrowDown, 25))
                        {
                            if (CanMoveDown(i, list.Count))
                            {
                                var temp = list[i + 1];
                                list[i + 1] = list[i];
                                list[i] = temp;
                            }
                        }
                        ContentColor(Color.white);
                        
                        BackgroundColor(Color.red);
                        if (i != 0 && Button(symbolX, 25))
                        {
                            keyValue.valuesStat.RemoveAt(i);
                            keyValue.valuesStatUid.RemoveAt(i);
                            ExitGUI();
                        }

                        BackgroundColor(Color.green);
                        if (i == keyValue.valuesStat.Count - 1 && Button("+", 25))
                        {
                            keyValue.valuesStat.Add(default);
                            keyValue.valuesStatUid.Add(default);
                        }
                        
                        BackgroundColor(Color.white);
                        EndRow();
                    }
                    
                    BackgroundColor(Color.yellow);
                    Stat objectToAdd = null;
                    StartRow();
                    objectToAdd = Object(objectToAdd, typeof(Stat), 150) as Stat;
                    LabelGrey("Drag/Drop single or multiple objects");
                    EndRow();
                    if (objectToAdd != null)
                    {
                        if (keyValue.valuesStat[0] == null)
                            keyValue.valuesStat[0] = objectToAdd;
                        else
                            keyValue.valuesStat.Add(objectToAdd);
                    }
                    BackgroundColor(Color.white);
                    
                    
                    EndVertical(); },
                () => {  if (copyPasteItemObject)
                    keyValue.valuesStat = copyPasteItemObject.dictionaries.Key(keyValue.key).valuesStat; });
            if (!structureOnly) return;
            foreach (Dictionaries d in allDictionariesStructure)
                d.Key(keyValue.key).showStat = keyValue.showStat;
        }

        public void DrawItemObject(KeyValue keyValue, bool structureOnly = false)
        {
            keyValue.showItemObject = ShowDictionaryValue(keyValue, keyValue.showItemObject, "Item Object", structureOnly, 
                () =>
                {
                    StartVertical();
                    Label("Item Object", true);
                    for (int i = 0; i < keyValue.valuesItemObject.Count; i++)
                    {
                        StartRow();
                        LabelGrey($"[{i}]", 25);
                        keyValue.valuesItemObject[i] = Object(keyValue.valuesItemObject[i], typeof(ItemObject), fieldWidth) as ItemObject;
                        if (keyValue.valuesItemObject[i])
                            keyValue.SetValue(i, keyValue.valuesItemObject[i]);
                        
                        var list = keyValue.valuesItemObject;
                        BackgroundColor(CanMoveUp(i, list.Count) ? Color.white : Color.black);
                        ContentColor(CanMoveUp(i, list.Count) ? Color.black : Color.grey);
                        if (Button(symbolArrowUp, 25))
                        {
                            if (CanMoveUp(i, list.Count))
                            {
                                var temp = list[i - 1];
                            list[i - 1] = list[i];
                            list[i] = temp;
                            }
                        }
                        BackgroundColor(CanMoveDown(i, list.Count) ? Color.white : Color.black);
                        ContentColor(CanMoveDown(i, list.Count) ? Color.black : Color.grey);
                        if (Button(symbolArrowDown, 25))
                        {if (CanMoveDown(i, list.Count))
                            {
                                var temp = list[i + 1];
                                list[i + 1] = list[i];
                                list[i] = temp;
                            }}
                        ContentColor(Color.white);
                        
                        BackgroundColor(Color.red);
                        if (i != 0 && Button(symbolX, 25))
                        {
                            keyValue.valuesItemObject.RemoveAt(i);
                            keyValue.valuesItemObjectUid.RemoveAt(i);
                            ExitGUI();
                        }

                        BackgroundColor(Color.green);
                        if (i == keyValue.valuesItemObject.Count - 1 && Button("+", 25))
                        {
                            keyValue.valuesItemObject.Add(default);
                            keyValue.valuesItemObjectUid.Add(default);
                        }
                        BackgroundColor(Color.white);
                        EndRow();
                    }
                    
                    BackgroundColor(Color.yellow);
                    ItemObject objectToAdd = null;
                    StartRow();
                    objectToAdd = Object(objectToAdd, typeof(ItemObject), 150) as ItemObject;
                    LabelGrey("Drag/Drop single or multiple objects");
                    EndRow();
                    if (objectToAdd != null)
                    {
                        if (keyValue.valuesItemObject[0] == null)
                            keyValue.valuesItemObject[0] = objectToAdd;
                        else
                            keyValue.valuesItemObject.Add(objectToAdd);
                    }
                    BackgroundColor(Color.white);
                    
                    EndVertical();
                },
                () => {  if (copyPasteItemObject)
                    keyValue.valuesItemObject = copyPasteItemObject.dictionaries.Key(keyValue.key).valuesItemObject; });
            if (!structureOnly) return;
            foreach (Dictionaries d in allDictionariesStructure)
                d.Key(keyValue.key).showItemObject = keyValue.showItemObject;
        }

        public void DrawItemObjectType(KeyValue keyValue, bool structureOnly = false)
        {
            keyValue.showItemObjectType = ShowDictionaryValue(keyValue, keyValue.showItemObjectType, "Item Object Type", structureOnly, 
                () => { 
                    
                    StartVertical();
                    Label("Item Object Type", true);
                    var thisList = GameModuleObjects<ItemObject>().Select(x => x.objectType).Distinct().ToList();
                    for (int i = 0; i < keyValue.valuesItemObjectType.Count; i++)
                    {
                        StartRow();
                        LabelGrey($"[{i}]", 25);
                        int valueIndex = thisList.IndexOf(keyValue.valuesItemObjectType[i]);
                        if (valueIndex < 0) 
                            valueIndex = 0;
                        String[] typeValues = thisList.ToArray();
                        if (typeValues.Length > 0)
                        {
                            keyValue.valuesItemObjectType[i] = typeValues[Popup(valueIndex, typeValues, fieldWidth)];
                        }
                        
                        var list = keyValue.valuesItemObjectType;
                        BackgroundColor(CanMoveUp(i, list.Count) ? Color.white : Color.black);
                        ContentColor(CanMoveUp(i, list.Count) ? Color.black : Color.grey);
                        if (Button(symbolArrowUp, 25))
                        {
                            if (CanMoveUp(i, list.Count))
                            {
                                var temp = list[i - 1];
                            list[i - 1] = list[i];
                            list[i] = temp;
                            }
                        }
                        BackgroundColor(CanMoveDown(i, list.Count) ? Color.white : Color.black);
                        ContentColor(CanMoveDown(i, list.Count) ? Color.black : Color.grey);
                        if (Button(symbolArrowDown, 25))
                        {
                            if (CanMoveDown(i, list.Count))
                            {
                                var temp = list[i + 1];
                                list[i + 1] = list[i];
                                list[i] = temp;
                            }
                        }
                        ContentColor(Color.white);
                        
                        BackgroundColor(Color.red);
                        if (i != 0 && Button(symbolX, 25))
                        {
                            keyValue.valuesItemObjectType.RemoveAt(i);
                            ExitGUI();
                        }

                        BackgroundColor(Color.green);
                        if (i == keyValue.valuesItemObjectType.Count - 1 && Button("+", 25))
                            keyValue.valuesItemObjectType.Add(default);
                        
                        BackgroundColor(Color.white);
                        EndRow();
                    }
                    
                    EndVertical();
                    
                    
                },
                () => {  if (copyPasteItemObject)
                    keyValue.valuesItemObjectType = copyPasteItemObject.dictionaries.Key(keyValue.key).valuesItemObjectType; });
            if (!structureOnly) return;
            foreach (Dictionaries d in allDictionariesStructure)
                d.Key(keyValue.key).showItemObjectType = keyValue.showItemObjectType;
        }

        public void DrawItemAttribute(KeyValue keyValue, bool structureOnly = false)
        {
            keyValue.showItemAttribute = ShowDictionaryValue(keyValue, keyValue.showItemAttribute, "Item Attribute", structureOnly, 
                () =>
                {
                    StartVertical();
                    Label("Item Attribute", true);
                    for (int i = 0; i < keyValue.valuesItemAttribute.Count; i++)
                    {
                        StartRow();
                        LabelGrey($"[{i}]", 25);
                        keyValue.valuesItemAttribute[i] = Object(keyValue.valuesItemAttribute[i], typeof(ItemAttribute), fieldWidth) as ItemAttribute;
                        if (keyValue.valuesItemAttribute[i])
                            keyValue.SetValue(i, keyValue.valuesItemAttribute[i]);
                        
                        var list = keyValue.valuesItemAttribute;
                        BackgroundColor(CanMoveUp(i, list.Count) ? Color.white : Color.black);
                        ContentColor(CanMoveUp(i, list.Count) ? Color.black : Color.grey);
                        if (Button(symbolArrowUp, 25))
                        {
                            if (CanMoveUp(i, list.Count))
                            {
                                var temp = list[i - 1];
                            list[i - 1] = list[i];
                            list[i] = temp;
                            }
                        }
                        BackgroundColor(CanMoveDown(i, list.Count) ? Color.white : Color.black);
                        ContentColor(CanMoveDown(i, list.Count) ? Color.black : Color.grey);
                        if (Button(symbolArrowDown, 25))
                        {if (CanMoveDown(i, list.Count))
                            {
                                var temp = list[i + 1];
                                list[i + 1] = list[i];
                                list[i] = temp;
                            }}
                        ContentColor(Color.white);
                        
                        BackgroundColor(Color.green);
                        if (i == keyValue.valuesItemAttribute.Count - 1 && Button("+", 25))
                        {
                            keyValue.valuesItemAttribute.Add(default);
                            keyValue.valuesItemAttributeUid.Add(default);
                        }
                        
                        BackgroundColor(Color.red);
                        if (i != 0 && Button(symbolX, 25))
                        {
                            keyValue.valuesItemAttribute.RemoveAt(i);
                            keyValue.valuesItemAttributeUid.RemoveAt(i);
                            ExitGUI();
                        }
                        
                        BackgroundColor(Color.white);
                        EndRow();
                    }
                    BackgroundColor(Color.yellow);
                    ItemAttribute objectToAdd = null;
                    StartRow();
                    objectToAdd = Object(objectToAdd, typeof(ItemAttribute), 150) as ItemAttribute;
                    LabelGrey("Drag/Drop single or multiple objects");
                    EndRow();
                    if (objectToAdd != null)
                    {
                        if (keyValue.valuesItemAttribute[0] == null)
                            keyValue.valuesItemAttribute[0] = objectToAdd;
                        else
                            keyValue.valuesItemAttribute.Add(objectToAdd);
                    }
                    BackgroundColor(Color.white);
                    
                    EndVertical();
                },
                () => {  if (copyPasteItemObject)
                    keyValue.valuesItemAttribute = copyPasteItemObject.dictionaries.Key(keyValue.key).valuesItemAttribute; });
            if (!structureOnly) return;
            foreach (Dictionaries d in allDictionariesStructure)
                d.Key(keyValue.key).showItemAttribute = keyValue.showItemAttribute;
        }

        public void DrawItemAttributeType(KeyValue keyValue, bool structureOnly = false)
        {
            keyValue.showItemAttributeType = ShowDictionaryValue(keyValue, keyValue.showItemAttributeType, "Item Attribute Type", structureOnly,
                () =>
                {
                    StartVertical();
                    var thisList = GameModuleObjects<ItemAttribute>().Select(x => x.objectType).Distinct().ToList();
                    Label("Item Attribute Type", true);
                    for (int i = 0; i < keyValue.valuesItemAttributeType.Count; i++)
                    {
                        StartRow();
                        LabelGrey($"[{i}]", 25);

                        int valueIndex = thisList.IndexOf(keyValue.valuesItemAttributeType[i]);
                        if (valueIndex < 0) 
                            valueIndex = 0;
                        String[] typeValues = thisList.ToArray();
                        if (typeValues.Length > 0)
                        {
                            keyValue.valuesItemAttributeType[i] = typeValues[Popup(valueIndex, typeValues, fieldWidth)];
                        }
                        
                        var list = keyValue.valuesItemAttributeType;
                        BackgroundColor(CanMoveUp(i, list.Count) ? Color.white : Color.black);
                        ContentColor(CanMoveUp(i, list.Count) ? Color.black : Color.grey);
                        if (Button(symbolArrowUp, 25))
                        {
                            if (CanMoveUp(i, list.Count))
                            {
                                var temp = list[i - 1];
                            list[i - 1] = list[i];
                            list[i] = temp;
                            }
                        }
                        BackgroundColor(CanMoveDown(i, list.Count) ? Color.white : Color.black);
                        ContentColor(CanMoveDown(i, list.Count) ? Color.black : Color.grey);
                        if (Button(symbolArrowDown, 25))
                        {if (CanMoveDown(i, list.Count))
                            {
                                var temp = list[i + 1];
                                list[i + 1] = list[i];
                                list[i] = temp;
                            }}
                        ContentColor(Color.white);
                        
                        BackgroundColor(Color.red);
                        if (i != 0 && Button(symbolX, 25))
                        {
                            keyValue.valuesItemAttributeType.RemoveAt(i);
                            ExitGUI();
                        }

                        BackgroundColor(Color.green);
                        if (i == keyValue.valuesItemAttributeType.Count - 1 && Button("+", 25))
                            keyValue.valuesItemAttributeType.Add(default);
                        
                        BackgroundColor(Color.white);
                        EndRow();
                    }
                    EndVertical();
                },
                () =>
                {
                    if (copyPasteItemObject)
                        keyValue.valuesItemAttributeType = copyPasteItemObject.dictionaries.Key(keyValue.key).valuesItemAttributeType;
                });
            if (!structureOnly) return;
            foreach (Dictionaries d in allDictionariesStructure)
                d.Key(keyValue.key).showItemAttributeType = keyValue.showItemAttributeType;
        }
        
        // December 21 2021 -- THe "ref" part of the method is removed, as we aren't doing the copyPaste area anymore.
        public void DrawCondition(KeyValue keyValue, bool structureOnly = false)
        {
            keyValue.showCondition = ShowDictionaryValue(keyValue, keyValue.showCondition, "Condition", structureOnly, 
                () =>
                {
                    StartVertical();
                    Label("Condition", true);
                    for (int i = 0; i < keyValue.valuesCondition.Count; i++)
                    {
                        StartRow();
                        LabelGrey($"[{i}]", 25);
                        keyValue.valuesCondition[i] = Object(keyValue.valuesCondition[i], typeof(Condition), fieldWidth) as Condition;
                        if (keyValue.valuesCondition[i])
                            keyValue.SetValue(i, keyValue.valuesCondition[i]);
                        
                        var list = keyValue.valuesCondition;
                        BackgroundColor(CanMoveUp(i, list.Count) ? Color.white : Color.black);
                        ContentColor(CanMoveUp(i, list.Count) ? Color.black : Color.grey);
                        if (Button(symbolArrowUp, 25))
                        {
                            if (CanMoveUp(i, list.Count))
                            {
                                var temp = list[i - 1];
                                list[i - 1] = list[i];
                                list[i] = temp;
                            }
                        }
                        BackgroundColor(CanMoveDown(i, list.Count) ? Color.white : Color.black);
                        ContentColor(CanMoveDown(i, list.Count) ? Color.black : Color.grey);
                        if (Button(symbolArrowDown, 25))
                        {
                            if (CanMoveDown(i, list.Count))
                            {
                                var temp = list[i + 1];
                                list[i + 1] = list[i];
                                list[i] = temp;
                            }
                        }
                        ContentColor(Color.white);
                        
                        BackgroundColor(Color.green);
                        if (i == keyValue.valuesCondition.Count - 1 && Button("+", 25))
                        {
                            keyValue.valuesCondition.Add(default);
                            keyValue.valuesConditionUid.Add(default);
                        }
                        
                        BackgroundColor(Color.red);
                        if (i != 0 && Button(symbolX, 25))
                        {
                            keyValue.valuesCondition.RemoveAt(i);
                            keyValue.valuesConditionUid.RemoveAt(i);
                            ExitGUI();
                        }
                        
                        BackgroundColor(Color.white);
                        EndRow();
                    }
                    
                    BackgroundColor(Color.yellow);
                    Condition objectToAdd = null;
                    StartRow();
                    objectToAdd = Object(objectToAdd, typeof(Condition), 150) as Condition;
                    LabelGrey("Drag/Drop single or multiple objects");
                    EndRow();
                    if (objectToAdd != null)
                    {
                        if (keyValue.valuesCondition[0] == null)
                            keyValue.valuesCondition[0] = objectToAdd;
                        else
                            keyValue.valuesCondition.Add(objectToAdd);
                    }
                    BackgroundColor(Color.white);
                    
                    
                    EndVertical();
                },
                () => {   });
            if (!structureOnly) return;
            foreach (Dictionaries d in allDictionariesStructure)
                d.Key(keyValue.key).showCondition = keyValue.showCondition;
        }

        public void DrawConditionType(KeyValue keyValue, bool structureOnly = false)
        {
            keyValue.showConditionType = ShowDictionaryValue(keyValue, keyValue.showConditionType, "Condition Type", structureOnly,
                () =>
                {
                    StartVertical();
                    var thisList = GameModuleObjects<Condition>().Select(x => x.objectType).Distinct().ToList();
                    Label("Condition Type", true);
                    for (int i = 0; i < keyValue.valuesConditionType.Count; i++)
                    {
                        StartRow();
                        LabelGrey($"[{i}]", 25);

                        int valueIndex = thisList.IndexOf(keyValue.valuesConditionType[i]);
                        if (valueIndex < 0) 
                            valueIndex = 0;
                        String[] typeValues = thisList.ToArray();
                        if (typeValues.Length > 0)
                        {
                            keyValue.valuesConditionType[i] = typeValues[Popup(valueIndex, typeValues, fieldWidth)];
                        }
                        
                        var list = keyValue.valuesConditionType;
                        BackgroundColor(CanMoveUp(i, list.Count) ? Color.white : Color.black);
                        ContentColor(CanMoveUp(i, list.Count) ? Color.black : Color.grey);
                        if (Button(symbolArrowUp, 25))
                        {
                            if (CanMoveUp(i, list.Count))
                            {
                                var temp = list[i - 1];
                            list[i - 1] = list[i];
                            list[i] = temp;
                            }
                        }
                        BackgroundColor(CanMoveDown(i, list.Count) ? Color.white : Color.black);
                        ContentColor(CanMoveDown(i, list.Count) ? Color.black : Color.grey);
                        if (Button(symbolArrowDown, 25))
                        {if (CanMoveDown(i, list.Count))
                            {
                                var temp = list[i + 1];
                                list[i + 1] = list[i];
                                list[i] = temp;
                            }}
                        ContentColor(Color.white);
                        
                        BackgroundColor(Color.red);
                        if (i != 0 && Button(symbolX, 25))
                        {
                            keyValue.valuesConditionType.RemoveAt(i);
                            ExitGUI();
                        }

                        BackgroundColor(Color.green);
                        if (i == keyValue.valuesConditionType.Count - 1 && Button("+", 25))
                            keyValue.valuesConditionType.Add(default);
                        
                        BackgroundColor(Color.white);
                        EndRow();
                    }
                    EndVertical();
                },
                () =>
                {
                    
                });
            if (!structureOnly) return;
            foreach (Dictionaries d in allDictionariesStructure)
                d.Key(keyValue.key).showConditionType = keyValue.showConditionType;
        }

        

        

        public void DrawLabel(string details)
        {
            Label(details);
        }

        public void DrawDetails(KeyValue keyValue, ref bool shouldDraw)
        {
            Debug.Log("DrawDetails");
            if (Button(shouldDraw ? "Hide" : "Show"))
            {
                shouldDraw = !shouldDraw;
            }

            if (!shouldDraw) return;

            Label(keyValue.key);
        }
        
        
        // CODE FOR DRAWING ALL BOXES
        
        bool ShowDictionaryValue(KeyValue keyValue, bool showThis, string labelName, bool structureOnly, Action content, Action content3, Action content2 = null)
        {
            if (!showThis && !keyValue.showAllValueTypes) return showThis;
            StartRow();
            AllValueColors(showThis);
            if (keyValue.showAllValueTypes && structureOnly && Button(labelName, 180))
            {
                showThis = !showThis;
            }
            if (showThis && !keyValue.showAllValueTypes && structureOnly)
            {
                Label(labelName);
            }
            ResetAllValueColors();
            
            if (!structureOnly) // Non-structure only
            {
                keyValue.showAllValueTypes = false;
                content?.Invoke();
                //LabelGrey(labelName);
            }
            else
            {
                // Content 2 used for "Structure" only
                content2?.Invoke();
            }
            EndRow();
            
            return showThis;
        }
        
        private void AllValueColors(bool isTrue)
        {
            BackgroundColor(isTrue ? Color.white : Color.black);
            ContentColor(isTrue ? Color.white : Color.grey);
        }
        
        private void ResetAllValueColors()
        {
            BackgroundColor(Color.white);
            ContentColor(Color.white);
        }
    }
}
