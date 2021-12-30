using System;
using System.Collections.Generic;
using UnityEngine;

namespace HUD
{
    public class InventoryHUD : MonoBehaviour
    {
        [SerializeField] private GameObject _inventoryResouceDisplayPrefab;

        private Dictionary<ItemData, InventoryResourceDisplay> _displayedItems = new Dictionary<ItemData, InventoryResourceDisplay>();

        private void Start()
        {
            GameEvents.OnInventoryAdded += GameEvents_OnInventoryAdded;
            GameEvents.OnInventoryRemoved += GameEvents_OnInventoryRemoved;
        }

        private void OnDestroy()
        {
            GameEvents.OnInventoryAdded -= GameEvents_OnInventoryAdded;
            GameEvents.OnInventoryRemoved -= GameEvents_OnInventoryRemoved;
        }

        private void GameEvents_OnInventoryAdded(ItemData itemData, int totalQuantity)
        {
            // Check if the item is already displayed
            if (_displayedItems.ContainsKey(itemData))
            {
                var display = _displayedItems[itemData];
                display.UpdateAmount(totalQuantity.ToString());
            }
            else
            {
                // Add this to the HUD
                var display = Instantiate(_inventoryResouceDisplayPrefab, transform).GetComponent<InventoryResourceDisplay>();
                display.Init(itemData, totalQuantity.ToString());
                _displayedItems.Add(itemData, display);
            }
        }
        
        private void GameEvents_OnInventoryRemoved(ItemData itemData, int totalQuantity)
        {
            // Check if there is none left, if so remove it from the HUD
            if (_displayedItems.ContainsKey(itemData))
            {
                if (totalQuantity > 0)
                {
                    _displayedItems[itemData].UpdateAmount(totalQuantity.ToString());
                }
                else
                {
                    var reference = _displayedItems[itemData];
                    _displayedItems.Remove(itemData);
                    Destroy(reference.gameObject);
                }
            }
        }
    }
}
