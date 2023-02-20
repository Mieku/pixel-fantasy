using SGoap;
using UnityEngine;

namespace Actions
{
    public class GoToAction : BasicAction
    {
        public AssignedInteractableSensor AssignedInteractableSensor;

        private Vector2 _target;
        public float DistanceToTarget => Vector2.Distance(_target, transform.position);

        public override bool PrePerform()
        {
            var destination = AssignedInteractableSensor.Destination;
            if (destination != null)
            {
                _target = (Vector2)destination;
            }
            
            return base.PrePerform();
        }

        public override EActionStatus Perform()
        {
            AgentData.NavMeshAgent.SetDestination(_target);

            if (DistanceToTarget <= 0.05f)
            {
                return EActionStatus.Success;
            }
            
            // Returning Running will keep this action going until we return Success.
            return EActionStatus.Running;
        }
    }
}
