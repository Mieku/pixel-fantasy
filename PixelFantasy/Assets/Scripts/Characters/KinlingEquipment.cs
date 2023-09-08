using System;
using Items;
using Managers;
using ScriptableObjects;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Characters
{
    public class KinlingEquipment : MonoBehaviour
    {
        [BoxGroup("Equipment Renderers")][SerializeField] private Transform _headGear;
        [BoxGroup("Equipment Renderers")][SerializeField] private Transform _bodyGear;
        [BoxGroup("Equipment Renderers")][SerializeField] private Transform _pantsGearHips;
        [BoxGroup("Equipment Renderers")][SerializeField] private Transform _pantsGearLeftLeg;
        [BoxGroup("Equipment Renderers")][SerializeField] private Transform _pantsGearRightLeg;
        [BoxGroup("Equipment Renderers")][SerializeField] private Transform _handsGearMainHand;
        [BoxGroup("Equipment Renderers")][SerializeField] private Transform _handsGearOffHand;
        [BoxGroup("Equipment Renderers")][SerializeField] private Transform _mainHandHeld;
        [BoxGroup("Equipment Renderers")][SerializeField] private Transform _offHandHeld;
        
        private GearPiece _headGearObj;
        private GearPiece _bodyGearObj;
        private GearPiece _pantsGearHipsObj;
        private GearPiece _pantsGearLeftLegObj;
        private GearPiece _pantsGearRightLegObj;
        private GearPiece _handsGearMainHandObj;
        private GearPiece _handsGearOffHandObj;
        private GearPiece _mainHandHeldObj;
        private GearPiece _offHandHeldObj;
        
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

        private UnitActionDirection _curDirection;
        
        public void AssignDirection(UnitActionDirection direction)
        {
            _curDirection = direction;
        }
        
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
            DisplayGear(equipmentState);
        }

        private void DisplayGear(EquipmentState equipmentState)
        {
            var type = equipmentState.EquipmentData.Type;
            var equip = equipmentState.EquipmentData;
            ClearDisplayedGear(type);

            switch (type)
            {
                case EquipmentType.Head:
                    if (equip.HatGear != null)
                    {
                        var gear = Instantiate(equip.HatGear, _headGear);
                        gear.AssignDirection(_curDirection);
                        _headGearObj = gear;
                    }
                    break;
                case EquipmentType.Body:
                    if (equip.BodyGear != null)
                    {
                        var gear = Instantiate(equip.BodyGear, _bodyGear);
                        gear.AssignDirection(_curDirection);
                        _bodyGearObj = gear;
                    }
                    break;
                case EquipmentType.Pants:
                    if (equip.HipsGear != null)
                    {
                        var gear = Instantiate(equip.HipsGear, _pantsGearHips);
                        gear.AssignDirection(_curDirection);
                        _pantsGearHipsObj = gear;
                    }
                    if (equip.LeftLegGear != null)
                    {
                        var gear = Instantiate(equip.LeftLegGear, _pantsGearLeftLeg);
                        gear.AssignDirection(_curDirection);
                        _pantsGearLeftLegObj = gear;
                    }
                    if (equip.RightLegGear != null)
                    {
                        var gear = Instantiate(equip.RightLegGear, _pantsGearRightLeg);
                        gear.AssignDirection(_curDirection);
                        _pantsGearRightLegObj = gear;
                    }
                    break;
                case EquipmentType.Hands:
                    if (equip.MainHandGear != null)
                    {
                        var gear = Instantiate(equip.MainHandGear, _handsGearMainHand);
                        gear.AssignDirection(_curDirection);
                        _handsGearMainHandObj = gear;
                    }
                    if (equip.OffHandGear != null)
                    {
                        var gear = Instantiate(equip.OffHandGear, _handsGearOffHand);
                        gear.AssignDirection(_curDirection);
                        _handsGearOffHandObj = gear;
                    }
                    break;
                case EquipmentType.MainHand:
                    if (equip.MainHandHeldGear != null)
                    {
                        var gear = Instantiate(equip.MainHandHeldGear, _mainHandHeld);
                        gear.AssignDirection(_curDirection);
                        _mainHandHeldObj = gear;
                    }
                    break;
                case EquipmentType.OffHand:
                    if (equip.OffHandHeldGear != null)
                    {
                        var gear = Instantiate(equip.OffHandHeldGear, _offHandHeld);
                        gear.AssignDirection(_curDirection);
                        _offHandHeldObj = gear;
                    }
                    break;
                case EquipmentType.BothHands:
                    if (equip.MainHandHeldGear != null)
                    {
                        var gear = Instantiate(equip.MainHandHeldGear, _mainHandHeld);
                        gear.AssignDirection(_curDirection);
                        _mainHandHeldObj = gear;
                    }
                    break;
                case EquipmentType.Carried:
                    Debug.LogError("Carried is not built yet");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ClearDisplayedGear(EquipmentType type)
        {
            switch (type)
            {
                case EquipmentType.Head:
                    if (_headGearObj != null)
                    {
                        Destroy(_headGearObj.gameObject);
                        _headGearObj = null;
                    }
                    break;
                case EquipmentType.Body:
                    if (_bodyGearObj != null)
                    {
                        Destroy(_bodyGearObj.gameObject);
                        _bodyGearObj = null;
                    }
                    break;
                case EquipmentType.Pants:
                    if (_pantsGearHipsObj != null)
                    {
                        Destroy(_pantsGearHipsObj.gameObject);
                        _pantsGearHipsObj = null;
                    }
                    if (_pantsGearLeftLegObj != null)
                    {
                        Destroy(_pantsGearLeftLegObj.gameObject);
                        _pantsGearLeftLegObj = null;
                    }
                    if (_pantsGearRightLegObj != null)
                    {
                        Destroy(_pantsGearRightLegObj.gameObject);
                        _pantsGearRightLegObj = null;
                    }
                    break;
                case EquipmentType.Hands:
                    if (_handsGearMainHandObj != null)
                    {
                        Destroy(_handsGearMainHandObj.gameObject);
                        _handsGearMainHandObj = null;
                    }
                    if (_handsGearOffHandObj != null)
                    {
                        Destroy(_handsGearOffHandObj.gameObject);
                        _handsGearOffHandObj = null;
                    }
                    break;
                case EquipmentType.MainHand:
                    if (_mainHandHeldObj != null)
                    {
                        Destroy(_mainHandHeldObj.gameObject);
                        _mainHandHeldObj = null;
                    }
                    break;
                case EquipmentType.OffHand:
                    if (_offHandHeldObj != null)
                    {
                        Destroy(_offHandHeldObj.gameObject);
                        _offHandHeldObj = null;
                    }
                    break;
                case EquipmentType.BothHands:
                    if (_mainHandHeldObj != null)
                    {
                        Destroy(_mainHandHeldObj.gameObject);
                        _mainHandHeldObj = null;
                    }
                    if (_offHandHeldObj != null)
                    {
                        Destroy(_offHandHeldObj.gameObject);
                        _offHandHeldObj = null;
                    }
                    break;
                case EquipmentType.Carried:
                    Debug.LogError("Carried is not built yet");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
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
