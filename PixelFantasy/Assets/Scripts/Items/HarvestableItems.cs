using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Items
{
    [Serializable]
    public class HarvestableItems
    {
        [SerializeField] private List<ItemDropRate> _itemDrops = new List<ItemDropRate>();

        public List<CostSettings> GetItemDrop()
        {
            if (_itemDrops == null || _itemDrops.Count == 0)
            {
                return new List<CostSettings>();
            }

            var result = new List<CostSettings>();
            foreach (var harvestableItem in _itemDrops)
            {
                int quantity = Random.Range(harvestableItem.MinDrop, harvestableItem.MaxDrop + 1);
                if (quantity > 0)
                {
                    CostSettings drop = new CostSettings
                    {
                        Item = harvestableItem.Item,
                        Quantity = quantity
                    };
                    result.Add(drop);
                }
            }

            return result;
        }

        public List<CostSettings> GetDropAverages()
        {
            var results = new List<CostSettings>();
            foreach (var harvestableItem in _itemDrops)
            {

                int average = (harvestableItem.MinDrop + harvestableItem.MaxDrop) / 2;
                if (average < 1) average = 1;

                CostSettings costSettings = new CostSettings
                {
                    Item = harvestableItem.Item,
                    Quantity = average,
                };
                results.Add(costSettings);
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
        public ItemSettings Item;
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