using System.Collections.Generic;
using Buildings;
using HUD.Tooltip;
using Items;
using Systems.Crafting.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Building_Details.Scripts
{
    public class CraftingOrdersBuildingPanel : BuildingPanel
    {
        [SerializeField] private Toggle _acceptNewOrdersToggle;
        [SerializeField] private Toggle _prioritizeOrdersMatsToggle;
        [SerializeField] private GameObject _craftingLayout; // If nothing is being crafted or queued, hide
        
        [Header("Current Production")]
        [SerializeField] private TooltipTrigger _curProdTooltip;
        [SerializeField] private Image _curProdIcon;
        [SerializeField] private Image _curProdMatsBar;
        [SerializeField] private Image _curProdWorkBar;
        [SerializeField] private Image _curProdBG;
        [SerializeField] private Sprite _defaultBG;
        [SerializeField] private Sprite _redBG;
        [SerializeField] private GameObject _curProdDecreasePriorityBtn;

        [Header("Queue")] 
        [SerializeField] private Transform _queueParent;
        [SerializeField] private CraftQueueItem _craftQueueItemPrefab;

        private List<CraftQueueItem> _displayedQueueItems = new List<CraftQueueItem>();
        private CraftingBuilding _craftingBuilding => _building as CraftingBuilding;
        private CraftingTable _craftingTable => _craftingBuilding.CraftingTable;
        
        protected override void Show()
        {
            Refresh();
        }

        protected override void Refresh()
        {
            _acceptNewOrdersToggle.SetIsOnWithoutNotify(_craftingBuilding.AcceptNewOrders);
            _prioritizeOrdersMatsToggle.SetIsOnWithoutNotify(_craftingBuilding.PrioritizeOrdersWithMats);

            RefreshCurrentInProd();
            RefreshQueue();
            RefreshLayout();
        }
        
        public override void Hide()
        {
            base.Hide();
        }

        private void RefreshCurrentInProd()
        {
            var curCraftingOrder = _craftingBuilding.CurrentCraftingOrder;
            if (curCraftingOrder?.CraftedItem == null)
            {
                _craftingLayout.SetActive(false);
            }
            else
            {
                _craftingLayout.SetActive(true);
                _curProdIcon.sprite = curCraftingOrder.CraftedItem.ItemSprite;
                _curProdMatsBar.fillAmount = _craftingTable.GetPercentMaterialsReceived();
                _curProdWorkBar.fillAmount = _craftingTable.GetPercentCraftingComplete();

                _curProdTooltip.Header = curCraftingOrder.CraftedItem.ItemName;
                _curProdTooltip.Content = curCraftingOrder.CraftedItem.MaterialsList;

                if (curCraftingOrder.State == CraftingOrder.EOrderState.Queued)
                {
                    _curProdBG.sprite = curCraftingOrder.CanBeCrafted(_building) ? _defaultBG : _redBG;
                }
                else
                {
                    _curProdBG.sprite = _defaultBG;
                }
                
                if (curCraftingOrder.State == CraftingOrder.EOrderState.Queued && _craftingBuilding.QueuedOrders().Count > 0)
                {
                    _curProdDecreasePriorityBtn.SetActive(true);
                }
                else
                {
                    _curProdDecreasePriorityBtn.SetActive(false);
                }
            }
        }

        private void RefreshQueue()
        {
            ClearDisplayedQueueItems();
            
            var queuedOrders = _craftingBuilding.QueuedOrders();
            foreach (var queuedOrder in queuedOrders)
            {
                var queuedItem = Instantiate(_craftQueueItemPrefab, _queueParent);
                queuedItem.Init(queuedOrder, _craftingBuilding);
                _displayedQueueItems.Add(queuedItem);
            }
        }

        private void ClearDisplayedQueueItems()
        {
            foreach (var displayedQueueItem in _displayedQueueItems)
            {
                Destroy(displayedQueueItem.gameObject);
            }
            _displayedQueueItems.Clear();
        }

        public void OnCurProdDecreasePriorityPressed()
        {
            if (_craftingBuilding.CurrentCraftingOrder != null)
            {
                _craftingBuilding.ReturnCurrentOrderToQueue();
                GameEvents.Trigger_OnBuildingChanged(_building);
            }
        }

        public void OnAcceptNewOrdersToggleChanged(bool value)
        {
            _craftingBuilding.AcceptNewOrders = value;
        }

        public void OnPrioritizeOrdersWithMatsToggleChanged(bool value)
        {
            _craftingBuilding.PrioritizeOrdersWithMats = value;
        }
    }
}
