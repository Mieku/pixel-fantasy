using System;
using System.Collections.Generic;
using Buildings;
using Buildings.Building_Panels;
using Managers;
using ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Building_Details.Scripts
{
    public class StorageCategoryDetails : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _categoryNameText;
        [SerializeField] private TextMeshProUGUI _categoryAmountText;
        [SerializeField] private Image _collapseIcon;
        [SerializeField] private Sprite _openSprite, _closedSprite;
        [SerializeField] private GameObject _contentHandle;
        [SerializeField] private ItemStorageDisplay _storageDisplayPrefab;

        private EItemCategory _category;
        private Building _building;
        private CategoryDisplayState _state;
        private List<ItemStorageDisplay> _displayedStorageItems = new List<ItemStorageDisplay>();
        private StorageBuildingPanel _storageBuildingPanel;
        private bool _rulesPanelOpen;
        
        public void Init(EItemCategory category, Building building, StorageBuildingPanel parentPanel)
        {
            _category = category;
            _building = building;
            _storageBuildingPanel = parentPanel;
            
            SetState(CategoryDisplayState.Open);
            _categoryNameText.text = _category.GetDescription();
            Refresh();
        }

        public void Refresh()
        {
            List<ItemStorageDisplay> _displaysToRemove = new List<ItemStorageDisplay>();
            var logiBills = _building.LogisticBills;
            
            // Refresh Values
            int storageUsed = _building.GetStorageUsedForCategory(_category);
            int maxStorage = _building.GetMaxStorageForCategory(_category);
            _categoryAmountText.text = $"{storageUsed} / {maxStorage}";

            foreach (var itemDisplay in _displayedStorageItems)
            {
                bool hasRule = false;
                int amountStored = _building.AmountItemStored(itemDisplay.ItemData);
                foreach (var logi in logiBills)
                {
                    if (logi.Item == itemDisplay.ItemData)
                    {
                        hasRule = true;
                        itemDisplay.RefreshValues(amountStored, logi.Value);
                        break;
                    }
                }

                if (!hasRule)
                {
                    if (amountStored == 0)
                    {
                        _displaysToRemove.Add(itemDisplay);
                    }
                    else
                    {
                        itemDisplay.RefreshValues(amountStored);
                    }
                }
            }
            
            // Remove unneeded
            foreach (var displayToRemove in _displaysToRemove)
            {
                _displayedStorageItems.Remove(displayToRemove);
                Destroy(displayToRemove.gameObject);
            }
            _displaysToRemove.Clear();
            
            var allStoredItems = _building.GetStoredItemsByCategory(_category);
            foreach (var storedItem in allStoredItems)
            {
                var equivDisplay = _displayedStorageItems.Find(display => display.ItemData == storedItem.Item);
                if (equivDisplay == null)
                {
                    InventoryLogisticBill equivLogiBill = logiBills.Find(bill => bill.Item == storedItem.Item);
                    if (equivLogiBill != null)
                    {
                        var itemDisplay = Instantiate(_storageDisplayPrefab, _contentHandle.transform);
                        itemDisplay.Init(storedItem.Item, storedItem.Quantity, equivLogiBill.Value);
                        _displayedStorageItems.Add(itemDisplay);
                    }
                    else
                    {
                        var itemDisplay = Instantiate(_storageDisplayPrefab, _contentHandle.transform);
                        itemDisplay.Init(storedItem.Item, storedItem.Quantity);
                        _displayedStorageItems.Add(itemDisplay);
                    }
                }
            }
            
            // Search for any missed bills and add them
            foreach (var logiBill in logiBills)
            {
                var equivStored = allStoredItems.Find(item => item.Item == logiBill.Item);
                if (equivStored == null)
                {
                    var itemDisplay = Instantiate(_storageDisplayPrefab, _contentHandle.transform);
                    itemDisplay.Init(logiBill.Item, 0, logiBill.Value);
                    _displayedStorageItems.Add(itemDisplay);
                }
            }
            
            // Move the rule button to be last
            _assignLogisticsBtn.transform.SetAsLastSibling();
        }
        
        public void OnPressed()
        {
            if(_rulesPanelOpen) return;
            
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
            _storageBuildingPanel.RefreshLayout();
        }

        #region Rules Panel

        [SerializeField] private GameObject _assignLogisticsBtn;
        [SerializeField] private TMP_InputField _amountInputField;
        [SerializeField] private StorageItemOption _storageItemOptionPrefab;
        [SerializeField] private Transform _itemOptionParent;
        [SerializeField] private GameObject _rulePanelHandle;
        [SerializeField] private Button _assignRuleBtn;
        [SerializeField] private Button _removeRuleBtn;
        [SerializeField] private GameObject _ruleControlsHandle;
        [SerializeField] private GameObject _selectItemHandle;

        private List<StorageItemOption> _displayedOptions = new List<StorageItemOption>();
        private ItemData _selectedOption;
        private const int DEFAULT_START_MAINTAIN_AMOUNT = 10;

        private void OpenRulesPanel()
        {
            _rulesPanelOpen = true;
            _rulePanelHandle.SetActive(true);
            
            ClearDisplayedOptions();
            DisplayOptions();
            SelectOption(null);
        }

        private void CloseRulesPanel()
        {
            _rulesPanelOpen = false;
            _rulePanelHandle.SetActive(false);
            
            ClearDisplayedOptions();
        }

        private void CreateRule()
        {
            var maintainValue = int.Parse(_amountInputField.text);
            var logiBills = _building.LogisticBills;
            var releventRule = logiBills.Find(bill => bill.Item == _selectedOption);
            if (releventRule != null)
            {
                // Check if a rule already exists and update it
                releventRule.Value = maintainValue;
            }
            else
            {
                // if doesn't, make a new rule
                var newLogiBill = new InventoryLogisticBill(InventoryLogisticBill.LogisticType.Maintain,
                    _selectedOption, maintainValue, _building);
                
                _building.AddLogisticBill(newLogiBill);
            }
        }

        private void RemoveRule()
        {
            if (_selectedOption == null)
            {
                Debug.LogError("Attempted to remove a rule for null item");
                return;
            }
            
            var logiBills = _building.LogisticBills;
            var releventRule = logiBills.Find(bill => bill.Item == _selectedOption);
            if (releventRule == null)
            {
                Debug.LogError("Could not find a relevent rule to remove");
                return;
            }
            else
            {
                _building.RemoveLogisticBill(releventRule);
            }
        }

        private void SelectOption(ItemData itemData)
        {
            _selectedOption = itemData;

            // Show Indicator
            foreach (var displayedOption in _displayedOptions)
            {
                displayedOption.SetSelected(displayedOption.ItemData == _selectedOption);
            }
            
            if (_selectedOption != null)
            {
                _ruleControlsHandle.SetActive(true);
                _selectItemHandle.SetActive(false);
                _assignRuleBtn.interactable = true;
                
                // Update values if has a rule
                var logiBills = _building.LogisticBills;
                var equivLogiBill = logiBills.Find(bill => bill.Item == _selectedOption);
                if (equivLogiBill != null)
                {
                    // Has a rule already
                    _removeRuleBtn.interactable = true;
                    _amountInputField.SetTextWithoutNotify(equivLogiBill.Value.ToString());
                }
                else
                {
                    // Doesn't already have a rule
                    _removeRuleBtn.interactable = false;
                    _amountInputField.SetTextWithoutNotify(DEFAULT_START_MAINTAIN_AMOUNT.ToString());
                }
            }
            else
            {
                _ruleControlsHandle.SetActive(false);
                _selectItemHandle.SetActive(true);
                _assignRuleBtn.interactable = false;
                _removeRuleBtn.interactable = false;
            }
        }

        private void DisplayOptions()
        {
            var allItemsForCategory = Librarian.Instance.GetAllItemsForCategory(_category, true);
            var logiBills = _building.LogisticBills;

            foreach (var catOption in allItemsForCategory)
            {
                var equivLogiBill = logiBills.Find(bill => bill.Item == catOption);
                var option = Instantiate(_storageItemOptionPrefab, _itemOptionParent);
                option.Init(catOption, false, equivLogiBill != null, OnItemOptionPressed);
                _displayedOptions.Add(option);
            }
        }

        private void ClearDisplayedOptions()
        {
            foreach (var displayedOption in _displayedOptions)
            {
                Destroy(displayedOption.gameObject);
            }
            _displayedOptions.Clear();
        }
        
        public void OnAddRulePressed()
        {
            OpenRulesPanel();
        }

        public void OnBlockerPressed()
        {
            CloseRulesPanel();
        }

        public void OnAssignPressed()
        {
            CreateRule();
            CloseRulesPanel();
            Refresh();
        }

        public void OnCancelPressed()
        {
            CloseRulesPanel();
        }

        public void OnRemoveRulePressed()
        {
            RemoveRule();
            CloseRulesPanel();
            Refresh();
        }

        public void OnIncreaseAmountPressed()
        {
            var curValue = _amountInputField.text;
            int newValue = int.Parse(curValue) + 1;
            _amountInputField.text = newValue.ToString();
        }

        public void OnDecreaseAmountPressed()
        {
            var curValue = _amountInputField.text;
            int newValue = int.Parse(curValue) - 1;
            if (newValue < 0)
            {
                newValue = 0;
            }
            
            _amountInputField.text = newValue.ToString();
        }

        public void OnInputValueChanged(string value)
        {
            
        }

        public void OnItemOptionPressed(ItemData itemData)
        {
            SelectOption(itemData);
        }
        
        #endregion
        
        
        public enum CategoryDisplayState
        {
            Closed,
            Open,
        }
    }
}
