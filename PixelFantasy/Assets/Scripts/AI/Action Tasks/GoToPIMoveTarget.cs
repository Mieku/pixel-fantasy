using Managers;
using NodeCanvas.Framework;
using UnityEngine;

namespace AI.Action_Tasks
{
    public class GoToPIMoveTarget : ActionTask
    {
        public BBParameter<string> PIMoveTargetUID;
        public BBParameter<string> KinlingUID;
    
        protected override void OnExecute()
        {
            var kinling = KinlingsDatabase.Instance.GetKinling(KinlingUID.value);
            PlayerInteractable requester = PlayerInteractableDatabase.Instance.Query(PIMoveTargetUID.value);
            var movePos = requester.UseagePosition(kinling.transform.position);
            if (movePos == null)
            {
                EndAction(false);
                return;
            }
            
            kinling.KinlingAgent.SetMovePosition(movePos, OnReachedPosition, OnImpossibleMove);
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