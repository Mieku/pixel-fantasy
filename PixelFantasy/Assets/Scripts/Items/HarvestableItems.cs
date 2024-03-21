using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data.Item;
using Databrain.Attributes;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Items
{
    [Serializable]
    public class HarvestableItems
    {
        [SerializeField] private List<ItemDropRate> _itemDrops = new List<ItemDropRate>();

        public List<ItemAmount> GetItemDrop()
        {
            if (_itemDrops == null || _itemDrops.Count == 0)
            {
                return new List<ItemAmount>();
            }

            var result = new List<ItemAmount>();
            foreach (var harvestableItem in _itemDrops)
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
            foreach (var harvestableItem in _itemDrops)
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
        
        public HarvestableItems Clone()
        {
            HarvestableItems copy = (HarvestableItems)this.MemberwiseClone();
            copy._itemDrops = this._itemDrops.Select(drop => drop.Clone()).ToList();//.Select(itemAmount => itemAmount.Clone()).ToList();
            return copy;
        }
    }

    [Serializable]
    public class ItemDropRate
    {
        [DataObjectDropdown("DataLibrary", true)] public ItemDataSettings Item;
        public int MinDrop;
        public int MaxDrop;
        
        public ItemDropRate Clone()
        {
            return new ItemDropRate
            {
                Item = this.Item,
                MinDrop = this.MinDrop,
                MaxDrop = this.MaxDrop,
            };
        }
    }
}