using Characters;
using Managers;
using NodeCanvas.Framework;
using UnityEngine;

namespace AI.Action_Tasks
{
    public class GoToPIMoveTarget : KinlingActionTask
    {
        public BBParameter<string> PIMoveTargetUID;
        public BBParameter<string> KinlingUID;

        public bool _dropHeldItemOnInterrupt;
        private Kinling _kinling;
    
        protected override void OnExecute()
        {
            _kinling = KinlingsDatabase.Instance.GetKinling(KinlingUID.value);
            PlayerInteractable requester = PlayerInteractableDatabase.Instance.Query(PIMoveTargetUID.value);
            var movePos = requester.UseagePosition(_kinling.transform.position);
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

                if (_dropHeldItemOnInterrupt)
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