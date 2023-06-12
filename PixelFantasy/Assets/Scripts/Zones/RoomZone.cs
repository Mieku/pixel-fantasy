using System.Collections.Generic;
using Buildings;
using Characters;
using Controllers;
using ScriptableObjects;
using UnityEngine;

namespace Zones
{
    public class RoomZone : Zone
    {
        public override ZoneType ZoneType => ZoneType.Room;
        public int MaxOccupants => _roomData.MaxOccupants;

        protected List<UnitState> _occupants = new List<UnitState>();

        public List<UnitState> Occupants => _occupants;

        public RoomZone(string uid, List<Vector3Int> gridPositions, LayeredRuleTile layeredRuleTile, RoomData roomData) : base(uid, gridPositions, layeredRuleTile, roomData)
        {
            
        }

        public void AddOccupant(UnitState unitState)
        {
            _occupants.Add(unitState);
        }

        public void RemoveOccupant(UnitState unitState)
        {
            _occupants.Remove(unitState);
        }

        protected override void AssignName()
        {
            Name = _roomData.RoomName;
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
