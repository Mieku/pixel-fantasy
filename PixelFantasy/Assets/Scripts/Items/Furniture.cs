using System;
using System.Collections.Generic;
using System.Linq;
using Characters;
using CodeMonkey.Utils;
using Controllers;
using Handlers;
using Interfaces;
using Managers;
using ScriptableObjects;
using Sirenix.OdinInspector;
using Systems.Stats.Scripts;
using TaskSystem;
using UnityEngine;

namespace Items
{
    public class Furniture : PlayerInteractable, IClickableObject
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
        
        public FurnitureData RuntimeData;
        //public FurnitureSettings FurnitureSettings;
        
        protected SpriteRenderer[] _allSprites;
        protected List<SpriteRenderer> _useageMarkers;
        protected readonly List<Material> _materials = new List<Material>();
        private int _fadePropertyID;

        protected bool _isPlanning;
        protected PlacementDirection _direction;
        private bool _isOutlineLocked;
        private ClickObject _clickObject;
        private DyeSettings _dyeOverride;
        private readonly List<string> _invalidPlacementTags = new List<string>() { "Water", "Wall", "Obstacle"};

        public Action OnChanged { get; set; }

        protected virtual void Awake()
        {
            _fadePropertyID = Shader.PropertyToID("_OuterOutlineFade");
            _clickObject = GetComponent<ClickObject>();

            GameEvents.OnLeftClickUp += GameEvents_OnLeftClickUp;
            GameEvents.OnRightClickUp += GameEvents_OnRightClickUp;
        }

        private void OnDestroy()
        {
            FurnitureManager.Instance.DeregisterFurniture(this);
            
            GameEvents.OnLeftClickUp -= GameEvents_OnLeftClickUp;
            GameEvents.OnRightClickUp -= GameEvents_OnRightClickUp;
        }

        private void Start()
        {
            
        }

        protected void Saved()
        {
            
        }

        protected void Loaded()
        {
            
        }
        
        public virtual void StartPlanning(FurnitureSettings furnitureSettings, PlacementDirection initialDirection, DyeSettings dye)
        {
            _isPlanning = true;
            _dyeOverride = dye;
            AssignDirection(initialDirection);
            foreach (var spriteRenderer in _allSprites)
            {
                _materials.Add(spriteRenderer.material);
            }
            
            DisplayUseageMarkers(true);
            EnablePlacementObstacle(false);
        }
        
        public virtual void CompletePlanning()
        {
            _isPlanning = false;
            OnChanged?.Invoke();
        }

        public virtual void InitializeFurniture(FurnitureSettings furnitureSettings, PlacementDirection direction, DyeSettings dye)
        {
            //FurnitureSettings = furnitureSettings;
            _dyeOverride = dye;
            RuntimeData = new FurnitureData();
            RuntimeData.InitData(furnitureSettings);
            RuntimeData.LinkedFurniture = this;
            RuntimeData.Direction = direction;
            
            SetState(RuntimeData.State);
            AssignDirection(direction);
        }

        public PlacementDirection RotatePlan(bool isClockwise)
        {
            return SetNextDirection(isClockwise);
        }

        public virtual bool IsAvailable
        {
            get
            {
                if (RuntimeData != null && RuntimeData.State != EFurnitureState.Built) return false;

                if (RuntimeData.InUse || RuntimeData.HasUseBlockingCommand)
                {
                    return false;
                }
                    
                return true;
            }
        }
        
        public void AssignCommand(Command command, object payload = null)
        {
            if (command.name == "Move Furniture Command")
            {
                RuntimeData.HasUseBlockingCommand = true;
                Debug.LogError("Move Furniture Command is not built yet"); // TODO: Just as a reminder
            }
            
            if (command.name == "Deconstruct Furniture Command")
            {
                RuntimeData.HasUseBlockingCommand = true;
                if (RuntimeData.State != EFurnitureState.Built)
                {
                    CancelFurnitureConstruction();
                }
                else
                {
                    CreateTask(command, payload);
                }
            }
            else
            {
                CreateTask(command, payload);
            }
        }

        public override void CancelCommand(Command command)
        {
            RuntimeData.HasUseBlockingCommand = false;
            
            base.CancelCommand(command);
        }

        protected void CancelFurnitureConstruction()
        {
            FurnitureManager.Instance.DeregisterFurniture(this);
            
            CancelRequestorTasks();
                
            // Spawn All the resources used
            SpawnUsedResources(100f);

            // Delete this blueprint
            Destroy(gameObject);
        }
        
