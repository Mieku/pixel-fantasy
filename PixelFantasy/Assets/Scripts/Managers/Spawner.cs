using System;
using System.Collections.Generic;
using Characters;
using CodeMonkey.Utils;
using Controllers;
using Items;
using ScriptableObjects;
using Systems.Buildings.Scripts;
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
    
        [SerializeField] private Transform _structureParent;
        [SerializeField] private Transform _flooringParent;
        [SerializeField] private Transform _furnitureParent;

        [SerializeField] private GameObject _soilPrefab;
        
        [SerializeField] private SpriteRenderer _placementIcon;
        [SerializeField] private Sprite _genericPlacementSprite;

        [SerializeField] private Transform _kinlingsParent;
        [SerializeField] private Kinling _kinlingPrefab;

        [SerializeField] private Transform _miscParent;

        private bool _showPlacement;
        private List<string> _invalidPlacementTags = new List<string>();
        private List<string> _requiredPlacementTags = new List<string>();
        private Color? _colourOverride;
        
        // Structure
        private bool _planningStructure;
        private Vector2 _startPos;
        private List<GameObject> _blueprints = new List<GameObject>();
        private List<Vector2> _plannedGrid = new List<Vector2>();
        
        private PlacementDirection _prevPlacementDirection;
        public CropSettings CropSettings { get; set; }

        public Transform ItemsParent => _itemsParent;
        public Transform StructureParent => _structureParent;
        public Transform FlooringParent => _flooringParent;
        public Transform MiscParent => _miscParent;
        
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
                    _plannedFurniture.CompletePlanning();
                    _plannedFurniture.InitializeFurniture(_selectedFurnitureSettings, _prevPlacementDirection, _selectedDyeOverride);
                    _plannedFurniture = null;
                    
                    // Allows the player to place multiple
                    PlanFurniture(_selectedFurnitureSettings, _prevPlacementDirection, _selectedDyeOverride);
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
                    SpawnSoilTile(Helper.ConvertMousePosToGridPos(mousePos), CropSettings);
                }
            }
            else if (inputState == PlayerInputState.BuildDoor && _plannedDoor != null)
            {
                if (_plannedDoor.CheckPlacement())
                {
                    var plannedDoor = _plannedDoor;
                    _plannedDoor = null;
                    PlayerInputController.Instance.ChangeState(PlayerInputState.None);
                    plannedDoor.SetState(EConstructionState.Blueprint);
                    plannedDoor.TriggerPlaced();
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
            _requiredPlacementTags = null;
            CancelPlanning();
            _selectedFurnitureSettings = null;
            _selectedDyeOverride = null;
            _plannedDoor = null;
            _prevPlacementDirection = default;
        }

        public void ShowPlacementIconForReqTags(bool show, Sprite icon = null,
            List<String> requiredPlacementTags = null, float sizeOverride = 1f, Color? colourOverride = null)
        {
            _placementIcon.enabled = true;
            _colourOverride = colourOverride;

            if (icon == null)
            {
                icon = _genericPlacementSprite;
            }
            
            List<string> tags = null;
            if (requiredPlacementTags != null)
            {
                tags = new List<string>(requiredPlacementTags);
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
                _requiredPlacementTags = tags;
                _invalidPlacementTags = new List<string>();
            }
            else
            {   _placementIcon.gameObject.SetActive(false);
                _showPlacement = false;
            }
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
                _requiredPlacementTags = new List<string>();
            }
            else
            {   _placementIcon.gameObject.SetActive(false);
                _showPlacement = false;
                _requiredPlacementTags = null;
            }
        }

        private void Update()
        {
            if (_showPlacement)
            {
                var gridPos = Helper.ConvertMousePosToGridPos(UtilsClass.GetMouseWorldPosition());
                _placementIcon.transform.position = gridPos;
                if (Helper.IsGridPosValidToBuild(gridPos, _invalidPlacementTags, _requiredPlacementTags))
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

            if (_selectedFurnitureSettings != null)
            {
                if (Input.GetKeyDown(KeyCode.E)) // Clockwise
                {
                    if (_plannedFurniture != null)
                    {
                        _prevPlacementDirection = _plannedFurniture.RotatePlan(true);
                    }
                }
                
                if (Input.GetKeyDown(KeyCode.Q)) // Counter Clockwise
                {
                    if (_plannedFurniture != null)
                    {
                        _prevPlacementDirection = _plannedFurniture.RotatePlan(false);
                    }
                }
            }
        }

        private int _agentPriority = 0;
        public Kinling SpawnKinling(string fullname, Vector2 spawnPosition)
        {
            Kinling kinling = Instantiate(_kinlingPrefab, _kinlingsParent);
            kinling.transform.position = spawnPosition;
            kinling.gameObject.name = $"Kinling_{fullname}";
            
            kinling.KinlingAgent.SetPriority(_agentPriority);
            _agentPriority++;
            if (_agentPriority > 99)
            {
                _agentPriority = 0;
            }
            
            return kinling;
        }

        // public Item SpawnItemWithRuntimeData(ItemData data, Vector3 position, bool canBeHauled)
        // {
        //     var itemObject = Instantiate(_itemPrefab, position, Quaternion.identity, _itemsParent);
        //     if (itemObject.TryGetComponent<Item>(out var initializable))
        //     {
        //         initializable.LoadItemData(data, canBeHauled);
        //         itemObject.name = data.ItemName;
        //     }
        //     else
        //     {
        //         Debug.LogError("The spawned item does not implement Item.");
        //     }
        //
        //     return itemObject.GetComponent<Item>();
        // }
        
        // public Item SpawnItem(ItemSettings settings, Vector3 position, bool canBeHauled, Transform parent = null)
        // {
        //     var itemObject = Instantiate(_itemPrefab, position, Quaternion.identity, parent ?? _itemsParent);
        //     if (itemObject.TryGetComponent<Item>(out var initializable))
        //     {
        //         initializable.InitializeItem(settings, canBeHauled);
        //         itemObject.name = settings.ItemName;
        //     }
        //     else
        //     {
        //         Debug.LogError("The spawned item does not implement Item.");
        //     }
        //
        //     return itemObject.GetComponent<Item>();
        // }
        
        // public List<Item> SpawnItem(ItemSettings settings, Vector3 position, bool canBeHauled, int amount, Transform parent = null)
        // {
        //     List<Item> results = new List<Item>();
        //
        //     for (int i = 0; i < amount; i++)
        //     {
        //         var itemObject = Instantiate(_itemPrefab, position, Quaternion.identity, parent ?? _itemsParent);
        //         if (itemObject.TryGetComponent<Item>(out var initializable))
        //         {
        //             initializable.InitializeItem(settings, canBeHauled);
        //             results.Add(itemObject.GetComponent<Item>());
        //         }
        //         else
        //         {
        //             Debug.LogError("The spawned item does not implement Item.");
        //         }
        //     }
        //
        //
        //     return results;
        // }
        
        private Door _plannedDoor;
        public void PlanDoor(DoorSettings doorSettings, DyeSettings matColour, Action onDoorPlaced = null)
        {
            _plannedDoor = Instantiate(doorSettings.DoorPrefab, _structureParent);
            _plannedDoor.Init(doorSettings, matColour);
            _plannedDoor.OnDoorPlaced = onDoorPlaced;
        }

        private Furniture _plannedFurniture;
        private FurnitureSettings _selectedFurnitureSettings;
        private DyeSettings _selectedDyeOverride;
        public Furniture SpawnFurniture(Furniture prefab, Vector3 position, Transform parent = null)
        {
            var furnitureObject = Instantiate(prefab, position, Quaternion.identity, parent ?? _furnitureParent);
            return furnitureObject.GetComponent<Furniture>();
        }

        public void PlanFurniture(FurnitureSettings furnitureSettings, PlacementDirection direction, DyeSettings dye)
        {
            _selectedFurnitureSettings = furnitureSettings;
            _selectedDyeOverride = dye;
           
            var prefab = furnitureSettings.FurniturePrefab;
            var position = Helper.ConvertMousePosToGridPos(UtilsClass.GetMouseWorldPosition());
        
            _plannedFurniture = SpawnFurniture(prefab, position);
            _plannedFurniture.StartPlanning(furnitureSettings, direction, dye);
            _prevPlacementDirection = direction;
        }
        
        public void SpawnPlant(Vector3 spawnPosition, GrowingResourceSettings settings)
        {
            spawnPosition = new Vector3(spawnPosition.x, spawnPosition.y, -1);
            var plant = Instantiate(settings.Prefab, spawnPosition, Quaternion.identity);
            plant.transform.SetParent(_resourceParent);
            plant.GetComponent<GrowingResource>().InitializeResource(settings);
        }
        
        #region Structure

        private void CancelPlanning()
        {
            PlayerInputController.Instance.ChangeState(PlayerInputState.None);
            ClearPlannedBlueprint();
            _plannedGrid.Clear();

            if (_plannedFurniture != null)
            {
                Destroy(_plannedFurniture.gameObject);
                _plannedFurniture = null;
            }

            if (_plannedDoor != null)
            {
                Destroy(_plannedDoor.gameObject);
                _plannedDoor = null;
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
                    SpawnSoilTile(gridPos, CropSettings);
                }
            }

            ClearPlannedBlueprint();
            _plannedGrid.Clear();
            _planningStructure = false;
            CancelInput();
        }

        public void SpawnSoilTile(Vector2 spawnPos, CropSettings cropSettings)
        {
            var soil = Instantiate(_soilPrefab, spawnPos, Quaternion.identity);
            soil.transform.SetParent(_flooringParent);
            soil.GetComponent<Crop>().Init(cropSettings);
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
