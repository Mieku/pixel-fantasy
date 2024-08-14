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
        
        //private Tilemap _structureTilemap;
        public override string DisplayName => RuntimeWallData.SelectedWallOption.WallName;

        protected override void Awake()
        {
            base.Awake();
            
            //_structureTilemap = TilemapController.Instance.GetTilemap(TilemapLayer.Structure);
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
            LoadWallState(data.State);
            
            StructureDatabase.Instance.RegisterStructure(this);
            RefreshTaskIcon();
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

        private void LoadWallState(EConstructionState state)
        {
            switch (state)
            {
                case EConstructionState.Blueprint:
                    EnableObstacle(false);
                    break;
                case EConstructionState.Built:
                    EnableObstacle(true);
                    break;
                case EConstructionState.Planning:
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
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

        private void BuiltState_Enter()
        {
            ColourTile(Color.white);
            EnableObstacle(true);
        }
 
        public override void RefreshTile()
        {
            //var cell = _structureTilemap.WorldToCell(transform.position);
            
            bool isInterior = StructureDatabase.Instance.IsInteriorBelow(Cell.CellPos);
            if (isInterior)
            {
                TilemapController.Instance.SetTile(TilemapLayer.Structure ,transform.position, RuntimeWallData.SelectedWallOption.InteriorRuleTile);
                //_structureTilemap.SetTile(cell, RuntimeWallData.SelectedWallOption.InteriorRuleTile);
            }
            else
            {
                TilemapController.Instance.SetTile(TilemapLayer.Structure ,transform.position, RuntimeWallData.SelectedWallOption.ExteriorRuleTile);
                //_structureTilemap.SetTile(cell, RuntimeWallData.SelectedWallOption.ExteriorRuleTile);
            }
        }

        private void ClearTile()
        {
            TilemapController.Instance.SetTile(TilemapLayer.Structure ,transform.position, null);
            //
            // var cell = _structureTilemap.WorldToCell(transform.position);
            // _structureTilemap.SetTile(cell ,null);
        }

        private void ColourTile(Color colour)
        {
            TilemapController.Instance.ColourTile(TilemapLayer.Structure, transform.position, colour);
            //
            // var cell = _structureTilemap.WorldToCell(transform.position);
            // _structureTilemap.SetColor(cell, colour);
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
            AssignWallState(EConstructionState.Built);
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
