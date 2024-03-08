using Managers;
using ScriptableObjects;
using UnityEngine;

namespace Items
{
    public class Storage : Furniture, IFurnitureInitializable
    {
        public Transform StoredItemParent;
        
        public StorageData StorageData => Data as StorageData;
        
        public new bool Init(FurnitureSettings settings, FurnitureVarient varient = null, DyeSettings dye = null)
        {
            if (settings is StorageSettings storageSettings)
            {
                Data = new StorageData(storageSettings, varient, dye);
                
                AssignDirection(Data.Direction);
                foreach (var spriteRenderer in _allSprites)
                {
                    _materials.Add(spriteRenderer.material);
                }
                
                return true; // Initialization successful
            }
            else
            {
                Debug.LogError("Invalid settings type provided.");
                return false; // Initialization failed
            }
        }
        
        protected override void Built_Enter()
        {
            InventoryManager.Instance.AddStorage(this);
            GameEvents.Trigger_RefreshInventoryDisplay();

            base.Built_Enter();
        }
        
        public void DepositItems(Item item)
        {
            if (!StorageData.Incoming.Contains(item))
            {
                Debug.LogError("Tried to deposit an item that was not set as incoming");
                return;
            } 
            
            item.transform.parent = StoredItemParent;
            item.AssignedStorage = this;
            item.gameObject.SetActive(false);
            StorageData.Stored.Add(item);
            StorageData.Incoming.Remove(item);
            
            GameEvents.Trigger_RefreshInventoryDisplay();
        }

        /// <summary>
        /// To be used when initializing the game or loading saves
        /// </summary>
        public void ForceDepositItem(Item item)
        {
            item.transform.parent = StoredItemParent;
            item.AssignedStorage = this;
            item.gameObject.SetActive(false);
            StorageData.Stored.Add(item);
            
            GameEvents.Trigger_RefreshInventoryDisplay();
        }
        
        public void WithdrawItem(Item item)
        {
            if (!StorageData.Stored.Contains(item))
            {
                Debug.LogError("Tried to withdraw an item that is not stored");
            }

            if (!StorageData.Claimed.Contains(item))
            {
                Debug.LogError("Tried to withdraw an item that is not claimed");
            }
            
            item.AssignedStorage = null;
            StorageData.Stored.Remove(item);
            StorageData.Claimed.Remove(item);
            
            item.gameObject.SetActive(true);
            GameEvents.Trigger_RefreshInventoryDisplay();
        }
        
        public Item WithdrawItem(ItemState itemState)
        {
            Item result = null;
            foreach (var claimedItem in StorageData.Claimed)
            {
                if (claimedItem.State.Equals(itemState))
                {
                    StorageData.Claimed.Remove(claimedItem);
                    break;
                }
            }
            
            foreach (var storedItem in StorageData.Stored)
            {
                if (storedItem.State.Equals(itemState))
                {
                    StorageData.Stored.Remove(storedItem);
                    result = storedItem;
                    break;
                }
            }

            if (result != null)
            {
                result.gameObject.SetActive(true);
            }
            
            GameEvents.Trigger_RefreshInventoryDisplay();
            return result;
        }
    }
}