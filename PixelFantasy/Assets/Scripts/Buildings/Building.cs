using System;
using System.Collections.Generic;
using System.Linq;
using Buildings.Building_Panels;
using Characters;
using CodeMonkey.Utils;
using Controllers;
using Items;
using Managers;
using ScriptableObjects;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Systems.Currency.Scripts;
using Systems.Notifications.Scripts;
using TaskSystem;
using UnityEngine;
using UnityEngine.Rendering;

namespace Buildings
{
    public enum BuildingType
    {
        Home = 0,
        Production = 1,
        Stockpile = 2,
        TownHall = 3,
        Crafting = 4,
    }

    public interface IBuilding
    {
        public int GetStorageUsedForCategory(EItemCategory category);
        public int GetMaxStorageForCategory(EItemCategory category);
        public int AmountItemStored(ItemData itemData);
        public List<ItemAmount> GetStoredItemsByCategory(EItemCategory category);
        public ChairFurniture FindAvailableChair();
        public Vector2 GetRandomIndoorsPosition(Unit kinling);
        public List<Storage> GetBuildingStorages();
        public Storage FindBuildingStorage(ItemData itemData);
        public JobData GetBuildingJob();
    }
    
    public abstract class Building : Construction
    {
        public string BuildingID;
        public abstract BuildingType BuildingType { get; }
        
        [SerializeField] protected BuildingData _buildingData;
        [SerializeField] private Footings _footings;
        [SerializeField] private GameObject _constructionFence;
        [SerializeField] private GameObject _internalHandle;
        [SerializeField] private GameObject _exteriorHandle;
        [SerializeField] private DoorOpener _doorOpener;
        [SerializeField] private GameObject _placementObstacle;
        [SerializeField] private GameObject _obstaclesHandle;
        [SerializeField] private GameObject _shadowboxHandle;
        [SerializeField] private Transform _constructionStandPos;
        [SerializeField] private BuildingInteriorDetector _buildingInteriorDetector;
        [SerializeField] private GameObject _furnitureParentHandle;
        [SerializeField] private GameObject _exteriorElementsHandle;
        [SerializeField] private GameObject _exteriorFurnitureParentHandle;
        [SerializeField] protected BuildingNotification _buildingNotification;
        [SerializeField] protected BuildingAnimator _animator;

        [TitleGroup("Layering")] [SerializeField] private float _buildingDepth;
        [TitleGroup("Layering")] [SerializeField] private SortingGroup _exteriorSortingGroup;
        [TitleGroup("Layering")] [SerializeField] private SpriteRenderer _interiorSortingGroup;
        [TitleGroup("Layering")] [SerializeField] private SpriteRenderer _interiorForegroundSortingGroup;
        [TitleGroup("Layering")] [SerializeField] private SortingGroup _doorSortingGroup;
        public const float _exteriorOffset = 0.25f;
        public const float _interiorOffset = 0.5f;
        public const float _interiorForegroundOffset = 0.3f;
        public const float _doorOffset = 0;

        public BuildingData BuildingData => _buildingData;
        public BuildingState State => _state;

        protected BuildingState _state;
        private string _buildingName;
        protected bool _defaultToInternalView;
        private bool _beingMoved;
        private bool _repairsRequested;
        private bool _haulingTasksCreated;

        protected List<BuildingNote> _buildingNotes = new List<BuildingNote>();
        protected List<Unit> _occupants = new List<Unit>();
        protected List<Furniture> _allFurniture = new List<Furniture>();
        protected List<InventoryLogisticBill> _logisticsBills = new List<InventoryLogisticBill>();
        
        private float _logiCheckTimer;

        public List<InventoryLogisticBill> LogisticBills => _logisticsBills;
        public List<Furniture> AllFurniture => _allFurniture;
        public Action<List<Furniture>> OnBuildingFurnitureChanged;
        public Action OnBuildingPlaced;
        public float CurrentDurability { get; set; }

        // Furniture
        private bool _showCraftableFurniture;
        private bool _isDetailsOpen;

