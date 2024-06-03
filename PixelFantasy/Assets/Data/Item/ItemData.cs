using System;
using System.Collections.Generic;
using System.ComponentModel;
using Databrain;
using Databrain.Attributes;
using Items;
using Managers;
using TaskSystem;
using UnityEngine;

namespace Data.Item
{
    
    
    [DataObjectAddToRuntimeLibrary]
    public class ItemData : DataObject
    {
        // Runtime
        [ExposeToInspector, DatabrainSerialize] public int Durability;
        [ExposeToInspector, DatabrainSerialize] public EItemQuality Quality;
        [ExposeToInspector, DatabrainSerialize] public bool IsAllowed;
        [ExposeToInspector, DatabrainSerialize] public Task CurrentTask;
        [ExposeToInspector, DatabrainSerialize] public string CarryingKinlingUID;
        [ExposeToInspector, DatabrainSerialize] public IStorage AssignedStorage;
        [ExposeToInspector, DatabrainSerialize] public Vector2 Position;
        [ExposeToInspector, DatabrainSerialize] public Items.Item LinkedItem;

        [ExposeToInspector, DatabrainSerialize]  public ItemSettings Settings;

        public virtual void InitData(ItemSettings settings)
        {
            Settings = settings;
            Durability = Settings.MaxDurability;
            IsAllowed = true;
            Quality = settings.DefaultQuality;
        }

        public Items.Item CreateItemObject(Vector2 pos, bool createHaulTask)
        {
            var prefab = Resources.Load<Items.Item>($"Prefabs/ItemPrefab");
            Items.Item itemObj = Instantiate(prefab, pos, Quaternion.identity, ParentsManager.Instance.ItemsParent);
            itemObj.name = title;
            itemObj.LoadItemData(this, createHaulTask);
            LinkedItem = itemObj;
            return itemObj;
        }
        
        public void UnclaimItem()
        {
            if (AssignedStorage == null)
            {
                Debug.LogError("Tried to unclaim an item that is not assigned to storage");
                return;
            }
            
            AssignedStorage.RestoreClaimed(this);
        }

        public void ClaimItem()
        {
            if (AssignedStorage == null)
            {
                Debug.LogError("Tried to Claim an item that is not assigned to storage");
                return;
            }

            if (!AssignedStorage.ClaimItem(this))
            {
                Debug.LogError("Failed to claim item");
            }
        }

        public bool Equals(ItemData other)
        {
            return Settings == other.Settings;
        }

        /// <summary>
        /// The percentage of durability remaining ex: 0.5 = 50%
        /// </summary>
        public float DurabilityPercent
        {
            get
            {
                var percent = (float)Durability / (float)Settings.MaxDurability;
                return percent;
            }
        }

        public Color32 GetQualityColour()
        {
            switch (Quality)
            {
                case EItemQuality.Poor:
                    return Librarian.Instance.GetColour("Poor Quality");
                case EItemQuality.Common:
                    return Librarian.Instance.GetColour("Common Quality");
                case EItemQuality.Remarkable:
                    return Librarian.Instance.GetColour("Remarkable Quality");
                case EItemQuality.Excellent:
                    return Librarian.Instance.GetColour("Excellent Quality");
                case EItemQuality.Mythical:
                    return Librarian.Instance.GetColour("Mythical Quality");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Gets the item's text details for the player, for example decay or crafter... etc
        /// </summary>
        public virtual List<DetailsText> GetDetailsTexts()
        {
            List<DetailsText> results = new List<DetailsText>();
            return results;
        }
    }

    public class DetailsText
    {
        public string Header;
        public string Message;
        public bool HasHeader => !string.IsNullOrEmpty(Header);

        public DetailsText(string header, string message)
        {
            Header = header;
            Message = message;
        }
    }

    public enum EItemQuality
    {
        [Description("Poor")] Poor = 0, // grey
        [Description("Common")] Common = 1, // white
        [Description("Remarkable")] Remarkable = 2, // green
        [Description("Enchanted")] Excellent = 3, // blue
        [Description("Mythical")] Mythical = 4, // gold
    }
}
