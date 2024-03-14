using System.Collections.Generic;
using Data.Item;
using Managers;
using ScriptableObjects;
using UnityEngine;

namespace Items
{
    public class Storage : Furniture
    {
        public StorageData RuntimeStorageData => RuntimeData as StorageData;
        
        public void ForceLoadItems(List<ItemAmount> itemsToForceLoad)
        {
            _isPlanning = false;
            DataLibrary.RegisterInitializationCallback((() =>
            {
                RuntimeData = (StorageData) DataLibrary.CloneDataObjectToRuntime(Data as StorageData, gameObject);
                RuntimeData.InitData();
                RuntimeData.State = EFurnitureState.Built;
                RuntimeData.Direction = _direction;
                SetState(RuntimeData.State);
                AssignDirection(_direction);
                
                // Create ItemDatas
                foreach (var itemAmount in itemsToForceLoad)
                {
                    for (int i = 0; i < itemAmount.Quantity; i++)
                    {
                        var itemData = (ItemData) DataLibrary.CloneDataObjectToRuntime(itemAmount.Item);
                        itemData.InitData();
                        ForceDepositItem(itemData);
                    }
                }
            }));
            DataLibrary.OnSaved += Saved;
            DataLibrary.OnLoaded += Loaded;
        }
        
        protected override void Built_Enter()
        {
            InventoryManager.Instance.AddStorage(this);
            GameEvents.Trigger_RefreshInventoryDisplay();

            base.Built_Enter();
        }
        
        public void DepositItems(ItemData itemData)
        {
            if (!RuntimeStorageData.Incoming.Contains(itemData))
            {
                Debug.LogError("Tried to deposit an item that was not set as incoming");
                return;
            } 
            
            RuntimeStorageData.Stored.Add(itemData);
            RuntimeStorageData.Incoming.Remove(itemData);
            
            GameEvents.Trigger_RefreshInventoryDisplay();
        }

        /// <summary>
        /// To be used when initializing the game or loading saves
        /// </summary>
        public void ForceDepositItem(ItemData itemData)
        {
            RuntimeStorageData.Stored.Add(itemData);
            itemData.AssignedStorage = RuntimeStorageData;
            
            GameEvents.Trigger_RefreshInventoryDisplay();
        }
    }
}