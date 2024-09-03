using System.Collections.Generic;
using Managers;
using UnityEngine;

namespace HUD
{
    public class InventoryHUD : MonoBehaviour
    {
        [SerializeField] private GameObject _inventoryResouceDisplayPrefab;

        private Dictionary<ItemSettings, InventoryResourceDisplay> _displayedItems = new Dictionary<ItemSettings, InventoryResourceDisplay>();

        private void Start()
        {
            _inventoryResouceDisplayPrefab.gameObject.SetActive(false);
            
            GameEvents.OnInventoryChanged += GameEvents_OnInventoryChanged;
            
            GameEvents_OnInventoryChanged();
        }

        private void OnDestroy()
        {
            GameEvents.OnInventoryChanged -= GameEvents_OnInventoryChanged;
        }

        private void GameEvents_OnInventoryChanged()
        {
            var availableInv = InventoryManager.Instance.GetAvailableInventoryQuantities();
            List<ItemSettings> removeList = new List<ItemSettings>();
            
            // Update current displayed
            foreach (var displayedItem in _displayedItems)
            {
                if (availableInv.ContainsKey(displayedItem.Key))
                {
                    var value = availableInv[displayedItem.Key];
                    if (value > 0)
                    {
                        displayedItem.Value.UpdateAmount(availableInv[displayedItem.Key].ToString());
                    }
                    else
                    {
                        removeList.Add(displayedItem.Key);
                    }
                }
                else // No longer exists, remove
                {
                    removeList.Add(displayedItem.Key);
                }
            }
            
            // Remove No longer used
            foreach (var removeMe in removeList)
            {
                if (_displayedItems.ContainsKey(removeMe))
                {
                    var reference = _displayedItems[removeMe];
                    _displayedItems.Remove(removeMe);
                    Destroy(reference.gameObject);
                }
            }
            
            // Add in anything new
            foreach (var available in availableInv)
            {
                if (!_displayedItems.ContainsKey(available.Key))
                {
                    var display = Instantiate(_inventoryResouceDisplayPrefab, transform).GetComponent<InventoryResourceDisplay>();
                    display.gameObject.SetActive(true);
                    display.Init(available.Key, available.Value.ToString());
                    _displayedItems.Add(available.Key, display);
                }
            }
        }
    }
}
