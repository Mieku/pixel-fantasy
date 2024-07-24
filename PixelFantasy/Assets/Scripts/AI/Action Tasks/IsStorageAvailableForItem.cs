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
        public BBParameter<string> RequesterUID;
        
        protected override bool OnCheck()
        {
            // Make sure requester is an item
            Item item = ItemsDatabase.Instance.FindItemObject(RequesterUID.value);
            if (item == null) return false;

            IStorage storage = item.RuntimeData.AssignedStorage;
            if (storage == null)
            {
                storage = InventoryManager.Instance.GetAvailableStorage(item.RuntimeData.Settings);
            }
            
            return storage != null;
        }
    }
}
