using System.Collections.Generic;
using System.Linq;
using Databrain.Attributes;
using Items;
using Managers;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Serialization;

namespace Data.Item
{
    public class StorageData : FurnitureData
    {
        // Runtime
        [ExposeToInspector, DatabrainSerialize] public List<ItemData> Stored = new List<ItemData>();
        [ExposeToInspector, DatabrainSerialize] public List<ItemData> Incoming = new List<ItemData>();
        [ExposeToInspector, DatabrainSerialize] public List<ItemData> Claimed = new List<ItemData>();

        public StorageSettings StorageSettings => FurnitureSettings as StorageSettings;
        
        [field: ExposeToInspector]
        [field: DatabrainSerialize]
        public StorageConfigs StorageConfigs { get; private set; }

        public override void InitData(FurnitureSettings furnitureSettings)
        {
            base.InitData(furnitureSettings);
        }
        
        public int AmountCanBeDeposited(ItemSettings itemSettings)
        {
            if (!IsItemValidToStore(itemSettings))
            {
                return 0;
            }

            int maxStorage = StorageSettings.MaxStorage;
            
            return maxStorage - (Stored.Count + Incoming.Count);
        }
        
        public int AmountCanBeWithdrawn(ItemSettings itemSettings)
        {
            if (!IsItemValidToStore(itemSettings)) return 0;

            return NumStored(itemSettings) - NumClaimed(itemSettings);
        }
        
        private int NumStored(ItemSettings itemSettings)
        {
            int result = 0;
            foreach (var storedItem in Stored)
            {
                if (storedItem.Settings == itemSettings)
                {
                    result++;
                }
            }

            return result;
        }

        private int NumClaimed(ItemSettings itemSettings)
        {
            int result = 0;
            foreach (var claimedItem in Claimed)
            {
                if (claimedItem.Settings == itemSettings)
                {
                    result++;
                }
            }

            return result;
        }

        private int NumIncoming(ItemSettings itemSettings)
        {
            int result = 0;
            foreach (var incomingItem in Incoming)
            {
                if (incomingItem.Settings == itemSettings)
                {
                    result++;
                }
            }

            return result;
        }
        
        public void CancelIncoming(ItemData itemData)
        {
            if (IsSpecificItemDataIncoming(itemData))
            {
                Incoming.Remove(itemData);
            }
            
            GameEvents.Trigger_RefreshInventoryDisplay();
        }
        
        public void SetIncoming(ItemData itemData)
        {
            if (!IsItemValidToStore(itemData.Settings))
            {
                Debug.LogError("Attempting to store the wrong item category");
                return;
            }

            var availableSpace = AmountCanBeDeposited(itemData.Settings);
            if (availableSpace <= 0)
            {
                Debug.LogError("Attempted to set incoming with no space available");
                return;
            }
            
            Incoming.Add(itemData);
            GameEvents.Trigger_RefreshInventoryDisplay();
        }

