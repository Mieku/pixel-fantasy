using System;
using System.Collections.Generic;
using System.Linq;
using AI;
using Characters;
using CodeMonkey.Utils;
using Handlers;
using Interfaces;
using Managers;
using Newtonsoft.Json;
using ScriptableObjects;
using Sirenix.OdinInspector;
using Systems.Input_Management;
using Systems.Lighting.Scripts;
using Systems.Stats.Scripts;
using UnityEngine;

namespace Items
{
    public class Furniture : PlayerInteractable, IConstructable
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
        protected override List<SpriteRenderer> SpritesToOutline => _allSprites.ToList();

        protected SpriteRenderer[] _allSprites;
        protected List<SpriteRenderer> _useageMarkers;
        protected readonly List<Material> _materials = new List<Material>();
        private int _fadePropertyID;

        protected bool _isPlanning;
        protected PlacementDirection _direction;
        private bool _isOutlineLocked;
        private DyeSettings _dyeOverride;
        private readonly List<string> _invalidPlacementTags = new List<string>() { "Water", "Wall", "Obstacle", "Clearance"};
        protected List<LightSource> _lightSources = new List<LightSource>();
        
        public override string PendingTaskUID
        {
            get => RuntimeData.PendingTaskUID;
            set => RuntimeData.PendingTaskUID = value;
        }

        public override bool IsSimilar(PlayerInteractable otherPI)
        {
            if (otherPI is Furniture furniture)
            {
                return RuntimeData.FurnitureSettings == furniture.RuntimeData.FurnitureSettings;
            }

            return false;
        }

        protected virtual void Awake()
        {
            _fadePropertyID = Shader.PropertyToID("_OuterOutlineFade");
            _lightSources = GetComponentsInChildren<LightSource>(true).ToList();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            if (!_isPlanning)
            {
                FurnitureDatabase.Instance.DeregisterFurniture(RuntimeData);
                PlayerInteractableDatabase.Instance.DeregisterPlayerInteractable(this);
            }

            CancelRequesterTasks(false);
        }
        
        public virtual void LoadData(FurnitureData data)
        {
            _isPlanning = false;
            RuntimeData = data;
            AssignDirection(data.Direction);
            
            SetState(data.FurnitureState);
            RefreshTaskIcon();
            
            RefreshAllowedDisplay();
            RefreshAllowCommands();
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
            EnableLights(false);
        }
        
        public virtual void CompletePlanning()
        {
            _isPlanning = false;
            
            RefreshAllowedDisplay();
            RefreshAllowCommands();
            
            CreateConstructionHaulingTasks();
            
            InformChanged();
        }

