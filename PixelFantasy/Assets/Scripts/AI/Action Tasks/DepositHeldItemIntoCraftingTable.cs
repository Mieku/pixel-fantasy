using Characters;
using Handlers;
using Interfaces;
using Items;
using Managers;
using NodeCanvas.Framework;

namespace AI.Action_Tasks
{
    public class DepositHeldItemIntoCraftingTable : KinlingActionTask
    {
        public BBParameter<string> TablesUID;
        public BBParameter<string> KinlingUID;
        public BBParameter<string> ClaimedItemUID;

        private Kinling _kinling;
        private ItemData _item;
        private CraftingTable _table;
        
        protected override void OnExecute()
        {
            _kinling = KinlingsDatabase.Instance.GetKinling(KinlingUID.value);
            _item = ItemsDatabase.Instance.Query(ClaimedItemUID.value);
            _table = (CraftingTable) FurnitureDatabase.Instance.FindFurnitureObject(TablesUID.value);

            if (_item == null || _kinling == null || _table == null)
            {
                EndAction(false);
                return;
            }
            
            _table.ReceiveMaterial(_item);
            _kinling.RuntimeData.HeldStackID = null;
            ClaimedItemUID.value = null;
            
            EndAction(true);
        }

        protected override void OnStopInternal(bool interrupt)
        {
            if (interrupt)
            {
                // drop item
                _kinling.DropCarriedItem();
                _item.UnclaimItem();
            }

            _kinling = null;
            _item = null;
            _table = null;
        }
    }
}
