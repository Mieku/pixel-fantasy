using Handlers;
using Managers;
using NodeCanvas.Framework;

namespace AI.Action_Tasks
{
    public class FindReceivingStorage : KinlingActionTask
    {
        public BBParameter<string> ItemUID;
        public BBParameter<string> ResultingStorage;

        protected override void OnExecute()
        {
            var item = ItemsDatabase.Instance.Query(ItemUID.value);
            var storage = InventoryManager.Instance.GetAvailableStorage(item.Settings);
            storage.SetIncoming(item);
            ResultingStorage.value = storage.UniqueID;
        }
    }
}
