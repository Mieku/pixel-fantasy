using System;
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
        [SerializeField] private TextMeshProUGUI _itemName;

        private CraftingOrder _order;
        private CraftingOrderQueue _queue;
        private Action _refreshCallback;
        
        public void Init(CraftingOrder order, CraftingOrderQueue queue, bool showIncreasePriority, bool showDecreasePriority, Action refreshCallback)
        {
            _order = order;
            _queue = queue;
            _refreshCallback = refreshCallback;
            
            _itemIcon.sprite = order.CraftedItem.ItemSprite;
            _itemName.text = order.CraftedItem.ItemName;
            
            _increasePriorityArrow.SetActive(showIncreasePriority);
            _decreasePriorityArrow.SetActive(showDecreasePriority);

            // Is current in production
            if (_queue == null)
            {
                _increasePriorityArrow.SetActive(false);
                _decreasePriorityArrow.SetActive(false);
                _playPauseBtn.SetActive(false);
                _deleteBtn.SetActive(false);
            }
        }

        private void Update()
        {
            _materialFill.fillAmount = _order.GetPercentMaterialsReceived();
            _progressFill.fillAmount = _order.OrderProgress;
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
            _refreshCallback?.Invoke();
        }

        public void OnDeletePressed()
        {
            _queue.CancelOrder(_order);
            _refreshCallback?.Invoke();
        }

        #endregion
    }
}
