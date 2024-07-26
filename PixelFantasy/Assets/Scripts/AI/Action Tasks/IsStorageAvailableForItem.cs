using Handlers;
using Items;
using Managers;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace AI.Action_Tasks
{
    [Category("Custom/Conditions")]
    [Name("Is Storage Available for Item")]
    [Description("Returns true if there is storage")]
    public class IsStorageAvailableForItem : ConditionTask
    {
        public BBParameter<string> ItemUID;
        public bool TryToClaim;
        
        protected override bool OnCheck()
        {
            // Make sure requester is an item
            ItemData item = ItemsDatabase.Instance.Query(ItemUID.value);
            if (item == null) return false;

            IStorage storage = item.AssignedStorage;
            if (storage == null)
            {
                storage = InventoryManager.Instance.GetAvailableStorage(item.Settings);
            }

            if (TryToClaim && storage != null)
            {
                item.AssignedStorageID = storage.UniqueID;
                storage.SetIncoming(item);
            }
            
            return storage != null;
        }
    }
}
