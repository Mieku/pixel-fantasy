using System;
using System.Collections.Generic;
using System.Linq;
using Buildings;
using Items;
using Managers;
using ScriptableObjects;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Building_Details.Scripts
{
    public class StockpileCategoryDetails : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _categoryNameText;
        [SerializeField] private TextMeshProUGUI _categoryAmountText;
        [SerializeField] private Image _collapseIcon;
        [SerializeField] private Sprite _openSprite, _closedSprite;
        [SerializeField] private GameObject _contentHandle;
        [SerializeField] private ItemStockpileDisplay _stockpileDisplayPrefab;

        private EItemCategory _category;
        private IStockpileBuilding _building;
        private CategoryDisplayState _state;
        private List<ItemStockpileDisplay> _displayedStockpileItems = new List<ItemStockpileDisplay>();
        private StockpileBuildingPanel _stockpileBuildingPanel;

        public void Init(EItemCategory category, Building building, StockpileBuildingPanel parentPanel)
        {
            _category = category;
            _building = building as IStockpileBuilding;
            _stockpileBuildingPanel = parentPanel;
            
            SetState(CategoryDisplayState.Open);
            _categoryNameText.text = _category.GetDescription();
            CreateDisplays();
            Refresh();
        }

        public void Refresh()
        {
            // Refresh Values
            int storageUsed = _building.GetStorageUsedForCategory(_category);
            int maxStorage = _building.GetMaxStorageForCategory(_category);
            _categoryAmountText.text = $"{storageUsed} / {maxStorage}";

            foreach (var itemDisplay in _displayedStockpileItems)
            {
                int amountStored = _building.AmountItemStored(itemDisplay.ItemData);
                bool isAllowed = _building.IsItemStockpileAllowed(itemDisplay.ItemData);
                itemDisplay.RefreshValues(amountStored, isAllowed);
            }
        }

        private void CreateDisplays()
        {
            // Get all possible items in alphabetical order
            var allItemsForCategory = Librarian.Instance.GetAllItemsForCategory(_category, true);

            // Get all the building's stored items
            var allStoredItems = _building.GetStoredItemsByCategory(_category);
            List<ItemAmount> itemsToDisplay = new List<ItemAmount>();

            foreach (var itemForCategory in allItemsForCategory)
            {
                int quantity = 0;
                var storedAmount = allStoredItems.Find(i => i.Item == itemForCategory);
                if (storedAmount != null)
                {
                    quantity = storedAmount.Quantity;
                }
                
                ItemAmount amount = new ItemAmount
                {
                    Item = itemForCategory,
                    Quantity = quantity
                };
                
                itemsToDisplay.Add(amount);
            }
            
            // Sort the stocked items in order of quantity, the no quantity ones are in alphabetical
            var sortedList = itemsToDisplay.OrderByDescending(i => i.Quantity).ToList();

            foreach (var sortedItem in sortedList)
            {
                bool isAllowed = _building.IsItemStockpileAllowed(sortedItem.Item);
                var itemDisplay = Instantiate(_stockpileDisplayPrefab, _contentHandle.transform);
                itemDisplay.Init(sortedItem, isAllowed, OnAllowStockpileChanged);
                _displayedStockpileItems.Add(itemDisplay);
            }
        }

        private void OnAllowStockpileChanged(ItemData itemData, bool isAllowed)
        {
            _building.SetAllowedStockpileItem(itemData, isAllowed);
        }
        
        public void OnPressed()
        {
            if (_state == CategoryDisplayState.Closed)
            {
                SetState(CategoryDisplayState.Open);
            }
            else
            {
                SetState(CategoryDisplayState.Closed);
            }
        }

        private void SetState(CategoryDisplayState newState)
        {
            _state = newState;
            switch (_state)
            {
                case CategoryDisplayState.Closed:
                    Enter_Closed();
                    break;
                case CategoryDisplayState.Open:
                    Enter_Open();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
            }
        }

        private void Enter_Closed()
        {
            _collapseIcon.sprite = _closedSprite;
            _contentHandle.SetActive(false);
            RefreshLayout();
        }

        private void Enter_Open()
        {
            _collapseIcon.sprite = _openSprite;
            _contentHandle.SetActive(true);
            RefreshLayout();
        }

        private void RefreshLayout()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            _stockpileBuildingPanel.RefreshLayout();
        }

        public enum CategoryDisplayState
        {
            Closed,
            Open,
        }
    }
}
