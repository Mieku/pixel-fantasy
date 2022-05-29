using System;
using System.Collections.Generic;
using CodeMonkey.Utils;
using Gods;
using HUD;
using Items;
using ScriptableObjects;
using UnityEngine;

namespace Controllers
{
    public class HUDOptions : MonoBehaviour
    {
        [SerializeField] private List<ConstructionOrder> _cheatOrders;

        [SerializeField] private List<MassOrder> _massOrders;
        [SerializeField] private List<ConstructionOrder> _zoneOrders;
        [SerializeField] private List<ConstructionOrder> _wallOrders;
        [SerializeField] private List<ConstructionOrder> _floorOrders;
        [SerializeField] private List<ConstructionOrder> _doorOrders;
        [SerializeField] private List<ConstructionOrder> _productionOrders;
        [SerializeField] private List<ConstructionOrder> _furnitureOrders;
        [SerializeField] private List<ConstructionOrder> _lightingOrders;
        
        [SerializeField] private DirtTile _dirtPrefab;

        [SerializeField] private SpawnFurnitureController _furnitureController;

        #region Button Hooks

        public void CheatsPressed()
        {
            DisplayOrders(_cheatOrders);
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

        public void ProductionPressed()
        {
            DisplayOrders(_productionOrders);
        }

        public void FurniturePressed()
        {
            DisplayOrders(_furnitureOrders);
        }

        public void LightingPressed()
        {
            DisplayOrders(_lightingOrders);
        }

        #endregion

        private void ClearOptions()
        {
            HUDOrders.Instance.ClearOrders();
        }

        private void DisplayOrders(List<ConstructionOrder> orders)
        {
            ClearOptions();
            
            foreach (var order in orders)
            {
                Action onPressed = DetermineOnPressedAction(order.DataKey, order.OrderType, order.SubMenu, orders);
                HUDOrders.Instance.CreateOrderButton(order.Icon, onPressed, false);
            }
        }

        private void DisplayMassOrders(List<MassOrder> orders)
        {
            ClearOptions();
            
            foreach (var order in orders)
            {
                Sprite icon = Librarian.Instance.GetOrderIcon(order.OrderName);
                Action onPressed = DetermineOnPressedOrderAction(order.MassOrderType);
                HUDOrders.Instance.CreateOrderButton(icon, onPressed, false);
            }
        }

        private Action DetermineOnPressedOrderAction(Order orderType)
        {
            void OnOnpressed()
            {
                SelectionManager.Instance.BeginOrdersSelectionBox(orderType);
            }

            return OnOnpressed;
        }

        private Action DetermineOnPressedAction(string dataKey, OrderType orderType, List<ConstructionOrder> subMenu, List<ConstructionOrder> curMenu)
        {
            Action onpressed = null;
            
            switch (orderType)
            {
                case OrderType.BuildStructure:
                    onpressed += () =>
                    {
                        BuildStructurePressed(dataKey);
                    };
                    break;
                case OrderType.Zone:
                    onpressed += () =>
                    {
                        BuildZonePressed(dataKey);
                    };
                    break;
                case OrderType.ClearGrass:
                    onpressed += ClearGrassPressed;
                    break;
                case OrderType.BuildFloor:
                    onpressed += () =>
                    {
                        BuildFloorPressed(dataKey);
                    };
                    break;
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
                default:
                    throw new ArgumentOutOfRangeException(nameof(orderType), orderType, null);
            }

            return onpressed;
        }

        public void BuildFurniturePressed(string key)
        {
            FurnitureData item = Librarian.Instance.GetFurnitureData(key);
            _furnitureController.SpawnFurniturePressed(item);
        }

        public void BuildStructurePressed(string key)
        {
            PlayerInputController.Instance.ChangeState(PlayerInputState.BuildStructure, key);
            
            var structureData = Librarian.Instance.GetStructureData(PlayerInputController.Instance.StoredKey);
            Spawner.Instance.StructureData = structureData;
            Spawner.Instance.ShowPlacementIcon(true, structureData.Icon, structureData.InvalidPlacementTags);
        }
        
        public void ClearGrassPressed()
        {
            PlayerInputController.Instance.ChangeState(PlayerInputState.BuildFlooring, "Dirt");
            Spawner.Instance.ShowPlacementIcon(true, _dirtPrefab.Icon, _dirtPrefab.InvalidPlacementTags);
        }

        public void BuildFloorPressed(string key)
        {
            PlayerInputController.Instance.ChangeState(PlayerInputState.BuildFlooring, key);
            
            var floorData = Librarian.Instance.GetFloorData(PlayerInputController.Instance.StoredKey);
            Spawner.Instance.FloorData = floorData;
            Spawner.Instance.ShowPlacementIcon(true, floorData.Icon, floorData.InvalidPlacementTags);
        }

        public void BuildZonePressed(string key)
        {
            if (key == "Storage")
            {
                BuildStorageZone();
            } 
            else // Assume Farm if unknown
            {
                BuildFarmZone(key);
            }
        }

        private void BuildStorageZone()
        {
            PlayerInputController.Instance.ChangeState(PlayerInputState.BuildStorage);
            Spawner.Instance.ShowPlacementIcon(true, InventoryController.Instance.GetStorageZoneBlueprintSprite(), InventoryController.Instance.StoragePlacementInvalidTags);
        }

        private void ShowSubMenu(List<ConstructionOrder> subMenu, List<ConstructionOrder> curMenu, bool hasBackBtn)
        {
            if (hasBackBtn && subMenu[0].OrderName != "Back")
            {
                ConstructionOrder backbtn = new ConstructionOrder
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
            // Will likely need to specify more unique values for farms
            Spawner.Instance.ShowPlacementIcon(true, InventoryController.Instance.GetStorageZoneBlueprintSprite(), InventoryController.Instance.StoragePlacementInvalidTags);

            // var floorData = Librarian.Instance.GetFloorData(PlayerInputController.Instance.StoredKey);
            // Spawner.Instance.FloorData = floorData;
            // Spawner.Instance.ShowPlacementIcon(true, floorData.Icon, floorData.InvalidPlacementTags);
        }
    }
}
