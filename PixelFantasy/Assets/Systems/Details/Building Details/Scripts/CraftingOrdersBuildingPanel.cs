using System.Collections.Generic;
using System.Linq;
using Buildings;
using HUD.Tooltip;
using Items;
using ScriptableObjects;
using Systems.Crafting.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Building_Details.Scripts
{
    public class CraftingOrdersBuildingPanel : BuildingPanel
    {
        [SerializeField] private Toggle _prioritizeOrdersMatsToggle;
        
        [Header("Current Production")]
        [SerializeField] private TooltipTrigger _curProdTooltip;
        [SerializeField] private Image _curProdIcon;
        [SerializeField] private Image _curProdMatsBar;
        [SerializeField] private Image _curProdWorkBar;
        [SerializeField] private Image _curProdBG;
        [SerializeField] private Sprite _defaultBG;
        [SerializeField] private Sprite _redBG;
        [SerializeField] private GameObject _curProdDecreasePriorityBtn;
        [SerializeField] private CraftingOrderControlPanel _craftingOrderControlPanel;
        [SerializeField] private Button _addOrderBtn;
        [SerializeField] private GameObject _inProgessDetails;
        [SerializeField] private GameObject _nothingProgessDetails;

        [Header("Queue")] 
        [SerializeField] private Transform _queueParent;
        [SerializeField] private CraftQueueItem _craftQueueItemPrefab;

        private List<CraftQueueItem> _displayedQueueItems = new List<CraftQueueItem>();
        private ICraftingBuilding _craftingBuilding => _building as ICraftingBuilding;
        private bool _craftingOrderControlsOpen;
        
        protected override void Show()
        {
            Refresh();
        }

        protected override void Refresh()
        {
            _prioritizeOrdersMatsToggle.SetIsOnWithoutNotify(_craftingBuilding.PrioritizeOrdersWithMats);

            RefreshCurrentInProd();
            RefreshQueue();
            RefreshLayout();

            _addOrderBtn.interactable = _craftingBuilding.CraftingOptions.Count != 0;
            HideCraftingOrderControlPanel();
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
                _nothingProgessDetails.SetActive(true);
                _inProgessDetails.SetActive(false);
                _curProdDecreasePriorityBtn.SetActive(false);
            }
            else
            {
                var craftingTable = _craftingBuilding.FindCraftingTable(curCraftingOrder.CraftedItem);
                _nothingProgessDetails.SetActive(false);
                _inProgessDetails.SetActive(true);
                _curProdIcon.sprite = curCraftingOrder.CraftedItem.ItemSprite;
                _curProdMatsBar.fillAmount = craftingTable.GetPercentMaterialsReceived();
                _curProdWorkBar.fillAmount = craftingTable.GetPercentCraftingComplete();

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
                bool isFirst = queuedOrders.First() == queuedOrder;
                bool isLast = queuedOrders.LastOrDefault() == queuedOrder;
                
                var queuedItem = Instantiate(_craftQueueItemPrefab, _queueParent);
                queuedItem.Init(queuedOrder, _craftingBuilding, isFirst, isLast);
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

        public void OnPrioritizeOrdersWithMatsToggleChanged(bool value)
        {
            _craftingBuilding.PrioritizeOrdersWithMats = value;
        }

        public void AddOrderPressed()
        {
            if (_craftingOrderControlsOpen)
            {
                HideCraftingOrderControlPanel();
            }
            else
            {
                ShowCraftingOrderControlPanel();
            }
        }

        private void ShowCraftingOrderControlPanel()
        {
            _craftingOrderControlsOpen = true;
            _craftingOrderControlPanel.gameObject.SetActive(true);
            _craftingOrderControlPanel.Init(_craftingBuilding, AddCraftingOrder);
        }

        private void HideCraftingOrderControlPanel()
        {
            _craftingOrderControlsOpen = false;
            _craftingOrderControlPanel.gameObject.SetActive(false);
        }

        public void AddCraftingOrder(CraftedItemData itemToCraft)
        {
            _craftingBuilding.CreateLocalOrder(itemToCraft);
            
            RefreshCurrentInProd();
            RefreshQueue();
            RefreshLayout();
        }
    }
}
