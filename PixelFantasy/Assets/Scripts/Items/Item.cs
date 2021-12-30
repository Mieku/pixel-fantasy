using System;
using Controllers;
using Gods;
using Sirenix.OdinInspector;
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
                ItemSlot emptySlot = InventoryController.Instance.GetAvailableItemSlot();
                if (emptySlot != null)
                {
                    emptySlot.HasItemIncoming(true);
                    var originalParent = transform.parent;
                    var task = new HaulingTask.TakeItemToItemSlot
                    {
                        itemPosition = transform.position,
                        itemSlotPosition = emptySlot.GetPosition(),
                        grabItem = (UnitTaskAI unitTaskAI) =>
                        {
                            transform.SetParent(unitTaskAI.transform);
                        },
                        dropItem = () =>
                        {
                            transform.SetParent(originalParent);
                            emptySlot.SetItemTransform(transform);
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

        private void OnValidate()
        {
            DisplayItemSprite();
        }
    }
}
