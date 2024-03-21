using System.Collections.Generic;
using System.Linq;
using Data.Item;
using Data.Structure;
using Databrain;
using Databrain.Attributes;
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
    public class Construction : PlayerInteractable, IClickableObject
    {
        protected Action _onDeconstructed;
        
        [SerializeField] private List<Transform> _workPoints;
        
        public string DisplayName => RuntimeData.name;
        
        public DataLibrary DataLibrary;
        
        [DataObjectDropdown("DataLibrary", true)]
        public ConstructionData Data;
        public ConstructionData RuntimeData;
        
        public PlayerInteractable GetPlayerInteractable()
        {
            return this;
        }

        protected virtual void Awake()
        {
            
        }
        
        public void AssignCommand(Command command, object payload = null)
        {
            CreateTask(command, payload);
        }
        
        public virtual bool DoConstruction(float workAmount)
        {
            RuntimeData.RemainingWork -= workAmount;
            Changed();
            if (RuntimeData.RemainingWork <= 0)
            {
                CompleteConstruction();
                return true;
            }
            
            return false;
        }

        public virtual bool DoDeconstruction(float workAmount)
        {
            RuntimeData.RemainingWork -= workAmount;
            Changed();
            if (RuntimeData.RemainingWork <= 0)
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
            if (RuntimeData.State != EConstructionState.Built)
            {
                CancelRequestorTasks();
                
                // Spawn All the resources used
                SpawnUsedResources(100f);

                // Delete this blueprint
                Destroy(gameObject);
            }
        }
        
        public override void ReceiveItem(ItemData itemData)
        {
            RuntimeData.RemoveFromIncomingItems(itemData);
            
            Destroy(itemData.LinkedItem.gameObject);
            
            RuntimeData.RemoveFromPendingResourceCosts(itemData.Settings);
            RuntimeData.DeductFromMaterialCosts(itemData.Settings);
            
            if (RuntimeData.RemainingMaterialCosts.Count == 0)
            {
                CreateConstructTask();
            }
            
            Changed();
        }
        
        public override Vector2? UseagePosition(Vector2 requestorPosition)
        {
            List<(Transform, float)> distances = new List<(Transform, float)>();

            if (_workPoints.Count == 0)
            {
                var pathResult = Helper.DoesPathExist(requestorPosition, transform.position);
                if (pathResult.pathExists)
                {
                    return transform.position;
                }
            }
            
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
        
        public virtual void CompleteConstruction()
        {
            RuntimeData.RemainingWork = RuntimeData.CraftRequirements.WorkCost;
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
        
        public void CheckIfAllResourcesLoaded()
        {
            if (RuntimeData.State == EConstructionState.Built) return;
            
            if (RuntimeData.RemainingMaterialCosts.Count == 0)
            {
                CreateConstructTask();
            }
        }
        
        public virtual void CreateConstructTask(bool autoAssign = true)
        {
            Task constuctTask = new Task("Build Construction", ETaskType.Construction, this, EToolType.BuildersHammer);
            constuctTask.Enqueue();
        }

        public virtual void CreateDeconstructionTask(bool autoAssign = true, Action onDeconstructed = null)
        {
            _onDeconstructed = onDeconstructed;
            Task constuctTask = new Task("Deconstruct", ETaskType.Construction, this, EToolType.BuildersHammer);
            constuctTask.Enqueue();
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

        protected virtual void EnqueueCreateTakeResourceToBlueprintTask(ItemDataSettings resourceSettings)
        {
            Task task = new Task("Withdraw Item Construction", ETaskType.Hauling, this, EToolType.None)
            {
                Payload = resourceSettings,
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
        
        public virtual void SpawnUsedResources(float percentReturned)
        {
            // Spawn All the resources used
            var totalCosts = RuntimeData.CraftRequirements.GetMaterialCosts();
            var remainingCosts = RuntimeData.RemainingMaterialCosts;
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
        
        protected virtual void CancelTasks()
        {
            // Drop all incoming resources
            foreach (var incomingItem in RuntimeData.IncomingItems)
            {
                incomingItem.LinkedItem.SeekForSlot();
            }
            RuntimeData.PendingResourceCosts.Clear();
            RuntimeData.IncomingItems.Clear();
        }
    }
}
