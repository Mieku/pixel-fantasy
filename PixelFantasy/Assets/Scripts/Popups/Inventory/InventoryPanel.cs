using System.Collections.Generic;
using Items;
using ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zones;

namespace Popups.Inventory
{
    public class InventoryPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _storageName;
        [SerializeField] private InventoryPanelSlot _slotPrefab;
        [SerializeField] private RectTransform _panelRect;
        [SerializeField] private GridLayoutGroup _grid;
        [SerializeField] private Canvas _canvas;

        private List<InventoryPanelSlot> _slots = new List<InventoryPanelSlot>();
        private const float MIN_WIDTH = 3.25f;
        private const float MIN_HEIGHT = 4.75f;
        private const float SLOT_ADDITION = 1.325f;
        private Storage _storage;

        public bool IsOpen => _canvas.gameObject.activeSelf;

        public void Init(StorageItemData storageItemData, List<StorageSlot> inventory, Storage storage)
        {
            _storage = storage;
            _canvas.gameObject.SetActive(true);

            _storageName.text = storageItemData.ItemName;
            SetupSlots(storageItemData.NumColumns, storageItemData.NumRows);
            UpdateDisplayedInventory(inventory);
            _storage.LockOutline(true, true);
        }

        private void SetupSlots(int columns, int rows)
        {
            _grid.constraintCount = columns;

            // Resize the panel
            float width = MIN_WIDTH;
            if (columns > 2)
            {
                int additionalCols = columns - 2;
                width = MIN_WIDTH + (SLOT_ADDITION * additionalCols);
            }

            float height = MIN_HEIGHT;
            if (rows > 2)
            {
                int additionalRows = rows - 2;
                height = MIN_HEIGHT + (SLOT_ADDITION * additionalRows);
            }

            _panelRect.sizeDelta = new Vector2(width, height);
            
            ClearSlots();

            for (int i = 0; i < columns * rows; i++)
            {
                var slot = Instantiate(_slotPrefab, _grid.transform).GetComponent<InventoryPanelSlot>();
                slot.Init(OnSlotPressed);
                _slots.Add(slot);
            }
        }

        public void UpdateDisplayedInventory(List<StorageSlot> inventory)
        {
            // Clear current displayed
            foreach (var slot in _slots)
            {
                slot.Clear();
            }

            int index = 0;
            foreach (var invSlot in inventory)
            {
                if (invSlot.Item != null && invSlot.NumStored > 0)
                {
                    _slots[index].SetValues(invSlot.Item.ItemSprite, invSlot.NumStored);
                    index++;
                }
            }
        }

        private void OnSlotPressed(InventoryPanelSlot slot)
        {
            
        }

        private void ClearSlots()
        {
            foreach (var slot in _slots)
            {
                Destroy(slot.gameObject);
            }
            _slots.Clear();
        }

        public void OnExitPressed()
        {
            ClearSlots();
            _canvas.gameObject.SetActive(false);
            _storage.LockOutline(false, false);
        }
    }
}