        [Button("Update Layering")]
        public void UpdateLayering()
        {
            var yPos = transform.position.y;
            var buildingDepthOffset = yPos + _buildingDepth;
            var exteriorOffset = 0f - (yPos + _exteriorOffset) * 10;
            var interiorOffset = 0f - (buildingDepthOffset + _interiorOffset) * 10;
            var interiorForegroundOffset = 0f - (yPos + _interiorForegroundOffset) * 10;
            var doorOffset = 0f - (yPos + _doorOffset) * 10;

            _exteriorSortingGroup.sortingOrder = (int)exteriorOffset;
            _interiorSortingGroup.sortingOrder = (int)interiorOffset;
            _interiorForegroundSortingGroup.sortingOrder = (int)interiorForegroundOffset;
            _doorSortingGroup.sortingOrder = (int)doorOffset;
        }
        
        public int GetStorageUsedForCategory(EItemCategory category)
        {
            var allStorage = GetBuildingStorages();
            int result = 0;
            foreach (var storage in allStorage)
            {
                if (storage.AcceptedCategories.Contains(category))
                {
                    result += storage.UsedStorage;
                }
            }

            return result;
        }

        public int GetMaxStorageForCategory(EItemCategory category)
        {
            var allStorage = GetBuildingStorages();
            int result = 0;
            foreach (var storage in allStorage)
            {
                if (storage.AcceptedCategories.Contains(category))
                {
                    result += storage.MaxStorage;
                }
            }

            return result;
        }
        
        public int AmountItemStored(ItemData itemData)
        {
            var allQuantities = GetBuildingInventoryQuantities();
            if (allQuantities.TryGetValue(itemData, out var stored))
            {
                return stored;
            }
            else
            {
                return 0;
            }
        }
        
        public List<ItemAmount> GetStoredItemsByCategory(EItemCategory category)
        {
            List<ItemAmount> results = new List<ItemAmount>();
            var allQuantities = GetBuildingInventoryQuantities();
            foreach (var kvp in allQuantities)
            {
                if (kvp.Key.Category == category)
                {
                    var storedResult = results.Find(i => i.Item == kvp.Key);
                    if (storedResult == null)
                    {
                        storedResult = new ItemAmount
                        {
                            Item = kvp.Key,
                            Quantity = kvp.Value
                        };
                        results.Add(storedResult);
                    }
                    else
                    {
                        storedResult.Quantity += kvp.Value;
                    }
                }
            }

            return results;
        }

        public bool IsColliderInInterior(Collider2D colliderToCheck)
        {
            return _buildingInteriorDetector.IsColliderInInterior(colliderToCheck);
        }
        
        public bool IsPositionInInterior(Vector2 pos) {
            return _buildingInteriorDetector.IsPositionInInterior(pos);
        }

        public void TriggerPlaced()
        {
            if (OnBuildingPlaced != null)
            {
                OnBuildingPlaced.Invoke();
            }
        }
        
        public void CheckLogistics()
        {
            foreach (var bill in _logisticsBills)
            {
                bill.CheckBill(this);
            }
        }
        
        public void AddLogisticBill(InventoryLogisticBill newBill)
        {
            _logisticsBills.Add(newBill);
        }

        public void RemoveLogisticBill(InventoryLogisticBill billToRemove)
        {
            if (!_logisticsBills.Contains(billToRemove))
            {
                Debug.LogError($"Tried to remove a not existing bill: {billToRemove}");
                return;
            }
            else
            {
                _logisticsBills.Remove(billToRemove);
            }
        }

        public void UpdateLogisticBill(InventoryLogisticBill originalBill, InventoryLogisticBill newBill)
        {
            for (int i = 0; i < _logisticsBills.Count; i++)
            {
                if (_logisticsBills[i].Item == originalBill.Item &&
                    _logisticsBills[i].Type == originalBill.Type &&
                    _logisticsBills[i].Value == originalBill.Value &&
                    _logisticsBills[i].Building == originalBill.Building)
                {
                    _logisticsBills[i] = newBill;
                    return;
                }
            }
            
            Debug.LogError($"Tried to update an not existing bill: {originalBill}, created instead");
            AddLogisticBill(newBill);
        }

