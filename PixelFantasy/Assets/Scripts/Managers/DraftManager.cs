using System.Collections.Generic;
using Characters;
using UnityEditor;
using UnityEngine;

namespace Managers
{
    public class DraftManager : Singleton<DraftManager>
    {
        [SerializeField] private KinlingPositionPreview _positionPreviewPrefab;
    
        private List<KinlingPositionPreview> _positionPreviews = new List<KinlingPositionPreview>();
        private List<Kinling> _selectedDraftedKinlings = new List<Kinling>();
        private Vector2 _previewStartPosition;
        private List<string> _invalidTags = new List<string>() { "Obstacle", "Water" };
        private Dictionary<Kinling, Vector2> _orderedPositions = new Dictionary<Kinling, Vector2>();

        public void BeginOrdersPreview(List<Kinling> draftedKinlings, Vector2 startPos)
        {
            ClearPreviews();
            _selectedDraftedKinlings = draftedKinlings;
            _previewStartPosition = startPos;
            foreach (var kinling in _selectedDraftedKinlings)
            {
                CreatePositionPreview(kinling);
            }
        }

        // Happens on right click drag
        public void ContinueOrdersPreview(Vector2 currentPos)
        {
            _orderedPositions.Clear();
            
            var positions = GetPositionsWithin(_previewStartPosition, currentPos, _selectedDraftedKinlings.Count);
            for (int i = 0; i < _selectedDraftedKinlings.Count; i++)
            {
                var kinling = _selectedDraftedKinlings[i];
                var pathPos = kinling.KinlingAgent.FindClosestPosInPath(positions[i]);
                var snappedPathPos = Helper.SnapToGridPos(pathPos);
                _positionPreviews[i].transform.position = snappedPathPos;
                _orderedPositions.Add(kinling, snappedPathPos);
            }
        }

        // Happens on right click released
        public void CompleteOrdersPreview()
        {
            foreach (var kvp in _orderedPositions)
            {
                var kinling = kvp.Key;
                var pos = kvp.Value;
                kinling.DraftedHandler.AssignMoveOrder(pos);
            }
            
            ClearPreviews();
            _selectedDraftedKinlings.Clear();
        }

        private void ClearPreviews()
        {
            foreach (var preview in _positionPreviews)
            {
                Destroy(preview.gameObject);
            }
            _positionPreviews.Clear();
        }

        private void CreatePositionPreview(Kinling kinling)
        {
            var preview = Instantiate(_positionPreviewPrefab, transform);
            preview.gameObject.SetActive(true);
            preview.Init(kinling.RuntimeData);
            _positionPreviews.Add(preview);
        }
        
        private List<Vector2> GetPositionsWithin(Vector2 startPos, Vector2 endPos, int numPositions)
        {
            List<Vector2> results = new List<Vector2>();

            // Generate positions based on the number of positions required
            if (numPositions == 1)
            {
                results.Add(endPos);
            }
            else
            {
                // Add the start position as the first item
                results.Add(startPos);

                if (numPositions == 2)
                {
                    results.Add(endPos);
                }
                else if (numPositions > 2)
                {
                    // Calculate the step size based on the number of positions to generate
                    Vector2 step = (endPos - startPos) / (numPositions - 1);

                    // Generate intermediate positions
                    for (int i = 1; i < numPositions - 1; i++)
                    {
                        Vector2 position = startPos + step * i;
                        results.Add(position);
                    }

                    // Add the end position as the last item
                    results.Add(endPos);
                }
            }

            List<Vector2> gridPositions = new List<Vector2>();

            // For each of the positions, snap it to a grid position and validate
            foreach (var roughPos in results)
            {
                var snappedPosition = Helper.SnapToGridPos(roughPos);

                if (!Helper.DoesGridContainTags(snappedPosition, _invalidTags) && !gridPositions.Contains(snappedPosition))
                {
                    gridPositions.Add(snappedPosition);
                }
                else
                {
                    // Check adjacent positions until one doesn't have the invalid tags or isn't already taken
                    var validPosition = FindNearestValidPosition(snappedPosition, gridPositions);

                    if (validPosition != null)
                    {
                        gridPositions.Add(validPosition.Value);
                    }
                    else
                    {
                        // If no valid position is found, handle accordingly (e.g., skip or log a warning)
                        Debug.LogWarning($"No valid position found near {snappedPosition}. Skipping this position.");
                    }
                }
            }

            return gridPositions;
        }
        
        private Vector2? FindNearestValidPosition(Vector2 start, List<Vector2> occupiedPositions)
        {
            Queue<Vector2> queue = new Queue<Vector2>();
            HashSet<Vector2> visited = new HashSet<Vector2>();
            queue.Enqueue(start);
            visited.Add(start);

            while (queue.Count > 0)
            {
                Vector2 current = queue.Dequeue();

                List<Vector2> neighbors = GetAdjacentPositions(current);

                foreach (var neighbor in neighbors)
                {
                    var snappedNeighbor = Helper.SnapToGridPos(neighbor);

                    if (visited.Contains(snappedNeighbor))
                        continue;

                    visited.Add(snappedNeighbor);

                    if (!Helper.DoesGridContainTags(snappedNeighbor, _invalidTags) && !occupiedPositions.Contains(snappedNeighbor))
                    {
                        return snappedNeighbor;
                    }
                    else
                    {
                        queue.Enqueue(snappedNeighbor);
                    }
                }
            }

            // No valid position found
            return null;
        }

        // Helper method to get adjacent positions
        private List<Vector2> GetAdjacentPositions(Vector2 position)
        {
            List<Vector2> positions = new List<Vector2>();

            // Adjust these offsets based on your grid's coordinate system
            positions.Add(position + new Vector2(1, 0));  // Right
            positions.Add(position + new Vector2(-1, 0)); // Left
            positions.Add(position + new Vector2(0, 1));  // Up
            positions.Add(position + new Vector2(0, -1)); // Down

            positions.Add(position + new Vector2(1, 1));   // Up-Right
            positions.Add(position + new Vector2(-1, 1));  // Up-Left
            positions.Add(position + new Vector2(1, -1));  // Down-Right
            positions.Add(position + new Vector2(-1, -1)); // Down-Left

            return positions;
        }

    }
}