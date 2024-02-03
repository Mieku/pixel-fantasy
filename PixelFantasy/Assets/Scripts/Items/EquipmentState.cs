using System;
using ScriptableObjects;
using UnityEngine;

namespace Items
{
    public class EquipmentState
    {
        public GearState Head;
        public GearState Body;
        public GearState Pants;
        public GearState Hands;
        public GearState MainHand;
        public GearState OffHand;
        public GearState Necklace;
        public GearState Ring1;
        public GearState Ring2;
        
        public EquipmentState()
        {
            // Constructor to initialize a new instance with default values
            Head = null;
            Body = null;
            Pants = null;
            Hands = null;
            MainHand = null;
            OffHand = null;
            Necklace = null;
            Ring1 = null;
            Ring2 = null;
        }

        // Constructor to create a copy of an existing instance
        public EquipmentState(EquipmentState other)
        {
            if(other.Head != null) Head = new GearState(other.Head);
            if(other.Body != null) Body = new GearState(other.Body);
            if(other.Pants != null) Pants = new GearState(other.Pants);
            if(other.Hands != null) Hands = new GearState(other.Hands);
            if(other.MainHand != null) MainHand = new GearState(other.MainHand);
            if(other.OffHand != null) OffHand = new GearState(other.OffHand);
            if(other.Necklace != null) Necklace = new GearState(other.Necklace);
            if(other.Ring1 != null) Ring1 = new GearState(other.Ring1);
            if(other.Ring2 != null) Ring2 = new GearState(other.Ring2);
        }

        public void SetGear(GearState gear)
        {
            if (gear == null || gear.GearData == null)
            {
                Debug.LogError($"Attempted to set null Gear");
                return;
            }

            var type = gear.GearData.Type;
            switch (type)
            {
                case GearType.Head:
                    Head = gear;
                    break;
                case GearType.Body:
                    Body = gear;
                    break;
                case GearType.Pants:
                    Pants = gear;
                    break;
                case GearType.Hands:
                    Hands = gear;
                    break;
                case GearType.MainHand:
                    MainHand = gear;
                    break;
                case GearType.OffHand:
                    OffHand = gear;
                    break;
                case GearType.Necklace:
                    Necklace = gear;
                    break;
                case GearType.Ring:
                    Ring1 = gear;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public void ClearGear(GearType type)
        {
            switch (type)
            {
                case GearType.Head:
                    Head = null;
                    break;
                case GearType.Body:
                    Body = null;
                    break;
                case GearType.Pants:
                    Pants = null;
                    break;
                case GearType.Hands:
                    Hands = null;
                    break;
                case GearType.MainHand:
                    MainHand = null;
                    break;
                case GearType.OffHand:
                    OffHand = null;
                    break;
                case GearType.Necklace:
                    Necklace = null;
                    break;
                case GearType.Ring:
                    Ring1 = null;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
        
        public GearState GetGearByType(GearType type)
        {
            switch (type)
            {
                case GearType.Head:
                    return Head;
                case GearType.Body:
                    return Body;
                case GearType.Pants:
                    return Pants;
                case GearType.Hands:
                    return Hands;
                case GearType.MainHand:
                    return MainHand;
                case GearType.OffHand:
                    return OffHand;
                case GearType.Necklace:
                    return Necklace;
                case GearType.Ring:
                    if (Ring1 != null)
                    {
                        return Ring1;
                    }
                    return Ring2;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}
