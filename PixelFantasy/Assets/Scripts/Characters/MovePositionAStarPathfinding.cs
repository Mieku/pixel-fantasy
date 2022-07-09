using System;
using Characters.Interfaces;
using Gods;
using Pathfinding;
using UnityEngine;

namespace Characters
{
    public class MovePositionAStarPathfinding : MonoBehaviour, IMovePosition
    {
        private AIPath aiPath;
        private ICharacterAnimController charAnimController;
        private Action onReachedMovePosition;
        private bool reachedDestination;
        private Vector3 _movePosition;

        private float defaultSpeed;
        private float defaultSlowdownDist;

        private void Awake()
        {
            aiPath = GetComponent<AIPath>();
            charAnimController = GetComponent<ICharacterAnimController>();

            defaultSpeed = aiPath.maxSpeed;
            defaultSlowdownDist = aiPath.slowdownDistance;
        }
       
        private void OnSpeedUpdated()
        {
            var speedMod = TimeManager.Instance.GameSpeedMod;
            aiPath.maxSpeed = defaultSpeed * speedMod;
            aiPath.slowdownDistance = defaultSlowdownDist * speedMod;
        }

        public void SetMovePosition(Vector3 movePosition, Action onReachedMovePosition)
        {
            _movePosition = movePosition;
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
                if (!reachedDestination && aiPath.velocity == Vector3.zero)
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
            OnSpeedUpdated();
        }
        
        public MovePosAStarData GetSaveData()
        {
            return new MovePosAStarData
            {
                AiPath = aiPath,
                MovePosition = _movePosition,
                OnReachedMovePosition = onReachedMovePosition,
                ReachedDestination = reachedDestination,
            };
        }

        public void SetLoadData(MovePosAStarData movePosAStarData)
        {
            aiPath = movePosAStarData.AiPath;
            onReachedMovePosition = movePosAStarData.OnReachedMovePosition;
            reachedDestination = movePosAStarData.ReachedDestination;

            if (!reachedDestination)
            {
                SetMovePosition(movePosAStarData.MovePosition, onReachedMovePosition);
            }
        }
        
        public struct MovePosAStarData
        {
            public Vector3 MovePosition;
            public AIPath AiPath;
            public Action OnReachedMovePosition;
            public bool ReachedDestination;
        }
    }
}
