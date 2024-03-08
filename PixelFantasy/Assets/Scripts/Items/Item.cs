using System;
using System.Collections.Generic;
using Characters;
using DataPersistence;
using Interfaces;
using Managers;
using ScriptableObjects;
using Systems.Skills.Scripts;
using Systems.SmartObjects.Scripts;
using TaskSystem;
using UnityEngine;
using UnityEngine.Serialization;
using Zones;

namespace Items
{
    public class Item : PlayerInteractable, IClickableObject, IPersistent
    {
        [FormerlySerializedAs("_itemData")] [SerializeField] private ItemSettings _itemSettings;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private ClickObject _clickObject;

        private Task _currentTask;
        
        public string _assignedSlotUID;
        private string _assignedUnitUID;
        private bool _isHeld;
        private Kinling _carryingKinling;

        private Transform _originalParent;

        public Storage AssignedStorage;

        public ItemState State { get; private set; }
        
        public PlayerInteractable GetPlayerInteractable()
        {
            return this;
        }

        public void AssignCommand(Command command, object payload = null)
        {
            CreateTask(command, payload);
        }

        public ClickObject GetClickObject()
        {
            return _clickObject;
        }
        
        public void InitializeItem(ItemSettings itemSettings, bool allowed, ItemState state = null, bool populateInteraction = true)
        {
            _itemSettings = itemSettings;
            IsAllowed = allowed;

            if (state == null || state.Settings == null)
            {
                InitUID();
                State = _itemSettings.CreateState(UniqueId, this);
            }
            else
            {
                State = state;
                UniqueId = State.UID;
            }
            
            DisplayItemSprite();

            if (allowed)
            {
                SeekForSlot();
            }
        }

        public void SeekForSlot()
        {
            if (AssignedStorage == null && !_isHeld)
            {
                AssignedStorage = InventoryManager.Instance.GetAvailableStorage(this);
                if (AssignedStorage != null)
                {
                    AssignedStorage.StorageData.SetIncoming(this);
                    CreateHaulTask();
                }
            }
        }

        private float _seekTimer;
        private void Update()
        {
            if (AssignedStorage == null && !_isHeld)
            {
                _seekTimer += Time.deltaTime;
                if (_seekTimer > 1f)
                {
                    _seekTimer = 0;
                    SeekForSlot();
                }
            }
        }

        public void CreateHaulTask()
        {
            Task task = new Task("Store Item", ETaskType.Hauling, this, EToolType.None);

            TaskManager.Instance.AddTask(task);
            _currentTask = task;
        }

        public void CancelTask(bool lookToHaul = true)
        {
            if (_currentTask != null)
            {
                if (AssignedStorage != null)
                {
                    AssignedStorage.StorageData.CancelIncoming(this);
                    AssignedStorage = null;
                }

                if (_carryingKinling != null)
                {
                    _carryingKinling.TaskAI.CancelTask(_currentTask.TaskId);
                }
                
                _currentTask.Cancel();

                if (lookToHaul)
                {
                    SeekForSlot();
                }
            }
        }

        private void GameEvent_OnInventoryAvailabilityChanged()
        {
            SeekForSlot();
        }

        private void SetHeld(bool isHeld)
        {
            _isHeld = isHeld;
        }

        public void ItemPickedUp(Kinling kinling)
        {
            _carryingKinling = kinling;
            SetHeld(true);
        }

        public void ItemDropped()
        {
            SetHeld(false);
            _carryingKinling = null;
            
            if (_onItemRelocatedCallback != null)
            {
                _onItemRelocatedCallback.Invoke();
            }

            if (IsAllowed)
            {
                SeekForSlot();
            }
        }

        public void AddItemToSlot()
        {
            _isHeld = false;
            AssignedStorage.DepositItems(this);
        }

        private void DisplayItemSprite()
        {
            if (_itemSettings == null) return;

            _spriteRenderer.sprite = _itemSettings.ItemSprite;
        }

