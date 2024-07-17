using System;
using System.Collections.Generic;
using Managers;
using Systems.Crafting.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Generic_Details.Scripts
{
    public class CraftingQueueEntry : MonoBehaviour
    {
        [SerializeField] private GameObject _increasePriorityArrow, _decreasePriorityArrow, _playPauseBtn, _deleteBtn;
        [SerializeField] private Image _itemIcon;
        [SerializeField] private Image _pausePlayIcon;
        [SerializeField] private Sprite _pauseSpr, _playSpr;
        [SerializeField] private Image _progressFill;
        [SerializeField] private Image _materialFill;
        [SerializeField] private TMP_Dropdown _fulfillmentDropdown;
        [SerializeField] private TMP_InputField _amountInput;
        [SerializeField] private GameObject _amountHandle;
        [SerializeField] private GameObject _inProgressIcon;

        private CraftingOrder _order;
        private CraftingOrderQueue _queue;
        private Action _refreshCallback;
        
        public void Init(CraftingOrder order, CraftingOrderQueue queue, Action refreshCallback)
        {
            _order = order;
            _queue = queue;
            _refreshCallback = refreshCallback;
            
            _itemIcon.sprite = order.GetOrderedItemSettings.ItemSprite;
            
            _increasePriorityArrow.SetActive(!queue.IsFirstInQueue(order));
            _decreasePriorityArrow.SetActive(!queue.IsLastInQueue(order));
            
            RefreshAmountInput();
            RefreshFulfillmentDropdown();
            RefreshPlayPause();
        }

        private void Update()
        {
            _materialFill.fillAmount = _order.GetPercentMaterialsReceived();
            _progressFill.fillAmount = _order.OrderProgress;
            
            _inProgressIcon.SetActive(_order.IsOrderInProgress());
        }

        #region Button Hooks

        public void IncreasePriorityPressed()
        {
            _queue.IncreaseOrderPriority(_order);
            _refreshCallback?.Invoke();
        }
        
        public void DecreasePriorityPressed()
        {
            _queue.DecreaseOrderPriority(_order);
            _refreshCallback?.Invoke();
        }

        public void OnPlayPausePressed()
        {
            _order.IsPaused = !_order.IsPaused;
            _refreshCallback?.Invoke();
            
            RefreshPlayPause();
        }

        private void RefreshPlayPause()
        {
            if (_order.IsPaused)
            {
                _pausePlayIcon.sprite = _playSpr;
            }
            else
            {
                _pausePlayIcon.sprite = _pauseSpr;
            }
        }

        public void OnDeletePressed()
        {
            _queue.CancelOrder(_order);
            _refreshCallback?.Invoke();
        }

        public void FulfillmentDropdownChanged(int value)
        {
            _order.FulfillmentType = (CraftingOrder.EFulfillmentType)value;
            
            RefreshAmountInput();
        }

        private void RefreshFulfillmentDropdown()
        {
            _fulfillmentDropdown.ClearOptions();
            var options = new List<string>();
            foreach (var fulfillType in Enum.GetValues(typeof(CraftingOrder.EFulfillmentType)))
            {
                options.Add(fulfillType.ToString());
            }
            
            _fulfillmentDropdown.AddOptions(options);
            _fulfillmentDropdown.SetValueWithoutNotify((int)_order.FulfillmentType);
        }

        public void OnAmountInputChanged(string value)
        {
            int amount = int.Parse(value);
            amount = Mathf.Clamp(amount, 0, 999);
            _order.Amount = amount;
            
            _amountInput.SetTextWithoutNotify(amount.ToString());
        }

        public void OnInputSelected()
        {
            _amountInput.SetTextWithoutNotify(_order.Amount + "");
        }

        public void OnInputDeselected()
        {
            RefreshAmountInput();
        }

        private void RefreshAmountInput()
        {
            if (_order.FulfillmentType == CraftingOrder.EFulfillmentType.Forever)
            {
                _amountHandle.SetActive(false);
                return;
            }
            
            _amountHandle.SetActive(true);

            if (_order.FulfillmentType == CraftingOrder.EFulfillmentType.Until)
            {
                int availableAmount = InventoryManager.Instance.GetAmountAvailable(_order.ItemToCraftSettings);
                _amountInput.SetTextWithoutNotify($"{availableAmount}/{ _order.Amount}");
            }
            else
            {
                _amountInput.SetTextWithoutNotify(_order.Amount + "");
            }
        }

        public void OnIncreaseAmountPressed()
        {
            // Check if Ctrl is held down and set increaseAmount accordingly
            int increaseAmount = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ? 10 : 1;
            
            var amount = Mathf.Clamp(_order.Amount + increaseAmount, 0, 999);
            _order.Amount = amount;
            
            RefreshAmountInput();
        }

        public void OnDecreaseAmountPressed()
        {
            // Check if Ctrl is held down and set decreaseAmount accordingly
            int decreaseAmount = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ? 10 : 1;
            
            var amount = Mathf.Clamp(_order.Amount - decreaseAmount, 0, 999);
            _order.Amount = amount;
            
            RefreshAmountInput();
        }

        #endregion
    }
}
