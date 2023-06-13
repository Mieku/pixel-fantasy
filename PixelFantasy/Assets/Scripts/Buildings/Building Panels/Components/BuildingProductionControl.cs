using System;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zones;

namespace Buildings.Building_Panels.Components
{
    public class BuildingProductionControl : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _productName;
        [SerializeField] private TextMeshProUGUI _quantityDisplay;
        [SerializeField] private Image _productIcon;
        [SerializeField] private TMP_Dropdown _productionTypeDD;
        [SerializeField] private GameObject _upArrow;
        [SerializeField] private GameObject _downArrow;
        [SerializeField] private GameObject _suspendBtn;

        private ProductOrder _order;
        private ProductionZone _productionZone;
        private Action<ProductOrder> _onValueChanged;
        private int _index;

        public void Init(ProductOrder order, ProductionZone room, Action<ProductOrder> onValueChanged, int index)
        {
            _order = order;
            _productionZone = room;
            _onValueChanged = onValueChanged;
            _index = index;

            RefeshDisplay();
        }

        private void RefeshDisplay()
        {
            _productName.text = _order.craftedItemData.ItemName;
            _productIcon.sprite = _order.craftedItemData.ItemSprite;
            if (_order.isSuspended)
            {
                _suspendBtn.gameObject.transform.localRotation = new Quaternion(0, 0, 90, 0);
            }
            else
            {
                _suspendBtn.gameObject.transform.localRotation = new Quaternion(0, 0, 0, 0);
            }

            switch (_order.productionType)
            {
                case ProductOrder.ProductionType.Finite:
                    _productionTypeDD.SetValueWithoutNotify(0);
                    _quantityDisplay.text = $"{_order.valueSet - _order.amountMade}x";
                    break;
                case ProductOrder.ProductionType.MaintainAmount:
                    _productionTypeDD.SetValueWithoutNotify(1);
                    int amountAvailable = InventoryManager.Instance.GetAmountAvailable(_order.craftedItemData);
                    _quantityDisplay.text = $"{amountAvailable}/{_order.valueSet}";
                    break;
                case ProductOrder.ProductionType.Infinite:
                    _productionTypeDD.SetValueWithoutNotify(2);
                    _quantityDisplay.text = "Infinite";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (_index == 0)
            {
                _upArrow.SetActive(false);
            }
            else
            {
                _upArrow.SetActive(true);
            }

            int totalOrders = _productionZone.OrderQueue.Count;
            if (_index < totalOrders - 1)
            {
                _downArrow.SetActive(true);
            }
            else
            {
                _downArrow.SetActive(false);
            }
        }

        private void UpdateBuildingOrder()
        {
            _onValueChanged.Invoke(_order);
        }
        
        #region Button Hooks

        public void DeletePressed()
        {
            _productionZone.OrderQueue.DeleteOrder(_order);
            UpdateBuildingOrder();
        }

        public void SuspendPressed()
        {
            _order.isSuspended = !_order.isSuspended;
            UpdateBuildingOrder();
        }

        public void UpPressed()
        {
            _productionZone.OrderQueue.IncreaseOrderPriority(_order);
            UpdateBuildingOrder();
        }

        public void DownPressed()
        {
            _productionZone.OrderQueue.DecreaseOrderPriority(_order);
            UpdateBuildingOrder();
        }

        public void IncreasePressed()
        {
            _order.valueSet++;
            UpdateBuildingOrder();
        }

        public void DecreasePressed()
        {
            if (_order.valueSet > 0)
            {
                _order.valueSet--;
            }
            else
            {
                _order.valueSet = 0;
            }
            UpdateBuildingOrder();
        }

        public void OnDropdownChanged(TMP_Dropdown dropdown)
        {
            var value = dropdown.value;
            if (value == 0)
            {
                _order.productionType = ProductOrder.ProductionType.Finite;
            } 
            else if (value == 1)
            {
                _order.productionType = ProductOrder.ProductionType.MaintainAmount;
            }
            else
            {
                _order.productionType = ProductOrder.ProductionType.Infinite;
            }
            UpdateBuildingOrder();
        }

        #endregion
    }
}
