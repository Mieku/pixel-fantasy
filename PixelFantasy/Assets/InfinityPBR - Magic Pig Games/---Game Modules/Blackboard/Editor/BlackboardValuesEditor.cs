using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static InfinityPBR.Modules.GameModuleUtilities;

namespace InfinityPBR.Modules
{
    [CustomEditor(typeof(BlackboardValues))]
    [CanEditMultipleObjects]
    [Serializable]
    public class BlackboardValuesEditor : ModulesScriptableObjectEditor
    {
        private BlackboardValues _this;
        
        private int fieldWidth = 200;
        private DictionariesDrawer dictionariesDrawer;
        private StatModificationLevelDrawer statModificationLevelDrawer;

        private string _newTopic = "Topic";
        private string _newSubject = "Subject";

        protected override void Setup()
        {
            _this = (BlackboardValues) target;
            dictionariesDrawer = new DictionariesDrawerEditor(fieldWidth);
            statModificationLevelDrawer = new StatModificationLevelDrawerEditor(fieldWidth);
        }

        protected override void Header()
        {
            BeginChangeCheck();
            Validations();
            ShowHeader(_this, _this.Uid());
            EndChangeCheck();
        }

        protected override void Draw()
        {
            if (!InitialSetup()) return;
            
            LabelGrey("Blackboard Values - " + _this.objectType);
            LabelBig(_this.objectName, 18, true);

            BeginChangeCheck();

            ShowInfo();
            Space();

            ShowOptions();
            Space();

            ShowAllValues();

            ShowAddNew();
           
            Footer();

            CheckConflicts();

            EndChangeCheck();
            EditorUtility.SetDirty(_this);
        }

        private void ShowOptions()
        {
            if (Button("Order Alphabetically", 200))
            {
                _this.startingNotes = _this.startingNotes.OrderBy(x => x.name).ToList();
            }
        }


        private void ShowAddNew()
        {
            StartRow();
            _newTopic = TextField(_newTopic, 150);
            _newSubject = TextField(_newSubject, 150);

            bool nameIsDuplicate = NameIsDuplicate(_newTopic, _newSubject, 0);
            Colors(nameIsDuplicate ? Color.red : Color.yellow, Color.white);
            if (Button($"{(nameIsDuplicate ? "Duplicate Name: " : "Add Blackboard Note ")}\"{_newTopic} - {_newSubject}\"", 250) && !nameIsDuplicate)
            {
                _this.startingNotes.Add(new BlackboardNote(_newTopic, _newSubject));
                ExitGUI();
            }
            ResetColor();
            EndRow();
        }

        private void ShowInfo()
        {
            StartVerticalBox();
            Label("Define starting notes in this and other \"Blackboard Values\" objects. Attach these to the Blackboard prefab in your scene, and " +
                  "they will be added to the blackboard at runtime as Blackboard Notes, where you and the quest system can access them. The Quest system will also be " +
                  "able to utilize these Blackboard Notes to determine the results of active Quests.\n\n" +
                  "As these are very flexible objects, you will need to create scripts to update the values as needed.", false, true);
            EndVerticalBox();
        }

        private void ShowAllValues() => _this.startingNotes.ForEach(x => ShowValues(x));

        private void ShowValues(BlackboardNote blackboardNote)
        {
            StartVerticalBox();
            ShowTitleAndExpander(blackboardNote);
            if (!ShowNote(blackboardNote))
            {
                EndVerticalBox();
                return;
            }
            
            Space();
            
            ShowExposedTypes(blackboardNote);
            Space();
            
            ShowData(blackboardNote);

            ShowDelete(blackboardNote);
            
            EndVerticalBox();
        }

        private void ShowDelete(BlackboardNote blackboardNote)
        {
            Colors(Color.red, Color.white);
            if (Button("Remove Note", 120))
            {
                if (Dialog($"Delete {blackboardNote.name}?", $"Deleting {blackboardNote.name} will remove it from any " +
                                                             "Quest which utilizes it."))
                {
                    _this.startingNotes.Remove(blackboardNote);
                    ExitGUI();
                }
            }
            ResetColor();
        }

