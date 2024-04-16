using System.Linq;
using UnityEngine;
using static InfinityPBR.Modules.GameModuleUtilities;

namespace InfinityPBR.Modules
{
    public class GameConditionListContainsDrawer : QuestConditionDrawer
    {
        private string[] _conditionTypes;
        private string[] ConditionTypes => _conditionTypes;
        private string[] _availableTypes;
        private string[] AvailableTypes => _availableTypes;
        private bool _cachedAvailableTypes;
        private int _selectedTypeIndex;

        protected void OnEnable()
        {
            _conditionTypes = GameModuleObjectTypes<Condition>();
        }
        
        public override bool CanHandle(QuestCondition questCondition) 
            => questCondition is GameConditionListContains;

        protected override void ShowSpecificData(QuestCondition questCondition) 
            => ShowData(questCondition as GameConditionListContains);

        private void ShowData(GameConditionListContains questCondition)
        {
            CacheAvailableTypes(questCondition);
            
            ShowValueContents(questCondition);

            if (questCondition.valueContents 
                is QuestCondition.ValueContents.IsEmpty 
                or QuestCondition.ValueContents.IsNotEmpty)
                return;

            ShowQuantity(questCondition);
            ShowGrouping(questCondition);
            Space();
            ShowAspect(questCondition);
            Space();
            ShowTypes(questCondition);
            ShowConditions(questCondition);
        }

        private void CacheAvailableTypes(GameConditionListContains questCondition, bool force = false)
        {
            if (_cachedAvailableTypes && !force && questCondition != null) return;

            _availableTypes = ConditionTypes.Except(questCondition.types).ToArray();
            _cachedAvailableTypes = true;
        }

        private void ShowTypes(GameConditionListContains questCondition)
        {
            if (questCondition.gameModuleAspect != QuestCondition.GameModuleAspect.Type) return;

            if (ConditionTypes.Length == 0)
            {
                ContentColor(Color.red);
                Label("No types are available to add");
                ResetColor();
                return;
            }
            
            if (AvailableTypes.Length == 0)
                LabelGrey("All types have been added");
            else
            {
                if (_selectedTypeIndex >= AvailableTypes.Length)
                    _selectedTypeIndex = 0;

                StartRow();
                BackgroundColor(Color.yellow);
                _selectedTypeIndex = Popup(_selectedTypeIndex, AvailableTypes, 200);
                if (Button("Add", 50))
                {
                    questCondition.types.Add(AvailableTypes[_selectedTypeIndex]);
                    CacheAvailableTypes(questCondition, true);
                }

                ResetColor();
                EndRow();
            }
            
            if (questCondition.types.Count == 0)
                LabelGrey("No types added: Will compare against the Count of all conditions in the list.");

            DrawTypes(questCondition);
        }

        private void ShowConditions(GameConditionListContains questCondition)
        {
            if (questCondition.gameModuleAspect != QuestCondition.GameModuleAspect.Item) return;
            
            StartRow();
            Label($"Add Condition {symbolInfo}", "Will be looking for GameCondition versions of these in the GameConditionsList.", 120);
            DrawAddCondition(questCondition);
            EndRow();

            if (questCondition.value.Count == 0)
                LabelGrey("No conditions added: Will compare against the Count of all conditions in the list.");

            DrawValues(questCondition);
        }

        private void ShowAspect(GameConditionListContains questCondition)
        {
            StartRow();
            questCondition.gameModuleAspect = Toolbar(questCondition.gameModuleAspect, 300);
            EndRow();
        }

        private void ShowGrouping(GameConditionListContains questCondition)
        {
            if (questCondition.value.Count <= 1) return;
            
            StartRow();
            Label($"Grouping {symbolInfo}", "Determines if any of the values in the list, sum of all in total, or each " +
                                            "individually must match the quantity comparison.\n\n" +
                                            "Any: At least one of the items in the list must meet the comparison " +
                                            "requirements." +
                                            "\n\nSum: The total sum of all items combined must meet the comparison requirements." +
                                            "\n\nEach: Each item in the list must individually meet the requirements.", 120);
            questCondition.groupOfObjects = (QuestCondition.GroupOfObjects) EnumPopup(questCondition.groupOfObjects, 200);
            EndRow();
        }

        private void ShowQuantity(GameConditionListContains questCondition)
        {
            StartRow();
            Label($"Quantity {symbolInfo}", "The quantity used in the comparison.", 120);
            questCondition.quantity = Int(questCondition.quantity, 200);
            EndRow();
        }

        private void ShowValueContents(GameConditionListContains questCondition)
        {
            Label("Game Condition List", true);
            StartRow();
            Label($"Value Contents {symbolInfo}", "This is the criteria used to determine if the Quest Condition has been met.", 120);
            questCondition.valueContents = (QuestCondition.ValueContents) EnumPopup(questCondition.valueContents, 200);
            EndRow();
        }

        private void DrawAddCondition(GameConditionListContains questCondition)
        {
            BackgroundColor(Color.yellow);
            var newValue = Object(null, typeof(Condition), 200) as Condition;
            if (newValue != null)
            {
                if (questCondition.value.Contains(newValue))
                    return;
                
                questCondition.value.Add(newValue);
            }
            ResetColor();
        }

        private void DrawValues(GameConditionListContains questCondition)
        {
            foreach (var obj in questCondition.value)
            {
                StartRow();
                if (XButton())
                {
                    questCondition.value.Remove(obj);
                    ExitGUI();
                }

                Object(obj, typeof(Condition), 200);
                EndRow();
            }
        }
        
        private void DrawTypes(GameConditionListContains questCondition)
        {
            foreach (var objectType in questCondition.types)
            {
                StartRow();
                if (XButton())
                {
                    questCondition.types.Remove(objectType);
                    CacheAvailableTypes(questCondition, true);
                    ExitGUI();
                }

                TextField(objectType, 200);
                EndRow();
            }
        }
    }
}