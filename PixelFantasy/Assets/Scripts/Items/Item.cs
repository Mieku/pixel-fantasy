using System;
using System.Collections.Generic;
using DataPersistence;
using Interfaces;
using Managers;
using ScriptableObjects;
using Systems.SmartObjects.Scripts;
using TaskSystem;
using UnityEngine;
using UnityEngine.Serialization;
using Zones;

namespace Items
{
    public class Item : PlayerInteractable, IClickableObject, IPersistent
    {
        [SerializeField] private ItemData _itemData;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private ClickObject _clickObject;
        [SerializeField] private SmartObjectItem _smartObject;

        private Task _currentTask;
        
        public string _assignedSlotUID;
        private string _assignedUnitUID;
        private bool _isHeld;

        private Transform _originalParent;

        public Storage AssignedStorage;

        public ItemState State { get; private set; }

        public ClickObject GetClickObject()
        {
            return _clickObject;
        }
        
        public void InitializeItem(ItemData itemData, bool allowed, ItemState state = null, bool populateInteraction = true)
        {
            _itemData = itemData;
            IsAllowed = allowed;

            if (state == null || state.Data == null)
            {
                InitUID();
                State = _itemData.CreateState(UniqueId, this);
            }
            else
            {
                State = state;
                UniqueId = State.UID;
            }

            _smartObject.Init(this, populateInteraction);
            
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
                    AssignedStorage.SetIncoming(this);
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
            Task task = new Task
            {
                TaskId = "Store Item",
                Requestor = this,
                TaskType = TaskType.Haul
            };
            
            TaskManager.Instance.AddTask(task);
            _currentTask = task;
        }

        public void CancelTask(bool lookToHaul = true)
        {
            if (_currentTask != null)
            {
                AssignedStorage = null;
                _currentTask.Cancel();
                
                SeekForSlot();
            }
        }

        private void GameEvent_OnInventoryAvailabilityChanged()
        {
            SeekForSlot();
        }

        public void SetHeld(bool isHeld)
        {
            _isHeld = isHeld;
        }

        public void AddItemToSlot()
        {
            _isHeld = false;
            AssignedStorage.DepositItems(this);
        }

        private void DisplayItemSprite()
        {
            if (_itemData == null) return;

            _spriteRenderer.sprite = _itemData.ItemSprite;
            _spriteRenderer.transform.localScale = _itemData.DefaultSpriteScale;
        }

        public bool IsSameItemType(Item item)
        {
            return _itemData == item.GetItemData();
        }

        public ItemData GetItemData()
        {
            return _itemData;
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
            
            if (_itemData != null && State == null)
            {
                InitializeItem(_itemData, true, State);
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
        
        public object CaptureState()
        {
            return new Data
            {
                UID = UniqueId,
                Position = transform.position,
                ItemData = _itemData,
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

            InitializeItem(itemState.ItemData, IsAllowed);
        }

        public struct Data
        {
            public string UID;
            public Vector3 Position;
            public ItemData ItemData;
            public Transform OriginalParent;
            public bool IsAllowed;
            public bool IsClickDisabled;
            
            public string AssignedSlotUID;
            public string AssignedUnitUID;
            public bool IsHeld;
        }
    }
}
