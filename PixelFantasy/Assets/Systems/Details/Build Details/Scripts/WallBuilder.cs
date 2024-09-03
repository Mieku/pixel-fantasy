using System;
using System.Collections.Generic;
using Controllers;
using Managers;
using ScriptableObjects;
using Systems.Build_Controls.Scripts;
using Systems.Buildings.Scripts;
using Systems.CursorHandler.Scripts;
using Systems.Input_Management;
using UnityEngine;

namespace Systems.Details.Build_Details.Scripts
{
    public class WallBuilder : MonoBehaviour
    {
        [SerializeField] private Wall _wallPrefab;
        [SerializeField] private Sprite _placementIcon;

        private WallSettings _wallSettings;
        private DyeSettings _colour;
        private Action _onCompleteCallback;

        private List<string> _invalidPlacementTags = new List<string>() { "Water", "Wall", "Structure", "Obstacle", "Clearance" };

        public List<string> InvalidPlacementTags => _invalidPlacementTags;
        public WallSettings WallSettings => _wallSettings;
        

        public void BeginWallBuild(WallSettings wallSettings, DyeSettings colour, Action onComplete)
        {
            _wallSettings = wallSettings;
            _colour = colour;
            _onCompleteCallback = onComplete;

            // Set up the cursor and placement icon
            InputManager.Instance.SetInputMode(InputMode.WallPlanning);
            Spawner.Instance.ShowPlacementIcon(true, _placementIcon, _invalidPlacementTags);
        }

        public void CancelWallBuild()
        {
            // Reset cursor and placement icon when build is canceled
            Spawner.Instance.ShowPlacementIcon(false);
            InputManager.Instance.ReturnToDefault();
            _onCompleteCallback?.Invoke();
        }
        
        public void SpawnWall(Vector3 spawnPosition)
        {
            if (Helper.IsGridPosValidToBuild(spawnPosition, _invalidPlacementTags))
            {
                spawnPosition = new Vector3(spawnPosition.x, spawnPosition.y, -1);
                var wall = Instantiate(_wallPrefab, spawnPosition, Quaternion.identity);
                wall.transform.SetParent(ParentsManager.Instance.StructuresParent);
                wall.Init(_wallSettings, _colour);
            }
        }

        public void SpawnLoadedWall(WallData wallData)
        {
            var wall = Instantiate(_wallPrefab, wallData.Position, Quaternion.identity);
            wall.transform.SetParent(ParentsManager.Instance.StructuresParent);
            wall.LoadData(wallData);
        }
    }
}
