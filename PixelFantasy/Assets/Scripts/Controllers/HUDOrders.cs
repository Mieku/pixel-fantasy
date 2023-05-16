using System;
using System.Collections.Generic;
using HUD;
using Managers;
using UnityEngine;

namespace Controllers
{
    public class HUDOrders : Singleton<HUDOrders>
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
            foreach (var command in _selectionData.Commands)
            {
                bool isActive = selectionData.Requestor.IsPending(command);
                CreateCommand(command, selectionData.Requestor, isActive);
            }
        }

        private void CreateCommand(Command command, Interactable requestor, bool isActive)
        {
            Sprite icon = command.Icon;

            void OnPressed()
            {
                if (isActive)
                {
                    requestor.CancelCommand(command);
                }
                else
                {
                    requestor.CreateTask(command);
                }
                GameEvents.Trigger_RefreshSelection();
            }
            
            CreateOrderButton(icon, OnPressed, isActive, command.Name);
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
