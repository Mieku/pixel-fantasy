using System;
using Controllers;
using Data.Structure;
using Items;
using Managers;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Systems.Floors.Scripts
{
    public class Floor : Construction
    {
        private Tilemap _floorTilemap;
        
        public FloorData RuntimeFloorData => RuntimeData as FloorData;
        
        protected override void Awake()
        {
            base.Awake();

            _floorTilemap = TilemapController.Instance.GetTilemap(TilemapLayer.Flooring);
        }
        
        public void Init(FloorSettings settings, FloorStyle style)
        {
            DataLibrary.RegisterInitializationCallback(() =>
            {
                RuntimeData = (FloorData) DataLibrary.CloneDataObjectToRuntime(Data, gameObject);
                RuntimeFloorData.AssignFloorSettings(settings, style);
                RuntimeFloorData.Position = transform.position;
                RuntimeFloorData.title = settings.title;
                
                DataLibrary.OnSaved += Saved;
                DataLibrary.OnLoaded += Loaded;
            });
            
            AssignFloorState(EConstructionState.Blueprint);
        }
        
        protected void Saved()
        {
            
        }

        protected void Loaded()
        {
            
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

        public void ChangeFloorState(EConstructionState newState)
        {
            if (RuntimeFloorData.State != newState)
            {
                switch (RuntimeFloorData.State)
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

                AssignFloorState(newState);
            }
        }

        private void BlueprintState_Enter()
        {
            SetTile();
            ColourTile(Librarian.Instance.GetColour("Blueprint"));
            CreateConstructionHaulingTasks();
        }
        
        private void BlueprintState_Exit()
        {
            
        }

        private void BuiltState_Enter()
        {
            ColourTile(Color.white);
        }
        
        private void BuiltState_Exit()
        {
            
        }

        private void SetTile()
        {
            var cell = _floorTilemap.WorldToCell(transform.position);
            _floorTilemap.SetTile(cell, RuntimeFloorData.FloorStyle.Tiles);
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
            IsClickDisabled = true;
            ChangeFloorState(EConstructionState.Built);
        }
        
        public override void CompleteDeconstruction()
        {
            base.CompleteDeconstruction();

            ClearTile();
        }
    }
}
