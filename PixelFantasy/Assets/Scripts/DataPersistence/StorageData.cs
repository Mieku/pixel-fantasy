using System.Collections.Generic;
using Handlers;
using Items;
using Newtonsoft.Json;
using UnityEngine;

    public class StorageData : FurnitureData
    {
        // Runtime
        public List<string> StoredUIDs = new List<string>();
        public List<string> IncomingUIDs = new List<string>();
        public List<string> ClaimedUIDs = new List<string>();
        
        [JsonRequired] private StorageConfigs _storageConfigs;
        
        [JsonIgnore] public StorageSettings StorageSettings => FurnitureSettings as StorageSettings;
        [JsonIgnore] public StorageConfigs StorageConfigs => _storageConfigs;
        
        public override void InitData(FurnitureSettings furnitureSettings)
        {
            base.InitData(furnitureSettings);
            
            _storageConfigs = new StorageConfigs();
            _storageConfigs.PasteConfigs(StorageSettings.DefaultConfigs);
            IsAllowed = true;
        }
        
        public int AmountCanBeDeposited(ItemSettings itemSettings)
        {
            if (!IsItemValidToStore(itemSettings))
            {
                return 0;
            }

            int maxStorage = StorageSettings.MaxStorage;
            
            return maxStorage - (StoredUIDs.Count + IncomingUIDs.Count);
        }
        
        public int AmountCanBeWithdrawn(ItemSettings itemSettings)
        {
            if (!IsItemValidToStore(itemSettings)) return 0;

            return NumStored(itemSettings) - NumClaimed(itemSettings);
        }
        
        private int NumStored(ItemSettings itemSettings)
        {
            int result = 0;
            foreach (var storedItemUID in StoredUIDs)
            {
                var storedItem = ItemsDatabase.Instance.Query(storedItemUID);
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
            foreach (var claimedItemUID in ClaimedUIDs)
            {
                var claimedItem = ItemsDatabase.Instance.Query(claimedItemUID);
                if (claimedItem.Settings == itemSettings)
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
                IncomingUIDs.Remove(itemData.UniqueID);
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
            
            IncomingUIDs.Add(itemData.UniqueID);
            GameEvents.Trigger_RefreshInventoryDisplay();
        }

        public bool IsSpecificItemDataClaimed(ItemData itemData)
        {
            foreach (var claimed in ClaimedUIDs)
            {
                if (claimed == itemData.UniqueID)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsSpecificItemDataIncoming(ItemData itemData)
        {
            foreach (var incoming in IncomingUIDs)
            {
                if (incoming == itemData.UniqueID)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsSpecificItemDataStored(ItemData itemData)
        {
            foreach (var stored in StoredUIDs)
            {
                if (stored == itemData.UniqueID)
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
                Debug.LogError($"Item Claim: {itemData.ItemName} was not restored, was not found in claimed");
                return;
            }

            ClaimedUIDs.Remove(itemData.UniqueID);
            GameEvents.Trigger_RefreshInventoryDisplay();
        }

        public ItemData GetItemDataOfType(ItemSettings itemSettings)
        {
            int amountClaimable = AmountCanBeWithdrawn(itemSettings);
            if (amountClaimable <= 0)
            {
                return null;
            }
            
            foreach (var storedItemUID in StoredUIDs)
            {
                var storedItem = ItemsDatabase.Instance.Query(storedItemUID);
                if (storedItem.Settings == itemSettings && !IsSpecificItemDataClaimed(storedItem))
                {
                    return storedItem;
                }
            }
            
            return null;
        }
       
        public bool ClaimItem(ItemData itemToClaim)
        {
            foreach (var storedItem in StoredUIDs)
            {
                if (storedItem == itemToClaim.UniqueID)
                {
                    if (IsSpecificItemDataClaimed(itemToClaim))
                    {
                        Debug.LogError($"Attempted to Claim {itemToClaim.ItemName}, but it was already claimed");
                        return false;
                    }
                    
                    ClaimedUIDs.Add(itemToClaim.UniqueID);
                    GameEvents.Trigger_RefreshInventoryDisplay();
                    return true;
                }
            }
            
            Debug.LogError($"Could not find {itemToClaim.ItemName} in storage");
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
        
        public bool IsItemInStorage(ItemSettings itemSettings)
        {
            if (IsItemValidToStore(itemSettings))
            {
                foreach (var storedItemUID in StoredUIDs)
                {
                    var storedItem = ItemsDatabase.Instance.Query(storedItemUID);
                    if (storedItem.Settings == itemSettings && !IsSpecificItemDataClaimed(storedItem))
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
                foreach (var storedItemUID in StoredUIDs)
                {
                    var storedItem = ItemsDatabase.Instance.Query(storedItemUID);
                    if (storedItem.Settings == itemData.Settings && !IsSpecificItemDataClaimed(itemData))
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
            foreach (var storedItemUID in StoredUIDs)
            {
                var storedItem = ItemsDatabase.Instance.Query(storedItemUID);
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
                foreach (var incomingItemUID in IncomingUIDs)
                {
                    var incomingItem = ItemsDatabase.Instance.Query(incomingItemUID);
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
        
        public Items.ItemStack WithdrawItem(ItemData itemData)
        {
            if (!IsSpecificItemDataStored(itemData))
            {
                Debug.LogError("Tried to withdraw an item that is not stored");
            }

            if (!IsSpecificItemDataClaimed(itemData))
            {
                Debug.LogError("Tried to withdraw an item that is not claimed");
            }
            
            StoredUIDs.Remove(itemData.UniqueID);
            ClaimedUIDs.Remove(itemData.UniqueID);
            
            GameEvents.Trigger_RefreshInventoryDisplay();
            OnChanged();

            itemData.State = EItemState.Carried;
            itemData.AssignedStorageID = null;

            var item = ItemsDatabase.Instance.CreateItemObject(itemData, Position);
            return item;
        }
        
        public void DepositItems(ItemStack itemStack)
        {
            var runtimeDatas = itemStack.ItemDatas;
            foreach (var runtimeData in runtimeDatas)
            {
                if (!IsSpecificItemDataIncoming(runtimeData))
                {
                    Debug.LogError("Tried to deposit an item that was not set as incoming");
                    return;
                }
            
                runtimeData.AssignedStorageID = UniqueID;
                runtimeData.State = EItemState.Stored;
                runtimeData.CarryingKinlingUID = null;
                runtimeData.Position = Helper.SnapToGridPos(Position);

                StoredUIDs.Add(runtimeData.UniqueID);
                IncomingUIDs.Remove(runtimeData.UniqueID);
            }
        
            GameEvents.Trigger_RefreshInventoryDisplay();
            OnChanged();
        }

        public void LoadInItemData(ItemData itemData)
        {
            StoredUIDs.Add(itemData.UniqueID);
            GameEvents.Trigger_RefreshInventoryDisplay();
        }
        
        public List<InventoryAmount> GetInventoryAmounts()
        {
            List<InventoryAmount> results = new List<InventoryAmount>();

            foreach (var storedItemUID in StoredUIDs)
            {
                var storedItem = ItemsDatabase.Instance.Query(storedItemUID);
                var recorded = results.Find(i => i.ItemSettings == storedItem.Settings);
                if (recorded == null)
                {
                    recorded = new InventoryAmount(storedItem.Settings);
                    results.Add(recorded);
                }

                recorded.AddStored(storedItem);
            }

            foreach (var claimedItemUID in ClaimedUIDs)
            {
                var claimedItem = ItemsDatabase.Instance.Query(claimedItemUID);
                var recorded = results.Find(i => i.ItemSettings == claimedItem.Settings);
                if (recorded == null)
                {
                    recorded = new InventoryAmount(claimedItem.Settings);
                    results.Add(recorded);
                }
                
                recorded.AddClaimed(claimedItem);
            }
            
            foreach (var incomingItemUID in IncomingUIDs)
            {
                var incomingItem = ItemsDatabase.Instance.Query(incomingItemUID);
                var recorded = results.Find(i => i.ItemSettings == incomingItem.Settings);
                if (recorded == null)
                {
                    recorded = new InventoryAmount(incomingItem.Settings);
                    results.Add(recorded);
                }
                
                recorded.AddIncoming(incomingItem);
            }

            return results;
        }
    }
