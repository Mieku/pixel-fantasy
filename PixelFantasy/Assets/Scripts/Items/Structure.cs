using System;
using System.Collections.Generic;
using Actions;
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
    public class Structure : Construction
    {
        [SerializeField] private ProgressBar _progressBar;
        [SerializeField] private DynamicGridObstacle _gridObstacle;

        private StructureData _structureData;

        private List<int> _assignedTaskRefs = new List<int>();
        private Tilemap _structureTilemap;

        public TaskType PendingTask;
        
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

        private void CreateConstructionHaulingTasks()
        {
            var resourceCosts = _structureData.GetResourceCosts();
            CreateConstuctionHaulingTasksForItems(resourceCosts);
        }
        
        public override float GetWorkPerResource()
        {
            return _structureData.GetWorkPerResource();
        }

        public override void CompleteConstruction()
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
                incomingItem.EnqueueTaskForHauling();
            }
            
            _pendingResourceCosts.Clear();
        }
        
        public override void CompleteDeconstruction()
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
            //SetIcon(null);

            if (_incomingUnit != null)
            {
                _incomingUnit.CancelTask();
            }
        }

        public bool IsDeconstucting()
        {
            return _isDeconstructing;
        }

        private void Refresh()
        {
            SetTile();
            ShowBlueprint(!_isBuilt);
        }

        public override List<ActionBase> GetActions()
        {
            return AvailableActions;
        }

        public override List<ItemAmount> GetResourceCosts()
        {
            return _structureData.GetResourceCosts();
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
                IncomingResourceCosts = _incomingResourceCosts,
                HasIncomingUnit = _hasUnitIncoming,
            };
        }

        public override void RestoreState(object data)
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
            _incomingResourceCosts = state.IncomingResourceCosts;
            _hasUnitIncoming = state.HasIncomingUnit;

            // var incomingItemsGUIDS = state.IncomingItemsUIDs;
            // var itemsHandler = ControllerManager.Instance.ItemsHandler;
            // _incomingItems.Clear();
            // foreach (var incomingItemGUID in incomingItemsGUIDS)
            // {
            //     var itemObj = UIDManager.Instance.GetGameObject(incomingItemGUID);//itemsHandler.GetItemByUID(incomingItemGUID);
            //     var item = itemObj.GetComponent<Item>();
            //     if (item != null)
            //     {
            //         _incomingItems.Add(item);
            //     }
            // }

            Refresh();
            
            base.RestoreState(data);
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
            public bool HasIncomingUnit;
            public UnitTaskAI IncomingUnit; // TODO: will likely need to use GUID
            public Tilemap StructureTilemap;
            public TaskType PendingTask;
            public List<ItemAmount> IncomingResourceCosts;
        }
    }
}
