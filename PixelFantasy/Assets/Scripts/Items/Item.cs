using System;
using System.Collections.Generic;
using Characters;
using Interfaces;
using Managers;
using TaskSystem;
using UnityEngine;

namespace Items
{
    public class Item : PlayerInteractable, IClickableObject
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private ClickObject _clickObject;

        private Task _currentTask;
        
        public string _assignedSlotUID;
        private string _assignedUnitUID;
        private bool _isHeld;
        private Kinling _carryingKinling;

        private Transform _originalParent;

        public IStorage AssignedStorage;

        protected ItemSettings _settings;

        public ItemData RuntimeData;
        public Action OnChanged { get; set; }
        
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
        
        public void InitializeItem(ItemSettings settings, bool allowed)
        {
            _settings = settings;
            RuntimeData = _settings.CreateItemData();
            //RuntimeData.InitData(settings);
            RuntimeData.Position = transform.position;
            
            DisplayItemSprite();

            if (allowed)
            {
                SeekForSlot();
            }
        }

        public void LoadItemData(ItemData data, bool canHaul)
        {
            RuntimeData = data;
            RuntimeData.LinkedItem = this;
            DisplayItemSprite();

            if (canHaul)
            {
                SeekForSlot();
            }
        }
        
        protected void Saved()
        {
            
        }

        protected void Loaded()
        {
            
        }
       
        public void SeekForSlot()
        {
            if (AssignedStorage == null && !_isHeld)
            {
                AssignedStorage = InventoryManager.Instance.GetAvailableStorage(RuntimeData.Settings);
                if (AssignedStorage != null)
                {
                    AssignedStorage.SetIncoming(RuntimeData);
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

            if (RuntimeData != null)
            {
                RuntimeData.Position = transform.position;
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
                    AssignedStorage.CancelIncoming(RuntimeData);
                    AssignedStorage = null;
                }

                if (_carryingKinling != null)
                {
                    _carryingKinling.TaskAI.CancelTask(_currentTask.TaskId);
                }
                
                _currentTask.Cancel();
                _currentTask = null;
                
                CancelRequestorTasks();

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
            _spriteRenderer.sprite = RuntimeData.Settings.ItemSprite;
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
        }

        private void OnDestroy()
        {
            GameEvents.OnInventoryAvailabilityChanged -= GameEvent_OnInventoryAvailabilityChanged;
            
            if(_currentTask != null)
                _currentTask.Cancel();

            if (RuntimeData != null)
            {
                RuntimeData.LinkedItem = null;
            }
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

        public string DisplayName => RuntimeData.Settings.ItemName;

        public override Vector2? UseagePosition(Vector2 requestorPosition)
        {
            return transform.position;
        }
    }
}
