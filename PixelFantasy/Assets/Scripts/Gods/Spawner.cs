using System;
using System.Collections.Generic;
using CodeMonkey.Utils;
using Controllers;
using HUD;
using Items;
using ScriptableObjects;
using UnityEngine;

namespace Gods
{
    public class Spawner : God<Spawner>
    {
        [SerializeField] private Transform _itemsParent;
        [SerializeField] private GameObject _itemPrefab;
        
        [SerializeField] private Transform _resourceParent;
        [SerializeField] private GameObject _treePrefab;
        [SerializeField] private GameObject _plantPrefab;
    
        [SerializeField] private Transform _structureParent;
        [SerializeField] private GameObject _structurePrefab;
        
        [SerializeField] private GameObject _dirtTilePrefab;
        [SerializeField] private Transform _flooringParent;
        
        [SerializeField] private SpriteRenderer _placementIcon;

        private bool _showPlacement;
        private List<string> _invalidPlacementTags = new List<string>();
        
        // Structure
        private bool _planningStructure;
        private Vector2 _startPos;
        private List<GameObject> _blueprints = new List<GameObject>();
        private List<Vector2> _plannedGrid = new List<Vector2>();
        
        public StructureData StructureData { get; set; }

        private void OnEnable()
        {
            GameEvents.OnLeftClickDown += GameEvents_OnLeftClickDown;
            GameEvents.OnLeftClickHeld += GameEvents_OnLeftClickHeld;
            GameEvents.OnLeftClickUp += GameEvents_OnLeftClickUp;
            GameEvents.OnRightClickDown += GameEvents_OnRightClickDown;
            GameEvents.OnRightClickHeld += GameEvents_OnRightClickHeld;
            GameEvents.OnRightClickUp += GameEvents_OnRightClickUp;
        }

        private void OnDisable()
        {
            GameEvents.OnLeftClickDown -= GameEvents_OnLeftClickDown;
            GameEvents.OnLeftClickHeld -= GameEvents_OnLeftClickHeld;
            GameEvents.OnLeftClickUp -= GameEvents_OnLeftClickUp;
            GameEvents.OnRightClickDown -= GameEvents_OnRightClickDown;
            GameEvents.OnRightClickHeld -= GameEvents_OnRightClickHeld;
            GameEvents.OnRightClickUp -= GameEvents_OnRightClickUp;
        }

        protected virtual void GameEvents_OnLeftClickDown(Vector3 mousePos, PlayerInputState inputState, bool isOverUI) 
        {
            if (isOverUI) return;
            if (inputState == PlayerInputState.BuildStructure || inputState == PlayerInputState.BuildFlooring)
            {
                _planningStructure = true;
                _startPos = Helper.ConvertMousePosToGridPos(mousePos);
            }
        }
        
        protected virtual void GameEvents_OnLeftClickHeld(Vector3 mousePos, PlayerInputState inputState, bool isOverUI) 
        {
            if (inputState == PlayerInputState.BuildStructure)
            {
                PlanStructure(mousePos, StructureData.PlanningMode);
            }

            if (inputState == PlayerInputState.BuildFlooring)
            {
                if (PlayerInputController.Instance.StoredKey == "Dirt")
                {
                    PlanDirt(mousePos);
                }
            }
        }
        
        protected virtual void GameEvents_OnLeftClickUp(Vector3 mousePos, PlayerInputState inputState, bool isOverUI)
        {
            if (isOverUI) return;
            
            if (inputState == PlayerInputState.BuildStructure && _planningStructure)
            {
                SpawnPlannedStructure();
            }
            else if (inputState == PlayerInputState.BuildStructure)
            {
                var structureData = Librarian.Instance.GetStructureData(PlayerInputController.Instance.StoredKey);
                SpawnStructure(structureData, Helper.ConvertMousePosToGridPos(mousePos));
                
            }
            else if (inputState == PlayerInputState.BuildFlooring)
            {
                if (PlayerInputController.Instance.StoredKey == "Dirt")
                {
                    if (_planningStructure)
                    {
                        SpawnPlannedDirt();
                    }
                    else
                    {
                        SpawnDirtTile(Helper.ConvertMousePosToGridPos(mousePos));
                    }
                }
            }
        }
        
