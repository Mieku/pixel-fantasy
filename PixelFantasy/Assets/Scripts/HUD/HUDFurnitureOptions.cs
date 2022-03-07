using System;
using Controllers;
using Gods;
using Items;
using ScriptableObjects;
using UnityEngine;

namespace HUD
{
    public class HUDFurnitureOptions : InputAwareComponent
    {
        private FurnitureData _curFurnitureData;
        
        public void SpawnFurniturePressed(FurnitureData furnitureData)
        {
            PlayerInputController.Instance.ChangeState(PlayerInputState.BuildFurniture);

            _curFurnitureData = furnitureData;

            ShowPlacementIcon();
        }

        private void ShowPlacementIcon()
        {
            float iconSize = _curFurnitureData.GetFurniturePrefab(PlacementDirection.Down).GetComponent<FurniturePrefab>()
                .FurnitureRenderer.gameObject.transform.localScale.x;
            Spawner.Instance.FurnitureData = _curFurnitureData;
            
            var icon = _curFurnitureData.GetFurniturePrefab(Spawner.Instance.PlacementDirection);
            if (icon == null)
            {
                Spawner.Instance.PlacementDirection =
                    _curFurnitureData.GetNextAvailablePlacementDirection(Spawner.Instance.PlacementDirection);
                icon = _curFurnitureData.GetFurniturePrefab(Spawner.Instance.PlacementDirection);
            }

            Spawner.Instance.ShowPlacementObject(true, icon, _curFurnitureData.InvalidPlacementTags, iconSize);
        }

        private void RotateClockwise()
        {
            Spawner.Instance.SetNextPlacementDirection(true);
            ShowPlacementIcon();
        }

        private void RotateCounterClockwise()
        {
            Spawner.Instance.SetNextPlacementDirection(false);
            ShowPlacementIcon();
        }

        private void Update()
        {
            if (PlayerInputController.Instance.GetCurrentState() == PlayerInputState.BuildFurniture)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    RotateClockwise();
                }
                
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    RotateCounterClockwise();
                }
            }
        }
    }
}
