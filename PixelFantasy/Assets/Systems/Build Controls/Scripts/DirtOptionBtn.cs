using System.Collections.Generic;
using Controllers;
using Managers;
using ScriptableObjects;
using Systems.CursorHandler.Scripts;
using Systems.Floors.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace Systems.Build_Controls.Scripts
{
    public class DirtOptionBtn : OptionBtn
    {
        [SerializeField] private Image _btnBG;
        [SerializeField] private Sprite _defaultBtnSpr;
        [SerializeField] private Sprite _activeBtnSpr;
        [SerializeField] private Dirt _dirtPrefab;
        [SerializeField] private TileBase _dirtRuleTile;
        [SerializeField] private Sprite _dirtIcon;
        [SerializeField] protected GameObject _detailsPanel;
        [SerializeField] protected TextMeshProUGUI _optionName;
        
        private bool _isEnabled;
        private bool _isPlanning;
        private Vector2 _startPos;
        private List<Vector2> _plannedGrid = new List<Vector2>();
        private List<TilePlan> _plannedTiles = new List<TilePlan>();
        private Transform _floorParent;
        private int _optionIndex;

        private List<string> _invalidPlacementTags = new List<string>() { "Water", "Wall", "Floor", "Obstacle" };

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

        public void Init(SubCategoryBtn categoryBtn)
        {
            _ownerCategoryBtn = categoryBtn;
            _floorParent = Spawner.Instance.FlooringParent;
        }
        
        protected override void ToggledOn()
        {
            _btnBG.sprite = _activeBtnSpr;
            CursorManager.Instance.ChangeCursorState(ECursorState.AreaSelect);
            Spawner.Instance.ShowPlacementIcon(true, _dirtIcon, _invalidPlacementTags);
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
            _optionName.text = "Dirt Path";
        }

        private void HideDetailsPanel()
        {
            _detailsPanel.SetActive(false);
        }

        protected override void TriggerOptionEffect()
        {
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
            
            PlanDirt(mousePos);
        }

        protected void GameEvents_OnLeftClickUp(Vector3 mousePos, PlayerInputState inputState, bool isOverUI)
        {
            if (!_isEnabled) return;

            if (_isPlanning)
            {
                SpawnPlannedDirt();
            }
            else
            {
                if (!isOverUI)
                {
                    SpawnDirt(Helper.ConvertMousePosToGridPos(mousePos));
                }
            }
        }
        
        private void PlanDirt(Vector2 mousePos)
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
                    var tilePlanGO = new GameObject("Dirt Tile Plan", typeof(TilePlan));
                    tilePlanGO.transform.position = gridPos;

                    var tilePlan = tilePlanGO.GetComponent<TilePlan>();
                    var tileMap = TilemapController.Instance.GetTilemap(TilemapLayer.Dirt);
                    
                    Color placementColour;
                    if (Helper.IsGridPosValidToBuild(gridPos, _invalidPlacementTags))
                    {
                        placementColour = Librarian.Instance.GetColour("Placement Green");
                    }
                    else
                    {
                        placementColour = Librarian.Instance.GetColour("Placement Red");
                    }
                    
                    tilePlan.Init(_dirtRuleTile, tileMap, placementColour);

                    _plannedTiles.Add(tilePlan);
                }
            }
        }
        
        private void SpawnPlannedDirt()
        {
            if (!_isPlanning) return;
            
            Spawner.Instance.ShowPlacementIcon(true, _dirtIcon, _invalidPlacementTags);

            ClearTilePlan();
            
            foreach (var gridPos in _plannedGrid)
            {
                if (Helper.IsGridPosValidToBuild(gridPos, _invalidPlacementTags))
                {
                    SpawnDirt(gridPos);
                }
            }
            
            _plannedGrid.Clear();
            _isPlanning = false;
        }
        
        public void SpawnDirt(Vector3 spawnPosition)
        {
            if (Helper.IsGridPosValidToBuild(spawnPosition, _invalidPlacementTags))
            {
                spawnPosition = new Vector3(spawnPosition.x, spawnPosition.y, -1);
                var road = Instantiate(_dirtPrefab, spawnPosition, Quaternion.identity);
                road.transform.SetParent(_floorParent);
                road.Init();
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
