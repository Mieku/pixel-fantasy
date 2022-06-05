using System;
using System.Collections.Generic;
using Controllers;
using DataPersistence;
using Gods;
using HUD;
using Interfaces;
using Pathfinding;
using ScriptableObjects;
using Sirenix.OdinInspector;
using Tasks;
using Unit;
using UnityEngine;

namespace Items
{
    public class Floor : MonoBehaviour, IClickableObject, IPersistent
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private GraphUpdateScene _pathGraphUpdater;
        [SerializeField] private SpriteRenderer _icon;

        private FloorData _floorData;
        private List<ItemAmount> _resourceCost;
        private bool _isBuilt;
        private List<int> _assignedTaskRefs = new List<int>();
        private List<Item> _incomingItems = new List<Item>();
        private bool _isDeconstructing;
        private UnitTaskAI _incomingUnit;
        private Vector2 _floorPos;

        public string GUID;
        
        private TaskMaster taskMaster => TaskMaster.Instance;

        public FloorData FloorData => _floorData;
        public Vector2 FloorPos => _floorPos;
        
        public bool IsClickDisabled { get; set; }

        [Button("Assign GUID")]
        private void SetGUID()
        {
            if (GUID == "")
            {
                GUID = Guid.NewGuid().ToString();
            }
        }

        private void Start()
        {
            SetGUID();
        }

        public ClickObject GetClickObject()
        {
            // TODO: Add Click Object to floors
            return null;
        }

        public void Init(FloorData floorData)
        {
            IsClickDisabled = false;
            _floorData = floorData;
            _floorPos = transform.position;
            _resourceCost = new List<ItemAmount> (_floorData.GetResourceCosts());
            UpdateSprite();
            IsAllowed = true;
            UpdateStretchToWalls();
            ShowBlueprint(true);
            PrepForConstruction();
        }

        private void UpdateSprite()
        {
            _spriteRenderer.sprite = _floorData.FloorSprite;
        }

        public void UpdateStretchToWalls()
        {
            if (!_floorData.StretchToWall) return;
            
            // Reset values
            transform.position = _floorPos;
            _spriteRenderer.size = Vector2.one;
            
            // Check if a wall is next to the tile or below
            var leftPos = new Vector2(_floorPos.x - 1, _floorPos.y);
            var rightPos = new Vector2(_floorPos.x + 1, _floorPos.y);

            bool wallLeft = Helper.DoesGridContainTag(leftPos, "Wall");
            bool wallRight = Helper.DoesGridContainTag(rightPos, "Wall");

            // Stretch the art to meet wall
            if (wallLeft)
            {
                transform.position = new Vector3(transform.position.x - 0.25f, transform.position.y, 0);
                _spriteRenderer.size = new Vector2(_spriteRenderer.size.x + 0.5f, _spriteRenderer.size.y);
            }
            if (wallRight)
            {
                transform.position = new Vector3(transform.position.x + 0.25f, transform.position.y, 0);
                _spriteRenderer.size = new Vector2(_spriteRenderer.size.x + 0.5f, _spriteRenderer.size.y);
            }
        }
        
        private void ShowBlueprint(bool showBlueprint)
        {
            if (showBlueprint)
            {
                _spriteRenderer.color = Librarian.Instance.GetColour("Blueprint");
                _pathGraphUpdater.enabled = false;
            }
            else
            {
                _spriteRenderer.color = Color.white;
                _pathGraphUpdater.enabled = true;
            }
        }
        
