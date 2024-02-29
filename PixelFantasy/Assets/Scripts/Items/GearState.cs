using System;
using Characters;
using ScriptableObjects;
using UnityEngine;

namespace Items
{
    [Serializable]
    public class GearState : ItemState
    {
        public Kinling Owner;
        public GearData GearData => Data as GearData;
        public DyePaletteData AssignedDye;
        
        public GearState(GearData data, string uid, Item item) : base(data, uid, item)
        {
            Data = data;
            UID = uid;
            _linkedItem = item;
        }
        
        public GearState(GearState other) : base(other)
        {
            Owner = other.Owner;
            AssignedDye = other.AssignedDye;
        }

        public bool CanBeEquippedByUnit(Kinling kinling)
        {
            if (Durability <= 0) return false;
           
            return true;
        }
    }
}
