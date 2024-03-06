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
        public GearSettings GearSettings => Settings as GearSettings;
        public DyeSettings AssignedDye;
        
        public GearState(GearSettings settings, string uid, Item item) : base(settings, uid, item)
        {
            Settings = settings;
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