        private void PrepForConstruction()
        {
            // Check if the structure is on dirt, if not make task to clear the grass
            // if (!IsOnDirt() || (_dirtTile != null && !_dirtTile.IsBuilt))
            // {
            //     ClearGrass();
            //     return;
            // }
            
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
                        growResource.CreateCutPlantTask();
                    }
                }
            }
        }
        
        public void InformDirtReady()
        {
            CreateConstructionHaulingTasks();
        }
        
        private void CreateConstructionHaulingTasks()
        {
            var resourceCosts = _resourceCost;
            foreach (var resourceCost in resourceCosts)
            {
                for (int i = 0; i < resourceCost.Quantity; i++)
                {
                    CreateTakeResourceToBlueprintTask(resourceCost.Item);
                }
            }
        }

        private void CreateTakeResourceToBlueprintTask(ItemData resourceData)
        {
            var taskRef = taskMaster.HaulingTaskSystem.EnqueueTask(() =>
            {
                Item resource = ControllerManager.Instance.InventoryController.ClaimResource(resourceData);
                if (resource != null)
                {
                    var task = new HaulingTask.TakeResourceToBlueprint
                    {
                        resourcePosition = resource.transform.position,
                        blueprintPosition = transform.position,
                        grabResource = (UnitTaskAI unitTaskAI) =>
                        {
                            resource.transform.SetParent(unitTaskAI.transform);
                            resource.gameObject.SetActive(true);
                            ControllerManager.Instance.InventoryController.DeductClaimedResource(resource);
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
                CreateConstructFloorTask();
            }
        }
        
        private void CreateConstructFloorTask()
        {
            // Clear old refs
            _assignedTaskRefs.Clear();
            
            var task = new ConstructionTask.ConstructStructure
            {
                structurePosition = transform.position,
                workAmount = _floorData.GetWorkPerResource(),
                completeWork = CompleteConstruction
            };
            
            _assignedTaskRefs.Add(task.GetHashCode());
            
            taskMaster.ConstructionTaskSystem.AddTask(task);
        }
        
        private void CompleteConstruction()
        {
            ShowBlueprint(false);
            _isBuilt = true;
            IsClickDisabled = true;
        }
        
        public void CancelConstruction()
        {
            if (!_isBuilt)
            {
                CancelTasks();
                
                // Spawn All the resources used
                SpawnUsedResources(100f);

                // Delete this blueprint
                Destroy(gameObject);
            }
        }

        private void CancelTasks()
        {
            if (_assignedTaskRefs == null || _assignedTaskRefs.Count == 0) return;

            foreach (var taskRef in _assignedTaskRefs)
            {
                taskMaster.FarmingTaskSystem.CancelTask(taskRef);
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
            
            _incomingItems.Clear();
        }

        private void SpawnUsedResources(float percentReturned)
        {
            // Spawn All the resources used
            var totalCosts = _floorData.GetResourceCosts();
            var remainingCosts = _resourceCost;
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

        public void CreateDeconstructionTask()
        {
            _isDeconstructing = true;
            SetIcon("Hammer");
            var task = new ConstructionTask.DeconstructStructure()
            {
                claimStructure = (UnitTaskAI unitTaskAI) =>
                {
                    _incomingUnit = unitTaskAI;
                },
                structurePosition = transform.position,
                workAmount = _floorData.GetWorkPerResource(),
                completeWork = CompleteDeconstruction
            };
            
            _assignedTaskRefs.Add(task.GetHashCode());
            
            taskMaster.ConstructionTaskSystem.AddTask(task);
        }

        private void CompleteDeconstruction()
        {
            _assignedTaskRefs.Clear();

            _incomingUnit = null;
            // Spawn some of the used resources
            SpawnUsedResources(50f);

            var infoPanel = FindObjectOfType<SelectedItemInfoPanel>();
            if (infoPanel != null)
            {
                infoPanel.HideItemDetails();
            }

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
        
        public void ToggleAllowed(bool isAllowed)
        {
            IsAllowed = isAllowed;
            if (IsAllowed)
            {
                _icon.gameObject.SetActive(false);
                _icon.sprite = null;
                PrepForConstruction();
            }
            else
            {
                _icon.gameObject.SetActive(true);
                _icon.sprite = Librarian.Instance.GetSprite("Lock");
                CancelTasks();
            }
        }

        public bool IsAllowed { get; set; }
        
        public List<Order> GetOrders()
        {
            List<Order> results = new List<Order>();
            
            results.Add(Order.Disallow);

            if (_isBuilt)
            {
                results.Add(Order.Deconstruct);
            }
            else
            {
                results.Add(Order.Cancel);
            }

            return results;
        }

        public bool IsOrderActive(Order order)
        {
            switch (order)
            {
                case Order.Disallow:
                    return false;
                case Order.Deconstruct:
                    return IsDeconstucting();
                case Order.Cancel:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException(nameof(order), order, null);
            }
        }

        public void AssignOrder(Order orderToAssign)
        {
            switch (orderToAssign)
            {
                case Order.Deconstruct:
                    CreateDeconstructionTask();
                    break;
                case Order.Cancel:
                    if (_isDeconstructing)
                    {
                        CancelDeconstruction();
                    } else if (!_isBuilt)
                    {
                        CancelConstruction();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(orderToAssign), orderToAssign, null);
            }
        }

        public object CaptureState()
        {
            List<string> incomingItemsGUIDS = new List<string>();
            foreach (var incomingItem in _incomingItems)
            {
                incomingItemsGUIDS.Add(incomingItem.GUID);
            }
            
            return new Data
            {
                GUID = this.GUID,
                FloorData = _floorData,
                ResourceCost = _resourceCost,
                IsBuilt = _isBuilt,
                AssignedTaskRefs = _assignedTaskRefs,
                IncomingItemsGUIDs = incomingItemsGUIDS,
                IsDeconstructing = _isDeconstructing,
                IncomingUnit = _incomingUnit,
                FloorPos = _floorPos,
                IsClickDisabled = IsClickDisabled,
                IsAllowed = IsAllowed,
            };
        }

        public void RestoreState(object data)
        {
            var state = (Data)data;

            this.GUID = state.GUID;
            _floorData = state.FloorData;
            _resourceCost = state.ResourceCost;
            _isBuilt = state.IsBuilt;
            _assignedTaskRefs = state.AssignedTaskRefs;
            _isDeconstructing = state.IsDeconstructing;
            _incomingUnit = state.IncomingUnit;
            _floorPos = state.FloorPos;
            transform.position = _floorPos;
            IsClickDisabled = state.IsClickDisabled;
            IsAllowed = state.IsAllowed;
            
            var incomingItemsGUIDS = state.IncomingItemsGUIDs;
            var itemsHandler = ControllerManager.Instance.ItemsHandler;
            _incomingItems.Clear();
            foreach (var incomingItemGUID in incomingItemsGUIDS)
            {
                var item = itemsHandler.GetItemByGUID(incomingItemGUID);
                if (item != null)
                {
                    _incomingItems.Add(item);
                }
            }
            
            ShowBlueprint(!_isBuilt);
            UpdateSprite();
            UpdateStretchToWalls();
        }

        public struct Data
        {
            public string GUID;
            public FloorData FloorData;
            public List<ItemAmount> ResourceCost;
            public bool IsBuilt;
            public List<int> AssignedTaskRefs;
            public List<string> IncomingItemsGUIDs;
            public bool IsDeconstructing;
            public UnitTaskAI IncomingUnit; // TODO: will likely need to use GUID
            public Vector2 FloorPos;
            public bool IsClickDisabled;
            public bool IsAllowed;
        }
    }
}
