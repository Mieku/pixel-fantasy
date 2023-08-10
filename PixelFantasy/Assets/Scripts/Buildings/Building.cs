using System;
using System.Collections.Generic;
using CodeMonkey.Utils;
using Items;
using Managers;
using ScriptableObjects;
using TaskSystem;
using UnityEngine;

namespace Buildings
{
    public class Building : Construction
    {
        public string BuildingID;
        
        [SerializeField] private BuildingData _buildingData;
        [SerializeField] private Footings _footings;
        [SerializeField] private GameObject _internalHandle;
        [SerializeField] private GameObject _exteriorHandle;
        [SerializeField] private GameObject _roofHandle;
        [SerializeField] private GameObject _doorHandle;
        [SerializeField] private GameObject _floorHandle;
        [SerializeField] private GameObject _obstaclesHandle;
        [SerializeField] private GameObject _shadowboxHandle;
        [SerializeField] private Transform _constructionStandPos;

        private BuildingState _state;

        protected override void Awake()
        {
            base.Awake();
            GameEvents.OnHideRoofsToggled += ToggleInternalView;
        }

        private void Start()
        {
            ToggleInternalView(false);
        }

        private void OnDestroy()
        {
            GameEvents.OnHideRoofsToggled -= ToggleInternalView;
        }

        public void SetState(BuildingState state)
        {
            _state = state;
            switch (state)
            {
                case BuildingState.Planning:
                    Plan_Enter();
                    break;
                case BuildingState.Construction:
                    Construction_Enter();
                    break;
                case BuildingState.Built:
                    Built_Enter();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        public void ToggleInternalView(bool showInternal)
        {
            if (showInternal)
            {
                _roofHandle.SetActive(false);
                _exteriorHandle.SetActive(false);
                _shadowboxHandle.SetActive(true);
            }
            else
            {
                _roofHandle.SetActive(true);
                _exteriorHandle.SetActive(true);
                _shadowboxHandle.SetActive(false);
            }
        }
        
        public bool CheckPlacement()
        {
            return _footings.FootingsValid(_buildingData.InvalidPlacementTags);
        }

        private void Update()
        {
            if (_state == BuildingState.Planning)
            {
                FollowCursor();
                CheckPlacement();
            }
        }

        private void Plan_Enter()
        {
            _footings.DisplayFootings(true);
            _obstaclesHandle.SetActive(false);
        }

        private void Construction_Enter()
        {
            _footings.DisplayFootings(false);
            _obstaclesHandle.SetActive(true);
            ColourSprites(Librarian.Instance.GetColour("Blueprint"));
            
            _remainingResourceCosts = new List<ItemAmount> (_buildingData.GetResourceCosts());
            CreateConstructionHaulingTasks();
        }

        public override void CompleteConstruction()
        {
            base.CompleteConstruction();
            SetState(BuildingState.Built);
        }

        private void Built_Enter()
        {
            ColourSprites(Color.white);
        }
        
        private void FollowCursor()
        {
            var cursorPos = Helper.ConvertMousePosToGridPos(UtilsClass.GetMouseWorldPosition());
            gameObject.transform.position = cursorPos;
        }

        /// <summary>
        /// No point keeping the sorter after the building is placed
        /// </summary>
        private void RemoveRendererSorter()
        {
            var sorter = GetComponent<PositionRendererSorter>();
            if (sorter != null)
            {
                sorter.DestroySelf();
            }
        }

        private void ColourSprites(Color colour)
        {
            var internalSpriteRenderers = _internalHandle.GetComponentsInChildren<SpriteRenderer>(true);
            var exteriorlSpriteRenderers = _exteriorHandle.GetComponentsInChildren<SpriteRenderer>(true);
            var roofSpriteRenderers = _roofHandle.GetComponentsInChildren<SpriteRenderer>(true);
            var doorSpriteRenderers = _doorHandle.GetComponentsInChildren<SpriteRenderer>(true);
            var floorSpriteRenderers = _floorHandle.GetComponentsInChildren<SpriteRenderer>(true);

            foreach (var rend in internalSpriteRenderers)
            {
                rend.color = colour;
            }
            
            foreach (var rend in exteriorlSpriteRenderers)
            {
                rend.color = colour;
            }
            
            foreach (var rend in roofSpriteRenderers)
            {
                rend.color = colour;
            }
            
            foreach (var rend in doorSpriteRenderers)
            {
                rend.color = colour;
            }
            
            foreach (var rend in floorSpriteRenderers)
            {
                rend.color = colour;
            }
        }
        
        private void CreateConstructionHaulingTasks()
        {
             var resourceCosts = _buildingData.GetResourceCosts();
             CreateConstuctionHaulingTasksForItems(resourceCosts);
        }

        protected override void EnqueueCreateTakeResourceToBlueprintTask(ItemData resourceData)
        {
            Task task = new Task
            {
                TaskId = "Withdraw Item Construction",
                Requestor = this,
                Payload = resourceData.ItemName,
                TaskType = TaskType.Haul,
            };
            TaskManager.Instance.AddTask(task);
        }

        public override void CreateConstructTask(bool autoAssign = true)
        {
            Task constuctTask = new Task()
            {
                TaskId = "Build Building",
                Requestor = this,
            };
            constuctTask.Enqueue();
        }

        public Vector2 ConstructionStandPosition()
        {
            return _constructionStandPos.position;
        }
        
        public enum BuildingState
        {
            Planning,
            Construction,
            Built,
        }
        
    }
}
