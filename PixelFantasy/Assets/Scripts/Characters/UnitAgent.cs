using System;
using Characters.Interfaces;
using Gods;
using UnityEngine;
using UnityEngine.AI;

namespace Characters
{
    public class UnitAgent : MonoBehaviour, IMovePosition
    {
        private NavMeshAgent _agent;
        private Action _onReachedMovePosition;
        private bool _inTransit;
        private ICharacterAnimController _charAnimController;
        private float _defaultSpeed;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _agent.updateRotation = false;
            _agent.updateUpAxis = false;
            _defaultSpeed = _agent.speed;
            
            _charAnimController = GetComponent<ICharacterAnimController>();
            
            OnSpeedUpdated();
        }

        public void SetMovePosition(Vector3 movePosition, Action onReachedMovePosition = null)
        {
            _inTransit = true;
            _onReachedMovePosition = onReachedMovePosition;
            _agent.SetDestination(movePosition);
            _charAnimController.SetMovementVelocity(movePosition);
        }

        public bool IsDestinationPossible(Vector3 position)
        {
            NavMeshPath path = new NavMeshPath();
            _agent.CalculatePath(position, path);
            return path.status == NavMeshPathStatus.PathComplete;
        }

        private void DetermineIfDestination()
        {
            if (!_agent.pathPending && _inTransit)
            {
                if (_agent.remainingDistance <= _agent.stoppingDistance)
                {
                    if (!_agent.hasPath || _agent.velocity.sqrMagnitude == 0f)
                    {
                        _inTransit = false;
                        if (_onReachedMovePosition != null)
                        {
                            _onReachedMovePosition.Invoke();   
                        }
                    }
                }
            }
        }
        
        private void RefreshAnimVector()
        {
            var moveVelo = _agent.velocity;
            _charAnimController.SetMovementVelocity(moveVelo);
        }
        
        private void OnSpeedUpdated()
        {
            var speedMod = TimeManager.Instance.GameSpeedMod;
            _agent.speed = _defaultSpeed * speedMod;
        }

        private void Update()
        {
           DetermineIfDestination();
           RefreshAnimVector();
           OnSpeedUpdated();
        }
    }
}
