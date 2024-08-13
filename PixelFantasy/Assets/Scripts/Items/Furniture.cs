using System;
using System.Collections.Generic;
using System.Linq;
using AI;
using Characters;
using CodeMonkey.Utils;
using Controllers;
using Handlers;
using Interfaces;
using Managers;
using Newtonsoft.Json;
using ScriptableObjects;
using Sirenix.OdinInspector;
using Systems.Stats.Scripts;
using TaskSystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Items
{
    public class Furniture : PlayerInteractable, IClickableObject, IConstructable
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
        public override string UniqueID => RuntimeData.UniqueID;

        protected SpriteRenderer[] _allSprites;
        protected List<SpriteRenderer> _useageMarkers;
        protected readonly List<Material> _materials = new List<Material>();
        private int _fadePropertyID;

        protected bool _isPlanning;
        protected PlacementDirection _direction;
        private bool _isOutlineLocked;
        private ClickObject _clickObject;
        private DyeSettings _dyeOverride;
        private readonly List<string> _invalidPlacementTags = new List<string>() { "Water", "Wall", "Obstacle", "Clearance"};
        
        public override string PendingTaskUID
        {
            get => RuntimeData.PendingTaskUID;
            set => RuntimeData.PendingTaskUID = value;
        }

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
            if (!_isPlanning)
            {
                FurnitureDatabase.Instance.DeregisterFurniture(RuntimeData);
                PlayerInteractableDatabase.Instance.DeregisterPlayerInteractable(this);
            }
            
            GameEvents.OnLeftClickUp -= GameEvents_OnLeftClickUp;
            GameEvents.OnRightClickUp -= GameEvents_OnRightClickUp;
        }
        
        public virtual void LoadData(FurnitureData data)
        {
            _isPlanning = false;
            RuntimeData = data;
            AssignDirection(data.Direction);
            
            SetState(data.FurnitureState);
            RefreshTaskIcon();
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
            _dyeOverride = dye;
            RuntimeData = new FurnitureData();
            RuntimeData.InitData(furnitureSettings);
            RuntimeData.Direction = direction;
            
            SetState(RuntimeData.FurnitureState);
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
                if (RuntimeData == null) return false;
                if (RuntimeData.FurnitureState != EFurnitureState.Built) return false;

                if (RuntimeData.InUse || RuntimeData.HasUseBlockingCommand)
                {
                    return false;
                }
                    
                return true;
            }
        }
        
        public override void AssignCommand(Command command)
        {
            if (command.name == "Move Furniture Command")
            {
                RuntimeData.HasUseBlockingCommand = true;
                Debug.LogError("Move Furniture Command is not built yet"); // TODO: Just as a reminder
            }
            
            if (command.name == "Deconstruct Furniture Command")
            {
                RuntimeData.HasUseBlockingCommand = true;
                if (RuntimeData.FurnitureState != EFurnitureState.Built)
                {
                    CancelConstruction();
                }
                else
                {
                    CreateTask(command);
                }
            }
            else
            {
                CreateTask(command);
            }
        }

        public override void CancelPendingTask()
        {
            RuntimeData.HasUseBlockingCommand = false;
            
            base.CancelPendingTask();
        }

        public virtual void CancelConstruction()
        {
            FurnitureDatabase.Instance.DeregisterFurniture(RuntimeData);
            
            CancelPendingTask();
                
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
            List<CostSettings> difference = new List<CostSettings>();
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
                    CostSettings refund = new CostSettings
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
                        var data = refundCost.Item.CreateItemData();
                        ItemsDatabase.Instance.CreateItemObject(data, transform.position, true);
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
            RuntimeData.FurnitureState = newState;
            switch (RuntimeData.FurnitureState)
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
            PlayerInteractableDatabase.Instance.RegisterPlayerInteractable(this);
            DisplayUseageMarkers(false);
            EnablePlacementObstacle(true);
            Show(true);
            ColourArt(ColourStates.Blueprint);
            CreateConstructionHaulingTasks();
            Commands.Add(Librarian.Instance.GetCommand("Deconstruct Furniture"));
        }
        
        protected virtual void Built_Enter()
        {
            RuntimeData.Position = transform.position;
            RuntimeData.RemainingWork = RuntimeData.FurnitureSettings.CraftRequirements.WorkCost;
            DisplayUseageMarkers(false);
            EnablePlacementObstacle(true);
            ColourArt(ColourStates.Built);

            // For when it is loaded into this state
            if (!Commands.Contains(Librarian.Instance.GetCommand("Deconstruct Furniture")))
            {
                Commands.Add(Librarian.Instance.GetCommand("Deconstruct Furniture"));
            }
            
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
        
        private void CreateConstructionHaulingTasks()
        {
            if (RuntimeData.FurnitureSettings.CraftRequirements.CostSettings.Count == 0)
            {
                SetState(EFurnitureState.Built);
                return;
            }
            
            var resourceCosts = RuntimeData.RemainingMaterialCosts;
            CreateConstuctionHaulingTasksForItems(resourceCosts);
        }
        
        public void CreateConstuctionHaulingTasksForItems(List<CostData> remainingResources)
        {
            foreach (var resourceCost in remainingResources)
            {
                for (int i = 0; i < resourceCost.Quantity; i++)
                {
                    EnqueueCreateTakeResourceToBlueprintTask(resourceCost.Item);
                }
            }
        }
        
        protected virtual void EnqueueCreateTakeResourceToBlueprintTask(ItemSettings resourceSettings)
        {
            Dictionary<string, object> taskData = new Dictionary<string, object> { { "ItemSettingsID", resourceSettings.name } };

            AI.Task task = new AI.Task("Withdraw Item For Constructable", "Gathering Materials" ,ETaskType.Hauling, this, taskData);
            TasksDatabase.Instance.AddTask(task);
        }

        public override void ReceiveItem(ItemData itemData)
        {
            RuntimeData.RemoveFromIncomingItems(itemData);
            
            Destroy(itemData.GetLinkedItem().gameObject);
            
            itemData.CarryingKinlingUID = null;
            
            RuntimeData.RemoveFromPendingResourceCosts(itemData.Settings);
            RuntimeData.DeductFromMaterialCosts(itemData.Settings);
            
            if (RuntimeData.RemainingMaterialCosts.Count == 0)
            {
                CreateConstructTask();
            }
            
            OnChanged?.Invoke();
        }

        public bool DoConstruction(StatsData stats)
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
        
        public virtual void CreateConstructTask(bool autoAssign = true)
        {
            AI.Task task = new AI.Task("Build Structure", $"Crafting {RuntimeData.ItemName}" ,ETaskType.Construction, this);
            TasksDatabase.Instance.AddTask(task);
        }
        
        public virtual void CompleteDeconstruction()
        {
            FurnitureDatabase.Instance.DeregisterFurniture(RuntimeData);
                
            // Spawn All the resources used
            SpawnUsedResources(50f);
            
            // Delete this
            Destroy(gameObject);
            
            OnChanged?.Invoke();
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
            
            if (RuntimeData.FurnitureState != EFurnitureState.Built) return;
            if(_isOutlineLocked) return;
            
            TriggerOutline(true);
        }

        private void OnMouseExit()
        {
            if (RuntimeData == null) return;
            
            if (RuntimeData.FurnitureState != EFurnitureState.Built) return;
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

        public void AddToIncomingItems(ItemData itemData)
        {
            RuntimeData.AddToIncomingItems(itemData);
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
        
        [JsonIgnore] public string DisplayName => RuntimeData.ItemName;

        public PlayerInteractable GetPlayerInteractable()
        {
            return this;
        }
        
        public Kinling GetCrafter()
        {
            if (!WasCrafted) return null;

            return KinlingsDatabase.Instance.GetKinling(RuntimeData.CraftersUID);
        }

        public bool WasCrafted => !string.IsNullOrEmpty(RuntimeData.CraftersUID);

        public NeedChange InUseNeedChange => RuntimeData.FurnitureSettings.InUseNeedChange;
    }
}
