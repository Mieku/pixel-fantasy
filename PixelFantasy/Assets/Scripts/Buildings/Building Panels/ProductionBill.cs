using System;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Buildings.Building_Panels
{
    public class ProductionBill : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _productName;
        [SerializeField] private TextMeshProUGUI _quantity;
        [SerializeField] private Image _productImage;
        [SerializeField] private GameObject _upArrow;
        [SerializeField] private GameObject _downArrow;
        [SerializeField] private GameObject _suspendBtn;
        [SerializeField] private TMP_Dropdown _productionTypeDD;
        
        private ProductOrder _order;
        private CraftingBuilding _building;
        private Action<ProductionBill> _onBillPriorityChangedCallback;

        private void Start()
        {
            GameEvents.RefreshInventoryDisplay += InventoryChanged;
        }

        private void OnDestroy()
        {
            GameEvents.RefreshInventoryDisplay -= InventoryChanged;
        }

        private void InventoryChanged()
        {
            RefreshDisplay();
        }

        public void Init(ProductOrder order, CraftingBuilding building, Action<ProductionBill> onBillPriorityChangedCallback)
        {
            _order = order;
            _building = building;
            _onBillPriorityChangedCallback = onBillPriorityChangedCallback;
            
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            // _productName.text = _order.craftedItemData.ItemName;
            // _productImage.sprite = _order.craftedItemData.ItemSprite;
            //
            // // Rotate Icon if suspended
            // if (_order.isSuspended)
            // {
            //     _suspendBtn.gameObject.transform.localRotation = new Quaternion(0, 0, 45, 0);
            // }
            // else
            // {
            //     _suspendBtn.gameObject.transform.localRotation = new Quaternion(0, 0, 0, 0);
            // }
            //
            // switch (_order.productionType)
            // {
            //     case ProductOrder.ProductionType.Finite:
            //         _productionTypeDD.SetValueWithoutNotify(0);
            //         _quantity.text = $"{_order.valueSet - _order.amountMade}x";
            //         break;
            //     case ProductOrder.ProductionType.MaintainAmount:
            //         _productionTypeDD.SetValueWithoutNotify(1);
            //         int amountAvailable = InventoryManager.Instance.GetAmountAvailable(_order.craftedItemData);
            //         _quantity.text = $"{amountAvailable}/{_order.valueSet}";
            //         break;
            //     case ProductOrder.ProductionType.Infinite:
            //         _productionTypeDD.SetValueWithoutNotify(2);
            //         _quantity.text = "Infinite";
            //         break;
            //     default:
            //         throw new ArgumentOutOfRangeException();
            // }
            //
            // _upArrow.SetActive(true);
            // _downArrow.SetActive(true);
            // var thisOrderIndex = _building.OrderQueue.AllOrders.IndexOf(_order);
            // if (thisOrderIndex == 0)
            // {
            //     _upArrow.SetActive(false);
            // }
            //
            // if (thisOrderIndex == _building.OrderQueue.AllOrders.Count - 1)
            // {
            //     _downArrow.SetActive(false);
            // }
        }

        public void IncreaseQtyPressed()
        {
            _order.valueSet++;
            RefreshDisplay();
        }

        public void DecreaseQtyPressed()
        {
            if (_order.valueSet > 0)
            {
                _order.valueSet--;
            }
            else
            {
                _order.valueSet = 0;
            }
            RefreshDisplay();
        }

        public void IncreasePriorityPressed()
        {
            // _building.OrderQueue.IncreaseOrderPriority(_order);
            // _onBillPriorityChangedCallback.Invoke(this);
        }

        public void DecreasePriorityPressed()
        {
            // _building.OrderQueue.DecreaseOrderPriority(_order);
            // _onBillPriorityChangedCallback.Invoke(this);
        }

        public void SuspendPressed()
        {
            _order.isSuspended = !_order.isSuspended;
            RefreshDisplay();
        }

        public void DeleteBillPressed()
        {
            // _building.OrderQueue.DeleteOrder(_order);
            // _onBillPriorityChangedCallback.Invoke(this);
        }

        public void OnDropdownChanged(Int32 value)
        {
            _order.productionType = value switch
            {
                0 => ProductOrder.ProductionType.Finite,
                1 => ProductOrder.ProductionType.MaintainAmount,
                _ => ProductOrder.ProductionType.Infinite
            };
            RefreshDisplay();
        }
    }
}
