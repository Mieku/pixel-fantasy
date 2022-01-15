using System;
using System.Collections.Generic;
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
        
        [SerializeField] private List<ResourceCost> _resourceCosts;
        [SerializeField] private List<string> _invalidPlacementTags;

        public List<ResourceCost> GetResourceCosts()
        {
            List<ResourceCost> clone = new List<ResourceCost>();
            foreach (var resourceCost in _resourceCosts)
            {
                ResourceCost cost = new ResourceCost
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
    public class ResourceCost
    {
        public ItemData Item;
        public int Quantity;
    }
}
