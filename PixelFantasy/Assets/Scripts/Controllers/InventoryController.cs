using System;
using System.Collections.Generic;
using DataPersistence;
using DataPersistence.States;
using Gods;
using Handlers;
using Items;
using ScriptableObjects;
using UnityEngine;

namespace Controllers
{
    public class InventoryController : Saveable
    {
        protected override string StateName => "InventoryController";
        public override int LoadOrder => 1;
        
        private List<StorageSlot> _storageSlots = new List<StorageSlot>();
        private Dictionary<ItemData, int> _inventory = new Dictionary<ItemData, int>();
        private Dictionary<ItemData, int> _pendingInventory = new Dictionary<ItemData, int>();
        
        [SerializeField] private GameObject _storageZonePrefab;
        [SerializeField] private Transform _storageParent;

        [SerializeField] private StorageHandler _storageHandler;

        public Dictionary<ItemData, int> Inventory => _inventory;

        public StorageSlot GetAvailableStorageSlot(Item item)
        {
            // First check if there is any place that can stack
            foreach (var itemSlot in _storageSlots)
            {
                if (itemSlot.CanStack(item))
                {
                    RemoveItemFromPending(item);
                    itemSlot.AddItemIncoming(item);
                    return itemSlot;
                }
            }
            
            // Then find empty
            foreach (var itemSlot in _storageSlots)
            {
                if (itemSlot.IsEmpty())
                {
                    RemoveItemFromPending(item);
                    itemSlot.AddItemIncoming(item);
                    return itemSlot;
                }
            }

            return null;
        }

        public bool HasItemAvailable(ItemData itemData, int quantity)
        {
            if (_inventory.ContainsKey(itemData))
            {
                return _inventory[itemData] >= quantity;
            }
            else
            {
                return false;
            }
        }

        public int AvailableItemQuantity(ItemData itemData)
        {
            if (_inventory.ContainsKey(itemData))
            {
                return _inventory[itemData];
            }
            else
            {
                return 0;
            }
        }

        public void AddNewStorageSlot(StorageSlot newSlot)
        {
            _storageSlots.Add(newSlot);
        }

        /// <summary>
        /// Spawns an item slot in the game at the target location
        /// </summary>
        public void SpawnStorageSlot(Vector3 position)
        {
            var storageSlot = Instantiate(_storageZonePrefab, position, Quaternion.identity);
            storageSlot.transform.SetParent(_storageParent);
            var storage = storageSlot.GetComponent<StorageSlot>();
            storage.Init();
            AddNewStorageSlot(storage);
        }

        public void AddToInventory(ItemData itemData, int quantity)
        {
            if (_inventory.ContainsKey(itemData))
            {
                _inventory[itemData] += quantity;
            }
            else
            {
                _inventory.Add(itemData, quantity);
            }
            
            
            GameEvents.Trigger_OnInventoryAdded(itemData, _inventory[itemData]);
        }

        public void RemoveFromInventory(ItemData itemData, int quantity)
        {
            if (_inventory.ContainsKey(itemData))
            {
                _inventory[itemData] -= quantity;
            
                GameEvents.Trigger_OnInventoryRemoved(itemData, _inventory[itemData]);
            }
            else
            {
                Debug.LogError($"Tried removing {itemData.ItemName} from inventory, but it doesn't exist!");
            }
        }

        
        public StorageSlot ClaimResource(ItemData itemData)
        {
            // Can afford?
            if (_inventory.ContainsKey(itemData))
            {
                // TODO: This can be improved by comparing all the slots that have the resource's distance from the destination and choosing the closest
                foreach (var storageSlot in _storageSlots)
                {
                    if (!storageSlot.IsEmpty() && storageSlot.GetStoredType() == itemData)
                    {
                        var slot = storageSlot.ClaimItem();
                        if (slot != null)
                        {
                            //RemoveFromInventory(itemData, 1);
                            return slot;
                        }
                    }
                }
            }
            
            return null;
        }

        public void DeductClaimedResource(Item claimedResource)
        {
            foreach (var storageSlot in _storageSlots)
            {
                if (storageSlot.HasItemClaimed(claimedResource))
                {
                    storageSlot.RemoveClaimedItem();
                    break;
                }
            }
        }

