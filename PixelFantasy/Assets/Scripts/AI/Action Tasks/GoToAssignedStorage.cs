using Characters;
using Handlers;
using Managers;
using NodeCanvas.Framework;

namespace AI.Action_Tasks
{
    public class GoToAssignedStorage : KinlingActionTask
    {
        public BBParameter<string> ItemUID;
        public BBParameter<string> KinlingUID;

        private Kinling _kinling;
    
        protected override void OnExecute()
        {
            _kinling = KinlingsDatabase.Instance.GetKinling(KinlingUID.value);
            var item = ItemsDatabase.Instance.FindItemObject(ItemUID.value);

            var assignedStorage = item.RuntimeData.AssignedStorage;

            var movePos = assignedStorage.AccessPosition(_kinling.transform.position, item.RuntimeData);
            if (movePos == null)
            {
                EndAction(false);
                return;
            }
            
            _kinling.KinlingAgent.SetMovePosition(movePos, OnReachedPosition, OnImpossibleMove);
        }
        
        protected override void OnStopInternal(bool interrupt)
        {
            if (interrupt)
            {
                // Stop Moving
                _kinling.KinlingAgent.SetMovePosition(_kinling.transform.position);
                
                // If they are carrying item, drop it
                if (_kinling.HeldItem != null)
                {
                    _kinling.DropCarriedItem(true);
                }
            }

            _kinling = null;
        }

        private void OnReachedPosition()
        {
            EndAction(true);
        }

        private void OnImpossibleMove()
        {
            EndAction(false);
        }
    }
}
