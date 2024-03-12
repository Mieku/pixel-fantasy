using System;
using System.ComponentModel;
using Data.Item;
using Items;
using Sirenix.OdinInspector;
using Systems.SmartObjects.Scripts;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "ItemSettings", menuName = "Settings/Items/Item Settings")]
    public class ItemSettings : ScriptableObject
    {
        [TitleGroup("Item Settings")] public string ItemName;
        [TitleGroup("Item Settings")] public string ItemDescription;
        [TitleGroup("Item Settings")] [PreviewField] public Sprite ItemSprite;
        [TitleGroup("Item Settings")] public int Durability = 100;
        [TitleGroup("Item Settings")] public EItemCategory Category;

        // public virtual ItemState CreateState(string uid, Item item)
        // {
        //     return new ItemState(this, uid, item);
        // }
        //
        // public virtual ItemState CreateState()
        // {
        //     string uid = $"{ItemName}_{Guid.NewGuid()}";
        //     return new ItemState(this, uid, null);
        // }
        //
        // public virtual string GetDetailsMsg(string headerColourCode = "#272736")
        // {
        //     string msg = "";
        //     msg += $"<color={headerColourCode}>Durability:</color> <b>{Durability}</b>\n";
        //     return msg;
        // }
    }
    
    // public enum EItemCategory
    // {
    //     [Description("Materials")] Materials,
    //     [Description("Tools")] Tool,
    //     [Description("Clothing")] Clothing,
    //     [Description("Food")] Food,
    //     [Description("Furniture")] Furniture,
    //     [Description("Specific Storage")] SpecificStorage,
    //     [Description("Bulky Resource")] BulkyResource,
    // }
}
