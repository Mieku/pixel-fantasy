using System;
using Character.Interfaces;
using Pathfinding;
using UnityEngine;

namespace Character
{
    public class MovePositionAStarPathfinding : MonoBehaviour, IMovePosition
    {
        private AIPath aiPath;
        private ICharacterAnimController charAnimController;
        private Action onReachedMovePosition;
        private bool reachedDestination;

        private void Awake()
        {
            aiPath = GetComponent<AIPath>();
            charAnimController = GetComponent<ICharacterAnimController>();
        }

        public void SetMovePosition(Vector3 movePosition, Action onReachedMovePosition)
        {
            aiPath.destination = movePosition;
            reachedDestination = false;
            this.onReachedMovePosition = onReachedMovePosition;
            charAnimController.SetMovementVelocity(movePosition);
        }

        private void RefreshAnimVector()
        {
            var moveVelo = aiPath.velocity;
            charAnimController.SetMovementVelocity(moveVelo);
        }

        private void DetermineIfDestination()
        {
            if (aiPath.reachedDestination)
            {
                if (!reachedDestination)
                {
                    reachedDestination = true;
                    onReachedMovePosition.Invoke();
                }
            }
            else
            {
                reachedDestination = false;
            }
        }

        private void Update()
        {
            DetermineIfDestination();
            RefreshAnimVector();
        }
    }
}