        public virtual void InitializeFurniture(FurnitureSettings furnitureSettings, PlacementDirection direction, DyeSettings dye)
        {
            _dyeOverride = dye;
            RuntimeData = new FurnitureData();
            RuntimeData.InitData(furnitureSettings);
            RuntimeData.Direction = direction;
            
            IsAllowed = true;
            RefreshAllowedDisplay();
            RefreshAllowCommands();
            
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

                if (RuntimeData.InUse || RuntimeData.HasUseBlockingCommand || !RuntimeData.IsAllowed)
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
                if (command.CommandID == "Forbid")
                {
                    CancelPendingTask();
                    IsAllowed = false;
                }
                else if (command.CommandID == "Allow")
                {
                    IsAllowed = true;
                }
                else
                {
                    CreateTask(command);
                }
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

            var spawnPos = Helper.SnapToGridPos(transform.position);
            foreach (var refundCost in difference)
            {
                for (int i = 0; i < refundCost.Quantity; i++)
                {
                    if (Helper.RollDice(percentReturned))
                    {
                        var data = refundCost.Item.CreateItemData(spawnPos);
                        ItemsDatabase.Instance.CreateItemObject(data, spawnPos);
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
            }
            else
            {
                DisplaySprites(false);
            }
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
            
            InformChanged();
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
                bool result = !Helper.IsTagAtPosition(useageMarker.position, "Obstacle");

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

            // if for some reason there is no remaining position, log an error but also just provide the furniture's transform position
            if (distances.Count == 0)
            {
                Debug.LogError($"Could not find a possible position for {gameObject.name}");
                return transform.position;
            }
            
            // Compile the positions that pass the above tests and sort them by distance
            var sortedDistances = distances.OrderBy(x => x.Item2).Select(x => x.Item1).ToList();
            var selectedDistance = sortedDistances[0];
            return selectedDistance.position;
        }

        protected void InProduction_Enter()
        {
            RuntimeData.Position = transform.position;
            PlayerInteractableDatabase.Instance.RegisterPlayerInteractable(this);
            DisplayUseageMarkers(false);
            EnablePlacementObstacle(true);
            Show(true);
            ColourArt(ColourStates.Blueprint);
            AddCommand("Deconstruct Furniture");

            EnableLights(false);
        }
        
        protected virtual void Built_Enter()
        {
            RuntimeData.Position = transform.position;
            RuntimeData.RemainingWork = RuntimeData.FurnitureSettings.CraftRequirements.WorkCost;
            DisplayUseageMarkers(false);
            EnablePlacementObstacle(true);
            ColourArt(ColourStates.Built);
            EnableLights(true);

            AddCommand("Deconstruct Furniture");
            AddCommand("Move Furniture");
            
            RefreshAllowCommands();
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
            
            var resourceCosts = RuntimeData.FurnitureSettings.CraftRequirements.GetMaterialCosts();
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
        
        protected void EnqueueCreateTakeResourceToBlueprintTask(ItemSettings resourceSettings)
        {
            Dictionary<string, object> taskData = new Dictionary<string, object> { { "ItemSettingsID", resourceSettings.name } };

            Task task = new Task("Withdraw Item For Constructable", "Gathering Materials" ,ETaskType.Hauling, this, taskData);
            TasksDatabase.Instance.AddTask(task);
        }

        public override void ReceiveItem(ItemData itemData)
        {
            RuntimeData.RemoveFromIncomingItems(itemData);

            var item = (ItemStack) itemData.GetLinkedItem();
            Destroy(item.gameObject);
            
            itemData.CarryingKinlingUID = null;
            
            RuntimeData.RemoveFromPendingResourceCosts(itemData.Settings);
            RuntimeData.DeductFromMaterialCosts(itemData.Settings);
            
            if (RuntimeData.HasAllMaterials())
            {
                CreateConstructTask();
            }
            
            InformChanged();
        }

        public bool DoConstruction(StatsData stats, out float progress)
        {
            var workAmount = stats.GetActionSpeedForSkill(ESkillType.Crafting, true);
            RuntimeData.RemainingWork -= workAmount;
            
            if (RuntimeData.RemainingWork <= 0)
            {
                SetState(EFurnitureState.Built);
                progress = 1;
                return true;
            }
            else
            {
                progress = RuntimeData.ConstructionPercent;
                InformChanged();
                return false;
            }
        }
        
        public virtual bool DoDeconstruction(StatsData stats, out float progress)
        {
            var workAmount = stats.GetActionSpeedForSkill(ESkillType.Crafting, true);
            RuntimeData.RemainingWork -= workAmount;
            InformChanged();
            
            if (RuntimeData.RemainingWork <= 0)
            {
                CompleteDeconstruction();
                progress = 1;
                return true;
            }
            else
            {
                progress = RuntimeData.ConstructionPercent;
                return false;
            }
        }
        
        public virtual void CreateConstructTask(bool autoAssign = true)
        {
            Task task = new Task("Build Structure", $"Crafting {RuntimeData.ItemName}" ,ETaskType.Construction, this);
            TasksDatabase.Instance.AddTask(task);
        }
        
        public virtual void CompleteDeconstruction()
        {
            FurnitureDatabase.Instance.DeregisterFurniture(RuntimeData);
                
            // Spawn the resources used
            SpawnUsedResources(50f);
            
            InformChanged();
            
            // Delete this
            Destroy(gameObject);
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
            bool result = Helper.IsGridPosValidToBuild(transform.position, _invalidPlacementTags, null, gameObject);

            // Check the useage markers
            if (_useageMarkers != null && _useageMarkers.Count > 0)
            {
                bool markersPass = false;
                foreach (var marker in _useageMarkers)
                {
                    if (Helper.IsGridPosValidToBuild(marker.transform.position, _invalidPlacementTags, null, gameObject))
                    {
                        marker.color = Color.white;
                        markersPass = true;
                        break;
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
                else
                {
                    result = true;
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
        
        [JsonIgnore] public override string DisplayName => RuntimeData.ItemName;
        
        public Kinling GetCrafter()
        {
            if (!WasCrafted) return null;

            return KinlingsDatabase.Instance.GetKinling(RuntimeData.CraftersUID);
        }

        public bool WasCrafted => !string.IsNullOrEmpty(RuntimeData.CraftersUID);

        public NeedChange InUseNeedChange => RuntimeData.FurnitureSettings.InUseNeedChange;

        public void EnableLights(bool showLights)
        {
            foreach (var lightSource in _lightSources)
            {
                lightSource.SetLightOn(showLights);
            }
        }
        
        public bool IsAllowed
        {
            get => RuntimeData.IsAllowed;
            set
            {
                RuntimeData.IsAllowed = value;

                if (value == false)
                {
                    // Cancel all tasks
                    CancelPendingTask();
                    CancelRequesterTasks(true);
                }
                
                RefreshAllowCommands();
                RefreshAllowedDisplay();
                InformChanged();
            }
        }

        protected void RefreshAllowedDisplay()
        {
            if (!IsAllowed)
            {
                var forbidCmd = GameSettings.Instance.LoadCommand("Forbid");
                AssignTaskIcon(forbidCmd);
            }
            else
            {
                AssignTaskIcon(null);
            }
        }
        
        protected void RefreshAllowCommands()
        {
            if (RuntimeData.FurnitureState == EFurnitureState.InProduction)
            {
                if (IsAllowed)
                {
                    AddCommand("Forbid", true);
                    RemoveCommand("Allow");
                }
                else
                {
                    AddCommand("Allow", true);
                    RemoveCommand("Forbid");
                }
            }
            else
            {
                RemoveCommand("Allow");
                RemoveCommand("Forbid");
            }
        }

        public override bool IsForbidden()
        {
            return !IsAllowed;
        }
    }
}
