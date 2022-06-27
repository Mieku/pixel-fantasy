using System;
using System.Collections.Generic;
using System.Xml;
using Actions;
using Controllers;
using Gods;
using ScriptableObjects;
using Tasks;
using Characters;
using DataPersistence;
using UnityEngine;

namespace Items
{
    public class Furniture : Interactable, IPersistent
    {
        [SerializeField] private ActionTakeResourceToBlueprint _takeResourceToBlueprintAction;
        
        private FurnitureData _furnitureData;
        protected FurniturePrefab _prefab;
        protected List<ItemAmount> _remainingResourceCosts;
        private List<ItemAmount> _pendingResourceCosts; // Claimed by a task but not used yet
        private List<ItemAmount> _incomingResourceCosts; // The item is on its way
        protected bool _isBuilt;
        protected List<int> _assignedTaskRefs = new List<int>();
        protected List<Item> _incomingItems = new List<Item>();
        private bool _isDeconstructing;
        private UnitTaskAI _incomingUnit;
        protected bool _isCraftingTable;
        private PlacementDirection _placementDirection;

        public TaskType PendingTask;
        
        protected Transform UsagePosition => _prefab.UsagePostion;
        public FurnitureData FurnitureData => _furnitureData;
        
        protected TaskMaster taskMaster => TaskMaster.Instance;
        protected CraftMaster craftMaster => CraftMaster.Instance;

        public virtual void Init(FurnitureData furnitureData, PlacementDirection direction)
        {
            _furnitureData = furnitureData;
            _remainingResourceCosts = new List<ItemAmount>(_furnitureData.ResourceCosts);
            _pendingResourceCosts = new List<ItemAmount>();
            _placementDirection = direction;
            CreatePrefab(direction);
            ShowBlueprint(true);
            PrepForConstruction();
        }

        private void CreatePrefab(PlacementDirection placementDirection)
        {
            var prefabObj = Instantiate(_furnitureData.GetFurniturePrefab(placementDirection), transform);
            _prefab = prefabObj.GetComponent<FurniturePrefab>();
        }
        
        public void AddToIncomingItems(Item item)
        {
            _incomingItems ??= new List<Item>();
            _incomingItems.Add(item);
            
            _incomingResourceCosts ??= new List<ItemAmount>();

            foreach (var cost in _incomingResourceCosts)
            {
                if (cost.Item == item.GetItemData())
                {
                    cost.Quantity += 1;
                    return;
                }
            }
            
            _incomingResourceCosts.Add(new ItemAmount
            {
                Item = item.GetItemData(),
                Quantity = 1
            });
        }
        
        public void RemoveFromIncomingItems(Item item)
        {
            _incomingItems ??= new List<Item>();
            _incomingItems.Remove(item);
            
            foreach (var cost in _incomingResourceCosts)
            {
                if (cost.Item == item.GetItemData())
                {
                    cost.Quantity -= 1;
                    if (cost.Quantity <= 0)
                    {
                        _incomingResourceCosts.Remove(cost);
                    }

                    return;
                }
            }
        }
        
        private void ShowBlueprint(bool showBlueprint)
        {
            if (showBlueprint)
            {
                _prefab.FurnitureRenderer.color = Librarian.Instance.GetColour("Blueprint");
            }
            else
            {
                _prefab.FurnitureRenderer.color = Color.white;
                gameObject.layer = 4;
            }
        }

        private void PrepForConstruction()
        {
            if (_furnitureData.ConstructionMethod == ConstructionMethod.Hand)
            {
                // Build like you would a structure
                CreateConstructionHaulingTasks();
            }
            else
            {
                var resourceCosts = GetRemainingMissingItems();
                CraftMissingItems(resourceCosts);
                CraftingTask craftItem = new CraftingTask();
                craftItem.FurnitureData = _furnitureData;

                CreateCraftingTask(craftItem);

                EnqueueCreateInstallTask(craftItem);
            }
        }

        private void EnqueueCreateInstallTask(CraftingTask craftingTask)
        {
            var taskRef = taskMaster.HaulingTaskSystem.EnqueueTask(() =>
            {
                var slot = ControllerManager.Instance.InventoryController.ClaimResource(craftingTask.ItemData);
                if (slot != null)
                {
                    return CreateInstallTask(slot);
                }
                else
                {
                    return null;
                }
            }).GetHashCode();
            _assignedTaskRefs.Add(taskRef);
        }
        
