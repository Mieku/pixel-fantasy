using System;
using System.Collections;
using System.Collections.Generic;
using Data.Item;
using ScriptableObjects;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Items
{
    [Serializable]
    public class HarvestableItems
    {
        [SerializeField] private List<HarvestableItem> _harvestableItems = new List<HarvestableItem>();

        public List<ItemAmount> GetItemDrop()
        {
            if (_harvestableItems == null || _harvestableItems.Count == 0)
            {
                return new List<ItemAmount>();
            }

            var result = new List<ItemAmount>();
            foreach (var harvestableItem in _harvestableItems)
            {
                int quantity = Random.Range(harvestableItem.MinDrop, harvestableItem.MaxDrop + 1);
                if (quantity > 0)
                {
                    ItemAmount drop = new ItemAmount
                    {
                        Item = harvestableItem.Item,
                        Quantity = quantity
                    };
                    result.Add(drop);
                }
            }

            return result;
        }

        public List<ItemAmount> GetDropAverages()
        {
            var results = new List<ItemAmount>();
            foreach (var harvestableItem in _harvestableItems)
            {

                int average = (harvestableItem.MinDrop + harvestableItem.MaxDrop) / 2;
                if (average < 1) average = 1;

                ItemAmount itemAmount = new ItemAmount
                {
                    Item = harvestableItem.Item,
                    Quantity = average,
                };
                results.Add(itemAmount);
            }

            return results;
        }
    }

    [Serializable]
    public class HarvestableItem
    {
        public ItemData Item;
        public int MinDrop;
        public int MaxDrop;
    }
}