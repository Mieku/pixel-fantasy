using System.ComponentModel;
using Characters;
using Databrain;
using Databrain.Attributes;
using ScriptableObjects;
using Sirenix.OdinInspector;
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
        public int Durability = 100;
        public EItemCategory Category;
        public Items.Item LinkedItem;
        public bool IsAllowed;
        
        
        public Task _currentTask;
        
        public string _assignedSlotUID;
        public string _assignedUnitUID;
        public bool _isHeld;
        public Kinling _carryingKinling;

        public Transform _originalParent;

        public StorageData AssignedStorage;
        public Sprite ItemSprite;
        
        
        
        public string ItemName => title;
        
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
