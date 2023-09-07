using System;
using Items;
using Managers;
using ScriptableObjects;
using UnityEngine;

namespace Characters
{
    public class KinlingEquipment : MonoBehaviour
    {
        public EquipmentState Head;
        public EquipmentState Body;
        public EquipmentState Pants;
        public EquipmentState Hands;
        public EquipmentState MainHand;
        public EquipmentState OffHand;
        public EquipmentState Necklace;
        public EquipmentState Ring1;
        public EquipmentState Ring2;

        public ItemState Carried;
        
        public void Equip(Item item)
        {
            var equipmentState = item.State as EquipmentState;
            Destroy(item.gameObject);

            switch (equipmentState.EquipmentData.Type)
            {
                case EquipmentType.Head:
                    Head = equipmentState;
                    break;
                case EquipmentType.Body:
                    Body = equipmentState;
                    break;
                case EquipmentType.Pants:
                    Pants = equipmentState;
                    break;
                case EquipmentType.Hands:
                    Hands = equipmentState;
                    break;
                case EquipmentType.MainHand:
                    MainHand = equipmentState;
                    break;
                case EquipmentType.OffHand:
                    OffHand = equipmentState;
                    break;
                case EquipmentType.BothHands:
                    MainHand = equipmentState;
                    break;
                case EquipmentType.Necklace:
                    Necklace = equipmentState;
                    break;
                case EquipmentType.Ring:
                    if (Ring1 == null)
                    {
                        Ring1 = equipmentState;
                    }
                    else
                    {
                        Ring2 = equipmentState;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public Item Unequip(EquipmentState equipment)
        {
            var data = equipment.EquipmentData;
            if (data == null) return null;

            if (!HasEquipped(data)) return null;
            
            // Remove the item and drop it
            switch (data.Type)
            {
                case EquipmentType.Head:
                    Head = null;
                    break;
                case EquipmentType.Body:
                    Body = null;
                    break;
                case EquipmentType.Pants:
                    Pants = null;
                    break;
                case EquipmentType.Hands:
                    Hands = null;
                    break;
                case EquipmentType.MainHand:
                    MainHand = null;
                    break;
                case EquipmentType.OffHand:
                    OffHand = null;
                    break;
                case EquipmentType.BothHands:
                    MainHand = null;
                    break;
                case EquipmentType.Necklace:
                    Necklace = null;
                    break;
                case EquipmentType.Ring:
                    if (Ring1.Data == data)
                    {
                        Ring1 = null;
                    }
                    else
                    {
                        Ring2 = null;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            var droppedItem = Spawner.Instance.SpawnItem(equipment.Data, transform.position, true);
            return droppedItem;
        }

        public bool HasEquipped(EquipmentData equipmentData)
        {
            if (equipmentData == null) return true;

            switch (equipmentData.Type)
            {
                case EquipmentType.Head:
                    if (Head.Data == equipmentData) return true;
                    break;
                case EquipmentType.Body:
                    if (Body.Data == equipmentData) return true;
                    break;
                case EquipmentType.Pants:
                    if (Pants.Data == equipmentData) return true;
                    break;
                case EquipmentType.Hands:
                    if (Hands.Data == equipmentData) return true;
                    break;
                case EquipmentType.MainHand:
                    if (MainHand.Data == equipmentData) return true;
                    break;
                case EquipmentType.OffHand:
                    if (OffHand.Data == equipmentData) return true;
                    break;
                case EquipmentType.BothHands:
                    if (MainHand.Data == equipmentData) return true;
                    break;
                case EquipmentType.Necklace:
                    if (Necklace.Data == equipmentData) return true;
                    break;
                case EquipmentType.Ring:
                    if (Ring1.Data == equipmentData) return true;
                    if (Ring2.Data == equipmentData) return true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            return false;
        }

        public EquipmentState GetEquipmentByType(EquipmentType type)
        {
            switch (type)
            {
                case EquipmentType.Head:
                    return Head;
                case EquipmentType.Body:
                    return Body;
                case EquipmentType.Pants:
                    return Pants;
                case EquipmentType.Hands:
                    return Hands;
                case EquipmentType.MainHand:
                    return MainHand;
                case EquipmentType.OffHand:
                    return OffHand;
                case EquipmentType.BothHands:
                    if (MainHand != null)
                    {
                        return MainHand;
                    }
                    return OffHand;
                case EquipmentType.Necklace:
                    return Necklace;
                case EquipmentType.Ring:
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
