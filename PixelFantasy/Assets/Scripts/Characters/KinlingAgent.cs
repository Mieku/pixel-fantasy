using System;
using System.Collections.Generic;
using Characters.Interfaces;
using Managers;
using Sirenix.OdinInspector;
using Systems.Stats.Scripts;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using Avatar = Systems.Appearance.Scripts.Avatar;
using Random = UnityEngine.Random;

namespace Characters
{
    public class KinlingAgent : MonoBehaviour, IMovePosition
    {
        [SerializeField] private GameObject _smallDotPrefab;
        [SerializeField] private GameObject _largeDotPrefab;
        [SerializeField] private GameObject _goalPrefab;
        [SerializeField] private float _dotSpacing;
        
        private List<GameObject> pathVisuals = new List<GameObject>();  // To keep track of instantiated path objects
        
        private NavMeshAgent _agent;
        private Action _onReachedMovePosition;
        private bool _inTransit;
        private float _defaultSpeed;
        private float _defaultAcceleration;
        private float _defaultAngularSpeed;
        private Kinling _kinling;
        private NavMeshPath _currentPath;
        private bool _isPathVisible;
        private bool _isInitialized;
        private Avatar _avatar;

        private const float NEAREST_POINT_SEARCH_RANGE = 5f;
        private const float PIXELS_PER_UNIT = 16;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _agent.updateRotation = false;
            _agent.updateUpAxis = false;

            _kinling = GetComponent<Kinling>();
            _avatar = _kinling.Avatar;
        }

        private void Start()
        {
            _defaultSpeed = _agent.speed + _kinling.Stats.GetAttributeModifierBonus(EAttributeType.WalkSpeed, _agent.speed);
            _defaultAcceleration = _agent.acceleration;
            _defaultAngularSpeed = _agent.angularSpeed;
            _isInitialized = true;
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

        [ShowInInspector] private Vector2? _targetMovePosition;
        public bool SetMovePosition(Vector2? movePosition, Action onReachedMovePosition = null, Action onImpossiblePosition = null)
        {
            _targetMovePosition = movePosition;
            if (movePosition == null)
            {
                onImpossiblePosition?.Invoke();
                _targetMovePosition = null;
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
                    _targetMovePosition = null;
                    return false; // Failed to find a valid position on the NavMesh
                }

                _agent.Warp(hit.position); // Use Warp to accurately place the agent on the NavMesh
            }

            Vector3 targetPosition = new Vector3(movePosition.Value.x, movePosition.Value.y, 0 );
            targetPosition = RoundToPixel(targetPosition, PIXELS_PER_UNIT);

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
                _targetMovePosition = null;
                return true; // Successful in setting a destination and calculating the path
            }
            else
            {
                onImpossiblePosition?.Invoke();
                _targetMovePosition = null;
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
            _avatar.SetMovementVelocity(moveVelo);
        }
        
        private void OnSpeedUpdated()
        {
            if (!_isInitialized) return;
            
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
            if(!_kinling.HasInitialized) return;
            if(!_isInitialized) return;

            DetermineIfDestination();
            RefreshAnimVector();
            OnSpeedUpdated();

            transform.position = RoundToPixel(transform.position, PIXELS_PER_UNIT);
            _kinling.RuntimeData.Position = transform.position;
        }
        
        private Vector3 RoundToPixel(Vector3 position, float pixelsPerUnit)
        {
            position.x = Mathf.Round(position.x * pixelsPerUnit) / pixelsPerUnit;
            position.y = Mathf.Round(position.y * pixelsPerUnit) / pixelsPerUnit;
            return position;
        }
        
        private void DisplayPath()
        {
            // Clear previous path visualization
            foreach (var visual in pathVisuals)
            {
                Destroy(visual);
            }
            pathVisuals.Clear();

            if (_currentPath.corners.Length < 2) return; // No path to display

            Vector3 previousCorner = RoundToPixel(_currentPath.corners[0], PIXELS_PER_UNIT);

            // Optionally, instantiate a large dot at the start corner
            GameObject startCornerVisual = Instantiate(_largeDotPrefab, previousCorner, Quaternion.identity);
            startCornerVisual.transform.SetParent(ParentsManager.Instance.MiscParent);
            pathVisuals.Add(startCornerVisual);

            int lastCornerIndex = _currentPath.corners.Length - 1;

            // Iterate through the path's corners, excluding the last corner
            for (int i = 1; i < lastCornerIndex; i++)
            {
                Vector3 currentCorner = RoundToPixel(_currentPath.corners[i], PIXELS_PER_UNIT);

                // Draw small dots between the previous corner and the current corner
                DrawDottedLine(previousCorner, currentCorner);

                // Instantiate a large dot at the current corner (pivot point)
                GameObject cornerVisual = Instantiate(_largeDotPrefab, currentCorner, Quaternion.identity);
                cornerVisual.transform.SetParent(ParentsManager.Instance.MiscParent);
                pathVisuals.Add(cornerVisual);

                previousCorner = currentCorner; // Update the previous corner
            }

            // Draw small dots from the second-to-last corner to the last corner
            Vector3 lastCorner = RoundToPixel(_currentPath.corners[lastCornerIndex], PIXELS_PER_UNIT);
            DrawDottedLine(previousCorner, lastCorner);

            // Place the movement goal sprite at the end of the path
            GameObject goalVisual = Instantiate(_goalPrefab, lastCorner, Quaternion.identity);
            goalVisual.transform.SetParent(ParentsManager.Instance.MiscParent);
            pathVisuals.Add(goalVisual);
        }

        private void DrawDottedLine(Vector3 start, Vector3 end)
        {
            float distance = Vector3.Distance(start, end);
            int dotCount = Mathf.Max(1, Mathf.CeilToInt(distance / _dotSpacing));

            // Adjust spacing to fit exactly into the distance
            float adjustedSpacing = distance / dotCount;

            Vector3 direction = (end - start).normalized;

            for (int i = 1; i < dotCount; i++)
            {
                Vector3 dotPosition = RoundToPixel(start + direction * adjustedSpacing * i, PIXELS_PER_UNIT);
                GameObject dotVisual = Instantiate(_smallDotPrefab, dotPosition, Quaternion.identity);
                dotVisual.transform.SetParent(ParentsManager.Instance.MiscParent);
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
        
        public Vector2 FindClosestPosInPath(Vector2 endPos)
        {
            var result = Helper.FindClosestPointInPathToDestination(transform.position, endPos, _agent);
            return result;
        }
    }
}