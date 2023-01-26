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
                bool isActive = selectionData.CancellableActions.Contains(action);
                CreateOrder(action, selectionData.Requestor, isActive);
            }
        }
        
        private void CreateOrder(ActionBase action, Interactable requestor, bool isActive)
        {
            Sprite icon = action.Icon;
            
            void OnPressed()
            {
                if (isActive)
                {
                    requestor.CancelTask(action);
                }
                else
                {
                    requestor.CreateTask(action);
                }
                GameEvents.Trigger_RefreshSelection();
            }
            
            CreateOrderButton(icon, OnPressed, isActive, action.id);
        }

        public void CreateOrderButton(Sprite icon, Action onPressed, bool isActive, string buttonName)
        {
            var orderBtn = Instantiate(_orderBtnPrefab, _ordersParent).GetComponent<OrderButton>();
            orderBtn.Init(icon, onPressed, isActive, buttonName);
            _displayedOrders.Add(orderBtn);
        }

        #region Orders

        private void DisallowOrder()
        {
            var clickObject = _selectionData.ClickObject;
            clickObject.Owner.ToggleAllowed(!clickObject.Owner.IsAllowed);
        }
        
        #endregion
    }
}
