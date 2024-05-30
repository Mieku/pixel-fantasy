using System.Collections.Generic;
using Data.Item;
using Managers;
using UnityEngine;

namespace Items
{
    public class Storage : Furniture, IStorage
    {
        public StorageData RuntimeStorageData => RuntimeData as StorageData;
        
        
        // public void ForceLoadItems(List<ItemData> itemsToForceLoad, FurnitureSettings settings)
        // {
        //     _isPlanning = false;
        //     var data = settings.CreateInitialDataObject();
        //     DataLibrary.RegisterInitializationCallback((() =>
        //     {
        //         RuntimeData = (StorageData) DataLibrary.CloneDataObjectToRuntime(data as StorageData, gameObject);
        //         RuntimeStorageData.InitData(settings);
        //         RuntimeStorageData.State = EFurnitureState.Built;
        //         RuntimeStorageData.Direction = _direction;
        //         RuntimeStorageData.LinkedFurniture = this;
        //         SetState(RuntimeStorageData.State);
        //         AssignDirection(_direction);
        //         
        //         foreach (var itemData in itemsToForceLoad)
        //         {
        //             RuntimeStorageData.ForceDepositItem(itemData);
        //         }
        //     }));
        //     DataLibrary.OnSaved += Saved;
        //     DataLibrary.OnLoaded += Loaded;
        // }
        
        protected override void Built_Enter()
        {
            InventoryManager.Instance.AddStorage(this);
            GameEvents.Trigger_RefreshInventoryDisplay();

            base.Built_Enter();
        }
        
        public List<ItemData> Stored => RuntimeStorageData.Stored;
        public List<ItemData> Incoming => RuntimeStorageData.Incoming;
        public List<ItemData> Claimed => RuntimeStorageData.Claimed;
        public StorageConfigs StorageConfigs => RuntimeStorageData.StorageConfigs;

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
            OnChanged?.Invoke();
        }

        public Item WithdrawItem(ItemData itemData)
        {
            return RuntimeStorageData.WithdrawItem(itemData);
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
                stored.CreateItemObject(RuntimeStorageData.Position, true);
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
                if (incoming.LinkedItem != null)
                {
                    incoming.LinkedItem.CancelTask();
                }

                if (incoming.CurrentTask != null)
                {
                    incoming.CurrentTask.Cancel();
                }
            }
            
            for (int i = Stored.Count - 1; i >= 0; i--)
            {
                var stored = Stored[i];
                if (stored.LinkedItem != null)
                {
                    stored.LinkedItem.CancelTask();
                }

                if (stored.CurrentTask != null)
                {
                    stored.CurrentTask.Cancel();
                }
            }
        }
        
        public bool IsCategoryAllowed(EItemCategory category)
        {
            return RuntimeStorageData.StorageSettings.AcceptedCategories.Contains(category);
        }
    }
}