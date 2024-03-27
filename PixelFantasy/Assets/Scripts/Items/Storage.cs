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
        
        public void ForceLoadItems(List<ItemData> itemsToForceLoad, FurnitureSettings settings)
        {
            _isPlanning = false;
            var data = settings.CreateInitialDataObject();
            DataLibrary.RegisterInitializationCallback((() =>
            {
                RuntimeData = (StorageData) DataLibrary.CloneDataObjectToRuntime(data as StorageData, gameObject);
                RuntimeStorageData.InitData(settings);
                RuntimeStorageData.State = EFurnitureState.Built;
                RuntimeStorageData.Direction = _direction;
                RuntimeStorageData.LinkedFurniture = this;
                SetState(RuntimeStorageData.State);
                AssignDirection(_direction);
                
                foreach (var itemData in itemsToForceLoad)
                {
                    RuntimeStorageData.ForceDepositItem(itemData);
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