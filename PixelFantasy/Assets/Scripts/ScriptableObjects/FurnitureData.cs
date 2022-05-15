using System;
using System.Collections.Generic;
using Gods;
using Items;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "FurnitureData", menuName = "CraftedData/FurnitureData", order = 1)]
    public class FurnitureData : ScriptableObject
    {
        [SerializeField] private string _furnitureName;
        [SerializeField] private ItemData _itemData;
        [SerializeField] private GameObject _downFurniturePrefab;
        [SerializeField] private GameObject _upFurniturePrefab;
        [SerializeField] private GameObject _leftFurniturePrefab;
        [SerializeField] private GameObject _rightFurniturePrefab;
        [SerializeField] private Sprite _iconSprite;
        [SerializeField] private float _workCost;
        [ShowIf("_constructionMethod", Items.ConstructionMethod.Hand)][SerializeField] private List<ItemAmount> _resourceCosts;
        [SerializeField] private List<string> _invalidPlacementTags = new List<string> { "Water", "Wall", "Zone", "Furniture" };
        [SerializeField] private ConstructionMethod _constructionMethod;
        [SerializeField] private List<Order> _options;
        [SerializeField] private bool _isCraftingTable;
        [SerializeField] private CraftingType _craftingType;

        public string FurnitureName => _furnitureName;
        public Sprite Icon => _iconSprite;
        public float WorkCost => _workCost;
        public ConstructionMethod ConstructionMethod => _constructionMethod;
        public bool IsCraftingTable => _isCraftingTable;
        public List<Order> Options => _options;
        public CraftingType CraftingType => _craftingType;
        public ItemData ItemData => _itemData;

        public GameObject GetFurniturePrefab(PlacementDirection direction)
        {
            switch (direction)
            {
                case PlacementDirection.Down:
                    return _downFurniturePrefab;
                case PlacementDirection.Up:
                    return _upFurniturePrefab;
                case PlacementDirection.Left:
                    return _leftFurniturePrefab;
                case PlacementDirection.Right:
                    return _rightFurniturePrefab;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }
        
        public List<ItemAmount> ResourceCosts
        {
            get
            {
                if (ConstructionMethod == ConstructionMethod.Hand)
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
                else
                {
                    return ItemData.ResourceCosts;
                }
            }
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
        
        public float GetWorkPerResource()
        {
            int totalQuantity = 0;
            foreach (var resourceCost in _resourceCosts)
            {
                totalQuantity += resourceCost.Quantity;
            }

            return WorkCost / totalQuantity;
        }

        public PlacementDirection GetNextAvailablePlacementDirection(PlacementDirection prevDirection)
        {
            var up = GetFurniturePrefab(PlacementDirection.Up);
            var down = GetFurniturePrefab(PlacementDirection.Down);
            var left = GetFurniturePrefab(PlacementDirection.Left);
            var right = GetFurniturePrefab(PlacementDirection.Right);

            switch (prevDirection)
            {
                case PlacementDirection.Down:
                    if (left != null) return PlacementDirection.Left;
                    if (up != null) return PlacementDirection.Up;
                    if (right != null) return PlacementDirection.Right;
                    return PlacementDirection.Down;
                case PlacementDirection.Left:
                    if (up != null) return PlacementDirection.Up;
                    if (right != null) return PlacementDirection.Right;
                    if (down != null) return PlacementDirection.Down;
                    return PlacementDirection.Left;
                case PlacementDirection.Up:
                    if (right != null) return PlacementDirection.Right;
                    if (down != null) return PlacementDirection.Down;
                    if (left != null) return PlacementDirection.Left;
                    return PlacementDirection.Up;
                case PlacementDirection.Right:
                    if (down != null) return PlacementDirection.Down;
                    if (left != null) return PlacementDirection.Left;
                    if (up != null) return PlacementDirection.Up;
                    return PlacementDirection.Right;
                default:
                    throw new ArgumentOutOfRangeException(nameof(prevDirection), prevDirection, null);
            }
        }
    }
}