        private void AssignFurnitureToBuilding()
        {
            if (_furnitureParentHandle != null)
            {
                var interiorFurniture = _furnitureParentHandle.GetComponentsInChildren<Furniture>();
                foreach (var furniture in interiorFurniture)
                {
                    furniture.AssignBuilding(this);
                }
            }

            if (_exteriorFurnitureParentHandle != null)
            {
                var exteriorFurniture = _exteriorFurnitureParentHandle.GetComponentsInChildren<Furniture>();
                foreach (var furniture in exteriorFurniture)
                {
                    furniture.AssignBuilding(this);
                }
            }
        }

        public void EnableFurniture(bool isEnabled)
        {
            _furnitureParentHandle.SetActive(isEnabled);
        }

        public bool ToggleShowCraftableFurniture()
        {
            _showCraftableFurniture = !_showCraftableFurniture;
            foreach (var furniture in _allFurniture)
            {
                furniture.ShowCraftable(_showCraftableFurniture);
            }

            return _showCraftableFurniture;
        }

        private void OrderCraftableFurniture()
        {
            foreach (var furniture in _allFurniture)
            {
                furniture.Order();
            }
        }
        
        public bool ShowCraftableFurniture()
        {
            _showCraftableFurniture = true;
            foreach (var furniture in _allFurniture)
            {
                furniture.ShowCraftable(_showCraftableFurniture);
            }

            return _showCraftableFurniture;
        }
        
        public void RegisterFurniture(Furniture furniture)
        {
            if (_allFurniture.Contains(furniture))
            {
                Debug.LogError($"Attempted to register already registered furniture: {furniture.FurnitureItemData.ItemName}");
                return;
            }
            
            _allFurniture.Add(furniture);
            
            if(OnBuildingFurnitureChanged != null)
                OnBuildingFurnitureChanged.Invoke(_allFurniture);
        }

        public void DeregisterFurniture(Furniture furniture)
        {
            if (!_allFurniture.Contains(furniture))
            {
                Debug.LogError($"Attempted to deregister not registered furniture: {furniture.FurnitureItemData.ItemName}");
                return;
            }
            
            _allFurniture.Remove(furniture);
            
            if(OnBuildingFurnitureChanged != null)
                OnBuildingFurnitureChanged.Invoke(_allFurniture);
        }

        private void DisplayExteriorElements(bool shouldDisplay)
        {
            if (_exteriorElementsHandle != null)
            {
                _exteriorElementsHandle.SetActive(shouldDisplay);
            }
        }

        public Furniture GetAvailableFurniture(FurnitureItemData furnitureItemData)
        {
            return _allFurniture.FirstOrDefault(f => f.FurnitureItemData == furnitureItemData && f.IsAvailable);
        }

        public bool ContainsCraftingTableForItem(CraftedItemData item)
        {
            var tables = CraftingTables;
            foreach (var table in tables)
            {
                if (item.IsCraftingTableValid(table.FurnitureItemData))
                {
                    return true;
                }
            }

            return false;
        }

        public CraftingTable GetCraftingTableForItem(CraftedItemData item)
        {
            var tables = CraftingTables;
            foreach (var table in tables)
            {
                if (item.IsCraftingTableValid(table.FurnitureItemData))
                {
                    return table;
                }
            }

            return null;
        }

        public List<Storage> GetBuildingStorages()
        {
            List<Storage> results = new List<Storage>();
            foreach (var furniture in _allFurniture)
            {
                Storage storage = furniture as Storage;
                if (storage != null)
                {
                    if (storage.IsAvailable)
                    {
                        results.Add(storage);
                    }
                }
            }

            return results;
        }
        
        public Storage FindBuildingStorage(ItemData itemData)
        {
            foreach (var storage in GetBuildingStorages())
            {
                if(storage.AmountCanBeDeposited(itemData) > 0)
                {
                    return storage;
                }
            }

            return null;
        }

