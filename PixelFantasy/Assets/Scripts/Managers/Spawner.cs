using System;
using System.Collections.Generic;
using Buildings;
using CodeMonkey.Utils;
using Controllers;
using Items;
using ScriptableObjects;
using UnityEngine;
using Zones;

namespace Managers
{
    public class Spawner : Singleton<Spawner>
    {
        [SerializeField] private Transform _itemsParent;
        [SerializeField] private GameObject _itemPrefab;
        
        [SerializeField] private Transform _resourceParent;
        [SerializeField] private GameObject _treePrefab;
        [SerializeField] private GameObject _plantPrefab;
    
        [SerializeField] private Transform _structureParent;
        [SerializeField] private GameObject _structurePrefab;
        [SerializeField] private GameObject _buildingPrefab;
        [SerializeField] private StructurePiece _wallPrefab;
        
        [SerializeField] private Transform _doorsParent;
        [SerializeField] private GameObject _doorPrefab;
        
        [SerializeField] private GameObject _dirtTilePrefab;
        [SerializeField] private GameObject _floorPrefab;
        [SerializeField] private Transform _flooringParent;

        [SerializeField] private GameObject _craftingTablePrefab;
        [SerializeField] private GameObject _furniturePrefab;
        [SerializeField] private Transform _furnitureParent;

        [SerializeField] private GameObject _soilPrefab;
        
        [SerializeField] private SpriteRenderer _placementIcon;
        [SerializeField] private Sprite _genericPlacementSprite;
        
        [SerializeField] private GameObject _settlementFlag;

        [SerializeField] private Transform _storageParent;
        [SerializeField] private GameObject _storageContainerPrefab;

        [SerializeField] private Transform _roofParent;
        [SerializeField] private Roof _roofPrefab;

        private bool _showPlacement;
        private List<string> _invalidPlacementTags = new List<string>();
        private Color? _colourOverride;
        
        // Structure
        private bool _planningStructure;
        private Vector2 _startPos;
        private List<GameObject> _blueprints = new List<GameObject>();
        private List<Vector2> _plannedGrid = new List<Vector2>();
        
        public PlacementDirection PlacementDirection;
        public StructureData StructureData { get; set; }
        public WallData WallData { get; set; }
        public BuildingData BuildingData { get; set; }
        public DoorData DoorData { get; set; }
        public FloorData FloorData { get; set; }
        public CropData CropData { get; set; }
        public RoofData RoofData { get; set; }

        public Transform ItemsParent => _itemsParent;
        public Transform BuildingsParent => _structureParent;

        public PlacementDirection SetNextPlacementDirection(bool isClockwise)
        {
            if (isClockwise)
            {
                PlacementDirection = Helper.GetNextDirection(PlacementDirection);
            }
            else
            {
                PlacementDirection = Helper.GetPrevDirection(PlacementDirection);
            }
            
            return PlacementDirection;
        }
        
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
            if (inputState == PlayerInputState.BuildStructure 
                || inputState == PlayerInputState.BuildFlooring
                || inputState == PlayerInputState.BuildFarm 
                || inputState == PlayerInputState.BuildWall
                || inputState == PlayerInputState.BuildRoof)
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

            if (inputState == PlayerInputState.BuildWall)
            {
                PlanWall(mousePos);
            }

            if (inputState == PlayerInputState.BuildRoof)
            {
                PlanRoof(mousePos);
            }
            
            if (inputState == PlayerInputState.BuildFlooring)
            {
                if (PlayerInputController.Instance.StoredKey == "Dirt")
                {
                    PlanDirt(mousePos);
                }
                else
                {
                    PlanFloor(mousePos, FloorData.PlanningMode);
                }
            }

