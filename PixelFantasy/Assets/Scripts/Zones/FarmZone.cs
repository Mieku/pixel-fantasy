using System.Collections.Generic;
using Popups.Zone_Popups;
using ScriptableObjects;
using UnityEngine;

namespace Zones
{
    public class FarmZone : Zone
    {
        public override ZoneType ZoneType => ZoneType.Farm;

        public FarmZone(string uid, List<Vector3Int> gridPositions, LayeredRuleTile layeredRuleTile) : base(uid, gridPositions, layeredRuleTile)
        {
            
        }

        protected override void AssignName()
        {
            Name = ZoneTypeData.ZoneTypeName; // TODO: Make this generate unique and interesting names
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
    }
}
