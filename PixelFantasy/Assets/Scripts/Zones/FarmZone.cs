using System.Collections.Generic;
using Gods;
using Popups.Zone_Popups;
using ScriptableObjects;
using UnityEngine;

namespace Zones
{
    public class FarmZone : Zone
    {
        public override ZoneType ZoneType => ZoneType.Farm;

        private CropData _cropAssigned;
        private Family _owner;

        public FarmZone(string uid, List<Vector3Int> gridPositions, LayeredRuleTile layeredRuleTile) : base(uid, gridPositions, layeredRuleTile)
        {
            
        }

        protected override void AssignName()
        {
            Name = ZoneTypeData.ZoneTypeName;
        }
        
        public override void ClickZone()
        {
            base.ClickZone();
            
            FarmZonePopup.Show(this);
        }

        public override void UnclickZone()
        {
            base.UnclickZone();

            if (FarmZonePopup.Instance != null)
            {
                FarmZonePopup.Hide();
            }
        }

        public void AssignCrop(CropData crop)
        {
            if (crop != null)
            {
                EditZoneName(crop.CropName + " Farm");
            }
            else
            {
                EditZoneName("Farm");
            }
            
            // Make sure the assigned crop is new, if so, set them up to be planted
            if (_cropAssigned != crop)
            {
                SetCropTiles(crop, WorldPositions);
            }

            _cropAssigned = crop;
        }

        private void SetCropTiles(CropData crop, List<Vector2> worldPositions)
        {
            foreach (var worldPosition in worldPositions)
            {
                var cropAtPos = GetCropAtPos(worldPosition);

                if (cropAtPos == null)
                {
                    // if there is no crop already there
                    Spawner.Instance.SpawnSoilTile(worldPosition, crop, _owner);
                }
                else
                {
                    cropAtPos.ChangeCrop(crop);
                }
            }
        }

        public CropData GetAssignedCrop()
        {
            return _cropAssigned;
        }

        private Crop GetCropAtPos(Vector2 worldPos)
        {
            var gosAtPos = Helper.GetGameObjectsOnTile(worldPos);
            foreach (var goAtPos in gosAtPos)
            {
                var crop = goAtPos.GetComponent<Crop>();
                if (crop != null)
                {
                    return crop;
                }
            }

            return null;
        }

        public override void ExpandZone(List<Vector3Int> newCells)
        {
            base.ExpandZone(newCells);
            List<Vector2> newWorldPoses = ConvertGridPositionsToWorldPositions(newCells);
            SetCropTiles(_cropAssigned, newWorldPoses);
        }

        public override void ShrinkZone(List<Vector3Int> cellsToRemove)
        {
            base.ShrinkZone(cellsToRemove);
            List<Vector2> removeWorldPoses = ConvertGridPositionsToWorldPositions(cellsToRemove);
            SetCropTiles(null, removeWorldPoses);
        }

        public override void RemoveZone()
        {
            base.RemoveZone();
        }
    }
}