            if (inputState == PlayerInputState.BuildFarm)
            {
                PlanFarm(mousePos);
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
            else if (inputState == PlayerInputState.BuildWall && _planningStructure)
            {
                SpawnPlannedWall();
            }
            else if (inputState == PlayerInputState.BuildWall)
            {
                var wallData = Librarian.Instance.GetWallData(PlayerInputController.Instance.StoredKey);
                SpawnWall(wallData, Helper.ConvertMousePosToGridPos(mousePos));
            }
            else if (inputState == PlayerInputState.BuildRoof && _planningStructure)
            {
                SpawnPlannedRoof();
            }
            else if (inputState == PlayerInputState.BuildRoof)
            {
                var roofData = Librarian.Instance.GetRoofData(PlayerInputController.Instance.StoredKey);
                SpawnRoof(roofData, Helper.ConvertMousePosToGridPos(mousePos));
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
                else
                {
                    if (_planningStructure)
                    {
                        SpawnPlannedFloor();
                    }
                    else
                    {
                        var floorData = Librarian.Instance.GetFloorData(PlayerInputController.Instance.StoredKey);
                        SpawnFloor(floorData, Helper.ConvertMousePosToGridPos(mousePos));
                    }
                }
            }
            else if (inputState == PlayerInputState.BuildFurniture && _plannedFurniture != null)
            {
                if (_plannedFurniture.CheckPlacement())
                {
                    //PlayerInputController.Instance.ChangeState(PlayerInputState.None);
                    _plannedFurniture.PrepareToBuild();
                    var key = _plannedFurniture.FurnitureItemData.ItemName;
                    _plannedFurniture = null;
                    
                    // Allows the player to make multiple
                    var furnitureData = Librarian.Instance.GetItemData(key) as FurnitureItemData;
                    PlanFurniture(furnitureData);
                }
            }
            else if (inputState == PlayerInputState.BuildFarm)
            {
                if (_planningStructure)
                {
                    SpawnPlannedFarm();
                }
                else
                {
                    SpawnSoilTile(Helper.ConvertMousePosToGridPos(mousePos), CropData, null);
                }
            }
            else if (inputState == PlayerInputState.BuildDoor)
            {
                SpawnDoor(DoorData, Helper.ConvertMousePosToGridPos(mousePos));
            }
            else if (inputState == PlayerInputState.BuildBuilding && _plannedBuilding != null)
            {
                if (_plannedBuilding.CheckPlacement())
                {
                    var plannedBuilding = _plannedBuilding;
                    _plannedBuilding = null;
                    PlayerInputController.Instance.ChangeState(PlayerInputState.None);
                    plannedBuilding.SetState(Building.BuildingState.Construction);
                    plannedBuilding.TriggerPlaced();
                }
            }
        }
        
        protected virtual void GameEvents_OnRightClickDown(Vector3 mousePos, PlayerInputState inputState, bool isOverUI) 
        {
            
        }
        
        protected virtual void GameEvents_OnRightClickHeld(Vector3 mousePos, PlayerInputState inputState, bool isOverUI) 
        {
            
        }
        
        protected virtual void GameEvents_OnRightClickUp(Vector3 mousePos, PlayerInputState inputState, bool isOverUI) 
        {
            CancelInput();
        }

        public void CancelInput()
        {
            PlayerInputController.Instance.ChangeState(PlayerInputState.None);
            ShowPlacementIcon(false);
            _invalidPlacementTags.Clear();
            CancelPlanning();
            PlacementDirection = PlacementDirection.Down;
            _plannedFurnitureItemData = null;
            _plannedBuilding = null;
        }

        public void ShowPlacementIcon(bool show, Sprite icon = null, List<String> invalidPlacementTags = null, float sizeOverride = 1f, Color? colourOverride = null)
        {
            _placementIcon.enabled = true;
            _colourOverride = colourOverride;

            if (icon == null)
            {
                icon = _genericPlacementSprite;
            }

            List<string> tags = null;
            if (invalidPlacementTags != null)
            {
                tags = new List<string>(invalidPlacementTags);
            }
            
            if (_placementIcon.transform.childCount > 0)
            {
                var child = _placementIcon.transform.GetChild(0).gameObject;
                Destroy(child);
            }
            
            if (show)
            {
                _placementIcon.gameObject.transform.localScale = new Vector2(sizeOverride, sizeOverride);
                _placementIcon.sprite = icon;
                _placementIcon.gameObject.SetActive(true);
                _showPlacement = true;
                _invalidPlacementTags = tags;
            }
            else
            {   _placementIcon.gameObject.SetActive(false);
                _showPlacement = false;
            }
        }
        
        public void ShowPlacementObject(bool show, GameObject icon = null, List<String> invalidPlacementTags = null, float sizeOverride = 1f)
        {
            _placementIcon.enabled = false;
            
            if (_placementIcon.transform.childCount > 0)
            {
                var child = _placementIcon.transform.GetChild(0).gameObject;
                Destroy(child);
            }

            if (show)
            {
                var placementObj = Instantiate(icon,_placementIcon.transform);
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
                    Color placementColour = (Color)(_colourOverride != null ? _colourOverride : Librarian.Instance.GetColour("Placement Green"));
                    _placementIcon.color = placementColour;
                    if (_placementIcon.transform.childCount > 0)
                    {
                        var renderers = _placementIcon.GetComponentsInChildren<SpriteRenderer>();
                        foreach (var renderer in renderers)
                        {
                            renderer.color = placementColour;
                        }
                    }
                }
                else
                {
                    _placementIcon.color = Librarian.Instance.GetColour("Placement Red");
                    if (_placementIcon.transform.childCount > 0)
                    {
                        var renderers = _placementIcon.GetComponentsInChildren<SpriteRenderer>();
                        foreach (var renderer in renderers)
                        {
                            renderer.color = Librarian.Instance.GetColour("Placement Red");
                        }
                    }
                }
            }

