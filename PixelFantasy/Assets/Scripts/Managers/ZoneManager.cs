using System;
using System.Collections.Generic;
using Buildings;
using Controllers;
using HUD;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zones;

namespace Managers
{
    public class ZoneManager : Singleton<ZoneManager>
    {
        [SerializeField] private GameObject _zonesTilemap;
        [SerializeField] private RuleTile _zoneRuleTile;
        [SerializeField] private Transform _panelCanvas;
        
        public GameObject ZonePanelPrefab;
        
        public List<IZone> Zones = new List<IZone>();
        
        private bool _zonesVisible;
        private bool _isPlanningZone;
        private Vector2 _startPos;
        private List<Vector2> _plannedGrid = new List<Vector2>();
        private List<Vector3Int> _pendingCells = new List<Vector3Int>();
        private Tilemap _zonesTM;
        private Tilemap _pendingZonesTM;
        private ZoneType _curZoneType;
        private List<string> _defaultInvalidTagsForZone = new List<string>() { "Water", "Zone" };
        private IZone _zoneToModify;
        private Building _requestorBuilding;

        protected override void Awake()
        {
            base.Awake();
            GameEvents.OnLeftClickDown += GameEvents_OnLeftClickDown;
            GameEvents.OnLeftClickHeld += GameEvents_OnLeftClickHeld;
            GameEvents.OnLeftClickUp += GameEvents_OnLeftClickUp;
            GameEvents.OnRightClickDown += GameEvents_OnRightClickDown;
            GameEvents.OnRightClickHeld += GameEvents_OnRightClickHeld;
            GameEvents.OnRightClickUp += GameEvents_OnRightClickUp;

            _zonesTM = TilemapController.Instance.GetTilemap(TilemapLayer.Zones);
            _pendingZonesTM = TilemapController.Instance.GetTilemap(TilemapLayer.PendingZones);
        }

        private void OnDestroy()
        {
            GameEvents.OnLeftClickDown -= GameEvents_OnLeftClickDown;
            GameEvents.OnLeftClickHeld -= GameEvents_OnLeftClickHeld;
            GameEvents.OnLeftClickUp -= GameEvents_OnLeftClickUp;
            GameEvents.OnRightClickDown -= GameEvents_OnRightClickDown;
            GameEvents.OnRightClickHeld -= GameEvents_OnRightClickHeld;
            GameEvents.OnRightClickUp -= GameEvents_OnRightClickUp;
        }

        private void Update()
        {
            DisplayZoneInput();
        }

        public void PlanZone(ZoneType zoneType, Building requestorBuilding = null)
        {
            _curZoneType = zoneType;
            _requestorBuilding = requestorBuilding;
            
            var zoneColour = Librarian.Instance.GetZoneTypeData(_curZoneType).Colour;
            ShowZones(true);
            PlayerInputController.Instance.ChangeState(PlayerInputState.Zone);
            Spawner.Instance.ShowPlacementIcon(true, _zoneRuleTile.m_DefaultSprite, _defaultInvalidTagsForZone, 1f, zoneColour);
        }

        #region GameEvents

        private void GameEvents_OnLeftClickDown(Vector3 mousePos, PlayerInputState inputState, bool isOverUI)
        {
            StartPlanningZone_LeftDown(mousePos, inputState, isOverUI);
            DetectZoneClicked(mousePos, inputState, isOverUI);
        }
        
        private void GameEvents_OnLeftClickHeld(Vector3 mousePos, PlayerInputState inputState, bool isOverUI)
        {
            PlanZone_LeftHeld(mousePos, inputState, isOverUI);
        }
        
        private void GameEvents_OnLeftClickUp(Vector3 mousePos, PlayerInputState inputState, bool isOverUI)
        {
            CreateZone_LeftUp(mousePos, inputState, isOverUI);
        }
        
        private void GameEvents_OnRightClickDown(Vector3 mousePos, PlayerInputState inputState, bool isOverUI)
        {
            StartShrinkingZone_RightDown(mousePos, inputState, isOverUI);
        }

        private void GameEvents_OnRightClickHeld(Vector3 mousePos, PlayerInputState inputState, bool isOverUI)
        {
            SelectShrinkingZone_RightHeld(mousePos, inputState, isOverUI);
        }
        
        private void GameEvents_OnRightClickUp(Vector3 mousePos, PlayerInputState inputState, bool isOverUI)
        {
            // Finish Shrinking Zone Here
            SkrinkZone_RightUp(mousePos, inputState, isOverUI);
            
            UnclickAllZones();
            if (inputState != PlayerInputState.Zone) return;
            CancelInput();
        }

        private void GameEvents_OnTabPressed()
        {
            ShowZones(!_zonesVisible);
        }
        
