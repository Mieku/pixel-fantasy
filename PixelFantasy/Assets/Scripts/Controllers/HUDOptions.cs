using System;
using System.Collections.Generic;
using HUD;
using Managers;
using Popups;
using ScriptableObjects;
using UnityEngine;
using Order = HUD.Order;

namespace Controllers
{
    public class HUDOptions : MonoBehaviour
    {
        [SerializeField] private List<Order> _cheatOrders;

        [SerializeField] private List<MassOrder> _massOrders;
        [SerializeField] private List<Order> _zoneOrders;
        [SerializeField] private List<Order> _wallOrders;
        [SerializeField] private List<Order> _floorOrders;
        [SerializeField] private List<Order> _doorOrders;
        [SerializeField] private List<Order> _buildingsOrders;
        [SerializeField] private List<Order> _furnitureOrders;
        [SerializeField] private List<Order> _roofOrders;
        
        [SerializeField] private Sprite _zoneSprite;

        #region Button Hooks

        public void CheatsPressed()
        {
            DisplayOrders(_cheatOrders);
        }

        public void KinlingsPressed()
        {
            KinlingsPopup.Show();
        }

        public void MassOrdersPressed()
        {
            DisplayMassOrders(_massOrders);
        }

        public void ZonePressed()
        {
            DisplayOrders(_zoneOrders);
        }

        public void WallPressed()
        {
            DisplayOrders(_wallOrders);
        }

        public void FloorPressed()
        {
            DisplayOrders(_floorOrders);
        }

        public void DoorPressed()
        {
            DisplayOrders(_doorOrders);
        }

        public void BuildingsPressed()
        {
            DisplayOrders(_buildingsOrders);
        }

        public void FurniturePressed()
        {
            DisplayOrders(_furnitureOrders);
        }

        public void RoofPressed()
        {
            DisplayOrders(_roofOrders);
        }

        #endregion

        private void ClearOptions()
        {
            HUDOrders.Instance.ClearOrders();
        }

        private void DisplayOrders(List<Order> orders)
        {
            ClearOptions();
            
            foreach (var order in orders)
            {
                Action onPressed = DetermineOnPressedAction(order.DataKey, order.OrderType, order.SubMenu, orders);
                HUDOrders.Instance.CreateOrderButton(order.Icon, onPressed, false, order.OrderName);
            }
        }

        private void DisplayMassOrders(List<MassOrder> orders)
        {
            ClearOptions();
            
            foreach (var order in orders)
            {
                // Sprite icon = Librarian.Instance.GetOrderIcon(order.OrderName);
                // Action onPressed = DetermineOnPressedOrderAction(order.MassOrderType);
                // HUDOrders.Instance.CreateOrderButton(icon, onPressed, false, order.OrderName);
            }
        }

        // private Action DetermineOnPressedOrderAction(ActionBase orderType)
        // {
        //     void OnOnpressed()
        //     {
        //         SelectionManager.Instance.BeginOrdersSelectionBox(orderType);
        //     }
        //
        //     return OnOnpressed;
        // }

        private Action DetermineOnPressedAction(string dataKey, OrderType orderType, List<Order> subMenu, List<Order> curMenu)
        {
            Action onpressed = null;
            
            switch (orderType)
            {
                case OrderType.BuildFurniture:
                    onpressed += () =>
                    {
                        BuildFurniturePressed(dataKey);
                    };
                    break;
                case OrderType.SubMenu:
                    onpressed += () =>
                    {
                        ShowSubMenu(subMenu, curMenu, true);
                    };
                    break;
                case OrderType.Menu:
                    onpressed += () =>
                    {
                        ShowSubMenu(subMenu, curMenu, false);
                    };
                    break;
                case OrderType.BuildBuilding:
                    onpressed += () =>
                    {
                        BuildBuildingPressed(dataKey);
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(orderType), orderType, null);
            }

            return onpressed;
        }

        public void BuildFurniturePressed(string key)
        {
            Spawner.Instance.CancelInput();
            PlayerInputController.Instance.ChangeState(PlayerInputState.BuildFurniture, key);

            var furnitureData = Librarian.Instance.GetItemData(key) as FurnitureItemData;
            Spawner.Instance.PlanFurniture(furnitureData);
        }

        public void BuildBuildingPressed(string key)
        {
            Spawner.Instance.CancelInput();
            PlayerInputController.Instance.ChangeState(PlayerInputState.BuildBuilding, key);
            
            var building = Librarian.Instance.GetBuilding(PlayerInputController.Instance.StoredKey);
            Spawner.Instance.PlanBuilding(building);
        }
        
        private void ShowSubMenu(List<Order> subMenu, List<Order> curMenu, bool hasBackBtn)
        {
            if (hasBackBtn && subMenu[0].OrderName != "Back")
            {
                Order backbtn = new Order
                {
                    Icon = Librarian.Instance.GetOrderIcon("Back"),
                    OrderName = "Back",
                    OrderType = OrderType.Menu,
                    SubMenu = curMenu
                };

                subMenu.Insert(0, backbtn);
            }

            DisplayOrders(subMenu);
        }

        private void BuildFarmZone(string cropKey)
        {
            PlayerInputController.Instance.ChangeState(PlayerInputState.BuildFarm);

            var cropData = Librarian.Instance.GetCropData(cropKey);
            Spawner.Instance.CropData = cropData;
            Spawner.Instance.ShowPlacementIcon(true, _zoneSprite, new List<string>
            {
                "Water",
                "Zone"
            });
        }
    }
}