        public int NumSpacesAvailable(Item item)
        {
            int totalRemaining = 0;
            foreach (var storageSlot in _storageSlots)
            {
                if (storageSlot.CanHaveMoreIncoming())
                {
                    totalRemaining += storageSlot.SpaceRemaining(item);
                }
            }

            int amountPending = 0;
            if (_pendingInventory.ContainsKey(item.GetItemData()))
            {
                amountPending = _pendingInventory[item.GetItemData()];
            }

            totalRemaining -= amountPending;

            return totalRemaining;
        }

        public void AddItemToPending(Item item)
        {
            if (_pendingInventory.ContainsKey(item.GetItemData()))
            {
                _pendingInventory[item.GetItemData()]++;
            }
            else
            {
                _pendingInventory.Add(item.GetItemData(), 1);
            }
        }

        public void RemoveItemFromPending(Item item)
        {
            if (_pendingInventory.ContainsKey(item.GetItemData()))
            {
                _pendingInventory[item.GetItemData()]--;
            }
        }

        public bool HasSpaceForItem(Item item)
        {
            var numSpaces = NumSpacesAvailable(item);

            if (numSpaces > 0)
            {
                //AddItemToPending(item);
                return true;
            }

            return false;
        }

        #region Storage
        [SerializeField] private Sprite _storageZoneBlueprint;
        
        private bool _planningStorage;
        private Vector2 _startPos;
        private List<GameObject> _blueprints = new List<GameObject>();
        public readonly List<string> StoragePlacementInvalidTags = new List<string>
        {
            "Water",
            "Zone"
        };

        public Sprite GetStorageZoneBlueprintSprite()
        {
            return _storageZoneBlueprint;
        }
        
        private void OnEnable()
        {
            GameEvents.OnLeftClickDown += GameEvents_OnLeftClickDown;
            GameEvents.OnLeftClickHeld += GameEvents_OnLeftClickHeld;
            GameEvents.OnLeftClickUp += GameEvents_OnLeftClickUp;
            GameEvents.OnRightClickDown += GameEvents_OnRightClickDown;
        }

        private void OnDisable()
        {
            GameEvents.OnLeftClickDown -= GameEvents_OnLeftClickDown;
            GameEvents.OnLeftClickHeld -= GameEvents_OnLeftClickHeld;
            GameEvents.OnLeftClickUp -= GameEvents_OnLeftClickUp;
            GameEvents.OnRightClickDown -= GameEvents_OnRightClickDown;
        }
        

        protected void GameEvents_OnRightClickDown(Vector3 mousePos, PlayerInputState inputState, bool isOverUI)
        {
            if (inputState == PlayerInputState.BuildStorage)
            {
                CancelPlanning();
            }
        }

        protected void GameEvents_OnLeftClickDown(Vector3 mousePos, PlayerInputState inputState, bool isOverUI)
        {
            if (inputState == PlayerInputState.BuildStorage)
            {
                _planningStorage = true;
                _startPos = Helper.ConvertMousePosToGridPos(mousePos);
            }
        }

        protected void GameEvents_OnLeftClickHeld(Vector3 mousePos, PlayerInputState inputState, bool isOverUI)
        {
            if (inputState == PlayerInputState.BuildStorage)
            {
                PlanStorageSlots(mousePos);
            }
        }

        protected void GameEvents_OnLeftClickUp(Vector3 mousePos, PlayerInputState inputState, bool isOverUI)
        {
            if (inputState == PlayerInputState.BuildStorage)
            {
                SpawnPlannedStorageSlots();
            }
        }
        
                private void CancelPlanning()
        {
            PlayerInputController.Instance.ChangeState(PlayerInputState.None);
            ClearPlannedSlots();
            _plannedGrid.Clear();
        }

