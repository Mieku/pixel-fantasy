using System;
using System.ComponentModel;
using Databrain;
using Databrain.Attributes;
using Managers;
using UnityEngine;

namespace Data.Item
{
    [Serializable]
    public enum EItemCategory
    {
        [Description("Materials")] Materials,
        [Description("Tools")] Tool,
        [Description("Clothing")] Clothing,
        [Description("Food")] Food,
        [Description("Furniture")] Furniture,
        [Description("Specific Storage")] SpecificStorage,
        [Description("Bulky Resource")] BulkyResource,
    }
    
    public class ItemSettings : DataObject
    {
        // Settings
        [SerializeField] protected EItemCategory _category;
        [SerializeField] protected int _maxDurability;
        [DataObjectDropdown(true), SerializeField] private ItemData _baseData;
        
        public EItemCategory Category => _category;
        public string ItemName => title;
        public Sprite ItemSprite => icon;
        public int MaxDurability => _maxDurability;
        
        public virtual string GetDetailsMsg(string headerColourCode = "#272736")
        {
            string msg = "";
            msg += $"<color={headerColourCode}>Durability:</color> <b>{MaxDurability}</b>\n";
            return msg;
        }

        public ItemData CreateInitialDataObject()
        {
            var dataLibrary = Librarian.Instance.DataLibrary;
            var runtimeData = (ItemData)dataLibrary.CloneDataObjectToRuntime(_baseData);
            return runtimeData;
        }
    }
}
