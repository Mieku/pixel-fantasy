using System;
using System.Collections.Generic;
using Controllers;
using Gods;
using HUD;
using Pathfinding;
using ScriptableObjects;
using Tasks;
using Unit;
using UnityEngine;

namespace Items
{
    public class Structure : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
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
        private List<GameObject> _neighbours;
        
        private TaskMaster taskMaster => TaskMaster.Instance;

        public void Init(StructureData structureData)
        {
            _structureData = structureData;
            _resourceCost = new List<ItemAmount> (_structureData.GetResourceCosts());
            UpdateSprite(true);
            _progressBar.ShowBar(false);
            ShowBlueprint(true);
            CreateConstructionHaulingTasks();
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
                _spriteRenderer.color = Librarian.Instance.GetColour("Blueprint");
                _gridObstacle.enabled = false;
            }
            else
            {
                _spriteRenderer.color = Color.white;
                _gridObstacle.enabled = true;
                gameObject.layer = 4;
            }
        }

        public void UpdateSprite(bool informNeighbours)
        {
            // collect data on connections
            var neighbourData = _structureData.GetNeighbourData(transform.position);
            _neighbours = neighbourData.Neighbours;
            var connectData = neighbourData.WallNeighbourConnectionInfo;

            // use connection data to update sprite
            _spriteRenderer.sprite = _structureData.GetSprite(connectData);

            // If inform neighbours, tell neighbours to UpdateSprite (but they shouldn't inform their neighbours
            if (informNeighbours)
            {
                RefreshNeighbours();
            }
        }

        private void RefreshNeighbours()
        {
            foreach (var neighbour in _neighbours)
            {
                var structure = neighbour.GetComponent<Structure>();
                if (structure != null)
                {
                    structure.UpdateSprite(false);
                }
            }
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
                RefreshNeighbours();
            
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
            RefreshNeighbours();

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
    }
}
