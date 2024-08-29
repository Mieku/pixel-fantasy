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

        private List<ItemStack> _itemStacks = new List<ItemStack>();

        public void RegisterItem(ItemData data)
        {
            _registeredItems.Add(data.UniqueID, data);
        }

        public void DeregisterItem(ItemData data)
        {
            _registeredItems.Remove(data.UniqueID);
        }
        
        public void RegisterStack(ItemStack stack)
        {
            _itemStacks.Add(stack);
        }

        public void DeregisterStack(ItemStack stack)
        {
            _itemStacks.Remove(stack);
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
        
        public Item CreateItemObject(ItemData data, Vector2 pos)
        {
            var prefab = Resources.Load<Item>($"Prefabs/ItemPrefab");
            Item itemObj = Instantiate(prefab, pos, Quaternion.identity, transform);
            itemObj.name = data.Settings.ItemName;
            itemObj.LoadItemData(data);
            return itemObj;
        }

        public ItemStack FindItemStackAtPosition(Vector2 pos)
        {
            var snappedPos = Helper.SnapToGridPos(pos);
            var stack = _itemStacks.Find(s => s.Position == snappedPos);
            return stack;
        }

        public ItemStack CreateStack(Item item1, Item item2)
        {
            var pos = Helper.SnapToGridPos(item1.transform.position);
            
            var itemData1 = item1.RuntimeData;
            var itemData2 = item2.RuntimeData;
            Destroy(item1.gameObject);
            Destroy(item2.gameObject);

            itemData1.State = EItemState.Stacked;
            itemData2.State = EItemState.Stacked;
            
            var prefab = Resources.Load<ItemStack>($"Prefabs/ItemStackPrefab");
            ItemStack itemStack = Instantiate(prefab, pos, Quaternion.identity, transform);
            itemStack.name = $"{itemData1.ItemName} Stack";

            List<string> itemUIDs = new List<string>(){ itemData1.UniqueID, itemData2.UniqueID };
            itemStack.InitStack(itemUIDs);
            return itemStack;
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
            CreateItemObject(data, data.Position);
        }
    }
}
