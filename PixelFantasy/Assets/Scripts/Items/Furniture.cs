using System;
using System.Collections.Generic;
using Controllers;
using Gods;
using ScriptableObjects;
using Tasks;
using Unit;
using UnityEngine;

namespace Items
{
    public class Furniture : MonoBehaviour
    {
        private FurnitureData _furnitureData;
        protected FurniturePrefab _prefab;
        protected List<ItemAmount> _resourceCost;
        private bool _isBuilt;
        protected List<int> _assignedTaskRefs = new List<int>();
        protected List<Item> _incomingItems = new List<Item>();
        private bool _isDeconstructing;
        private UnitTaskAI _incomingUnit;

        protected Transform UsagePosition => _prefab.UsagePostion;
        public FurnitureData FurnitureData => _furnitureData;
        
        protected TaskMaster taskMaster => TaskMaster.Instance;
        protected CraftMaster craftMaster => CraftMaster.Instance;

        public virtual void Init(FurnitureData furnitureData, PlacementDirection direction)
        {
            _furnitureData = furnitureData;
            _resourceCost = new List<ItemAmount>(_furnitureData.ResourceCosts);
            CreatePrefab(direction);
            ShowBlueprint(true);
            PrepForConstruction();
        }

        private void CreatePrefab(PlacementDirection placementDirection)
        {
            var prefabObj = Instantiate(_furnitureData.GetFurniturePrefab(placementDirection), transform);
            _prefab = prefabObj.GetComponent<FurniturePrefab>();
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
                var resourceCosts = _furnitureData.ResourceCosts;
                CraftMissingItems(resourceCosts);
                CraftingTask craftItem = new CraftingTask();
                craftItem.FurnitureData = _furnitureData;

                CreateCraftingTask(craftItem);

                CreateInstallTask(craftItem);
            }
        }
        
        private void CreateInstallTask(CraftingTask craftingTask)
        {
            var taskRef = taskMaster.HaulingTaskSystem.EnqueueTask(() =>
            {
                Item resource = InventoryController.Instance.ClaimResource(craftingTask.ItemData);
                if (resource != null)
                {
                    var task = new HaulingTask.TakeResourceToBlueprint
                    {
                        resourcePosition = resource.transform.position,
                        blueprintPosition = transform.position,
                        grabResource = (UnitTaskAI unitTaskAI) =>
                        {
                            resource.transform.SetParent(unitTaskAI.transform);
                            InventoryController.Instance.DeductClaimedResource(resource);
                            _incomingItems.Add(resource);
                        },
                        useResource = () =>
                        {
                            _incomingItems.Remove(resource);
                            resource.gameObject.SetActive(false);
                            AddResourceToBlueprint(resource.GetItemData());
                            Destroy(resource.gameObject);
                            InstallFurniture();
                        },
                    };

                    _assignedTaskRefs.Add(task.GetHashCode());
                    return task;
                }
                else
                {
                    return null;
                }
            }).GetHashCode();
            _assignedTaskRefs.Add(taskRef);
        }

        private void CreateConstructionHaulingTasks()
        {
            var resourceCosts = _furnitureData.ResourceCosts;
            
            CraftMissingItems(resourceCosts);
            
            foreach (var resourceCost in resourceCosts)
            {
                for (int i = 0; i < resourceCost.Quantity; i++)
                {
                    CreateTakeResourceToBlueprintTask(resourceCost.Item);
                }
            }
        }

        private void CraftMissingItems(List<ItemAmount> requiredResources)
        {
            foreach (var resource in requiredResources)
            {
                var remainingNeeded = resource.Quantity - InventoryController.Instance.AvailableItemQuantity(resource.Item);
                for (int i = 0; i < remainingNeeded; i++)
                {
                    CraftingTask craft = new CraftingTask();
                    craft.ItemData = resource.Item;
                    CreateCraftingTask(craft);
                }
            }
        }

        private void CreateTakeResourceToBlueprintTask(ItemData resourceData)
        {
            var taskRef = taskMaster.HaulingTaskSystem.EnqueueTask(() =>
            {
                Item resource = InventoryController.Instance.ClaimResource(resourceData);
                if (resource != null)
                {
                    var task = new HaulingTask.TakeResourceToBlueprint
                    {
                        resourcePosition = resource.transform.position,
                        blueprintPosition = transform.position,
                        grabResource = (UnitTaskAI unitTaskAI) =>
                        {
                            resource.transform.SetParent(unitTaskAI.transform);
                            InventoryController.Instance.DeductClaimedResource(resource);
                            _incomingItems.Add(resource);
                        },
                        useResource = () =>
                        {
                            _incomingItems.Remove(resource);
                            resource.gameObject.SetActive(false);
                            AddResourceToBlueprint(resource.GetItemData());
                            Destroy(resource.gameObject);
                            CheckIfAllResourcesLoaded();
                        },
                    };

                    _assignedTaskRefs.Add(task.GetHashCode());
                    return task;
                }
                else
                {
                    return null;
                }
            }).GetHashCode();
            _assignedTaskRefs.Add(taskRef);
        }

        private void CreateCraftingTask(CraftingTask itemToCraft)
        {
            craftMaster.CreateCraftingTask(itemToCraft);
        }

        private void ItemWasCrafted(Furniture furniture, Item craftedItem)
        {
            if (furniture == this && craftedItem != null)
            {
                var task = new HaulingTask.TakeResourceToBlueprint
                {
                    resourcePosition = craftedItem.transform.position,
                    blueprintPosition = transform.position,
                    grabResource = (UnitTaskAI unitTaskAI) =>
                    {
                        craftedItem.transform.SetParent(unitTaskAI.transform);
                        InventoryController.Instance.DeductClaimedResource(craftedItem);
                        _incomingItems.Add(craftedItem);
                    },
                    useResource = () =>
                    {
                        _incomingItems.Remove(craftedItem);
                        craftedItem.gameObject.SetActive(false);
                        AddResourceToBlueprint(craftedItem.GetItemData());
                        Destroy(craftedItem.gameObject);
                        CheckIfAllResourcesLoaded();
                    },
                };

                _assignedTaskRefs.Add(task.GetHashCode());
            }
        }

        private void AddResourceToBlueprint(ItemData itemData)
        {
            foreach (var cost in _resourceCost)
            {
                if (cost.Item == itemData && cost.Quantity > 0)
                {
                    cost.Quantity--;
                    if (cost.Quantity <= 0)
                    {
                        _resourceCost.Remove(cost);
                    }

                    return;
                }
            }
        }
        
        private void CheckIfAllResourcesLoaded()
        {
            if (_resourceCost.Count == 0)
            {
                CreateConstructFurnitureTask();
            }
        }
        
        private void CreateConstructFurnitureTask()
        {
            // Clear old refs
            _assignedTaskRefs.Clear();
            
            var task = new ConstructionTask.ConstructStructure
            {
                structurePosition = transform.position,
                workAmount = _furnitureData.GetWorkPerResource(),
                completeWork = CompleteConstruction
            };
            
            _assignedTaskRefs.Add(task.GetHashCode());
            
            taskMaster.ConstructionTaskSystem.AddTask(task);
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
    }

    [Serializable]
    public enum PlacementDirection
    {
        Down,
        Up,
        Left, 
        Right
    }

    [Serializable]
    public enum ConstructionMethod
    {
        None, // Aka: Natural Resource
        Hand,
        Carpentry
    }
}
