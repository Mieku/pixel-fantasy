using System;
using System.Collections.Generic;
using Characters;
using Items;
using Managers;
using ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Popups.Kinling_Info_Popup
{
    public class KinlingInfoGearContent : MonoBehaviour
    {
        [SerializeField] private GearContentSlot _headSlot;
        [SerializeField] private GearContentSlot _bodySlot;
        [SerializeField] private GearContentSlot _pantsSlot;
        [SerializeField] private GearContentSlot _glovesSlot;
        [SerializeField] private GearContentSlot _mainHandSlot;
        [SerializeField] private GearContentSlot _offHandSlot;
        [SerializeField] private GearContentSlot _necklaceSlot;
        [SerializeField] private GearContentSlot _ring1Slot;
        [SerializeField] private GearContentSlot _ring2Slot;

        [SerializeField] private int _numSlotsPerRow;
        [SerializeField] private int _defaultMinSlotsShown;
        [SerializeField] private Transform _ownedItemSlotParent;
        [SerializeField] private KinlingInfoOwnedItemSlot _ownedItemSlotPrefab;
        [SerializeField] private TextMeshProUGUI _gearDetailsName;
        [SerializeField] private TextMeshProUGUI _gearDetails;
        [SerializeField] private Button _unequipBtn;
        [SerializeField] private Button _equipBtn;
        
        private Unit _unit;
        private List<KinlingInfoOwnedItemSlot> _displayedOwnedItemSlots = new List<KinlingInfoOwnedItemSlot>();
        private GearContentSlot _currentGearSlot;
        private KinlingInfoOwnedItemSlot _currentSelectedItem;

        private void Start()
        {
            _headSlot.OnPressedCallback += OnGearSlotPressed;
            _bodySlot.OnPressedCallback += OnGearSlotPressed;
            _pantsSlot.OnPressedCallback += OnGearSlotPressed;
            _glovesSlot.OnPressedCallback += OnGearSlotPressed;
            _mainHandSlot.OnPressedCallback += OnGearSlotPressed;
            _offHandSlot.OnPressedCallback += OnGearSlotPressed;
            _necklaceSlot.OnPressedCallback += OnGearSlotPressed;
            _ring1Slot.OnPressedCallback += OnGearSlotPressed;
            _ring2Slot.OnPressedCallback += OnGearSlotPressed;
        }

        public void Show(Unit unit)
        {
            _unit = unit;
            
            gameObject.SetActive(true);
            RefreshGear();
            
            OnGearSlotPressed(EquipmentType.Head, _headSlot);
        }

        private void RefreshGear()
        {
            var head = _unit.Equipment.Head;
            var body = _unit.Equipment.Body;
            var pants = _unit.Equipment.Pants;
            var gloves = _unit.Equipment.Hands;
            var mainHand = _unit.Equipment.MainHand;
            var offHand = _unit.Equipment.OffHand;
            var necklace = _unit.Equipment.Necklace;
            var ring1 = _unit.Equipment.Ring1;
            var ring2 = _unit.Equipment.Ring2;
            
            _headSlot.AssignEquipment(head);
            _bodySlot.AssignEquipment(body);
            _pantsSlot.AssignEquipment(pants);
            _glovesSlot.AssignEquipment(gloves);
            _mainHandSlot.AssignEquipment(mainHand);
            _offHandSlot.AssignEquipment(offHand);
            _necklaceSlot.AssignEquipment(necklace);
            _ring1Slot.AssignEquipment(ring1);
            _ring2Slot.AssignEquipment(ring2);
        }

        private void RefreshGearDetails(EquipmentState equipmentState)
        {
            if (equipmentState.EquipmentData != null)
            {
                _gearDetailsName.text = equipmentState.EquipmentData.ItemName;
                _gearDetails.text = $"{equipmentState.Durability} / {equipmentState.EquipmentData.Durability} Durability";
            }
            else
            {
                _gearDetailsName.text = "Assign Gear";
                _gearDetails.text = "";
            }
        }
        
        private void DisplayItemsForSlot(EquipmentType equipmentType)
        {
            foreach (var displayedOwnedItemSlot in _displayedOwnedItemSlots)
            {
                Destroy(displayedOwnedItemSlot.gameObject);
            }
            _displayedOwnedItemSlots.Clear();
            
            var curEquipped = _unit.Equipment.GetEquipmentByType(equipmentType);
            var allAvailableItems = InventoryManager.Instance.GetAvailableInventory();

            int minItems = 1;
            List<ItemState> applicableItems = new List<ItemState>();
            foreach (var availableItem in allAvailableItems)
            {
                var equipmentData = availableItem.Key as EquipmentData;
                if (equipmentData != null)
                {
                    if (equipmentData.Type == equipmentType)
                    {
                        minItems += availableItem.Value.Count;
                        foreach (var item in availableItem.Value)
                        {
                            applicableItems.Add(item);
                        }
                    }
                }
            }
            
            int extraRows = 0;
            int excess = minItems - _defaultMinSlotsShown;
            if (excess > 0)
            {
                extraRows = excess / _numSlotsPerRow;
            }

            int totalSlots = _defaultMinSlotsShown + (extraRows * _numSlotsPerRow);
            for (int i = 0; i < totalSlots; i++)
            {
                var newSlot = Instantiate(_ownedItemSlotPrefab, _ownedItemSlotParent);
                newSlot.Init(OnInventorySlotPressed);
                _displayedOwnedItemSlots.Add(newSlot);
            }
            
            int index = 0;
            if (curEquipped.EquipmentData != null)
            {
                _displayedOwnedItemSlots[index].AssignItem(curEquipped, true);
                _displayedOwnedItemSlots[index].TriggerSelected();
                index++;
            }
            
            foreach (var item in applicableItems)
            {
                _displayedOwnedItemSlots[index].AssignItem(item);
                index++;
            }
        }
        
        private void OnGearSlotPressed(EquipmentType equipmentType, GearContentSlot pressedSlot)
        {
            if (_currentGearSlot != null)
            {
                _currentGearSlot.DisplaySelected(false);
            }

            _currentGearSlot = pressedSlot;
            _currentGearSlot.DisplaySelected(true);
            
            RefreshGearDetails(_unit.Equipment.GetEquipmentByType(equipmentType));
            DisplayItemsForSlot(equipmentType);
        }

        private void OnInventorySlotPressed(ItemState item, KinlingInfoOwnedItemSlot slotPressed)
        {
            if (_currentSelectedItem != null)
            {
                _currentSelectedItem.DisplaySelected(false);
            }

            _currentSelectedItem = slotPressed;
            _currentSelectedItem.DisplaySelected(true);
            
            // TODO: The inventory needs to be saving the item's state. Not the data!
            
            RefreshGearDetails(item as EquipmentState);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}
