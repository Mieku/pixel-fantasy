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
        [SerializeField] private List<ResourceCost> _resourceCosts;

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
        
        public Sprite GetSprite(WallNeighbourConnectionInfo connections)
        {
            return WallSprites.GetWallSprite(connections);
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
