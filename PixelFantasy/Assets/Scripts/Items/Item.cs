using System;
using Controllers;
using Gods;
using Interfaces;
using ScriptableObjects;
using Tasks;
using Unit;
using UnityEngine;

namespace Items
{
    public class Item : MonoBehaviour, IClickableObject
    {
        [SerializeField] private ItemData _itemData;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private SpriteRenderer _icon;

        private int _assignedTaskRef;
        private UnitTaskAI _incomingUnit;
        private Transform _originalParent;
        private bool isAllowed;

        private TaskMaster taskMaster => TaskMaster.Instance;
        
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
                            transform.position = Helper.ConvertMousePosToGridPos(transform.position);
                            transform.SetParent(_originalParent);
                            InventoryController.Instance.AddToInventory(_itemData, 1);
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
        }

        public bool IsClickDisabled { get; set; }
    }
}
