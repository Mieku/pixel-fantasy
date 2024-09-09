using System;
using System.Collections.Generic;
using CodeMonkey.Utils;
using Controllers;
using Managers;
using ScriptableObjects;
using UnityEngine;

namespace Systems.Buildings.Scripts
{
    public class Door : StructurePiece
    {
        [SerializeField] private SpriteRenderer _doorSprite;
        [SerializeField] private SpriteRenderer _doormatSprite;
        [SerializeField] private GameObject _topCBlocker, _bottomCBlocker, _leftCBlocker, _rightCBlocker;
        [SerializeField] private RuleTile _wallFillerRT;
        [SerializeField] private DoorOpener _horizontalDoorOpener;
        [SerializeField] private DoorOpener _verticalDoorOpener;

        private bool _isBeingPlaced => RuntimeDoorData.State == EConstructionState.Planning;
        
        public DoorData RuntimeDoorData => RuntimeData as DoorData;
        public override string DisplayName => RuntimeDoorData.DoorSettings.DoorName;
        protected override List<SpriteRenderer> SpritesToOutline => new List<SpriteRenderer> { _doorSprite, _doormatSprite };

        public override bool IsSimilar(PlayerInteractable otherPI)
        {
            if (otherPI is Door)
            {
                return true;
            }

            return false;
        }

        public void Init(DoorSettings doorSettings, DyeSettings matColour)
        {
            RuntimeData = new DoorData();
            RuntimeDoorData.AssignDoorSettings(doorSettings, matColour);
            
            SetState(EConstructionState.Planning);
            SetOrientationVertical(false);

            IsAllowed = true;
            RefreshAllowedDisplay();
            RefreshAllowCommands();
        }
        
        public override void LoadData(ConstructionData data)
        {
            RuntimeData = data;
            LoadState(data.State);
            SetOrientationVertical(RuntimeDoorData.IsVertical);
            
            StructureDatabase.Instance.RegisterStructure(this);
            RefreshTaskIcon();
            RefreshAllowedDisplay();
            RefreshAllowCommands();
            
            if (RuntimeDoorData.IsConstructionBlocked)
            {
                var wall = StructureDatabase.Instance.GetWallAtCell(Cell.CellPos);
                wall.OnDestroyed = (interactable =>
                {
                    CreateConstructionHaulingTasks();
                    RuntimeDoorData.IsConstructionBlocked = false;
                });
            }
        }

        private void LoadState(EConstructionState state)
        {
            switch (state)
            {
                case EConstructionState.Blueprint:
                    ColourSprite(Librarian.Instance.GetColour("Blueprint"));
                    break;
                case EConstructionState.Built:
                    ColourSprite(Color.white);
                    break;
                case EConstructionState.Planning:
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        public void OnPlanningComplete()
        {
            OnPlaced();
        }
  
        public void SetState(EConstructionState state)
        {
            RuntimeDoorData.State = state;
            switch (state)
            {
                case EConstructionState.Planning: 
                    break;
                case EConstructionState.Blueprint:
                    Construction_Enter();
                    break;
                case EConstructionState.Built:
                    BuiltState_Enter();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private void Construction_Enter()
        {
            ColourSprite(Librarian.Instance.GetColour("Blueprint"));

            var wall = StructureDatabase.Instance.GetWallAtCell(Cell.CellPos);
            if (wall != null)
            {
                // Check if there's a wall under the door, if so cancel or deconstruct it first
                RuntimeDoorData.IsConstructionBlocked = true;
                wall.CreateDeconstructionTask(() =>
                {
                    CreateConstructionHaulingTasks();
                    RuntimeDoorData.IsConstructionBlocked = false;
                });
            }
            else
            {
                CreateConstructionHaulingTasks();
                RuntimeDoorData.IsConstructionBlocked = false;
            }
            
            RefreshAllowedDisplay();
            RefreshAllowCommands();
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
            var cursorPos = Helper.SnapToGridPos(UtilsClass.GetMouseWorldPosition());
            gameObject.transform.position = cursorPos;
        }
        
        private void BuiltState_Enter()
        {
            ColourSprite(Color.white);
            AddWallFiller();
            
            RefreshAllowedDisplay();
            RefreshAllowCommands();
        }

        private void AddWallFiller()
        {
            TilemapController.Instance.SetTile(TilemapLayer.Structure, 
                transform.position, _wallFillerRT);
        }

        private void RemoveWallFiller()
        {
            TilemapController.Instance.SetTile(TilemapLayer.Structure, 
                transform.position, null);
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
            var resourceCosts = RuntimeDoorData.DoorSettings.CraftRequirements.GetMaterialCosts();
            CreateConstuctionHaulingTasksForItems(resourceCosts);
        }
        
        public override void CompleteConstruction()
        {
            base.CompleteConstruction();
            SetState(EConstructionState.Built);
            
            RefreshAllowedDisplay();
            RefreshAllowCommands();
        }
        
        public bool CheckPlacement()
        {
            bool hasLeftWall = StructureDatabase.Instance.GetStructureCell(Cell.CellPos + Vector2Int.left) == EStructureCell.Wall;
            bool hasRightWall = StructureDatabase.Instance.GetStructureCell(Cell.CellPos + Vector2Int.right) == EStructureCell.Wall;
            bool hasDownWall = StructureDatabase.Instance.GetStructureCell(Cell.CellPos + Vector2Int.down) == EStructureCell.Wall;
            bool hasUpWall = StructureDatabase.Instance.GetStructureCell(Cell.CellPos + Vector2Int.up) == EStructureCell.Wall;

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
            RuntimeDoorData.IsVertical = isVertical;
            
            if (isVertical)
            {
                _doorSprite.sprite = RuntimeDoorData.DoorSettings.VerticalDoorframe;;
                _doormatSprite.sprite = RuntimeDoorData.DoorSettings.VerticalDoormat;
                _topCBlocker.SetActive(false);
                _bottomCBlocker.SetActive(false);
                _leftCBlocker.SetActive(true);
                _rightCBlocker.SetActive(true);
                _horizontalDoorOpener.gameObject.SetActive(false);
                _verticalDoorOpener.gameObject.SetActive(true);
            }
            else
            {
                _doorSprite.sprite = RuntimeDoorData.DoorSettings.HorizontalDoorframe;
                _doormatSprite.sprite = RuntimeDoorData.DoorSettings.HorizontalDoormat;
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

        public override void DoCopy()
        {
            base.DoCopy();
            HUDController.Instance.ShowBuildStructureDetails(RuntimeDoorData.DoorSettings);
        }
        
        public override void DeletePiece()
        {
            Destroy(gameObject);
        }
    }
}
