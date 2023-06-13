using System.Collections.Generic;
using Buildings;
using Characters;
using Controllers;
using Items;
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
        
        private List<Furniture> _availableFurniture = new List<Furniture>();

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

        public bool ContainsFurniture(FurnitureItemData furnitureItemData)
        {
            var result = GetFurniture(furnitureItemData);
            return result != null;
        }
        
        public void AddFurniture(Furniture furniture)
        {
            _availableFurniture.Add(furniture);
            GameEvents.Trigger_OnRoomFurnitureChanged(this);
        }

        public void RemoveFurniture(Furniture furniture)
        {
            _availableFurniture.Remove(furniture);
            GameEvents.Trigger_OnRoomFurnitureChanged(this);
        }

        public Furniture GetFurniture(FurnitureItemData furnitureItemData)
        {
            foreach (var furniture in _availableFurniture)
            {
                if (furniture.FurnitureItemData == furnitureItemData)
                {
                    return furniture;
                }
            }

            return null;
        }
        
        public Storage FindRoomStorage(ItemData itemData)
        {
            foreach (var furniture in _availableFurniture)
            {
                Storage storage = furniture as Storage;
                if (storage != null)
                {
                    if(storage.AmountCanBeDeposited(itemData) > 0)
                    {
                        return storage;
                    }
                }
            }

            return null;
        }
    }
}
