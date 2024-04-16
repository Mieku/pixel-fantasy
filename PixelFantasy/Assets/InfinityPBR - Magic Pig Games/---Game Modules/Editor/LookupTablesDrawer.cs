using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static InfinityPBR.Modules.GameModuleUtilities;
using static InfinityPBR.Modules.Utilities;


namespace InfinityPBR.Modules
{
    public class LookupTablesDrawer : GameModulesDrawer
    {

        // -------------------------------
        // PROPERTIES AND VARIABLES -- UPDATE THESE FOR THIS TYPE
        // -------------------------------
        private LookupTable _modulesObject;
        const string ThisTypePlural = "Lookup Tables";
        const string ThisType = "LookupTable";
        private string ClassNamePlural => "LookupTables";
        private string ClassName => "LookupTable";
        private string DocsURL => "https://infinitypbr.gitbook.io/infinity-pbr/game-modules/lookup-table";
        private string DocsURLLabel => "Lookup Table";
        private string[] GameModuleObjectTypes(bool recompute = false) => GameModuleObjectTypes<LookupTable>(recompute);
        private LookupTable[] GameModuleObjects(bool recompute = false) => GameModuleObjects<LookupTable>(recompute);
        private LookupTable[] GameModuleObjectsOfType(string type, bool recompute = false) => GameModuleObjectsOfType<LookupTable>(type, recompute);
        // -------------------------------
        
        // -------------------------------
        // REQUIRED - NO UPDATE NEEDED
        // -------------------------------
        private Vector2 _scrollPosition;
        private int _fieldWidth;
        private DictionariesDrawer _dictionariesDrawer;
        private StatModificationLevelDrawer _statModificationLevelDrawer;
        private ItemAttributeDrawer _itemAttributeDrawer;
        private ConditionDrawer _conditionDrawer;
        private string[] _menuBarOptions;
        public bool drawnByGameModulesWindow = true;
        
        //private int _lootItemsIndex = 0;
        //private List<LootItems> _cachedLootItems = new List<LootItems>();
        //private List<string> _cachedLootItemsNames = new List<string>();
        //private List<LootItems> _activeLootItems = new List<LootItems>();
        // -------------------------------

