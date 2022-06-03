using System;
using System.Collections.Generic;
using Controllers;
using DataPersistence;
using Gods;
using Interfaces;
using ScriptableObjects;
using Tasks;
using Unit;
using UnityEngine;

namespace Items
{
    public class Item : MonoBehaviour, IClickableObject, IPersistent
    {
        [SerializeField] private ItemData _itemData;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private SpriteRenderer _icon;
        [SerializeField] private ClickObject _clickObject;

        private int _assignedTaskRef;
        private UnitTaskAI _incomingUnit;
        private Transform _originalParent;

        private TaskMaster taskMaster => TaskMaster.Instance;

        public ClickObject GetClickObject()
        {
            return _clickObject;
        }

        public void InitializeItem(ItemData itemData, bool allowed)
        {
            _itemData = itemData;
            IsAllowed = allowed;
            
            DisplayItemSprite();

            if (IsAllowed)
            {
                CreateHaulTask();
            }
        }
        
        public void CreateHaulTask()
        {
            _assignedTaskRef = taskMaster.HaulingTaskSystem.EnqueueTask(() =>
            {
                if (InventoryController.Instance.HasSpaceForItem(this))
                {
                    var slot = InventoryController.Instance.GetAvailableStorageSlot(this);
                    _originalParent = transform.parent;
                    var task = new HaulingTask.TakeItemToItemSlot
                    {
                        item = this,
                        claimItemSlot = (UnitTaskAI unitTaskAI) =>
                        {
                            unitTaskAI.claimedSlot = slot;
                            _incomingUnit = unitTaskAI;
                        },
                        itemPosition = transform.position,
                        grabItem = (UnitTaskAI unitTaskAI) =>
                        {
                            transform.SetParent(unitTaskAI.transform);
                        },
                        dropItem = () =>
                        {
                            InventoryController.Instance.AddToInventory(_itemData, 1);
                            Destroy(gameObject);
                        },
                    };
                    _assignedTaskRef = task.GetHashCode();
                    return task;
                }
                else
                {
                    return null;
                }
            }).GetHashCode();
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
                _icon.gameObject.SetActive(false);
                _icon.sprite = null;
                CreateHaulTask();
            }
            else
            {
                _icon.gameObject.SetActive(true);
                _icon.sprite = Librarian.Instance.GetSprite("Lock");
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

        public object CaptureState()
        {
            return new Data
            {
                Position = transform.position,
                ItemData = _itemData,
                AssignedTaskRef = _assignedTaskRef,
                IncomingUnit = _incomingUnit,
                OriginalParent = _originalParent,
                IsAllowed = this.IsAllowed,
                IsClickDisabled = this.IsClickDisabled,
            };
        }

        public void RestoreState(object data)
        {
            var itemState = (Data)data;

            transform.position = itemState.Position;
            _assignedTaskRef = itemState.AssignedTaskRef;
            _incomingUnit = itemState.IncomingUnit;
            _originalParent = itemState.OriginalParent;
            IsAllowed = itemState.IsAllowed;
            IsClickDisabled = itemState.IsClickDisabled;
            
            InitializeItem(itemState.ItemData, IsAllowed);
        }

        public struct Data
        {
            public Vector3 Position;
            public ItemData ItemData;
            public int AssignedTaskRef;
            public UnitTaskAI IncomingUnit;
            public Transform OriginalParent;
            public bool IsAllowed;
            public bool IsClickDisabled;
        }
    }
}
