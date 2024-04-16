using System;
using System.Linq;
using UnityEngine;
using static InfinityPBR.Modules.Utilities;
using static InfinityPBR.Modules.GameModuleUtilities;

namespace InfinityPBR.Modules
{
    public class ItemObjectRewardDrawer : QuestRewardDrawer
    {
        public override bool CanHandle(QuestReward questReward) 
            => questReward is ItemObjectReward;

        protected override void ShowSpecificData(QuestReward questReward) 
            => ShowDataForItemObjectReward(questReward as ItemObjectReward);

        protected override void OnEnable() => DoCache();

        protected override void DoCache()
        {
            // Item Object Reward
            GameModuleObjects<LootBox>(true);
            _lootBoxTypes = GameModuleObjectTypes<LootBox>(true);

            if (string.IsNullOrWhiteSpace(_selectedType) && AvailableLootBoxTypes > 0)
            {
                _selectedType = _lootBoxTypes[0];
                CacheSelectedLootBoxType();
            }

            _availableLookupTables = LookupTableArray();
            _availableLookupTablesNames = _availableLookupTables.Select(x => x.name).ToArray();
            //_madeCache = true;
        }
        
        //private bool _madeCache = false;
        private LookupTable[] _availableLookupTables;
        private string[] _availableLookupTablesNames;
        
        // Item Object Reward Caching
        //private LootBox[] _lootBoxes;
        private LootBox[] _lootBoxesOfSelectedType;
        private LootBox[] LootBoxesOfType(string type) => GameModuleObjects<LootBox>().Where(x => x.objectType == type).OrderBy(x => x.objectName).ToArray();

        private string[] LootBoxesOfSelectedTypeNames =>
            _lootBoxesOfSelectedType.Select(x => x.objectName).ToArray();

        private LootBox _selectedLootBox;
        private int _selectedLootBoxIndex;
        private string[] _lootBoxTypes = Array.Empty<string>();
        private string _selectedType = "";
        private int _selectedTypeIndex;

        private int AvailableLootBoxTypes => _lootBoxTypes.Length;
        
        private void ShowDataForItemObjectReward(ItemObjectReward itemObjectReward)
        {
            Label("Item Object Rewards", true);
            Label("Item Object rewards are driven by the Loot module. Create a Loot Box for each reward " +
                  "you would like to provide.", false, true);
            ContentColor(Color.yellow);
            Label("IMPORTANT: You will need to add a custom class to the QuestRepository which implements " +
                  "IHandleQuestRewards and specify how you would like to handle the GameItemObjectList which results" +
                  " from this reward.", true, true);
            ResetColor();
            Space();
            ShowLootBoxes(itemObjectReward);
            Space();

            ShowAddLootBox(itemObjectReward);
        }

        private void ShowAddLootBox(ItemObjectReward itemObjectReward)
        {
            // Must have some available
            if (AvailableLootBoxTypes == 0) 
                return;

            // Ensure we don't have too few for the index
            if (_selectedTypeIndex >= _lootBoxTypes.Length)
                _selectedTypeIndex = _lootBoxTypes.Length - 1;
            
            BackgroundColor(Color.yellow);
            StartVerticalBox();
            StartRow();
            // Show the popup & update if needed
            var cachedTypeIndex = _selectedTypeIndex;
            Label("Select LootBox", 100);
            _selectedTypeIndex = Popup(_selectedTypeIndex, _lootBoxTypes, 120);
            if (cachedTypeIndex != _selectedTypeIndex)
            {
                _selectedType = _lootBoxTypes[_selectedTypeIndex];
                CacheSelectedLootBoxType();
                _selectedLootBoxIndex = 0;
            }

            if (_selectedLootBoxIndex >= LootBoxesOfSelectedTypeNames.Length)
                _selectedLootBoxIndex = LootBoxesOfSelectedTypeNames.Length - 1;
            _selectedLootBoxIndex = Popup(_selectedLootBoxIndex, LootBoxesOfSelectedTypeNames, 120);

            if (Button("Add", 50))
                itemObjectReward.AddLootBox(_lootBoxesOfSelectedType[_selectedLootBoxIndex]);
            
            EndRow();
            EndVerticalBox();
            ResetColor();
        }
        
        private void CacheSelectedLootBoxType() => _lootBoxesOfSelectedType = LootBoxesOfType(_selectedType);

        private void ShowLootBoxes(ItemObjectReward itemObjectReward)
        {
            for (var index = 0; index < itemObjectReward.lootBoxes.Count; index++)
            {
                var lootBox = itemObjectReward.lootBoxes[index];
                StartRow();
                BackgroundColor(Color.red);
                if (Button(symbolX, 25))
                {
                    itemObjectReward.RemoveLootBox(index);
                    ExitGUI();
                }
                ResetColor();
                Label(lootBox.objectName, 150);
                Object(itemObjectReward.lootBoxes[index], typeof(LootBox), 150);
                ResetColor();
                EndRow();
            }
        }
    }
}