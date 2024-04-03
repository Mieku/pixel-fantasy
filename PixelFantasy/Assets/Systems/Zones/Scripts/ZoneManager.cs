using System;
using System.Collections.Generic;
using System.Linq;
using Controllers;
using Data.Zones;
using Databrain;
using Managers;
using Systems.Build_Controls.Scripts;
using Systems.CursorHandler.Scripts;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Systems.Zones.Scripts
{
    public class ZoneManager : Singleton<ZoneManager>
    {
        public DataLibrary DataLibrary;
        
        [SerializeField] private Sprite _placementIcon;
        [SerializeField] private LayeredTilemapManager _zoneLayeredTilemap;
        [SerializeField] private StockpileZoneData _genericStockpileZoneData;
        [SerializeField] private FarmingZoneData _genericFarmZoneData;
        [SerializeField] private ZoneCellObject _zoneCellObjectPrefab;

        private bool _zonesVisible;
        private bool _isEnabled;
        private bool _isPlanning;
        private Vector2 _startPos;
        private List<Vector2> _plannedGrid = new List<Vector2>();
        private List<TilePlan> _plannedTiles = new List<TilePlan>();
        
        private List<ZoneData> _currentZones = new List<ZoneData>();
        private List<StockpileZoneData> _currentStockpileZones = new List<StockpileZoneData>();
        private List<FarmingZoneData> _currentFarmZones = new List<FarmingZoneData>();
       
        private List<string> _invalidPlacementTags => new List<string>() { "Water", "Zone"};

        private ZoneSettings _curZoneSettings;

        public void SelectZone(ZoneData zone)
        {
            Debug.Log(zone.ZoneName + " was selected");
        }

        public void BeginPlanningZone(ZoneSettings zoneSettings)
        {
            _curZoneSettings = zoneSettings;
            
            CursorManager.Instance.ChangeCursorState(ECursorState.AreaSelect);
            Spawner.Instance.ShowPlacementIcon(true, _placementIcon, _invalidPlacementTags);
            _isEnabled = true;
        }
        
        public void CancelZone()
        {
            _isEnabled = false;
            CursorManager.Instance.ChangeCursorState(ECursorState.Default);
            Spawner.Instance.ShowPlacementIcon(false);
            
            ClearTilePlan();
            _curZoneSettings = null;
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
            if(!_isEnabled) return;
            
            CancelZone();
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
            
            PlanZone(mousePos);
        }

        protected void GameEvents_OnLeftClickUp(Vector3 mousePos, PlayerInputState inputState, bool isOverUI)
        {
            if (!_isEnabled) return;

            if (_isPlanning)
            {
                SpawnPlannedZone();
            }
            else
            {
                if (!isOverUI)
                {
                    int layer = _zoneLayeredTilemap.GetLowestNotUsedLayer();
                    List<Vector2> cells = new List<Vector2>();
                    cells.Add(Helper.ConvertMousePosToGridPos(mousePos));
                    SpawnZone(cells.First(), layer);
                    var zone = CreateZone(cells, layer);
                    SelectZone(zone);
                }
            }
        }
        
        private void PlanZone(Vector2 mousePos)
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
                
                    tilePlan.Init(_curZoneSettings.Tiles, tileMap, placementColour);
            
                    _plannedTiles.Add(tilePlan);
                }
            }
        }
        
        private void SpawnPlannedZone()
        {
            if (!_isPlanning) return;
            int zoneLayer = _zoneLayeredTilemap.GetLowestNotUsedLayer();
            
            Spawner.Instance.ShowPlacementIcon(true, _placementIcon, _invalidPlacementTags);
            ClearTilePlan();


            List<Vector2> validGrid = new List<Vector2>();
            foreach (var gridPos in _plannedGrid)
            {
                if (Helper.IsGridPosValidToBuild(gridPos, _invalidPlacementTags))
                {
                    validGrid.Add(gridPos);
                    SpawnZone(gridPos, zoneLayer);
                }
            }
            
            var zone = CreateZone(validGrid, zoneLayer);
            
            _plannedGrid.Clear();
            _isPlanning = false;
            
            SelectZone(zone);
        }
        
        public ZoneData CreateZone(List<Vector2> tilePositions, int layer)
        {
            List<ZoneCellObject> cellObjects = new List<ZoneCellObject>();
            switch (_curZoneSettings.ZoneType)
            {
                case EZoneType.Stockpile:
                    var stockpileRuntimeData = (StockpileZoneData)DataLibrary.CloneDataObjectToRuntime(_genericStockpileZoneData);
                    stockpileRuntimeData.Cells = new List<Vector2>(tilePositions);
                    stockpileRuntimeData.AssignedLayer = layer;
                    stockpileRuntimeData.IsVisible = _zonesVisible;
                    stockpileRuntimeData.InitData((StockpileZoneSettings)_curZoneSettings);
                    _currentStockpileZones.Add(stockpileRuntimeData);
                    _currentZones.Add(stockpileRuntimeData);
                    
                    foreach (var tilePosition in tilePositions)
                    {
                        // Create a zone cell gameobject
                        var cellObj = Instantiate(_zoneCellObjectPrefab, tilePosition, Quaternion.identity, transform);
                        cellObj.Init(stockpileRuntimeData);
                        cellObjects.Add(cellObj);
                    }
                    stockpileRuntimeData.ZoneCellObjects = cellObjects;
                    
                    // DataLibrary.OnSaved += Saved;
                    // DataLibrary.OnLoaded += Loaded;

                    return stockpileRuntimeData;
                case EZoneType.Farm:
                    var farmRuntimeData = (FarmingZoneData)DataLibrary.CloneDataObjectToRuntime(_genericFarmZoneData);
                    farmRuntimeData.Cells = new List<Vector2>(tilePositions);
                    farmRuntimeData.AssignedLayer = layer;
                    farmRuntimeData.IsVisible = _zonesVisible;
                    farmRuntimeData.InitData((FarmingZoneSettings)_curZoneSettings);
                    _currentFarmZones.Add(farmRuntimeData);
                    _currentZones.Add(farmRuntimeData);
                    
                    foreach (var tilePosition in tilePositions)
                    {
                        // Create a zone cell gameobject
                        var cellObj = Instantiate(_zoneCellObjectPrefab, tilePosition, Quaternion.identity, transform);
                        cellObj.Init(farmRuntimeData);
                        cellObjects.Add(cellObj);
                    }
                    farmRuntimeData.ZoneCellObjects = cellObjects;
                    
                    // DataLibrary.OnSaved += Saved;
                    // DataLibrary.OnLoaded += Loaded;

                    return farmRuntimeData;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public void SpawnZone(Vector3 spawnPosition, int layer)
        {
            if (Helper.IsGridPosValidToBuild(spawnPosition, _invalidPlacementTags))
            {
                var tileMap = _zoneLayeredTilemap.FindOrCreateLayer(layer);
                var cell = tileMap.WorldToCell(spawnPosition);
                tileMap.SetTile(cell, _curZoneSettings.Tiles);
                tileMap.SetColor(cell, _curZoneSettings.ZoneColour);
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

        public Vector2 ZoneCenter(List<Vector2> cells)
        {
            List<float> horCells = new List<float>();
            List<float> vertCells = new List<float>();

            foreach (var cell in cells)
            {
                Vector3Int normCell = new Vector3Int
                {
                    x = (int)cell.x,
                    y = (int)cell.y,
                    z = 0
                };

                var pos = _zoneLayeredTilemap.grid.CellToWorld(normCell);
                horCells.Add(pos.x + 0.5f);
                vertCells.Add(pos.y + 0.5f);
            }

            var horMin = horCells.Min();
            var horMax = horCells.Max();
            var vertMin = vertCells.Min();
            var vertMax = vertCells.Max();

            Vector2 result = new Vector2();
            result.x = (horMin + horMax) / 2f;
            result.y = (vertMin + vertMax) / 2f;
            return result;
        }
    }
}
