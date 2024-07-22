using Managers;
using NodeCanvas.Framework;
using UnityEngine;

namespace AI.Action_Tasks
{
    public class GoToRequester : ActionTask
    {
        public BBParameter<PlayerInteractable> Requester;
        public BBParameter<string> KinlingUID;
    
        protected override void OnExecute()
        {
            var kinling = KinlingsDatabase.Instance.GetKinling(KinlingUID.value);
            var movePos = Requester.value.UseagePosition(kinling.transform.position);
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