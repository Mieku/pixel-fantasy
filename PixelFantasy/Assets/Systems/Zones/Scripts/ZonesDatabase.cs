using System;
using System.Collections.Generic;
using System.Linq;
using Controllers;
using Managers;
using Sirenix.OdinInspector;
using Systems.Build_Controls.Scripts;
using Systems.CursorHandler.Scripts;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Systems.Zones.Scripts
{
    public class ZonesDatabase : Singleton<ZonesDatabase>
    {
        [SerializeField] private Sprite _placementIcon;
        [SerializeField] private LayeredTilemapManager _zoneLayeredTilemap;
        [SerializeField] private TileBase _defaultZoneTiles;
        [SerializeField] private StockpileZoneData _genericStockpileZoneData;
        [SerializeField] private FarmingZoneData _genericFarmZoneData;
        
        private bool _isEnabled;
        private bool _isPlanning;
        private bool _isPlanningShrink;
        private Vector2 _startPos;
        private List<Vector2> _plannedGrid = new List<Vector2>();
        private readonly List<TilePlan> _plannedTiles = new List<TilePlan>();
        [ShowInInspector] private List<ZoneData> _currentZones = new List<ZoneData>();
       
        private List<string> _invalidPlacementTags => new List<string>() { "Water", "Zone", "Obstacle"};
        private List<string> _requiredPlacementTagsForShrink => new List<string>() { "Zone" };

        private ZoneSettings _curZoneSettings;
        private ZoneData _curSelectedZone;
        private ZoneData _expandingZone;
        private Action _planningCompleteCallback;

        public List<ZoneData> SaveZonesData()
        {
            foreach (var zoneData in _currentZones)
            {
                if (zoneData is StockpileZoneData sZoneData)
                {
                    sZoneData.PrepSave();
                }
            }
            
            return _currentZones;
        }

        public void LoadZonesData(List<ZoneData> data)
        {
            foreach (var zoneData in data)
            {
                LoadZone(zoneData);
            }
        }

        public void ClearAllZones()
        {
            var zones = _currentZones.ToList();
            foreach (var zone in zones)
            {
                zone.ClearCells();
            }
            
            _currentZones.Clear();
        }

        public void SelectZone(ZoneData zone)
        {
            UnselectZone();
            _curSelectedZone = zone;
            
            foreach (var cell in zone.Cells)
            {
                DrawTileCell(cell, zone.AssignedLayer, zone.SelectedTiles, zone.ZoneColour);
            }
            
            HUDController.Instance.ShowZoneDetails(zone);
        }

        public void UnselectZone()
        {
            if (_curSelectedZone != null)
            {
                foreach (var cell in _curSelectedZone.Cells)
                {
                    DrawTileCell(cell, _curSelectedZone.AssignedLayer, _curSelectedZone.DefaultTiles, _curSelectedZone.ZoneColour);
                }
                _curSelectedZone = null;
                
                HUDController.Instance.HideDetails();
            }
        }

        public void DeleteZone(ZoneData zoneData)
        {
            if (zoneData.ZoneType == ZoneSettings.EZoneType.Stockpile)
            {
                InventoryManager.Instance.RemoveStorage((StockpileZoneData) zoneData);
            }
            
            _currentZones.Remove(zoneData);
            
            var tileMap = _zoneLayeredTilemap.FindOrCreateLayer(zoneData.AssignedLayer);
            foreach (var cellPos in zoneData.Cells)
            {
                var cell = tileMap.WorldToCell(cellPos);
                tileMap.SetTile(cell, null);
            }
            
            _zoneLayeredTilemap.RemoveLayer(zoneData.AssignedLayer);

            foreach (var cellObject in zoneData.ZoneCells)
            {
                cellObject.DeleteCell();
            }
            
            if (_curSelectedZone != null && _curSelectedZone.AssignedLayer == zoneData.AssignedLayer)
            {
                _curSelectedZone = null;
            }
            
            GameEvents.Trigger_RefreshInventoryDisplay();
            HUDController.Instance.HideDetails();
        }

        public void BeginPlanningZone(ZoneSettings zoneSettings, Action planningCompleteCallback)
        {
            _curZoneSettings = zoneSettings;
            _planningCompleteCallback = planningCompleteCallback;
            
            CursorManager.Instance.ChangeCursorState(ECursorState.AreaSelect);
            Spawner.Instance.ShowPlacementIcon(true, _placementIcon, _invalidPlacementTags);
            _isEnabled = true;
        }

        /// <summary>
        /// Expands the zone if connected to the original, or creates a copy if not connected
        /// </summary>
        public void BeginPlanningZoneExpansion(ZoneData original, Action planningCompleteCallback)
        {
            _expandingZone = original;
            _curZoneSettings = original.Settings;
            _planningCompleteCallback = planningCompleteCallback;
            
            CursorManager.Instance.ChangeCursorState(ECursorState.AreaSelect);
            Spawner.Instance.ShowPlacementIcon(true, _placementIcon, _invalidPlacementTags);
            _isEnabled = true;
        }

        // Shrinks any zone
        public void BeginPlanningZoneShrinking(Action onCompleteCallback)
        {
            _planningCompleteCallback = onCompleteCallback;
            
            CursorManager.Instance.ChangeCursorState(ECursorState.AreaSelect);
            Spawner.Instance.ShowPlacementIconForReqTags(true, _placementIcon, _requiredPlacementTagsForShrink);
            _isEnabled = true;
            _isPlanningShrink = true;
        }
        
        public void CancelZonePlanning()
        {
            _isEnabled = false;
            CursorManager.Instance.ChangeCursorState(ECursorState.Default);
            Spawner.Instance.ShowPlacementIcon(false);
            
            ClearTilePlan();
            _curZoneSettings = null;
            _expandingZone = null;
            _isPlanningShrink = false;
            _planningCompleteCallback?.Invoke();
        }
        
        protected override void Awake()
        {
            base.Awake();
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
            if (!_isEnabled)
            {
                UnselectZone();
                return;
            }
            
            CancelZonePlanning();
        }
    
        protected void GameEvents_OnLeftClickDown(Vector3 mousePos, PlayerInputState inputState, bool isOverUI)
        {
            if (!_isEnabled) return;
            if (isOverUI) return;

            _isPlanning = true;
            _startPos = Helper.SnapToGridPos(mousePos);
        }

        protected void GameEvents_OnLeftClickHeld(Vector3 mousePos, PlayerInputState inputState, bool isOverUI)
        {
            if (!_isEnabled) return;
            if (!_isPlanning) return;

            if (_isPlanningShrink)
            {
                PlanZoneShrink(mousePos);
            }
            else
            {
                PlanZone(mousePos);
            }
        }

        protected void GameEvents_OnLeftClickUp(Vector3 mousePos, PlayerInputState inputState, bool isOverUI)
        {
            if (!_isEnabled) return;
            if (isOverUI) return;

            if (_isPlanning)
            {
                if (_isPlanningShrink)
                {
                    ShrinkPlannedZone();
                }
                else
                {
                    SpawnPlannedZone();
                }
                
                CancelZonePlanning();
            }
        }
        
        private void PlanZone(Vector2 mousePos)
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

        private void PlanZoneShrink(Vector2 mousePos)
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

        private void ShrinkPlannedZone()
        {
            // Make sure to update all zones affected!
            if (!_isPlanning) return;
            
            Spawner.Instance.ShowPlacementIconForReqTags(true, _placementIcon, _requiredPlacementTagsForShrink);
            ClearTilePlan();
            
            foreach (var gridPos in _plannedGrid)
            {
                if (Helper.IsGridPosValidToBuild(gridPos, new List<string>(), _requiredPlacementTagsForShrink))
                {
                    Vector3Int cell = new Vector3Int
                    {
                        x = (int)gridPos.x,
                        y = (int)gridPos.y,
                        z = 0
                    };
                    
                    
                    ZoneData affectedZone = GetZoneAtPosition(cell);
                    if (affectedZone != null)
                    {
                        affectedZone.RemoveCell(cell);
                    }
                }
            }
            
            _plannedGrid.Clear();
            _isPlanning = false;
            _expandingZone = null;
            _isPlanningShrink = false;

            if (_curSelectedZone != null)
            {
                SelectZone(_curSelectedZone);
            }
            else
            {
                UnselectZone();
            }
        }
        
        private void SpawnPlannedZone()
        {
            if (!_isPlanning) return;
            
            Spawner.Instance.ShowPlacementIcon(true, _placementIcon, _invalidPlacementTags);
            ClearTilePlan();

            ZoneData zone;

            // Zone Expansion Logic
            if (_expandingZone != null)
            {
                bool isAdjacent = CheckIfAdjacent(_expandingZone.Cells, _plannedGrid);
                if (isAdjacent)
                {
                    // Expand the zone
                    int zoneLayer = _expandingZone.AssignedLayer;
                    List<Vector3Int> validGrid = new List<Vector3Int>();
                    foreach (var gridPos in _plannedGrid)
                    {
                        if (Helper.IsGridPosValidToBuild(gridPos, _invalidPlacementTags))
                        {
                            Vector3Int cell = new Vector3Int
                            {
                                x = (int)gridPos.x,
                                y = (int)gridPos.y
                            };
                            validGrid.Add(cell);
                            DrawTileCell(gridPos, zoneLayer, _expandingZone.DefaultTiles, _expandingZone.ZoneColour);
                        }
                    }

                    zone = ExpandZone(validGrid, _expandingZone);
                }
                else
                {
                    // Create a new zone with a copy of all the data (except name, and layer id)
                    int zoneLayer = _zoneLayeredTilemap.GetLowestNotUsedLayer();
                    List<Vector3Int> validGrid = new List<Vector3Int>();
                    foreach (var gridPos in _plannedGrid)
                    {
                        if (Helper.IsGridPosValidToBuild(gridPos, _invalidPlacementTags))
                        {
                            Vector3Int cell = new Vector3Int
                            {
                                x = (int)gridPos.x,
                                y = (int)gridPos.y
                            };
                            validGrid.Add(cell);
                            DrawTileCell(gridPos, zoneLayer, _expandingZone.DefaultTiles, _expandingZone.ZoneColour);
                        }
                    }

                    zone = CopyZone(validGrid, zoneLayer, _expandingZone);
                }
            }
            else
            {
                int zoneLayer = _zoneLayeredTilemap.GetLowestNotUsedLayer();

                List<Vector3Int> validGrid = new List<Vector3Int>();
                foreach (var gridPos in _plannedGrid)
                {
                    if (Helper.IsGridPosValidToBuild(gridPos, _invalidPlacementTags))
                    {
                        Vector3Int cell = new Vector3Int
                        {
                            x = (int)gridPos.x,
                            y = (int)gridPos.y
                        };
                        validGrid.Add(cell);
                        DrawTileCell(gridPos, zoneLayer, _curZoneSettings.DefaultTiles, _curZoneSettings.ZoneColour);
                    }
                }
            
                zone = CreateZone(validGrid, zoneLayer);
            }
            
            _plannedGrid.Clear();
            _isPlanning = false;
            _expandingZone = null;
            
            SelectZone(zone);
        }
        
        public ZoneData CreateZone(List<Vector3Int> tilePositions, int layer)
        {
            List<ZoneCell> cellObjects = new List<ZoneCell>();
            switch (_curZoneSettings.ZoneType)
            {
                case ZoneSettings.EZoneType.Stockpile:
                    var stockpileRuntimeData = new StockpileZoneData();
                    
                    stockpileRuntimeData.Cells = new List<Vector3Int>(tilePositions);
                    stockpileRuntimeData.AssignedLayer = layer;
                    stockpileRuntimeData.InitData((StockpileZoneSettings)_curZoneSettings);
                    _currentZones.Add(stockpileRuntimeData);
                    
                    foreach (var tilePosition in tilePositions)
                    {
                        // Create zone cell game objects
                        var cellObj = Instantiate(stockpileRuntimeData.Settings.CellPrefab, tilePosition, Quaternion.identity, transform);
                        cellObj.Init(stockpileRuntimeData, tilePosition);
                        cellObjects.Add(cellObj);
                    }
                    stockpileRuntimeData.ZoneCells = cellObjects;
                    
                    InventoryManager.Instance.AddStorage(stockpileRuntimeData);

                    return stockpileRuntimeData;
                case ZoneSettings.EZoneType.Farm:
                    var farmRuntimeData = new FarmingZoneData();
                    farmRuntimeData.Cells = new List<Vector3Int>(tilePositions);
                    farmRuntimeData.AssignedLayer = layer;
                    farmRuntimeData.InitData((FarmingZoneSettings)_curZoneSettings);
                    _currentZones.Add(farmRuntimeData);
                    
                    foreach (var tilePosition in tilePositions)
                    {
                        // Create zone cell game objects
                        var cellObj = Instantiate(farmRuntimeData.Settings.CellPrefab, tilePosition, Quaternion.identity, transform);
                        cellObj.Init(farmRuntimeData, tilePosition);
                        cellObjects.Add(cellObj);
                    }
                    farmRuntimeData.ZoneCells = cellObjects;

                    return farmRuntimeData;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public void LoadZone(ZoneData zoneData)
        {
            List<Vector3Int> tilePositions = zoneData.Cells;
            
            List<ZoneCell> cellObjects = new List<ZoneCell>();
            switch (zoneData.ZoneType)
            {
                case ZoneSettings.EZoneType.Stockpile:
                    var stockpileRuntimeData = (StockpileZoneData) zoneData; 
                    _currentZones.Add(stockpileRuntimeData);
                    
                    foreach (var tilePosition in tilePositions)
                    {
                        // Create zone cell game objects
                        var cellObj = Instantiate(stockpileRuntimeData.Settings.CellPrefab, tilePosition, Quaternion.identity, transform);
                        cellObj.Init(stockpileRuntimeData, tilePosition);
                        cellObjects.Add(cellObj);
                    }
                    stockpileRuntimeData.ZoneCells = cellObjects;

                    InventoryManager.Instance.AddStorage(stockpileRuntimeData);
                    
                    break;
                case ZoneSettings.EZoneType.Farm:
                    var farmRuntimeData = (FarmingZoneData) zoneData;
                    _currentZones.Add(farmRuntimeData);
                    
                    foreach (var tilePosition in tilePositions)
                    {
                        // Create zone cell game objects
                        var cellObj = Instantiate(farmRuntimeData.Settings.CellPrefab, tilePosition, Quaternion.identity, transform);
                        cellObj.Init(farmRuntimeData, tilePosition);
                        cellObjects.Add(cellObj);
                    }
                    farmRuntimeData.ZoneCells = cellObjects;

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            // Redraw the zone
            foreach (var cell in zoneData.Cells)
            {
                DrawTileCell(cell, zoneData.AssignedLayer, zoneData.DefaultTiles, zoneData.ZoneColour);
            }
        }

        public ZoneData CopyZone(List<Vector3Int> tilePositions, int layer, ZoneData originalData, List<ZoneCell> cellsToTransfer = default)
        {
            List<ZoneCell> cellObjects = new List<ZoneCell>();
            switch (originalData.Settings.ZoneType)
            {
                case ZoneSettings.EZoneType.Stockpile:
                    var stockpileRuntimeData = new StockpileZoneData(); 
                    stockpileRuntimeData.Cells = new List<Vector3Int>(tilePositions);
                    stockpileRuntimeData.AssignedLayer = layer;
                    stockpileRuntimeData.CopyData(originalData as StockpileZoneData);
                    _currentZones.Add(stockpileRuntimeData);
                    
                    foreach (var tilePosition in tilePositions)
                    {
                        // Create zone cell game objects
                        var existingCell = cellsToTransfer?.Find(cell => cell.CellPos == tilePosition);
                        if (existingCell != null)
                        {
                            existingCell.TransferOwner(stockpileRuntimeData);
                            cellObjects.Add(existingCell);
                        }
                        else
                        {
                            var cellObj = Instantiate(stockpileRuntimeData.Settings.CellPrefab, tilePosition, Quaternion.identity, transform);
                            cellObj.Init(stockpileRuntimeData, tilePosition);
                            cellObjects.Add(cellObj);
                        }
                    }
                    stockpileRuntimeData.ZoneCells = cellObjects;

                    InventoryManager.Instance.AddStorage(stockpileRuntimeData);
                    
                    return stockpileRuntimeData;
                case ZoneSettings.EZoneType.Farm:
                    var farmRuntimeData = new FarmingZoneData();
                    farmRuntimeData.Cells = new List<Vector3Int>(tilePositions);
                    farmRuntimeData.AssignedLayer = layer;
                    farmRuntimeData.CopyData(originalData as FarmingZoneData);
                    _currentZones.Add(farmRuntimeData);
                    
                    foreach (var tilePosition in tilePositions)
                    {
                        var existingCell = cellsToTransfer?.Find(cell => cell.CellPos == tilePosition);
                        if (existingCell != null)
                        {
                            existingCell.TransferOwner(farmRuntimeData);
                            cellObjects.Add(existingCell);
                        }
                        else
                        {
                            // Create zone cell game objects
                            var cellObj = Instantiate(farmRuntimeData.Settings.CellPrefab, tilePosition, Quaternion.identity, transform);
                            cellObj.Init(farmRuntimeData, tilePosition);
                            cellObjects.Add(cellObj);
                        }
                    }
                    farmRuntimeData.ZoneCells = cellObjects;

                    return farmRuntimeData;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public ZoneData ExpandZone(List<Vector3Int> expansion, ZoneData zone)
        {
            List<ZoneCell> cellObjects = new List<ZoneCell>();
            foreach (var tilePosition in expansion)
            {
                // Create zone cell game objects
                var cellObj = Instantiate(zone.Settings.CellPrefab, tilePosition, Quaternion.identity, transform);
                cellObj.Init(zone, tilePosition);
                cellObjects.Add(cellObj);
            }
            zone.ZoneCells.AddRange(cellObjects);
            zone.Cells.AddRange(new List<Vector3Int>(expansion));
            
            return zone;
        }
        
        public void DrawTileCell(Vector3 spawnPosition, int layer, TileBase tile, Color colour)
        {
            var tileMap = _zoneLayeredTilemap.FindOrCreateLayer(layer);
            var cell = tileMap.WorldToCell(spawnPosition);
            tileMap.SetTile(cell, tile);
            tileMap.SetColor(cell, colour);
        }

        public void ClearTileCell(Vector3Int cell, int layer)
        {
            var tileMap = _zoneLayeredTilemap.FindOrCreateLayer(layer);
            tileMap.SetTile(cell, null);
        }

        public void ColourZone(ZoneData zone, Color? colour)
        {
            var tilemap = _zoneLayeredTilemap.GetLayer(zone.AssignedLayer);
            foreach (var cell in zone.Cells)
            {
                if (colour == null)
                {
                    tilemap.SetColor(cell, zone.ZoneColour);
                }
                else
                {
                    tilemap.SetColor(cell, (Color)colour);
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

        public Vector2 ZoneCenter(List<Vector3Int> cells)
        {
            List<float> horCells = new List<float>();
            List<float> vertCells = new List<float>();

            foreach (var cell in cells)
            {
                var pos = _zoneLayeredTilemap.grid.CellToWorld(cell);
                horCells.Add(pos.x + 0.5f);
                vertCells.Add(pos.y + 0.5f);
            }

            var horMin = horCells.Min();
            var horMax = horCells.Max();
            var vertMin = vertCells.Min();
            var vertMax = vertCells.Max();

            Vector2 result = new Vector2
            {
                x = (horMin + horMax) / 2f,
                y = (vertMin + vertMax) / 2f
            };
            
            return result;
        }

        /// <summary>
        /// Check if any of _plannedGrid are adjacent to (and valid) to the expanding zone
        /// </summary>
        public bool CheckIfAdjacent(List<Vector3Int> exisitingCells, List<Vector2> plannedPositions)
        {
            foreach (var plannedPos in plannedPositions)
            {
                if (Helper.IsGridPosValidToBuild(plannedPos, _invalidPlacementTags))
                {
                    var neighbours = GetNeighbourCells(plannedPos);
                    foreach (var neighbour in neighbours)
                    {
                        if (exisitingCells.Contains(neighbour))
                        {
                            return true;
                        }
                    }
                }
            }
            
            return false;
        }

        public List<Vector3Int> GetNeighbourCells(Vector2 startPos)
        {
            Vector3Int startCell = new Vector3Int
            {
                x = (int)startPos.x,
                y = (int)startPos.y,
                z = 0
            };

            List<Vector3Int> results = new List<Vector3Int>();
            results.Add(startCell + Vector3Int.up);
            results.Add(startCell + Vector3Int.down);
            results.Add(startCell + Vector3Int.left);
            results.Add(startCell + Vector3Int.right);

            return results;
        }

        public ZoneData GetZoneAtPosition(Vector3Int cell)
        {
            return _currentZones.FirstOrDefault(zone => zone.Cells.Contains(cell));
        }

        public int GetUniqueLayerId()
        {
            return _zoneLayeredTilemap.GetLowestNotUsedLayer();
        }
     }
}
