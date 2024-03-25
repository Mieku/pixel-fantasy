using System.Collections.Generic;
using Controllers;
using Data.Dye;
using Managers;
using Systems.Build_Controls.Scripts;
using Systems.Buildings.Scripts;
using Systems.CursorHandler.Scripts;
using UnityEngine;
using WallSettings = Data.Structure.WallSettings;

namespace Systems.Details.Build_Details.Scripts
{
    public class WallBuilder : MonoBehaviour
    {
        [SerializeField] private Wall _wallPrefab;
        [SerializeField] private Sprite _placementIcon;
        
        private WallSettings _wallSettings;
        private DyeData _colour;
        private bool _isEnabled;
        private bool _isPlanning;
        private Vector2 _startPos;
        private List<Vector2> _plannedGrid = new List<Vector2>();
        private List<TilePlan> _plannedTiles = new List<TilePlan>();
        private List<string> _invalidPlacementTags => new List<string>() { "Water", "Wall", "Structure", "Obstacle" };
        
        private void Awake()
        {
            GameEvents.OnLeftClickDown += GameEvents_OnLeftClickDown;
            GameEvents.OnLeftClickHeld += GameEvents_OnLeftClickHeld;
            GameEvents.OnLeftClickUp += GameEvents_OnLeftClickUp;
            GameEvents.OnRightClickDown += GameEvents_OnRightClickDown;
        }

        private void OnDestroy()
        {
            GameEvents.OnLeftClickDown -= GameEvents_OnLeftClickDown;
            GameEvents.OnLeftClickHeld -= GameEvents_OnLeftClickHeld;
            GameEvents.OnLeftClickUp -= GameEvents_OnLeftClickUp;
            GameEvents.OnRightClickDown -= GameEvents_OnRightClickDown;
        }

        protected void GameEvents_OnRightClickDown(Vector3 mousePos, PlayerInputState inputState, bool isOverUI)
        {
            if(!_isEnabled) return;
            
            CancelWallBuild();
        }
    
        protected void GameEvents_OnLeftClickDown(Vector3 mousePos, PlayerInputState inputState, bool isOverUI)
        {
            if (!_isEnabled) return;
            if (isOverUI) return;

            _isPlanning = true;
            _startPos = Helper.ConvertMousePosToGridPos(mousePos);
        }

        protected void GameEvents_OnLeftClickHeld(Vector3 mousePos, PlayerInputState inputState, bool isOverUI)
        {
            if (!_isEnabled) return;
            if (!_isPlanning) return;
            
            PlanWall(mousePos);
        }

        protected void GameEvents_OnLeftClickUp(Vector3 mousePos, PlayerInputState inputState, bool isOverUI)
        {
            if (!_isEnabled) return;

            if (_isPlanning)
            {
                SpawnPlannedWall();
            }
            else
            {
                if (!isOverUI)
                {
                    SpawnWall(Helper.ConvertMousePosToGridPos(mousePos));
                }
            }
        }

        public void BeginWallBuild(WallSettings wallSettings, DyeData colour)
        {
            _wallSettings = wallSettings;
            _colour = colour;
            CursorManager.Instance.ChangeCursorState(ECursorState.AreaSelect);
            Spawner.Instance.ShowPlacementIcon(true, _placementIcon, _invalidPlacementTags);
            _isEnabled = true;
        }

        public void CancelWallBuild()
        {
            _isEnabled = false;
            CursorManager.Instance.ChangeCursorState(ECursorState.Default);
            Spawner.Instance.ShowPlacementIcon(false);
            
            ClearTilePlan();
        }

        private void PlanWall(Vector2 mousePos)
        {
            Spawner.Instance.ShowPlacementIcon(false);
            
            Vector3 curGridPos = Helper.ConvertMousePosToGridPos(mousePos);
            List<Vector2> gridPositions = new List<Vector2>();
            
            gridPositions = Helper.GetBoxPositionsBetweenPoints(_startPos, curGridPos);
            
            if (gridPositions.Count != _plannedGrid.Count)
            {
                _plannedGrid = gridPositions;
            
                // Clear previous display, then display new blueprints
                ClearTilePlan();
            
                foreach (var gridPos in gridPositions)
                {
                    var tilePlanGO = new GameObject("Tile Plan", typeof(TilePlan));
                    tilePlanGO.transform.position = gridPos;
            
                    var tilePlan = tilePlanGO.GetComponent<TilePlan>();
                    var tileMap = TilemapController.Instance.GetTilemap(TilemapLayer.Structure);
                    Color placementColour;
                    if (Helper.IsGridPosValidToBuild(gridPos, _invalidPlacementTags))
                    {
                        placementColour = Librarian.Instance.GetColour("Placement Green");
                    }
                    else
                    {
                        placementColour = Librarian.Instance.GetColour("Placement Red");
                    }
                
                    tilePlan.Init(_wallSettings.ExteriorRuleTile, tileMap, placementColour);
            
                    _plannedTiles.Add(tilePlan);
                }
            }
        }
    
        private void SpawnPlannedWall()
        {
            if (!_isPlanning) return;
            
            Spawner.Instance.ShowPlacementIcon(true, _placementIcon, _invalidPlacementTags);
            ClearTilePlan();
            
            foreach (var gridPos in _plannedGrid)
            {
                if (Helper.IsGridPosValidToBuild(gridPos, _invalidPlacementTags))
                {
                    SpawnWall(gridPos);
                }
            }
            
            _plannedGrid.Clear();
            _isPlanning = false;
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
    
        private void ClearTilePlan()
        {
            foreach (var tilePlan in _plannedTiles)
            {
                tilePlan.Clear();
                Destroy(tilePlan.gameObject);
            }
            _plannedTiles.Clear();
        }
    }
}
