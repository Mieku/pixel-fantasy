using System.Collections.Generic;
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
        [TitleGroup("Item Data")] public int MaxStackSize;
        [TitleGroup("Item Data")] [PreviewField] public Sprite ItemSprite;
        [TitleGroup("Item Data")] public Vector2 DefaultSpriteScale = Vector2.one;
        [TitleGroup("Item Data")] public int Durability = 100;
        [TitleGroup("Item Data")] public InteractionConfiguration[] InteractionConfigs;

        public virtual ItemState CreateState(string uid, Item item)
        {
            return new ItemState(this, uid, item);
        }
    }
}
