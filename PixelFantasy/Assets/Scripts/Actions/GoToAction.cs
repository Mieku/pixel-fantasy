using System;
using Characters.Interfaces;
using Gods;
using SGoap;
using UnityEngine;
using UnityEngine.AI;

namespace Actions
{
    public class GoToAction : BasicAction
    {
        public Transform TargetPosition;
        
        public bool IsDestinationPossible()
        {
            NavMeshPath path = new NavMeshPath();
            AgentData.NavMeshAgent.CalculatePath(TargetPosition.position, path);
            return path.status == NavMeshPathStatus.PathComplete;
        }

        public override void DynamicallyEvaluateCost()
        {
            Cost = AgentData.DistanceToTarget;
        }

        public override EActionStatus Perform()
        {
            AgentData.NavMeshAgent.SetDestination(TargetPosition.position);

            RefreshAnimVector();

            
            if (AgentData.NavMeshAgent.remainingDistance <= AgentData.NavMeshAgent.stoppingDistance)
            {
                return EActionStatus.Success;
            }
            
            // Returning Running will keep this action going until we return Success.
            return EActionStatus.Running;
        }
        
        private void RefreshAnimVector()
        {
            var moveVelo = AgentData.NavMeshAgent.velocity;
            AgentData.Animator.SetMovementVelocity(moveVelo);
        }
    }
}
