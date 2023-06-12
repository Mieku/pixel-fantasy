using System.Collections.Generic;
using Buildings;
using Controllers;
using ScriptableObjects;
using UnityEngine;

namespace Zones
{
    public class ProductionZone : RoomZone
    {
        private ProductionRoomData _prodRoomData => _roomData as ProductionRoomData;
        public override ZoneType ZoneType => ZoneType.Workshop;
        
        public ProductionZone(string uid, List<Vector3Int> gridPositions, LayeredRuleTile layeredRuleTile, ProductionRoomData roomData) : base(uid, gridPositions, layeredRuleTile, roomData)
        {
            
        }
        
        public override void ClickZone()
        {
            base.ClickZone();
            
            HUDController.Instance.ShowRoomDetails(this);
        }

        public override void UnclickZone()
        {
            base.UnclickZone();
        }
    }
}
