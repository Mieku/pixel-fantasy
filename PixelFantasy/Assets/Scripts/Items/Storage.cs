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
                RuntimeStorageData.InitData();
                RuntimeStorageData.State = EFurnitureState.Built;
                RuntimeStorageData.Direction = _direction;
                RuntimeStorageData.LinkedFurniture = this;
                SetState(RuntimeStorageData.State);
                AssignDirection(_direction);
                
                // Create ItemDatas
                foreach (var itemAmount in itemsToForceLoad)
                {
                    for (int i = 0; i < itemAmount.Quantity; i++)
                    {
                        var itemClone = (ItemData) DataLibrary.CloneDataObjectToRuntime(itemAmount.Item);
                        var itemData = itemClone.GetRuntimeData();
                        itemData.InitData();
                        RuntimeStorageData.ForceDepositItem(itemData);
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
    }
}