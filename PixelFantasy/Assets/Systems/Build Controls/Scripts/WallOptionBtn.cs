using System;
using System.Collections.Generic;
using CodeMonkey.Utils;
using Controllers;
using Managers;
using Systems.Buildings.Scripts;
using Systems.CursorHandler.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Systems.Build_Controls.Scripts
{
    public class WallOptionBtn : OptionBtn
    {
        [SerializeField] private Image _btnBG;
        [SerializeField] private Sprite _defaultBtnSpr;
        [SerializeField] private Sprite _activeBtnSpr;
        [SerializeField] private Wall _wallPrefab;
        
        [SerializeField] protected Transform _costsLayout;
        [SerializeField] protected BuildControlCostDisplay _costDisplayPrefab;
        [SerializeField] protected GameObject _detailsPanel;
        [SerializeField] protected TextMeshProUGUI _optionName;
        [SerializeField] protected TextMeshProUGUI _optionDetails;
        
        private List<BuildControlCostDisplay> _displayedCosts = new List<BuildControlCostDisplay>();
    
        private WallOption _wallOption;
        private WallSettings _wallSettings;
        private bool _isEnabled;
        private bool _isPlanning;
        private Vector2 _startPos;
        
        private List<Vector2> _plannedGrid = new List<Vector2>();
        private List<TilePlan> _plannedTiles = new List<TilePlan>();
        private Transform _structureParent;
        private int _optionIndex;
    
        private void Awake()
        {
            GameEvents.OnLeftClickDown += GameEvents_OnLeftClickDown;
            GameEvents.OnLeftClickHeld += GameEvents_OnLeftClickHeld;
            GameEvents.OnLeftClickUp += GameEvents_OnLeftClickUp;
        }

        private void OnDestroy()
        {
            GameEvents.OnLeftClickDown -= GameEvents_OnLeftClickDown;
            GameEvents.OnLeftClickHeld -= GameEvents_OnLeftClickHeld;
            GameEvents.OnLeftClickUp -= GameEvents_OnLeftClickUp;
        }

        public void Init(WallOption wallOption, WallSettings wallSettings, SubCategoryBtn categoryBtn)
        {
            _wallOption = wallOption;
            _wallSettings = wallSettings;
            _ownerCategoryBtn = categoryBtn;
            _structureParent = Spawner.Instance.StructureParent;

            _icon.sprite = _wallOption.OptionIcon;
        }
    
        protected override void ToggledOn()
        {
            _btnBG.sprite = _activeBtnSpr;
            ShowDetailsPanel();
        }

        protected override void ToggledOff()
        {
            _btnBG.sprite = _defaultBtnSpr;
            _isEnabled = false;
            CursorManager.Instance.ChangeCursorState(ECursorState.Default);
            Spawner.Instance.ShowPlacementIcon(false);
            HideDetailsPanel();
        }

        private void ShowDetailsPanel()
        {
            _detailsPanel.SetActive(true);
            _optionName.text = _wallSettings.WallName;
            _optionDetails.text = _wallOption.OptionName;

            foreach (var cost in _wallOption.OptionResourceCosts)
            {
                var costDisplay = Instantiate(_costDisplayPrefab, _costsLayout);
                costDisplay.Init(cost);
                _displayedCosts.Add(costDisplay);
            }
        }

        private void HideDetailsPanel()
        {
            foreach (var displayedCost in _displayedCosts)
            {
                Destroy(displayedCost.gameObject);
            }
            _displayedCosts.Clear();
            
            _detailsPanel.SetActive(false);
        }

        protected override void TriggerOptionEffect()
        {
            CursorManager.Instance.ChangeCursorState(ECursorState.AreaSelect);
            Spawner.Instance.ShowPlacementIcon(true, _wallOption.OptionIcon, _wallSettings.InvalidPlacementTags);
            _isEnabled = true;
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
            
            PlanWall(mousePos, _wallOption);
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
                    SpawnWall(_wallOption, Helper.ConvertMousePosToGridPos(mousePos));
                }
            }
        }

        private void PlanWall(Vector2 mousePos, WallOption wallOption)
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
                    if (Helper.IsGridPosValidToBuild(gridPos, _wallSettings.InvalidPlacementTags))
                    {
                        placementColour = Librarian.Instance.GetColour("Placement Green");
                    }
                    else
                    {
                        placementColour = Librarian.Instance.GetColour("Placement Red");
                    }
                
                    tilePlan.Init(wallOption.ExteriorWallRules, tileMap, placementColour);
            
                    _plannedTiles.Add(tilePlan);
                }
            }
        }
    
        private void SpawnPlannedWall()
        {
            if (!_isPlanning) return;
            
            Spawner.Instance.ShowPlacementIcon(true, _wallOption.OptionIcon, _wallSettings.InvalidPlacementTags);
            ClearTilePlan();
            
            foreach (var gridPos in _plannedGrid)
            {
                if (Helper.IsGridPosValidToBuild(gridPos, _wallSettings.InvalidPlacementTags))
                {
                    SpawnWall(_wallOption, gridPos);
                }
            }
            
            _plannedGrid.Clear();
            _isPlanning = false;
        }
    
        public void SpawnWall(WallOption wallOption, Vector3 spawnPosition)
        {
            if (Helper.IsGridPosValidToBuild(spawnPosition, _wallSettings.InvalidPlacementTags))
            {
                spawnPosition = new Vector3(spawnPosition.x, spawnPosition.y, -1);
                var wall = Instantiate(_wallPrefab, spawnPosition, Quaternion.identity);
                wall.transform.SetParent(_structureParent);
                //wall.Init(wallOption);
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
