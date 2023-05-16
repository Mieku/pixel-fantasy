using System.Collections.Generic;
using CodeMonkey.Utils;
using Items;
using Managers;
using ScriptableObjects;
using TaskSystem;
using UnityEngine;

namespace Buildings
{
    public class BuildingNode : Interactable
    {
        protected List<ItemAmount> _remainingResourceCosts = new List<ItemAmount>();
        protected List<ItemAmount> _pendingResourceCosts = new List<ItemAmount>();
    
        [SerializeField] private Transform _exteriorRoot;

        public Interior Interior => _buildingData.Interior;

        private Building _building;
        private bool _isPlanning;
        private BuildingData _buildingData;
        protected float _remainingWork;

        public List<string> InvalidPlacementTags => _buildingData.InvalidPlacementTags;
        public BuildingData BuildingData => _buildingData;
        public Building Building => _building;
    
        public void Plan(BuildingData buildingData)
        {
            _buildingData = buildingData;
            _remainingWork = _buildingData.WorkCost;
            // Follows cursor
            _isPlanning = true;
            _building = Instantiate(_buildingData.Exterior, _exteriorRoot).GetComponent<Building>();
            _building.Init(this);
        }

        public void PrepareToBuild()
        {
            // Stop Following cursor, set build task
            _isPlanning = false;
            _building.SetBlueprint();
            _building.IsPlaced = true;
            _remainingResourceCosts = new List<ItemAmount> (_buildingData.GetResourceCosts());
            CreateConstructionHaulingTasks();
        }
    
        private void CreateConstructionHaulingTasks()
        {
            var resourceCosts = _buildingData.GetResourceCosts();
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

        protected void EnqueueCreateTakeResourceToBlueprintTask(ItemData resourceData)
        {
            Task task = new Task
            {
                TaskId = "Withdraw Item",
                Requestor = this,
                Payload = resourceData.ItemName,
                Profession = _buildingData.CraftersProfession,
            };
            TaskManager.Instance.AddTask(task);
        }
    
        public override void ReceiveItem(Item item)
        {
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
        }
    
        public void CreateConstructTask()
        {
            Task constuctTask = new Task()
            {
                TaskId = "Build Building",
                Requestor = this,
                Profession = _buildingData.CraftersProfession,
            };
            constuctTask.Enqueue();
        }
    
        public bool DoConstruction(float workAmount)
        {
            _remainingWork -= workAmount;
            if (_remainingWork <= 0)
            {
                CompleteConstruction();
                return true;
            }
            
            return false;
        }

        private void CompleteConstruction()
        {
            _remainingWork = _buildingData.WorkCost;
            _building.ColourArt(Building.ColourStates.Built);
            var interior = InteriorsManager.Instance.GenerateInterior(this);
            _building.IsBuilt = true;
            _building.Interior = interior;
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

        private void Update()
        {
            FollowCursor();
            CheckPlacement();
        }

        private void FollowCursor()
        {
            if(!_isPlanning) return;
        
            var cursorPos = Helper.ConvertMousePosToGridPos(UtilsClass.GetMouseWorldPosition());
            gameObject.transform.position = cursorPos;
        }

        public bool CheckPlacement()
        {
            if(!_isPlanning) return false;

            if (_building.CheckPlacement())
            {
                _building.ColourArt(Building.ColourStates.CanPlace);
                return true;
            }
            else
            {
                _building.ColourArt(Building.ColourStates.CantPlace);
                return false;
            }
        }

        public void LinkEntrance(Transform interiorEntrancePos)
        {
            _building.EntranceLink.endTransform = interiorEntrancePos;
        }
    }
}
