using System;
using Controllers;
using Managers;
using ScriptableObjects;
using TaskSystem;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Systems.Buildings.Scripts
{
    public class Wall : StructurePiece
    {
        [SerializeField] private GameObject _obstacle;
        
        public WallData RuntimeWallData => RuntimeData as WallData;
        
        private Tilemap _structureTilemap;
        public override string DisplayName => RuntimeWallData.SelectedWallOption.WallName;

        protected override void Awake()
        {
            base.Awake();
            
            _structureTilemap = TilemapController.Instance.GetTilemap(TilemapLayer.Structure);
        }

        public void Init(WallSettings wallSettings, DyeSettings colour)
        {
            RuntimeData = new WallData();
            RuntimeWallData.AssignWallOption(wallSettings, colour);
            
            AssignWallState(EConstructionState.Blueprint);
        }

        public override void LoadData(ConstructionData data)
        {
            RuntimeData = data;
            AssignWallState(data.State);
            
            StructureDatabase.Instance.RegisterStructure(this);
        }

        private void AssignWallState(EConstructionState state)
        {
            RuntimeData.State = state;
            switch (RuntimeData.State)
            {
                case EConstructionState.Blueprint:
                    BlueprintState_Enter();
                    break;
                case EConstructionState.Built:
                    BuiltState_Enter();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public void ChangeWallState(EConstructionState newState)
        {
            if (RuntimeData.State != newState)
            {
                switch (RuntimeData.State)
                {
                    case EConstructionState.Blueprint:
                        BlueprintState_Exit();
                        break;
                    case EConstructionState.Built:
                        BuiltState_Exit();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                AssignWallState(newState);
            }
        }

        public override void CreateConstructTask(bool autoAssign = true)
        {
            Task constuctTask = new Task("Build Construction", ETaskType.Construction, this, EToolType.BuildersHammer);
            constuctTask.Enqueue();
        }
        
        public override void CreateDeconstructionTask(bool autoAssign = true, Action onDeconstructed = null)
        {
            if (RuntimeData.State == EConstructionState.Built)
            {
                _onDeconstructed = onDeconstructed;
                Task constructTask = new Task("Deconstruct Construction", ETaskType.Construction, this, EToolType.BuildersHammer);
                constructTask.Enqueue();
            }
            else
            {
                CancelConstruction();
                onDeconstructed?.Invoke();
            }
        }

        private void BlueprintState_Enter()
        {
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
            
            bool isInterior = StructureDatabase.Instance.IsInteriorBelow(Cell.CellPos);
            if (isInterior)
            {
                _structureTilemap.SetTile(cell, RuntimeWallData.SelectedWallOption.InteriorRuleTile);
            }
            else
            {
                _structureTilemap.SetTile(cell, RuntimeWallData.SelectedWallOption.ExteriorRuleTile);
            }
        }

        private void ClearTile()
        {
            var cell = _structureTilemap.WorldToCell(transform.position);
            _structureTilemap.SetTile(cell ,null);
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

            ClearTile();
        }

        public override void CancelConstruction()
        {
            base.CancelConstruction();
            ClearTile();
        }

        public override void DeletePiece()
        {
            ClearTile();
            Destroy(gameObject);
        }

        private void CreateConstructionHaulingTasks()
        {
            var resourceCosts = RuntimeData.RemainingMaterialCosts;
            CreateConstuctionHaulingTasksForItems(resourceCosts);
        }
        
        public override void CompleteConstruction()
        {
            base.CompleteConstruction();
            ChangeWallState(EConstructionState.Built);
        }

        public void EnableObstacle(bool isEnabled)
        {
            _obstacle.SetActive(isEnabled);
        }

        public override void DoCopy()
        {
            base.DoCopy();
            HUDController.Instance.ShowBuildStructureDetails(RuntimeWallData.SelectedWallOption);
        }
    }
}
