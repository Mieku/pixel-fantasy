using System;
using System.Collections.Generic;
using Controllers;
using Items;
using Managers;
using ScriptableObjects;
using Systems.Skills.Scripts;
using TaskSystem;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Systems.Buildings.Scripts
{
    public class Wall : StructurePiece
    {
        [SerializeField] private SpriteRenderer _cellRenderer;
        [SerializeField] private GameObject _obstacle;
        
        private WallOption _wallOption;
        private EWallState _wallState;
        private Tilemap _structureTilemap;

        public enum EWallState
        {
            
            Blueprint,
            Built,
        }

        private void AssignWallState(EWallState state)
        {
            _wallState = state;
            switch (_wallState)
            {
                case EWallState.Blueprint:
                    BlueprintState_Enter();
                    break;
                case EWallState.Built:
                    BuiltState_Enter();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public void ChangeWallState(EWallState newState)
        {
            if (_wallState != newState)
            {
                switch (_wallState)
                {
                    case EWallState.Blueprint:
                        BlueprintState_Exit();
                        break;
                    case EWallState.Built:
                        BuiltState_Exit();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                AssignWallState(newState);
            }
        }
        
        protected override void Awake()
        {
            base.Awake();
            
            _structureTilemap = TilemapController.Instance.GetTilemap(TilemapLayer.Structure);
            _cellRenderer.enabled = false;
        }

        public void Init(WallOption wallOption)
        {
            _wallOption = wallOption;
            
            AssignWallState(EWallState.Blueprint);
        }

        public override void CreateConstructTask(bool autoAssign = true)
        {
            Task constuctTask = new Task("Build Construction", this, _wallOption.RequiredJob, EToolType.BuildersHammer, SkillType.Construction);
            constuctTask.Enqueue();
        }
        
        public override void CreateDeconstructionTask(bool autoAssign = true, Action onDeconstructed = null)
        {
            _onDeconstructed = onDeconstructed;
            Task constuctTask = new Task("Deconstruct", this, _wallOption.RequiredJob, EToolType.BuildersHammer, SkillType.Construction);
            constuctTask.Enqueue();
        }

        private void BlueprintState_Enter()
        {
            _remainingResourceCosts = _wallOption.OptionResourceCosts;
            _cellRenderer.enabled = false;
            OnPlaced();
            EnableObstacle(false);
            RefreshTile();
            ColourTile(Librarian.Instance.GetColour("Blueprint"));
            CreateConstructionHaulingTasks();
        }
        
        private void BlueprintState_Exit()
        {
            
        }

        private void BuiltState_Enter()
        {
            ColourTile(Color.white);
            EnableObstacle(true);
        }
        
        private void BuiltState_Exit()
        {
            
        }
        
        public override void RefreshTile()
        {
            var cell = _structureTilemap.WorldToCell(transform.position);
            
            bool isInterior = StructureManager.Instance.IsInteriorBelow(Cell.CellPos);
            if (isInterior)
            {
                _structureTilemap.SetTile(cell, _wallOption.InteriorWallRules);
            }
            else
            {
                _structureTilemap.SetTile(cell, _wallOption.ExteriorWallRules);
            }
        }

        private void ColourTile(Color colour)
        {
            var cell = _structureTilemap.WorldToCell(transform.position);
            _structureTilemap.SetColor(cell, colour);
        }

        public override void CompleteDeconstruction()
        {
            OnDeconstructed();
            base.CompleteDeconstruction();
        }
        
        private void CreateConstructionHaulingTasks()
        {
            var resourceCosts = _wallOption.OptionResourceCosts;
            CreateConstuctionHaulingTasksForItems(resourceCosts);
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
        
        public override void CompleteConstruction()
        {
            base.CompleteConstruction();
            _isBuilt = true;
            IsClickDisabled = true;
            ChangeWallState(EWallState.Built);
        }

        public void EnableObstacle(bool isEnabled)
        {
            _obstacle.SetActive(isEnabled);
        }
    }
}
