using System.Collections.Generic;
using System.Linq;
using DataPersistence;
using HUD;
using Interfaces;
using Managers;
using ScriptableObjects;
using Systems.Skills.Scripts;
using TaskSystem;
using UnityEngine;
using Action = System.Action;

namespace Items
{
    public class Construction : PlayerInteractable, IPersistent, IClickableObject
    {
        protected List<ItemAmount> _remainingResourceCosts = new List<ItemAmount>();
        protected List<ItemAmount> _pendingResourceCosts = new List<ItemAmount>(); // Claimed by a task but not used yet
        protected List<ItemAmount> _incomingResourceCosts = new List<ItemAmount>(); // The item is on its way
        protected List<Item> _incomingItems = new List<Item>();
    
        protected bool _isBuilt;
        protected bool _isDeconstructing;
        protected bool _hasUnitIncoming;
        protected Action _onDeconstructed;
        protected float _remainingWork;
        
        [SerializeField] private List<Transform> _workPoints;

        public float RemainingWork => _remainingWork;
        public string DisplayName => GetConstructionData().ConstructionName;
        
        public PlayerInteractable GetPlayerInteractable()
        {
            return this;
        }
        
        public virtual ConstructionData GetConstructionData()
        {
            return null;
        }
        
        public bool IsBuilt
        {
            get => _isBuilt;
            set => _isBuilt = value;
        }
        
        public bool IsDeconstructing
        {
            get => _isDeconstructing;
            set => _isDeconstructing = value;
        }

        protected virtual void Awake()
        {
            _remainingWork = GetWorkAmount();
        }
        
        public void AssignCommand(Command command, object payload = null)
        {
            CreateTask(command, payload);
        }
        
        public virtual bool DoConstruction(float workAmount)
        {
            _remainingWork -= workAmount;
            Changed();
            if (_remainingWork <= 0)
            {
                CompleteConstruction();
                return true;
            }
            
            return false;
        }

        public virtual bool DoDeconstruction(float workAmount)
        {
            _remainingWork -= workAmount;
            Changed();
            if (_remainingWork <= 0)
            {
                CompleteDeconstruction();
                return true;
            }
            
            return false;
        }

        // When values change this should be called, is a hook for callbacks
        protected virtual void Changed()
        {
            
        }

        public virtual void CancelConstruction()
        {
            if (!_isBuilt)
            {
                CancelRequestorTasks();
                
                // Spawn All the resources used
                SpawnUsedResources(100f);

                // Delete this blueprint
                Destroy(gameObject);
            }
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

        public override void ReceiveItem(Item item)
        {
            RemoveFromIncomingItems(item);
            
            var itemData = item.GetItemData();
            Destroy(item.gameObject);
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

                    break;
                }
            }
            
            if (_remainingResourceCosts.Count == 0)
            {
                CreateConstructTask();
            }
            
            Changed();
        }
        
        public override Vector2? UseagePosition(Vector2 requestorPosition)
        {
            List<(Transform, float)> distances = new List<(Transform, float)>();
            foreach (var workPoint in _workPoints)
            {
                var pathResult = Helper.DoesPathExist(requestorPosition, workPoint.position);
                if (pathResult.pathExists)
                {
                    float distance = Helper.GetPathLength(pathResult.navMeshPath);
                    distances.Add((workPoint, distance));
                }
            }
            
            if (distances.Count == 0)
            {
                return null;
            }
            
            // Compile the positions that pass the above tests and sort them by distance
            var sortedDistances = distances.OrderBy(x => x.Item2).Select(x => x.Item1).ToList();
            var selectedDistance = sortedDistances[0];
            
            Vector2 result = selectedDistance.position;
            return result;
        }

        public virtual float GetWorkPerResource()
        {
            return 0;
        }
        
        public virtual void CompleteConstruction()
        {
            _remainingWork = 0;
        }

        public virtual void CompleteDeconstruction()
        {
            // Spawn some of the used resources
            SpawnUsedResources(50f);
            
            // Update the neighbours
            var collider = GetComponent<BoxCollider2D>();
            collider.enabled = false;

            var infoPanel = FindObjectOfType<SelectedItemInfoPanel>();
            if (infoPanel != null)
            {
                infoPanel.HideAllDetails();
            }

            if (_onDeconstructed != null)
            {
                _onDeconstructed.Invoke();
            }
            
            Changed();
            
            // Delete the structure
            Destroy(gameObject);
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
        
        private void RemoveFromIncomingItems(Item item)
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

        protected List<ItemAmount> GetClaimedResourcesCosts()
        {
            _pendingResourceCosts ??= new List<ItemAmount>();
            _incomingResourceCosts ??= new List<ItemAmount>();
            
            var results = new List<ItemAmount>();
            foreach (var pendingResourceCost in _pendingResourceCosts)
            {
                results.Add(pendingResourceCost);
            }

            foreach (var incomingResourceCost in _incomingResourceCosts)
            {
                foreach (var result in results)
                {
                    if (result.Item == incomingResourceCost.Item)
                    {
                        result.Quantity -= incomingResourceCost.Quantity;
                        break;
                    }
                }
            }

            return results;
        }
        
        public void CheckIfAllResourcesLoaded()
        {
            if (_isBuilt) return;
            
            if (_remainingResourceCosts.Count == 0)
            {
                CreateConstructTask();
            }
        }
        
        public virtual void CreateConstructTask(bool autoAssign = true)
        {
            Task constuctTask = new Task("Build Construction", this, GetConstructionData().RequiredConstructorJob, EToolType.BuildersHammer, SkillType.Construction);
            constuctTask.Enqueue();
        }

        public virtual void CreateDeconstructionTask(bool autoAssign = true, Action onDeconstructed = null)
        {
            _onDeconstructed = onDeconstructed;
            Task constuctTask = new Task("Deconstruct", this, GetConstructionData().RequiredConstructorJob, EToolType.BuildersHammer, SkillType.Construction);
            constuctTask.Enqueue();
        }
    
        public virtual object CaptureState()
        {
            throw new System.NotImplementedException();
        }

        public virtual void RestoreState(object data)
        {
            if (!_isBuilt)
            {
                var missingItems = GetRemainingMissingItems();
                CreateConstuctionHaulingTasksForItems(missingItems);
            
                CheckIfAllResourcesLoaded();
            }

            if (_isDeconstructing && !_hasUnitIncoming)
            {
                CreateDeconstructionTask();
            }
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

        protected virtual void EnqueueCreateTakeResourceToBlueprintTask(ItemData resourceData)
        {
            Task task = new Task("Withdraw Item Construction", this, null, EToolType.None, SkillType.None)
            {
                Payload = resourceData.ItemName,
            };
            TaskManager.Instance.AddTask(task);
        }

        public ClickObject GetClickObject()
        {
            return GetComponent<ClickObject>();
        }

        public bool IsClickDisabled { get; set; }
        public bool IsAllowed { get; set; }
        public void ToggleAllowed(bool isAllowed)
        {
            throw new System.NotImplementedException();
        }
        
        public virtual List<Command> GetCommands()
        {
            return Commands;
        }
        
        public virtual List<ItemAmount> GetResourceCosts()
        {
            throw new System.NotImplementedException();
        }
        
        public virtual void SpawnUsedResources(float percentReturned)
        {
            // Spawn All the resources used
            var totalCosts = GetResourceCosts();
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
    }
}