            if (_plannedFurnitureItemData != null)
            {
                if (Input.GetKeyDown(KeyCode.E)) // Clockwise
                {
                    SetNextPlacementDirection(true);
                    if (_plannedFurniture != null)
                    {
                        Destroy(_plannedFurniture.gameObject);
                        _plannedFurniture = null;
                    }

                    PlanFurniture(_plannedFurnitureItemData);
                }
                
                if (Input.GetKeyDown(KeyCode.Q)) // Counter Clockwise
                {
                    SetNextPlacementDirection(false);
                    if (_plannedFurniture != null)
                    {
                        Destroy(_plannedFurniture.gameObject);
                        _plannedFurniture = null;
                    }

                    PlanFurniture(_plannedFurnitureItemData);
                }
            }
        }

        public void SpawnItem(ItemData itemData, Vector3 spawnPosition, bool canBeHauled, int quantity)
        {
            for (int i = 0; i < quantity; i++)
            {
                spawnPosition = new Vector3(spawnPosition.x, spawnPosition.y, -1);
                var item = Instantiate(_itemPrefab, spawnPosition, Quaternion.identity);
                item.transform.SetParent(_itemsParent);
                var itemScript = item.GetComponent<Item>();
                itemScript.InitializeItem(itemData, canBeHauled);
            }
        }
        
        public Item SpawnItem(ItemData itemData, Vector3 spawnPosition, bool canBeHauled, ItemState itemState = null, bool populateInteractions = true)
        {
            spawnPosition = new Vector3(spawnPosition.x, spawnPosition.y, -1);
            var item = Instantiate(_itemPrefab, spawnPosition, Quaternion.identity);
            item.transform.SetParent(_itemsParent);
            var itemScript = item.GetComponent<Item>();
            itemScript.InitializeItem(itemData, canBeHauled, itemState, populateInteractions);
            
            item.SetActive(true);
            return itemScript;
        }

        public void SpawnStructure(StructureData structureData, Vector3 spawnPosition)
        {
            if (Helper.IsGridPosValidToBuild(spawnPosition, structureData.InvalidPlacementTags))
            {
                spawnPosition = new Vector3(spawnPosition.x, spawnPosition.y, -1);
                var structureObj = Instantiate(_structurePrefab, spawnPosition, Quaternion.identity);
                structureObj.transform.SetParent(_structureParent);
                var structure = structureObj.GetComponent<Structure>();
                structure.Init(structureData);
            }
        }

        private void SpawnRoof(RoofData roofData, Vector3 spawnPosition)
        {
            if (Helper.IsGridPosValidToBuild(spawnPosition, roofData.InvalidPlacementTags))
            {
                spawnPosition = new Vector3(spawnPosition.x, spawnPosition.y, -1);
                var roof = Instantiate(_roofPrefab, spawnPosition, Quaternion.identity);
                roof.transform.SetParent(_roofParent);
                roof.Init(roofData);
            }
        }

        public void SpawnWall(WallData wallData, Vector3 spawnPosition)
        {
            if (Helper.IsGridPosValidToBuild(spawnPosition, wallData.InvalidPlacementTags))
            {
                spawnPosition = new Vector3(spawnPosition.x, spawnPosition.y, -1);
                var wall = Instantiate(_wallPrefab, spawnPosition, Quaternion.identity);
                wall.transform.SetParent(_structureParent);
                wall.Init(wallData);
            }
        }

        public Storage SpawnStorageContainer(StorageItemData storageData, Vector3 spawnPosition)
        {
            var containerObj = Instantiate(_storageContainerPrefab, spawnPosition, Quaternion.identity);
            containerObj.transform.SetParent(_storageParent);
            var container = containerObj.GetComponent<Storage>();
            container.Init(storageData);
            return container;
        }
        
