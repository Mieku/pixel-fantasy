using System;
using System.Collections.Generic;
using Items;
using Managers;
using ScriptableObjects;
using Sirenix.OdinInspector;
using TaskSystem;
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

        public EquipmentState EquipmentState = new EquipmentState();
        public EquipmentState DesiredEquipmentState = new EquipmentState();

        public Item Carried;

        private UnitActionDirection _curDirection;
        private Unit _unit;

        private void Start()
        {
            ShowCurrentGear();
        }

        public void Init(Unit unit)
        {
            _unit = unit;
        }

        [Button("Show Current Gear")]
        private void ShowCurrentGear()
        {
            DisplayEquipmentState(EquipmentState);
        }

        public void DisplayEquipmentState(EquipmentState equipmentState)
        {
            if(equipmentState.Head != null) DisplayGear(equipmentState.Head); 
            else ClearDisplayedGear(GearType.Head);
            
            if(equipmentState.Body != null) DisplayGear(equipmentState.Body);
            else ClearDisplayedGear(GearType.Body);
            
            if(equipmentState.Pants != null) DisplayGear(equipmentState.Pants);
            else ClearDisplayedGear(GearType.Pants);
            
            if(equipmentState.MainHand != null) DisplayGear(equipmentState.MainHand);
            else ClearDisplayedGear(GearType.MainHand);
            
            if(equipmentState.OffHand != null) DisplayGear(equipmentState.OffHand);
            else ClearDisplayedGear(GearType.OffHand);
            
            if(equipmentState.Hands != null) DisplayGear(equipmentState.Hands);
            else ClearDisplayedGear(GearType.Hands);
        }

        public void AssignDirection(UnitActionDirection direction)
        {
            _curDirection = direction;
            
            if(_headGearObj != null)  _headGearObj.AssignDirection(_curDirection);
            if(_bodyGearObj != null) _bodyGearObj.AssignDirection(_curDirection);
            if(_pantsGearHipsObj != null) _pantsGearHipsObj.AssignDirection(_curDirection);
            if(_pantsGearLeftLegObj != null) _pantsGearLeftLegObj.AssignDirection(_curDirection);
            if(_pantsGearRightLegObj != null) _pantsGearRightLegObj.AssignDirection(_curDirection);
            if(_handsGearMainHandObj != null) _handsGearMainHandObj.AssignDirection(_curDirection);
            if(_handsGearOffHandObj != null) _handsGearOffHandObj.AssignDirection(_curDirection);
            if(_mainHandHeldObj != null) _mainHandHeldObj.AssignDirection(_curDirection);
            if(_offHandHeldObj != null) _offHandHeldObj.AssignDirection(_curDirection);
        }
        
        public void Equip(Item item)
        {
            var equipmentState = item.State as GearState;
            Destroy(item.gameObject);

            switch (equipmentState.GearData.Type)
            {
                case GearType.Head:
                    EquipmentState.Head = equipmentState;
                    if (EquipmentState.Head.Equals(DesiredEquipmentState.Head)) DesiredEquipmentState.Head = null;
                    break;
                case GearType.Body:
                    EquipmentState.Body = equipmentState;
                    if (EquipmentState.Body.Equals(DesiredEquipmentState.Body)) DesiredEquipmentState.Body = null;
                    break;
                case GearType.Pants:
                    EquipmentState.Pants = equipmentState;
                    if (EquipmentState.Pants.Equals(DesiredEquipmentState.Pants)) DesiredEquipmentState.Pants = null;
                    break;
                case GearType.Hands:
                    EquipmentState.Hands = equipmentState;
                    if (EquipmentState.Hands.Equals(DesiredEquipmentState.Hands)) DesiredEquipmentState.Hands = null;
                    break;
                case GearType.MainHand:
                    EquipmentState.MainHand = equipmentState;
                    if (EquipmentState.MainHand.Equals(DesiredEquipmentState.MainHand)) DesiredEquipmentState.MainHand = null;
                    break;
                case GearType.OffHand:
                    EquipmentState.OffHand = equipmentState;
                    if (EquipmentState.OffHand.Equals(DesiredEquipmentState.OffHand)) DesiredEquipmentState.OffHand = null;
                    break;
                case GearType.BothHands:
                    EquipmentState.MainHand = equipmentState;
                    if (EquipmentState.MainHand.Equals(DesiredEquipmentState.MainHand)) DesiredEquipmentState.MainHand = null;
                    break;
                case GearType.Necklace:
                    EquipmentState.Necklace = equipmentState;
                    if (EquipmentState.Necklace.Equals(DesiredEquipmentState.Necklace)) DesiredEquipmentState.Necklace = null;
                    break;
                case GearType.Ring:
                    if (EquipmentState.Ring1 == null)
                    {
                        EquipmentState.Ring1 = equipmentState;
                        if (EquipmentState.Ring1.Equals(DesiredEquipmentState.Ring1)) DesiredEquipmentState.Ring1 = null;
                    }
                    else
                    {
                        EquipmentState.Ring2 = equipmentState;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            DisplayGear(equipmentState);
            
            GameEvents.Trigger_OnKinlingChanged(_unit);
        }

        private void DisplayGear(GearState gearState)
        {
            var type = gearState.GearData.Type;
            var equip = gearState.GearData;
            ClearDisplayedGear(type);

            switch (type)
            {
                case GearType.Head:
                    if (equip.HatGear != null)
                    {
                        _headGearObj = SpawnGearOnBody(equip.HatGear, gearState, _headGear);
                    }
                    break;
                case GearType.Body:
                    if (equip.BodyGear != null)
                    {
                        _bodyGearObj = SpawnGearOnBody(equip.BodyGear, gearState, _bodyGear);
                    }
                    break;
                case GearType.Pants:
                    if (equip.HipsGear != null)
                    {
                        _pantsGearHipsObj = SpawnGearOnBody(equip.HipsGear, gearState, _pantsGearHips);
                    }
                    if (equip.LeftLegGear != null)
                    {
                        _pantsGearLeftLegObj = SpawnGearOnBody(equip.LeftLegGear, gearState, _pantsGearLeftLeg);
                    }
                    if (equip.RightLegGear != null)
                    {
                        _pantsGearRightLegObj = SpawnGearOnBody(equip.RightLegGear, gearState, _pantsGearRightLeg);
                    }
                    break;
                case GearType.Hands:
                    if (equip.MainHandGear != null)
                    {
                        _handsGearMainHandObj = SpawnGearOnBody(equip.MainHandGear, gearState, _handsGearMainHand);
                    }
                    if (equip.OffHandGear != null)
                    {
                        _handsGearOffHandObj = SpawnGearOnBody(equip.OffHandGear, gearState, _handsGearOffHand);
                    }
                    break;
                case GearType.MainHand:
                    if (equip.MainHandHeldGear != null)
                    {
                        _mainHandHeldObj = SpawnGearOnBody(equip.MainHandHeldGear, gearState, _mainHandHeld);
                    }
                    break;
                case GearType.OffHand:
                    if (equip.OffHandHeldGear != null)
                    {
                        _offHandHeldObj = SpawnGearOnBody(equip.OffHandHeldGear, gearState, _offHandHeld);
                    }
                    break;
                case GearType.BothHands:
                    if (equip.MainHandHeldGear != null)
                    {
                        _mainHandHeldObj = SpawnGearOnBody(equip.MainHandHeldGear, gearState, _mainHandHeld);
                    }
                    break;
                case GearType.Carried:
                    Debug.LogError("Carried is not built yet");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private GearPiece SpawnGearOnBody(GearPiece gearPiecePrefab, GearState gearState, Transform parent)
        {
            var gear = Instantiate(gearPiecePrefab, parent);
            gear.AssignDirection(_curDirection);
            gear.AssignDyePallet(gearState.AssignedDye);
            gear.gameObject.layer = parent.gameObject.layer;

            foreach (Transform child in gear.transform)
            {
                child.gameObject.layer = parent.gameObject.layer;
            }
            return gear;
        }

        private void ClearDisplayedGear(GearType type)
        {
            switch (type)
            {
                case GearType.Head:
                    if (_headGearObj != null)
                    {
                        Destroy(_headGearObj.gameObject);
                        _headGearObj = null;
                    }
                    break;
                case GearType.Body:
                    if (_bodyGearObj != null)
                    {
                        Destroy(_bodyGearObj.gameObject);
                        _bodyGearObj = null;
                    }
                    break;
                case GearType.Pants:
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
                case GearType.Hands:
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
                case GearType.MainHand:
                    if (_mainHandHeldObj != null)
                    {
                        Destroy(_mainHandHeldObj.gameObject);
                        _mainHandHeldObj = null;
                    }
                    break;
                case GearType.OffHand:
                    if (_offHandHeldObj != null)
                    {
                        Destroy(_offHandHeldObj.gameObject);
                        _offHandHeldObj = null;
                    }
                    break;
                case GearType.BothHands:
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
                case GearType.Carried:
                    Debug.LogError("Carried is not built yet");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public Item Unequip(GearState gear)
        {
            var data = gear.GearData;
            if (data == null) return null;

            if (!HasEquipped(data)) return null;
            
            // Remove the item and drop it
            switch (data.Type)
            {
                case GearType.Head:
                    EquipmentState.Head = null;
                    break;
                case GearType.Body:
                    EquipmentState.Body = null;
                    break;
                case GearType.Pants:
                    EquipmentState.Pants = null;
                    break;
                case GearType.Hands:
                    EquipmentState.Hands = null;
                    break;
                case GearType.MainHand:
                    EquipmentState.MainHand = null;
                    break;
                case GearType.OffHand:
                    EquipmentState.OffHand = null;
                    break;
                case GearType.BothHands:
                    EquipmentState.MainHand = null;
                    break;
                case GearType.Necklace:
                    EquipmentState.Necklace = null;
                    break;
                case GearType.Ring:
                    if (EquipmentState.Ring1.Data == data)
                    {
                        EquipmentState.Ring1 = null;
                    }
                    else
                    {
                        EquipmentState.Ring2 = null;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            ClearDisplayedGear(data.Type);
            
            var droppedItem = Spawner.Instance.SpawnItem(gear.Data, transform.position, true, gear);
            return droppedItem;
        }

        public bool HasEquipped(GearData gearData)
        {
            if (gearData == null) return true;

            switch (gearData.Type)
            {
                case GearType.Head:
                    if (EquipmentState.Head.Data == gearData) return true;
                    break;
                case GearType.Body:
                    if (EquipmentState.Body.Data == gearData) return true;
                    break;
                case GearType.Pants:
                    if (EquipmentState.Pants.Data == gearData) return true;
                    break;
                case GearType.Hands:
                    if (EquipmentState.Hands.Data == gearData) return true;
                    break;
                case GearType.MainHand:
                    if (EquipmentState.MainHand.Data == gearData) return true;
                    break;
                case GearType.OffHand:
                    if (EquipmentState.OffHand.Data == gearData) return true;
                    break;
                case GearType.BothHands:
                    if (EquipmentState.MainHand.Data == gearData) return true;
                    break;
                case GearType.Necklace:
                    if (EquipmentState.Necklace.Data == gearData) return true;
                    break;
                case GearType.Ring:
                    if (EquipmentState.Ring1.Data == gearData) return true;
                    if (EquipmentState.Ring2.Data == gearData) return true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            return false;
        }
        
        public Task CheckDesiredEquipment()
        {
            if (DesiredEquipmentState.GetGearByType(GearType.Head) != null) 
                return CreateEquipTask(DesiredEquipmentState.GetGearByType(GearType.Head));
            
            if (DesiredEquipmentState.GetGearByType(GearType.Body) != null) 
                return CreateEquipTask(DesiredEquipmentState.GetGearByType(GearType.Body));
            
            if (DesiredEquipmentState.GetGearByType(GearType.Pants) != null) 
                return CreateEquipTask(DesiredEquipmentState.GetGearByType(GearType.Pants));
            
            if (DesiredEquipmentState.GetGearByType(GearType.Hands) != null) 
                return CreateEquipTask(DesiredEquipmentState.GetGearByType(GearType.Hands));
            
            if (DesiredEquipmentState.GetGearByType(GearType.MainHand) != null) 
                return CreateEquipTask(DesiredEquipmentState.GetGearByType(GearType.MainHand));
            
            if (DesiredEquipmentState.GetGearByType(GearType.OffHand) != null) 
                return CreateEquipTask(DesiredEquipmentState.GetGearByType(GearType.OffHand));
            
            if (DesiredEquipmentState.GetGearByType(GearType.BothHands) != null) 
                return CreateEquipTask(DesiredEquipmentState.GetGearByType(GearType.BothHands));
            
            if (DesiredEquipmentState.GetGearByType(GearType.Necklace) != null) 
                return CreateEquipTask(DesiredEquipmentState.GetGearByType(GearType.Necklace));
            
            if (DesiredEquipmentState.GetGearByType(GearType.Ring) != null) 
                return CreateEquipTask(DesiredEquipmentState.GetGearByType(GearType.Ring));
            
            // Nothing desired
            return null;
        }

        private Task CreateEquipTask(GearState gearState)
        {
            Task equipItemTask = new Task("Equip Item", gearState.LinkedItem, null)
            {
                Materials = new List<Item> { gearState.LinkedItem },
            };

            return equipItemTask;
        }

        public void AssignDesiredEquipment(GearState desiredGear)
        {
            DesiredEquipmentState.SetGear(desiredGear);
        }
    }
}
