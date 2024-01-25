using System;
using System.Collections.Generic;
using System.Linq;
using Buildings;
using Characters;
using CodeMonkey.Utils;
using Controllers;
using Handlers;
using Interfaces;
using Managers;
using ScriptableObjects;
using Sirenix.OdinInspector;
using Systems.Crafting.Scripts;
using Systems.SmartObjects.Scripts;
using TaskSystem;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using Zones;

namespace Items
{
    public class Furniture : PlayerInteractable, IClickableObject
    {
        public enum EFurnitureState
        {
            Planning,
            InProduction,
            Built,
            Craftable,
            Moving,
        }

        [TitleGroup("South")] [SerializeField] protected GameObject _southHandle;
        [TitleGroup("South")] [SerializeField] protected Transform _southSpritesRoot;
        [TitleGroup("South")] [SerializeField] protected Transform _southUseageMarkersRoot;
        
        [TitleGroup("West")] [SerializeField] protected GameObject _westHandle;
        [TitleGroup("West")] [SerializeField] protected Transform _westSpritesRoot;
        [TitleGroup("West")] [SerializeField] protected Transform _westUseageMarkersRoot;
        
        [TitleGroup("North")] [SerializeField] protected GameObject _northHandle;
        [TitleGroup("North")] [SerializeField] protected Transform _northSpritesRoot;
        [TitleGroup("North")] [SerializeField] protected Transform _northUseageMarkersRoot;
        
        [TitleGroup("East")] [SerializeField] protected GameObject _eastHandle;
        [TitleGroup("East")] [SerializeField] protected Transform _eastSpritesRoot;
        [TitleGroup("East")] [SerializeField] protected Transform _eastUseageMarkersRoot;
        
        [TitleGroup("General")]
        [SerializeField] protected FurnitureItemData _furnitureItemData;
        [SerializeField] protected SmartObject _smartObject;
        public PlacementDirection CurrentDirection;

        public EFurnitureState FurnitureState = EFurnitureState.Craftable;
        public string CraftersUID;

        private SpriteRenderer[] _allSprites;
        protected List<SpriteRenderer> _useageMarkers;
        private List<Material> _materials = new List<Material>();
        private int _fadePropertyID;
        
        private float _remainingWork;
        private bool _isOutlineLocked;
        protected Building _parentBuilding;
        private int _durabiliy;

        private Color _availableTransparent;
        private Color _unavailableTransparent;
        private ClickObject _clickObject;
        private Collider2D _placementCollider;

        private CraftingOrder _craftingOrder;
       
        public FurnitureItemData FurnitureItemData => _furnitureItemData;
        public Building ParentBuilding => _parentBuilding;

        protected virtual void Awake()
        {
            if(_smartObject != null) _smartObject.gameObject.SetActive(false);
            AssignDirection(CurrentDirection);
            
            _fadePropertyID = Shader.PropertyToID("_OuterOutlineFade");
            foreach (var spriteRenderer in _allSprites)
            {
                _materials.Add(spriteRenderer.material);
            }
            _availableTransparent = Librarian.Instance.GetColour("Available Transparent");
            _unavailableTransparent = Librarian.Instance.GetColour("Unavailable Transparent");
            _clickObject = GetComponent<ClickObject>();
            _placementCollider = GetComponent<Collider2D>();
            
            FurnitureManager.Instance.RegisterFurniture(this);

            GameEvents.OnLeftClickUp += GameEvents_OnLeftClickUp;
            GameEvents.OnRightClickUp += GameEvents_OnRightClickUp;
        }

        private void OnDestroy()
        {
            if (_parentBuilding != null)
            {
                _parentBuilding.DeregisterFurniture(this);
            }
            
            FurnitureManager.Instance.DeregisterFurniture(this);
            
            GameEvents.OnLeftClickUp -= GameEvents_OnLeftClickUp;
            GameEvents.OnRightClickUp -= GameEvents_OnRightClickUp;
        }

