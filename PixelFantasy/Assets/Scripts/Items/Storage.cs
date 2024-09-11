using System.Collections.Generic;
using Handlers;
using Managers;
using ScriptableObjects;
using Systems.Input_Management;
using UnityEngine;

namespace Items
{
    public class Storage : Furniture, IStorage
    {
        public StorageData RuntimeStorageData => RuntimeData as StorageData;
        public List<string> StoredUIDs => RuntimeStorageData.StoredUIDs;
        public List<string> IncomingUIDs => RuntimeStorageData.IncomingUIDs;
        public List<string> ClaimedUIDs => RuntimeStorageData.ClaimedUIDs;
        public StorageConfigs StorageConfigs => RuntimeStorageData.StorageConfigs;

        protected override void Built_Enter()
        {
            InventoryManager.Instance.AddStorage(this);
            GameEvents.Trigger_OnInventoryChanged();

            base.Built_Enter();
        }
        
        public override void InitializeFurniture(FurnitureSettings furnitureSettings, PlacementDirection direction, DyeSettings dye)
        {
            var storageSettings = (StorageSettings) furnitureSettings;
            
            // _dyeOverride = dye;
            RuntimeData = new StorageData();
            RuntimeStorageData.InitData(storageSettings);
            RuntimeData.Direction = direction;
            
            SetState(RuntimeData.FurnitureState);
            AssignDirection(direction);
            InitializeIndicators(furnitureSettings.ShowUsePositions, furnitureSettings.MinAvailableUsePositions);
        }

        public void SetIncoming(ItemData itemData)
        {
            RuntimeStorageData.SetIncoming(itemData);
        }

        public void CancelIncoming(ItemData itemData)
        {
            RuntimeStorageData.CancelIncoming(itemData);
        }

        public void DepositItems(ItemStack itemStack)
        {
            RuntimeStorageData.DepositItems(itemStack);
            Destroy(itemStack.gameObject);
            InformChanged();
        }

        public ItemStack WithdrawItem(ItemData itemData)
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

        public int TotalAmountStored => RuntimeStorageData.StoredUIDs.Count + RuntimeStorageData.IncomingUIDs.Count;
        
        public override void CompleteDeconstruction()
        {
            InventoryManager.Instance.RemoveStorage(this);
            
            // Drop the contents
            CancelStoredItemTasks();
            
            foreach (var storedUID in StoredUIDs)
            {
                var item = ItemsDatabase.Instance.Query(storedUID);
                item.State = EItemState.Loose;
                item.AssignedStorageID = null;
                ItemsDatabase.Instance.CreateItemObject(item, RuntimeStorageData.Position);
            }
            StoredUIDs.Clear();
            
            
            GameEvents.Trigger_OnInventoryChanged();
            
            base.CompleteDeconstruction();
        }
        
        public void CancelStoredItemTasks()
        {
            for (int i = IncomingUIDs.Count - 1; i >= 0; i--)
            {
                var incomingUID = IncomingUIDs[i];
                var item = ItemsDatabase.Instance.Query(incomingUID);
                if (item.CurrentTask != null && !string.IsNullOrEmpty(item.CurrentTaskID))
                {
                    item.CurrentTask.Cancel();
                }
            }
            
            for (int i = StoredUIDs.Count - 1; i >= 0; i--)
            {
                var storedUID = StoredUIDs[i];
                var item = ItemsDatabase.Instance.Query(storedUID);
                if (item.CurrentTask != null && !string.IsNullOrEmpty(item.CurrentTaskID))
                {
                    item.CurrentTask.Cancel();
                }
            }
        }
        
        public bool IsCategoryAllowed(EItemCategory category)
        {
            return RuntimeStorageData.StorageSettings.AcceptedCategories.Contains(category);
        }
    }
}