        private HaulingTask.TakeResourceToBlueprint CreateInstallTask(StorageSlot slot)
        {
            Item resource;
            var task = new HaulingTask.TakeResourceToBlueprint
            {
                TargetUID = UniqueId,
                resourcePosition = slot.transform.position,
                blueprintPosition = transform.position,
                grabResource = (UnitTaskAI unitTaskAI) =>
                {
                    PendingTask = TaskType.None;
                    
                    // Get item from the slot
                    resource = slot.GetItem();
                            
                    resource.gameObject.SetActive(true);
                    unitTaskAI.AssignHeldItem(resource);
                            
                    _incomingItems.Add(resource);
                },
                useResource = (heldItem) =>
                {
                    _incomingItems.Remove(heldItem);
                    heldItem.gameObject.SetActive(false);
                    AddResourceToBlueprint(heldItem.GetItemData());
                    Destroy(heldItem.gameObject);
                    InstallFurniture();
                },
            };

            PendingTask = TaskType.TakeResourceToBlueprint;
            _assignedTaskRefs.Add(task.GetHashCode());
            return task;
        }

        private void CreateConstructionHaulingTasks()
        {
            var resourceCosts = GetRemainingMissingItems();
            
            
            CraftMissingItems(resourceCosts);
            
            foreach (var resourceCost in resourceCosts)
            {
                for (int i = 0; i < resourceCost.Quantity; i++)
                {
                    EnqueueCreateTakeResourceToBlueprintTask(resourceCost.Item);
                }
            }
        }

        protected void CraftMissingItems(List<ItemAmount> requiredResources)
        {
            foreach (var resource in requiredResources)
            {
                float remainingNeeded = resource.Quantity - ControllerManager.Instance.InventoryController.AvailableItemQuantity(resource.Item);
                float amountMadePerCraft = resource.Item.CraftedQuantity;
                var numTasks = (int)Math.Ceiling(remainingNeeded / amountMadePerCraft);

                for (int i = 0; i < numTasks; i++)
                {
                    CraftingTask craft = new CraftingTask();
                    craft.ItemData = resource.Item;
                    CreateCraftingTask(craft);
                }
            }
        }
        
        public void AddToPendingResourceCosts(ItemData itemData, int quantity = 1)
        {
            _pendingResourceCosts ??= new List<ItemAmount>();

            foreach (var cost in _pendingResourceCosts)
            {
                if (cost.Item == itemData)
                {
                    cost.Quantity += quantity;
                    return;
                }
            }
            
            _pendingResourceCosts.Add(new ItemAmount
            {
                Item = itemData,
                Quantity = quantity
            });
        }

        public void RemoveFromPendingResourceCosts(ItemData itemData, int quantity = 1)
        {
            foreach (var cost in _pendingResourceCosts)
            {
                if (cost.Item == itemData)
                {
                    cost.Quantity -= quantity;
                    if (cost.Quantity <= 0)
                    {
                        _pendingResourceCosts.Remove(cost);
                    }

                    return;
                }
            }
        }
        
        public List<ItemAmount> GetRemainingMissingItems()
        {
            _pendingResourceCosts ??= new List<ItemAmount>();
            _incomingResourceCosts ??= new List<ItemAmount>();
            List<ItemAmount> result = new List<ItemAmount>();
            
            foreach (var remainingResourceCost in _remainingResourceCosts)
            {
                ItemAmount amount = new ItemAmount
                {
                    Item = remainingResourceCost.Item,
                    Quantity = remainingResourceCost.Quantity,
                };
                
                result.Add(amount);
            }
            
            foreach (var pendingCost in _pendingResourceCosts)
            {
                foreach (var resultCost in result)
                {
                    if (resultCost.Item == pendingCost.Item)
                    {
                        resultCost.Quantity -= pendingCost.Quantity;
                        if (resultCost.Quantity <= 0)
                        {
                            result.Remove(resultCost);
                        }
                    }
                }
            }
            
            foreach (var incomingCost in _incomingResourceCosts)
            {
                foreach (var resultCost in result)
                {
                    if (resultCost.Item == incomingCost.Item)
                    {
                        resultCost.Quantity -= incomingCost.Quantity;
                        if (resultCost.Quantity <= 0)
                        {
                            result.Remove(resultCost);
                        }
                    }
                }
            }
            
            return result;
        }

