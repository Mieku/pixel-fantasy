using System.Collections.Generic;
using Items;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "ItemData", menuName = "CraftedData/ItemData", order = 1)]
    public class ItemData : ScriptableObject
    {
        [TitleGroup("Item Data")] public string ItemName;
        [TitleGroup("Item Data")] public int MaxStackSize;
        [TitleGroup("Item Data")] [PreviewField] public Sprite ItemSprite;
        [TitleGroup("Item Data")] public Vector2 DefaultSpriteScale = Vector2.one;
        [TitleGroup("Item Data")] public int Durability;

        public virtual ItemState CreateState(string uid)
        {
            return new ItemState(this, uid);
        }
    }
}
