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

        private List<string> _invalidPlacementTags = new List<string>() { "Water", "Wall", "Structure", "Obstacle", "Clearance" };

        public List<string> InvalidPlacementTags => _invalidPlacementTags;
        public WallSettings WallSettings => _wallSettings;
        

        public void BeginWallBuild(WallSettings wallSettings, DyeSettings colour)
        {
            _wallSettings = wallSettings;
            _colour = colour;

            // Set up the cursor and placement icon
            InputManager.Instance.SetInputMode(InputMode.WallPlanning);
            CursorManager.Instance.ChangeCursorState(ECursorState.AreaSelect);
            Spawner.Instance.ShowPlacementIcon(true, _placementIcon, _invalidPlacementTags);
        }

        public void CancelWallBuild()
        {
            // Reset cursor and placement icon when build is canceled
            CursorManager.Instance.ChangeCursorState(ECursorState.Default);
            Spawner.Instance.ShowPlacementIcon(false);
            InputManager.Instance.ReturnToDefault();
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

        // private void PlanWall(Vector2 mousePos)
        // {
        //     Spawner.Instance.ShowPlacementIcon(false);
        //     
        //     Vector3 curGridPos = Helper.SnapToGridPos(mousePos);
        //     List<Vector2> gridPositions = new List<Vector2>();
        //     
        //     gridPositions = Helper.GetBoxPositionsBetweenPoints(_startPos, curGridPos);
        //     
        //     if (gridPositions.Count != _plannedGrid.Count)
        //     {
        //         _plannedGrid = gridPositions;
        //     
        //         // Clear previous display, then display new blueprints
        //         ClearTilePlan();
        //     
        //         foreach (var gridPos in gridPositions)
        //         {
        //             var tilePlanGO = new GameObject("Tile Plan", typeof(TilePlan));
        //             tilePlanGO.transform.position = gridPos;
        //     
        //             var tilePlan = tilePlanGO.GetComponent<TilePlan>();
        //             var tileMap = TilemapController.Instance.GetTilemap(TilemapLayer.Structure);
        //             Color placementColour;
        //             if (Helper.IsGridPosValidToBuild(gridPos, _invalidPlacementTags))
        //             {
        //                 placementColour = Librarian.Instance.GetColour("Placement Green");
        //             }
        //             else
        //             {
        //                 placementColour = Librarian.Instance.GetColour("Placement Red");
        //             }
        //         
        //             tilePlan.Init(_wallSettings.ExteriorRuleTile, tileMap, placementColour);
        //     
        //             _plannedTiles.Add(tilePlan);
        //         }
        //     }
        // }
        //
        // private void SpawnPlannedWall()
        // {
        //     if (!_isPlanning) return;
        //     
        //     Spawner.Instance.ShowPlacementIcon(true, _placementIcon, _invalidPlacementTags);
        //     ClearTilePlan();
        //     
        //     foreach (var gridPos in _plannedGrid)
        //     {
        //         if (Helper.IsGridPosValidToBuild(gridPos, _invalidPlacementTags))
        //         {
        //             SpawnWall(gridPos);
        //         }
        //     }
        //     
        //     _plannedGrid.Clear();
        //     _isPlanning = false;
        // }
    
        // public void SpawnWall(Vector3 spawnPosition)
        // {
        //     if (Helper.IsGridPosValidToBuild(spawnPosition, _invalidPlacementTags))
        //     {
        //         spawnPosition = new Vector3(spawnPosition.x, spawnPosition.y, -1);
        //         var wall = Instantiate(_wallPrefab, spawnPosition, Quaternion.identity);
        //         wall.transform.SetParent(ParentsManager.Instance.StructuresParent);
        //         wall.Init(_wallSettings, _colour);
        //     }
        // }
        //
        // public void SpawnLoadedWall(WallData wallData)
        // {
        //     var wall = Instantiate(_wallPrefab, wallData.Position, Quaternion.identity);
        //     wall.transform.SetParent(ParentsManager.Instance.StructuresParent);
        //     wall.LoadData(wallData);
        // }
    
        // private void ClearTilePlan()
        // {
        //     foreach (var tilePlan in _plannedTiles)
        //     {
        //         tilePlan.Clear();
        //         Destroy(tilePlan.gameObject);
        //     }
        //     _plannedTiles.Clear();
        // }
    }
}
