using Characters;
using Handlers;
using Items;
using Managers;
using NodeCanvas.Framework;
using UnityEngine;

namespace AI.Action_Tasks
{
    public class DepositFuelAction : KinlingActionTask
    {
        public BBParameter<string> RequesterUID;
        public BBParameter<string> KinlingUID;
        public BBParameter<string> ClaimedItemUID;

        private Kinling _kinling;
        private ItemData _item;
        private IFueledFurniture _requester;
        
        protected override void OnExecute()
        {
            _kinling = KinlingsDatabase.Instance.GetKinling(KinlingUID.value);
            _item = ItemsDatabase.Instance.Query(ClaimedItemUID.value);
            _requester = (IFueledFurniture)PlayerInteractableDatabase.Instance.Query(RequesterUID.value);
            
            if(_item == null || _kinling == null) EndAction(false);
            
            _requester.Refuel(_item);
            _kinling.RuntimeData.HeldStackID = null;
            
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
