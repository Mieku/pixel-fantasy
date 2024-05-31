using System.Collections.Generic;
using System.Linq;
using Items;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Generic_Details.Scripts
{
    public class StorageInventoryDisplay : MonoBehaviour
    {
        [BoxGroup("Capacity"), SerializeField] private Image _capacityFill;
        [BoxGroup("Capacity"), SerializeField] private TextMeshProUGUI _capacityDetails;

        [BoxGroup("Inventory"), SerializeField] private InventoryItemDisplay _itemDisplayPrefab;
        [BoxGroup("Inventory"), SerializeField] private Transform _itemDisplayParent;
        private List<InventoryItemDisplay> _displayedInventory = new List<InventoryItemDisplay>();

        private IStorage _storage;

        public void RefreshDisplay(IStorage storage)
        {
            _itemDisplayPrefab.gameObject.SetActive(false);
            _storage = storage;
            
            RefreshCapacityDisplay();
            RefreshInventory();
        }

        public void ClearDisplay()
        {
            ClearDisplayedInventory();
        }

        private void RefreshInventory()
        {
            var inventoryAmounts = _storage.GetInventoryAmounts();
            
            // Clear no longer used inventory
            foreach (var existingDisplay in _displayedInventory.ToList())
            {
                var respectiveAmount = inventoryAmounts.Find(i => i.ItemSettings == existingDisplay.ItemSettings);
                if (respectiveAmount == null)
                {
                    _displayedInventory.Remove(existingDisplay);
                    Destroy(existingDisplay.gameObject);
                }
            }

            foreach (var inventoryAmount in inventoryAmounts)
            {
                var existingDisplay = _displayedInventory.Find(d => d.ItemSettings == inventoryAmount.ItemSettings);
                if (existingDisplay != null)
                {
                    existingDisplay.Init(inventoryAmount);
                }
                else
                {
                    var display = Instantiate(_itemDisplayPrefab, _itemDisplayParent);
                    display.gameObject.SetActive(true);
                    display.Init(inventoryAmount);
                    _displayedInventory.Add(display);
                }
            }
        }

        private void ClearDisplayedInventory()
        {
            foreach (var invDisplay in _displayedInventory)
            {
                Destroy(invDisplay.gameObject);
            }
            _displayedInventory.Clear();
        }
        
        private void RefreshCapacityDisplay()
        {
            int maxCapacity = _storage.MaxCapacity;
            int totalStored = _storage.TotalAmountStored;
            float percentFull = (float)totalStored / maxCapacity;

            _capacityFill.fillAmount = percentFull;
            _capacityDetails.text = $"{totalStored} / {maxCapacity}";
        }
    }
}
