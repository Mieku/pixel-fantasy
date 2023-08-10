using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;

namespace Buildings.Building_Panels.Components
{
    public class BuildingInventoryDisplay : MonoBehaviour
    {
        [SerializeField] private GameObject _inventoryPanel;
        [SerializeField] private List<InventoryDisplaySlot> _slots;

        private BuildingPanel _buildingPanel;
        private bool _isOpen;
        private Dictionary<ItemData, int> _inventory;

        private void Start()
        {
            _buildingPanel = gameObject.GetComponentInParent<BuildingPanel>();
            ClosePanel();
        }

        public void ExitPressed()
        {
            ClosePanel();
        }

        public void ShowInventoryPressed()
        {
            if (_isOpen)
            {
                ClosePanel();
            }
            else
            {
                OpenPanel();
            }
        }

        private void OpenPanel()
        {
            _inventoryPanel.SetActive(true);
            _isOpen = true;
            RefreshInventory();
        }

        private void ClosePanel()
        {
            _inventoryPanel.SetActive(false);
            _isOpen = false;
        }

        private void RefreshInventory()
        {
            ClearAllSlots();
            _inventory = _buildingPanel.buildingOld.GetBuildingInventory();
            int slotIndex = 0;
            if (_inventory != null)
            {
                foreach (var invKVPair in _inventory)
                {
                    _slots[slotIndex].ShowItem(invKVPair.Key, invKVPair.Value);
                    slotIndex++;
                }
            }
        }

        private void ClearAllSlots()
        {
            foreach (var slot in _slots)
            {
                slot.Clear();
            }
        }
    }
}