        public void SpawnDoor(DoorData doorData, Vector3 spawnPosition)
        {
            if (Helper.IsGridPosValidToBuild(spawnPosition, doorData.InvalidPlacementTags))
            {
                spawnPosition = new Vector3(spawnPosition.x, spawnPosition.y, -1);
                var doorObj = Instantiate(_doorPrefab, spawnPosition, Quaternion.identity);
                doorObj.transform.SetParent(_doorsParent);
                var door = doorObj.GetComponent<Door>();
                door.Init(doorData);
            }
        }
        
        private Building _plannedBuilding;
        public void PlanBuilding(Building building, Action onBuildingPlaced = null)
        {
            _plannedBuilding = Instantiate(building, _structureParent);
            _plannedBuilding.SetState(Building.BuildingState.Planning);
            _plannedBuilding.OnBuildingPlaced = onBuildingPlaced;
        }

        private Furniture _plannedFurniture;
        private FurnitureItemData _plannedFurnitureItemData;
        public void PlanFurniture(FurnitureItemData furnitureData)
        {
            _plannedFurnitureItemData = furnitureData;
            var cursorPos = Helper.ConvertMousePosToGridPos(UtilsClass.GetMouseWorldPosition());
            var furniture = Instantiate(furnitureData.GetFurniturePrefab(PlacementDirection), cursorPos, Quaternion.identity, _furnitureParent);
            furniture.Plan(furnitureData);
            _plannedFurniture = furniture;
        }

        public void SpawnFloor(FloorData floorData, Vector3 spawnPosition)
        {
            if (Helper.IsGridPosValidToBuild(spawnPosition, floorData.InvalidPlacementTags))
            {
                spawnPosition = new Vector3(spawnPosition.x, spawnPosition.y, -1);
                var floorObj = Instantiate(_floorPrefab, spawnPosition, Quaternion.identity);
                floorObj.transform.SetParent(_flooringParent);
                var floor = floorObj.GetComponent<Floor>();
                floor.Init(floorData);
            }
        }

        public void SpawnTree(Vector3 spawnPosition, GrowingResourceData growingResourceData)
        {
            spawnPosition = new Vector3(spawnPosition.x, spawnPosition.y, -1);
            var tree = Instantiate(_treePrefab, spawnPosition, Quaternion.identity);
            tree.transform.SetParent(_resourceParent);
            tree.GetComponent<GrowingResource>().Init(growingResourceData, _treePrefab);
        }

        public void SpawnPlant(Vector3 spawnPosition, GrowingResourceData growingResourceData)
        {
            spawnPosition = new Vector3(spawnPosition.x, spawnPosition.y, -1);
            var plant = Instantiate(_plantPrefab, spawnPosition, Quaternion.identity);
            plant.transform.SetParent(_resourceParent);
            plant.GetComponent<GrowingResource>().Init(growingResourceData, _plantPrefab);
        }

        public void SpawnDirtTile(Vector3 spawnPosition, Structure requestingStructure = null)
        {
            spawnPosition = new Vector3(spawnPosition.x, spawnPosition.y, -1);
            var dirt = Instantiate(_dirtTilePrefab, spawnPosition, Quaternion.identity);
            dirt.transform.SetParent(_flooringParent);
            dirt.GetComponent<DirtTile>().Init(requestingStructure);
        }
        
        public DirtTile SpawnDirtTile(Vector3 spawnPosition, Floor requestingFloor)
        {
            spawnPosition = new Vector3(spawnPosition.x, spawnPosition.y, -1);
            var dirt = Instantiate(_dirtTilePrefab, spawnPosition, Quaternion.identity);
            dirt.transform.SetParent(_flooringParent);
            var dirtTile = dirt.GetComponent<DirtTile>();
            dirtTile.Init(requestingFloor);
            return dirtTile;
        }

        #region Structure