        private void ShowExposedTypes(BlackboardNote blackboardNote)
        {
            if (!ShowNote(blackboardNote)) return;
            
            StartRow();
            var key = $"{blackboardNote.name}-ExposedTypes";
            bool show = GetBool(key);
            ColorsIf(show, Color.white, Color.black, Color.white, Color.grey);
            SetBool(key, ButtonToggle(show, $"{(show ? symbolCircleOpen : symbolDash)}", 25));
            ResetColor();
            Label("Exposed Data Types", "Toggle on/off each datatype. If off, the data will still exist on the Blackboard Note object, but " +
                                                   "it will not be visible in the various custom inspectors. It can still be manipulated at runtime.", 200, true);
            EndRow();
            if (!GetBool(key)) return;
            
            StartRow();
            StartVertical();
            Label($"General Types {symbolInfo}", "You can set the initial value of these types in this inspector.", 140);
            TypeToggle(blackboardNote, "string");
            TypeToggle(blackboardNote, "int");
            TypeToggle(blackboardNote, "float");
            TypeToggle(blackboardNote, "bool");
            TypeToggle(blackboardNote, "GameObject");
            EndVertical();
            
            StartVertical();
            Label($"Module Types {symbolInfo}", "These can only be set at runtime. Exposing them here allows other inspectors, such as the " +
                                                "\"Quest\" module, to utilize these data types.", 140);
            TypeToggle(blackboardNote, "Game Stat");
            TypeToggle(blackboardNote, "Game Item Object");
            TypeToggle(blackboardNote, "Game Item Attribute");
            TypeToggle(blackboardNote, "Game Condition");
            //TypeToggle(blackboardNote, "Game Quest"); // OCTOBER 10 - -Will be added with the Quests Module
            EndVertical();
            
            StartVertical();
            Label("", 140);
            TypeToggle(blackboardNote, "Loot Box");
            TypeToggle(blackboardNote, "Game Stat List");
            TypeToggle(blackboardNote, "Game Item Object List");
            TypeToggle(blackboardNote, "Game Condition List");
            //TypeToggle(blackboardNote, "Game Quest List"); // OCTOBER 10 - -Will be added with the Quests Module
            EndVertical();
            EndRow();
        }

        private void TypeToggle(BlackboardNote blackboardNote, string dataType)
        {
            ColorsIf(GetBool($"{blackboardNote.name} {dataType}"), Color.white, Color.black, Color.white, Color.grey);
            SetBool($"{blackboardNote.name} {dataType}",
                ButtonToggle(GetBool($"{blackboardNote.name} {dataType}"), dataType, 140));
            ResetColor();
        }

        private void ShowData(BlackboardNote blackboardNote)
        {
            if (!ShowNote(blackboardNote)) return;

            StartRow();
            //Label("", 25);
            Label($"Data Type {symbolInfo}", "Toggle on or off this datatype. If off, the data will still exist on the Blackboard Note object, but " +
                                             "it will not be visible in the various custom inspectors. It can still be manipulated at runtime.", 100, true);
            Label($"Data Value {symbolInfo}",
                "This is the starting value. If the Blackboard Note already exists on the blackboard, this value will not replace " +
                "the existing value.", 150, true);
            EndRow();

            ShowDataString(blackboardNote, "string");
            ShowDataString(blackboardNote, "int");
            ShowDataString(blackboardNote, "float");
            ShowDataString(blackboardNote, "bool");
            ShowDataString(blackboardNote, "GameObject");
        }

        private void ShowDataString(BlackboardNote blackboardNote, string dataType)
        {
            if (!GetBool($"{blackboardNote.name} {dataType}")) return;
            
            StartRow();
            Label(dataType, 100);
            
            // Show Values
            if (dataType == "string") blackboardNote.valueString = TextField(blackboardNote.valueString, 150);
            if (dataType == "int") blackboardNote.valueInt = Int(blackboardNote.valueInt, 100);
            if (dataType == "float") blackboardNote.valueFloat = Float(blackboardNote.valueFloat, 100);
            if (dataType == "bool") blackboardNote.valueBool = Check(blackboardNote.valueBool, 25);
            if (dataType == "GameObject") blackboardNote.valueGameObject = Object(blackboardNote.valueGameObject, typeof(GameObject), 150) as GameObject;

            EndRow();
        }
        