        /// <summary>
        /// Begin recording all the slots being planned for construction,
        /// Also display their blue print on the tiles,
        /// If player right clicks, this is cancelled
        /// </summary>
        private List<Vector2> _plannedGrid = new List<Vector2>();
        private void PlanStorageSlots(Vector2 mousePos)
        {
            if (!_planningStorage) return;

            Vector3 curGridPos = Helper.ConvertMousePosToGridPos(mousePos);
            List<Vector2> gridPositions = Helper.GetRectangleGridPositionsBetweenPoints(_startPos, curGridPos);
            if (gridPositions.Count != _plannedGrid.Count)
            {
                _plannedGrid = gridPositions;
                
                // Clear previous display, then display new blueprints
                ClearPlannedSlots();

                foreach (var gridPos in gridPositions)
                {
                    var blueprint = new GameObject("blueprint", typeof(SpriteRenderer));
                    blueprint.transform.position = gridPos;
                    var spriteRenderer = blueprint.GetComponent<SpriteRenderer>();
                    spriteRenderer.sprite = _storageZoneBlueprint;
                    if (Helper.IsGridPosValidToBuild(gridPos, StoragePlacementInvalidTags))
                    {
                        spriteRenderer.color = Librarian.Instance.GetColour("Placement Green");
                    }
                    else
                    {
                        spriteRenderer.color = Librarian.Instance.GetColour("Placement Red");
                    }
                    
                    spriteRenderer.sortingLayerName = "Item";
                    _blueprints.Add(blueprint);
                }
            }
            
        }

        private void ClearPlannedSlots()
        {
            foreach (var blueprint in _blueprints)
            {
                Destroy(blueprint);
            }
            _blueprints.Clear();
        }

        /// <summary>
        /// Using the planned tiles recorded when held,
        /// Spawn the Item slots on the tiles
        /// </summary>
        private void SpawnPlannedStorageSlots()
        {
            if (!_planningStorage) return;

            foreach (var gridPos in _plannedGrid)
            {
                if (Helper.IsGridPosValidToBuild(gridPos, StoragePlacementInvalidTags))
                {
                    CreateStorageSlot(gridPos);
                }
            }

            ClearPlannedSlots();
            _plannedGrid.Clear();
            _planningStorage = false;
            CancelPlanning();
        }

        private void CreateStorageSlot(Vector2 pos)
        {
            SpawnStorageSlot(pos);
        }

        #endregion

        private void RefreshInventoryDisplay()
        {
            foreach (var slot in _storageSlots)
            {
                var itemType = slot.GetStoredType();
                if (itemType != null && _inventory.ContainsKey(itemType))
                {
                    GameEvents.Trigger_OnInventoryAdded(itemType, _inventory[itemType]);
                }
            }
        }

        protected override void ClearChildStates(List<object> childrenStates) { } // Not used

        protected override void SetChildStates(List<object> childrenStates) {} // Not used

        public override void ClearData(GameState state)
        {
            _storageSlots.Clear();
        }

        public override void LoadData(GameState state)
        {
            // Loads its own data, not children
            var invState = state.States[StateName];
            var invData = (InventoryData)invState;

            _inventory = invData.Inventory;
            //_pendingInventory = invData.PendingInventory;
            
            // Finds the Storage slot based on the GUID
            foreach (var storageSlotGUID in invData.StorageSlotsUIDs)
            {
                var storageSlot = _storageHandler.GetStorageSlotByUID(storageSlotGUID);
                if (storageSlot != null)
                {
                    _storageSlots.Add(storageSlot);
                }
            }

            RefreshInventoryDisplay();
        }

        public override void SaveState(ref GameState state)
        {
            List<string> storageSlotsGUIDs = new List<string>();
            foreach (var storageSlot in _storageSlots)
            {
                storageSlotsGUIDs.Add(storageSlot.UniqueId);
            }
            
            // Saves its own data, not children
            state.States[StateName] = new InventoryData
            {
                StorageSlotsUIDs = storageSlotsGUIDs,
                Inventory = _inventory,
                //PendingInventory = _pendingInventory,
            };
        }

        public struct InventoryData
        {
            public List<string> StorageSlotsUIDs;
            public Dictionary<ItemData, int> Inventory;
            //public Dictionary<ItemData, int> PendingInventory;
        }
    }
}
