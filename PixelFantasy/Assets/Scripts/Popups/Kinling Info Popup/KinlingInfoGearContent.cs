using System;
using System.Collections.Generic;
using Characters;
using Items;
using Managers;
using ScriptableObjects;
using TaskSystem;
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
        
        private Kinling _kinling;
        private List<KinlingInfoOwnedItemSlot> _displayedOwnedItemSlots = new List<KinlingInfoOwnedItemSlot>();
        private GearContentSlot _currentGearSlot;
        private KinlingInfoOwnedItemSlot _currentSelectedItem;
        private GearState _selectedGear;

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

            GameEvents.OnKinlingChanged += GameEvent_OnKinlingChanged;
        }

        private void OnDestroy()
        {
            GameEvents.OnKinlingChanged -= GameEvent_OnKinlingChanged;
        }

        private void GameEvent_OnKinlingChanged(Kinling kinling)
        {
            if (_kinling != kinling) return;
            
            Refresh();
        }

        public void Show(Kinling kinling)
        {
            _kinling = kinling;
            
            gameObject.SetActive(true);
            Refresh();
        }

        public void Refresh()
        {
            RefreshGear();

            if (_selectedGear == null)
            {
                DisplaySlotDetails(GearType.Head);
            }
            else
            {
                DisplaySlotDetails(_selectedGear.GearSettings.Type);
            }
            
            
            StageManager.Instance.StagedKinling.ApplyAppearance(_kinling.GetAppearance().GetAppearanceState());
            StageManager.Instance.StagedKinling.ApplyEquipment(_kinling.Equipment.EquipmentState);
        }

        private void DisplaySlotDetails(GearType type)
        {
            switch (type)
            {
                case GearType.Head:
                    OnGearSlotPressed(GearType.Head, _headSlot);
                    break;
                case GearType.Body:
                    OnGearSlotPressed(GearType.Body, _bodySlot);
                    break;
                case GearType.Pants:
                    OnGearSlotPressed(GearType.Pants, _pantsSlot);
                    break;
                case GearType.Hands:
                    OnGearSlotPressed(GearType.Hands, _glovesSlot);
                    break;
                case GearType.MainHand:
                    OnGearSlotPressed(GearType.MainHand, _mainHandSlot);
                    break;
                case GearType.OffHand:
                    OnGearSlotPressed(GearType.OffHand, _offHandSlot);
                    break;
                case GearType.Necklace:
                    OnGearSlotPressed(GearType.Necklace, _necklaceSlot);
                    break;
                case GearType.Ring:
                    OnGearSlotPressed(GearType.Ring, _ring1Slot);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private void RefreshGear()
        {
            var equipment = _kinling.Equipment.EquipmentState;
            
            var head = equipment.Head;
            var body = equipment.Body;
            var pants = equipment.Pants;
            var gloves = equipment.Hands;
            var mainHand = equipment.MainHand;
            var offHand = equipment.OffHand;
            var necklace = equipment.Necklace;
            var ring1 = equipment.Ring1;
            var ring2 = equipment.Ring2;
            
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

        private void RefreshGearDetails(GearState gearState)
        {
            _selectedGear = gearState;
            
            if (gearState != null && gearState.GearSettings != null)
            {
                _gearDetailsName.text = gearState.GearSettings.ItemName;
                _gearDetails.text = $"{gearState.Durability} / {gearState.GearSettings.Durability} Durability";
                
                var curEquipped = _kinling.Equipment.EquipmentState.GetGearByType(gearState.GearSettings.Type);
                if (curEquipped != null && curEquipped.Equals(gearState))
                {
                    _unequipBtn.gameObject.SetActive(true);
                    _equipBtn.gameObject.SetActive(false);
                }
                else
                {
                    _unequipBtn.gameObject.SetActive(false);
                    _equipBtn.gameObject.SetActive(true);
                }
            }
            else
            {
                _gearDetailsName.text = "Assign Gear";
                _gearDetails.text = "";
                
                _unequipBtn.gameObject.SetActive(false);
                _equipBtn.gameObject.SetActive(false);
            }
        }
        
        private void DisplayItemsForSlot(GearType gearType)
        {
            foreach (var displayedOwnedItemSlot in _displayedOwnedItemSlots)
            {
                Destroy(displayedOwnedItemSlot.gameObject);
            }
            _displayedOwnedItemSlots.Clear();
            
            var curEquipped = _kinling.Equipment.EquipmentState.GetGearByType(gearType);
            var allAvailableItems = InventoryManager.Instance.GetAvailableInventory();

            int minItems = 1;
            List<ItemState> applicableItems = new List<ItemState>();
            foreach (var availableItem in allAvailableItems)
            {
                var equipmentData = availableItem.Key as GearSettings;
                if (equipmentData != null)
                {
                    if (equipmentData.Type == gearType)
                    {
                        minItems += availableItem.Value.Count;
                        foreach (var item in availableItem.Value)
                        {
                            var equipment = item.State as GearState;
                            if (equipment != null && equipment.CanBeEquippedByUnit(_kinling))
                            {
                                applicableItems.Add(item.State);
                            }
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
            if (curEquipped != null && curEquipped.GearSettings != null)
            {
                _displayedOwnedItemSlots[index].AssignItem(curEquipped, true);
                _displayedOwnedItemSlots[index].TriggerSelected();
                index++;
            }
            
            foreach (var item in applicableItems)
            {
                _displayedOwnedItemSlots[index].AssignItem(item);
                
                // Check if this item is in the desired equipment state, if so show the equipping indicator
                var gearState = item as GearState;
                var desired = _kinling.Equipment.DesiredEquipmentState.GetGearByType(gearState.GearSettings.Type);
                if (desired != null && desired.Equals(gearState))
                {
                    _displayedOwnedItemSlots[index].ShowEquippingIndicator(true);
                }
                else
                {
                    _displayedOwnedItemSlots[index].ShowEquippingIndicator(false);
                }
                
                index++;
            }
        }
        
        private void OnGearSlotPressed(GearType gearType, GearContentSlot pressedSlot)
        {
            if (_currentGearSlot != null)
            {
                _currentGearSlot.DisplaySelected(false);
            }

            _currentGearSlot = pressedSlot;
            _currentGearSlot.DisplaySelected(true);
            
            RefreshGearDetails(_kinling.Equipment.EquipmentState.GetGearByType(gearType));
            DisplayItemsForSlot(gearType);
        }

        private void OnInventorySlotPressed(ItemState item, KinlingInfoOwnedItemSlot slotPressed)
        {
            if (_currentSelectedItem != null)
            {
                _currentSelectedItem.DisplaySelected(false);
            }

            _currentSelectedItem = slotPressed;
            _currentSelectedItem.DisplaySelected(true);
            
            RefreshGearDetails(item as GearState);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        public void UnequipPressed()
        {
            _kinling.Equipment.Unequip(_selectedGear);
            RefreshGear();
            
            DisplaySlotDetails(_selectedGear.GearSettings.Type);
        }

        public void EquipPressed()
        {
            _kinling.Equipment.AssignDesiredEquipment(_selectedGear);
            
            RefreshGear();
            
            DisplaySlotDetails(_selectedGear.GearSettings.Type);
        }
    }
}
