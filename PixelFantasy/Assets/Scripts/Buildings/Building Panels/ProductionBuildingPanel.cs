using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;

namespace Buildings.Building_Panels
{
    public class ProductionBuildingPanel : MonoBehaviour
    {
        [SerializeField] private Transform _productsSlotsParent;
        [SerializeField] private ProductOption _productOptionPrefab;
        [SerializeField] private Transform _billsParent;
        [SerializeField] private ProductionBill _billPrefab;
        
        private ProductionBuilding _building;
        private List<ProductOption> _displayedProductOptions = new List<ProductOption>();
        private ProductOption _curSelectedProductOption;
        private List<ProductionBill> _displayedBills = new List<ProductionBill>();

        public void Init(Building building)
        {
            _building = building as ProductionBuilding;
            gameObject.SetActive(true);
            
            RefreshProductOptions();
            RefreshBills();
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        private void RefreshProductOptions()
        {
            foreach (var option in _displayedProductOptions)
            {
                Destroy(option.gameObject);
            }
            _displayedProductOptions.Clear();
            
            var options = _building.GetProductionOptions();
            foreach (var option in options)
            {
                var prodOption = Instantiate(_productOptionPrefab, _productsSlotsParent);
                prodOption.Init(option, OnOptionSelected);
                _displayedProductOptions.Add(prodOption);
            }
        }

        private void OnOptionSelected(CraftedItemData craftedItemData, ProductOption productOption)
        {
            if (_curSelectedProductOption != null)
            {
                _curSelectedProductOption.DisplaySelected(false);
            }
            _curSelectedProductOption = productOption;
            
            CreateBill(craftedItemData);
        }

        private void CreateBill(CraftedItemData craftedItemData)
        {
            //Create order
            ProductOrder order = new ProductOrder(craftedItemData);
            
            // Add order to building
            _building.OrderQueue.AddOrder(order);
            
            // Refresh Controls Display
            RefreshBills();
        }

        private void RefreshBills()
        {
            foreach (var bill in _displayedBills)
            {
                Destroy(bill.gameObject);
            }
            _displayedBills.Clear();
            
            var orders = _building.OrderQueue;
            foreach (var order in orders.AllOrders)
            {
                var bill = Instantiate(_billPrefab, _billsParent);
                bill.Init(order, _building, OnBillPriorityChanged);
                _displayedBills.Add(bill);
            }
        }

        private void OnBillPriorityChanged(ProductionBill bill)
        {
            RefreshBills();
        }
    }
}
