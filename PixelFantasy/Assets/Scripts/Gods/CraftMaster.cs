using System;
using System.Collections.Generic;
using Controllers;
using Items;
using ScriptableObjects;
using Tasks;
using Characters;
using UnityEngine;
using Zones;

namespace Gods
{
    public class CraftMaster : God<CraftMaster>
    {
        private List<CraftingTable> _carpentryTables = new List<CraftingTable>();
        private List<CraftingTask> _carpentryTaskList = new List<CraftingTask>();

        public ItemData plank; // TODO: Remove this after testing
        private const float QUEUE_CHECK_TIMER = 0.2f; // 200ms

        public TaskType PendingTask;
        private TaskMaster taskMaster => TaskMaster.Instance;

        protected override void Awake()
        {
            base.Awake();
            GameEvents.OnSavingGameBeginning += OnBeginSave;
        }

        private void OnDestroy()
        {
            GameEvents.OnSavingGameBeginning -= OnBeginSave;
        }

        private void OnBeginSave()
        {
            _carpentryTables.Clear();
            _carpentryTaskList.Clear();
        }

        private void Start()
        {
            InvokeRepeating(nameof(CheckCraftingQueues), 0, QUEUE_CHECK_TIMER);
        }

        private void CheckCraftingQueues()
        {
            CheckCraftingQueue(_carpentryTaskList);
        }

        private void CheckCraftingQueue(List<CraftingTask> craftingQueue)
        {
            for (int i = craftingQueue.Count - 1; i >= 0; i--)
            {
                var task = craftingQueue[i];
                craftingQueue.Remove(task);
                CreateCraftingTask(task);
            }
        }

        private void Update()
        {
            // if (Input.GetKeyDown(KeyCode.P))
            // {
            //     Debug.Log("Plank!");
            //     CraftingTask craft = new CraftingTask();
            //     craft.ItemData = plank;
            //     CreateCraftingTask(craft);
            // }
        }

        public void AddCraftingTable(CraftingTable table, CraftingType craftingType)
        {
            if (craftingType == CraftingType.Carpentry)
            {
                _carpentryTables.Add(table);
            }
        }
        
        public void RemoveCraftingTable(CraftingTable table, CraftingType craftingType)
        {
            if (craftingType == CraftingType.Carpentry)
            {
                _carpentryTables.Remove(table);
            }
        }

