using System;
using System.Collections.Generic;
using Items;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "StructureData", menuName = "StructureData", order = 1)]
    public class StructureData : ScriptableObject
    {
        public string StructureName;
        public DynamicWallData WallSprites;
        public float WorkCost;
        public Sprite Icon;
        
        [SerializeField] private List<ItemAmount> _resourceCosts;
        [SerializeField] private List<string> _invalidPlacementTags;
        [SerializeField] private List<Option> _options;

        public List<ItemAmount> GetResourceCosts()
        {
            List<ItemAmount> clone = new List<ItemAmount>();
            foreach (var resourceCost in _resourceCosts)
            {
                ItemAmount cost = new ItemAmount
                {
                    Item = resourceCost.Item,
                    Quantity = resourceCost.Quantity
                };
                clone.Add(cost);
            }

            return clone;
        }

        public List<string> InvalidPlacementTags
        {
            get
            {
                List<string> clone = new List<string>();
                foreach (var tag in _invalidPlacementTags)
                {
                    clone.Add(tag);
                }

                return clone;
            }
        }

        public List<Option> Options
        {
            get
            {
                List<Option> clone = new List<Option>();
                foreach (var option in _options)
                {
                    clone.Add(option);
                }

                return clone;
            }
        }

        public Sprite GetSprite(WallNeighbourConnectionInfo connections)
        {
            return WallSprites.GetWallSprite(connections);
        }
        
        public Sprite GetSprite()
        {
            return Icon;
        }

        public float GetWorkPerResource()
        {
            int totalQuantity = 0;
            foreach (var resourceCost in _resourceCosts)
            {
                totalQuantity += resourceCost.Quantity;
            }

            return WorkCost / totalQuantity;
        }
    }

    [Serializable]
    public class ItemAmount
    {
        public ItemData Item;
        public int Quantity;
    }
}
