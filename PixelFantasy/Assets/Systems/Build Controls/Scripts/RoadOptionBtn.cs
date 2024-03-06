using System.Collections.Generic;
using Controllers;
using Managers;
using ScriptableObjects;
using Systems.CursorHandler.Scripts;
using Systems.Roads.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Build_Controls.Scripts
{
    public class RoadOptionBtn : OptionBtn
    {
        [SerializeField] private Image _btnBG;
        [SerializeField] private Sprite _defaultBtnSpr;
        [SerializeField] private Sprite _activeBtnSpr;
        [SerializeField] private Road _roadPrefab;
        
        [SerializeField] protected Transform _costsLayout;
        [SerializeField] protected BuildControlCostDisplay _costDisplayPrefab;
        [SerializeField] protected GameObject _detailsPanel;
        [SerializeField] protected TextMeshProUGUI _optionName;
        [SerializeField] protected TextMeshProUGUI _optionDetails;
        
        private List<BuildControlCostDisplay> _displayedCosts = new List<BuildControlCostDisplay>();

        private RoadOption _roadOption;
        private RoadSettings _roadSettings;
        private bool _isEnabled;
        private bool _isPlanning;
        private Vector2 _startPos;
        
        private List<Vector2> _plannedGrid = new List<Vector2>();
        private List<TilePlan> _plannedTiles = new List<TilePlan>();
        private Transform _floorParent;
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

        public void Init(RoadOption roadOption, RoadSettings roadSettings, SubCategoryBtn categoryBtn)
        {
            _roadOption = roadOption;
            _roadSettings = roadSettings;
            _ownerCategoryBtn = categoryBtn;
            _floorParent = Spawner.Instance.FlooringParent;

            _icon.sprite = _roadOption.OptionIcon;
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
            _optionName.text = _roadSettings.RoadName;
            _optionDetails.text = _roadOption.OptionName;

            foreach (var cost in _roadOption.OptionResourceCosts)
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
            Spawner.Instance.ShowPlacementIcon(true, _roadOption.OptionIcon, _roadSettings.InvalidPlacementTags);
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
            
            PlanFloor(mousePos, _roadOption);
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
                    SpawnFloor(_roadOption, Helper.ConvertMousePosToGridPos(mousePos));
                }
            }
        }
        
        private void PlanFloor(Vector2 mousePos, RoadOption roadOption)
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
                    if (Helper.IsGridPosValidToBuild(gridPos, _roadSettings.InvalidPlacementTags))
                    {
                        placementColour = Librarian.Instance.GetColour("Placement Green");
                    }
                    else
                    {
                        placementColour = Librarian.Instance.GetColour("Placement Red");
                    }
                    
                    tilePlan.Init(roadOption.RoadRuleTile, tileMap, placementColour);

                    _plannedTiles.Add(tilePlan);
                }
            }
        }
        
        private void SpawnPlannedFloor()
        {
            if (!_isPlanning) return;
            
            Spawner.Instance.ShowPlacementIcon(true, _roadOption.OptionIcon, _roadSettings.InvalidPlacementTags);
            ClearTilePlan();

            foreach (var gridPos in _plannedGrid)
            {
                if (Helper.IsGridPosValidToBuild(gridPos, _roadSettings.InvalidPlacementTags))
                {
                    SpawnFloor(_roadOption, gridPos);
                }
            }

            _plannedGrid.Clear();
            _isPlanning = false;
        }
        
        public void SpawnFloor(RoadOption roadOption, Vector3 spawnPosition)
        {
            if (Helper.IsGridPosValidToBuild(spawnPosition, _roadSettings.InvalidPlacementTags))
            {
                spawnPosition = new Vector3(spawnPosition.x, spawnPosition.y, -1);
                var road = Instantiate(_roadPrefab, spawnPosition, Quaternion.identity);
                road.transform.SetParent(_floorParent);
                road.Init(roadOption);
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
