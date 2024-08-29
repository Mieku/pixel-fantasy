using System;
using System.Collections.Generic;
using System.Linq;
using AI;
using Items;
using Managers;
using Sirenix.OdinInspector;
using Systems.Game_Setup.Scripts;
using UnityEngine;

namespace Handlers
{
    public class ItemsDatabase : Singleton<ItemsDatabase>
    {
        [ShowInInspector] private Dictionary<string, ItemData> _registeredItems = new Dictionary<string, ItemData>();

        private float _seekTimer;
        
        public void RegisterItem(ItemData data)
        {
            _registeredItems.Add(data.UniqueID, data);
        }

        public void DeregisterItem(ItemData data)
        {
            _registeredItems.Remove(data.UniqueID);
        }

        public ItemData Query(string uniqueID)
        {
            return _registeredItems[uniqueID];
        }

        public List<ItemData> FindAllItemDatasAtPosition(Vector2 pos)
        {
            List<ItemData> results = new List<ItemData>();
            var snappedPos = Helper.SnapToGridPos(pos);
            foreach (var kvp in _registeredItems)
            {
                var itemPos = Helper.SnapToGridPos(kvp.Value.Position);
                
                if (itemPos == snappedPos)
                {
                    results.Add(kvp.Value);
                }
            }

            return results;
        }
        
        public Item FindItemObject(string itemDataUID)
        {
            var itemData = Query(itemDataUID);
            switch (itemData.State)
            {
                case EItemState.Loose:
                    var items = PlayerInteractableDatabase.Instance.RegisteredPlayerInteractables.OfType<Item>().ToList();
                    var result = items.Find(i => i.ContainsItemData(itemDataUID));
                    return result;
                case EItemState.BeingProcessed:
                case EItemState.Stored:
                    return null;
                case EItemState.Carried:
                    var carryingKinling = KinlingsDatabase.Instance.GetKinling(itemData.CarryingKinlingUID);
                    return carryingKinling.HeldItem;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public Item CreateItemObject(ItemData data, Vector2 pos)
        {
            var prefab = Resources.Load<Item>($"Prefabs/ItemPrefab");
            Item itemObj = Instantiate(prefab, pos, Quaternion.identity, transform);
            itemObj.name = data.Settings.ItemName;
            itemObj.LoadItemData(data);
            return itemObj;
        }

        private void Update()
        {
            if (GameManager.Instance.GameIsQuitting) return;
            if (!GameManager.Instance.GameIsLoaded) return;
            
            _seekTimer += Time.deltaTime;
            if (_seekTimer > 1f)
            {
                _seekTimer = 0;
                SearchForStorage();
            }
        }

        private void SearchForStorage()
        {
            var allItemDatas = _registeredItems.Values.ToList();
            foreach (var itemData in allItemDatas)
            {
                if (itemData.AssignedStorage == null && itemData.State is EItemState.Loose && itemData.IsAllowed)
                {
                    var storage = InventoryManager.Instance.GetAvailableStorage(itemData.Settings);
                    if (storage != null)
                    {
                        PlayerInteractable requester = FindItemObject(itemData.UniqueID);
                        if (requester != null)
                        {
                            itemData.AssignedStorageID = storage.UniqueID;
                            storage.SetIncoming(itemData);
                        
                            Task task = new Task("Store Item", $"Storing {itemData.ItemName}", ETaskType.Hauling, requester);
                            task.TaskData.Add("ItemDataUID", itemData.UniqueID);
                            TasksDatabase.Instance.AddTask(task);
                            itemData.CurrentTaskID = task.UniqueID;
                        }
                    }
                }
            }
        }

        public Dictionary<string, ItemData> SaveItemsData()
        {
            return _registeredItems;
        }

        public void LoadItemsData(Dictionary<string, ItemData> loadedItemDatas)
        {
            foreach (var kvp in loadedItemDatas)
            {
                var itemData = kvp.Value;
                RegisterItem(itemData);
                
                switch (itemData.State)
                {
                    case EItemState.Loose:
                        LoadLooseItem(itemData);
                        break;
                    case EItemState.BeingProcessed:
                    case EItemState.Stored:
                    case EItemState.Carried:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public void ClearAllItems()
        {
            var allItems = transform.GetComponentsInChildren<Item>();
            foreach (var item in allItems.Reverse())
            {
                DestroyImmediate(item.gameObject);
            }
            
            _registeredItems.Clear();
        }

        private void LoadLooseItem(ItemData data)
        {
            CreateItemObject(data, data.Position);
        }
    }
}
