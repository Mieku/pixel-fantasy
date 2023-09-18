using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Buildings.Building_Panels
{
    public class BuildingInventory : MonoBehaviour
    {
        [SerializeField] private int _numInitialSlots;
        [SerializeField] private int _numSlotsPerNewLine;
        [SerializeField] private Transform _slotParent;
        [SerializeField] private BuildingInventorySlot _slotPrefab;
        [SerializeField] private BuildingLogisticsPanel _logisticsPanel;

        private Building _building;
        private List<BuildingInventorySlot> _displayedSlots = new List<BuildingInventorySlot>();

        public Building Building => _building;
        
        private void Start()
        {
            
        }

        private void OnDestroy()
        {
            
        }

        public void Close()
        {
            GameEvents.RefreshInventoryDisplay -= GameEvent_RefreshInventoryDisplay;
            
            DestroySlots();
            _logisticsPanel.ClosePanel();
        }

        public void Open(Building building)
        {
            GameEvents.RefreshInventoryDisplay += GameEvent_RefreshInventoryDisplay;
            
            _building = building;
            CreateEmptySlots();
            RefreshSlots();
        }

        // Triggered when the global inventory changes
        private void GameEvent_RefreshInventoryDisplay()
        {
            if (_building != null)
            {
                RefreshSlots();
            }
        }
        
        private void ClearAllSlots()
        {
            foreach (var slot in _displayedSlots)
            {
                slot.Clear();
            }
        }
        
        private void RefreshSlots()
        {
            ClearAllSlots();
            var buildingInventory = _building.GetBuildingInventoryQuantities();
            int slotIndex = 0;
            foreach (var itemQuantity in buildingInventory)
            {
                _displayedSlots[slotIndex].ShowItem(itemQuantity.Key, itemQuantity.Value);
                slotIndex++;
            }
            
            RefreshLogisticsSlotDisplays();
        }

        private void DestroySlots()
        {
            foreach (var displayedSlot in _displayedSlots)
            {
                Destroy(displayedSlot.gameObject);
            }
            _displayedSlots.Clear();
        }

        private void CreateEmptySlots()
        {
            var buildingInventory = _building.GetBuildingInventory();
            var totalRequired = buildingInventory.Count + _building.LogisticBills.Count;
            
            int amountSlots = 0;
            if (totalRequired < _numInitialSlots)
            {
                amountSlots = _numInitialSlots;
            }
            else
            {
                int additional = totalRequired - _numInitialSlots;
                int numLines = additional / _numSlotsPerNewLine;
                numLines++;
                amountSlots = (numLines * _numSlotsPerNewLine) + _numInitialSlots;
            }

            for (int i = 0; i < amountSlots; i++)
            {
                var slot = Instantiate(_slotPrefab, _slotParent);
                slot.Init(_logisticsPanel);
                _displayedSlots.Add(slot);
            }
        }

        private void RefreshLogisticsSlotDisplays()
        {
            foreach (var bill in _building.LogisticBills)
            {
                int filledSlotIndex = 0;
                foreach (var slot in _displayedSlots)
                {
                    if (slot.SlotItem != null || slot.Bill != null)
                    {
                        if (slot.SlotItem == bill.Item)
                        {
                            slot.AssignBill(bill);
                            break;
                        }

                        filledSlotIndex++;
                    }
                    
                }
                
                // No Stored Inventory Has Bill
                // So make an empty to include it
                _displayedSlots[filledSlotIndex].AssignBill(bill);
            }
        }

        public void CreateLogisticBill(InventoryLogisticBill newBill)
        {
            _building.AddLogisticBill(newBill);
            RefreshSlots();
        }

        public void RemoveLogisticsBill(InventoryLogisticBill billToRemove)
        {
            _building.RemoveLogisticBill(billToRemove);
            RefreshSlots();
        }

        public void UpdateLogisticBill(InventoryLogisticBill originalbill, InventoryLogisticBill newBill)
        {
            _building.UpdateLogisticBill(originalbill, newBill);
            RefreshSlots();
        }
    }
}