        protected virtual void Start()
        {
            // Check if this was placed in a room, if so add it to the room
            var building = Helper.IsPositionInBuilding(transform.position);
            if (building != null)
            {
                AssignBuilding(building);
            }
            
            AssignState(FurnitureState);
        }
        
        public bool IsAvailable
        {
            get
            {
                if (FurnitureState != EFurnitureState.Built)
                {
                    return false;
                }
                else
                {
                    if (_parentBuilding != null)
                    {
                        return _parentBuilding.State == Building.BuildingState.Built;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
        }
        
        public void AssignCommand(Command command, object payload = null)
        {
            CreateTask(command, payload);
        }
        
        protected void SetNextDirection(bool isClockwise)
        {
            if (isClockwise)
            {
                if (!AssignDirection(Helper.GetNextDirection(CurrentDirection)))
                {
                    SetNextDirection(true);
                }
            }
            else
            {
                if (!AssignDirection(Helper.GetPrevDirection(CurrentDirection)))
                {
                    SetNextDirection(false);
                }
            }
            
            DisplayUseageMarkers(true);
        }

        protected bool AssignDirection(PlacementDirection direction)
        {
            if(_northHandle != null) _northHandle.SetActive(false);
            if(_eastHandle != null) _eastHandle.SetActive(false);
            if(_southHandle != null) _southHandle.SetActive(false);
            if(_westHandle != null) _westHandle.SetActive(false);
            
            CurrentDirection = direction;
            
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

            return true;
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
            switch (CurrentDirection)
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
            if (isOverUI) return;

            if (FurnitureState == EFurnitureState.Moving)
            {
                if (CheckPlacement())
                {
                    SetState(_prevousState);
                }
            }
        }
        
        private void GameEvents_OnRightClickUp(Vector3 mousePos, PlayerInputState inputState, bool isOverUI)
        {
            if (isOverUI) return;

            if (FurnitureState == EFurnitureState.Moving)
            {
                Moving_Cancelled();
                SetState(_prevousState);
            }
        }

        private EFurnitureState _prevousState;
        public void SetState(EFurnitureState newState)
        {
            if(FurnitureState == newState) return;
            
            _prevousState = FurnitureState;
            switch (FurnitureState)
            {
                case EFurnitureState.Planning:
                    break;
                case EFurnitureState.InProduction:
                    break;
                case EFurnitureState.Built:
                    break;
                case EFurnitureState.Craftable:
                    //Craftable_Exit();
                    break;
                case EFurnitureState.Moving:
                    Moving_Exit();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            AssignState(newState);
        }

        public void AssignState(EFurnitureState newState)
        {
            FurnitureState = newState;
            switch (FurnitureState)
            {
                case EFurnitureState.Planning:
                    Planning_Enter();
                    break;
                case EFurnitureState.InProduction:
                    InProduction_Enter();
                    break;
                case EFurnitureState.Built:
                    Built_Enter();
                    break;
                case EFurnitureState.Craftable:
                    Craftable_Enter();
                    break;
                case EFurnitureState.Moving:
                    Moving_Enter();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public virtual void Init(FurnitureItemData furnitureItemData)
        {
            _furnitureItemData = furnitureItemData;
        }

        public List<Transform> UseagePositions()
        {
            List<Transform> results = new List<Transform>();
            foreach (var marker in _useageMarkers)
            {
                results.Add(marker.transform);
            }

            return results;
        }

        public Transform UseagePosition()
        {
            // TODO: Expand this to handle best position for situation
            return UseagePositions()[0];
        }

        public void ShowCraftable(bool isShown)
        {
            if(FurnitureState != EFurnitureState.Craftable) return;
            
            if (isShown)
            {
                Show(true);
                // If it is possible to craft, show the item in transparent. When clicked add an order to craft it
                if (CanBeCrafted())
                {
                    ColourArt(_availableTransparent);
                }
                else // If it is not possible to craft, show the item in red transparent. When clicked, explain why
                {
                    ColourArt(_unavailableTransparent);
                }
            }
            else
            {
                Show(false);
            }
        }

        public bool CanBeCrafted()
        {
            // Check if the required crafter exists
            var requiredJob = _furnitureItemData.RequiredCraftingJob;
            if (!UnitsManager.Instance.AnyUnitHaveJob(requiredJob)) return false;
            
            // Check if crafting table exits
            var requiredFurniture = _furnitureItemData.RequiredCraftingTable;
            if (!FurnitureManager.Instance.DoesFurnitureExist(requiredFurniture)) return false;
            
            return true;
        }

        private void Craftable_Enter()
        {
            
        }

        // private void Craftable_Exit()
        // {
        //     if (_parentBuilding != null)
        //     {
        //         _parentBuilding.DeregisterCraftableFurniture(this);
        //     }
        // }

        private Vector2 _beforeMovedPosition;
        private void Moving_Enter()
        {
            _beforeMovedPosition = transform.position;
            DisplayUseageMarkers(true);
        }

        private void Moving_Cancelled()
        {
            transform.position = _beforeMovedPosition;
        }

        private void Moving_Exit()
        {
            DisplayUseageMarkers(false);
        }
        
        private void Planning_Enter()
        {
            _remainingWork = _furnitureItemData.WorkCost;
            DisplayUseageMarkers(true);
        }

        public void InProduction_Enter()
        {
            _remainingWork = _furnitureItemData.WorkCost;
            DisplayUseageMarkers(false);
            Show(true);
            ColourArt(ColourStates.Blueprint);
            CreateFurnitureHaulingTask();
        }

        public void DisplayUseageMarkers(bool showMarkers)
        {
            foreach (var marker in _useageMarkers)
            {
                marker.gameObject.SetActive(showMarkers);
            }
        }

        public float DurabilityPercentage()
        {
            return (float)_durabiliy / _furnitureItemData.Durability;
        }
        
        private void CreateFurnitureHaulingTask()
        {
            // If item exists, claim it
            var claimedItem = InventoryManager.Instance.ClaimItem(_furnitureItemData);
            if (claimedItem != null)
            {
                Task task = new Task("Place Furniture", this, Librarian.Instance.GetJob("Worker"), EToolType.None)
                {
                    Materials = new List<Item>(){ claimedItem },
                };
                TaskManager.Instance.AddTask(task);
            }
            else
            {
                _craftingOrder = new CraftingOrder(_furnitureItemData, 
                    this,
                    CraftingOrder.EOrderType.Furniture,
                    OnOrderClaimed, 
                    OnCraftingOrderDelivered,
                    OnCraftingOrderCancelled);
            }
        }

        private void OnOrderClaimed()
        {
            
        }

        private void OnCraftingOrderDelivered()
        {
            
        }

        private void OnCraftingOrderCancelled()
        {
            
        }
        
        public void OnCraftingBillCancelled()
        {
            
        }

        public override void ReceiveItem(Item item)
        {
            CraftersUID = item.State.CraftersUID;
            Destroy(item.gameObject);
        }

        public void PlaceFurniture(Item furnitureItem)
        {
            CraftersUID = furnitureItem.State.CraftersUID;
            Destroy(furnitureItem.gameObject);
            
            SetState(EFurnitureState.Built);
        }
        
        public bool DoPlacement(float workAmount)
        {
            _remainingWork -= workAmount;
            if (_remainingWork <= 0)
            {
                SetState(EFurnitureState.Built);
                return true;
            }
            
            return false;
        }

        protected virtual void Built_Enter()
        {
            DisplayUseageMarkers(false);
            _durabiliy = _furnitureItemData.Durability;
            _remainingWork = _furnitureItemData.WorkCost;
            ColourArt(ColourStates.Built);
            
            if(_smartObject != null) _smartObject.gameObject.SetActive(true);
        }

        public void AssignBuilding(Building building)
        {
            if (_parentBuilding == building) return;
            
            if (building == null)
            {
                Debug.LogError($"Attmepted to assign {_furnitureItemData.ItemName} to null building");
                return;
            }

            _parentBuilding = building;

            building.RegisterFurniture(this);

            // if (FurnitureState == EFurnitureState.Craftable)
            // {
            //     building.RegisterCraftableFurniture(this);
            // }
        }
        
        private void Update()
        {
            if (FurnitureState is EFurnitureState.Planning or EFurnitureState.Moving)
            {
                FollowCursor();
                CheckPlacement();
            }

            HandleRotation();
            
        }

        private void HandleRotation()
        {
            if (FurnitureState is not (EFurnitureState.Planning or EFurnitureState.Moving)) return;
            if (Input.GetKeyDown(KeyCode.E)) // Clockwise
            {
                SetNextDirection(true);
            }

            if (Input.GetKeyDown(KeyCode.Q)) // Counter Clockwise
            {
                SetNextDirection(false);
            }
        }
        
        private void OnMouseEnter()
        {
            if (FurnitureState != EFurnitureState.Built) return;
            if(_isOutlineLocked) return;
            
            TriggerOutline(true);
        }

        private void OnMouseExit()
        {
            if (FurnitureState != EFurnitureState.Built) return;
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
                    if(FurnitureState == EFurnitureState.Planning)
                    {
                        ColourArt(ColourStates.Blueprint);
                    }
                }
            }
        }
        
        public virtual bool CheckPlacement()
        {
            bool result = Helper.IsGridPosValidToBuild(transform.position, _furnitureItemData.InvalidPlacementTags);

            if (result && _parentBuilding != null)
            {
                result = _parentBuilding.IsColliderInInterior(_placementCollider);
            }

            // Check the useage markers
            if (_useageMarkers != null)
            {
                bool markersPass = false;
                foreach (var marker in _useageMarkers)
                {
                    if (Helper.IsGridPosValidToBuild(marker.transform.position, _furnitureItemData.InvalidPlacementTags))
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

        public bool CanBeOrdered()
        {
            if (FurnitureState != EFurnitureState.Craftable) return false;
            if (!CanBeCrafted()) return false;


            return true;
        }

        public void Order()
        {
            if (FurnitureState == EFurnitureState.Craftable)
            {
                SetState(EFurnitureState.InProduction);
            }
        }

        public void Trigger_Move()
        {
            SetState(EFurnitureState.Moving);
        }

        public ClickObject GetClickObject()
        {
            return _clickObject;
        }

        public bool IsClickDisabled { get; set; }
        public bool IsAllowed { get; set; }
        public void ToggleAllowed(bool isAllowed)
        {
            IsAllowed = isAllowed;
        }

        public List<Command> GetCommands()
        {
            return Commands;
        }
        
        public string DisplayName
        {
            get
            {
                string result = _furnitureItemData.ItemName;
                switch (FurnitureState)
                {
                    case EFurnitureState.Planning:
                        break;
                    case EFurnitureState.InProduction:
                        result += " (Ordered)";
                        break;
                    case EFurnitureState.Built:
                        break;
                    case EFurnitureState.Craftable:
                        result += " (Blueprint)";
                        break;
                    case EFurnitureState.Moving:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return result;
            }
        }
        
        public PlayerInteractable GetPlayerInteractable()
        {
            return this;
        }
        
        public Unit GetCrafter()
        {
            if (!WasCrafted) return null;

            return UnitsManager.Instance.GetUnit(CraftersUID);
        }

        public bool WasCrafted => !string.IsNullOrEmpty(CraftersUID);

        public NeedChange InUseNeedChange => _furnitureItemData.InUseNeedChange;
    }
}
