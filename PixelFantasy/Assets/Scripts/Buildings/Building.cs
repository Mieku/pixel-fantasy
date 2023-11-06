using System;
using System.Collections.Generic;
using Buildings.Building_Panels;
using Characters;
using CodeMonkey.Utils;
using Controllers;
using Items;
using Managers;
using ScriptableObjects;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using TaskSystem;
using UnityEngine;
using UnityEngine.Rendering;

namespace Buildings
{
    public class Building : Construction
    {
        public string BuildingID;
        
        [SerializeField] protected BuildingData _buildingData;
        [SerializeField] private Footings _footings;
        [SerializeField] private GameObject _internalHandle;
        [SerializeField] private GameObject _exteriorHandle;
        [SerializeField] private GameObject _roofHandle;
        [SerializeField] private DoorOpener _doorOpener;
        [SerializeField] private GameObject _floorHandle;
        [SerializeField] private GameObject _obstaclesHandle;
        [SerializeField] private GameObject _shadowboxHandle;
        [SerializeField] private Transform _constructionStandPos;
        [SerializeField] private BuildingInteriorDetector _buildingInteriorDetector;
        [SerializeField] private GameObject _furnitureParentHandle;
        
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

        private BuildingState _state;
        private string _buildingName;
        private bool _internalViewToggled;

        private List<BuildingNote> _buildingNotes = new List<BuildingNote>();
        private List<Unit> _occupants = new List<Unit>();
        private List<Furniture> _allFurniture = new List<Furniture>();
        private List<InventoryLogisticBill> _logisticsBills = new List<InventoryLogisticBill>();
        
        private float _logiCheckTimer;

        public List<InventoryLogisticBill> LogisticBills => _logisticsBills;
        public List<Furniture> AllFurniture => _allFurniture;
        public Action<List<Furniture>> OnBuildingFurnitureChanged;
        public Action OnBuildingPlaced;

        // Furniture
        private bool _showCraftableFurniture;
        private List<Furniture> _craftableFurniture = new List<Furniture>();
        private bool _isDetailsOpen;

