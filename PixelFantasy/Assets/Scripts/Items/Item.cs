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
using UnityEngine;

namespace Items
{
    public class Item : Interactable, IClickableObject, IPersistent
    {
        [SerializeField] private ItemData _itemData;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private ClickObject _clickObject;

        private int _assignedTaskRef;
        private UnitTaskAI _incomingUnit;
        private Transform _originalParent;
        
        public TaskType PendingTask;
 
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

            if (IsAllowed && PendingTask == TaskType.None)
            {
                CreateHaulTask();
            }
        }
        
        public void CreateHaulTask()
        {
            HaulingTask.TakeItemToItemSlot task = null;
            _assignedTaskRef = taskMaster.HaulingTaskSystem.EnqueueTask(() =>
            {
                if (!SaveManager.Instance.IsLoading && ControllerManager.Instance.InventoryController.HasSpaceForItem(this))
                {
                    ControllerManager.Instance.InventoryController.AddItemToPending(this);
                    var slot = ControllerManager.Instance.InventoryController.GetAvailableStorageSlot(this);
                    _originalParent = transform.parent;
                    return CreateHaulTaskForSlot(slot);
                }
                else
                {
                    return null;
                }
            }).GetHashCode();
        }

        public HaulingTask.TakeItemToItemSlot CreateHaulTaskForSlot(StorageSlot slot)
        {
            _originalParent = transform.parent;
            var task = new HaulingTask.TakeItemToItemSlot
            {
                TargetUID = UniqueId,
                item = this,
                claimItemSlot = (UnitTaskAI unitTaskAI) =>
                {
                    PendingTask = TaskType.None;
                    unitTaskAI.claimedSlot = slot;
                    _incomingUnit = unitTaskAI;
                },
                itemPosition = transform.position,
                grabItem = (UnitTaskAI unitTaskAI) =>
                {
                    unitTaskAI.AssignHeldItem(this);
                },
                dropItem = () =>
                {
                    ControllerManager.Instance.InventoryController.AddToInventory(_itemData, 1);
                    Destroy(gameObject);
                },
            };
            PendingTask = TaskType.TakeItemToItemSlot;
            _assignedTaskRef = task.GetHashCode();
            return task;
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
                CreateHaulTask();
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
        }

        private void OnDestroy()
        {
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

        public List<Order> GetOrders()
        {
            List<Order> results = new List<Order>();

            if (IsAllowed)
            {
                results.Add(Order.Disallow);
            }
            else
            {
                results.Add(Order.Allow);
            }
            

            return results;
        }

        public List<ActionBase> GetActions()
        {
            return AvailableActions;
        }

        public bool IsOrderActive(Order order)
        {
            switch (order)
            {
                case Order.Disallow:
                    return !IsAllowed;
                default:
                    throw new ArgumentOutOfRangeException(nameof(order), order, null);
            }
        }

        public void AssignOrder(Order orderToAssign)
        {
            switch (orderToAssign)
            {
                case Order.Allow:
                    ToggleAllowed(true);
                    break;
                case Order.Disallow:
                    ToggleAllowed(false);
                    break;
            }
        }
        
        protected virtual void RestorePendingTask(TaskType pendingTask)
        {
            if(pendingTask == TaskType.None) return;

            if (pendingTask == TaskType.TakeItemToItemSlot)
            {
                CreateHaulTask();
            }
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
            
            InitializeItem(itemState.ItemData, IsAllowed);
            RestorePendingTask(itemState.PendingTask);
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
            public TaskType PendingTask;
        }
    }
}
