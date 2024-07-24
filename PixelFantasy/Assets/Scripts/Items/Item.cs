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

        public void CancelTask(bool lookToHaul = true)
        {
            // if (RuntimeData.CurrentTask != null)
            // {
            //     if (RuntimeData.AssignedStorage != null)
            //     {
            //         RuntimeData.AssignedStorage.CancelIncoming(RuntimeData);
            //         RuntimeData.AssignedStorageID = null;
            //     }
            //
            //     if (!string.IsNullOrEmpty(RuntimeData.CarryingKinlingUID))
            //     {
            //         var carryingKinling = KinlingsDatabase.Instance.GetKinling(RuntimeData.CarryingKinlingUID);
            //         carryingKinling.TaskAI.CancelTask(RuntimeData.CurrentTask.TaskId);
            //     }
            //     
            //     RuntimeData.CurrentTask.Cancel();
            //     RuntimeData.CurrentTask = null;
            //     
            //     CancelRequestorTasks();
            //
            //     if (lookToHaul)
            //     {
            //         SeekForSlot();
            //     }
            // }
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
            RuntimeData.CarryingKinlingUID = null;
            RuntimeData.State = EItemState.Loose;
            
            if (_onItemRelocatedCallback != null)
            {
                _onItemRelocatedCallback.Invoke();
            }

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

        

        private Action _onItemRelocatedCallback;
        public void RelocateItem(Action onItemRelocated, Vector2 newLocation)
        {
            if (RuntimeData.CurrentTaskID != "Relocate Item")
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
