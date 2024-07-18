using System.Collections.Generic;
using Controllers;
using Managers;
using Systems.Build_Controls.Scripts;
using Systems.CursorHandler.Scripts;
using Systems.Floors.Scripts;
using UnityEngine;

namespace Systems.Details.Build_Details.Scripts
{
    public class FloorBuilder : MonoBehaviour
    {
        [SerializeField] private Floor _floorPrefab;
        [SerializeField] private Sprite _placementIcon;

        private FloorStyle _floorStyle;
        private FloorSettings _floorSettings;
        private bool _isEnabled;
        private bool _isPlanning;
        private Vector2 _startPos;
        private List<Vector2> _plannedGrid = new List<Vector2>();
        private List<TilePlan> _plannedTiles = new List<TilePlan>();
        private List<string> _invalidPlacementTags => new List<string>() { "Water", "Wall", "Floor", "Obstacle" };

        public void BeginFloorBuild(FloorSettings settings, StyleOption style)
        {
            _floorSettings = settings;
            _floorStyle = style as FloorStyle;
            
            CursorManager.Instance.ChangeCursorState(ECursorState.AreaSelect);
            Spawner.Instance.ShowPlacementIcon(true, _placementIcon, _invalidPlacementTags);
            _isEnabled = true;
        }

        public void UpdateStyle(StyleOption style)
        {
            _floorStyle = style as FloorStyle;
        }
        
        public void CancelFloorBuild()
        {
            _isEnabled = false;
            CursorManager.Instance.ChangeCursorState(ECursorState.Default);
            Spawner.Instance.ShowPlacementIcon(false);
            
            ClearTilePlan();
        }
        
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
            
            CancelFloorBuild();
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
            
            PlanFloor(mousePos);
        }

        protected void GameEvents_OnLeftClickUp(Vector3 mousePos, PlayerInputState inputState, bool isOverUI)
        {
            if (!_isEnabled) return;

            if (_isPlanning)
            {
                SpawnPlannedFloor();
            }
            else
            {
                if (!isOverUI)
                {
                    SpawnFloor(Helper.ConvertMousePosToGridPos(mousePos));
                }
            }
        }
        
        private void PlanFloor(Vector2 mousePos)
        {
            Spawner.Instance.ShowPlacementIcon(false);
            
            Vector3 curGridPos = Helper.ConvertMousePosToGridPos(mousePos);
            List<Vector2> gridPositions = new List<Vector2>();
            
            gridPositions = Helper.GetRectangleGridPositionsBetweenPoints(_startPos, curGridPos);
            
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
                    var tileMap = TilemapController.Instance.GetTilemap(TilemapLayer.Flooring);
                    Color placementColour;
                    if (Helper.IsGridPosValidToBuild(gridPos, _invalidPlacementTags))
                    {
                        placementColour = Librarian.Instance.GetColour("Placement Green");
                    }
                    else
                    {
                        placementColour = Librarian.Instance.GetColour("Placement Red");
                    }
                
                    tilePlan.Init(_floorStyle.Tiles, tileMap, placementColour);
            
                    _plannedTiles.Add(tilePlan);
                }
            }
        }
        
        private void SpawnPlannedFloor()
        {
            if (!_isPlanning) return;
            
            Spawner.Instance.ShowPlacementIcon(true, _placementIcon, _invalidPlacementTags);
            ClearTilePlan();
            
            foreach (var gridPos in _plannedGrid)
            {
                if (Helper.IsGridPosValidToBuild(gridPos, _invalidPlacementTags))
                {
                    SpawnFloor(gridPos);
                }
            }
            
            _plannedGrid.Clear();
            _isPlanning = false;
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