        public Dictionary<ItemData, List<Item>> GetBuildingInventory()
        {
            Dictionary<ItemData, List<Item>> results = new Dictionary<ItemData, List<Item>>();
            var storages = GetBuildingStorages();
            foreach (var storage in storages)
            {
                var storedItems = storage.AvailableInventory;
                foreach (var storedKVP in storedItems)
                {
                    if (!results.ContainsKey(storedKVP.Key))
                    {
                        results.Add(storedKVP.Key, new List<Item>());
                    }

                    foreach (var item in storedKVP.Value)
                    {
                        results[storedKVP.Key].Add(item);
                    }
                }
            }

            return results;
        }
        
        public Dictionary<ItemData, int> GetBuildingInventoryQuantities()
        {
            Dictionary<ItemData, int> results = new Dictionary<ItemData, int>();
            var availableInventory = GetBuildingInventory();
            foreach (var availKVP in availableInventory)
            {
                if (!results.ContainsKey(availKVP.Key))
                {
                    results.Add(availKVP.Key, availKVP.Value.Count);
                }
                else
                {
                    results[availKVP.Key] += availKVP.Value.Count;
                }
            }

            return results;
        }

        public virtual string OccupantAdjective => "Occupants";

        public virtual List<Unit> GetPotentialOccupants()
        {
            return UnitsManager.Instance.HomelessKinlings;
        }

        public List<Unit> GetOccupants()
        {
            return _occupants;
        }

        public virtual void AddOccupant(Unit unit)
        {
            _occupants.Add(unit);
            
            unit.AssignedWorkplace = this;
        }

        public virtual void RemoveOccupant(Unit unit)
        {
            _occupants.Remove(unit);
            
            unit.AssignedWorkplace = null;
        }
        
        public List<BuildingNote> BuildingNotes => _buildingNotes;

        public string BuildingName
        {
            get
            {
                if (_buildingName.IsNullOrWhitespace())
                {
                    _buildingName = _buildingData.ConstructionName;
                }

                return _buildingName;
            }
            set => _buildingName = value;
        }

        protected override void Awake()
        {
            base.Awake();
            _defaultToInternalView = BuildingsManager.Instance.ShowInteriorByDefault;
            _doorOpener = GetComponentInChildren<DoorOpener>(true);
            GameEvents.OnHideRoofsToggled += GameEvents_OnHideRoofsToggled;
            GameEvents.MinuteTick += GameEvents_MinuteTick;
            EnableFurniture(false);
        }

        protected virtual void Start()
        {
            TryToggleInternalView(false);
            CurrentDurability = _buildingData.MaxDurability;
            IncludeDefaultLogistics();
            AssignFurnitureToBuilding();
            
            if (_state != BuildingState.BeingPlaced)
            {
                BuildingsManager.Instance.RegisterBuilding(this);
            }
        }

        protected virtual void OnDestroy()
        {
            GameEvents.OnHideRoofsToggled -= GameEvents_OnHideRoofsToggled;
            GameEvents.MinuteTick -= GameEvents_MinuteTick;

            if (_state != BuildingState.BeingPlaced)
            {
                BuildingsManager.Instance.DeregisterBuilding(this);
            }
        }

        protected virtual void GameEvents_MinuteTick()
        {
            CheckForIssues();
        }

        protected virtual bool CheckForIssues()
        {
            // Check for things like upgradeable furniture, etc
            bool hasIssue = false;

            return false;
        }

        private void IncludeDefaultLogistics()
        {
            var defaultLogi = _buildingData.DefaultLogistics;
            foreach (var logi in defaultLogi)
            {
                InventoryLogisticBill bill = new InventoryLogisticBill(logi.Type, logi.Item, logi.Value, this);
                AddLogisticBill(bill);
            }
        }

        public void OnBuildingClicked()
        {
            if (_state == BuildingState.BeingPlaced || IsBuildingMoving) return;

            HUDController.Instance.ShowBuildingDetails(this);
        }

        public void OnShowDetails()
        {
            _isDetailsOpen = true;
            TryToggleInternalView(true);
        }

        public void OnHideDetails()
        {
            _isDetailsOpen = false;
            TryToggleInternalView(false);
        }

        private bool _isCursorOnBuilding;
        public void OnCursorEnter()
        {
            _isCursorOnBuilding = true;
            if (_state == BuildingState.BeingPlaced) return;

            TryToggleInternalView(true);
        }

