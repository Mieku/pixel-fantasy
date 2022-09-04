using System.Collections.Generic;
using Actions;
using Characters;
using Gods;
using HUD;
using Pathfinding;
using ScriptableObjects;
using UnityEngine;

namespace Items
{
    public class Door : Construction
    {
        [SerializeField] private SpriteRenderer _doorRenderer;
        [SerializeField] private DynamicGridObstacle _gridObstacle;
        
        private DoorData _doorData;
        private bool _isLocked;
        private int _defaultLayerNum;
        private bool _isHorizontal;
        
        public void Init(DoorData doorData)
        {
            _defaultLayerNum = gameObject.layer;
            _doorData = doorData;
            _remainingResourceCosts = new List<ItemAmount> (_doorData.GetResourceCosts());
            _pendingResourceCosts = new List<ItemAmount>();
            ShowBlueprint(true);
            PrepForConstruction();
        }
        
        public override List<ItemAmount> GetResourceCosts()
        {
            return _doorData.GetResourceCosts();
        }

        public void ToggleLocked()
        {
            SetLocked(!_isLocked);
        }
        
        public void SetLocked(bool isLocked)
        {
            _isLocked = isLocked;
            _gridObstacle.enabled = _isLocked;
            if (_isLocked)
            {
                DisplayTaskIcon(Librarian.Instance.GetSprite("Lock"));
                _gridObstacle.enabled = true;
                gameObject.layer = 3;
            }
            else
            {
                DisplayTaskIcon(null);
                _gridObstacle.enabled = false;
                gameObject.layer = _defaultLayerNum;
            }
        }

        public override List<ActionBase> GetActions()
        {
            return AvailableActions;
        }

        public override ConstructionData GetConstructionData()
        {
            return _doorData;
        }
        
        private void PrepForConstruction()
        {
            DetermineDirection();
            
            if (Helper.DoesGridContainTag(transform.position, "Nature"))
            {
                ClearNatureFromTile();
                return;
            }
            
            // If placed on a wall, deconstruct the wall first
            if (Helper.DoesGridContainTag(transform.position, "Wall"))
            {
                DeconstructWall();
                return;
            }
            
            // Once on dirt create the hauling tasks
            CreateConstructionHaulingTasks();
        }

        private void DeconstructWall()
        {
            var wallGOs = Helper.GetGameObjectsOnTile(transform.position, "Wall");
            foreach (var wallGO in wallGOs)
            {
                var wall = wallGO.GetComponent<Structure>();
                if (wall != null)
                {
                    if (wall.IsBuilt)
                    {
                        wall.CreateDeconstructionTask(true, PrepForConstruction);
                    }
                    else
                    {
                        wall.CancelConstruction();
                        PrepForConstruction();
                        return;
                    }
                }
            }
        }

        public void DetermineDirection()
        {
            // If there is a wall above or below it, use Vertical. Else Horizontal
            var pos = transform.position;
            var topPos = new Vector2(pos.x, pos.y + 1);
            var bottomPos = new Vector2(pos.x, pos.y - 1);
            bool isWallAbove = Helper.DoesGridContainTag(topPos, "Wall");
            bool isWallBelow = Helper.DoesGridContainTag(bottomPos, "Wall");
            if (isWallAbove || isWallBelow)
            {
                _doorRenderer.sprite = _doorData.VerticalSprite;
                _isHorizontal = false;
            }
            else
            {
                _doorRenderer.sprite = _doorData.HorizontalSprite;
                _isHorizontal = true;
            }
        }
        
        private void CreateConstructionHaulingTasks()
        {
            var resourceCosts = _doorData.GetResourceCosts();
            CreateConstuctionHaulingTasksForItems(resourceCosts);
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
        
        private void ShowBlueprint(bool showBlueprint)
        {
            if (showBlueprint)
            {
                _doorRenderer.color = Librarian.Instance.GetColour("Blueprint");
            }
            else
            {
                _doorRenderer.color = Color.white;
            }
        }
        
        public override float GetWorkPerResource()
        {
            return _doorData.GetWorkPerResource();
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
                
                // Delete this blueprint
                Destroy(gameObject);
            }
        }
        
        public override void CompleteDeconstruction()
        {
            // Spawn some of the used resources
            SpawnUsedResources(50f);
            
            var infoPanel = FindObjectOfType<SelectedItemInfoPanel>();
            if (infoPanel != null)
            {
                infoPanel.HideItemDetails();
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
            ShowBlueprint(!_isBuilt);

            if (_isHorizontal)
            {
                _doorRenderer.sprite = _doorData.HorizontalSprite;
            }
            else
            {
                _doorRenderer.sprite = _doorData.VerticalSprite;
            }
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
                DoorData = _doorData,
                ResourceCost = _remainingResourceCosts,
                IsBuilt = _isBuilt,
                IncomingItemsUIDs = incomingItemsGUIDS,
                IsDeconstructing = _isDeconstructing,
                IncomingUnit = _incomingUnit,
                IncomingResourceCosts = _incomingResourceCosts,
                HasIncomingUnit = _hasUnitIncoming,
                IsLocked = _isLocked,
                IsHorizontal = _isHorizontal,
            };
        }

        public override void RestoreState(object data)
        {
            var state = (Data)data;
            
            UniqueId = state.UID;
            transform.position = state.Position;
            _doorData = state.DoorData;
            _remainingResourceCosts = state.ResourceCost;
            _isBuilt = state.IsBuilt;
            _isDeconstructing = state.IsDeconstructing;
            _incomingUnit = state.IncomingUnit;
            _incomingResourceCosts = state.IncomingResourceCosts;
            _hasUnitIncoming = state.HasIncomingUnit;
            _isHorizontal = state.IsHorizontal;
            
            SetLocked(state.IsLocked);
            
            Refresh();
            
            base.RestoreState(data);
        }

        public struct Data
        {
            public string UID;
            public Vector3 Position;
            public DoorData DoorData;
            public List<ItemAmount> ResourceCost;
            public bool IsBuilt;
            public List<string> IncomingItemsUIDs;
            public bool IsDeconstructing;
            public bool HasIncomingUnit;
            public UnitTaskAI IncomingUnit; // TODO: will likely need to use GUID
            public List<ItemAmount> IncomingResourceCosts;
            public bool IsLocked;
            public bool IsHorizontal;
        }
    }
}
