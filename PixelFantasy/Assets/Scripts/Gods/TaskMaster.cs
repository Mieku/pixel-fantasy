using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using CodeMonkey.Utils;
using Gods;
using Unit;
using UnityEngine;
using UnityEngine.Rendering;

namespace Gods
{
    public class TaskMaster : God<TaskMaster>
    {
        private TaskSystem taskSystem = new TaskSystem();
        private List<ItemSlot> itemSlots = new List<ItemSlot>();
        
        // For Testing placeholder
        public Sprite rubble;
        public Sprite wood;
        public Sprite whitePixel;

        public TaskSystem GetTaskSystem()
        {
            return taskSystem;
        }

        private void Start()
        {
            FunctionPeriodic.Create(taskSystem.DequeueTasks, 0.2f);
        }

        private void Update()
        {
            // Left Click
            if (Input.GetMouseButtonDown(0))
            {
                CreateWood();
            }
            
            // Right Click
            if (Input.GetMouseButtonDown(1))
            {
                CreateItemSlot();
            }
        }

        // TODO: Remove these when no longer needed
        #region Used For Testing

        private void CreateItemSlot()
        {
            var itemSlotGO = SpawnItemSlot(UtilsClass.GetMouseWorldPosition());
            ItemSlot itemSlot = new ItemSlot(itemSlotGO.transform);
            itemSlots.Add(itemSlot);
        }

        private void CreateWood()
        {
            var woodGO = SpawnWood(UtilsClass.GetMouseWorldPosition());
            taskSystem.EnqueueTask(() =>
            {
                ItemSlot emptySlot = null;
                foreach (var itemSlot in itemSlots)
                {
                    if (itemSlot.IsEmpty())
                    {
                        emptySlot = itemSlot;
                        break;
                    }
                }

                if (emptySlot != null)
                {
                    emptySlot.HasItemIncoming(true);
                    var task = new TaskSystem.Task.TakeItemToItemSlot
                    {
                        itemPosition = woodGO.transform.position,
                        itemSlotPosition = emptySlot.GetPosition(),
                        grabItem = (UnitTaskAI unitTaskAI) =>
                        {
                            woodGO.transform.SetParent(unitTaskAI.transform);
                        },
                        dropItem = () =>
                        {
                            woodGO.transform.SetParent(null);
                            emptySlot.SetItemTransform(woodGO.transform);
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
        
        private void AssignMoveLocation()
        {
            var newTask = new TaskSystem.Task.MoveToPosition
            {
                targetPosition = UtilsClass.GetMouseWorldPosition()
            };
            taskSystem.AddTask(newTask);
        }

        /// <summary>
        /// A variation of CreateGarbageOnMousePos, that doesn't allow for the task to be available immediately
        /// This is used to demo a task with a conditional
        /// </summary>
        /// <param name="secToWait">The amount of time to wait before this task can be executed</param>
        private void CreateGarbageAndWait(float secToWait)
        {
            var garbage = SpawnRubble(UtilsClass.GetMouseWorldPosition());
            var garbageSpriteRenderer = garbage.GetComponent<SpriteRenderer>();
            float cleanupTime = Time.time + secToWait;
            
            taskSystem.EnqueueTask(() =>
            {
                if (Time.time >= cleanupTime)
                {
                    var newTask = new TaskSystem.Task.GarbageCleanup
                    {
                        targetPosition = garbage.transform.position,
                        cleanUpAction = () =>
                        {
                            float alpha = 1f;
                            FunctionUpdater.Create(() =>
                            {
                                alpha -= Time.deltaTime;
                                garbageSpriteRenderer.color = new Color(1, 1, 1, alpha);
                                if (alpha <= 0f)
                                {
                                    Destroy(garbage);
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            });
                        }
                    };
                    return newTask;
                }
                else
                {
                    return null;
                }
            });
        }
        
        private void CreateGarbageOnMousePos()
        {
            var garbage = SpawnRubble(UtilsClass.GetMouseWorldPosition());
            var garbageSpriteRenderer = garbage.GetComponent<SpriteRenderer>();
            var newTask = new TaskSystem.Task.GarbageCleanup
            {
                targetPosition = garbage.transform.position,
                cleanUpAction = () =>
                {
                    float alpha = 1f;
                    FunctionUpdater.Create(() =>
                    {
                        alpha -= Time.deltaTime * 1.1f;
                        garbageSpriteRenderer.color = new Color(1, 1, 1, alpha);
                        if (alpha <= 0f)
                        {
                            Destroy(garbage);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    });
                }
            };
            taskSystem.AddTask(newTask);
        }

        private GameObject SpawnRubble(Vector3 position)
        {
            GameObject gameObject = new GameObject("Rubble", typeof(SpriteRenderer));
            gameObject.GetComponent<SpriteRenderer>().sprite = rubble;
            gameObject.GetComponent<SpriteRenderer>().sortingLayerName = "Item";
            gameObject.transform.position = position;

            return gameObject;
        }
        
        private GameObject SpawnWood(Vector3 position)
        {
            GameObject gameObject = new GameObject("Wood", typeof(SpriteRenderer));
            gameObject.GetComponent<SpriteRenderer>().sprite = wood;
            gameObject.GetComponent<SpriteRenderer>().sortingLayerName = "Item";
            gameObject.transform.position = position;
            gameObject.transform.localScale = new Vector3(.75f, .75f);

            return gameObject;
        }
        
        private GameObject SpawnItemSlot(Vector3 position)
        {
            GameObject gameObject = new GameObject("Item Slot", typeof(SpriteRenderer));
            gameObject.GetComponent<SpriteRenderer>().sprite = whitePixel;
            gameObject.GetComponent<SpriteRenderer>().color = new Color(.5f, .5f, .5f);
            gameObject.GetComponent<SpriteRenderer>().sortingLayerName = "Ground";
            gameObject.GetComponent<SpriteRenderer>().sortingOrder = 10;
            gameObject.transform.position = position;
            gameObject.transform.localScale = new Vector3(1, 1);

            return gameObject;
        }

        #endregion
        
        // TODO: For testing
        private class ItemSlot
        {
            private Transform itemSlotTransform;
            private Transform itemTransform;
            private bool hasItemIncoming;

            public ItemSlot(Transform itemSlotTransform)
            {
                this.itemSlotTransform = itemSlotTransform;
                SetItemTransform(null);
            }

            public bool IsEmpty()
            {
                return itemTransform == null && !hasItemIncoming;
            }

            public void HasItemIncoming(bool hasItemIncoming)
            {
                this.hasItemIncoming = hasItemIncoming;
            }

            public void SetItemTransform(Transform itemTransform)
            {
                this.itemTransform = itemTransform;
                hasItemIncoming = false;
                UpdateSprite();
            }

            public Vector3 GetPosition()
            {
                return itemSlotTransform.position;
            }

            public void UpdateSprite()
            {
                itemSlotTransform.GetComponent<SpriteRenderer>().color = IsEmpty() ? Color.gray : Color.red;
            }
        }
    }
}
