using System;
using System.Collections.Generic;
using DataPersistence;
using Gods;
using Interfaces;
using ScriptableObjects;
using TaskSystem;
using UnityEngine;
using UnityEngine.Serialization;
using Zones;

namespace Items
{
    public class Item : Interactable, IClickableObject, IPersistent
    {
        [SerializeField] private ItemData _itemData;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private ClickObject _clickObject;

        private Task _currentTask;
        
        public string _assignedSlotUID;
        private string _assignedUnitUID;
        private bool _isHeld;

        private Transform _originalParent;

        public Storage AssignedStorage;

        //[FormerlySerializedAs("AssignedStorageSlot")] public StorageTile assignedStorageTile;

        public ClickObject GetClickObject()
        {
            return _clickObject;
        }
        
        public void InitializeItem(ItemData itemData, bool allowed)
        {
            _itemData = itemData;
            IsAllowed = allowed;

            InitUID();
            
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
                    AssignedStorage.SetIncoming(_itemData, 1);
                    CreateHaulTask();
                }
            }
        }

        public void CreateHaulTask()
        {
            Task task = new Task
            {
                Category = TaskCategory.Hauling,
                TaskId = "Store Item",
                Requestor = this
            };

            var zone = Helper.IsPositionInZone(transform.position);
            if (zone != null)
            {
                zone.Building.BuildingTasks.AddTask(task);
            }
            else
            {
                TaskManager.Instance.AddTask(task);
            }
            
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
            AssignedStorage.DepositItems(_itemData, 1);
            Destroy(gameObject);
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
            
            if (_itemData != null)
            {
                InitializeItem(_itemData, true);
            }
        }

        private void OnDestroy()
        {
            GameEvents.OnInventoryAvailabilityChanged -= GameEvent_OnInventoryAvailabilityChanged;
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
