using System;
using System.Collections.Generic;
using System.Linq;
using Items;
using Managers;
using UnityEngine;

namespace Handlers
{
    public class ItemsDatabase : Singleton<ItemsDatabase>
    {
        private List<ItemData> _registeredItems = new List<ItemData>();

        public void RegisterItem(ItemData data)
        {
            _registeredItems.Add(data);
        }

        public void DeregisterItem(ItemData data)
        {
            _registeredItems.Remove(data);
        }

        public ItemData Query(string uniqueID)
        {
            return _registeredItems.Find(i => i.UniqueID == uniqueID);
        }
        
        public Item FindItemObject(string uniqueID)
        {
            var itemData = _registeredItems.Find(i => i.UniqueID == uniqueID);
            switch (itemData.State)
            {
                case EItemState.Loose:
                    var allItems = transform.GetComponentsInChildren<Item>();
                    foreach (var item in allItems)
                    {
                        if (item.RuntimeData.UniqueID == uniqueID)
                        {
                            return item;
                        }
                    }
                    return null;
                case EItemState.Stored:
                    return null;
                case EItemState.Carried:
                    var carryingKinling = KinlingsDatabase.Instance.GetKinling(itemData.CarryingKinlingUID);
                    return carryingKinling.HeldItem;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            return null;
        }
        
        public Item CreateItemObject(ItemData data, Vector2 pos, bool createHaulTask)
        {
            var prefab = Resources.Load<Item>($"Prefabs/ItemPrefab");
            Item itemObj = Instantiate(prefab, pos, Quaternion.identity, transform);
            itemObj.name = data.Settings.ItemName;
            itemObj.LoadItemData(data, createHaulTask);
            return itemObj;
        }

        public List<ItemData> GetItemsData()
        {
            return _registeredItems;
        }

        public void LoadItemsData(List<ItemData> loadedItemDatas)
        {
            foreach (var itemData in loadedItemDatas)
            {
                switch (itemData.State)
                {
                    case EItemState.Loose:
                        LoadLooseItem(itemData);
                        break;
                    case EItemState.Stored:
                        LoadStoredItem(itemData);
                        break;
                    case EItemState.Carried:
                        LoadCarriedItem(itemData);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                RegisterItem(itemData);
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
            CreateItemObject(data, data.Position, data.IsAllowed);
        }

        private void LoadStoredItem(ItemData data)
        {
            // Loaded by the storage
        }

        private void LoadCarriedItem(ItemData data)
        {
            // Loaded by the Kinling
        }
    }
}
