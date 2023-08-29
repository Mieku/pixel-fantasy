using System.Collections.Generic;
using System.Linq;
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
            UpdateContainedFurniture();
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

        public override void ExpandZone(List<Vector3Int> newCells)
        {
            base.ExpandZone(newCells);
            
            UpdateContainedFurniture();
        }

        public override void ShrinkZone(List<Vector3Int> cellsToRemove)
        {
            base.ShrinkZone(cellsToRemove);
            
            
            UpdateContainedFurniture();
        }

        public override void RemoveZone()
        {
            base.RemoveZone();
            
            UpdateContainedFurniture();
        }

        private void UpdateContainedFurniture()
        {
            // foreach (var furniture in _availableFurniture)
            // {
            //     furniture.AssignParentRoom(null);
            // }
            // _availableFurniture.Clear();
            //
            // foreach (var gridPos in WorldPositions)
            // {
            //     var furniture = Helper.GetObjectAtPosition<Furniture>(gridPos);
            //     if (furniture != null)
            //     {
            //         furniture.AssignParentRoom(this);
            //     }
            // }
            //
            // _availableFurniture = _availableFurniture.Distinct().ToList();
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
        
        public Dictionary<ItemData, int> GetRoomInventory()
        {
            Dictionary<ItemData, int> results = new Dictionary<ItemData, int>();
            foreach (var furniture in _availableFurniture)
            {
                Storage storage = furniture as Storage;
                if (storage != null)
                {
                    var storedItems = storage.AvailableInventory;
                    foreach (var itemKVP in storedItems)
                    {
                        if (results.ContainsKey(itemKVP.Key))
                        {
                            results[itemKVP.Key] += itemKVP.Value;
                        }
                        else
                        {
                            results.Add(itemKVP.Key, itemKVP.Value);
                        }
                    }
                }
            }

            return results;
        }

        public List<FurnitureItemData> BuildFurnitureOptions()
        {
            return new List<FurnitureItemData>(_roomData.BuildFurnitureOptions);
        }
    }
}
