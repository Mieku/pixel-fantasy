using System;
using System.Collections.Generic;
using CodeMonkey.Utils;
using Gods;
using HUD;
using ScriptableObjects;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Controllers
{
    public class HUDOptions : MonoBehaviour
    {
        [SerializeField] private List<ConstructionOrder> _cheatOrders;
        
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
                Action onPressed = DetermineOnPressedAction(order.DataKey, order.OrderType);
                HUDOrders.Instance.CreateOrderButton(order.Icon, onPressed, false);
            }
        }

        private Action DetermineOnPressedAction(string dataKey, OrderType orderType)
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
        }

        private void BuildStorageZone()
        {
            PlayerInputController.Instance.ChangeState(PlayerInputState.BuildStorage);
        }
    }
}
