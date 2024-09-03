using System;
using System.Collections.Generic;
using Managers;
using Systems.Floors.Scripts;
using Systems.Input_Management;
using UnityEngine;

namespace Systems.Details.Build_Details.Scripts
{
    public class FloorBuilder : MonoBehaviour
    {
        [SerializeField] private Floor _floorPrefab;
        [SerializeField] private Sprite _placementIcon;

        private FloorStyle _floorStyle;
        private FloorSettings _floorSettings;
        private Action _onCompleteCallback;
        private bool _isPlanning;
        private List<string> _invalidPlacementTags => new List<string>() { "Water", "Wall", "Floor", "Obstacle" };
        
        public FloorSettings FloorSettings => _floorSettings;
        public FloorStyle FloorStyle => _floorStyle;
        public List<string> InvalidPlacementTags => _invalidPlacementTags;

        public void BeginFloorBuild(FloorSettings settings, StyleOption style, Action onComplete)
        {
            _floorSettings = settings;
            _floorStyle = style as FloorStyle;
            _onCompleteCallback = onComplete;
            _isPlanning = true;
            
            InputManager.Instance.SetInputMode(InputMode.FloorPlanning);
            
            Spawner.Instance.ShowPlacementIcon(true, _placementIcon, _invalidPlacementTags);
        }

        public void UpdateStyle(StyleOption style)
        {
            _floorStyle = style as FloorStyle;
        }
        
        public void CancelFloorBuild()
        {
            if (_isPlanning)
            {
                _isPlanning = false;
                Spawner.Instance.ShowPlacementIcon(false);
                InputManager.Instance.ReturnToDefault();
                _onCompleteCallback?.Invoke();
            }
        }
        
        public void SpawnFloor(Vector3 spawnPosition)
        {
            if (Helper.IsGridPosValidToBuild(spawnPosition, _invalidPlacementTags))
            {
                spawnPosition = new Vector3(spawnPosition.x, spawnPosition.y, -1);
                var floor = Instantiate(_floorPrefab, spawnPosition, Quaternion.identity);
                floor.transform.SetParent(ParentsManager.Instance.FlooringParent);
                floor.Init(_floorSettings, _floorStyle);
            }
        }

        public Floor SpawnLoadedFloor(FloorData data)
        {
            var floor = Instantiate(_floorPrefab, data.Position, Quaternion.identity);
            floor.transform.SetParent(ParentsManager.Instance.FlooringParent);
            floor.LoadData(data);
            return floor;
        }
    }
}