        protected virtual void GameEvents_OnRightClickDown(Vector3 mousePos, PlayerInputState inputState, bool isOverUI) 
        {
            CancelInput();
        }
        
        protected virtual void GameEvents_OnRightClickHeld(Vector3 mousePos, PlayerInputState inputState, bool isOverUI) 
        {
            
        }
        
        protected virtual void GameEvents_OnRightClickUp(Vector3 mousePos, PlayerInputState inputState, bool isOverUI) 
        {
            
        }

        private void CancelInput()
        {
            PlayerInputController.Instance.ChangeState(PlayerInputState.None);
            ShowPlacementIcon(false);
            _invalidPlacementTags.Clear();
            CancelPlanning();
        }

        public void ShowPlacementIcon(bool show, Sprite icon = null, List<String> invalidPlacementTags = null)
        {
            if (show)
            {
                _placementIcon.sprite = icon;
                _placementIcon.gameObject.SetActive(true);
                _showPlacement = true;
                _invalidPlacementTags = invalidPlacementTags;
            }
            else
            {   _placementIcon.gameObject.SetActive(false);
                _showPlacement = false;
            }
        }

        private void Update()
        {
            if (_showPlacement)
            {
                var gridPos = Helper.ConvertMousePosToGridPos(UtilsClass.GetMouseWorldPosition());
                _placementIcon.transform.position = gridPos;
                if (Helper.IsGridPosValidToBuild(gridPos, _invalidPlacementTags))
                {
                    _placementIcon.color = Librarian.Instance.GetColour("Placement Green");
                }
                else
                {
                    _placementIcon.color = Librarian.Instance.GetColour("Placement Red");
                }
            }
        }

        public void SpawnItem(ItemData itemData, Vector2 spawnPosition, bool canBeHauled)
        {
            var item = Instantiate(_itemPrefab, spawnPosition, Quaternion.identity);
            item.transform.SetParent(_itemsParent);
            var itemScript = item.GetComponent<Item>();
            itemScript.InitializeItem(itemData, canBeHauled);
        }

        public void SpawnStructure(StructureData structureData, Vector2 spawnPosition)
        {
            if (Helper.IsGridPosValidToBuild(spawnPosition, structureData.InvalidPlacementTags))
            {
                var structureObj = Instantiate(_structurePrefab, spawnPosition, Quaternion.identity);
                structureObj.transform.SetParent(_structureParent);
                var structure = structureObj.GetComponent<Structure>();
                structure.Init(structureData);
            }
        }

        public void SpawnTree(Vector2 spawnPosition, GrowingResourceData growingResourceData)
        {
            var tree = Instantiate(_treePrefab, spawnPosition, Quaternion.identity);
            tree.transform.SetParent(_resourceParent);
            tree.GetComponent<GrowingResource>().Init(growingResourceData);
        }

        public void SpawnPlant(Vector2 spawnPosition, GrowingResourceData growingResourceData)
        {
            var plant = Instantiate(_plantPrefab, spawnPosition, Quaternion.identity);
            plant.transform.SetParent(_resourceParent);
            plant.GetComponent<GrowingResource>().Init(growingResourceData);
        }

        public void SpawnDirtTile(Vector2 spawnPosition, Structure requestingStructure = null)
        {
            var dirt = Instantiate(_dirtTilePrefab, spawnPosition, Quaternion.identity);
            dirt.transform.SetParent(_flooringParent);
            dirt.GetComponent<DirtTile>().Init(requestingStructure);
        }

        #region Structure

        private void CancelPlanning()
        {
            PlayerInputController.Instance.ChangeState(PlayerInputState.None);
            ClearPlannedBlueprint();
            _plannedGrid.Clear();
        }
        
        private void ClearPlannedBlueprint()
        {
            foreach (var blueprint in _blueprints)
            {
                Destroy(blueprint);
            }
            _blueprints.Clear();
        }
        
