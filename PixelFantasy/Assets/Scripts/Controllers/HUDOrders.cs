using System;
using System.Collections.Generic;
using Actions;
using Gods;
using HUD;
using Items;
using UnityEngine;

namespace Controllers
{
    public class HUDOrders : God<HUDOrders>
    {
        [SerializeField] private GameObject _orderBG;
        [SerializeField] private OrderButton _orderBtnPrefab;
        [SerializeField] private Transform _ordersParent;

        private List<OrderButton> _displayedOrders = new List<OrderButton>();
        private SelectionData _selectionData;
        
        public void HideOrders()
        {
            ClearOrders();
            // TODO: Build me!   
        }

        public void ClearOrders()
        {
            _selectionData = null;
            foreach (var order in _displayedOrders)
            {
                Destroy(order.gameObject);
            }
            
            _displayedOrders.Clear();
        }
        
        public void DisplayOrders(SelectionData selectionData)
        {
            ClearOrders();
            _selectionData = selectionData;
            foreach (var action in _selectionData.Actions)
            {
                bool isActive = _selectionData.ClickObject.IsActionActive(action);
                CreateOrder(action, selectionData.Requestor, isActive);
            }
        }

        private void CreateOrder(ActionBase action, Interactable requestor, bool isActive)
        {
            Sprite icon = action.Icon;
            
            void OnPressed()
            {
                requestor.CreateTask(action);
            }

            // switch (order)
            // {
            //     case Order.Deconstruct:
            //         icon = Librarian.Instance.GetOrderIcon("Deconstruct");
            //         break;
            //     case Order.CutPlant:
            //         icon = Librarian.Instance.GetOrderIcon("Cut");
            //         onPressed = CutPlantOrder;
            //         break;
            //     case Order.Harvest:
            //         icon = Librarian.Instance.GetOrderIcon("Harvest");
            //         onPressed = HarvestOrder;
            //         break;
            //     case Order.Cancel:
            //         icon = Librarian.Instance.GetOrderIcon("Cancel");
            //         break;
            //     case Order.Disallow:
            //         icon = Librarian.Instance.GetOrderIcon("Disallow");
            //         onPressed = DisallowOrder;
            //         break;
            //     default:
            //         throw new ArgumentOutOfRangeException(nameof(order), order, null);
            // }
            
            CreateOrderButton(icon, OnPressed, isActive);
        }

        public void CreateOrderButton(Sprite icon, Action onPressed, bool isActive)
        {
            var orderBtn = Instantiate(_orderBtnPrefab, _ordersParent).GetComponent<OrderButton>();
            orderBtn.Init(icon, onPressed, isActive);
            _displayedOrders.Add(orderBtn);
        }

        #region Orders

        private void DisallowOrder()
        {
            var clickObject = _selectionData.ClickObject;
            clickObject.Owner.ToggleAllowed(!clickObject.Owner.IsAllowed);
        }
        
        // private void CutPlantOrder()
        // {
        //     var clickObject = _selectionData.ClickObject;
        //     var tree = clickObject.GetComponent<TreeResource>();
        //     if (tree != null)
        //     {
        //         tree.CreateCutTreeTask();
        //         return;
        //     }
        //     
        //     var plant = clickObject.GetComponent<GrowingResource>();
        //     if (plant != null)
        //     {
        //         plant.CreateCutPlantTask();
        //     }
        // }

        // private void HarvestOrder()
        // {
        //     var clickObject = _selectionData.ClickObject;
        //     var plant = clickObject.GetComponent<GrowingResource>();
        //     if (plant != null)
        //     {
        //         plant.CreateHarvestFruitTask();
        //     }
        // }
        
        #endregion
    }
}
