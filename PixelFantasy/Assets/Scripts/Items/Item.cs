using System;
using System.Collections.Generic;
using AI;
using Characters;
using Interfaces;
using Managers;
using TaskSystem;
using UnityEngine;
using Task = AI.Task;

namespace Items
{
    public class Item : PlayerInteractable, IClickableObject
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private ClickObject _clickObject;
        
        private Transform _originalParent;
        
        public ItemData RuntimeData;
        public Action OnChanged { get; set; }
        public override string UniqueID => RuntimeData.UniqueID;
        
        public override string PendingTaskUID
        {
            get => RuntimeData.PendingTaskUID;
            set => RuntimeData.PendingTaskUID = value;
        }

        public PlayerInteractable GetPlayerInteractable()
        {
            return this;
        }
        
        public ClickObject GetClickObject()
        {
            return _clickObject;
        }
        
        public void InitializeItem(ItemSettings settings, bool allowed)
        {
            RuntimeData = settings.CreateItemData();
            RuntimeData.Position = transform.position;
            
            PlayerInteractableDatabase.Instance.RegisterPlayerInteractable(this);
            
            DisplayItemSprite();

            if (allowed)
            {
                SeekForSlot();
            }
        }

        public void LoadItemData(ItemData data, bool canHaul)
        {
            RuntimeData = data;
            DisplayItemSprite();

            PlayerInteractableDatabase.Instance.RegisterPlayerInteractable(this);
            
            if (canHaul)
            {
                SeekForSlot();
            }
        }
       
        public void SeekForSlot()
        {
            if (RuntimeData.AssignedStorage == null && RuntimeData.State != EItemState.Carried)
            {
                var storage = InventoryManager.Instance.GetAvailableStorage(RuntimeData.Settings);
                if (storage != null)
                {
                    RuntimeData.AssignedStorageID = storage.UniqueID;
                    storage.SetIncoming(RuntimeData);
                    CreateHaulTask();
                }
            }
        }

        private float _seekTimer;
        private void Update()
        {
            if (RuntimeData != null && RuntimeData.AssignedStorage == null && RuntimeData.State != EItemState.Carried)
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
            Task task = new Task("Store Item", ETaskType.Hauling, this);
            TasksDatabase.Instance.AddTask(task);
            RuntimeData.CurrentTaskID = task.UniqueID;
        }
        
        private void GameEvent_OnInventoryAvailabilityChanged()
        {
            SeekForSlot();
        }
        
        public void ItemPickedUp(Kinling kinling)
        {
            RuntimeData.CarryingKinlingUID = kinling.RuntimeData.UniqueID;
            RuntimeData.State = EItemState.Carried;
        }

        public void ItemDropped()
        {
            RuntimeData.AssignedStorageID = null;
            RuntimeData.CarryingKinlingUID = null;
            RuntimeData.State = EItemState.Loose;

            if (IsAllowed)
            {
                SeekForSlot();
            }
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
            PlayerInteractableDatabase.Instance.DeregisterPlayerInteractable(this);

            if (RuntimeData.CurrentTask != null)
            {
                RuntimeData.CurrentTask.Cancel();
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

        public string DisplayName => RuntimeData.Settings.ItemName;

        public override Vector2? UseagePosition(Vector2 requestorPosition)
        {
            return transform.position;
        }
    }
}