        public virtual void SpawnUsedResources(float percentReturned)
        {
            // Spawn All the resources used
            var totalCosts = RuntimeData.FurnitureSettings.CraftRequirements.GetMaterialCosts();
            var remainingCosts = RuntimeData.RemainingMaterialCosts;
            List<ItemAmount> difference = new List<ItemAmount>();
            foreach (var totalCost in totalCosts)
            {
                var remaining = remainingCosts.Find(c => c.Item == totalCost.Item);
                int remainingAmount = 0;
                if (remaining != null)
                {
                    remainingAmount = remaining.Quantity;
                }
                
                int amount = totalCost.Quantity - remainingAmount;
                if (amount > 0)
                {
                    ItemAmount refund = new ItemAmount
                    {
                        Item = totalCost.Item,
                        Quantity = amount
                    };
                    difference.Add(refund);
                }
            }

            foreach (var refundCost in difference)
            {
                for (int i = 0; i < refundCost.Quantity; i++)
                {
                    if (Helper.RollDice(percentReturned))
                    {
                        Spawner.Instance.SpawnItem(refundCost.Item, this.transform.position, true);
                    }
                }
            }
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
            
            OnChanged?.Invoke();
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
                bool result = Helper.IsGridPosValidToBuild(useageMarker.position, _invalidPlacementTags, new List<string>() ,gameObject);

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

        protected virtual void InProduction_Enter()
        {
            RuntimeData.Position = transform.position;
            FurnitureManager.Instance.RegisterFurniture(this);
            DisplayUseageMarkers(false);
            EnablePlacementObstacle(true);
            Show(true);
            ColourArt(ColourStates.Blueprint);
            CreateCraftFurnitureTask();
            Commands.Add(Librarian.Instance.GetCommand("Deconstruct Furniture"));
        }
        
        protected virtual void Built_Enter()
        {
            RuntimeData.Position = transform.position;
            RuntimeData.RemainingWork = RuntimeData.FurnitureSettings.CraftRequirements.WorkCost;
            DisplayUseageMarkers(false);
            EnablePlacementObstacle(true);
            ColourArt(ColourStates.Built);
            Commands.Add(Librarian.Instance.GetCommand("Move Furniture"));
        }

        public void DisplayUseageMarkers(bool showMarkers)
        {
            if (_useageMarkers == null) return;
            
            foreach (var marker in _useageMarkers)
            {
                marker.gameObject.SetActive(showMarkers);
            }
        }

        private void CreateCraftFurnitureTask()
        {
            // If there are no material costs, build instantly
            if (RuntimeData.FurnitureSettings.CraftRequirements.MaterialCosts.Count == 0)
            {
                SetState(EFurnitureState.Built);
                return;
            }

            Task task = new Task("Craft Furniture", ETaskType.Crafting, this, EToolType.None);
            TaskManager.Instance.AddTask(task);
        }

        public override void ReceiveItem(ItemData item)
        {
            Destroy(item.LinkedItem.gameObject);
            
            RuntimeData.RemoveFromPendingResourceCosts(item.Settings);
            RuntimeData.DeductFromMaterialCosts(item.Settings);
            
            OnChanged?.Invoke();
        }

        public bool DoCrafting(StatsData stats)
        {
            var workAmount = stats.GetActionSpeedForSkill(ESkillType.Crafting, true);
            RuntimeData.RemainingWork -= workAmount;
            if (RuntimeData.RemainingWork <= 0)
            {
                SetState(EFurnitureState.Built);
                return true;
            }
            
            OnChanged?.Invoke();
            return false;
        }
        
        public virtual bool DoDeconstruction(StatsData stats)
        {
            var workAmount = stats.GetActionSpeedForSkill(ESkillType.Crafting, true);
            RuntimeData.RemainingWork -= workAmount;
            OnChanged?.Invoke();
            if (RuntimeData.RemainingWork <= 0)
            {
                CompleteDeconstruction();
                return true;
            }
            
            return false;
        }
        
        public virtual void CompleteDeconstruction()
        {
            FurnitureManager.Instance.DeregisterFurniture(this);
                
            // Spawn All the resources used
            SpawnUsedResources(50f);
            
            // Delete this
            Destroy(gameObject);
            
            OnChanged?.Invoke();
        }

        public void PlaceFurniture(Item furnitureItem)
        {
            Destroy(furnitureItem.gameObject);
            
            SetState(EFurnitureState.Built);
        }
        
        public void DoPlacement()
        {
            // Placement is now instant
            SetState(EFurnitureState.Built);
        }
        
        protected virtual void Update()
        {
            if (_isPlanning)
            {
                FollowCursor();
                CheckPlacement();
            }
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
            bool result = Helper.IsGridPosValidToBuild(transform.position, _invalidPlacementTags);

            // Check the useage markers
            if (_useageMarkers != null && _useageMarkers.Count > 0)
            {
                bool markersPass = false;
                foreach (var marker in _useageMarkers)
                {
                    if (Helper.IsGridPosValidToBuild(marker.transform.position, _invalidPlacementTags))
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
        
        public string DisplayName => RuntimeData.ItemName;

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

        public NeedChange InUseNeedChange => RuntimeData.FurnitureSettings.InUseNeedChange;
    }
}
