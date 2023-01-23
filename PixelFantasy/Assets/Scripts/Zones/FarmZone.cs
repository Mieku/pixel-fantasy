using System.Collections.Generic;
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
    }
}
