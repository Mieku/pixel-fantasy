using System;
using System.Collections.Generic;
using Buildings;
using Buildings.Building_Panels;
using Items;
using ScriptableObjects;
using TMPro;
using UnityEngine;

namespace Systems.Details.Building_Details.Scripts
{
    public class CraftingOrderControlPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _itemTitleText;
        [SerializeField] private TextMeshProUGUI _itemDetailsText;
        [SerializeField] private Transform _optionsLayout;
        [SerializeField] private CraftingOrderOption _orderOptionPrefab;
        [SerializeField] private Transform _resouceCostsLayout;
        [SerializeField] private ResourceCost _resourceCostPrefab;
        
        
        private CraftingTable _craftingTable;
        private CraftedItemSettings _selectedItem;
        private List<CraftingOrderOption> _displayedOptions = new List<CraftingOrderOption>();
        private List<ResourceCost> _displayedCosts = new List<ResourceCost>();
        private Action<CraftedItemSettings> _addOrderCallback;
        
        public void Init(CraftingTable craftingTable, Action<CraftedItemSettings> addOrderCallback)
        {
            _craftingTable = craftingTable;
            _addOrderCallback = addOrderCallback;
            RefreshOptions();
            SelectOrder(_craftingTable.GetCraftingOptions()[0]);
        }

        private void RefreshOptions()
        {
            ClearOptions();
            _orderOptionPrefab.gameObject.SetActive(false);
            var options = _craftingTable.GetCraftingOptions();
            foreach (var option in options)
            {
                var orderOption = Instantiate(_orderOptionPrefab, _optionsLayout);
                orderOption.gameObject.SetActive(true);
                orderOption.Init(option, SelectOrder);
                _displayedOptions.Add(orderOption);
            }
        }

        private void ClearOptions()
        {
            foreach (var displayedOption in _displayedOptions)
            {
                Destroy(displayedOption.gameObject);
            }
            _displayedOptions.Clear();
        }

        private void SelectOrder(CraftedItemSettings item)
        {
            foreach (var displayedOption in _displayedOptions)
            {
                displayedOption.ShowSelection(false);
            }

            _selectedItem = item;
            _displayedOptions.Find(option => option.Item == item).ShowSelection(true);
            RefreshResourceCosts(_selectedItem);

            _itemTitleText.text = _selectedItem.ItemName;
            _itemDetailsText.text = _selectedItem.GetDetailsMsg("#272736");
        }

        private void RefreshResourceCosts(CraftedItemSettings craftedItemSettings)
        {
            _resourceCostPrefab.gameObject.SetActive(false);
            foreach (var cost in _displayedCosts)
            {
                Destroy(cost.gameObject);
            }
            _displayedCosts.Clear();

            var itemCosts = craftedItemSettings.CraftRequirements.GetResourceCosts();
            foreach (var itemCost in itemCosts)
            {
                var cost = Instantiate(_resourceCostPrefab, _resouceCostsLayout);
                cost.gameObject.SetActive(true);
                cost.Init(itemCost);
                _displayedCosts.Add(cost);
            }
        }

        public void AddOrderPressed()
        {
            _addOrderCallback.Invoke(_selectedItem);
        }
    }
}
