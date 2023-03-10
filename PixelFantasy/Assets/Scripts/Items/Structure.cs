using System.Collections.Generic;
using Controllers;
using Gods;
using HUD;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Items
{
    public class Structure : Construction
    {
        [SerializeField] private ProgressBar _progressBar;

        private StructureData _structureData;
        private Tilemap _structureTilemap;
        
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
            }
            else
            {
                ColourTile(Color.white);
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
                    //growResource.CreateTaskById("Cut Plant");
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
            ShowBlueprint(false);
            _isBuilt = true;
        }
        
        public override void CancelConstruction()
        {
            if (!_isBuilt)
            {
                //CancelAllTasks();
                
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

        public override List<ItemAmount> GetResourceCosts()
        {
            return _structureData.GetResourceCosts();
        }

        public override object CaptureState()
        {
            return new Data
            {
                UID = this.UniqueId,
                Position = transform.position,
                StructureData = _structureData,
                ResourceCost = _remainingResourceCosts,
                IsBuilt = _isBuilt,
                IsDeconstructing = _isDeconstructing,
                StructureTilemap = _structureTilemap,
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
            _isDeconstructing = state.IsDeconstructing;
            _structureTilemap = state.StructureTilemap;
            _incomingResourceCosts = state.IncomingResourceCosts;
            _hasUnitIncoming = state.HasIncomingUnit;

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
            public bool IsDeconstructing;
            public bool HasIncomingUnit;
            public Tilemap StructureTilemap;
            public List<ItemAmount> IncomingResourceCosts;
        }
    }
}
