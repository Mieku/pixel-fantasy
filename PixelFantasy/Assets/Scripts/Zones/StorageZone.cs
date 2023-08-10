using System.Collections.Generic;
using Buildings;
using Controllers;
using Popups.Zone_Popups;
using ScriptableObjects;
using UnityEngine;

namespace Zones
{
    public class StorageZone : Zone
    {
        public override ZoneType ZoneType => ZoneType.Storage;
        
        public StorageZone(string uid, List<Vector3Int> gridPositions, LayeredRuleTile layeredRuleTile) : base(uid, gridPositions, layeredRuleTile, (BuildingOld)null)
        {
            SetStorageTiles(WorldPositions);
        }
        
        protected override void AssignName()
        {
            Name = ZoneTypeData.ZoneTypeName;
        }
        
        public override void ClickZone()
        {
            base.ClickZone();
            
            StorageZonePopup.Show(this);
        }

        public override void UnclickZone()
        {
            base.UnclickZone();

            if (StorageZonePopup.Instance != null)
            {
                StorageZonePopup.Hide();
            }
        }

        private void SetStorageTiles(List<Vector2> worldPositions)
        {
            foreach (var worldPos in worldPositions)
            {
                //ControllerManager.Instance.InventoryController.SpawnStorageSlot(worldPos);
            }
        }
        
        private void RemoveStorageTiles(List<Vector2> worldPositions)
        {
            foreach (var worldPos in worldPositions)
            {
                //ControllerManager.Instance.InventoryController.RemoveStorageSlot(worldPos);
            }
        }
        
        public override void ExpandZone(List<Vector3Int> newCells)
        {
            base.ExpandZone(newCells);
            List<Vector2> newWorldPoses = ConvertGridPositionsToWorldPositions(newCells);
            SetStorageTiles(newWorldPoses);
        }

        public override void ShrinkZone(List<Vector3Int> cellsToRemove)
        {
            base.ShrinkZone(cellsToRemove);
            List<Vector2> removeWorldPoses = ConvertGridPositionsToWorldPositions(cellsToRemove);
            RemoveStorageTiles(removeWorldPoses);
        }

        public override void RemoveZone()
        {
            base.RemoveZone();
            RemoveStorageTiles(WorldPositions);
        }
    }
}