        #endregion

        #region Zone Display

        private void DetectZoneClicked(Vector3 mousePos, PlayerInputState inputState, bool isOverUI)
        {
            if (isOverUI) return;
            if(inputState != PlayerInputState.None) return;
            if (!_zonesVisible) return;
            
            // Click a zone if available
            var gridPos = Helper.ConvertMousePosToGridPos(mousePos);
            var zoneClicked = GetZoneByGridPos(gridPos);
            if (zoneClicked != null)
            {
                UnclickAllZones();
                zoneClicked.ClickZone();
            }
        }

        private void UnclickAllZones()
        {
            // Unclick all zones
            foreach (var zone in Zones)
            {
                zone.UnclickZone();
            }
        }
        
        private void StartPlanningZone_LeftDown(Vector3 mousePos, PlayerInputState inputState, bool isOverUI)
        {
            if (isOverUI) return;
            if (inputState != PlayerInputState.Zone) return;
            
            _startPos = Helper.ConvertMousePosToGridPos(mousePos);
            _isPlanningZone = true;

            // Detect if click is starting from a zone, if so, don't make a new one instead expand the clicked one
            if (Helper.DoesGridContainTag(_startPos, "Zones"))
            {
                var zoneTypeData = Librarian.Instance.GetZoneTypeData(_curZoneType);
                var potentialZoneToModify = GetZoneByGridPos(_startPos);
                if (potentialZoneToModify.ZoneTypeData == zoneTypeData)
                {
                    _zoneToModify = potentialZoneToModify;
                }
                else
                {
                    _zoneToModify = null;
                }
            }
            else
            {
                _zoneToModify = null;
            }
        }
        
        private void PlanZone_LeftHeld(Vector3 mousePos, PlayerInputState inputState, bool isOverUI)
        {
            if (inputState != PlayerInputState.Zone) return;
            if (!_isPlanningZone) return;
            
            Vector3 curGridPos = Helper.ConvertMousePosToGridPos(mousePos);
            List<Vector2> gridPositions = Helper.GetRectangleGridPositionsBetweenPoints(_startPos, curGridPos);

            if (gridPositions.Count != _plannedGrid.Count)
            {
                _plannedGrid = gridPositions;
                
                // Clear previous tiles, then display new tiles
                ClearPendingTiles();

                foreach (var gridPos in gridPositions)
                {
                    if (Helper.IsGridPosValidToBuild(gridPos, _defaultInvalidTagsForZone))
                    {
                        // Create Pending Tile
                        var cell = _zonesTM.WorldToCell(gridPos);
                        _pendingZonesTM.SetTile(cell, _zoneRuleTile);
                        _pendingZonesTM.SetColor(cell, Librarian.Instance.GetZoneTypeData(_curZoneType).Colour);
                        _pendingCells.Add(cell);
                    }
                }
            }
        }
        
        private void CreateZone_LeftUp(Vector3 mousePos, PlayerInputState inputState, bool isOverUI)
        {
            if (inputState != PlayerInputState.Zone) return;

            LayeredRuleTile zoneRuleTile;
            
            if (_zoneToModify != null)
            {
                zoneRuleTile = _zoneToModify.LayeredRuleTile;
            }
            else
            {
                zoneRuleTile = Instantiate(_zoneRuleTile) as LayeredRuleTile;
            }
            
            // Remove the tiles from the pending tilemap, and add to the zones tilemap
            foreach (var cell in _pendingCells)
            {
                _pendingZonesTM.SetTile(cell, null);
                _zonesTM.SetTile(cell, zoneRuleTile);
                _zonesTM.SetColor(cell, Librarian.Instance.GetZoneTypeData(_curZoneType).Colour);
            }
            
            // Turn the pending cells into a zone
            if (_pendingCells.Count == 0) return;
            if (_zoneToModify == null)
            {
                Zone newZone = CreateZoneByType(_curZoneType, _pendingCells, zoneRuleTile);
                Zones.Add(newZone);
            }
            else
            {
                _zoneToModify.ExpandZone(new List<Vector3Int>(_pendingCells));
            }
            
            _pendingCells.Clear();
        }

        private void StartShrinkingZone_RightDown(Vector3 mousePos, PlayerInputState inputState, bool isOverUI)
        {
            if (isOverUI) return;
            if (inputState != PlayerInputState.Zone) return;
            
            _startPos = Helper.ConvertMousePosToGridPos(mousePos);

            // Detect if click is starting from a zone, if so, begin shrinking the zone
            if (Helper.DoesGridContainTag(_startPos, "Zones"))
            {
                var potentialZoneToModify = GetZoneByGridPos(_startPos);
                _zoneToModify = potentialZoneToModify;
                _isPlanningZone = true;
            }
        }

