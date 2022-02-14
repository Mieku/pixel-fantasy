using Controllers;
using Gods;
using Items;
using ScriptableObjects;
using UnityEngine;

namespace HUD
{
    public class HUDFurnitureOptions : InputAwareComponent
    {
        public void SpawnFurniturePressed(FurnitureData furnitureData)
        {
            PlayerInputController.Instance.ChangeState(PlayerInputState.BuildFurniture);

            float iconSize = furnitureData.GetFurniturePrefab(PlacementDirection.Down).GetComponent<FurniturePrefab>()
                .FurnitureRenderer.gameObject.transform.localScale.x;
            Spawner.Instance.FurnitureData = furnitureData;
            Spawner.Instance.ShowPlacementObject(true, furnitureData.GetFurniturePrefab(PlacementDirection.Down), furnitureData.InvalidPlacementTags, iconSize);
        }
    }
}