        [Button("Update Layering")]
        public void UpdateLayering()
        {
            //_sortingGroup.sortingOrder = (int)(_sortingOrderBase - (yPos + _offset) * 10);
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

        public bool IsColliderInInterior(Collider2D colliderToCheck)
        {
            return _buildingInteriorDetector.IsColliderInInterior(colliderToCheck);
        }

        public void TriggerPlaced()
        {
            if (OnBuildingPlaced != null)
            {
                OnBuildingPlaced.Invoke();
            }
        }
        
        private void CheckLogistics()
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

        public void EnableFurniture(bool isEnabled)
        {
            _furnitureParentHandle.SetActive(isEnabled);
        }

        public void RegisterCraftableFurniture(Furniture furniture)
        {
            if (_craftableFurniture.Contains(furniture))
            {
                Debug.LogError($"Attempted to register already registered craftable furniture: {furniture.FurnitureItemData.ItemName}");
                return;
            }
            
            Debug.Log($"Craftable Furniture Registered: {furniture.FurnitureItemData.ItemName}");
            _craftableFurniture.Add(furniture);
            
            furniture.ShowCraftable(_showCraftableFurniture);
        }

        public void DeregisterCraftableFurniture(Furniture furniture)
        {
            if (!_craftableFurniture.Contains(furniture))
            {
                Debug.LogError($"Attempted to deregister not registered craftable furniture: {furniture.FurnitureItemData.ItemName}");
                return;
            }
            
            Debug.Log($"Craftable Furniture Deregistered: {furniture.FurnitureItemData.ItemName}");
            _craftableFurniture.Remove(furniture);
        }

        public bool ToggleShowCraftableFurniture()
        {
            _showCraftableFurniture = !_showCraftableFurniture;
            foreach (var craftableFurniture in _craftableFurniture)
            {
                craftableFurniture.ShowCraftable(_showCraftableFurniture);
            }

            return _showCraftableFurniture;
        }

        public void HideCraftableFurniture()
        {
            if (_showCraftableFurniture)
            {
                _showCraftableFurniture = false;
                foreach (var craftableFurniture in _craftableFurniture)
                {
                    craftableFurniture.ShowCraftable(_showCraftableFurniture);
                }
            }
        }
        
        public void RegisterFurniture(Furniture furniture)
        {
            if (_allFurniture.Contains(furniture))
            {
                Debug.LogError($"Attempted to register already registered furniture: {furniture.FurnitureItemData.ItemName}");
                return;
            }
            
            Debug.Log($"Furniture Registered: {furniture.FurnitureItemData.ItemName}");
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
            
            Debug.Log($"Furniture Deregistered: {furniture.FurnitureItemData.ItemName}");
            _allFurniture.Remove(furniture);
            
            if(OnBuildingFurnitureChanged != null)
                OnBuildingFurnitureChanged.Invoke(_allFurniture);
        }

        public List<Storage> GetBuildingStorages()
        {
            List<Storage> results = new List<Storage>();
            foreach (var furniture in _allFurniture)
            {
                Storage storage = furniture as Storage;
                if (storage != null)
                {
                    if (storage.FurnitureState == Furniture.EFurnitureState.Built)
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

        public List<Unit> GetOccupants()
        {
            return _occupants;
        }

        public void AddOccupant(Unit unit)
        {
            _occupants.Add(unit);

            if (BuildingData.WorkersJob != null)
            {
                unit.GetUnitState().ChangeJob(BuildingData.WorkersJob);
            }
        }

        public void RemoveOccupant(Unit unit)
        {
            _occupants.Remove(unit);
            
            if (BuildingData.WorkersJob != null)
            {
                unit.GetUnitState().ChangeJob(null);
            }
        }
        
        public List<BuildingNote> BuildingNotes
        {
            get
            {
                if (_buildingNotes.Count == 0)
                {
                    var notes = new List<BuildingNote>();
                    notes.Add(new BuildingNote("Everything is great!", true));
                    return notes;
                }

                return _buildingNotes;
            }
        }

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
            _doorOpener = GetComponentInChildren<DoorOpener>(true);
            GameEvents.OnHideRoofsToggled += GameEvents_OnHideRoofsToggled;
            EnableFurniture(false);
        }

        private void Start()
        {
            ToggleInternalView(false);
        }

        private void OnDestroy()
        {
            GameEvents.OnHideRoofsToggled -= GameEvents_OnHideRoofsToggled;
        }

        private void GameEvents_OnHideRoofsToggled(bool showInternal)
        {
            _internalViewToggled = showInternal;
            ToggleInternalView(showInternal);
        }

        public void OnBuildingClicked()
        {
            if (_state == BuildingState.Planning) return;

            HUDController.Instance.ShowBuildingDetails(this);
        }

        public void OnShowDetails()
        {
            _isDetailsOpen = true;
            CheckShouldToggleInternalView(true);
        }

        public void OnHideDetails()
        {
            _isDetailsOpen = false;
            CheckShouldToggleInternalView(false);
        }

        private bool _isCursorOnBuilding;
        public void OnCursorEnter()
        {
            _isCursorOnBuilding = true;
            if (_state == BuildingState.Planning) return;

            CheckShouldToggleInternalView(true);
        }

        public void OnCurserExit()
        {
            _isCursorOnBuilding = false;
            if (_state == BuildingState.Planning) return;

            CheckShouldToggleInternalView(false);
        }

        private void CheckShouldToggleInternalView(bool showInternal)
        {
            if (!_internalViewToggled)
            {
                if (!showInternal && _isDetailsOpen) return;
                if (!showInternal && _isCursorOnBuilding) return;
                if (!showInternal && IsBuildingFurnitureSelected()) return;
                
                ToggleInternalView(showInternal);
            }
        }

        private bool IsBuildingFurnitureSelected()
        {
            foreach (var furniture in _allFurniture)
            {
                if (furniture.GetClickObject().IsSelected) return true;
            }

            return false;
        }

        public void SetState(BuildingState state)
        {
            _state = state;
            switch (state)
            {
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
        }

        public void ToggleInternalView(bool showInternal)
        {
            if (showInternal)
            {
                if(_roofHandle != null) _roofHandle.SetActive(false);
                _exteriorHandle.SetActive(false);
                _shadowboxHandle.SetActive(true);
            }
            else
            {
                if(_roofHandle != null) _roofHandle.SetActive(true);
                
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
            if (_state == BuildingState.Planning)
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

        private void Plan_Enter()
        {
            _footings.DisplayFootings(true);
            _obstaclesHandle.SetActive(false);
            _doorOpener.LockClosed(true);
        }

        private void Construction_Enter()
        {
            _footings.DisplayFootings(false);
            _obstaclesHandle.SetActive(true);
            _doorOpener.LockClosed(true);
            ColourSprites(Librarian.Instance.GetColour("Blueprint"));
            
            _remainingResourceCosts = new List<ItemAmount> (_buildingData.GetResourceCosts());
            CreateConstructionHaulingTasks();
        }

        public override void CompleteConstruction()
        {
            base.CompleteConstruction();
            SetState(BuildingState.Built);
        }

        private void Built_Enter()
        {
            ColourSprites(Color.white);
            EnableFurniture(true);
            _doorOpener.LockClosed(false);
        }
        
        private void FollowCursor()
        {
            var cursorPos = Helper.ConvertMousePosToGridPos(UtilsClass.GetMouseWorldPosition());
            gameObject.transform.position = cursorPos;
        }

        /// <summary>
        /// No point keeping the sorter after the building is placed
        /// </summary>
        private void RemoveRendererSorter()
        {
            var sorter = GetComponent<PositionRendererSorter>();
            if (sorter != null)
            {
                sorter.DestroySelf();
            }
        }

        private void ColourSprites(Color colour)
        {
            var internalSpriteRenderers = _internalHandle.GetComponentsInChildren<SpriteRenderer>(true);
            var exteriorlSpriteRenderers = _exteriorHandle.GetComponentsInChildren<SpriteRenderer>(true);
            if (_roofHandle != null)
            {
                var roofSpriteRenderers = _roofHandle.GetComponentsInChildren<SpriteRenderer>(true);
                foreach (var rend in roofSpriteRenderers)
                {
                    rend.color = colour;
                }
            }
                    
            var doorSpriteRenderer = _doorOpener.GetComponent<SpriteRenderer>();

            if (_floorHandle != null)
            {
                var floorSpriteRenderers = _floorHandle.GetComponentsInChildren<SpriteRenderer>(true);
                foreach (var rend in floorSpriteRenderers)
                {
                    rend.color = colour;
                }
            }
            

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
        
        private void CreateConstructionHaulingTasks()
        {
             var resourceCosts = _buildingData.GetResourceCosts();
             CreateConstuctionHaulingTasksForItems(resourceCosts);
        }

        protected override void EnqueueCreateTakeResourceToBlueprintTask(ItemData resourceData)
        {
            Task task = new Task("Withdraw Item Construction", this)
            {
                Payload = resourceData.ItemName,
                TaskType = TaskType.Haul,
            };
            TaskManager.Instance.AddTask(task);
        }

        public override void CreateConstructTask(bool autoAssign = true)
        {
            Task constuctTask = new Task("Build Building", this);
            constuctTask.Enqueue();
        }

        public Vector2 ConstructionStandPosition()
        {
            return _constructionStandPos.position;
        }

        public float InteriorRenderOffset()
        {
            var interiorRendererSorter = _internalHandle.GetComponent<PositionRendererSorter>();
            if (interiorRendererSorter != null)
            {
                return interiorRendererSorter.Offset;
            }

            return 0f;
        }
        
        public enum BuildingState
        {
            Planning,
            Construction,
            Built,
        }

        public class BuildingNote
        {
            public string Note;
            public bool IsPositive;

            public BuildingNote(string note, bool isPositive)
            {
                Note = note;
                IsPositive = isPositive;
            }
        }
        
    }
}
