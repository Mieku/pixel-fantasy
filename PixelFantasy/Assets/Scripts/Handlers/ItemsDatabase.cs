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

        public ItemData CreateItemData(ItemSettings settings)
        {
            ItemData itemData = new ItemData();
            itemData.InitData(settings);
            return itemData;
        }
        
        public Item CreateItemObject(ItemData data, Vector2 pos, bool createHaulTask)
        {
            var prefab = Resources.Load<Item>($"Prefabs/ItemPrefab");
            Item itemObj = Instantiate(prefab, pos, Quaternion.identity, transform);
            itemObj.name = data.Settings.ItemName;
            itemObj.LoadItemData(data, createHaulTask);
            data.LinkedItem = itemObj;
            return itemObj;
        }

        public List<ItemData> GetItemsData()
        {
            return _registeredItems;
        }

        public void LoadItemsData(List<ItemData> loadedItemDatas)
        {
            ClearAllItems();

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
            }
        }

        public void ClearAllItems()
        {
            var items = _registeredItems.ToList();
            foreach (var itemData in items)
            {
                if (itemData.State == EItemState.Loose)
                {
                    Destroy(itemData.LinkedItem.gameObject);
                }
            }
            _registeredItems.Clear();
        }

        private void LoadLooseItem(ItemData data)
        {
            CreateItemObject(data, data.Position, data.IsAllowed);
        }

        private void LoadStoredItem(ItemData data)
        {
            
        }

        private void LoadCarriedItem(ItemData data)
        {
            
        }
    }
}
