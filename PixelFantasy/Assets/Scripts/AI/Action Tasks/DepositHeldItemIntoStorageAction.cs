using Characters;
using Handlers;
using Items;
using Managers;
using NodeCanvas.Framework;
using UnityEngine;

namespace AI.Action_Tasks
{
    public class DepositHeldItemIntoStorageAction : KinlingActionTask
    {
        public BBParameter<string> ItemsUID;
        public BBParameter<string> KinlingUID;

        private Kinling _kinling;
        private ItemData _item;
        
        protected override void OnExecute()
        {
            _kinling = KinlingsDatabase.Instance.GetKinling(KinlingUID.value);
            _item = ItemsDatabase.Instance.Query(ItemsUID.value);

            if (_item == null || _kinling == null)
            {
                EndAction(false);
                return;
            }
            
            _kinling.DepositHeldItemInStorage(_item.AssignedStorage);
            
            EndAction(true);
        }

        protected override void OnStopInternal(bool interrupt)
        {
            if (interrupt)
            {
                // drop item
                _kinling.DropCarriedItem();
            }

            _kinling = null;
            _item = null;
        }
    }
}
