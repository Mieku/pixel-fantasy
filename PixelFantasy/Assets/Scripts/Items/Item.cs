using System.Collections.Generic;
using System.Linq;
using AI;
using Characters;
using Handlers;
using Managers;
using UnityEngine;
using Task = AI.Task;

namespace Items
{
    public class Item : PlayerInteractable
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        
        private Transform _originalParent;
        
        public ItemData RuntimeData;
        public override string UniqueID => RuntimeData.UniqueID;

        public override string PendingTaskUID
        {
            get => RuntimeData.PendingTaskUID;
            set => RuntimeData.PendingTaskUID = value;
        }

        public override bool IsSimilar(PlayerInteractable otherPI)
        {
            if (otherPI is Item otherItem)
            {
                return RuntimeData.Settings == otherItem.RuntimeData.Settings;
            }

            return false;
        }

        public void LoadItemData(ItemData data)
        {
            RuntimeData = data;
            DisplayItemSprite();

            PlayerInteractableDatabase.Instance.RegisterPlayerInteractable(this);

            if (RuntimeData.State == EItemState.Loose)
            {
                PlaceOnGround();
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
            Task task = new Task("Store Item", $"Storing {RuntimeData.ItemName}", ETaskType.Hauling, this);
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
            
            PlaceOnGround();
        }

        private void PlaceOnGround()
        {
            var groundPos = Helper.SnapToGridPos(transform.position);
            var checkedPositions = new HashSet<Vector2>(); // To track checked positions
            int radius = 0;
            int maxRadius = 5; // Adjust the max radius as needed

            while (radius <= maxRadius)
            {
                var positionsToCheck = GetPositionsInRadius(groundPos, radius, checkedPositions);

                foreach (var pos in positionsToCheck)
                {
                    // Check if this position has already been checked
                    if (checkedPositions.Contains(pos))
                    {
                        continue;
                    }

                    // Mark this position as checked
                    checkedPositions.Add(pos);

                    // Check pos for item or stack
                    bool isPosInvalid = Helper.DoesGridContainTag(pos, "Obstacle");
                    if (isPosInvalid)
                    {
                        continue; // No position available, try next adjacent pos
                    }

                    ItemStack stack = ItemsDatabase.Instance.FindItemStackAtPosition(pos);
                    if (stack != null)
                    {
                        if (stack.CanItemJoinStack(RuntimeData))
                        {
                            stack.AddItemToStack(this);
                            return;
                        }
                        continue; // No position available, try next adjacent pos
                    } 

                    var itemDatasAtPos = ItemsDatabase.Instance.FindAllItemDatasAtPosition(pos);
                    var looseItem = itemDatasAtPos.Find(i => i.UniqueID != RuntimeData.UniqueID && i.State == EItemState.Loose);
                    if (looseItem != null)
                    {
                        if (RuntimeData.CanFormStack(looseItem))
                        {
                            // Create Stack
                            ItemsDatabase.Instance.CreateStack(looseItem.GetLinkedItem(), this);
                            return;
                        }
                        continue; // No position available, try next adjacent pos
                    }
                    
                    // Spot is free
                    transform.position = pos;
                    RuntimeData.Position = pos;
                    RuntimeData.State = EItemState.Loose;
                    if (IsAllowed)
                    {
                        SeekForSlot();
                    }
                    return;
                }

                // Increase the search radius if no valid position was found
                radius++;
            }

            // If no valid position found after expanding the search radius
            Debug.LogWarning("No valid position found to place the item, placing it loosely on the ground");
            transform.position = groundPos;
            RuntimeData.Position = groundPos;
            RuntimeData.State = EItemState.Loose;
            if (IsAllowed)
            {
                SeekForSlot();
            }
        }

        private List<Vector2> GetPositionsInRadius(Vector2 center, int radius, HashSet<Vector2> checkedPositions)
        {
            var positions = new List<Vector2>();

            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    // Only include positions at the edges of the current radius
                    if (Mathf.Abs(x) == radius || Mathf.Abs(y) == radius)
                    {
                        var pos = new Vector2(center.x + x, center.y + y);

                        if (!checkedPositions.Contains(pos))
                        {
                            positions.Add(pos);
                        }
                    }
                }
            }

            return positions;
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

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_isQuitting) return;
            
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
            if (IsSelected)
            {
                GameEvents.Trigger_RefreshSelection();
            }
        }

        public override string DisplayName => RuntimeData.Settings.ItemName;

        public override Vector2? UseagePosition(Vector2 requestorPosition)
        {
            return transform.position;
        }
    }
}
