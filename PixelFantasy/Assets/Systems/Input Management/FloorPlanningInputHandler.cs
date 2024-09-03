using System.Collections.Generic;
using CodeMonkey.Utils;
using Controllers;
using Managers;
using Systems.Build_Controls.Scripts;
using Systems.CursorHandler.Scripts;
using Systems.Details.Build_Details.Scripts;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Systems.Input_Management
{
    public class FloorPlanningInputHandler : MonoBehaviour, IInputHandler
    {
        private FloorBuilder _floorBuilder;
        private bool _isPlanning;
        private Vector2 _startPos;
        private List<Vector2> _plannedGrid = new List<Vector2>();
        private List<TilePlan> _plannedTiles = new List<TilePlan>();
        
        private void Awake()
        {
            // Register this handler with the InputManager
            InputManager.Instance.RegisterInputHandler(InputMode.FloorPlanning, this);

            _floorBuilder = FindObjectOfType<FloorBuilder>();
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
                _floorBuilder.CancelFloorBuild();
            }
        }
        
        private void StartPlanning()
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;

            _isPlanning = true;
            _startPos = Helper.SnapToGridPos(UtilsClass.GetMouseWorldPosition());

            // Optionally clear previous plans to start fresh
            ClearTilePlan();
        }

        private void ContinuePlanning()
        {
            Vector3 currentMousePos = Helper.SnapToGridPos(UtilsClass.GetMouseWorldPosition());
            var newGrid = Helper.GetRectangleGridPositionsBetweenPoints(_startPos, currentMousePos);
            
            if (newGrid.Count != _plannedGrid.Count)
            {
                _plannedGrid = newGrid;
                UpdatePlannedTiles();
            }
        }

        private void FinishPlanning()
        {
            if (_isPlanning)
            {
                ClearTilePlan();  // Optionally clear the visual indicators
                foreach (var gridPos in _plannedGrid)
                {
                    _floorBuilder.SpawnFloor(gridPos);
                }
                _plannedGrid.Clear();
                _isPlanning = false;  // End planning process
            }
        }
        
        private void CancelPlanning()
        {
            _isPlanning = false;  // Reset the planning state
            ClearTilePlan();  // Optionally clear the visual indicators
            _plannedGrid.Clear();
        }
        
        private void UpdatePlannedTiles()
        {
            ClearTilePlan();  // Clear previous display
            foreach (var gridPos in _plannedGrid)
            {
                var tilePlanGO = new GameObject("Tile Plan", typeof(TilePlan));
                tilePlanGO.transform.position = gridPos;

                var tilePlan = tilePlanGO.GetComponent<TilePlan>();
                var tileMap = TilemapController.Instance.GetTilemap(TilemapLayer.Flooring);
                Color placementColour = Helper.IsGridPosValidToBuild(gridPos, _floorBuilder.InvalidPlacementTags)
                    ? Librarian.Instance.GetColour("Placement Green")
                    : Librarian.Instance.GetColour("Placement Red");

                tilePlan.Init(_floorBuilder.FloorStyle.Tiles, tileMap, placementColour);
                _plannedTiles.Add(tilePlan);
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
