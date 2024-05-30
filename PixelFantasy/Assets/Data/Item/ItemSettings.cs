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
    
    [Serializable]
    public class ItemSettings : DataObject
    {
        // Settings
        [SerializeField] protected EItemCategory _category;
        [SerializeField] protected int _maxDurability;
        [SerializeField] protected int _maxStackSize = 25;
        [SerializeField] protected bool _hasQuality = true;
        [SerializeField] protected EItemQuality _defaultQuality = EItemQuality.Common;
        
        [DataObjectDropdown(true), SerializeField] private ItemData _baseData;
        
        public EItemCategory Category => _category;
        public string ItemName => title;
        public Sprite ItemSprite => icon;
        public int MaxDurability => _maxDurability;
        public int MaxStackSize => _maxStackSize;

        public bool CanBeStored => _maxStackSize > 0;
        public ItemData BaseData => _baseData;
        
        public EItemQuality DefaultQuality
        {
            get
            {
                if (_hasQuality)
                {
                    return _defaultQuality;
                }

                return EItemQuality.Common;
            }
        }
        
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
