using System;
using System.Collections.Generic;
using System.Linq;
using Items;
using Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Handlers
{
    public class ItemsDatabase : Singleton<ItemsDatabase>
    {
        [ShowInInspector] private Dictionary<string, ItemData> _registeredItems = new Dictionary<string, ItemData>();

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
        
        public Item FindItemObject(string uniqueID)
        {
            var itemData = Query(uniqueID);
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
        
        public Item CreateItemObject(ItemData data, Vector2 pos, bool createHaulTask)
        {
            var prefab = Resources.Load<Item>($"Prefabs/ItemPrefab");
            Item itemObj = Instantiate(prefab, pos, Quaternion.identity, transform);
            itemObj.name = data.Settings.ItemName;
            itemObj.LoadItemData(data, createHaulTask);
            return itemObj;
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
    }
}
