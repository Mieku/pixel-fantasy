using System.Collections.Generic;
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
    }
}
