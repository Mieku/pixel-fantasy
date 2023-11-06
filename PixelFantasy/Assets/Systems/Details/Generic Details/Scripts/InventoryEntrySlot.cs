using System;
using Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Generic_Details.Scripts
{
    public class InventoryEntrySlot : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _quantityDisplay;

        public StorageSlot StorageSlot;
        
        public void Init(StorageSlot storageSlot)
        {
            StorageSlot = storageSlot;

            if (StorageSlot.StoredItemData() != null && StorageSlot.NumStored > 0)
            {
                _icon.enabled = true;
                _icon.sprite = StorageSlot.StoredItemData().ItemSprite;
            }
            else
            {
                _icon.enabled = false;
            }

            if (StorageSlot.NumStored > 1)
            {
                _quantityDisplay.text = StorageSlot.NumStored.ToString();
            }
            else
            {
                _quantityDisplay.text = "";
            }
        }
    }
}
