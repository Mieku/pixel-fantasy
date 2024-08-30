using Characters;
using Handlers;
using Items;
using Managers;
using NodeCanvas.Framework;

namespace AI.Action_Tasks
{
    public class HoldItemAction : KinlingActionTask
    {
        public BBParameter<string> RequesterUID;
        public BBParameter<string> KinlingUID;
        public BBParameter<string> ItemDataUID;
        public BBParameter<string> HeldItemUID;

        private Kinling _kinling;
        private ItemStack _stack;

        protected override void OnExecute()
        {
            _kinling = KinlingsDatabase.Instance.GetKinling(KinlingUID.value);
            _stack = ItemsDatabase.Instance.FindItemObject(ItemDataUID.value);
            
            if(_stack == null || _kinling == null) EndAction(false);
            
            var heldItem = _kinling.HoldItem(_stack, ItemDataUID.value);
            HeldItemUID.value = ItemDataUID.value;
            
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
            _stack = null;
        }
    }
}
