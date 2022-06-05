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
using Unit;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Items
{
    public class Structure : MonoBehaviour, IPersistent
    {
        [SerializeField] private ProgressBar _progressBar;
        [SerializeField] private DynamicGridObstacle _gridObstacle;
        [SerializeField] private SpriteRenderer _icon;

        private StructureData _structureData;
        private List<ItemAmount> _resourceCost;
        private bool _isBuilt;
        private List<int> _assignedTaskRefs = new List<int>();
        private List<Item> _incomingItems = new List<Item>();
        private bool _isDeconstructing;
        private UnitTaskAI _incomingUnit;
        private Tilemap _structureTilemap;
        
        public string GUID;
        
        private TaskMaster taskMaster => TaskMaster.Instance;

        [Button("Assign GUID")]
        private void SetGUID()
        {
            if (GUID == "")
            {
                GUID = Guid.NewGuid().ToString();
            }
        }
        
        private void Awake()
        {
            _structureTilemap = TilemapController.Instance.GetTilemap(TilemapLayer.Structure);
            SetGUID();
        }

        public void Init(StructureData structureData)
        {
            _structureData = structureData;
            _resourceCost = new List<ItemAmount> (_structureData.GetResourceCosts());
            SetTile();
            _progressBar.ShowBar(false);
            ShowBlueprint(true);
            PrepForConstruction();
        }

        public StructureData GetStructureData()
        {
            return _structureData;
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
            // Check if the structure is on dirt, if not make task to clear the grass
            // if (!IsOnDirt())
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

        private bool IsOnDirt()
        {
            return Helper.DoesGridContainTag(transform.position, "Dirt");
        }

        private void ClearGrass()
        {
            Spawner.Instance.SpawnDirtTile(Helper.ConvertMousePosToGridPos(transform.position), this);
        }

        private void CreateConstructionHaulingTasks()
        {
            var resourceCosts = _structureData.GetResourceCosts();
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
        
        private void CreateConstructStructureTask()
        {
            // Clear old refs
            _assignedTaskRefs.Clear();
            
            var task = new ConstructionTask.ConstructStructure
            {
                structurePosition = transform.position,
                workAmount = _structureData.GetWorkPerResource(),
                completeWork = CompleteConstruction
            };
            
            _assignedTaskRefs.Add(task.GetHashCode());
            
            taskMaster.ConstructionTaskSystem.AddTask(task);
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
        }

        private void SpawnUsedResources(float percentReturned)
        {
            // Spawn All the resources used
            var totalCosts = _structureData.GetResourceCosts();
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
                workAmount = _structureData.GetWorkPerResource(),
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
                incomingItemsGUIDS.Add(incomingItem.GUID);
            }
            
            return new Data
            {
                GUID = this.GUID,
                Position = transform.position,
                StructureData = _structureData,
                ResourceCost = _resourceCost,
                IsBuilt = _isBuilt,
                AssignedTaskRefs = _assignedTaskRefs,
                IncomingItemsGUIDs = incomingItemsGUIDS,
                IsDeconstructing = _isDeconstructing,
                IncomingUnit = _incomingUnit,
                StructureTilemap = _structureTilemap,
            };
        }

        public void RestoreState(object data)
        {
            var state = (Data)data;

            GUID = state.GUID;
            transform.position = state.Position;
            _structureData = state.StructureData;
            _resourceCost = state.ResourceCost;
            _isBuilt = state.IsBuilt;
            _assignedTaskRefs = state.AssignedTaskRefs;
            _isDeconstructing = state.IsDeconstructing;
            _incomingUnit = state.IncomingUnit;
            _structureTilemap = state.StructureTilemap;

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

            Refresh();
        }

        public struct Data
        {
            public string GUID;
            public Vector3 Position;
            public StructureData StructureData;
            public List<ItemAmount> ResourceCost;
            public bool IsBuilt;
            public List<int> AssignedTaskRefs;
            public List<string> IncomingItemsGUIDs;
            public bool IsDeconstructing;
            public UnitTaskAI IncomingUnit; // TODO: will likely need to use GUID
            public Tilemap StructureTilemap;
        }
    }
}
