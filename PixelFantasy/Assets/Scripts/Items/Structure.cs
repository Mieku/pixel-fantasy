using System;
using System.Collections.Generic;
using Controllers;
using DataPersistence;
using Gods;
using HUD;
using Pathfinding;
using ScriptableObjects;
using Sirenix.OdinInspector;
using Tasks;
using Characters;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Items
{
    public class Structure : UniqueObject, IPersistent
    {
        [SerializeField] private ProgressBar _progressBar;
        [SerializeField] private DynamicGridObstacle _gridObstacle;
        [SerializeField] private SpriteRenderer _icon;

        private StructureData _structureData;
        private List<ItemAmount> _remainingResourceCosts;
        private List<ItemAmount> _pendingResourceCosts; // Claimed by a task but not used yet
        private bool _isBuilt;
        private List<int> _assignedTaskRefs = new List<int>();
        private List<Item> _incomingItems = new List<Item>();
        private bool _isDeconstructing;
        private UnitTaskAI _incomingUnit;
        private Tilemap _structureTilemap;

        public TaskType PendingTask;
        
        private TaskMaster taskMaster => TaskMaster.Instance;
        
        private void Awake()
        {
            _structureTilemap = TilemapController.Instance.GetTilemap(TilemapLayer.Structure);
        }

        public void Init(StructureData structureData)
        {
            _structureData = structureData;
            _remainingResourceCosts = new List<ItemAmount> (_structureData.GetResourceCosts());
            _pendingResourceCosts = new List<ItemAmount>();
            SetTile();
            _progressBar.ShowBar(false);
            ShowBlueprint(true);
            PrepForConstruction();
        }

        public StructureData GetStructureData()
        {
            return _structureData;
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

        private void AddResourceToBlueprint(ItemData itemData)
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

        private void CheckIfAllResourcesLoaded()
        {
            if (_isBuilt) return;
            
            if (_remainingResourceCosts.Count == 0)
            {
                CreateConstructStructureTask();
            }
        }

        private void ShowBlueprint(bool showBlueprint)
        {
            if (showBlueprint)
            {
                ColourTile(Librarian.Instance.GetColour("Blueprint"));
                _gridObstacle.enabled = false;
            }
            else
            {
                ColourTile(Color.white);
                _gridObstacle.enabled = true;
                gameObject.layer = 4;
            }
        }

        private void SetTile()
        {
            var cell = _structureTilemap.WorldToCell(transform.position);
            _structureTilemap.SetTile(cell, _structureData.RuleTile);
            InformNearbyFloors();
        }

        private void ClearTile()
        {
            var cell = _structureTilemap.WorldToCell(transform.position);
            _structureTilemap.SetTile(cell, null);
            InformNearbyFloors();
        }

        private void ColourTile(Color colour)
        {
            var cell = _structureTilemap.WorldToCell(transform.position);
            _structureTilemap.SetColor(cell, colour);
        }

        private void InformNearbyFloors()
        {
            var position = transform.position;
            var leftPos = new Vector2(position.x - 1, position.y);
            var rightPos = new Vector2(position.x + 1, position.y);

            var leftFloors = Helper.GetGameObjectsOnTile(leftPos, "Floor");
            var rightFloors = Helper.GetGameObjectsOnTile(rightPos, "Floor");

            foreach (var leftFloor in leftFloors)
            {
                leftFloor.GetComponent<Floor>().UpdateStretchToWalls();
            }
            foreach (var rightFloor in rightFloors)
            {
                rightFloor.GetComponent<Floor>().UpdateStretchToWalls();
            }
        }
        
        private void PrepForConstruction()
        {
            if (Helper.DoesGridContainTag(transform.position, "Nature"))
            {
                ClearNatureFromTile();
                return;
            }
            
            // Once on dirt create the hauling tasks
            CreateConstructionHaulingTasks();
        }
        
        private void ClearNatureFromTile()
        {
            var objectsOnTile = Helper.GetGameObjectsOnTile(transform.position);
            foreach (var tileObj in objectsOnTile)
            {
                var growResource = tileObj.GetComponent<GrowingResource>();
                if (growResource != null)
                {
                    growResource.TaskRequestors.Add(gameObject);

                    if (!growResource.QueuedToCut)
                    {
                        growResource.CreateTaskById("Cut Plant");
                    }
                }
            }
        }

        public void InformDirtReady()
        {
            CreateConstructionHaulingTasks();
        }

        private bool IsOnDirt()
        {
            return Helper.DoesGridContainTag(transform.position, "Dirt");
        }

        private void ClearGrass()
        {
            Spawner.Instance.SpawnDirtTile(Helper.ConvertMousePosToGridPos(transform.position), this);
        }

        public List<ItemAmount> GetRemainingMissingItems()
        {
            _pendingResourceCosts ??= new List<ItemAmount>();
            List<ItemAmount> result = new List<ItemAmount>(_remainingResourceCosts);
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
            
            return result;
        }

        private void CreateConstructionHaulingTasks()
        {
            var resourceCosts = _structureData.GetResourceCosts();
            CreateConstuctionHaulingTasksForItems(resourceCosts);
        }

        public void CreateConstuctionHaulingTasksForItems(List<ItemAmount> remainingResources)
        {
            foreach (var resourceCost in remainingResources)
            {
                for (int i = 0; i < resourceCost.Quantity; i++)
                {
                    EnqueueCreateTakeResourceToBlueprintTask(resourceCost.Item);
                }
            }
        }

        private void EnqueueCreateTakeResourceToBlueprintTask(ItemData resourceData)
        {
            var taskRef = taskMaster.HaulingTaskSystem.EnqueueTask(() =>
            {
                StorageSlot slot = ControllerManager.Instance.InventoryController.ClaimResource(resourceData);
                if (slot != null)
                {
                    AddToPendingResourceCosts(resourceData);
                    return CreateTakeResourceFromSlotToBlueprintTask(slot);
                }
                else
                {
                    return null;
                }
            }).GetHashCode();
            _assignedTaskRefs.Add(taskRef);
        }

        public HaulingTask.TakeResourceToBlueprint CreateTakeResourceFromSlotToBlueprintTask(StorageSlot slot)
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
                useResource = ( heldItem) =>
                {
                    _incomingItems.Remove(heldItem);
                    heldItem.gameObject.SetActive(false);
                    AddResourceToBlueprint(heldItem.GetItemData());
                    Destroy(heldItem.gameObject);
                    CheckIfAllResourcesLoaded();
                },
            };

            PendingTask = TaskType.TakeResourceToBlueprint;
            _assignedTaskRefs.Add(task.GetHashCode());
            return task;
        }
        
        public HaulingTask.TakeResourceToBlueprint CreateTakeResourceToBlueprintTask(Item resourceForBluePrint)
        {
            var task = new HaulingTask.TakeResourceToBlueprint
            {
                TargetUID = UniqueId,
                resourcePosition = resourceForBluePrint.transform.position,
                blueprintPosition = transform.position,
                grabResource = (UnitTaskAI unitTaskAI) =>
                {
                    PendingTask = TaskType.None;
                    resourceForBluePrint.gameObject.SetActive(true);
                    unitTaskAI.AssignHeldItem(resourceForBluePrint);
                    _incomingItems.Add(resourceForBluePrint);
                    
                },
                useResource = (heldItem) =>
                {
                    _incomingItems.Remove(heldItem);
                    heldItem.gameObject.SetActive(false);
                    AddResourceToBlueprint(heldItem.GetItemData());
                    Destroy(heldItem.gameObject);
                    CheckIfAllResourcesLoaded();
                },
            };

            PendingTask = TaskType.TakeItemToItemSlot;
            _assignedTaskRefs.Add(task.GetHashCode());
            return task;
        }

        public ConstructionTask.ConstructStructure CreateConstructStructureTask(bool autoAssign = true)
        {
            // Clear old refs
            _assignedTaskRefs.Clear();
            
            var task = new ConstructionTask.ConstructStructure
            {
                TargetUID = UniqueId,
                structurePosition = transform.position,
                workAmount = _structureData.GetWorkPerResource(),
                completeWork = CompleteConstruction
            };
            
            _assignedTaskRefs.Add(task.GetHashCode());

            if (autoAssign)
            {
                taskMaster.ConstructionTaskSystem.AddTask(task);
            }

            return task;
        }
        
        private void CompleteConstruction()
        {
            ShowBlueprint(false);
            _isBuilt = true;
        }

        public bool IsBuilt()
        {
            return _isBuilt;
        }

        public void CancelConstruction()
        {
            if (!_isBuilt)
            {
                CancelTasks();
                
                // Spawn All the resources used
                SpawnUsedResources(100f);
            
                // Update the neighbours
                var collider = GetComponent<BoxCollider2D>();
                collider.enabled = false;
                ClearTile();
            
                // Delete this blueprint
                Destroy(gameObject);
            }
        }

        private void CancelTasks()
        {
            if (_assignedTaskRefs == null || _assignedTaskRefs.Count == 0) return;

            foreach (var taskRef in _assignedTaskRefs)
            {
                taskMaster.HaulingTaskSystem.CancelTask(taskRef);
                taskMaster.ConstructionTaskSystem.CancelTask(taskRef);
            }
            _assignedTaskRefs.Clear();
            
            // Drop all incoming resources
            foreach (var incomingItem in _incomingItems)
            {
                incomingItem.CancelAssignedTask();
                incomingItem.CreateHaulTask();
            }
            
            _pendingResourceCosts.Clear();
        }

        private void SpawnUsedResources(float percentReturned)
        {
            // Spawn All the resources used
            var totalCosts = _structureData.GetResourceCosts();
            var remainingCosts = _remainingResourceCosts;
            List<ItemAmount> difference = new List<ItemAmount>();
            foreach (var totalCost in totalCosts)
            {
                var remaining = remainingCosts.Find(c => c.Item == totalCost.Item);
                int remainingAmount = 0;
                if (remaining != null)
                {
                    remainingAmount = remaining.Quantity;
                }
                
                int amount = totalCost.Quantity - remainingAmount;
                if (amount > 0)
                {
                    ItemAmount refund = new ItemAmount
                    {
                        Item = totalCost.Item,
                        Quantity = amount
                    };
                    difference.Add(refund);
                }
            }

            foreach (var refundCost in difference)
            {
                for (int i = 0; i < refundCost.Quantity; i++)
                {
                    if (Helper.RollDice(percentReturned))
                    {
                        Spawner.Instance.SpawnItem(refundCost.Item, this.transform.position, true);
                    }
                }
            }
        }

        public ConstructionTask.DeconstructStructure CreateDeconstructionTask(bool autoAssign = true)
        {
            _isDeconstructing = true;
            SetIcon("Hammer");
            var task = new ConstructionTask.DeconstructStructure()
            {
                TargetUID = UniqueId,
                claimStructure = (UnitTaskAI unitTaskAI) =>
                {
                    _incomingUnit = unitTaskAI;
                },
                structurePosition = transform.position,
                workAmount = _structureData.GetWorkPerResource(),
                completeWork = CompleteDeconstruction
            };
            
            _assignedTaskRefs.Add(task.GetHashCode());

            if (autoAssign)
            {
                taskMaster.ConstructionTaskSystem.AddTask(task);
            }

            return task;
        }

        private void CompleteDeconstruction()
        {
            _assignedTaskRefs.Clear();

            _incomingUnit = null;
            // Spawn some of the used resources
            SpawnUsedResources(50f);
            
            // Update the neighbours
            var collider = GetComponent<BoxCollider2D>();
            collider.enabled = false;

            var infoPanel = FindObjectOfType<SelectedItemInfoPanel>();
            if (infoPanel != null)
            {
                infoPanel.HideItemDetails();
            }

            ClearTile();

            // Delete the structure
            Destroy(gameObject);
        }

        public void CancelDeconstruction()
        {
            _isDeconstructing = false;
            CancelTasks();
            SetIcon(null);

            if (_incomingUnit != null)
            {
                _incomingUnit.CancelTask();
            }
        }

        public bool IsDeconstucting()
        {
            return _isDeconstructing;
        }

        private void SetIcon(string iconName)
        {
            if (string.IsNullOrEmpty(iconName))
            {
                _icon.sprite = null;
                _icon.gameObject.SetActive(false);
            }
            else
            {
                _icon.sprite = Librarian.Instance.GetSprite(iconName);
                _icon.gameObject.SetActive(true);
            }
        }

        private void Refresh()
        {
            SetTile();
            ShowBlueprint(!_isBuilt);
        }

        public object CaptureState()
        {
            List<string> incomingItemsGUIDS = new List<string>();
            foreach (var incomingItem in _incomingItems)
            {
                incomingItemsGUIDS.Add(incomingItem.UniqueId);
            }
            
            return new Data
            {
                UID = this.UniqueId,
                Position = transform.position,
                StructureData = _structureData,
                ResourceCost = _remainingResourceCosts,
                IsBuilt = _isBuilt,
                AssignedTaskRefs = _assignedTaskRefs,
                IncomingItemsUIDs = incomingItemsGUIDS,
                IsDeconstructing = _isDeconstructing,
                IncomingUnit = _incomingUnit,
                StructureTilemap = _structureTilemap,
                PendingTask = PendingTask,
            };
        }

        public void RestoreState(object data)
        {
            var state = (Data)data;

            UniqueId = state.UID;
            transform.position = state.Position;
            _structureData = state.StructureData;
            _remainingResourceCosts = state.ResourceCost;
            _isBuilt = state.IsBuilt;
            _assignedTaskRefs = state.AssignedTaskRefs;
            _isDeconstructing = state.IsDeconstructing;
            _incomingUnit = state.IncomingUnit;
            _structureTilemap = state.StructureTilemap;
            PendingTask = state.PendingTask;

            // var incomingItemsGUIDS = state.IncomingItemsUIDs;
            // var itemsHandler = ControllerManager.Instance.ItemsHandler;
            _incomingItems.Clear();
            // foreach (var incomingItemGUID in incomingItemsGUIDS)
            // {
            //     var item = itemsHandler.GetItemByUID(incomingItemGUID);
            //     if (item != null)
            //     {
            //         _incomingItems.Add(item);
            //     }
            // }

            Refresh();

            var missingItems = GetRemainingMissingItems();
            CreateConstuctionHaulingTasksForItems(missingItems);
            
            CheckIfAllResourcesLoaded();
        }

        public struct Data
        {
            public string UID;
            public Vector3 Position;
            public StructureData StructureData;
            public List<ItemAmount> ResourceCost;
            public bool IsBuilt;
            public List<int> AssignedTaskRefs;
            public List<string> IncomingItemsUIDs;
            public bool IsDeconstructing;
            public UnitTaskAI IncomingUnit; // TODO: will likely need to use GUID
            public Tilemap StructureTilemap;
            public TaskType PendingTask;
        }
    }
}