        public void OnCurserExit()
        {
            _isCursorOnBuilding = false;
            if (_state == BuildingState.Planning) return;

            TryToggleInternalView(false);
        }

        private bool IsBuildingFurnitureSelected()
        {
            foreach (var furniture in _allFurniture)
            {
                if (furniture.GetClickObject().IsSelected) return true;
            }

            return false;
        }

        protected void DisplayConstructionFence(bool displayFence)
        {
            _constructionFence.SetActive(displayFence);
        }

        public void SetState(BuildingState state)
        {
            _state = state;
            switch (state)
            {
                case BuildingState.BeingPlaced:
                    BeingPlaced_Enter();
                    break;
                case BuildingState.Planning:
                    Plan_Enter();
                    break;
                case BuildingState.Construction:
                    Construction_Enter();
                    break;
                case BuildingState.Built:
                    Built_Enter();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
            
            GameEvents.Trigger_OnBuildingChanged(this);
        }
        
        private void GameEvents_OnHideRoofsToggled(bool showInternal)
        {
            _defaultToInternalView = showInternal;
            TryToggleInternalView(showInternal);
        }
        
        protected void TryToggleInternalView(bool showInternal)
        {
            // // Cases where show always be no showing internal
            if (!IsInternalViewAllowed())
            {
                return;
            }
            
            if (!showInternal)
            {
                if (_defaultToInternalView || _isDetailsOpen || IsBuildingFurnitureSelected())
                {
                    ToggleInternalView(true);
                }
                else
                {
                    ToggleInternalView(false);
                }
            }
            else
            {
                ToggleInternalView(true);
            }
        }

        protected virtual bool IsInternalViewAllowed()
        {
            if (State is BuildingState.Built)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void ToggleInternalView(bool showInternal)
        {
            if (showInternal)
            {
                _exteriorHandle.SetActive(false);
                _shadowboxHandle.SetActive(true);
            }
            else
            {
                _exteriorHandle.SetActive(true);
                _shadowboxHandle.SetActive(false);
            }
        }
        
        public bool CheckPlacement()
        {
            return _footings.FootingsValid(_buildingData.InvalidPlacementTags);
        }

        private void Update()
        {
            if (_state == BuildingState.BeingPlaced)
            {
                FollowCursor();
                CheckPlacement();
                UpdateLayering();
            }

            if (_state == BuildingState.Built)
            {
                _logiCheckTimer += Time.deltaTime;
                if (_logiCheckTimer > 2)
                {
                    _logiCheckTimer = 0;
                    CheckLogistics();
                }
            }
        }

        private void BeingPlaced_Enter()
        {
            _footings.DisplayFootings(true);
            _placementObstacle.SetActive(false);
            _obstaclesHandle.SetActive(false);
            _doorOpener.LockClosed(true);
            DisplayConstructionFence(false);
            
            _doorSortingGroup.gameObject.SetActive(true);
            _internalHandle.SetActive(false);
            _shadowboxHandle.SetActive(false);
            _exteriorHandle.SetActive(true);

            DisplayExteriorElements(true);
        }

        private void Plan_Enter()
        {
            ColourSprites(Librarian.Instance.GetColour("Planning Transparent"));
            TryToggleInternalView(false);
            BuildingsManager.Instance.RegisterBuilding(this);
            _remainingResourceCosts = new List<ItemAmount> (_buildingData.GetResourceCosts());
            _remainingWork = GetWorkAmount();
            HUDController.Instance.ShowBuildingDetails(this, true);
            DisplayConstructionFence(false);
            _placementObstacle.SetActive(true);
            
            _doorSortingGroup.gameObject.SetActive(true);
            _internalHandle.SetActive(false);
            _shadowboxHandle.SetActive(false);
            _exteriorHandle.SetActive(true);

            DisplayExteriorElements(true);
        }

        private void Construction_Enter()
        {
            _footings.DisplayFootings(false);
            _doorOpener.LockClosed(true);
            ColourSprites(Librarian.Instance.GetColour("Blueprint"));
            ClearAreaForConstruction();
            DisplayConstructionFence(true);
            _placementObstacle.SetActive(true);
            
            _doorSortingGroup.gameObject.SetActive(false);
            _internalHandle.SetActive(false);
            _shadowboxHandle.SetActive(false);
            _exteriorHandle.SetActive(false);

            DisplayExteriorElements(false);
        }

        public override void CompleteConstruction()
        {
            base.CompleteConstruction();
            SetState(BuildingState.Built);
            Changed();
        }

        protected virtual void Built_Enter()
        {
            ColourSprites(Color.white);
            DisplayConstructionFence(false);
            EnableFurniture(true);
            ShowCraftableFurniture();
            OrderCraftableFurniture();
            _obstaclesHandle.SetActive(true);
            _placementObstacle.SetActive(true);
            _doorOpener.LockClosed(false);
            GameEvents.Trigger_OnCoinsIncomeChanged();
            
            _internalHandle.SetActive(true);
            _doorSortingGroup.gameObject.SetActive(true);
            DisplayExteriorElements(true);
            
            TryToggleInternalView(_defaultToInternalView);

            foreach (var furniture in _allFurniture)
            {
                if (furniture.FurnitureState == Furniture.EFurnitureState.Built)
                {
                    furniture.AssignState(Furniture.EFurnitureState.Built);
                }
                
                if (furniture.FurnitureState == Furniture.EFurnitureState.Craftable)
                {
                    furniture.SetState(Furniture.EFurnitureState.InProduction);
                }
            }
        }
        
        private void FollowCursor()
        {
            var cursorPos = Helper.ConvertMousePosToGridPos(UtilsClass.GetMouseWorldPosition());
            gameObject.transform.position = cursorPos;
        }

        public override float GetWorkAmount()
        {
            return _buildingData.WorkCost;
        }

        private void ColourSprites(Color colour)
        {
            var internalSpriteRenderers = _internalHandle.GetComponentsInChildren<SpriteRenderer>(true);
            var exteriorlSpriteRenderers = _exteriorHandle.GetComponentsInChildren<SpriteRenderer>(true);
                    
            var doorSpriteRenderer = _doorOpener.GetComponent<SpriteRenderer>();
            
            foreach (var rend in internalSpriteRenderers)
            {
                rend.color = colour;
            }
            
            foreach (var rend in exteriorlSpriteRenderers)
            {
                rend.color = colour;
            }

            doorSpriteRenderer.color = colour;
        }

        private void CheckIfAreaIsClear()
        {
            var clearables = _footings.GetClearbleResourcesInFootingsArea();
            var items = _footings.GetItemsInFootingArea();
            foreach (var itemToRelocate in items)
            {
                itemToRelocate.RelocateItem(CheckIfAreaIsClear, _constructionStandPos.position);
            }
            
            if (clearables.Count == 0 && items.Count == 0)
            {
                var notClear = _buildingNotes.Find(note => note.ID == "Area Not Clear");
                if (notClear != null)
                {
                    _buildingNotes.Remove(notClear);
                }

                if (!_haulingTasksCreated)
                {
                    _haulingTasksCreated = true;
                    CreateConstructionHaulingTasks();
                }
            }
            else
            {
                var notClear = _buildingNotes.Find(note => note.ID == "Area Not Clear");
                if (notClear == null)
                {
                    _buildingNotes.Add(new BuildingNote("Construction area is not clear", false, "Area Not Clear"));
                }
            }
        }

        private void ClearAreaForConstruction()
        {
            // Get a list of all the items in the footings area
            var clearables = _footings.GetClearbleResourcesInFootingsArea();
            foreach (var resourceToClear in clearables)
            {
                resourceToClear.ClearResource(CheckIfAreaIsClear);
            }
            
            CheckIfAreaIsClear();
        }
        
        private void CreateConstructionHaulingTasks()
        {
             var resourceCosts = _buildingData.GetResourceCosts();
             CreateConstuctionHaulingTasksForItems(resourceCosts);
        }

        protected override void EnqueueCreateTakeResourceToBlueprintTask(ItemData resourceData)
        {
            Task task = new Task("Withdraw Item Construction", this, Librarian.Instance.GetJob("Worker"), EToolType.None)
            {
                Payload = resourceData.ItemName,
            };
            TaskManager.Instance.AddTask(task);
        }

        public override void CreateConstructTask(bool autoAssign = true)
        {
            Task constuctTask = new Task("Build Building", this, Librarian.Instance.GetJob("Worker"), EToolType.BuildersHammer);
            constuctTask.Enqueue();
        }
        
        public override Vector2? UseagePosition(Vector2 requestorPosition)
        {
            return _constructionStandPos.position;
        }
        
        public int DailyUpkeep()
        {
            if (_state == BuildingState.Built)
            {
                return _buildingData.DailyUpkeep;
            }
            else
            {
                return 0;
            }
        }

        public bool AreBuildRequirementsMet
        {
            get
            {
                // TODO: Check furniture, zones...
                return true;
            }
        }
        
        public void ToggleMoveBuilding()
        {
            _beingMoved = !_beingMoved;
        }

        public bool IsBuildingMoving => _beingMoved;

        public void ToggleDeconstruct()
        {
            if (_isDeconstructing)
            {
                // Cancel Deconstruct
                _isDeconstructing = false;
            }
            else
            {
                // Begin Deconstruct
                _isDeconstructing = true;
            }
        }

        public void CancelBuilding()
        {
            if (_state == BuildingState.Built)
            {
                Debug.LogError("Should be able to cancel built building");
                return;
            }

            if (_state is BuildingState.Planning or BuildingState.BeingPlaced)
            {
                // Nothing Spent, just cancel
                HUDController.Instance.HideDetails();
                Destroy(gameObject);
                return;
            }

            if (_state == BuildingState.Construction)
            {
                // Refund, then cancel
                var price = _buildingData.Price;
                CurrencyManager.Instance.AddCoins(price);
                
                HUDController.Instance.HideDetails();
                Destroy(gameObject);
                return;
            }
        }
        
        public void RequestRepairs()
        {
            if (!_repairsRequested)
            {
                // TODO: Make a repair task
            }
            
            _repairsRequested = true;
        }

        public bool RepairsRequested => _repairsRequested;

        public bool SetBuild()
        {
            var price = _buildingData.Price;
            if (CurrencyManager.Instance.RemoveCoins(price))
            {
                SetState(BuildingState.Construction);
                return true;
            }
            else
            {
                return false;
            }
        }

        protected override void Changed()
        {
            GameEvents.Trigger_OnBuildingChanged(this);
        }

        public enum BuildingState
        {
            BeingPlaced,
            Planning,
            Construction,
            Built,
        }

        public class BuildingNote
        {
            public string Note;
            public bool IsPositive;
            public string ID;

            public BuildingNote(string note, bool isPositive, string id)
            {
                Note = note;
                IsPositive = isPositive;
                ID = id;
            }
        }

        public virtual Task GetBuildingTask()
        {
            return null;
        }

        public virtual JobData GetBuildingJob()
        {
            return Librarian.Instance.GetJob("Worker");
        }

        public ChairFurniture FindAvailableChair()
        {
            var allChairs = _allFurniture.OfType<ChairFurniture>().Where(seat =>
                seat.IsAvailable && seat.CanKinlingUseThis()).OrderBy(seat => seat.HasTable).ToList();

            if (allChairs.Count == 0)
            {
                return null;
            }

            return allChairs[0];
        }

        public List<CraftingTable> CraftingTables
        {
            get
            {
                return _allFurniture.OfType<CraftingTable>()
                    .Where(table => table.FurnitureState == Furniture.EFurnitureState.Built).ToList();
            }
        }

        public Vector2 GetRandomIndoorsPosition(Unit kinling)
        {
            Vector2? potentialPosition = _buildingInteriorDetector.GetRandomInteriorPosition(kinling);

            if (potentialPosition == null)
            {
                Debug.LogError($"Attempted to find a random interior position in {_buildingName}, but failed");
                return _constructionStandPos.position;
            }

            return (Vector2)potentialPosition;
        }
    }
}
