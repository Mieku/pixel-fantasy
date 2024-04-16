using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityPBR.Modules
{
    public abstract class InfinityEditorModules<T> : InfinityEditorScriptableObject<T> where T : ScriptableObject
    {
        protected override void OnEnable()
        {
            base.OnEnable();

            Cache();
        }

        protected override void Cache()
        {
            Debug.Log("[Not an error] Cache called on InfinityEditorModules -- Did you forget to override this?\n\nComment this out to ignore it forever.");
        }

        /*
         * ----------------------------------------------------------------------------------------------
         * NO MODULE REQUIRED
         * ----------------------------------------------------------------------------------------------
         */

        public static bool ShowFullInspector(string scriptName)
        {
            Space();
            SetBool("Show full inspector " + scriptName,
                LeftCheck("Show full inspector", GetBool("Show full inspector " + scriptName)));
            return GetBool("Show full inspector " + scriptName);
        }
        
        public static void ShowKeyPairHeader()
        {
            StartRow();
            Label("", 25);
            Label("Key", "This is the key which you will use to look up the information attached to an " +
                         "object.", 200);
            Label("Default Value", "When new items, prefixes, and suffixes are added, they will" +
                                   "start with these default values. Changing the value here will not " +
                                   "have any effect on existing items, prefixes, or suffixes.", 100);
            EndRow();
        }

        public static void ShowKeyValueListHeader()
        {
            StartRow();
            Label("Key", 200, true);
            Label("Type", 100, true);
            Label("Value", 300, true);
            EndRow();
        }

        public static void ShowLinkToDictionaries()
        {
            StartRow();
            BackgroundColor(Color.magenta);
            
            if (Button("Dictionaries Module is required.\nClick to get the Module on the Asset Store.",  400, 50))
                OpenURL("http://legacy.infinitypbr.com/AssetLinks/DictionaryModule.html");
            
            ResetColor();
            EndRow();
        }

        public static void ShowLinkToStatAndSkills()
        {
            StartRow();
            BackgroundColor(Color.magenta);
            
            if (Button("Stats Or Skills Module is required.\nClick to get the Module on the Asset Store.",  400, 50))
                OpenURL("http://legacy.infinitypbr.com/AssetLinks/StatsOrSkillsModule.html");

            ResetColor();
            EndRow();
        }

        /*
         * ----------------------------------------------------------------------------------------------
         * DICTIONARIES MODULE
         * ----------------------------------------------------------------------------------------------
         */

        public static KeyValue ShowAddNewKeyValuePair(Dictionaries dictionaries)
        {
            KeyValue addToAllDictionaries = null;
            if (String.IsNullOrEmpty(GetString("New KeyValue " + dictionaries.Uid())))
            {
                SetString("New KeyValue " + dictionaries.Uid(), "New keyValue");
            }
            BackgroundColor(mixed);
            StartVerticalBox();
            Label("Add new keyvalue", true);
            StartRow();
            SetString("New KeyValue " + dictionaries.Uid(), TextField(GetString("New KeyValue " + dictionaries.Uid()), 200));
            if (Button("Add", 100))
            {
                var newKey = GetString("New KeyValue " + dictionaries.Uid()).Trim();
                if (dictionaries.HasKeyValue(newKey))
                    Log("There is already a Key named " + newKey);
                else
                {
                    KeyValue newKeyValue = dictionaries.AddNewKeyValue(newKey);
                    addToAllDictionaries = newKeyValue;
                }
            }

            EndRow();
            EndVerticalBox();
            ResetColor();

            return addToAllDictionaries;
        }

        public static void ShowDictionaryHelpBox()
        {
            EditorGUILayout.HelpBox("\"Dictionaries\" in this context are not exactly the same as other Dictionaries that you " +
                                    "may be used to. These are key/value lists which can be serialized and are intended to be " +
                                    "used in a similar fashion to Dictionaries.\n\n" +
                                    "This will allow you to add your own custom key/value pairs. Anything added here will be added " +
                                    "to all of the Stat or Skills.\n\nSet keys here, and manage values on individual " +
                                    "Stat or Skill objects.", MessageType.Info);
        }
        
        public static void SetDictionaryEditorPrefs(Dictionaries dictionaries)
        {
            foreach (KeyValue keyValue in dictionaries.keyValues)
            {
                if (!HasKey("float " + keyValue.Uid()))
                    SetBool("float " + keyValue.Uid(), false);
                if (!HasKey("int " + keyValue.Uid()))
                    SetBool("int " + keyValue.Uid(), false);
                if (!HasKey("bool " + keyValue.Uid()))
                    SetBool("bool " + keyValue.Uid(), false);
                if (!HasKey("string " + keyValue.Uid()))
                    SetBool("string " + keyValue.Uid(), false);
                if (!HasKey("Animation " + keyValue.Uid()))
                    SetBool("Animation " + keyValue.Uid(), false);
                if (!HasKey("Texture2D " + keyValue.Uid()))
                    SetBool("Texture2D " + keyValue.Uid(), false);
                if (!HasKey("Sprite " + keyValue.Uid()))
                    SetBool("Sprite " + keyValue.Uid(), false);
                if (!HasKey("AudioClip " + keyValue.Uid()))
                    SetBool("AudioClip " + keyValue.Uid(), false);
                if (!HasKey("Prefab " + keyValue.Uid()))
                    SetBool("Prefab " + keyValue.Uid(), false);
                if (!HasKey("Vector3 " + keyValue.Uid()))
                    SetBool("Vector3 " + keyValue.Uid(), false);
                if (!HasKey("Vector2 " + keyValue.Uid())) 
                    SetBool("Vector2 " + keyValue.Uid(), false);
                if (!HasKey("Color " + keyValue.Uid()))
                    SetBool("Color " + keyValue.Uid(), false);
                if (!HasKey("LearnedStatOrSkill " + keyValue.Uid())) 
                    SetBool("LearnedStatOrSkill " + keyValue.Uid(), false);
                if (!HasKey("StatOrSkill " + keyValue.Uid()))
                    SetBool("StatOrSkill " + keyValue.Uid(), false);
                if (!HasKey("Item " + keyValue.Uid()))
                    SetBool("Item " + keyValue.Uid(), false);
            }
        }

        public static void ShowDictionaryItem(Object items, KeyValue keyValue)
        {
            if (!GetBool("Expand " + keyValue.Uid()))
                return;

            StartRow();
            Label("", 25);
            Label("", 200);
            BackgroundColor(GetBool("Item " + keyValue.Uid()) ? Color.white : dark);
            if (Button("Item",
                "Toggle whether this type is used for this keyValue pair", 200))
            {
                Undo.RecordObject(items, "Undo");
                SetBool("Item " + keyValue.Uid(), !GetBool("Item " + keyValue.Uid()));
            }

            ResetColor();

            EndRow();
        }
        
        public static void ShowDictionaryLearnedStatOrSkills(Object items, KeyValue keyValue)
        {
            if (!GetBool("Expand " + keyValue.Uid()))
                return;

            StartRow();
            Label("", 25);
            Label("", 200);
            BackgroundColor(GetBool("LearnedStatOrSkill " + keyValue.Uid()) ? Color.white : dark);
            if (Button("Learned StatOrSkill",
                "Toggle whether this type is used for this keyValue pair", 200))
            {
                Undo.RecordObject(items, "Undo");
                SetBool("LearnedStatOrSkill " + keyValue.Uid(), !GetBool("LearnedStatOrSkill " + keyValue.Uid()));
            }

            ResetColor();

            EndRow();
        }
        
        public static void ShowDictionaryStatOrSkills(Object items, KeyValue keyValue)
        {
            if (!GetBool("Expand " + keyValue.Uid()))
                return;

            StartRow();
            Label("", 25);
            Label("", 200);
            BackgroundColor(GetBool("StatOrSkill " + keyValue.Uid()) ? Color.white : dark);
            if (Button("StatOrSkill",
                "Toggle whether this type is used for this keyValue pair", 200))
            {
                Undo.RecordObject(items, "Undo");
                SetBool("StatOrSkill " + keyValue.Uid(), !GetBool("StatOrSkill " + keyValue.Uid()));
            }

            ResetColor();

            EndRow();
        }
        
        public static void AddToAllDictionaries(KeyValue newKeyValue, List<Dictionaries> allDictionaries, Object Item)
        {
            if (newKeyValue == null)
                return;

            if (allDictionaries == null)
                return;
            
            foreach (Dictionaries d in allDictionaries)
            {
                Undo.RecordObject(Item, "Undo");
                KeyValue newKeyValueItem = d.AddNewKeyValue(newKeyValue.key);
                newKeyValueItem.SetUid(newKeyValue.Uid());

                // Set Dirty
                EditorUtility.SetDirty(Item);
            }
        }
        
        public static void ShowDictionaryVector2(KeyValue keyValue, Object Item)
        {
            if (!GetBool("Expand " + keyValue.Uid()))
                return;
            
            StartRow();
            Label("", 25);
            Label("", 200);
            BackgroundColor(GetBool("Vector2 " + keyValue.Uid()) ? Color.white : dark);
            if (Button("Vector2", "Toggle whether this type is used for this keyValue pair", 200))
            {
                Undo.RecordObject(Item, "Undo");
                SetBool("Vector2 " + keyValue.Uid(), !GetBool("Vector2 " + keyValue.Uid()));
            }

            ResetColor();

            EndRow();
        }

        public static void ShowDictionaryVector3(KeyValue keyValue, Object Item)
        {
            if (!GetBool("Expand " + keyValue.Uid()))
                return;
            
            StartRow();
            Label("", 25);
            Label("", 200);
            BackgroundColor(GetBool("Vector3 " + keyValue.Uid()) ? Color.white : dark);
            if (Button("Vector3", "Toggle whether this type is used for this keyValue pair", 200))
            {
                Undo.RecordObject(Item, "Undo");
                SetBool("Vector3 " + keyValue.Uid(), !GetBool("Vector3 " + keyValue.Uid()));
            }

            ResetColor();

            EndRow();
        }

        public static void ShowDictionaryColor(KeyValue keyValue, Object Item)
        {
            if (!GetBool("Expand " + keyValue.Uid()))
                return;
            
            StartRow();
            Label("", 25);
            Label("", 200);
            BackgroundColor(GetBool("Color " + keyValue.Uid()) ? Color.white : dark);
            if (Button("Color",
                "Toggle whether this type is used for this keyValue pair", 200))
            {
                Undo.RecordObject(Item, "Undo");
                SetBool("Color " + keyValue.Uid(), !GetBool("Color " + keyValue.Uid()));
            }

            ResetColor();

            EndRow();
        }

        public static void ShowDictionaryPrefab(KeyValue keyValue, Object Item)
        {
            if (!GetBool("Expand " + keyValue.Uid()))
                return;
            
            StartRow();
            Label("", 25);
            Label("", 200);
            BackgroundColor(GetBool("Prefab " + keyValue.Uid()) ? Color.white : dark);
            if (Button("Prefab",
                "Toggle whether this type is used for this keyValue pair", 200))
            {
                Undo.RecordObject(Item, "Undo");
                SetBool("Prefab " + keyValue.Uid(), !GetBool("Prefab " + keyValue.Uid()));
            }

            ResetColor();

            EndRow();
        }

        public static void ShowDictionaryAudioClip(KeyValue keyValue, Object Item)
        {
            if (!GetBool("Expand " + keyValue.Uid()))
                return;
            
            StartRow();
            Label("", 25);
            Label("", 200);
            BackgroundColor(GetBool("AudioClip " + keyValue.Uid()) ? Color.white : dark);
            if (Button("AudioClip", "Toggle whether this type is used for this keyValue pair", 200))
            {
                Undo.RecordObject(Item, "Undo");
                SetBool("AudioClip " + keyValue.Uid(),
                    !GetBool("AudioClip " + keyValue.Uid()));
            }

            ResetColor();

            EndRow();
        }

        public static void ShowDictionaryTexture2D(KeyValue keyValue, Object Item)
        {
            if (!GetBool("Expand " + keyValue.Uid()))
                return;
            
            StartRow();
            Label("", 25);
            Label("", 200);
            BackgroundColor(GetBool("Texture2D " + keyValue.Uid()) ? Color.white : dark);
            if (Button("Texture2D", "Toggle whether this type is used for this keyValue pair", 200))
            {
                Undo.RecordObject(Item, "Undo");
                SetBool("Texture2D " + keyValue.Uid(),
                    !GetBool("Texture2D " + keyValue.Uid()));
            }

            ResetColor();

            EndRow();
        }
        
        public static void ShowDictionarySprite(KeyValue keyValue, Object Item)
        {
            if (!GetBool("Expand " + keyValue.Uid()))
                return;
            
            StartRow();
            Label("", 25);
            Label("", 200);
            BackgroundColor(GetBool("Sprite " + keyValue.Uid()) ? Color.white : dark);
            if (Button("Sprite", "Toggle whether this type is used for this keyValue pair", 200))
            {
                Undo.RecordObject(Item, "Undo");
                SetBool("Sprite " + keyValue.Uid(),
                    !GetBool("Sprite " + keyValue.Uid()));
            }

            ResetColor();

            EndRow();
        }

        public static void ShowDictionaryAnimation(KeyValue keyValue, Object Item)
        {
            if (!GetBool("Expand " + keyValue.Uid()))
                return;
            
            StartRow();
            Label("", 25);
            Label("", 200);
            BackgroundColor(GetBool("Animation " + keyValue.Uid()) ? Color.white : dark);
            if (Button("Animation", "Toggle whether this type is used for this keyValue pair", 200))
            {
                Undo.RecordObject(Item, "Undo");
                SetBool("Animation " + keyValue.Uid(), !GetBool("Animation " + keyValue.Uid()));
            }

            ResetColor();

            EndRow();
        }

        public static void ShowDictionaryString(KeyValue keyValue, Object Item)
        {
            if (!GetBool("Expand " + keyValue.Uid()))
                return;
            
            StartRow();
            Label("", 25);
            Label("", 200);

            BackgroundColor(GetBool("string " + keyValue.Uid()) ? Color.white : dark);
            if (Button("string", "Toggle whether this type is used for this keyValue pair", 200))
            {
                Undo.RecordObject(Item, "Undo");
                SetBool("string " + keyValue.Uid(), !GetBool("string " + keyValue.Uid()));
            }

            ResetColor();

            EndRow();
        }

        public static void ShowDictionaryBool(KeyValue keyValue, Object Item)
        {
            if (!GetBool("Expand " + keyValue.Uid()))
                return;
            
            StartRow();
            Label("", 25);
            Label("", 200);

            BackgroundColor(GetBool("bool " + keyValue.Uid()) ? Color.white : dark);
            if (Button("bool", "Toggle whether this type is used for this keyValue pair", 200))
            {
                Undo.RecordObject(Item, "Undo");
                SetBool("bool " + keyValue.Uid(), !GetBool("bool " + keyValue.Uid()));
            }

            ResetColor();

            EndRow();
        }

        public static void ShowDictionaryInt(KeyValue keyValue, Dictionaries dictionaries, List<Dictionaries> allDictionaries, Object Item)
        {
            if (!GetBool("Expand " + keyValue.Uid()))
                return;
            
            StartRow();
            BackgroundColor(red);
            if (Button("X", 25))
            {
                if (Dialog("Delete Custom Key/Value?", "Do you really want to delete " + keyValue.key +
                                                       "? This will remove it from all of the items, prefixes, and suffixes."))
                {
                    RemoveFromAllDictionaries(keyValue.key, dictionaries, allDictionaries, Item);
                    ExitGUI();
                }
            }

            ResetColor();
            Label("", 200);

            BackgroundColor(GetBool("int " + keyValue.Uid()) ? Color.white : dark);
            if (Button("int", "Toggle whether this type is used for this keyValue pair", 200))
            {
                Undo.RecordObject(Item, "Undo");
                SetBool("int " + keyValue.Uid(), !GetBool("int " + keyValue.Uid()));
            }

            ResetColor();

            EndRow();
        }

        public static void ShowDictionaryFloat(KeyValue keyValue, Dictionaries dictionaries, List<Dictionaries> allDictionaries, Object Item)
        {
            // WARNING THIS IS ALSO THE FIRST ROW!!!
            
            StartRow();
            if (Button(GetBool("Expand " + keyValue.Uid()) ? "-" : "+", 25))
            {
                Undo.RecordObject(Item, "Undo");
                SetBool("Expand " + keyValue.Uid(), !GetBool("Expand " + keyValue.Uid()));
            }

            string tempNameDictionary = keyValue.key;
            keyValue.key = DelayedText(keyValue.key, 200);
            if (tempNameDictionary != keyValue.key)
            {
                if (!ChangeNameDictionary(tempNameDictionary, keyValue.key, dictionaries, allDictionaries, Item))
                    keyValue.key = tempNameDictionary;
            }

            if (GetBool("Expand " + keyValue.Uid()))
            {
                BackgroundColor(GetBool("float " + keyValue.Uid()) ? Color.white : dark);
                if (Button("float", "Toggle whether this type is used for this keyValue pair", 200))
                {
                    Undo.RecordObject(Item, "Undo");
                    SetBool("float " + keyValue.Uid(), !GetBool("float " + keyValue.Uid()));
                }

                ResetColor();
            }

            EndRow();
        }
        
        private static void RemoveFromAllDictionaries(string key, Dictionaries dictionaries, List<Dictionaries> allDictionaries, Object Item)
        {
            dictionaries.RemoveKey(key);
            
            if (allDictionaries == null)
                return;
            
            foreach (Dictionaries d in allDictionaries)
            {
                Undo.RecordObject(Item, "Undo");
                d.RemoveKey(key);
            }
        }
        
        private static bool ChangeNameDictionary(string oldName, string newName, Dictionaries dictionaries, List<Dictionaries> allDictionaries, Object Item)
        {
            if (dictionaries.Count(newName) > 1)
                return false;

            if (allDictionaries == null)
                return true;
            
            for (int i = 0; i < allDictionaries.Count; i++)
            {
                Undo.RecordObject(Item, "Undo");
                allDictionaries[i].RenameKey(oldName, newName);
            }

            return true;
        }

        public static void MatchParentDictionary(Dictionaries child, Dictionaries parent)
        {
            // Remove any keyValues that exist here but do not exist on the main statsOrSkills object.
            foreach (KeyValue keyValue in child.keyValues)
            {
                if (!parent.HasKeyValue(keyValue.Uid()))
                {
                    child.RemoveKey(keyValue.key);
                    ExitGUI();
                }
            }
        }
    }
}
