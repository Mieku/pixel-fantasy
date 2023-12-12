using System.Collections.Generic;
using Buildings;
using HUD.Tooltip;
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
        
        protected override void Show()
        {
            Refresh();
        }

        protected override void Refresh()
        {
            _acceptNewOrdersToggle.SetIsOnWithoutNotify(_craftingBuilding.AcceptNewOrders);
            _prioritizeOrdersMatsToggle.SetIsOnWithoutNotify(_craftingBuilding.PrioritizeOrdersWithMats);
        }
        
        public override void Hide()
        {
            base.Hide();
        }

        public void OnCurProdDecreasePriorityPressed()
        {
            
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
