using System.Collections.Generic;
using Gods;
using ScriptableObjects;
using UnityEngine;

namespace Items
{
    public class Floor : Construction
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private FloorData _floorData;
        private List<int> _assignedTaskRefs = new List<int>();
        private Vector2 _floorPos;
        
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
            ShowBlueprint(true);
            PrepForConstruction();
        }

        private void UpdateSprite()
        {
            _spriteRenderer.sprite = _floorData.Icon;
        }
        
        private void ShowBlueprint(bool showBlueprint)
        {
            if (showBlueprint)
            {
                _spriteRenderer.color = Librarian.Instance.GetColour("Blueprint");
            }
            else
            {
                _spriteRenderer.color = Color.white;
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

                    // if (!growResource.QueuedToCut)
                    // {
                    //     growResource.CreateTaskById("Cut Plant");
                    // }
                    //growResource.CreateTaskById("Cut Plant");
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
            base.CompleteConstruction();
            ShowBlueprint(false);
            _isBuilt = true;
            IsClickDisabled = true;
        }
        
        public override void CancelConstruction()
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
            // Drop all incoming resources
            foreach (var incomingItem in _incomingItems)
            {
                incomingItem.SeekForSlot();
            }
            _pendingResourceCosts.Clear();
            _incomingItems.Clear();
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
                FloorPos = _floorPos,
                IsClickDisabled = IsClickDisabled,
                IsAllowed = IsAllowed,
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
            _floorPos = state.FloorPos;
            transform.position = _floorPos;
            IsClickDisabled = state.IsClickDisabled;
            IsAllowed = state.IsAllowed;
            _incomingResourceCosts = state.IncomingResourceCosts;
            _hasUnitIncoming = state.HasIncomingUnit;
            
            ShowBlueprint(!_isBuilt);
            UpdateSprite();

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
            public bool HasIncomingUnit;
            public Vector2 FloorPos;
            public bool IsClickDisabled;
            public bool IsAllowed;
            public List<ItemAmount> IncomingResourceCosts;
        }
    }
}
