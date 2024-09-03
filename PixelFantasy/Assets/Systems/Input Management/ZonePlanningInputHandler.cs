using System.Collections.Generic;
using CodeMonkey.Utils;
using Controllers;
using Managers;
using Systems.Build_Controls.Scripts;
using Systems.CursorHandler.Scripts;
using Systems.Zones.Scripts;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

namespace Systems.Input_Management
{
    public class ZonePlanningInputHandler : MonoBehaviour, IInputHandler
    {
        [SerializeField] private Sprite _placementIcon;
        [SerializeField] private TileBase _defaultZoneTiles;
        
        private bool _isPlanning;
        private Vector2 _startPos;
        private List<Vector2> _plannedGrid = new List<Vector2>();
        private List<TilePlan> _plannedTiles = new List<TilePlan>();
        private bool _isPlanningShrink;
        
        private List<string> _invalidPlacementTags => new List<string>() { "Water", "Zone", "Obstacle"};
        private List<string> _requiredPlacementTagsForShrink => new List<string>() { "Zone" };
        
        private void Start()
        {
            // Register this handler with the InputManager
            InputManager.Instance.RegisterInputHandler(InputMode.ZonePlanning, this);
        }
        
        public void HandleInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                StartPlanning();
            }
            else if (Input.GetMouseButton(0) && _isPlanning)
            {
                ContinuePlanning();
            }
            else if (Input.GetMouseButtonUp(0) && _isPlanning)
            {
                FinishPlanning();
            }

            if (_isPlanning && Input.GetMouseButtonDown(1))
            {
                CancelPlanning();
            }
            else if (Input.GetMouseButtonDown(1))
            {
                CancelPlanning();
                ZonesDatabase.Instance.CancelZonePlanning();
            }
        }

        public void PlanZone()
        {
            _isPlanningShrink = false;
            Spawner.Instance.ShowPlacementIcon(true, _placementIcon, _invalidPlacementTags);
        }

        public void PlanZoneExpansion()
        {
            _isPlanningShrink = false;
            Spawner.Instance.ShowPlacementIcon(true, _placementIcon, _invalidPlacementTags);
        }

        public void PlanZoneShrink()
        {
            _isPlanningShrink = true;
            Spawner.Instance.ShowPlacementIconForReqTags(true, _placementIcon, _requiredPlacementTagsForShrink);
        }
        
        private void StartPlanning()
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;

            _isPlanning = true;
            _startPos = Helper.SnapToGridPos(UtilsClass.GetMouseWorldPosition());
        }
        
        private void ContinuePlanning()
        {
            Vector3 currentMousePos = Helper.SnapToGridPos(UtilsClass.GetMouseWorldPosition());
            
            if (_isPlanningShrink)
            {
                ContinueZoneShrink(currentMousePos);
            }
            else
            {
                ContinuePlanZone(currentMousePos);
            }
        }
        
        private void ContinuePlanZone(Vector2 mousePos)
        {
            Spawner.Instance.ShowPlacementIcon(false);
            
            Vector3 curGridPos = Helper.SnapToGridPos(mousePos);
            List<Vector2> gridPositions = Helper.GetRectangleGridPositionsBetweenPoints(_startPos, curGridPos);
            
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
                    var tileMap = TilemapController.Instance.GetTilemap(TilemapLayer.PendingZones);
                    Color placementColour;
                    if (Helper.IsGridPosValidToBuild(gridPos, _invalidPlacementTags))
                    {
                        placementColour = Librarian.Instance.GetColour("Placement Green");
                    }
                    else
                    {
                        placementColour = Librarian.Instance.GetColour("Placement Red");
                    }
                
                    tilePlan.Init(_defaultZoneTiles, tileMap, placementColour);
            
                    _plannedTiles.Add(tilePlan);
                }
            }
        }
        
        private void ContinueZoneShrink(Vector2 mousePos)
        {
            Spawner.Instance.ShowPlacementIcon(false);
            
            Vector3 curGridPos = Helper.SnapToGridPos(mousePos);
            List<Vector2> gridPositions = Helper.GetRectangleGridPositionsBetweenPoints(_startPos, curGridPos);
            
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
                    var tileMap = TilemapController.Instance.GetTilemap(TilemapLayer.PendingZones);
                    Color placementColour;
                    if (Helper.IsGridPosValidToBuild(gridPos, new List<string>(), _requiredPlacementTagsForShrink))
                    {
                        placementColour = Librarian.Instance.GetColour("Placement Green");
                    }
                    else
                    {
                        placementColour = Librarian.Instance.GetColour("Placement Red");
                    }
                
                    tilePlan.Init(_defaultZoneTiles, tileMap, placementColour);
            
                    _plannedTiles.Add(tilePlan);
                }
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
        
        private void FinishPlanning()
        {
            if (_isPlanning)
            {
                Vector3 currentMousePos = Helper.SnapToGridPos(UtilsClass.GetMouseWorldPosition());
                CompletePlan(currentMousePos);
                
                _isPlanning = false;  // End planning process
            }
        }
        
        private void CancelPlanning()
        {
            ClearTilePlan();
            _isPlanning = false;  // Reset the planning state
            _isPlanningShrink = false;
            _plannedGrid.Clear();
            Spawner.Instance.ShowPlacementIcon(false);
        }
        
        public void CompletePlan(Vector2 mousePos)
        {
            if (_isPlanningShrink)
            {
                ZonesDatabase.Instance.ShrinkPlannedZone(_plannedGrid);
            }
            else
            {
                ZonesDatabase.Instance.SpawnPlannedZone(_plannedGrid);
            }
                
            ClearTilePlan();
            ZonesDatabase.Instance.CancelZonePlanning();
        }

        public void OnEnter()
        {
            CursorManager.Instance.ChangeCursorState(ECursorState.AreaSelect);
        }

        public void OnExit()
        {
            CursorManager.Instance.ChangeCursorState(ECursorState.Default);
        }
    }
}
