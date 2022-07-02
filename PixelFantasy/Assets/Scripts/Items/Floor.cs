using System;
using System.Collections.Generic;
using Actions;
using Controllers;
using DataPersistence;
using Gods;
using HUD;
using Interfaces;
using Pathfinding;
using ScriptableObjects;
using Sirenix.OdinInspector;
using Tasks;
using Characters;
using UnityEngine;

namespace Items
{
    public class Floor : Construction
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private GraphUpdateScene _pathGraphUpdater;

        private FloorData _floorData;
        private List<int> _assignedTaskRefs = new List<int>();
        private Vector2 _floorPos;
        
        public TaskType PendingTask;
        
        public FloorData FloorData => _floorData;
        public Vector2 FloorPos => _floorPos;

        public void Init(FloorData floorData)
        {
            IsClickDisabled = false;
            _floorData = floorData;
            _floorPos = transform.position;
            _remainingResourceCosts = new List<ItemAmount> (_floorData.GetResourceCosts());
            _pendingResourceCosts = new List<ItemAmount>();
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
        
        private void CreateConstructionHaulingTasks()
        {
            var resourceCosts = _floorData.GetResourceCosts();
            CreateConstuctionHaulingTasksForItems(resourceCosts);
        }
        
        public override float GetWorkPerResource()
        {
            return _floorData.GetWorkPerResource();
        }
        
        public override void CompleteConstruction()
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
                incomingItem.EnqueueTaskForHauling();
            }
            _pendingResourceCosts.Clear();
            _incomingItems.Clear();
        }

        public void CreateDeconstructionTask()
        {
            _isDeconstructing = true;
            //SetIcon("Hammer");
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

        public void CancelDeconstruction()
        {
            _isDeconstructing = false;
            CancelTasks();
            //SetIcon(null);

            if (_incomingUnit != null)
            {
                _incomingUnit.CancelTask();
            }
        }

        public override List<ActionBase> GetActions()
        {
            return AvailableActions;
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
        
        public override List<ItemAmount> GetResourceCosts()
        {
            return _floorData.GetResourceCosts();
        }
        
        public override object CaptureState()
        {
            List<string> incomingItemsGUIDS = new List<string>();
            foreach (var incomingItem in _incomingItems)
            {
                incomingItemsGUIDS.Add(incomingItem.UniqueId);
            }
            
            return new Data
            {
                UID = this.UniqueId,
                FloorData = _floorData,
                ResourceCost = _remainingResourceCosts,
                IsBuilt = _isBuilt,
                AssignedTaskRefs = _assignedTaskRefs,
                IncomingItemsGUIDs = incomingItemsGUIDS,
                IsDeconstructing = _isDeconstructing,
                IncomingUnit = _incomingUnit,
                FloorPos = _floorPos,
                IsClickDisabled = IsClickDisabled,
                IsAllowed = IsAllowed,
                PendingTask = PendingTask,
                IncomingResourceCosts = _incomingResourceCosts,
                HasIncomingUnit = _hasUnitIncoming,
            };
        }

        public override void RestoreState(object data)
        {
            var state = (Data)data;

            this.UniqueId = state.UID;
            _floorData = state.FloorData;
            _remainingResourceCosts = state.ResourceCost;
            _isBuilt = state.IsBuilt;
            _assignedTaskRefs = state.AssignedTaskRefs;
            _isDeconstructing = state.IsDeconstructing;
            _incomingUnit = state.IncomingUnit;
            _floorPos = state.FloorPos;
            transform.position = _floorPos;
            IsClickDisabled = state.IsClickDisabled;
            IsAllowed = state.IsAllowed;
            PendingTask = state.PendingTask;
            _incomingResourceCosts = state.IncomingResourceCosts;
            _hasUnitIncoming = state.HasIncomingUnit;
            
            // var incomingItemsGUIDS = state.IncomingItemsGUIDs;
            // var itemsHandler = ControllerManager.Instance.ItemsHandler;
            //_incomingItems.Clear();
            // foreach (var incomingItemGUID in incomingItemsGUIDS)
            // {
            //     var item = itemsHandler.GetItemByUID(incomingItemGUID);
            //     if (item != null)
            //     {
            //         _incomingItems.Add(item);
            //     }
            // }
            
            ShowBlueprint(!_isBuilt);
            UpdateSprite();
            UpdateStretchToWalls();

            base.RestoreState(data);
        }

        public struct Data
        {
            public string UID;
            public FloorData FloorData;
            public List<ItemAmount> ResourceCost;
            public bool IsBuilt;
            public List<int> AssignedTaskRefs;
            public List<string> IncomingItemsGUIDs;
            public bool IsDeconstructing;
            public UnitTaskAI IncomingUnit; // TODO: will likely need to use GUID
            public bool HasIncomingUnit;
            public Vector2 FloorPos;
            public bool IsClickDisabled;
            public bool IsAllowed;
            public TaskType PendingTask;
            public List<ItemAmount> IncomingResourceCosts;
        }
    }
}
