using System;
using System.Collections.Generic;
using Actions;
using Controllers;
using DataPersistence;
using Gods;
using Interfaces;
using ScriptableObjects;
using Sirenix.OdinInspector;
using Tasks;
using Characters;
using SGoap;
using TaskSystem;
using UnityEngine;
using Zones;

namespace Items
{
    public class Item : Interactable, IClickableObject, IPersistent
    {
        [SerializeField] private ItemData _itemData;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private ClickObject _clickObject;
        [SerializeField] private ActionTakeItemToItemSlot _takeItemToItemSlotAction;

        private GoalRequest _currentGoal;
        private Task _currentTask;
        
        public ActionBase PendingTask;
        public ActionBase InProgressTask;
        public string _assignedSlotUID;
        private string _assignedUnitUID;
        private bool _isHeld;

        private int _assignedTaskRef;
        private UnitTaskAI _incomingUnit;
        private Transform _originalParent;
        
        // public TaskType PendingTask;
        
        public StorageSlot AssignedStorageSlot;
 
        private TaskMaster taskMaster => TaskMaster.Instance;
        
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
            if (AssignedStorageSlot == null && !_isHeld)
            {
                AssignedStorageSlot = ControllerManager.Instance.InventoryController.GetAvailableStorageSlot(this);
                if (AssignedStorageSlot != null)
                {
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

            TaskManager.Instance.AddTask(task);
            _currentTask = task;
        }

        public void CancelTask(bool lookToHaul = true)
        {
            if (_currentTask != null)
            {
                AssignedStorageSlot = null;
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

        public void AssignUnit(UnitTaskAI unit)
        {
            if (unit == null)
            {
                _assignedUnitUID = "";
            }
            else
            {
                _assignedUnitUID = unit.UniqueId;
            }
        }

        public void SetAssignedSlot(StorageSlot slot)
        {
            if (slot == null)
            {
                _assignedSlotUID = "";
                return;
            }
            
            _assignedSlotUID = slot.UniqueId;
        }

        // public void EnqueueTaskForHauling()
        // {
        //     _assignedTaskRef = _takeItemToItemSlotAction.EnqueueTask(this, true);
        //     SetTaskToPending(_takeItemToItemSlotAction);
        // }
        
        public void OnTaskAccepted(ActionBase task)
        {
            SetTaskToAccepted(task);
        }

        public void OnTaskCompleted()
        {
            _assignedTaskRef = 0;
            InProgressTask = null;
        }
        
        public void SetTaskToAccepted(ActionBase task)
        {
            PendingTask = null;
            InProgressTask = task;
        }
        
        public void SetTaskToPending(ActionBase task)
        {
            PendingTask = task;
        }

        public void AddItemToSlot()
        {
            _isHeld = false;
            ControllerManager.Instance.InventoryController.AddToInventory(_itemData, 1);
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
                CancelAssignedTask();
            }
            
            RefreshSelection();
        }

        public bool IsAllowed { get; set; }

        public int GetAssignedTask()
        {
            return _assignedTaskRef;
        }

        public void ReenqueueAssignedTask()
        {
            if (_assignedTaskRef == 0) return;

            taskMaster.HaulingTaskSystem.CancelTask(_assignedTaskRef);
            var cancelSuccess = taskMaster.HaulingTaskSystem.CancelTask(_assignedTaskRef);
            if (!cancelSuccess)
            {
                taskMaster.HaulingTaskSystem.CancelTask(UniqueId);
            }

            transform.parent = _originalParent;
            if (_incomingUnit != null)
            {
                _incomingUnit.CancelTask();
            }
            
            //EnqueueTaskForHauling();
            _takeItemToItemSlotAction.CreateTask(this, true);
            RefreshSelection();
        }

        public void CancelAssignedTask()
        {
            if (_assignedTaskRef == 0) return;

            taskMaster.HaulingTaskSystem.CancelTask(_assignedTaskRef);
            transform.parent = _originalParent;
            if (_incomingUnit != null)
            {
                _incomingUnit.CancelTask();
            }
            
            RefreshSelection();
            _assignedTaskRef = 0;
        }

        private void Start()
        {
            GameEvents.OnInventoryAvailabilityChanged += GameEvent_OnInventoryAvailabilityChanged;
        }

        private void OnDestroy()
        {
            GameEvents.OnInventoryAvailabilityChanged -= GameEvent_OnInventoryAvailabilityChanged;

            if (_currentGoal != null)
            {
                GoalMaster.Instance.CancelGoal(_currentGoal);
            }

            if (_assignedTaskRef == 0) return;
            taskMaster.HaulingTaskSystem.CancelTask(_assignedTaskRef);
        }

        public bool IsClickDisabled { get; set; }

        private void RefreshSelection()
        {
            if (_clickObject.IsSelected)
            {
                GameEvents.Trigger_RefreshSelection();
            }
        }

        public List<ActionBase> GetActions()
        {
            var result = new List<ActionBase>();
            result.Add(_takeItemToItemSlotAction);
            return result;
        }
        
        public virtual List<Command> GetCommands()
        {
            return Commands;
        }
        
        public bool IsActionActive(ActionBase action)
        {
            throw new NotImplementedException();
        }

        public void AssignOrder(ActionBase orderToAssign)
        {
            switch (orderToAssign.id)
            {
                case "Allow":
                    ToggleAllowed(true);
                    break;
                case "Disallow":
                    ToggleAllowed(false);
                    break;
            }
        }
        
        public List<ActionBase> GetCancellableActions()
        {
            return null;
        }
        
        public object CaptureState()
        {
            return new Data
            {
                UID = UniqueId,
                Position = transform.position,
                ItemData = _itemData,
                AssignedTaskRef = _assignedTaskRef,
                IncomingUnit = _incomingUnit,
                OriginalParent = _originalParent,
                IsAllowed = this.IsAllowed,
                IsClickDisabled = this.IsClickDisabled,
                PendingTask = PendingTask,
                InProgressTask = InProgressTask,
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
            _assignedTaskRef = itemState.AssignedTaskRef;
            _incomingUnit = itemState.IncomingUnit;
            _originalParent = itemState.OriginalParent;
            IsAllowed = itemState.IsAllowed;
            IsClickDisabled = itemState.IsClickDisabled;

            PendingTask = itemState.PendingTask;
            InProgressTask = itemState.InProgressTask;
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
            public int AssignedTaskRef;
            public UnitTaskAI IncomingUnit;
            public Transform OriginalParent;
            public bool IsAllowed;
            public bool IsClickDisabled;

            public ActionBase PendingTask;
            public ActionBase InProgressTask;
            public string AssignedSlotUID;
            public string AssignedUnitUID;
            public bool IsHeld;
        }
    }
}
