using System;
using System.Collections.Generic;
using AI;
using Controllers;
using Items;
using Managers;
using Systems.Buildings.Scripts;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Systems.Floors.Scripts
{
    public class Floor : Construction
    {
        private Tilemap _floorTilemap;
        
        public FloorData RuntimeFloorData => RuntimeData as FloorData;
        public override string DisplayName => RuntimeFloorData.FloorSettings.FloorName;
        public override List<SpriteRenderer> SpritesToOutline => new List<SpriteRenderer> {  };

        public override bool IsSimilar(PlayerInteractable otherPI)
        {
            if (otherPI is Floor floor)
            {
                return RuntimeFloorData.FloorSettings == floor.RuntimeFloorData.FloorSettings;
            }

            return false;
        }

        protected override void Awake()
        {
            base.Awake();

            _floorTilemap = TilemapController.Instance.GetTilemap(TilemapLayer.Flooring);
        }
        
        public void Init(FloorSettings settings, FloorStyle style)
        {
            RuntimeData = new FloorData();
            RuntimeFloorData.AssignFloorSettings(settings, style);
            RuntimeFloorData.Position = transform.position;
            
            FlooringDatabase.Instance.RegisterFloor(this);
            
            AssignFloorState(EConstructionState.Blueprint);

            IsAllowed = true;
            RefreshAllowedDisplay();
            RefreshAllowCommands();
        }
        
        public override void LoadData(ConstructionData data)
        {
            RuntimeData = data;
            FlooringDatabase.Instance.RegisterFloor(this);
            RefreshTaskIcon();
            RefreshAllowedDisplay();
            RefreshAllowCommands();
        }

        private void AssignFloorState(EConstructionState state)
        {
            RuntimeFloorData.State = state;
            switch (RuntimeFloorData.State)
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

        private void BlueprintState_Enter()
        {
            SetTile();
            ColourTile(Librarian.Instance.GetColour("Blueprint"));
            CreateConstructionHaulingTasks();
            
            RefreshAllowedDisplay();
            RefreshAllowCommands();
        }

        private void BuiltState_Enter()
        {
            ColourTile(Color.white);
            
            RefreshAllowedDisplay();
            RefreshAllowCommands();
        }

        private void SetTile()
        {
            var cell = _floorTilemap.WorldToCell(transform.position);
            _floorTilemap.SetTile(cell, RuntimeFloorData.FloorStyleTileBase);
        }

        private void ClearTile()
        {
            var cell = _floorTilemap.WorldToCell(transform.position);
            _floorTilemap.SetTile(cell, null);
        }

        private void ColourTile(Color colour)
        {
            var cell = _floorTilemap.WorldToCell(transform.position);
            _floorTilemap.SetColor(cell, colour);
        }
        
        private void CreateConstructionHaulingTasks()
        {
            var resourceCosts = RuntimeFloorData.FloorSettings.CraftRequirements.GetMaterialCosts();
            CreateConstuctionHaulingTasksForItems(resourceCosts);
        }
        
        public override void CompleteConstruction()
        {
            base.CompleteConstruction();
            AssignFloorState(EConstructionState.Built);
            
            RefreshAllowedDisplay();
            RefreshAllowCommands();
        }
        
        public override void CompleteDeconstruction()
        {
            base.CompleteDeconstruction();

            ClearTile();
            
            FlooringDatabase.Instance.DeregisterFloor(this);
        }

        public override void CancelConstruction()
        {
            base.CancelConstruction();
            ClearTile();
            FlooringDatabase.Instance.DeregisterFloor(this);
        }

        public override void DoCopy()
        {
            base.DoCopy();
            HUDController.Instance.ShowBuildStructureDetails(RuntimeFloorData.FloorSettings);
        }

        public void DeleteFloor()
        {
            ClearTile();
            Destroy(gameObject);
        }
    }
}
