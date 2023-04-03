using System.Collections.Generic;
using Items;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "ItemData", menuName = "CraftedData/ItemData", order = 1)]
    public class ItemData : ScriptableObject
    {
        public string ItemName;
        public int MaxStackSize;
        
        [Tooltip("The Item's Weight in Grams")] 
        public float Weight;
    
        [PreviewField] public Sprite ItemSprite;
        public Vector2 DefaultSpriteScale = Vector2.one;
        
        [BoxGroup("Crafting", centerLabel: true)][SerializeField] private ConstructionMethod _constructionMethod;
        [BoxGroup("Crafting")][HideIf("_constructionMethod", Items.ConstructionMethod.None)][SerializeField] private List<ItemAmount> _resourceCosts;
        [BoxGroup("Crafting")][HideIf("_constructionMethod", Items.ConstructionMethod.None)][SerializeField] private int _craftedQuatity = 1;
        [BoxGroup("Crafting")][HideIf("_constructionMethod", Items.ConstructionMethod.None)][SerializeField] private float _workToCraft;
        
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

        public int CraftedQuantity
        {
            get
            {
                if (_craftedQuatity == 0)
                {
                    Debug.LogError($"{ItemName}: No Qanitity Set!, returning 1");
                    return 1;
                }
                else
                {
                    return _craftedQuatity;
                }
            }
        }
    }
}
