using Characters;
using Handlers;
using Items;
using Managers;
using NodeCanvas.Framework;
using UnityEngine;

namespace AI.Action_Tasks
{
    public class DepositHeldItemIntoStorageAction : ActionTask
    {
        public BBParameter<string> RequesterUID;
        public BBParameter<string> KinlingUID;

        private Kinling _kinling;
        private Item _item;
        
        protected override void OnExecute()
        {
            _kinling = KinlingsDatabase.Instance.GetKinling(KinlingUID.value);
            _item = ItemsDatabase.Instance.FindItemObject(RequesterUID.value);
            
            if(_item == null || _kinling == null) EndAction(false);
            
            _kinling.DepositHeldItemInStorage(_item.RuntimeData.AssignedStorage);
            
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
