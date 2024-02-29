using System;
using System.Collections.Generic;
using CodeMonkey.Utils;
using Controllers;
using Managers;
using ScriptableObjects;
using Systems.Skills.Scripts;
using TaskSystem;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Systems.Buildings.Scripts
{
    public class Door : StructurePiece
    {
        private Tilemap _wallTilemap;
        private EDoorState _doorState;
        private DoorSO _doorSO;
        public Action OnDoorPlaced;

        [SerializeField] private SpriteRenderer _doorSprite;
        [SerializeField] private SpriteRenderer _doormatSprite;
        [SerializeField] private GameObject _topCBlocker, _bottomCBlocker, _leftCBlocker, _rightCBlocker;
        [SerializeField] private RuleTile _wallFillerRT;
        [SerializeField] private DoorOpener _horizontalDoorOpener;
        [SerializeField] private DoorOpener _verticalDoorOpener;

        private bool _isBeingPlaced => _doorState == EDoorState.BeingPlaced;

        public enum EDoorState
        {
            BeingPlaced,
            Construction,
            Built,
        }
        
        public void SetState(EDoorState state)
        {
            _doorState = state;
            switch (_doorState)
            {
                case EDoorState.BeingPlaced: 
                    BeingPlaced_Enter();
                    break;
                case EDoorState.Construction:
                    Construction_Enter();
                    break;
                case EDoorState.Built:
                    BuiltState_Enter();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void BeingPlaced_Enter()
        {

        }

        private void Construction_Enter()
        {
            _remainingResourceCosts = _doorSO.GetResourceCosts();
            ColourSprite(Librarian.Instance.GetColour("Blueprint"));

            var wall = StructureManager.Instance.GetStructureAtCell(Cell.CellPos);
            if (wall != null)
            {
                // Check if there's a wall under the door, if so cancel or deconstruct it first
                wall.CreateDeconstructionTask(true, () =>
                {
                    CreateConstructionHaulingTasks();
                    OnPlaced();
                });
            }
            else
            {
                CreateConstructionHaulingTasks();
                OnPlaced();
            }
        }

        private void Update()
        {
            if (_isBeingPlaced)
            {
                FollowCursor();
                var canPlace = CheckPlacement();
                if (canPlace)
                {
                    ColourSprite(Librarian.Instance.GetColour("Placement Green"));
                }
                else
                {
                    ColourSprite(Librarian.Instance.GetColour("Placement Red"));
                }
            }
        }
        
        private void FollowCursor()
        {
            var cursorPos = Helper.ConvertMousePosToGridPos(UtilsClass.GetMouseWorldPosition());
            gameObject.transform.position = cursorPos;
        }

        protected override void Awake()
        {
            base.Awake();

            _wallTilemap = TilemapController.Instance.GetTilemap(TilemapLayer.Structure);
        }

        public void Init(DoorSO doorSO)
        {
            _doorSO = doorSO;
            SetState(EDoorState.BeingPlaced);
            SetOrientationVertical(false);
        }

        public override void CreateConstructTask(bool autoAssign = true)
        {
            Task constuctTask = new Task("Build Construction", ETaskType.Construction, this, EToolType.BuildersHammer);
            constuctTask.Enqueue();
        }
        
        public override void CreateDeconstructionTask(bool autoAssign = true, Action onDeconstructed = null)
        {
            _onDeconstructed = onDeconstructed;
            Task constuctTask = new Task("Deconstruct", ETaskType.Construction, this, EToolType.BuildersHammer);
            constuctTask.Enqueue();
        }
        
        private void BuiltState_Enter()
        {
            ColourSprite(Color.white);
            AddWallFiller();
        }
        
        private void BuiltState_Exit()
        {
            
        }

        private void AddWallFiller()
        {
            var cell = _wallTilemap.WorldToCell(transform.position);
            _wallTilemap.SetTile(cell ,_wallFillerRT);
        }

        private void RemoveWallFiller()
        {
            var cell = _wallTilemap.WorldToCell(transform.position);
            _wallTilemap.SetTile(cell ,null);
        }

        public override void CompleteDeconstruction()
        {
            RemoveWallFiller();
            OnDeconstructed();
            base.CompleteDeconstruction();
        }

        private void ColourSprite(Color colour)
        {
            _doorSprite.color = colour;
            _doormatSprite.color = colour;
            
            if(_horizontalDoorOpener != null) _horizontalDoorOpener.ColourDoorSprite(colour);
            
            if(_verticalDoorOpener != null) _verticalDoorOpener.ColourDoorSprite(colour);
        }
        
        private void CreateConstructionHaulingTasks()
        {
            var resourceCosts = _doorSO.GetResourceCosts();
            CreateConstuctionHaulingTasksForItems(resourceCosts);
        }

        public override List<ItemAmount> GetResourceCosts()
        {
            return _doorSO.GetResourceCosts();
        }
        
        public override void CompleteConstruction()
        {
            base.CompleteConstruction();
            _isBuilt = true;
            IsClickDisabled = true;
            SetState(EDoorState.Built);
        }
        
        public void TriggerPlaced()
        {
            if (OnDoorPlaced != null)
            {
                OnDoorPlaced.Invoke();
            }
        }
        
        public bool CheckPlacement()
        {
            bool hasLeftWall = StructureManager.Instance.GetStructureCell(Cell.CellPos + Vector2Int.left) == EStructureCell.Wall;
            bool hasRightWall = StructureManager.Instance.GetStructureCell(Cell.CellPos + Vector2Int.right) == EStructureCell.Wall;
            bool hasDownWall = StructureManager.Instance.GetStructureCell(Cell.CellPos + Vector2Int.down) == EStructureCell.Wall;
            bool hasUpWall = StructureManager.Instance.GetStructureCell(Cell.CellPos + Vector2Int.up) == EStructureCell.Wall;

            if (hasLeftWall && hasRightWall && !hasUpWall && !hasDownWall)
            {
                SetOrientationVertical(false);
                return true;
            }

            if (!hasLeftWall && !hasRightWall && hasUpWall && hasDownWall)
            {
                SetOrientationVertical(true);
                return true;
            }
            
            return false;
        }

        private void SetOrientationVertical(bool isVertical)
        {
            if (isVertical)
            {
                _doorSprite.sprite = _doorSO.VerticalDoorframe;
                _doormatSprite.sprite = _doorSO.VerticalDoormat;
                _topCBlocker.SetActive(false);
                _bottomCBlocker.SetActive(false);
                _leftCBlocker.SetActive(true);
                _rightCBlocker.SetActive(true);
                _horizontalDoorOpener.gameObject.SetActive(false);
                _verticalDoorOpener.gameObject.SetActive(true);
            }
            else
            {
                _doorSprite.sprite = _doorSO.HorizontalDoorframe;
                _doormatSprite.sprite = _doorSO.HorizontalDoormat;
                _topCBlocker.SetActive(true);
                _bottomCBlocker.SetActive(true);
                _leftCBlocker.SetActive(false);
                _rightCBlocker.SetActive(false);
                _horizontalDoorOpener.gameObject.SetActive(true);
                _verticalDoorOpener.gameObject.SetActive(false);
            }
        }

        public override void RefreshTile()
        {
            
        }
    }
}
