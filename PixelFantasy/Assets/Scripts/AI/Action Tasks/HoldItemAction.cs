using Characters;
using Handlers;
using Items;
using Managers;
using NodeCanvas.Framework;

namespace AI.Action_Tasks
{
    public class HoldItemAction : ActionTask
    {
        public BBParameter<string> RequesterUID;
        public BBParameter<string> KinlingUID;
        public BBParameter<string> HeldItemUID;

        private Kinling _kinling;
        private Item _item;

        protected override void OnExecute()
        {
            _kinling = KinlingsDatabase.Instance.GetKinling(KinlingUID.value);
            _item = ItemsDatabase.Instance.FindItemObject(RequesterUID.value);
            
            if(_item == null || _kinling == null) EndAction(false);

            HeldItemUID.value = _item.UniqueID;
            _kinling.HoldItem(_item);
            
            EndAction(true);
        }

        protected override void OnStop(bool interrupt)
        {
            if (interrupt)
            {
                // drop item
                _kinling.DropCarriedItem(true);
            }

            _kinling = null;
            _item = null;
        }
    }
}
