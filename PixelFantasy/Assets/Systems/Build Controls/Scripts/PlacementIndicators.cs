using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Systems.Build_Controls.Scripts
{
    public class PlacementIndicators : MonoBehaviour
    {
        [SerializeField] private Transform _placementPivot;
        [SerializeField] private Transform _obstaclesParent;
        [SerializeField] private Transform _usePosParent;
        
        private List<PlacementObstacle> _obstacles;
        private List<UsePosition> _usePositions;

        private void Awake()
        {
            _placementPivot.gameObject.GetComponent<SpriteRenderer>().enabled = false;
            _obstacles = _obstaclesParent.GetComponentsInChildren<PlacementObstacle>().ToList();
            _usePositions = _usePosParent.GetComponentsInChildren<UsePosition>().ToList();
        }
        
        public Vector2 PlacementPivot => _placementPivot.position;

        public List<Vector2> UsePositions(Transform parent)
        {
            List<Vector2> usePositions = new List<Vector2>();
            foreach (var usePosition in _usePositions)
            {
                if (usePosition.IsValid(parent))
                {
                    usePositions.Add(usePosition.GetPosition());
                }
            }
            return usePositions;
        }

        public void ShowUsePositions(bool show)
        {
            foreach (var usePosition in _usePositions)
            {
                usePosition.ShowMarker(show);
            }
        }

        public void EnableObstacles(bool enable)
        {
            foreach (var obstacle in _obstacles)
            {
                obstacle.gameObject.SetActive(enable);
            }

            foreach (var usePosition in _usePositions)
            {
                usePosition.EnableClearance(enable);
            }
        }

        public bool CheckPlacement(int minUsePositions, Transform parent)
        {
            bool canPlace = true;
            foreach (var obstacle in _obstacles)
            {
                if (obstacle.CheckPlacement(parent) == false)
                {
                    canPlace = false;
                }
            }
            
            int passedUsePositions = 0;
            foreach (var usePos in _usePositions)
            {
                if (usePos.CheckPlacement(parent))
                {
                    passedUsePositions++;
                }
            }
            
            return canPlace && passedUsePositions >= minUsePositions;
        }
    }
}