        public bool IsSpecificItemDataClaimed(ItemData itemData)
        {
            foreach (var claimed in Claimed)
            {
                if (claimed == itemData)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsSpecificItemDataIncoming(ItemData itemData)
        {
            foreach (var incoming in Incoming)
            {
                if (incoming == itemData)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsSpecificItemDataStored(ItemData itemData)
        {
            foreach (var stored in Stored)
            {
                if (stored == itemData)
                {
                    return true;
                }
            }

            return false;
        }
        
        public void RestoreClaimed(ItemData itemData)
        {
            if (!IsSpecificItemDataClaimed(itemData))
            {
                Debug.LogError($"Item Claim: {itemData.guid} was not restored, was not found in claimed");
                return;
            }

            Claimed.Remove(itemData);
            GameEvents.Trigger_RefreshInventoryDisplay();
        }

        public ItemData GetItemDataOfType(ItemSettings itemSettings)
        {
            int amountClaimable = AmountCanBeWithdrawn(itemSettings);
            if (amountClaimable <= 0)
            {
                return null;
            }
            
            foreach (var storedItem in Stored)
            {
                if (storedItem.Settings == itemSettings && !IsSpecificItemDataClaimed(storedItem))
                {
                    return storedItem;
                }
            }
            
            return null;
        }
       
        public bool ClaimItem(ItemData itemToClaim)
        {
            foreach (var storedItem in Stored)
            {
                if (storedItem == itemToClaim)
                {
                    if (IsSpecificItemDataClaimed(itemToClaim))
                    {
                        Debug.LogError($"Attempted to Claim {itemToClaim.guid}, but it was already claimed");
                        return false;
                    }
                    
                    Claimed.Add(itemToClaim);
                    GameEvents.Trigger_RefreshInventoryDisplay();
                    return true;
                }
            }
            
            Debug.LogError($"Could not find {itemToClaim.guid} in storage");
            return false;
        }
        
        public bool CanItemBeClaimed(ItemData itemData)
        {
            if (IsSpecificItemDataStored(itemData))
            {
                if (IsSpecificItemDataClaimed(itemData)) return false;

                return true;
            }

            return false;
        }

        public List<ItemData> GetAvailableInventory()
        {
            List<ItemData> results = new List<ItemData>();
            
            foreach (var storedItem in Stored)
            {
                if (!IsSpecificItemDataClaimed(storedItem))
                {
                    results.Add(storedItem);
                }
            }
        
            return results;
        }
        
        public bool IsItemInStorage(ItemSettings itemSettings)
        {
            if (IsItemValidToStore(itemSettings))
            {
                foreach (var storagedItem in Stored)
                {
                    if (storagedItem.Settings == itemSettings && !IsSpecificItemDataClaimed(storagedItem))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        
        public bool IsSpecificItemInStorage(ItemData itemData)
        {
            if (IsItemValidToStore(itemData.Settings))
            {
                foreach (var storagedItem in Stored)
                {
                    if (storagedItem.Settings == itemData.Settings && !IsSpecificItemDataClaimed(itemData))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public List<ToolData> GetAllToolItems(bool includeIncoming = false)
        {
            List<ToolData> toolItems = new List<ToolData>();
            foreach (var storedItem in Stored)
            {
                if (!IsSpecificItemDataClaimed(storedItem))
                {
                    if (storedItem is ToolData)
                    {
                        toolItems.Add(storedItem as ToolData);
                    }
                }
            }
            
            if (includeIncoming)
            {
                foreach (var incomingItem in Incoming)
                {
                    if (incomingItem is ToolData)
                    {
                        toolItems.Add(incomingItem as ToolData);
                    }
                }
            }

            return toolItems;
        }
        
        public bool IsItemValidToStore(ItemSettings itemSettings)
        {
            if (StorageSettings.AcceptedCategories.Contains(EItemCategory.SpecificStorage))
            {
                if (StorageSettings.SpecificStorage.Contains(itemSettings))
                {
                    return true;
                }
            }
            
            return StorageSettings.AcceptedCategories.Contains(itemSettings.Category);
        }
        
        public Items.Item WithdrawItem(ItemData itemData)
        {
            if (!IsSpecificItemDataStored(itemData))
            {
                Debug.LogError("Tried to withdraw an item that is not stored");
            }

            if (!IsSpecificItemDataClaimed(itemData))
            {
                Debug.LogError("Tried to withdraw an item that is not claimed");
            }
            
            Stored.Remove(itemData);
            Claimed.Remove(itemData);
            
            GameEvents.Trigger_RefreshInventoryDisplay();
            OnChanged();

            var item = itemData.CreateItemObject(Position, false);
            return item;
        }
        
        public void DepositItems(Items.Item item)
        {
            var runtimeData = item.RuntimeData;
            
            if (!IsSpecificItemDataIncoming(runtimeData))
            {
                Debug.LogError("Tried to deposit an item that was not set as incoming");
                return;
            }

            runtimeData.AssignedStorage = (Storage)LinkedFurniture;
            
            Stored.Add(runtimeData);
            Incoming.Remove(runtimeData);
            
            GameEvents.Trigger_RefreshInventoryDisplay();
            OnChanged();
            
            Destroy(item.gameObject);
        }
        
        /// <summary>
        /// To be used when initializing the game or loading saves
        /// </summary>
        public void ForceDepositItem(ItemData itemData)
        {
            itemData.AssignedStorage = (Storage)LinkedFurniture;
            itemData.Position = LinkedFurniture.transform.position;
            
            Stored.Add(itemData);
            
            GameEvents.Trigger_RefreshInventoryDisplay();
        }
    }
}