        private void SelectShrinkingZone_RightHeld(Vector3 mousePos, PlayerInputState inputState, bool isOverUI)
        {
            if (inputState != PlayerInputState.Zone) return;
            
            if (!_isPlanningZone) return;
            
            Vector3 curGridPos = Helper.ConvertMousePosToGridPos(mousePos);
            List<Vector2> gridPositions = Helper.GetRectangleGridPositionsBetweenPoints(_startPos, curGridPos);

            if (gridPositions.Count != _plannedGrid.Count)
            {
                _plannedGrid = gridPositions;
                
                // Clear previous tiles, then display new tiles
                ClearPendingTiles();

                foreach (var gridPos in gridPositions)
                {
                    if (!Helper.IsGridPosValidToBuild(gridPos, new List<string>{"Zones"}))
                    {
                        // Create Pending Tile
                        var cell = _zonesTM.WorldToCell(gridPos);
                        _pendingZonesTM.SetTile(cell, _zoneRuleTile);
                        _pendingZonesTM.SetColor(cell, Color.red);
                        _pendingCells.Add(cell);
                    }
                }
            }
        }

        private void SkrinkZone_RightUp(Vector3 mousePos, PlayerInputState inputState, bool isOverUI)
        {
            if (inputState != PlayerInputState.Zone) return;
            if (_zoneToModify == null) return;

            // Remove the tiles from the pending tilemap, and remove from the zones tilemap
            foreach (var cell in _pendingCells)
            {
                _pendingZonesTM.SetTile(cell, null);
                _zonesTM.SetTile(cell, null);
            }
            
            // Remove the pending cells from a zone
            if (_pendingCells.Count == 0) return;
            _zoneToModify.ShrinkZone(new List<Vector3Int>(_pendingCells));
            
            _pendingCells.Clear();
        }

        private Zone CreateZoneByType(ZoneType zoneType, List<Vector3Int> gridPositions,
            LayeredRuleTile zoneRuleTile)
        {
            string uid = AssignUID();
            
            switch (zoneType)
            {
                case ZoneType.Storage:
                    return new StorageZone(uid, gridPositions, zoneRuleTile);
                case ZoneType.Farm:
                    return new FarmZone(uid, gridPositions, zoneRuleTile);
                case ZoneType.Home:
                    return new HomeZone(uid, gridPositions, zoneRuleTile);
                case ZoneType.Forester:
                    return new ForesterZone(uid, gridPositions, zoneRuleTile, _requestorBuilding as ForestersLodge);
                case ZoneType.Workshop:
                    var workshopRoom = Librarian.Instance.GetRoom("Workshop");
                    return new ProductionZone(uid, gridPositions, zoneRuleTile, workshopRoom as ProductionRoomData);
                default:
                    throw new ArgumentOutOfRangeException(nameof(zoneType), zoneType, null);
            }
        }

        private void DisplayZoneInput()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ShowZones(!_zonesVisible);
            }
        }

        // Display the zones
        private void ShowZones(bool makeVisible)
        {
            UnclickAllZones();
            _zonesVisible = makeVisible;
            _zonesTilemap.SetActive(makeVisible);
            GameEvents.Trigger_OnZoneDisplayChanged(makeVisible);
        }

        private void ClearPendingTiles()
        {
            foreach (var _pendingCell in _pendingCells)
            {
                _pendingZonesTM.SetTile(_pendingCell, null);
            }
            
            _pendingCells.Clear();
        }
        
        #endregion

        public IZone GetZoneByGridPos(Vector2 gridPos)
        {
            var cell = _zonesTM.WorldToCell(gridPos);
            foreach (var zone in Zones)
            {
                if (zone.GridPositions.Contains(cell))
                {
                    return zone;
                }
            }

            return null;
        }

        private string AssignUID()
        {
            bool confirmed = false;
            string potentialUID = "";

            while (!confirmed)
            {
                confirmed = true;
                potentialUID = "" + Guid.NewGuid();
                foreach (var zone in Zones)
                {
                    if (zone.UID == potentialUID)
                    {
                        confirmed = false;
                    }
                }
            }

            return potentialUID;
        }

        private void CancelInput()
        {
            PlayerInputController.Instance.ChangeState(PlayerInputState.None);
            Spawner.Instance.ShowPlacementIcon(false);
            _isPlanningZone = false;
            ClearPendingTiles();
        }

        public ZonePanel CreatePanel(IZone zone, Vector2 centerPos)
        {
            var zonePanel = Instantiate(ZonePanelPrefab, _panelCanvas).GetComponent<ZonePanel>();
            zonePanel.transform.position = centerPos;
            zonePanel.Init(zone);
            return zonePanel;
        }
    }
}
