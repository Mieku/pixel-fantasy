using System;
using System.Collections.Generic;
using System.Linq;
using Characters;
using CodeMonkey.Utils;
using Controllers;
using Data.Item;
using Databrain;
using Databrain.Attributes;
using Handlers;
using Interfaces;
using Managers;
using ScriptableObjects;
using Sirenix.OdinInspector;
using Systems.Crafting.Scripts;
using Systems.Details.Build_Details.Scripts;
using TaskSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace Items
{
    public interface IFurnitureInitializable
    {
        bool Init(FurnitureData data, FurnitureVarient varient = null, DyeSettings dye = null);
    }
    
    public class Furniture : PlayerInteractable, IClickableObject, IFurnitureInitializable
    {
        [TitleGroup("South")] [SerializeField] protected GameObject _southHandle;
        [TitleGroup("South")] [SerializeField] protected Transform _southSpritesRoot;
        [TitleGroup("South")] [SerializeField] protected Transform _southUseageMarkersRoot;
        [TitleGroup("South")] [SerializeField] protected GameObject _southPlacementObstacle;
        
        [TitleGroup("West")] [SerializeField] protected GameObject _westHandle;
        [TitleGroup("West")] [SerializeField] protected Transform _westSpritesRoot;
        [TitleGroup("West")] [SerializeField] protected Transform _westUseageMarkersRoot;
        [TitleGroup("West")] [SerializeField] protected GameObject _westPlacementObstacle;
        
        [TitleGroup("North")] [SerializeField] protected GameObject _northHandle;
        [TitleGroup("North")] [SerializeField] protected Transform _northSpritesRoot;
        [TitleGroup("North")] [SerializeField] protected Transform _northUseageMarkersRoot;
        [TitleGroup("North")] [SerializeField] protected GameObject _northPlacementObstacle;
        
        [TitleGroup("East")] [SerializeField] protected GameObject _eastHandle;
        [TitleGroup("East")] [SerializeField] protected Transform _eastSpritesRoot;
        [TitleGroup("East")] [SerializeField] protected Transform _eastUseageMarkersRoot;
        [TitleGroup("East")] [SerializeField] protected GameObject _eastPlacementObstacle;

        public DataLibrary DataLibrary;
        
        [DataObjectDropdown("DataLibrary")]
        public FurnitureData Data;
        public FurnitureData RuntimeData;
        
        protected SpriteRenderer[] _allSprites;
        protected List<SpriteRenderer> _useageMarkers;
        protected readonly List<Material> _materials = new List<Material>();
        private int _fadePropertyID;

        protected bool _isPlanning;
        protected PlacementDirection _direction;
        private bool _isOutlineLocked;
        private ClickObject _clickObject;

        protected virtual void Awake()
        {
            _fadePropertyID = Shader.PropertyToID("_OuterOutlineFade");
            _clickObject = GetComponent<ClickObject>();

            GameEvents.OnLeftClickUp += GameEvents_OnLeftClickUp;
            GameEvents.OnRightClickUp += GameEvents_OnRightClickUp;
        }

        private void OnDestroy()
        {
            FurnitureManager.Instance.DeregisterFurniture(RuntimeData);
            
            GameEvents.OnLeftClickUp -= GameEvents_OnLeftClickUp;
            GameEvents.OnRightClickUp -= GameEvents_OnRightClickUp;
        }

        private void Start()
        {
            
        }

        private void Saved()
        {
            
        }

        private void Loaded()
        {
            
        }

        private void DataReady()
        {
            RuntimeData = (FurnitureData) DataLibrary.CloneDataObjectToRuntime(Data, gameObject);

            RuntimeData.Direction = _direction;
            SetState(RuntimeData.State);
            AssignDirection(_direction);
            
        }
        
        public void StartPlanning(BuildDetailsUI.SelectedFurnitureDetails selectedFurnitureDetails, PlacementDirection initialDirection)
        {
            _isPlanning = true;
            Data = selectedFurnitureDetails.Furniture;
            AssignDirection(initialDirection);
            foreach (var spriteRenderer in _allSprites)
            {
                _materials.Add(spriteRenderer.material);
            }
            
            DisplayUseageMarkers(true);
            EnablePlacementObstacle(false);
        }

        public PlacementDirection RotatePlan(bool isClockwise)
        {
            return SetNextDirection(isClockwise);
        }

        public virtual void CompletePlanning()
        {
            _isPlanning = false;
            DataLibrary.RegisterInitializationCallback(DataReady);
            DataLibrary.OnSaved += Saved;
            DataLibrary.OnLoaded += Loaded;
        }

        public bool Init(FurnitureData furnitureData, FurnitureVarient varient = null, DyeSettings dye = null)
        {
            Data = furnitureData;
            
            DataLibrary.RegisterInitializationCallback(DataReady);
            DataLibrary.OnSaved += Saved;
            DataLibrary.OnLoaded += Loaded;
        
            return true;
        }

        public bool IsAvailable
        {
            get
            {
                if (RuntimeData.State != EFurnitureState.Built && !RuntimeData.InUse)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        
        public void AssignCommand(Command command, object payload = null)
        {
            CreateTask(command, payload);
        }
        
        protected PlacementDirection SetNextDirection(bool isClockwise)
        {
            if (isClockwise)
            {
                if (!AssignDirection(Helper.GetNextDirection(_direction)))
                {
                    SetNextDirection(true);
                }
            }
            else
            {
                if (!AssignDirection(Helper.GetPrevDirection(_direction)))
                {
                    SetNextDirection(false);
                }
            }
            
            DisplayUseageMarkers(true);

            return _direction;
        }

        protected bool AssignDirection(PlacementDirection direction)
        {
            if(_northHandle != null) _northHandle.SetActive(false);
            if(_eastHandle != null) _eastHandle.SetActive(false);
            if(_southHandle != null) _southHandle.SetActive(false);
            if(_westHandle != null) _westHandle.SetActive(false);
            
            _direction = direction;
            
            switch (direction)
            {
                case PlacementDirection.South:
                    if (_southHandle == null) return false;
                    _southHandle.SetActive(true);
                    _allSprites = _southSpritesRoot.GetComponentsInChildren<SpriteRenderer>(true);
                    _useageMarkers = _southUseageMarkersRoot.GetComponentsInChildren<SpriteRenderer>(true).ToList();
                    break;
                case PlacementDirection.North:
                    if (_northHandle == null) return false;
                    _northHandle.SetActive(true);
                    _allSprites = _northSpritesRoot.GetComponentsInChildren<SpriteRenderer>(true);
                    _useageMarkers = _northUseageMarkersRoot.GetComponentsInChildren<SpriteRenderer>(true).ToList();
                    break;
                case PlacementDirection.West:
                    if (_westHandle == null) return false;
                    _westHandle.SetActive(true);
                    _allSprites = _westSpritesRoot.GetComponentsInChildren<SpriteRenderer>(true);
                    _useageMarkers = _westUseageMarkersRoot.GetComponentsInChildren<SpriteRenderer>(true).ToList();
                    break;
                case PlacementDirection.East:
                    if (_eastHandle == null) return false;
                    _eastHandle.SetActive(true);
                    _allSprites = _eastSpritesRoot.GetComponentsInChildren<SpriteRenderer>(true);
                    _useageMarkers = _eastUseageMarkersRoot.GetComponentsInChildren<SpriteRenderer>(true).ToList();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            if (!_isPlanning)
            {
                EnablePlacementObstacle(true);
            }

            return true;
        }

        protected void EnablePlacementObstacle(bool enable)
        {
            if(_eastPlacementObstacle != null) _eastPlacementObstacle.SetActive(false);
            if(_westPlacementObstacle != null) _westPlacementObstacle.SetActive(false);
            if(_northPlacementObstacle != null) _northPlacementObstacle.SetActive(false);
            if(_southPlacementObstacle != null) _southPlacementObstacle.SetActive(false);
            
            if (enable)
            {
                switch (_direction)
                {
                    case PlacementDirection.South:
                        _southPlacementObstacle.SetActive(true);
                        break;
                    case PlacementDirection.North:
                        _northPlacementObstacle.SetActive(true);
                        break;
                    case PlacementDirection.West:
                        _westPlacementObstacle.SetActive(true);
                        break;
                    case PlacementDirection.East:
                        _eastPlacementObstacle.SetActive(true);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void DisplaySprites(bool showSprites)
        {
            SpritesRoot().gameObject.SetActive(showSprites);

            if (!showSprites)
            {
                DisplayUseageMarkers(false);
            }
        }

        protected Transform SpritesRoot()
        {
            switch (_direction)
            {
                case PlacementDirection.South:
                    return _southSpritesRoot;
                case PlacementDirection.North:
                    return _northSpritesRoot;
                case PlacementDirection.West:
                    return _westSpritesRoot;
                case PlacementDirection.East:
                    return _eastSpritesRoot;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Show(bool isShown)
        {
            if (isShown)
            {
                DisplaySprites(true);
                IsClickDisabled = false;
            }
            else
            {
                DisplaySprites(false);
                IsClickDisabled = true;
            }
        }

        private void GameEvents_OnLeftClickUp(Vector3 mousePos, PlayerInputState inputState, bool isOverUI)
        {
            //if (isOverUI) return;
        }
        
        private void GameEvents_OnRightClickUp(Vector3 mousePos, PlayerInputState inputState, bool isOverUI)
        {

        }
        
        [ShowInInspector] 
        public void SetState(EFurnitureState newState)
        {
            RuntimeData.State = newState;
            switch (RuntimeData.State)
            {
                case EFurnitureState.InProduction:
                    InProduction_Enter();
                    break;
                case EFurnitureState.Built:
                    Built_Enter();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private List<Transform> UseagePositions()
        {
            List<Transform> results = new List<Transform>();
            foreach (var marker in _useageMarkers)
            {
                results.Add(marker.transform);
            }

            if (results.Count == 0)
            {
                results.Add(transform);
            }

            return results;
        }

        public override Vector2? UseagePosition(Vector2 requestorPosition)
        {
            List<Transform> potentialPositions = new List<Transform>();
            
            foreach (var useageMarker in UseagePositions())
            {
                bool result = Helper.IsGridPosValidToBuild(useageMarker.position, Data.InvalidPlacementTags, gameObject);

                if (result)
                {
                    potentialPositions.Add(useageMarker);
                }
            }
            
            List<(Transform, float)> distances = new List<(Transform, float)>();
            foreach (var potentialPosition in potentialPositions)
            {
                var pathResult = Helper.DoesPathExist(requestorPosition, potentialPosition.position);
                if (pathResult.pathExists)
                {
                    float distance = Helper.GetPathLength(pathResult.navMeshPath);
                    distances.Add((potentialPosition, distance));
                }
            }

            // if for some reason there is no remaining position, log an error but also just prove the furniture's transform position
            if (distances.Count == 0)
            {
                Debug.LogError($"Could not find a possible position for {gameObject.name}");
                return null;
            }
            
            // Compile the positions that pass the above tests and sort them by distance
            var sortedDistances = distances.OrderBy(x => x.Item2).Select(x => x.Item1).ToList();
            var selectedDistance = sortedDistances[0];
            return selectedDistance.position;
        }

        // public bool CanBeCrafted()
        // {
        //     // Check if crafting table exits
        //     foreach (var option in _furnitureItemData.RequiredCraftingTableOptions)
        //     {
        //         if (FurnitureManager.Instance.DoesFurnitureExist(option))
        //         {
        //             return true;
        //         }
        //     }
        //
        //     return false;
        // }
        
        // protected virtual void Planning_Enter()
        // {
        //     DisplayUseageMarkers(true);
        //     EnablePlacementObstacle(false);
        // }

        protected virtual void InProduction_Enter()
        {
            FurnitureManager.Instance.RegisterFurniture(RuntimeData);
            DisplayUseageMarkers(false);
            EnablePlacementObstacle(true);
            Show(true);
            ColourArt(ColourStates.Blueprint);
            CreateFurnitureHaulingTask();
        }
        
        protected virtual void Built_Enter()
        {
            DisplayUseageMarkers(false);
            EnablePlacementObstacle(true);
            ColourArt(ColourStates.Built);
        }

        public void DisplayUseageMarkers(bool showMarkers)
        {
            if (_useageMarkers == null) return;
            
            foreach (var marker in _useageMarkers)
            {
                marker.gameObject.SetActive(showMarkers);
            }
        }
        
        private void CreateFurnitureHaulingTask()
        {
            // If there are no material costs, build instantly
            if (RuntimeData.CraftRequirements.MaterialCosts.Count == 0)
            {
                SetState(EFurnitureState.Built);
                return;
            }
            
            // If item exists, claim it
            var claimedItem = InventoryManager.Instance.ClaimItem(RuntimeData);
            if (claimedItem)
            {
                Task task = new Task("Place Furniture", ETaskType.Hauling, this, EToolType.None)
                {
                    Materials = new List<ItemData>(){ RuntimeData },
                };
                TaskManager.Instance.AddTask(task);
            }
            else
            {
                var craftingOrder = new CraftingOrder(
                        RuntimeData, 
                        this, 
                        CraftingOrder.EOrderType.Furniture, 
                        true,
                        OnFurnitureOrderClaimed, 
                        OnFurnitureOrderDelivered, 
                        OnFurnitureOrderCancelled
                    );
            }
        }

        private void OnFurnitureOrderClaimed()
        {
            
        }

        private void OnFurnitureOrderDelivered()
        {
            
        }

        private void OnFurnitureOrderCancelled()
        {
            
        }

        public override void ReceiveItem(ItemData item)
        {
            Destroy(item.LinkedItem.gameObject);
        }

        public void PlaceFurniture(FurnitureData furnitureItem)
        {
            RuntimeData.CraftersUID = furnitureItem.CraftersUID;
            //Destroy(furnitureItem.gameObject);
            
            SetState(EFurnitureState.Built);
        }
        
        public bool DoPlacement(float workAmount)
        {
            RuntimeData.RemainingWork -= workAmount;
            if (RuntimeData.RemainingWork <= 0)
            {
                SetState(EFurnitureState.Built);
                return true;
            }
            
            return false;
        }
        
        protected virtual void Update()
        {
            //if (RuntimeData == null) return;
            
            if (_isPlanning)
            {
                FollowCursor();
                CheckPlacement();
            }

            HandleRotation();
        }

        private void HandleRotation()
        {
            // if (RuntimeData.State is not (EFurnitureState.Planning)) return;
            // if (Input.GetKeyDown(KeyCode.E)) // Clockwise
            // {
            //     SetNextDirection(true);
            // }
            //
            // if (Input.GetKeyDown(KeyCode.Q)) // Counter Clockwise
            // {
            //     SetNextDirection(false);
            // }
        }
        
        private void OnMouseEnter()
        {
            if (RuntimeData == null) return;
            
            if (RuntimeData.State != EFurnitureState.Built) return;
            if(_isOutlineLocked) return;
            
            TriggerOutline(true);
        }

        private void OnMouseExit()
        {
            if (RuntimeData == null) return;
            
            if (RuntimeData.State != EFurnitureState.Built) return;
            if(_isOutlineLocked) return;
            
            TriggerOutline(false);
        }
        
        private void FollowCursor()
        {
            gameObject.transform.position = UtilsClass.GetMouseWorldPosition();
        }
        
        public void LockOutline(bool isLocked, bool showOutline)
        {
            _isOutlineLocked = isLocked;
            TriggerOutline(showOutline);
        }
        
        private void TriggerOutline(bool showOuline)
        {
            foreach (var material in _materials)
            {
                if (showOuline)
                {
                    material.SetFloat(_fadePropertyID, 1);
                    ColourArt(ColourStates.Built);
                }
                else
                {
                    material.SetFloat(_fadePropertyID, 0);
                    if(_isPlanning)
                    {
                        ColourArt(ColourStates.Blueprint);
                    }
                }
            }
        }
        
        public virtual bool CheckPlacement()
        {
            bool result = Helper.IsGridPosValidToBuild(transform.position, Data.InvalidPlacementTags);

            // Check the useage markers
            if (_useageMarkers != null && _useageMarkers.Count > 0)
            {
                bool markersPass = false;
                foreach (var marker in _useageMarkers)
                {
                    if (Helper.IsGridPosValidToBuild(marker.transform.position, Data.InvalidPlacementTags))
                    {
                        marker.color = Color.white;
                        markersPass = true;
                    }
                    else
                    {
                        marker.color = Color.red;
                    }
                }

                if (!markersPass)
                {
                    result = false;
                }
            }

            if (result)
            {
                ColourArt(ColourStates.CanPlace);
            }
            else
            {
                ColourArt(ColourStates.CantPlace);
            }
            
            return result;
        }
        
        public void ColourArt(ColourStates colourState)
        {
            Color colour;
            switch (colourState)
            {
                case ColourStates.Built:
                    colour = Color.white;
                    break;
                case ColourStates.Blueprint:
                    colour = Librarian.Instance.GetColour("Blueprint");
                    break;
                case ColourStates.CanPlace:
                    colour = Librarian.Instance.GetColour("Placement Green");
                    break;
                case ColourStates.CantPlace:
                    colour = Librarian.Instance.GetColour("Placement Red");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(colourState), colourState, null);
            }
        
            ColourArt(colour);
        }

        private void ColourArt(Color colour)
        {
            if (_allSprites == null) return;
            foreach (var spriteRenderer in _allSprites)
            {
                spriteRenderer.color = colour;
            }
        }
        
        public enum ColourStates
        {
            Built,
            Blueprint,
            CanPlace,
            CantPlace,
        }

        public virtual bool CanKinlingUseThis()
        {
            if (!IsAvailable) return false;

            return true;
        }

        public void Trigger_Move()
        {

        }

        public ClickObject GetClickObject()
        {
            return _clickObject;
        }

        public bool IsClickDisabled { get; set; }


        public List<Command> GetCommands()
        {
            return Commands;
        }
        
        public string DisplayName => RuntimeData.title;

        public PlayerInteractable GetPlayerInteractable()
        {
            return this;
        }
        
        public Kinling GetCrafter()
        {
            if (!WasCrafted) return null;

            return KinlingsManager.Instance.GetUnit(RuntimeData.CraftersUID);
        }

        public bool WasCrafted => !string.IsNullOrEmpty(RuntimeData.CraftersUID);

        public NeedChange InUseNeedChange => RuntimeData.InUseNeedChange;
    }
}
