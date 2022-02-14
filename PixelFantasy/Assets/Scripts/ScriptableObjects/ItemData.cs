using System.Collections.Generic;
using Items;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "ItemData", menuName = "CraftedData/ItemData", order = 1)]
    public class ItemData : ScriptableObject
    {
        public string ItemName;
        public int MaxStackSize;
    
        [PreviewField] public Sprite ItemSprite;
        public Vector2 DefaultSpriteScale;

        [SerializeField] private List<Option> _options;
        [SerializeField] private ConstructionMethod _constructionMethod;
        [SerializeField] private List<ItemAmount> _resourceCosts;
        [SerializeField] private float _workToCraft;
        
        public ConstructionMethod ConstructionMethod => _constructionMethod;
        public float WorkToCraft => _workToCraft;
        
        public List<ItemAmount> ResourceCosts
        {
            get
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
    }
}
