using System;
using System.Collections.Generic;
using Buildings;
using CodeMonkey.Utils;
using Controllers;
using Items;
using ScriptableObjects;
using Systems.Notifications.Scripts;
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
        
        [SerializeField] private Transform _flooringParent;

        [SerializeField] private GameObject _craftingTablePrefab;
        [SerializeField] private GameObject _furniturePrefab;
        [SerializeField] private Transform _furnitureParent;

        [SerializeField] private GameObject _soilPrefab;
        
        [SerializeField] private SpriteRenderer _placementIcon;
        [SerializeField] private Sprite _genericPlacementSprite;
        
        [SerializeField] private Transform _storageParent;
        [SerializeField] private GameObject _storageContainerPrefab;

        private bool _showPlacement;
        private List<string> _invalidPlacementTags = new List<string>();
        private Color? _colourOverride;
        
        // Structure
        private bool _planningStructure;
        private Vector2 _startPos;
        private List<GameObject> _blueprints = new List<GameObject>();
        private List<Vector2> _plannedGrid = new List<Vector2>();
        
        public PlacementDirection PlacementDirection;
        public CropData CropData { get; set; }

        public Transform ItemsParent => _itemsParent;
        public Transform BuildingsParent => _structureParent;
        public Transform FlooringParent => _flooringParent;

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
            if (inputState == PlayerInputState.BuildFlooring
                || inputState == PlayerInputState.BuildFarm)
            {
                _planningStructure = true;
                _startPos = Helper.ConvertMousePosToGridPos(mousePos);
            }
        }
        
        protected virtual void GameEvents_OnLeftClickHeld(Vector3 mousePos, PlayerInputState inputState, bool isOverUI) 
        {
            if (inputState == PlayerInputState.BuildFarm)
            {
                PlanFarm(mousePos);
            }
        }
        
        protected virtual void GameEvents_OnLeftClickUp(Vector3 mousePos, PlayerInputState inputState, bool isOverUI)
        {
            if (isOverUI) return;
            
            if (inputState == PlayerInputState.BuildFurniture && _plannedFurniture != null)
            {
                if (_plannedFurniture.CheckPlacement())
                {
                    _plannedFurniture.SetState(Furniture.EFurnitureState.InProduction);
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
            else if (inputState == PlayerInputState.BuildBuilding && _plannedBuilding != null)
            {
                if (_plannedBuilding.CheckPlacement())
                {
                    var plannedBuilding = _plannedBuilding;
                    _plannedBuilding = null;
                    PlayerInputController.Instance.ChangeState(PlayerInputState.None);
                    plannedBuilding.SetState(Building.BuildingState.Planning);
                    plannedBuilding.TriggerPlaced();
                }
                else
                {
                    NotificationManager.Instance.Toast("Invalid Location");
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
            PlacementDirection = PlacementDirection.South;
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

        public Storage SpawnStorageContainer(StorageItemData storageData, Vector3 spawnPosition)
        {
            var containerObj = Instantiate(_storageContainerPrefab, spawnPosition, Quaternion.identity);
            containerObj.transform.SetParent(_storageParent);
            var container = containerObj.GetComponent<Storage>();
            container.Init(storageData);
            return container;
        }
        
        private Building _plannedBuilding;
        public void PlanBuilding(Building building, Action onBuildingPlaced = null)
        {
            _plannedBuilding = Instantiate(building, _structureParent);
            _plannedBuilding.SetState(Building.BuildingState.BeingPlaced);
            _plannedBuilding.OnBuildingPlaced = onBuildingPlaced;
        }

        private Furniture _plannedFurniture;
        private FurnitureItemData _plannedFurnitureItemData;
        public void PlanFurniture(FurnitureItemData furnitureData)
        {
            _plannedFurnitureItemData = furnitureData;
            var cursorPos = Helper.ConvertMousePosToGridPos(UtilsClass.GetMouseWorldPosition());
            var furniture = Instantiate(furnitureData.FurniturePrefab, cursorPos, Quaternion.identity, _furnitureParent);
            furniture.Init(furnitureData);
            furniture.SetState(Furniture.EFurnitureState.Planning);
            _plannedFurniture = furniture;
        }
        
        public void SpawnTree(Vector3 spawnPosition, GrowingResourceData growingResourceData)
        {
            spawnPosition = new Vector3(spawnPosition.x, spawnPosition.y, -1);
            var tree = Instantiate(_treePrefab, spawnPosition, Quaternion.identity);
            tree.transform.SetParent(_resourceParent);
            tree.GetComponent<GrowingResource>().Init(growingResourceData);
        }

        public void SpawnPlant(Vector3 spawnPosition, GrowingResourceData growingResourceData)
        {
            spawnPosition = new Vector3(spawnPosition.x, spawnPosition.y, -1);
            var plant = Instantiate(_plantPrefab, spawnPosition, Quaternion.identity);
            plant.transform.SetParent(_resourceParent);
            plant.GetComponent<GrowingResource>().Init(growingResourceData);
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
        
        #endregion
        
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

        public void SpawnSoilTile(Vector2 spawnPos, CropData cropData, Building building)
        {
            var soil = Instantiate(_soilPrefab, spawnPos, Quaternion.identity);
            soil.transform.SetParent(_flooringParent);
            soil.GetComponent<Crop>().Init(cropData, building);
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
        South,
        North,
        West, 
        East
    }
}