        public bool IsSameItemType(Item item)
        {
            return _itemSettings == item.GetItemData();
        }

        public ItemSettings GetItemData()
        {
            return _itemSettings;
        }

        private void OnValidate()
        {
            DisplayItemSprite();
        }

        public void ToggleAllowed(bool isAllowed)
        {
            IsAllowed = isAllowed;
            if (IsAllowed)
            {
                //_icon.gameObject.SetActive(false);
                //_icon.sprite = null;
                //CreateHaulTask();
            }
            else
            {
                //_icon.gameObject.SetActive(true);
                //_icon.sprite = Librarian.Instance.GetSprite("Lock");
                //CancelAssignedTask();
            }
            
            RefreshSelection();
        }

        public bool IsAllowed { get; set; }

        private void Start()
        {
            GameEvents.OnInventoryAvailabilityChanged += GameEvent_OnInventoryAvailabilityChanged;
            
            if (_itemSettings != null && State == null)
            {
                InitializeItem(_itemSettings, true, State);
            }
        }

        private void OnDestroy()
        {
            GameEvents.OnInventoryAvailabilityChanged -= GameEvent_OnInventoryAvailabilityChanged;
            
            if(_currentTask != null)
                _currentTask.Cancel();
        }

        public bool IsClickDisabled { get; set; }

        private void RefreshSelection()
        {
            if (_clickObject.IsSelected)
            {
                GameEvents.Trigger_RefreshSelection();
            }
        }
        
        public virtual List<Command> GetCommands()
        {
            return Commands;
        }

        public void UnclaimItem()
        {
            if (AssignedStorage == null)
            {
                Debug.LogError("Tried to unclaim an item that is not assigned to storage");
                return;
            }
            
            AssignedStorage.StorageData.RestoreClaimed(this);
        }

        public void ClaimItem()
        {
            if (AssignedStorage == null)
            {
                Debug.LogError("Tried to Claim an item that is not assigned to storage");
                return;
            }

            if (!AssignedStorage.StorageData.SetClaimedItem(this))
            {
                Debug.LogError("Failed to claim item");
            }
        }

        private Action _onItemRelocatedCallback;
        public void RelocateItem(Action onItemRelocated, Vector2 newLocation)
        {
            if (_currentTask is not { TaskId: "Relocate Item" })
            {
                CancelTask(false);
                
                _onItemRelocatedCallback = onItemRelocated;
                AssignCommand(Librarian.Instance.GetCommand("Relocate Item"), newLocation);
            }
        }

        public string DisplayName => _itemSettings.ItemName;

        public object CaptureState()
        {
            return new Data
            {
                UID = UniqueId,
                Position = transform.position,
                ItemSettings = _itemSettings,
                OriginalParent = _originalParent,
                IsAllowed = this.IsAllowed,
                IsClickDisabled = this.IsClickDisabled,
                AssignedSlotUID = _assignedSlotUID,
                AssignedUnitUID = _assignedUnitUID,
                IsHeld = _isHeld,
            };
        }

        public void RestoreState(object data)
        {
            var itemState = (Data)data;

            UniqueId = itemState.UID;
            transform.position = itemState.Position;
            _originalParent = itemState.OriginalParent;
            IsAllowed = itemState.IsAllowed;
            IsClickDisabled = itemState.IsClickDisabled;
            _assignedSlotUID = itemState.AssignedSlotUID;
            _assignedUnitUID = itemState.AssignedUnitUID;
            _isHeld = itemState.IsHeld;

            InitializeItem(itemState.ItemSettings, IsAllowed);
        }

        public struct Data
        {
            public string UID;
            public Vector3 Position;
            public ItemSettings ItemSettings;
            public Transform OriginalParent;
            public bool IsAllowed;
            public bool IsClickDisabled;
            
            public string AssignedSlotUID;
            public string AssignedUnitUID;
            public bool IsHeld;
        }

        public override Vector2? UseagePosition(Vector2 requestorPosition)
        {
            return transform.position;
        }
    }
}
