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
using Systems.Build_Controls.Scripts;
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
        [TitleGroup("South"), SerializeField] protected PlacementIndicators _southPlacementIndicators;
        
        [TitleGroup("West")] [SerializeField] protected GameObject _westHandle;
        [TitleGroup("West")] [SerializeField] protected Transform _westSpritesRoot;
        [TitleGroup("West"), SerializeField] protected PlacementIndicators _westPlacementIndicators;
        
        [TitleGroup("North")] [SerializeField] protected GameObject _northHandle;
        [TitleGroup("North")] [SerializeField] protected Transform _northSpritesRoot;
        [TitleGroup("North"), SerializeField] protected PlacementIndicators _northPlacementIndicators;
        
        [TitleGroup("East")] [SerializeField] protected GameObject _eastHandle;
        [TitleGroup("East")] [SerializeField] protected Transform _eastSpritesRoot;
        [TitleGroup("East"), SerializeField] protected PlacementIndicators _eastPlacementIndicators;
        
        public FurnitureData RuntimeData;
        public override string UniqueID => RuntimeData.UniqueID;
        protected override List<SpriteRenderer> SpritesToOutline => _allSprites.ToList();
        protected SpriteRenderer[] _allSprites;
        protected bool _isPlanning;
        protected PlacementDirection _direction;
        private DyeSettings _dyeOverride;
        protected List<LightSource> _lightSources = new List<LightSource>();

        protected PlacementIndicators _indicators
        {
            get
            {
                switch (_direction)
                {
                    case PlacementDirection.South:
                        return _southPlacementIndicators;
                    case PlacementDirection.North:
                        return _northPlacementIndicators;
                    case PlacementDirection.West:
                        return _westPlacementIndicators;
                    case PlacementDirection.East:
                        return _eastPlacementIndicators;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        
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
                    break;
                case PlacementDirection.North:
                    if (_northHandle == null) return false;
                    _northHandle.SetActive(true);
                    _allSprites = _northSpritesRoot.GetComponentsInChildren<SpriteRenderer>(true);
                    break;
                case PlacementDirection.West:
                    if (_westHandle == null) return false;
                    _westHandle.SetActive(true);
                    _allSprites = _westSpritesRoot.GetComponentsInChildren<SpriteRenderer>(true);
                    break;
                case PlacementDirection.East:
                    if (_eastHandle == null) return false;
                    _eastHandle.SetActive(true);
                    _allSprites = _eastSpritesRoot.GetComponentsInChildren<SpriteRenderer>(true);
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
            _indicators.EnableObstacles(enable);
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

        public override Vector2? UseagePosition(Vector2 requestorPosition)
        {
            List<Vector2> potentialPositions = _indicators.UsePositions(transform);
            
            List<(Vector2, float)> distances = new List<(Vector2, float)>();
            foreach (var potentialPosition in potentialPositions)
            {
                var pathResult = Helper.DoesPathExist(requestorPosition, potentialPosition);
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
            return selectedDistance;
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
            _indicators.ShowUsePositions(showMarkers);
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

            var item = itemData.GetLinkedItem();
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
        
        private void FollowCursor()
        {
            var diff = (Vector2)transform.position - _indicators.PlacementPivot;
            var snapPos = Helper.SnapToGridPos(UtilsClass.GetMouseWorldPosition());
            snapPos += diff;

            gameObject.transform.position =
                snapPos;
        }
        
        public virtual bool CheckPlacement()
        {
            bool canPlace = _indicators.CheckPlacement(1, transform);

            if (canPlace)
            {
                ColourArt(ColourStates.CanPlace);
            }
            else
            {
                ColourArt(ColourStates.CantPlace);
            }
            
            return canPlace;
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

        protected override void OnSelection()
        {
            base.OnSelection();
            _indicators.ShowUsePositions(true);
        }

        protected override void OnDeselection()
        {
            base.OnDeselection();
            _indicators.ShowUsePositions(false);
        }
    }
}
