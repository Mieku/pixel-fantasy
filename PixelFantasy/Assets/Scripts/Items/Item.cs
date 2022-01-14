using System;
using Controllers;
using Gods;
using Tasks;
using Unit;
using UnityEngine;

namespace Items
{
    public class Item : MonoBehaviour
    {
        [SerializeField] private ItemData _itemData;
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private bool _canHaul;

        private TaskMaster taskMaster => TaskMaster.Instance;
        
        public void InitializeItem(ItemData itemData, bool canHaul)
        {
            _itemData = itemData;
            _canHaul = canHaul;
            
            DisplayItemSprite();

            if (_canHaul)
            {
                CreateHaulTask();
            }
        }
        
        private void CreateHaulTask()
        {
            taskMaster.HaulingTaskSystem.EnqueueTask(() =>
            {
                if (InventoryController.Instance.HasSpaceForItem(this))
                {
                    var slot = InventoryController.Instance.GetAvailableStorageSlot(this);
                    var originalParent = transform.parent;
                    var task = new HaulingTask.TakeItemToItemSlot
                    {
                        item = this,
                        claimItemSlot = (UnitTaskAI unitTaskAI) =>
                        {
                            unitTaskAI.claimedSlot = slot;
                        },
                        itemPosition = transform.position,
                        grabItem = (UnitTaskAI unitTaskAI) =>
                        {
                            transform.SetParent(unitTaskAI.transform);
                        },
                        dropItem = () =>
                        {
                            transform.position = Helper.ConvertMousePosToGridPos(transform.position);
                            transform.SetParent(originalParent);
                            InventoryController.Instance.AddToInventory(_itemData, 1);
                        },
                    };
                    return task;
                }
                else
                {
                    return null;
                }
            });
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
    }
}