        private bool ShowNote(BlackboardNote blackboardNote) => GetBool(blackboardNote.name);

        private void ShowTitleAndExpander(BlackboardNote blackboardNote)
        {
            StartRow();
            ButtonOpenClose(blackboardNote.name, ShowNote(blackboardNote));
            Label(blackboardNote.name, true);
            EndRow();

            if (!ShowNote(blackboardNote)) return;

            var tempTopic = blackboardNote.topic;
            var tempSubject = blackboardNote.subject;
            
            StartRow();
            Label($"Topic {symbolInfo}",
                "This is the overall topic of the note, often something in a category like \"Player\" or \"Kills\", or \"Doors\".",
                60);
            blackboardNote.topic = DelayedText(blackboardNote.topic, 150);
            if (tempTopic != blackboardNote.topic)
            {
                blackboardNote.name = $"{blackboardNote.topic} - {blackboardNote.subject}";
                if (NameIsDuplicate(blackboardNote))
                {
                    Debug.Log("Duplicate name for Blackboard Note. Each note should be a single \"Topic\" and \"Subject\"");
                    blackboardNote.topic = tempTopic;
                }
                SetNoteName(blackboardNote);
            }
            EndRow();
            StartRow();
            Label($"Subject {symbolInfo}",
                "This is the specific subject of the note, often unique in a like \"Meters Moved\" or \"Orcs\", or \"Dungeon Secret Door\".",
                60);
            blackboardNote.subject = DelayedText(blackboardNote.subject, 150);
            if (tempSubject != blackboardNote.subject)
            {
                blackboardNote.name = $"{blackboardNote.topic} - {blackboardNote.subject}";
                if (NameIsDuplicate(blackboardNote))
                {
                    Debug.Log("Duplicate name for Blackboard Note. Each note should be a single \"Topic\" and \"Subject\"");
                    blackboardNote.subject = tempSubject;
                }
                SetNoteName(blackboardNote);
            }
            EndRow();
        }

        private void SetNoteName(BlackboardNote blackboardNote, bool setBool = true){
            
            blackboardNote.name = $"{blackboardNote.topic} - {blackboardNote.subject}";
            SetBool(blackboardNote.name, setBool);
        }
        private bool NameIsDuplicate(BlackboardNote blackboardNote, int min = 1) => GameModuleObjects<BlackboardValues>().SelectMany(x => x.startingNotes).Count(y => y.name == $"{blackboardNote.topic} - {blackboardNote.subject}") > min;
        private bool NameIsDuplicate(string topic, string subject, int min = 1) => GameModuleObjects<BlackboardValues>().SelectMany(x => x.startingNotes).Count(y => y.name == $"{topic} - {subject}") > min;

        private void ShowButtons()
        {
            StartRow();
            BackgroundColor(Color.white);
            EndRow();
        }

        private void CheckConflicts()
        {
            if (!_this) return;
            if (_this.dictionaries?.keyValues == null) return;
        }

        private void SetEditorWindowWidth()
        {
            
        }
        
        private void ShowHeader(string value) => LabelBig(value, 200, 18, true);
        
        public void ShowDictionary<T>(Dictionaries dictionaries, string objectName, string objectType, T Object) where T : ModulesScriptableObject
        {
            Undo.RecordObject(Object, "Undo Value Changes");
            dictionaries.Draw(dictionariesDrawer, "Quest", objectName, objectType);
        }

        private void Footer()
        {
            ShowFooter(Script);
            if (GetBool("Show full inspector " + Script.name))
                DrawDefaultInspector();
        }

        private void Validations()
        {
            
        }

        private bool InitialSetup()
        {
            return true;
        }
    }
}