        private void EnqueueCreateTakeResourceToBlueprintTask(ItemData resourceData)
        {
            _takeResourceToBlueprintAction.EnqueueTask(this, resourceData);
        }

        private void CreateCraftingTask(CraftingTask itemToCraft)
        {
            craftMaster.CreateCraftingTask(itemToCraft);
        }

        public void AddResourceToBlueprint(ItemData itemData)
        {
            RemoveFromPendingResourceCosts(itemData);
            
            foreach (var cost in _remainingResourceCosts)
            {
                if (cost.Item == itemData && cost.Quantity > 0)
                {
                    cost.Quantity--;
                    if (cost.Quantity <= 0)
                    {
                        _remainingResourceCosts.Remove(cost);
                    }

                    return;
                }
            }
        }

        public void CheckIfAllResourcesLoaded()
        {
            if (_isBuilt) return;
            
            if (_remainingResourceCosts.Count == 0)
            {
                CreateConstructFurnitureTask();
            }
        }
        
        public ConstructionTask.ConstructStructure CreateConstructFurnitureTask(bool autoAssign = true)
        {
            // Clear old refs
            _assignedTaskRefs.Clear();
            
            var task = new ConstructionTask.ConstructStructure
            {
                TargetUID = UniqueId,
                structurePosition = transform.position,
                workAmount = _furnitureData.GetWorkPerResource(),
                completeWork = CompleteConstruction
            };
            
            _assignedTaskRefs.Add(task.GetHashCode());

            if (autoAssign)
            {
                taskMaster.ConstructionTaskSystem.AddTask(task);
            }

            return task;
        }

        protected virtual void CompleteConstruction()
        {
            ShowBlueprint(false);
            _isBuilt = true;
        }

        protected virtual void InstallFurniture()
        {
            ShowBlueprint(false);
            _isBuilt = true;
        }

        public virtual object CaptureState()
        {
            return new Data
            {
                UID = this.UniqueId,
                Position = transform.position,
                PendingTask = PendingTask,
                FurnitureData = _furnitureData,
                IsBuilt = _isBuilt,
                IsDeconstructing = _isDeconstructing,
                IsCraftingTable = _isCraftingTable,
                RemainingResourceCosts = _remainingResourceCosts,
                PlacementDirection = _placementDirection,
                IncomingResourceCosts = _incomingResourceCosts,
            };
        }

        public virtual void RestoreState(object data)
        {
            var state = (Data)data;

            UniqueId = state.UID;
            transform.position = state.Position;
            PendingTask = state.PendingTask;
            _furnitureData = state.FurnitureData;
            _isBuilt = state.IsBuilt;
            _isDeconstructing = state.IsDeconstructing;
            _isCraftingTable = state.IsCraftingTable;
            _remainingResourceCosts = state.RemainingResourceCosts;
            _placementDirection = state.PlacementDirection;
            _incomingResourceCosts = state.IncomingResourceCosts;
            
            // var missingItems = GetRemainingMissingItems();
            // CraftMissingItems(missingItems);
            if (!_isBuilt)
            {
                PrepForConstruction();
            }
            
            CheckIfAllResourcesLoaded();
            CreatePrefab(_placementDirection);
            ShowBlueprint(!_isBuilt);
            
        }

        public struct Data
        {
            public string UID;
            public Vector3 Position;
            public TaskType PendingTask;
            public FurnitureData FurnitureData;
            public bool IsBuilt;
            public bool IsDeconstructing;
            public bool IsCraftingTable;
            public List<ItemAmount> RemainingResourceCosts;
            public PlacementDirection PlacementDirection;
            public List<ItemAmount> IncomingResourceCosts;

            public CraftingTable.CraftingTableData CraftingTableData;
        }
    }
    
    [Serializable]
    public enum ConstructionMethod
    {
        None, // Aka: Natural Resource
        Hand,
        Carpentry
    }
}
