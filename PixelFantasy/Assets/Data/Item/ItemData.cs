using System.ComponentModel;
using Databrain;
using Databrain.Attributes;
using Managers;
using TaskSystem;
using UnityEngine;

namespace Data.Item
{
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
    
    public class ItemData : DataObject
    {
        // Settings
        [SerializeField] protected EItemCategory _category;
        [SerializeField] protected int _maxDurability;
        
        public EItemCategory Category => _category;
        public string ItemName => title;
        public Sprite ItemSprite => icon;
        
        // Runtime
        [Foldout("Runtime"), ExposeToInspector, DatabrainSerialize] public int Durability;
        [Foldout("Runtime"), ExposeToInspector, DatabrainSerialize] public bool IsAllowed;
        [Foldout("Runtime"), ExposeToInspector, DatabrainSerialize] public Task CurrentTask;
        [Foldout("Runtime"), ExposeToInspector, DatabrainSerialize] public string CarryingKinlingUID;
        [Foldout("Runtime"), ExposeToInspector, DatabrainSerialize] public StorageData AssignedStorage;
        
        [Foldout("Runtime"), ExposeToInspector, DatabrainSerialize] public Items.Item LinkedItem; // Not sure about this...

        public virtual void InitData()
        {
            Durability = _maxDurability;
            IsAllowed = true;
        }

        public void CreateItemObject(Vector2 pos)
        {
            var prefab = Resources.Load<Items.Item>($"Prefabs/ItemPrefab");
            Items.Item itemObj = Instantiate(prefab, pos, Quaternion.identity, ParentsManager.Instance.ItemsParent);
            itemObj.name = title;
            itemObj.InitializeItem(this, true);
        }
        
        public virtual string GetDetailsMsg(string headerColourCode = "#272736")
        {
            string msg = "";
            msg += $"<color={headerColourCode}>Durability:</color> <b>{Durability}</b>\n";
            return msg;
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

            if (!AssignedStorage.SetClaimedItem(this))
            {
                Debug.LogError("Failed to claim item");
            }
        }
    }
}
