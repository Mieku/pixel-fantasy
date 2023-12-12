using Buildings;
using HUD.Tooltip;
using Managers;
using Systems.Crafting.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Building_Details.Scripts
{
    public class CraftQueueItem : MonoBehaviour
    {
        [SerializeField] private TooltipTrigger _tooltip;
        [SerializeField] private Image _bg;
        [SerializeField] private GameObject _increaseBtn;
        [SerializeField] private GameObject _decreaseBtn;
        [SerializeField] private Image _itemIcon;
        [SerializeField] private Sprite _defaultBG;
        [SerializeField] private Sprite _redBG;

        private CraftingOrder _order;
        private CraftingBuilding _building;

        public void Init(CraftingOrder order, CraftingBuilding building)
        {
            _order = order;
            _building = building;

            _itemIcon.sprite = _order.CraftedItem.ItemSprite;
            _tooltip.Header = _order.CraftedItem.ItemName;
            _tooltip.Content = _order.CraftedItem.MaterialsList;

            bool isFirst = CraftingOrdersManager.Instance.IsFirstInQueue(_order);
            bool isLast  = CraftingOrdersManager.Instance.IsLastInQueue(_order);
            _increaseBtn.SetActive(!isFirst);
            _decreaseBtn.SetActive(!isLast);

            var matsAvailable = _order.AreMaterialsAvailable();
            _bg.sprite = matsAvailable ? _defaultBG : _redBG;
        }
        
        
        public void OnIncreasePressed()
        {
            CraftingOrdersManager.Instance.IncreaseOrderPriority(_order);
            GameEvents.Trigger_OnBuildingChanged(_building);
        }

        public void OnDecreasePressed()
        {
            CraftingOrdersManager.Instance.DecreaseOrderPriority(_order);
            GameEvents.Trigger_OnBuildingChanged(_building);
        }
    }
}
