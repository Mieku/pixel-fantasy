using System;
using Characters.Interfaces;
using Managers;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Characters
{
    public class UnitAgent : MonoBehaviour, IMovePosition
    {
        private NavMeshAgent _agent;
        private Action _onReachedMovePosition;
        private bool _inTransit;
        private UnitAnimController _charAnimController;
        private float _defaultSpeed;

        private const float NEAREST_POINT_SEARCH_RANGE = 5f;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _agent.updateRotation = false;
            _agent.updateUpAxis = false;
            _defaultSpeed = _agent.speed;
            
            _charAnimController = GetComponent<Unit>().UnitAnimController;
            
            OnSpeedUpdated();
        }

        public void TeleportToPosition(Vector2 teleportPos, bool disableNav)
        {
            _agent.enabled = !disableNav;
            gameObject.transform.position = teleportPos;
        }

        public bool SetMovePosition(Vector2 movePosition, Action onReachedMovePosition = null)
        {
            if (_agent.SetDestination(movePosition))
            {
                _inTransit = true;
                _onReachedMovePosition = onReachedMovePosition;
                _charAnimController.SetMovementVelocity(movePosition);
                _charAnimController.Appearance.SetDirection(UnitActionDirection.Side);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsDestinationPossible(Vector2 position)
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
        
        public Vector3 PickLocationInRange(float range)
        {
            Vector3 searchLocation = transform.position;
            searchLocation += Random.Range(-range, range) * Vector3.up;
            searchLocation += Random.Range(-range, range) * Vector3.right;

            if (NavMesh.SamplePosition(searchLocation, out var hitResult, NEAREST_POINT_SEARCH_RANGE, NavMesh.AllAreas))
            {
                return hitResult.position;
            }

            return transform.position;
        }

        private void Update()
        {
           DetermineIfDestination();
           RefreshAnimVector();
           OnSpeedUpdated();
        }
    }
}
