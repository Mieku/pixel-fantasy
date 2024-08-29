using System.Collections.Generic;
using System.Linq;
using AI;
using Characters;
using Handlers;
using HUD;
using Interfaces;
using Systems.Stats.Scripts;
using UnityEngine;
using Action = System.Action;

namespace Items
{
    public abstract class Construction : PlayerInteractable, IConstructable
    {
        protected Action _onDeconstructed;
        
        [SerializeField] private List<Transform> _workPoints;
        
        public ConstructionData RuntimeData;
        public override string UniqueID => RuntimeData.UniqueID;
        public override string PendingTaskUID
        {
            get => RuntimeData.PendingTaskUID;
            set => RuntimeData.PendingTaskUID = value;
        }

        protected virtual void Awake()
        {
            
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        public abstract void LoadData(ConstructionData data);
        
        public override void AssignCommand(Command command)
        {
            if (command.name == "Deconstruct Construction Command")
            {
                if (RuntimeData.State != EConstructionState.Built)
                {
                    CancelConstruction();
                }
                else
                {
                    CreateTask(command);
                }
            } 
            else if (command.name == "Copy Command")
            {
                DoCopy();
            }
            else
            {
                CreateTask(command);
            }
        }

        public virtual void DoCopy()
        {
            
        }
        
        public virtual bool DoConstruction(StatsData stats, out float progress)
        {
            var workAmount = stats.GetActionSpeedForSkill(ESkillType.Construction, true);
            RuntimeData.RemainingWork -= workAmount;
            InformChanged();
            
            if (RuntimeData.RemainingWork <= 0)
            {
                CompleteConstruction();
                progress = 1;
                return true;
            }
            else
            {
                progress = RuntimeData.ConstructionPercent;
                return false;
            }
        }

        public virtual bool DoDeconstruction(StatsData stats, out float progress)
        {
            var workAmount = stats.GetActionSpeedForSkill(ESkillType.Construction, true);
            RuntimeData.RemainingWork -= workAmount;
            InformChanged();
            if (RuntimeData.RemainingWork <= 0)
            {
                CompleteDeconstruction();
                progress = 1;
                return true;
            }
            else
            {
                progress = 1 - (RuntimeData.RemainingWork / RuntimeData.Settings.CraftRequirements.WorkCost);
                return false;
            }
        }

        public virtual void CancelConstruction()
        {
            if (RuntimeData.State != EConstructionState.Built)
            {
                CancelPendingTask();
                
                // Spawn All the resources used
                RefundUsedResources();

                // Delete this blueprint
                Destroy(gameObject);
            }
        }
        
        public override void ReceiveItem(ItemData itemData)
        {
            var item = (Item) itemData.GetLinkedItem();
            Destroy(item.gameObject);
            
            RuntimeData.RemoveFromIncomingItems(itemData);
            RuntimeData.AddToReceivedItems(itemData);
            
            itemData.CarryingKinlingUID = null;
            
            RuntimeData.RemoveFromPendingResourceCosts(itemData.Settings);
            RuntimeData.DeductFromMaterialCosts(itemData.Settings);
            
            if (RuntimeData.RemainingMaterialCosts.Count == 0)
            {
                CreateConstructTask();
            }
            
            InformChanged();
        }
        
        public void AddToIncomingItems(ItemData itemData)
        {
            RuntimeData.AddToIncomingItems(itemData);
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
            RuntimeData.RemainingWork = RuntimeData.Settings.CraftRequirements.WorkCost;

            foreach (var itemUID in RuntimeData.ReceivedItemUIDs)
            {
                var itemData = ItemsDatabase.Instance.Query(itemUID);
                itemData.DeleteItemData();
            }
            RuntimeData.ReceivedItemUIDs.Clear();
        }

        public virtual void CompleteDeconstruction()
        {
            // Spawn some of the used resources
            SpawnUsedResources(50f);
            
            // Update the neighbours
            var col = GetComponent<BoxCollider2D>();
            col.enabled = false;

            var infoPanel = FindObjectOfType<SelectedItemInfoPanel>();
            if (infoPanel != null)
            {
                infoPanel.HideAllDetails();
            }

            if (_onDeconstructed != null)
            {
                _onDeconstructed.Invoke();
            }
            
            InformChanged();
            
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
        
        public virtual void CreateConstructTask()
        {
            Task task = new Task("Build Structure", $"Building {RuntimeData.Settings.ConstructionName}" ,ETaskType.Construction, this);
            TasksDatabase.Instance.AddTask(task);
        }

        public virtual void CreateDeconstructionTask(Action onDeconstructed = null)
        {
            if (RuntimeData.State == EConstructionState.Built)
            {
                _onDeconstructed = onDeconstructed;

                Task task = new Task("Deconstruct Structure", $"Deconstructing {RuntimeData.Settings.ConstructionName}" ,ETaskType.Construction, this);
                TasksDatabase.Instance.AddTask(task);
            }
            else
            {
                CancelConstruction();
                onDeconstructed?.Invoke();
            }
        }
        
        public void CreateConstuctionHaulingTasksForItems(List<CostData> remainingResources)
        {
            foreach (var resourceCost in remainingResources)
            {
                for (int i = 0; i < resourceCost.Quantity; i++)
                {
                    EnqueueCreateTakeResourceToBlueprintTask(resourceCost.Item);
                }
            }
        }

        protected virtual void EnqueueCreateTakeResourceToBlueprintTask(ItemSettings resourceSettings)
        {
            Dictionary<string, object> taskData = new Dictionary<string, object> { { "ItemSettingsID", resourceSettings.name } };

            Task task = new Task("Withdraw Item For Constructable", "Gathering Materials" ,ETaskType.Hauling, this, taskData);
            TasksDatabase.Instance.AddTask(task);
        }

        public bool IsClickDisabled { get; set; }
        public bool IsAllowed { get; set; }
        public void ToggleAllowed(bool isAllowed)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// For when the construction is not fully built
        /// </summary>
        protected void RefundUsedResources()
        {
            foreach (var itemUID in RuntimeData.ReceivedItemUIDs)
            {
                var itemData = ItemsDatabase.Instance.Query(itemUID);
                itemData.State = EItemState.Loose;
                ItemsDatabase.Instance.CreateItemObject(itemData, itemData.Position);
            }
            RuntimeData.ReceivedItemUIDs.Clear();
        }
        
        public virtual void SpawnUsedResources(float percentReturned)
        {
            // Spawn All the resources used
            var totalCosts = RuntimeData.Settings.CraftRequirements.GetMaterialCosts();
            var remainingCosts = RuntimeData.RemainingMaterialCosts;
            List<CostSettings> difference = new List<CostSettings>();
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
                    CostSettings refund = new CostSettings
                    {
                        Item = totalCost.Item,
                        Quantity = amount
                    };
                    difference.Add(refund);
                }
            }

            var spawnPos = Helper.SnapToGridPos(transform.position);
            foreach (var refundCost in difference)
            {
                for (int i = 0; i < refundCost.Quantity; i++)
                {
                    if (Helper.RollDice(percentReturned))
                    {
                        var data = refundCost.Item.CreateItemData(spawnPos);
                        ItemsDatabase.Instance.CreateItemObject(data, spawnPos);
                    }
                }
            }
        }
    }
}
