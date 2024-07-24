using System.Collections.Generic;
using Handlers;
using Managers;
using ScriptableObjects;
using UnityEngine;

namespace Items
{
    public class Storage : Furniture, IStorage
    {
        public string UniqueID => RuntimeData.UniqueID;
        public StorageData RuntimeStorageData => RuntimeData as StorageData;
        public List<ItemData> Stored => RuntimeStorageData.Stored;
        public List<ItemData> Incoming => RuntimeStorageData.Incoming;
        public List<ItemData> Claimed => RuntimeStorageData.Claimed;
        public StorageConfigs StorageConfigs => RuntimeStorageData.StorageConfigs;

        protected override void Built_Enter()
        {
            InventoryManager.Instance.AddStorage(this);
            GameEvents.Trigger_RefreshInventoryDisplay();

            base.Built_Enter();
        }
        
        public override void InitializeFurniture(FurnitureSettings furnitureSettings, PlacementDirection direction, DyeSettings dye)
        {
            var storageSettings = (StorageSettings) furnitureSettings;
            
            //FurnitureSettings = tableSettings;
            // _dyeOverride = dye;
            RuntimeData = new StorageData();
            RuntimeStorageData.InitData(storageSettings);
            RuntimeData.Direction = direction;
            
            SetState(RuntimeData.FurnitureState);
            AssignDirection(direction);
        }

        public override void LoadData(FurnitureData data)
        {
            base.LoadData(data);
        }

        public void SetIncoming(ItemData itemData)
        {
            RuntimeStorageData.SetIncoming(itemData);
        }

        public void CancelIncoming(ItemData itemData)
        {
            RuntimeStorageData.CancelIncoming(itemData);
        }

        public void DepositItems(Item item)
        {
            RuntimeStorageData.DepositItems(item);
            Destroy(item.gameObject);
            OnChanged?.Invoke();
        }

        public Item WithdrawItem(ItemData itemData)
        {
            return RuntimeStorageData.WithdrawItem(itemData);
        }

        public void LoadInItem(ItemData itemData)
        {
            RuntimeStorageData.LoadInItemData(itemData);
        }

        public bool ClaimItem(ItemData itemToClaim)
        {
            return RuntimeStorageData.ClaimItem(itemToClaim);
        }

        public void RestoreClaimed(ItemData itemData)
        {
            RuntimeStorageData.RestoreClaimed(itemData);
        }

        public bool IsItemInStorage(ItemSettings itemSettings)
        {
            return RuntimeStorageData.IsItemInStorage(itemSettings);
        }

        public bool IsSpecificItemInStorage(ItemData specificItem)
        {
            return RuntimeStorageData.IsSpecificItemInStorage(specificItem);
        }

        public ItemData GetItemDataOfType(ItemSettings itemSettings)
        {
            return RuntimeStorageData.GetItemDataOfType(itemSettings);
        }

        public int AmountCanBeDeposited(ItemSettings itemSettings)
        {
            return RuntimeStorageData.AmountCanBeDeposited(itemSettings);
        }

        public int AmountCanBeWithdrawn(ItemSettings itemSettings)
        {
            return RuntimeStorageData.AmountCanBeWithdrawn(itemSettings);
        }

        public List<InventoryAmount> GetInventoryAmounts()
        {
            return RuntimeStorageData.GetInventoryAmounts();
        }

        public List<ToolData> GetAllToolItems(bool includeIncoming = false)
        {
            return RuntimeStorageData.GetAllToolItems(includeIncoming);
        }

        public Vector2? AccessPosition(Vector2 requestorPosition, ItemData specificItem)
        {
            return UseagePosition(requestorPosition);
        }

        public override bool IsAvailable
        {
            get
            {
                bool result = base.IsAvailable;
                if (!result) return false;

                return RuntimeStorageData.IsAllowed;
            }
        }

        public int MaxCapacity => RuntimeStorageData.StorageSettings.MaxStorage;

        public int TotalAmountStored => RuntimeStorageData.Stored.Count + RuntimeStorageData.Incoming.Count;
        
        public override void CompleteDeconstruction()
        {
            InventoryManager.Instance.RemoveStorage(this);
            
            // Drop the contents
            CancelStoredItemTasks();
            
            foreach (var stored in Stored)
            {
                ItemsDatabase.Instance.CreateItemObject(stored, RuntimeStorageData.Position, true);
            }
            Stored.Clear();
            
            
            GameEvents.Trigger_RefreshInventoryDisplay();
            
            base.CompleteDeconstruction();
        }
        
        public void CancelStoredItemTasks()
        {
            for (int i = Incoming.Count - 1; i >= 0; i--)
            {
                var incoming = Incoming[i];
                if (incoming.GetLinkedItem() != null)
                {
                    incoming.GetLinkedItem().CancelTask();
                }

                if (incoming.CurrentTask != null && !string.IsNullOrEmpty(incoming.CurrentTaskID))
                {
                    incoming.CurrentTask.Cancel(true);
                }
            }
            
            for (int i = Stored.Count - 1; i >= 0; i--)
            {
                var stored = Stored[i];
                if (stored.GetLinkedItem() != null)
                {
                    stored.GetLinkedItem().CancelTask();
                }

                if (stored.CurrentTask != null && !string.IsNullOrEmpty(stored.CurrentTaskID))
                {
                    stored.CurrentTask.Cancel(true);
                }
            }
        }
        
        public bool IsCategoryAllowed(EItemCategory category)
        {
            return RuntimeStorageData.StorageSettings.AcceptedCategories.Contains(category);
        }
    }
}