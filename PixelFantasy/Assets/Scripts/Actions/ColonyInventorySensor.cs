using System;
using Controllers;
using Gods;
using Items;
using SGoap;
using Zones;

namespace Actions
{
    public class ColonyInventorySensor : Sensor
    {
        private InventoryController _inventory => ControllerManager.Instance.InventoryController;
        private StorageSlot _storageSlot;

        public StorageSlot FindItem(string itemName)
        {
            if (string.IsNullOrEmpty(itemName)) return null;
            
            var itemData = Librarian.Instance.GetItemData(itemName);
            _storageSlot = _inventory.ClaimItem(itemData);
            return _storageSlot;
        }

        public StorageSlot StorageSlot
        {
            get => _storageSlot;
            set => _storageSlot = value;
        }

        public void Reset()
        {
            _storageSlot = null;
        }

        private void Awake()
        {
            GameEvents.OnStorageSlotDeleted += GameEvents_OnStorageSlotDeleted;
        }

        private void OnDestroy()
        {
            GameEvents.OnStorageSlotDeleted -= GameEvents_OnStorageSlotDeleted;
        }

        private void GameEvents_OnStorageSlotDeleted(StorageSlot storageSlot)
        {
            if (_storageSlot != null && _storageSlot == storageSlot)
            {
                var currentRequest = AgentData.KinlingAgent.CurrentRequest;
                if (currentRequest != null)
                {
                    currentRequest.CancelRequest(true);   
                }
            }
        }


        public override void OnAwake()
        {
            
        }
    }
}
