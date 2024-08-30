using Managers;
using NodeCanvas.Framework;

namespace AI.Action_Tasks
{
    public class DropItemAction : KinlingActionTask
    {
        public BBParameter<string> KinlingUID;
        public BBParameter<string> DroppedItemUID;
        
        protected override void OnExecute()
        {
            var kinling = KinlingsDatabase.Instance.GetKinling(KinlingUID.value);
            var droppedItem = kinling.DropCarriedItem();
            if (droppedItem != null)
            {
                DroppedItemUID.value = droppedItem.UniqueID;
                EndAction(true);
            }
            else
            {
                EndAction(false);
            }
        }
    }
}