        private void CancelPlanning()
        {
            PlayerInputController.Instance.ChangeState(PlayerInputState.None);
            ClearPlannedBlueprint();
            _plannedGrid.Clear();

            if (_plannedBuilding != null)
            {
                Destroy(_plannedBuilding.gameObject);
                _plannedBuilding = null;
            }

            if (_plannedFurniture != null)
            {
                Destroy(_plannedFurniture.gameObject);
                _plannedFurniture = null;
            }
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

        private void SpawnPlannedRoof()
        {
            if (!_planningStructure) return;

            foreach (var gridPos in _plannedGrid)
            {
                if (Helper.IsGridPosValidToBuild(gridPos, RoofData.InvalidPlacementTags))
                {
                    SpawnRoof(RoofData, gridPos);
                }
            }

            ClearPlannedBlueprint();
            _plannedGrid.Clear();
            _planningStructure = false;
        }

        private void SpawnPlannedWall()
        {
            if (!_planningStructure) return;

            foreach (var gridPos in _plannedGrid)
            {
                if (Helper.IsGridPosValidToBuild(gridPos, WallData.InvalidPlacementTags))
                {
                    SpawnWall(WallData, gridPos);
                }
            }

            ClearPlannedBlueprint();
            _plannedGrid.Clear();
            _planningStructure = false;
        }

        private void SpawnPlannedFloor()
        {
            if (!_planningStructure) return;

            foreach (var gridPos in _plannedGrid)
            {
                if (Helper.IsGridPosValidToBuild(gridPos, FloorData.InvalidPlacementTags))
                {
                    SpawnFloor(FloorData, gridPos);
                }
            }

            ClearPlannedBlueprint();
            _plannedGrid.Clear();
            _planningStructure = false;
        }

        private void PlanRoof(Vector2 mousePos)
        {
            if (!_planningStructure) return;

            List<Vector2> gridPositions = new List<Vector2>();
            gridPositions = Helper.GetRectangleGridPositionsBetweenPoints(_startPos, mousePos);

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
                    spriteRenderer.sprite = _genericPlacementSprite;
                    if (Helper.IsGridPosValidToBuild(gridPos, RoofData.InvalidPlacementTags))
                    {
                        spriteRenderer.color = Librarian.Instance.GetColour("Placement Green");
                    }
                    else
                    {
                        spriteRenderer.color = Librarian.Instance.GetColour("Placement Red");
                    }
                    
                    spriteRenderer.sortingLayerName = "Roofing";
                    _blueprints.Add(blueprint);
                }
            }
        }

        private void PlanWall(Vector2 mousePos)
        {
            if (!_planningStructure) return;

            List<Vector2> gridPositions = new List<Vector2>();
            gridPositions = Helper.GetBoxPositionsBetweenPoints(_startPos, mousePos);

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
                    spriteRenderer.sprite = _genericPlacementSprite;
                    if (Helper.IsGridPosValidToBuild(gridPos, WallData.InvalidPlacementTags))
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

        private void PlanStructure(Vector2 mousePos, PlanningMode planningMode)
        {
            if (!_planningStructure) return;

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

        private void PlanFloor(Vector2 mousePos, PlanningMode planningMode)
        {
            if (!_planningStructure) return;

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
                    spriteRenderer.sprite = FloorData.Icon;
                    if (Helper.IsGridPosValidToBuild(gridPos, FloorData.InvalidPlacementTags))
                    {
                        spriteRenderer.color = Librarian.Instance.GetColour("Placement Green");
                    }
                    else
                    {
                        spriteRenderer.color = Librarian.Instance.GetColour("Placement Red");
                    }
                    
                    spriteRenderer.sortingLayerName = "Floor";
                    _blueprints.Add(blueprint);
                }
            }
            
        }

        #endregion

        private void PlanDirt(Vector2 mousePos)
        {
            if (!_planningStructure) return;

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
                    spriteRenderer.sprite = dirt.PlacementIcon;
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

        private void PlanFarm(Vector2 mousePos)
        {
            if (!_planningStructure) return;

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
                    var soil = _soilPrefab.GetComponent<Crop>();
                    spriteRenderer.sprite = _genericPlacementSprite;
                    if (Helper.IsGridPosValidToBuild(gridPos, soil.InvalidPlacementTags))
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

        private void SpawnPlannedFarm()
        {
            if (!_planningStructure) return;

            foreach (var gridPos in _plannedGrid)
            {
                var soil = _soilPrefab.GetComponent<Crop>();
                if (Helper.IsGridPosValidToBuild(gridPos, soil.InvalidPlacementTags))
                {
                    SpawnSoilTile(gridPos, CropData, null);
                }
            }

            ClearPlannedBlueprint();
            _plannedGrid.Clear();
            _planningStructure = false;
            CancelInput();
        }

        public void SpawnSoilTile(Vector2 spawnPos, CropData cropData, Family owner)
        {
            var soil = Instantiate(_soilPrefab, spawnPos, Quaternion.identity);
            soil.transform.SetParent(_flooringParent);
            soil.GetComponent<Crop>().Init(cropData, owner);
        }
    }

    public enum PlanningMode
    {
        Rectangle,
        Line,
        Single,
    }
    
    [Serializable]
    public enum PlacementDirection
    {
        Down,
        Up,
        Left, 
        Right
    }
}