        private void SpawnPlannedStructure()
        {
            if (!_planningStructure) return;

            foreach (var gridPos in _plannedGrid)
            {
                if (Helper.IsGridPosValidToBuild(gridPos, StructureData.InvalidPlacementTags))
                {
                    SpawnStructure(StructureData, gridPos);
                }
            }

            ClearPlannedBlueprint();
            _plannedGrid.Clear();
            _planningStructure = false;
            CancelInput();
        }
        
        private void PlanStructure(Vector2 mousePos, PlanningMode planningMode)
        {
            if (!_planningStructure) return;
            ShowPlacementIcon(false);

            Vector3 curGridPos = Helper.ConvertMousePosToGridPos(mousePos);
            List<Vector2> gridPositions = new List<Vector2>();

            switch (planningMode)
            {
                case PlanningMode.Rectangle:
                    gridPositions = Helper.GetRectangleGridPositionsBetweenPoints(_startPos, curGridPos);
                    break;
                case PlanningMode.Line:
                    gridPositions = Helper.GetLineGridPositionsBetweenPoints(_startPos, mousePos);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(planningMode), planningMode, null);
            }

            if (gridPositions.Count != _plannedGrid.Count)
            {
                _plannedGrid = gridPositions;
                
                // Clear previous display, then display new blueprints
                ClearPlannedBlueprint();

                foreach (var gridPos in gridPositions)
                {
                    var blueprint = new GameObject("blueprint", typeof(SpriteRenderer));
                    blueprint.transform.position = gridPos;
                    var spriteRenderer = blueprint.GetComponent<SpriteRenderer>();
                    spriteRenderer.sprite = StructureData.Icon;
                    if (Helper.IsGridPosValidToBuild(gridPos, StructureData.InvalidPlacementTags))
                    {
                        spriteRenderer.color = Librarian.Instance.GetColour("Placement Green");
                    }
                    else
                    {
                        spriteRenderer.color = Librarian.Instance.GetColour("Placement Red");
                    }
                    
                    spriteRenderer.sortingLayerName = "Structure";
                    _blueprints.Add(blueprint);
                }
            }
            
        }

        #endregion

        private void PlanDirt(Vector2 mousePos)
        {
            if (!_planningStructure) return;
            ShowPlacementIcon(false);

            Vector3 curGridPos = Helper.ConvertMousePosToGridPos(mousePos);
            List<Vector2> gridPositions = new List<Vector2>();
            gridPositions = Helper.GetRectangleGridPositionsBetweenPoints(_startPos, curGridPos);

            if (gridPositions.Count != _plannedGrid.Count)
            {
                _plannedGrid = gridPositions;
                
                // Clear previous display, then display new blueprints
                ClearPlannedBlueprint();

                foreach (var gridPos in gridPositions)
                {
                    var blueprint = new GameObject("blueprint", typeof(SpriteRenderer));
                    blueprint.transform.position = gridPos;
                    var spriteRenderer = blueprint.GetComponent<SpriteRenderer>();
                    var dirt = _dirtTilePrefab.GetComponent<DirtTile>();
                    spriteRenderer.sprite = dirt.Icon;
                    if (Helper.IsGridPosValidToBuild(gridPos, dirt.InvalidPlacementTags))
                    {
                        spriteRenderer.color = Librarian.Instance.GetColour("Placement Green");
                    }
                    else
                    {
                        spriteRenderer.color = Librarian.Instance.GetColour("Placement Red");
                    }
                    
                    spriteRenderer.sortingLayerName = "DirtTile";
                    _blueprints.Add(blueprint);
                }
            }
        }

        private void SpawnPlannedDirt()
        {
            if (!_planningStructure) return;

            foreach (var gridPos in _plannedGrid)
            {
                var dirt = _dirtTilePrefab.GetComponent<DirtTile>();
                if (Helper.IsGridPosValidToBuild(gridPos, dirt.InvalidPlacementTags))
                {
                    SpawnDirtTile(gridPos);
                }
            }

            ClearPlannedBlueprint();
            _plannedGrid.Clear();
            _planningStructure = false;
            CancelInput();
        }
    }

    public enum PlanningMode
    {
        Rectangle,
        Line
    }
}
