using System;
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
    }

    public enum EItemQuality
    {
        [Description("Poor")] Poor = 0,
        [Description("Common")] Common = 1,
        [Description("Remarkable")] Remarkable = 2,
        [Description("Enchanted")] Enchanted = 3,
        [Description("Mythical")] Mythical = 4,
    }
}