        // -------------------------------
        // METHODS -- UPDATE THESE FOR THIS TYPE
        // -------------------------------
        public void SetModulesObject(LookupTable modulesObject, bool drawnByWindow = true)
        {
            _modulesObject = modulesObject;
            _menuBarOptions = new[] { 
                "Settings", 
                "Stats Effects",
                "Dictionaries" 
            };
            _fieldWidth = 200;
            _dictionariesDrawer = new DictionariesDrawerEditor(_fieldWidth);
            _conditionDrawer = new ConditionDrawerEditor(_fieldWidth);
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

            DrawLinkToDocs();

            BeginChangeCheck();
            
            // If this is a duplicate uid and the button is clicked to reset it, redo Initial Setup
            if (!drawnByGameModulesWindow && CheckUid(_modulesObject.Uid(), _modulesObject, _duplicateUid))
                _initialSetup = false;

            
            if (!drawnByGameModulesWindow)
            {
                LabelGrey("Lookup Table - " + _modulesObject.objectType);
                LabelBig(_modulesObject.objectName, 18, true);
            }

            

            BlackLine();
            Undo.RecordObject(_modulesObject, "Undo");
            ShowCurves();
            BlackLine();
            Undo.RecordObject(_modulesObject, "Undo");
            ShowAddValue();
            Undo.RecordObject(_modulesObject, "Undo");
            ShowTable();
            
            EndChangeCheck(_modulesObject);
            EditorUtility.SetDirty(this);
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

        private void ShowCurves()
        {
            EnsureAtLeastThreeRows();
            
            StartRow();
            _modulesObject.showCurves = OnOffButton(_modulesObject.showCurves);
            Label($"Curves {symbolInfo}", "Use curves to set the Input & Output values.", 150);
            if (_modulesObject.useInputCurve)
                Label("<color=#777777>Input</color> <color=#77cc77><b>On</b></color>", 80,false,false,true);
            else
                Label("<color=#777777>Input</color> <color=#cc7777><b>Off</b></color>", 80,false,false,true);
            if (_modulesObject.useOutputCurve)
                Label("<color=#777777>Output</color> <color=#77cc77><b>On</b></color>", 80,false,false,true);
            else
                Label("<color=#777777>Output</color> <color=#cc7777><b>Off</b></color>", 80,false,false,true);
            EndRow();

            if (_modulesObject.showCurves)
            {
                StartRow();
                ShowInputCurve();
                ShowOutputCurve();
                EndRow();
                StartRow();
                Label("Add Rows: ", 60);
                AddManyQuantity = Int(AddManyQuantity, 40);
                if (Button("Add", 40))
                {
                    var lastInput = GetLastInput();
                    for (var i = 0; i <= AddManyQuantity; i++)
                    {
                        _modulesObject.Add(lastInput, 0);
                        lastInput += 1;
                    }
                }
                EndRow();
            }
        }

        private void ShowInputCurve()
        {
            _modulesObject.inputCurve = CheckCurve(_modulesObject.inputCurve);
            StartVertical();
            var newValue = LeftCheck("Use Input Curve", _modulesObject.useInputCurve);
            
            StartRow();
            Label("Min: ", 30);
            _modulesObject.inputCurveMin = Float(_modulesObject.inputCurveMin, 40);
            Label("Max: ", 30);
            _modulesObject.inputCurveMax = Float(_modulesObject.inputCurveMax, 40);
            Label("Decimals: ", 60);
            _modulesObject.inputDecimals = Int(_modulesObject.inputDecimals, 40);
            EndRow();
            
            if (!_modulesObject.useInputCurve && newValue)
            {
                if (Dialog("Are you sure?", "Changing this will reset the table."))
                    _modulesObject.useInputCurve = true;
            }
            else
                _modulesObject.useInputCurve = newValue;
            _modulesObject.inputCurve = Curve(_modulesObject.inputCurve,-1f, 0, 50);
            EndVertical();
            
            UpdateInputOnCurve();
        }
        
        private void ShowOutputCurve()
        {
            _modulesObject.outputCurve = CheckCurve(_modulesObject.outputCurve);
            StartVertical();
            var newValue = LeftCheck("Use Output Curve", _modulesObject.useOutputCurve);
            
            StartRow();
            Label("Min: ", 30);
            _modulesObject.outputCurveMin = Float(_modulesObject.outputCurveMin, 40);
            Label("Max: ", 30);
            _modulesObject.outputCurveMax = Float(_modulesObject.outputCurveMax, 40);
            Label("Decimals: ", 60);
            _modulesObject.outputDecimals = Int(_modulesObject.outputDecimals, 40);
            EndRow();
            
            if (!_modulesObject.useOutputCurve && newValue)
            {
                if (Dialog("Are you sure?", "Changing this will reset the table."))
                    _modulesObject.useOutputCurve = true;
            }
            else
                _modulesObject.useOutputCurve = newValue;
            _modulesObject.outputCurve = Curve(_modulesObject.outputCurve,-1f, 0, 50);
            EndVertical();

            UpdateOutputOnCurve();
        }

        private void UpdateInputOnCurve()
        {
            if (!_modulesObject.useInputCurve) return;

            var values = GenerateEvenlySpacedValuesFromCurve(
                    _modulesObject.inputCurve,
                    _modulesObject.inputCurveMin,
                    _modulesObject.inputCurveMax,
                    _modulesObject.table.Count,
                    _modulesObject.inputDecimals)
                .ToList();
            
            for (var i = 0; i < _modulesObject.table.Count; i++)
                _modulesObject.table[i].input = values[i];
        }
        
        private void UpdateOutputOnCurve()
        {
            if (!_modulesObject.useOutputCurve) return;
            
            var values = GenerateEvenlySpacedValuesFromCurve(
                    _modulesObject.outputCurve,
                    _modulesObject.outputCurveMin,
                    _modulesObject.outputCurveMax,
                    _modulesObject.table.Count,
                    _modulesObject.outputDecimals)
                .ToList();
            
            for (var i = 0; i < _modulesObject.table.Count; i++)
                _modulesObject.table[i].output = values[i];
        }


        private AnimationCurve CheckCurve(AnimationCurve curve)
        {
            if (curve.length != 0) return curve;
            
            Keyframe startKey = new Keyframe(0, 0);
            Keyframe endKey = new Keyframe(1, 1);
            
            return new AnimationCurve(startKey, endKey);
        }

        private void EnsureAtLeastThreeRows()
        {
            if (_modulesObject.table.Count > 2 || (!_modulesObject.useInputCurve && !_modulesObject.useOutputCurve)) return;
            
            var lastInput = GetLastInput();
            for (var i = 0; i < 4 - _modulesObject.table.Count; i++)
            {
                _modulesObject.Add(lastInput, 0);
                lastInput += 1;
            }
        }

        private float GetLastInput() => _modulesObject.table.Select(table => table.input).Prepend(0f).Max();

        private void ShowAddValue()
        {
            if (_modulesObject.useInputCurve || _modulesObject.useOutputCurve)
            {
                Label("<color=#777777>Additional Add Value options unavailable when using Curves</color>", true, false, true);
                return;
            }
            var isOn = _modulesObject.showAddValue;

            StartRow();
            _modulesObject.showAddValue = OnOffButton(_modulesObject.showAddValue);
            Label($"Add Values {symbolInfo}", "Add single Input/Output values, or a range", 150);
            EndRow();

            if (isOn)
            {
                Undo.RecordObject(_modulesObject, "Undo");
                ShowAddOne();
                BlackLine();
                Undo.RecordObject(_modulesObject, "Undo");
                ShowAddMany();
            }
            BlackLine();
        }

        
        private void ShowAddMany()
        {
            StartRow();
            Label("Add Many: ", 100, true);
            
            Label("Quantity", 65);
            AddManyQuantity = Int(AddManyQuantity, 40);
            Label("", 15);
            
            Label("Start", 65);
            AddManyStart = Int(AddManyStart, 40);
            Label("", 15);
            
            Label("Increment", 65);
            AddManyIncrement = Int(AddManyIncrement, 40);
            Label("", 15);
            EndRow();
            
            StartRow();
            Label("", 100);
            Label("Output Min", 65);
            OutputMin = Int(OutputMin, 40);
            Label("", 15);
            
            Label("Output Max", 65);
            OutputMax = Int(OutputMax, 40);
            Label("", 15);
            
            Label("Round to", 65);
            AddManyRound = Int(AddManyRound, 40);
            Label("", 15);
            EndRow();

            if (AddManyIncrement < 1)
                AddManyIncrement = 1;
            if (AddManyQuantity < 1)
                AddManyQuantity = 1;
            
            StartRow();
            Label("", 100);
            Colors(Color.yellow, Color.white);

            if (Button($"Add {AddManyQuantity}", 365))
            {
                var inputs = GenerateIncrementalValues(AddManyStart, AddManyIncrement, AddManyQuantity).ToList();
                var outputs = GenerateEvenlySpacedValues(OutputMin, OutputMax,
                    AddManyQuantity, AddManyRound).ToList();
                
                var runningValue = AddManyStart;
                for (int i = 0; i < AddManyQuantity; i++)
                {
                    if (_modulesObject.Add(inputs[i], outputs[i]))
                    {
                        _modulesObject.table[^1].manualOrder = _modulesObject.table.Count - 1;
                    }
                    else
                    {
                        Debug.Log($"Already had an input for {runningValue}.");
                    }
                    runningValue += AddManyIncrement;
                }
                
                if (_modulesObject.orderBy != LookupTable.LookupTableOrder.Shuffle)
                    _modulesObject.Reorder();
            }
            ResetColor();
            EndRow();
        }


        private int AddManyRound
        {
            get => SessionState.GetInt($"Lookup Table Round To {ThisType}", 0);
            set => SessionState.SetInt($"Lookup Table Round To {ThisType}", value);
        }
        
        private int AddManyQuantity
        {
            get => SessionState.GetInt($"Lookup Table Add Quantity {ThisType}", 0);
            set => SessionState.SetInt($"Lookup Table Add Quantity {ThisType}", value);
        }
        
        private int OutputMin
        {
            get => SessionState.GetInt($"Lookup Table Output Max {ThisType}", 0);
            set => SessionState.SetInt($"Lookup Table Output Max {ThisType}", value);
        }
        
        private int OutputMax
        {
            get => SessionState.GetInt($"Lookup Table Output Min {ThisType}", 0);
            set => SessionState.SetInt($"Lookup Table Output Min {ThisType}", value);
        }
        private int AddManyStart
        {
            get => SessionState.GetInt($"Lookup Table Add Start {ThisType}", 0);
            set => SessionState.SetInt($"Lookup Table Add Start {ThisType}", value);
        }
        private int AddManyIncrement
        {
            get => SessionState.GetInt($"Lookup Table Add Increment {ThisType}", 1);
            set => SessionState.SetInt($"Lookup Table Add Increment {ThisType}", value);
        }

        private void ShowAddOne()
        {
            StartRow();
            Label("Add One: ", 100, true);
            Label("Input", 65);
            AddInput = Float(AddInput, 40);
            Label("", 15);
            
            Label("Output", 65);
            AddOutput = Float(AddOutput, 40);
            Label("", 15);
            
            ShowAddButton();
            EndRow();
        }
        
        private float AddInput
        {
            get => SessionState.GetFloat($"Lookup Table Add Input {ThisType}", 0);
            set => SessionState.SetFloat($"Lookup Table Add Input {ThisType}", value);
        }
        
        private float AddOutput
        {
            get => SessionState.GetFloat($"Lookup Table Add Output {ThisType}", 0);
            set => SessionState.SetFloat($"Lookup Table Add Output {ThisType}", value);
        }

        private void ShowAddButton()
        {
            var hasInput = _modulesObject.table.FirstOrDefault(x => Math.Abs(x.input - AddInput) < 0.001) != null;
            ColorsIf(hasInput, Color.black, Color.yellow, Color.grey, Color.white);
            if (Button("Add", 100))
            {
                if (!hasInput)
                {
                    Undo.RecordObject(_modulesObject, "Undo");
                    if (_modulesObject.Add(AddInput, AddOutput))
                    {
                        _modulesObject.table[^1].manualOrder = _modulesObject.table.Count - 1;
                        if (_modulesObject.orderBy != LookupTable.LookupTableOrder.Shuffle)
                            _modulesObject.Reorder();
                        AddInput = AddInput + 1;
                        return;
                    }

                    ShowWarning();
                }
            }
            ResetColor();
            if (hasInput)
            {
                Label($"<color=#777777>Already has input</color> <color=#999999><b>{AddInput}</b></color>", false, false, true);
            }
        }

        private void ShowWarning()
        {
            Debug.LogWarning($"There is already an input with value {AddInput}");
        }
        
        private void ShowTable()
        {
            if (_modulesObject.table.Count == 0) return;

            Undo.RecordObject(_modulesObject, "Undo");
            OrderTableButton();
            Space();

            StartRow();
            Label("", 25);
            Label("Input", 100, true);
            Label("Output", 100, true);
            EndRow();

            for (var index = 0; index < _modulesObject.table.Count; index++)
            {
                var values = _modulesObject.table[index];
                if (_modulesObject.table.Count(x => Math.Abs(x.input - values.input) < 0.001) > 1)
                    BackgroundColor(Color.red);

                StartRow();
                if (_modulesObject.orderBy == LookupTable.LookupTableOrder.Manual 
                    && !_modulesObject.useInputCurve
                    && !_modulesObject.useOutputCurve)
                {
                    // Move Up Button
                    ColorsIf(CanMoveUp(index, _modulesObject.table.Count)
                        , Color.white
                        , Color.black
                        , Color.black
                        , Color.grey);
                    if (Button(symbolArrowUp, 25) && CanMoveUp(index, _modulesObject.table.Count))
                    {
                        MoveItemUp(_modulesObject.table, index);
                        _modulesObject.SaveManualOrder();
                    }
                
                    // Move Down Button
                    ColorsIf(CanMoveDown(index, _modulesObject.table.Count)
                        , Color.white
                        , Color.black
                        , Color.black
                        , Color.grey);
                    if (Button(symbolArrowDown, 25) && CanMoveDown(index, _modulesObject.table.Count))
                    {
                        MoveItemDown(_modulesObject.table, index);
                        _modulesObject.SaveManualOrder();
                    }
                    ContentColor(Color.white);
                }

                BackgroundColor(Color.white);
                Undo.RecordObject(_modulesObject, "Undo");
                values.input = Float(values.input, 100);
                Undo.RecordObject(_modulesObject, "Undo");
                values.output = Float(values.output, 100);
                if (XButton())
                {
                    Undo.RecordObject(_modulesObject, "Undo delete");
                    _modulesObject.RemoveLookupTable(values.input);
                    _modulesObject.SaveManualOrder();
                    ExitGUI();
                }

                EndRow();
            }
        }

        private void OrderTableButton()
        {
            if (_modulesObject.useInputCurve || _modulesObject.useOutputCurve)
            {
                Label("<color=#777777>Cannot reorder when using curves</color>", true, false, true);
                return;
            }
            StartRow();
            Label("Order By: ", 80);
            var tempOrder = _modulesObject.orderBy;
            _modulesObject.orderBy = (LookupTable.LookupTableOrder) EnumPopup(_modulesObject.orderBy, 150);
            if (tempOrder != _modulesObject.orderBy)
                _modulesObject.Reorder();
            EndRow();
        }
    }
}