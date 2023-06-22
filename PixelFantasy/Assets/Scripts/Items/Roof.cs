using System;
using System.Collections.Generic;
using Controllers;
using Managers;
using ScriptableObjects;
using TaskSystem;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Items
{
    public class Roof : Construction
    {
        [SerializeField] private GameObject _shadeHandle;
        [SerializeField] private SpriteRenderer _guideHandle;

        private Tilemap _roofTM;
        private RoofData _roofData;
        private State _state;
        private bool _displayGuide;
        private bool _displayGuideOverride;

        protected override void Awake()
        {
            base.Awake();
            GameEvents.OnRoofRefresh += GameEvent_OnRoofRefresh;
            GameEvents.OnRoofGuideToggled += EnableRoofGuide;
        }

        private void OnDestroy()
        {
            GameEvents.OnRoofRefresh -= GameEvent_OnRoofRefresh;
            GameEvents.OnRoofGuideToggled -= EnableRoofGuide;
        }

        private void GameEvent_OnRoofRefresh(Vector2 callerPos)
        {
            CheckWallDistances(callerPos);
        }

        private void EnableRoofGuide(bool shouldEnable)
        {
            _displayGuideOverride = shouldEnable;
            RefreshGuide();
        }

        public void Init(RoofData roofData)
        {
            _roofTM = TilemapController.Instance.GetTilemap(TilemapLayer.Roof);
            _roofData = roofData;
            _displayGuideOverride = RoofManager.Instance.RoofsShown;
            
            CheckWallDistances(transform.position);
        }
        
        public void CheckWallDistances(Vector2 callerPos)
        {
            if (!Helper.IsPositionWithinRadius(transform.position, callerPos, _roofData.MaxDistanceFromWall)) return; // No point calling if out of range
            
            var surroundingPoses = Helper.GetSurroundingGridPositions(transform.position, true, _roofData.MaxDistanceFromWall, true);
            foreach (var surroundingPos in surroundingPoses)
            {
                var wall = Helper.GetObjectAtPosition<StructurePiece>(surroundingPos);
                if (wall != null && wall.IsBuilt)
                {
                    if (!_isBuilt)
                    {
                        SetState(State.CanBuild);
                    }
                    return;
                }
            }
            
            if (_isBuilt)
            {
                // TODO: Handle Roof Collapse
                Debug.Log("This Roof Should Collapse");
            }
            else
            {
                SetState(State.OutOfRange);
            }
        }

        private void SetState(State state)
        {
            _state = state;
            switch (state)
            {
                case State.CanBuild:
                    SetCanBuild();
                    break;
                case State.OutOfRange:
                    SetOutOfRange();
                    break;
                case State.Built:
                    SetBuilt();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private void HideGuide()
        {
            _displayGuide = false;
            RefreshGuide();
        }
        
        private void ShowGuide(Color colour)
        {
            Color gcolour = new Color(colour.r, colour.g, colour.b, .40f);
            _guideHandle.color = gcolour;
            _displayGuide = true;
            RefreshGuide();
        }

        private void RefreshGuide()
        {
            if (_displayGuide && _displayGuideOverride)
            {
                _guideHandle.gameObject.SetActive(true);
            }
            else
            {
                _guideHandle.gameObject.SetActive(false);
            }
        }

        private void SetOutOfRange()
        {
            _shadeHandle.SetActive(false);
            ShowGuide(Color.red);
        }

        private void SetCanBuild()
        {
            _shadeHandle.SetActive(false);
            ShowGuide(Librarian.Instance.GetColour("Blueprint"));
            SetTile();
            ColourTile(Librarian.Instance.GetColour("Blueprint"));
            PrepForConstruction();
        }

        private void SetBuilt()
        {
            _isBuilt = true;
            _shadeHandle.SetActive(true);
            ColourTile(Color.white);
            HideGuide();
        }
        
        private void PrepForConstruction()
        {
            _isBuilt = false;
            _remainingResourceCosts = new List<ItemAmount> (_roofData.GetResourceCosts());
            _pendingResourceCosts = new List<ItemAmount>();
            
            CreateConstructionHaulingTasks();
        }
        
        private void CreateConstructionHaulingTasks()
        {
            var resourceCosts = _roofData.GetResourceCosts();
            CreateConstuctionHaulingTasksForItems(resourceCosts);
        }
        
        protected override void EnqueueCreateTakeResourceToBlueprintTask(ItemData resourceData)
        {
            Task task = new Task
            {
                TaskId = "Withdraw Item",
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
                TaskId = "Build Construction",
                Requestor = this,
                TaskType = TaskType.Construction,
            };
            constuctTask.Enqueue();
        }
    
        public override void CompleteConstruction()
        {
            base.CompleteConstruction();
            SetState(State.Built);
        }

        private void SetTile()
        {
            var cell = _roofTM.WorldToCell(GetCellPos());
            _roofTM.SetTile(cell, _roofData.RuleTile);
        }

        private void RemoveTile()
        {
            var cell = _roofTM.WorldToCell(GetCellPos());
            _roofTM.SetTile(cell, null);
        }

        private Vector2 GetCellPos()
        {
            // The cell is actually offset
            return new Vector2(transform.position.x, transform.position.y + 1f);
        }

        private void ColourTile(Color colour)
        {
            var cell = _roofTM.WorldToCell(GetCellPos());
            _roofTM.SetColor(cell, colour);
        }

        private enum State
        {
            CanBuild,
            OutOfRange,
            Built,
        }
    }
}
