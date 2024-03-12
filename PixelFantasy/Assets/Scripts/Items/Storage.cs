using Data.Item;
using Managers;
using ScriptableObjects;
using UnityEngine;

namespace Items
{
    public class Storage : Furniture
    {
        public Transform StoredItemParent;
        public StorageData StorageData => Data as Data.Item.StorageData;
        public StorageData RuntimeStorageData => RuntimeData as Data.Item.StorageData;
        
        protected override void Built_Enter()
        {
            InventoryManager.Instance.AddStorage(this);
            GameEvents.Trigger_RefreshInventoryDisplay();

            base.Built_Enter();
        }
        
        public void DepositItems(ItemData itemData)
        {
            if (!StorageData.Incoming.Contains(itemData))
            {
                Debug.LogError("Tried to deposit an item that was not set as incoming");
                return;
            } 
            
            // item.transform.parent = StoredItemParent;
            // item.AssignedStorage = this;
            // item.gameObject.SetActive(false);
            StorageData.Stored.Add(itemData);
            StorageData.Incoming.Remove(itemData);
            
            GameEvents.Trigger_RefreshInventoryDisplay();
        }

        /// <summary>
        /// To be used when initializing the game or loading saves
        /// </summary>
        public void ForceDepositItem(ItemData itemData)
        {
            // item.transform.parent = StoredItemParent;
            // item.AssignedStorage = this;
            // item.gameObject.SetActive(false);
            StorageData.Stored.Add(itemData);
            
            GameEvents.Trigger_RefreshInventoryDisplay();
        }
        
       
        
        // public Item WithdrawItem(ItemState itemState)
        // {
        //     Item result = null;
        //     foreach (var claimedItem in StorageData.Claimed)
        //     {
        //         if (claimedItem.State.Equals(itemState))
        //         {
        //             StorageData.Claimed.Remove(claimedItem);
        //             break;
        //         }
        //     }
        //     
        //     foreach (var storedItem in StorageData.Stored)
        //     {
        //         if (storedItem.State.Equals(itemState))
        //         {
        //             StorageData.Stored.Remove(storedItem);
        //             result = storedItem;
        //             break;
        //         }
        //     }
        //
        //     if (result != null)
        //     {
        //         result.gameObject.SetActive(true);
        //     }
        //     
        //     GameEvents.Trigger_RefreshInventoryDisplay();
        //     return result;
        // }
    }
}