        public void CreateCraftingTask(CraftingTask craftingTask)
        {
            var method = craftingTask.ConstructionMethod;
            if (method == ConstructionMethod.None) return;
            
            switch (method)
            {
                case ConstructionMethod.Carpentry:
                    CreateCarpentryTask(craftingTask);
                    break;
                default:
                    Debug.LogError($"Unknown method: {method}");
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void CreateCarpentryTask(CraftingTask craftingTask)
        {
            CraftingTable table = GetAvailableCraftingTable(craftingTask.ConstructionMethod);
            var requiredResources = craftingTask.ResourceCosts;

            bool resourcesAvailable = CheckResourcesAvailable(requiredResources);
            
            // Claim the resources

            if (resourcesAvailable && table != null)
            {
                table.AssignCraft(craftingTask);
                foreach (var claimedResource in requiredResources)
                {
                    for (int i = 0; i < claimedResource.Quantity; i++)
                    {
                        EnqueueCreateGatherResourceForCraftingTask(claimedResource.Item, table);
                    }

                }
            }
            else
            {
                // Queue this item for later
                _carpentryTaskList.Add(craftingTask);
            }
        }
        
        private void EnqueueCreateGatherResourceForCraftingTask(ItemData resourceData, CraftingTable table)
        {
            var taskRef = taskMaster.CarpentryTaskSystem.EnqueueTask(() =>
            {
                StorageSlot slot = ControllerManager.Instance.InventoryController.ClaimResource(resourceData);
                if (slot != null)
                {
                    return CreateGatherResourceForCraftingTask(slot, table);
                }
                else
                {
                    return null;
                }
            }).GetHashCode();
        }
        
        public CarpentryTask.GatherResourceForCrafting CreateGatherResourceForCraftingTask(Item item, CraftingTable table)
        {
            var task = new CarpentryTask.GatherResourceForCrafting
            {
                TargetUID = table.UniqueId,
                resourcePosition = item.transform.position,
                craftingTable = table,
                grabResource = (UnitTaskAI unitTaskAI) =>
                {
                    PendingTask = TaskType.None;
                    
                    item.gameObject.SetActive(true);
                    unitTaskAI.AssignHeldItem(item);
                    
                    table.AddIncomingItem(item);
                },
                useResource = (heldItem) =>
                {
                    table.RemoveIncomingItem(heldItem);
                    heldItem.gameObject.SetActive(false);
                    table.AddResourceToCrafting(heldItem.GetItemData());
                    Destroy(heldItem.gameObject);
                    table.CheckIfAllResourcesLoadedInCrafting();
                }
            };
                   
            PendingTask = TaskType.Carpentry_GatherResourceForCrafting;
            table.AddTaskReference(task.GetHashCode());
            return task;
        }

        public CarpentryTask.GatherResourceForCrafting CreateGatherResourceForCraftingTask(StorageSlot slot, CraftingTable table)
        {
            Item resource;
            var task = new CarpentryTask.GatherResourceForCrafting
            {
                TargetUID = table.UniqueId,
                resourcePosition = slot.transform.position,
                craftingTable = table,
                grabResource = (UnitTaskAI unitTaskAI) =>
                {
                    PendingTask = TaskType.None;
                    
                    // Get item from the slot
                    resource = slot.GetItem();
                    
                    resource.gameObject.SetActive(true);
                    unitTaskAI.AssignHeldItem(resource);
                    
                    table.AddIncomingItem(resource);
                },
                useResource = (resource) =>
                {
                    table.RemoveIncomingItem(resource);
                    resource.gameObject.SetActive(false);
                    table.AddResourceToCrafting(resource.GetItemData());
                    Destroy(resource.gameObject);
                    table.CheckIfAllResourcesLoadedInCrafting();
                }
            };
                   
            PendingTask = TaskType.Carpentry_GatherResourceForCrafting;
            table.AddTaskReference(task.GetHashCode());
            return task;
        }
        
        private bool CheckResourcesAvailable(List<ItemAmount> resources)
        {
            foreach (var resource in resources)
            {
                var isAvailable = ControllerManager.Instance.InventoryController.HasItemAvailable(resource.Item, resource.Quantity);
                if (!isAvailable)
                {
                    return false;
                }
            }

            return true;
        }

        public CraftingTable GetAvailableCraftingTable(ConstructionMethod method)
        {
            switch (method)
            {
                case ConstructionMethod.Carpentry:
                    if (_carpentryTables.Count == 0) return null;
                    foreach (var table in _carpentryTables)
                    {
                        if (!table.IsCraftingItem)
                        {
                            return table;
                        }
                    }
                    
                    return null;
                default:
                    throw new ArgumentOutOfRangeException(nameof(method), method, null);
            }
        }
    }
    
        
    [Serializable]
    public enum CraftingType
    {
        None,
        Carpentry,
    }
    
    public class CraftingTask
    {
        private ItemData _itemData;
        private FurnitureData _furnitureData;

        public ItemData ItemData
        {
            get
            {
                if (_itemData != null)
                {
                    return _itemData;
                }

                if (_furnitureData != null)
                {
                    return _furnitureData.ItemData;
                }
                
                Debug.LogError("Missing ItemData");
                return null;
            }
            set => _itemData = value;
        }

        public FurnitureData FurnitureData
        {
            get => _furnitureData;
            set => _furnitureData = value;
        }

        public ConstructionMethod ConstructionMethod
        {
            get
            {
                if (_itemData != null)
                {
                    return _itemData.ConstructionMethod;
                }

                if (_furnitureData != null)
                {
                    return _furnitureData.ConstructionMethod;
                }

                return ConstructionMethod.None;
            }
        }
        
        public List<ItemAmount> ResourceCosts
        {
            get
            {
                if (_itemData != null)
                {
                    return _itemData.ResourceCosts;
                }

                if (_furnitureData != null)
                {
                    return _furnitureData.ResourceCosts;
                }

                return null;
            }
        }

        public float WorkToCraft
        {
            get
            {
                if (_itemData != null)
                {
                    return _itemData.WorkToCraft;
                }

                if (_furnitureData != null)
                {
                    return _furnitureData.WorkCost;
                }

                return 0f;
            }
        }
    }
}
