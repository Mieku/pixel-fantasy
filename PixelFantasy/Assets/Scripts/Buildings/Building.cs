using System;
using System.Collections.Generic;
using Buildings.Building_Panels;
using Characters;
using CodeMonkey.Utils;
using Controllers;
using Items;
using Managers;
using ScriptableObjects;
using Sirenix.Utilities;
using TaskSystem;
using UnityEngine;

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
        [SerializeField] private GameObject _doorHandle;
        [SerializeField] private GameObject _floorHandle;
        [SerializeField] private GameObject _obstaclesHandle;
        [SerializeField] private GameObject _shadowboxHandle;
        [SerializeField] private Transform _constructionStandPos;

        public BuildingData BuildingData => _buildingData;

        private BuildingState _state;
        private string _buildingName;

        private List<BuildingNote> _buildingNotes = new List<BuildingNote>();
        private List<Unit> _occupants = new List<Unit>();
        private List<Furniture> _allFurniture = new List<Furniture>();
        private List<InventoryLogisticBill> _logisticsBills = new List<InventoryLogisticBill>();

        private float _logiCheckTimer;

        public List<InventoryLogisticBill> LogisticBills => _logisticsBills;
        public List<Furniture> AllFurniture => _allFurniture;

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
        
        public void RegisterFurniture(Furniture furniture)
        {
            if (_allFurniture.Contains(furniture))
            {
                Debug.LogError($"Attempted to register already registered furniture: {furniture.FurnitureItemData.ItemName}");
                return;
            }
            
            _allFurniture.Add(furniture);
        }

        public void DeregisterFurniture(Furniture furniture)
        {
            if (!_allFurniture.Contains(furniture))
            {
                Debug.LogError($"Attempted to deregister not registered furniture: {furniture.FurnitureItemData.ItemName}");
                return;
            }
            
            _allFurniture.Remove(furniture);
        }

        public List<Storage> GetBuildingStorages()
        {
            List<Storage> results = new List<Storage>();
            foreach (var furniture in _allFurniture)
            {
                Storage storage = furniture as Storage;
                if (storage != null)
                {
                    if (storage.IsBuilt)
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

        public Dictionary<ItemData, List<ItemState>> GetBuildingInventory()
        {
            Dictionary<ItemData, List<ItemState>> results = new Dictionary<ItemData, List<ItemState>>();
            var storages = GetBuildingStorages();
            foreach (var storage in storages)
            {
                var storedItems = storage.AvailableInventory;
                foreach (var storedKVP in storedItems)
                {
                    if (!results.ContainsKey(storedKVP.Key))
                    {
                        results.Add(storedKVP.Key, new List<ItemState>());
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
        }

        public void RemoveOccupant(Unit unit)
        {
            _occupants.Remove(unit);
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
            GameEvents.OnHideRoofsToggled += ToggleInternalView;
        }

        private void Start()
        {
            ToggleInternalView(false);
        }

        private void OnDestroy()
        {
            GameEvents.OnHideRoofsToggled -= ToggleInternalView;
        }

        public void OnBuildingClicked()
        {
            if (_state == BuildingState.Planning) return;

            HUDController.Instance.ShowBuildingDetails(this);
        }

        public void OnCursorEnter()
        {
            if (_state == BuildingState.Planning) return;
        }

        public void OnCurserExit()
        {
            if (_state == BuildingState.Planning) return;
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
                _roofHandle.SetActive(false);
                _exteriorHandle.SetActive(false);
                _shadowboxHandle.SetActive(true);
            }
            else
            {
                _roofHandle.SetActive(true);
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
        }

        private void Construction_Enter()
        {
            _footings.DisplayFootings(false);
            _obstaclesHandle.SetActive(true);
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
            var roofSpriteRenderers = _roofHandle.GetComponentsInChildren<SpriteRenderer>(true);
            var doorSpriteRenderers = _doorHandle.GetComponentsInChildren<SpriteRenderer>(true);
            var floorSpriteRenderers = _floorHandle.GetComponentsInChildren<SpriteRenderer>(true);

            foreach (var rend in internalSpriteRenderers)
            {
                rend.color = colour;
            }
            
            foreach (var rend in exteriorlSpriteRenderers)
            {
                rend.color = colour;
            }
            
            foreach (var rend in roofSpriteRenderers)
            {
                rend.color = colour;
            }
            
            foreach (var rend in doorSpriteRenderers)
            {
                rend.color = colour;
            }
            
            foreach (var rend in floorSpriteRenderers)
            {
                rend.color = colour;
            }
        }
        
        private void CreateConstructionHaulingTasks()
        {
             var resourceCosts = _buildingData.GetResourceCosts();
             CreateConstuctionHaulingTasksForItems(resourceCosts);
        }

        protected override void EnqueueCreateTakeResourceToBlueprintTask(ItemData resourceData)
        {
            Task task = new Task
            {
                TaskId = "Withdraw Item Construction",
                Requestor = this,
                Payload = resourceData.ItemName,
                TaskType = TaskType.Haul,
            };
            TaskManager.Instance.AddTask(task);
        }

        public override void CreateConstructTask(bool autoAssign = true)
        {
            Task constuctTask = new Task()
            {
                TaskId = "Build Building",
                Requestor = this,
            };
            constuctTask.Enqueue();
        }

        public Vector2 ConstructionStandPosition()
        {
            return _constructionStandPos.position;
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
