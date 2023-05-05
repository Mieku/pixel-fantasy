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
    
        [PreviewField] public Sprite ItemSprite;
        public Vector2 DefaultSpriteScale = Vector2.one;
    }
}
