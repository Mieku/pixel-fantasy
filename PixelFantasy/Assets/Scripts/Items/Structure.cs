using System.Collections.Generic;
using Actions;
using Controllers;
using Gods;
using HUD;
using Pathfinding;
using ScriptableObjects;
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
            InformNearbyDoors();
            PrepForConstruction();
        }
        
        public override ConstructionData GetConstructionData()
        {
            return _structureData;
        }

        private void InformNearbyDoors()
        {
            var pos = transform.position;
            var leftPos = new Vector2(pos.x - 1, pos.y);
            var rightPos = new Vector2(pos.x + 1, pos.y);
            var topPos = new Vector2(pos.x, pos.y + 1);
            var bottomPos = new Vector2(pos.x, pos.y - 1);

            var leftDoors = Helper.GetGameObjectsOnTile(leftPos, "Door");
            var rightDoors = Helper.GetGameObjectsOnTile(rightPos, "Door");
            var topDoors = Helper.GetGameObjectsOnTile(topPos, "Door");
            var bottomDoors = Helper.GetGameObjectsOnTile(bottomPos, "Door");

            foreach (var doorGO in leftDoors)
            {
                var door = doorGO.GetComponent<Door>();
                if (door != null)
                {
                    door.DetermineDirection();
                }
            }
            foreach (var doorGO in rightDoors)
            {
                var door = doorGO.GetComponent<Door>();
                if (door != null)
                {
                    door.DetermineDirection();
                }
            }
            foreach (var doorGO in topDoors)
            {
                var door = doorGO.GetComponent<Door>();
                if (door != null)
                {
                    door.DetermineDirection();
                }
            }
            foreach (var doorGO in bottomDoors)
            {
                var door = doorGO.GetComponent<Door>();
                if (door != null)
                {
                    door.DetermineDirection();
                }
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
        }

        private void ClearTile()
        {
            var cell = _structureTilemap.WorldToCell(transform.position);
            _structureTilemap.SetTile(cell, null);
        }

        private void ColourTile(Color colour)
        {
            var cell = _structureTilemap.WorldToCell(transform.position);
            _structureTilemap.SetColor(cell, colour);
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

                    // if (!growResource.QueuedToCut)
                    // {
                    //     
                    // }
                    growResource.CreateTaskById("Cut Plant");
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
            base.CompleteConstruction();
            IncomingUnit = null;
            ShowBlueprint(false);
            _isBuilt = true;
        }
        
        public override void CancelConstruction()
        {
            if (!_isBuilt)
            {
                CancelAllTasks();
                
                // Restore the claimed to the slots
                var claimed = GetClaimedResourcesCosts();
                foreach (var claimedAmount in claimed)
                {
                    ControllerManager.Instance.InventoryController.RestoreClaimedResource(claimedAmount);
                }
                
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
            
            if (_onDeconstructed != null)
            {
                _onDeconstructed.Invoke();
            }

            // Delete the structure
            Destroy(gameObject);
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
