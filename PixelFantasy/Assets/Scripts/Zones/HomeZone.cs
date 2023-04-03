using System.Collections.Generic;
using Popups;
using Popups.Zone_Popups;
using ScriptableObjects;
using UnityEngine;

namespace Zones
{
    public class HomeZone : Zone
    {
        public override ZoneType ZoneType => ZoneType.Home;
        
        public HomeZone(string uid, List<Vector3Int> gridPositions, LayeredRuleTile layeredRuleTile) : base(uid, gridPositions, layeredRuleTile)
        {
            
        }

        protected override void AssignName()
        {
            Name = ZoneTypeData.ZoneTypeName;
        }

        public override void ClickZone()
        {
            base.ClickZone();
            
            //HomePopup.Show(this);
        }

        public override void UnclickZone()
        {
            base.UnclickZone();

            if (HomePopup.Instance != null)
            {
                HomePopup.Hide();
            }
        }
    }
}
