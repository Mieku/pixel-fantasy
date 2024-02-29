// using System;
// using System.Collections.Generic;
// using Managers;
// using ScriptableObjects;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;
//
// namespace Buildings.Building_Panels
// {
//     public class BuildingLogisticsPanel : MonoBehaviour
//     {
//         [SerializeField] private int _numInitialSlots;
//         [SerializeField] private int _numSlotsPerNewLine;
//         [SerializeField] private TMP_InputField _quantityIF;
//         [SerializeField] private Toggle _toggleLogistics;
//         [SerializeField] private TMP_Dropdown _logisticsTypeDD;
//         [SerializeField] private BuildingLogisticsItemSlot _logisticSlotPrefab;
//         [SerializeField] private Transform _logisticsSlotParent;
//         [SerializeField] private BuildingInventory _inventory;
//         [SerializeField] private GameObject _logisticControlsHandle;
//         
//         private ItemData _assignedItem;
//         private BuildingInventorySlot _requestorSlot;
//         private List<BuildingLogisticsItemSlot> _logisticsSlots = new List<BuildingLogisticsItemSlot>();
//         private BuildingLogisticsItemSlot _currentSelectedSlot;
//
//         private bool _logisticToggled;
//         private int _logisticValue;
//         private InventoryLogisticBill.LogisticType _logisticType;
//         private bool _panelOpened;
//
//         public void OpenLogistics(BuildingInventorySlot requestorSlot)
//         {
//             if (_requestorSlot != null)
//             {
//                 _requestorSlot.ShowSelector(false);
//             }
//             
//             gameObject.SetActive(true);
//             _panelOpened = true;
//             _requestorSlot = requestorSlot;
//             _assignedItem = _requestorSlot.SlotItem;
//             if (_assignedItem == null && _requestorSlot.Bill != null)
//             {
//                 _assignedItem = _requestorSlot.Bill.Item;
//             } 
//             
//             _requestorSlot.ShowSelector(true);
//
//             RefreshSlots();
//         }
//         
//         private void ClearAllSlots()
//         {
//             foreach (var displayedSlot in _logisticsSlots)
//             {
//                 Destroy(displayedSlot.gameObject);
//             }
//             _logisticsSlots.Clear();
//         }
//         
//         private void CreateEmptySlots(int totalRequired)
//         {
//             int amountSlots = 0;
//             if (totalRequired < _numInitialSlots)
//             {
//                 amountSlots = _numInitialSlots;
//             }
//             else
//             {
//                 int additional = totalRequired - _numInitialSlots;
//                 int numLines = additional / _numSlotsPerNewLine;
//                 numLines++;
//                 amountSlots = (numLines * _numSlotsPerNewLine) + _numInitialSlots;
//             }
//
//             for (int i = 0; i < amountSlots; i++)
//             {
//                 var slot = Instantiate(_logisticSlotPrefab, _logisticsSlotParent);
//                 _logisticsSlots.Add(slot);
//             }
//         }
//         
//         private void RefreshSlots()
//         {
//             ClearAllSlots();
//             
//             var allInventory = InventoryManager.Instance.GetAvailableInventory(true);
//             List<ItemData> possibleItems = new List<ItemData>();
//             foreach (var kvp in allInventory)
//             {
//                 if (kvp.Key != null)
//                 {
//                     possibleItems.Add(kvp.Key);
//                 }
//             }
//             CreateEmptySlots(possibleItems.Count);
//             
//             int slotIndex = 0;
//             if (_assignedItem != null)
//             {
//                 // remove the current item to ensure is on top
//                 possibleItems.Remove(_assignedItem);
//                 
//                 _logisticsSlots[0].AssignItem(_assignedItem, OnItemSelected);
//                 AssignSlotSelected(_logisticsSlots[0]);
//                 slotIndex++;
//             }
//             else
//             {
//                 _logisticControlsHandle.SetActive(false);
//             }
//
//             foreach (var possibleItem in possibleItems)
//             {
//                 _logisticsSlots[slotIndex].AssignItem(possibleItem, OnItemSelected);
//                 slotIndex++;
//             }
//         }
//
//         private void AssignSlotSelected(BuildingLogisticsItemSlot slot)
//         {
//             if (_currentSelectedSlot != null)
//             {
//                 _currentSelectedSlot.DisplaySelector(false);
//             }
//
//             _currentSelectedSlot = slot;
//             slot.DisplaySelector(true);
//             
//             _logisticControlsHandle.SetActive(_currentSelectedSlot != null);
//             
//             RefreshBillDetails(_requestorSlot.Bill);
//         }
//
//         #region Button and Event Hooks
//
//         public void ClosePanel()
//         {
//             if (_requestorSlot != null)
//             {
//                 _requestorSlot.ShowSelector(false);
//             }
//             
//             gameObject.SetActive(false);
//             _panelOpened = false;
//         }
//
//         private void RefreshBillDetails(InventoryLogisticBill bill)
//         {
//             if (bill == null)
//             {
//                 _toggleLogistics.SetIsOnWithoutNotify(false);
//             }
//             else
//             {
//                 _toggleLogistics.SetIsOnWithoutNotify(true);
//                 _logisticsTypeDD.SetValueWithoutNotify((int)bill.Type);
//                 _quantityIF.SetTextWithoutNotify(bill.Value.ToString());
//             }
//             
//             UpdateLogisticsValues();
//         }
//
//         public void OnQuantityIFChanged(string value)
//         {
//             _logisticValue = int.Parse(value);
//             CheckBill();
//         }
//
//         public void OnLogisticsToggleChanged(bool value)
//         {
//             _logisticToggled = value;
//             CheckBill();
//         }
//
//         public void OnLogisticsDropdownChanged(Int32 value)
//         {
//             _logisticType = (InventoryLogisticBill.LogisticType)value;
//             CheckBill();
//         }
//
//         private void UpdateLogisticsValues()
//         {
//             _logisticValue = int.Parse(_quantityIF.text);
//             _logisticToggled = _toggleLogistics.isOn;
//             _logisticType = (InventoryLogisticBill.LogisticType)_logisticsTypeDD.value;
//         }
//
//         private void CheckBill()
//         {
//             if (!_panelOpened) return;
//             
//             if (_logisticToggled && _currentSelectedSlot.AssignedItem != null)
//             {
//                 InventoryLogisticBill potentialBill =
//                     new InventoryLogisticBill(_logisticType, _currentSelectedSlot.AssignedItem, _logisticValue, _inventory.Building);
//
//                 if (_requestorSlot.Bill == null)
//                 {
//                     _inventory.CreateLogisticBill(potentialBill);
//                 } 
//                 else if (!_requestorSlot.Bill.IsEqualTo(potentialBill))
//                 {
//                     _inventory.UpdateLogisticBill(_requestorSlot.Bill, potentialBill);
//                 }
//             }
//
//             // Remove the bill if toggled off and there is a bill assigned
//             if (!_logisticToggled && _requestorSlot.Bill != null)
//             {
//                 _inventory.RemoveLogisticsBill(_requestorSlot.Bill);
//                 _requestorSlot.RemoveBill();
//             }
//         }
//
//         // Triggered when an item in logistics is selected
//         public void OnItemSelected(ItemData itemData, BuildingLogisticsItemSlot slot)
//         {
//             AssignSlotSelected(slot);
//         }
//
//         #endregion
//         
//         
//     }
// }
