using System;
using System.Collections.Generic;
using System.ComponentModel;
using Items;
using Sirenix.OdinInspector;
using Systems.Mood.Scripts;
using Systems.SmartObjects.Scripts;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "ItemData", menuName = "ItemData/ItemData", order = 1)]
    public class ItemData : ScriptableObject
    {
        [TitleGroup("Item Data")] public string ItemName;
        [TitleGroup("Item Data")] public string ItemDescription;
        [TitleGroup("Item Data")] public int MaxStackSize;
        [TitleGroup("Item Data")] [PreviewField] public Sprite ItemSprite;
        [TitleGroup("Item Data")] public Vector2 DefaultSpriteScale = Vector2.one;
        [TitleGroup("Item Data")] public int Durability = 100;
        [TitleGroup("Item Data")] public InteractionConfiguration[] InteractionConfigs;
        [TitleGroup("Item Data")] public EItemCategory Category;

        public virtual ItemState CreateState(string uid, Item item)
        {
            return new ItemState(this, uid, item);
        }

        public virtual ItemState CreateState()
        {
            string uid = $"{ItemName}_{Guid.NewGuid()}";
            return new ItemState(this, uid, null);
        }
    }
    
    public enum EItemCategory
    {
        [Description("Materials")] Materials,
        [Description("Tools")] Tool,
        [Description("Clothing")] Clothing,
        [Description("Food")] Food,
        [Description("Furniture")] Furniture,
    }
}
