using System;
using System.Collections.Generic;
using Characters.Interfaces;
using Managers;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Characters
{
    public class KinlingAgent : MonoBehaviour, IMovePosition
    {
        [SerializeField] private GameObject _smallDotPrefab;
        [SerializeField] private GameObject _goalPrefab;
        [SerializeField] private float _dotSpacing;
        
        private NavMeshAgent _agent;
        private Action _onReachedMovePosition;
        private bool _inTransit;
        private KinlingAnimController _charAnimController;
        private float _defaultSpeed;
        private float _defaultAcceleration;
        private float _defaultAngularSpeed;
        private Kinling _kinling;
        private NavMeshPath _currentPath;
        private bool _isPathVisible;

        private const float NEAREST_POINT_SEARCH_RANGE = 5f;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _agent.updateRotation = false;
            _agent.updateUpAxis = false;

            _defaultSpeed = _agent.speed;
            _defaultAcceleration = _agent.acceleration;
            _defaultAngularSpeed = _agent.angularSpeed;
            _kinling = GetComponent<Kinling>();
            
            _charAnimController = GetComponent<Kinling>().kinlingAnimController;
            
            OnSpeedUpdated();
        }

        public void SetPriority(int priority)
        {
            _agent.avoidancePriority = priority;
        }
        
        public void TeleportToPosition(Vector2 teleportPos, bool disableNav)
        {
            _agent.enabled = !disableNav;
            gameObject.transform.position = teleportPos;
        }

        public bool SetMovePosition(Vector2? movePosition, Action onReachedMovePosition = null, Action onImpossiblePosition = null)
        {
            if (movePosition == null)
            {
                onImpossiblePosition?.Invoke();
                return false; // Early return if no movePosition provided
            }

            // Ensure the NavMeshAgent is enabled
            if (!_agent.enabled)
            {
                _agent.enabled = true;
            }

            // Wait until the agent is on the NavMesh
            if (!_agent.isOnNavMesh)
            {
                // Attempt to place the agent on the NavMesh
                if (!NavMesh.SamplePosition(transform.position, out NavMeshHit hit, NEAREST_POINT_SEARCH_RANGE, NavMesh.AllAreas))
                {
                    onImpossiblePosition?.Invoke();
                    return false; // Failed to find a valid position on the NavMesh
                }

                _agent.Warp(hit.position); // Use Warp to accurately place the agent on the NavMesh
            }

            Vector3 targetPosition = new Vector3(movePosition.Value.x, movePosition.Value.y, 0 );

            // Now that we're sure the agent is on the NavMesh and enabled, set the destination
            _agent.SetDestination(targetPosition);
            _inTransit = true;
            _onReachedMovePosition = onReachedMovePosition;

            // Retrieve and store the NavMesh path
            _currentPath = new NavMeshPath();
            if (_agent.CalculatePath(targetPosition, _currentPath))
            {
                if (_isPathVisible)
                {
                    DisplayPath();
                }
                return true; // Successful in setting a destination and calculating the path
            }
            else
            {
                onImpossiblePosition?.Invoke();
                return false; // Failed to calculate a path to the destination
            }
        }

        public bool IsDestinationPossible(Vector2 position)
        {
            var result = Helper.DoesPathExist(transform.position, position);
            return result.pathExists;
        }

        private void DetermineIfDestination()
        {
            // Ensure the agent is enabled and on the NavMesh before checking its state
            if (_agent.enabled && _agent.isOnNavMesh)
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
                    
                            // Clear the path visualization if the Kinling reached the destination
                            ClearPathVisualization();
                            _currentPath = null;
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

            if (speedMod == 0)
            {
                _agent.acceleration = 99;
            }
            else
            {
                _agent.acceleration = _defaultAcceleration * speedMod; // Adjust acceleration proportionally
                _agent.angularSpeed = _defaultAngularSpeed * speedMod; // Adjust angularSpeed proportionally
            }
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
        
        private List<GameObject> pathVisuals = new List<GameObject>();  // To keep track of instantiated path objects
        private void DisplayPath()
        {
            // Clear previous path visualization
            foreach (var visual in pathVisuals)
            {
                Destroy(visual);
            }
            pathVisuals.Clear();

            if (_currentPath.corners.Length < 2) return; // No path to display

            Vector3 previousCorner = _currentPath.corners[0];

            // Iterate through the path's corners
            for (int i = 1; i < _currentPath.corners.Length; i++)
            {
                Vector3 currentCorner = _currentPath.corners[i];
        
                // Draw small dots between the previous corner and the current corner
                DrawDottedLine(previousCorner, currentCorner);

                previousCorner = currentCorner; // Update the previous corner
            }

            // Place the movement goal sprite at the end of the path
            Vector3 goalPosition = _currentPath.corners[_currentPath.corners.Length - 1];
            GameObject goalVisual = Instantiate(_goalPrefab, goalPosition, Quaternion.identity);
            goalVisual.transform.SetParent(Spawner.Instance.MiscParent);
            pathVisuals.Add(goalVisual);
        }
        
        private void DrawDottedLine(Vector3 start, Vector3 end)
        {
            float distance = Vector3.Distance(start, end);
            int dotCount = Mathf.FloorToInt(distance / _dotSpacing);  // _dotSpacing is a serialized field or constant
            Vector3 direction = (end - start).normalized;

            Vector3 lastDotPosition = start;

            for (int i = 1; i <= dotCount; i++)
            {
                Vector3 dotPosition = start + direction * _dotSpacing * i;
                GameObject dotVisual = Instantiate(_smallDotPrefab, dotPosition, Quaternion.identity);
                dotVisual.transform.SetParent(Spawner.Instance.MiscParent);
                pathVisuals.Add(dotVisual);
                lastDotPosition = dotPosition;
            }

            // Ensure a dot is placed close to the end, but avoid overlap
            float distanceToLastDot = Vector3.Distance(end, lastDotPosition);
            float distanceToNextDot = Vector3.Distance(end, lastDotPosition + direction * _dotSpacing);

            // Place the last dot only if it doesn't overlap with the previous dot or the corner
            if (distanceToLastDot > _dotSpacing * 0.4f && distanceToNextDot > _dotSpacing * 0.4f)
            {
                GameObject dotVisual = Instantiate(_smallDotPrefab, end - direction * _dotSpacing, Quaternion.identity);
                dotVisual.transform.SetParent(Spawner.Instance.MiscParent);
                pathVisuals.Add(dotVisual);
            }
        }
        
        private void ClearPathVisualization()
        {
            foreach (var visual in pathVisuals)
            {
                Destroy(visual);
            }
            pathVisuals.Clear();
        }
        
        public void SetPathVisibility(bool showPath)
        {
            _isPathVisible = showPath;

            if (_isPathVisible && _currentPath != null)
            {
                // Path is toggled on and there is a path to display
                DisplayPath();
            }
            else
            {
                // Path is toggled off or there is no path
                ClearPathVisualization();
            }
        }
    }
}
