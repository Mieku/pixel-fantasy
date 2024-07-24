using Handlers;
using Managers;
using NodeCanvas.Framework;

namespace AI.Action_Tasks
{
    public class GoToAssignedStorage : ActionTask
    {
        public BBParameter<string> ItemUID;
        public BBParameter<string> KinlingUID;
    
        protected override void OnExecute()
        {
            var kinling = KinlingsDatabase.Instance.GetKinling(KinlingUID.value);
            var item = ItemsDatabase.Instance.FindItemObject(ItemUID.value);

            var assignedStorage = item.RuntimeData.AssignedStorage;

            var movePos = assignedStorage.AccessPosition(kinling.transform.position, item.RuntimeData);
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
