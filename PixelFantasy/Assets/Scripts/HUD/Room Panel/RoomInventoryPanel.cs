using System.Collections.Generic;
using Buildings.Building_Panels.Components;
using ScriptableObjects;
using UnityEngine;
using Zones;

namespace HUD.Room_Panel
{
    public class RoomInventoryPanel : MonoBehaviour
    {
        [SerializeField] private List<InventoryDisplaySlot> _slots;
        
        private RoomZone _zone;
        private Dictionary<ItemData, int> _inventory;

        public void Show(RoomZone zone)
        {
            _zone = zone;
            gameObject.SetActive(true);
            RefreshInventory();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
        
        private void RefreshInventory()
        {
            ClearAllSlots();
            _inventory = _zone.GetRoomInventory();
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
