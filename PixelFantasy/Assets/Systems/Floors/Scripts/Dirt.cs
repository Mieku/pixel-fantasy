using System;
using Controllers;
using Items;
using Managers;
using TaskSystem;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Systems.Floors.Scripts
{
    public class Dirt : Construction
    {
        private Tilemap _dirtTilemap;
        private EDirtState _dirtState;

        [SerializeField] private TileBase _dirtRuleTile;
        public override string DisplayName => "Dirt";

        public enum EDirtState
        {
            
            Blueprint,
            Built,
        }

        private void AssignDirtState(EDirtState state)
        {
            _dirtState = state;
            switch (_dirtState)
            {
                case EDirtState.Blueprint:
                    BlueprintState_Enter();
                    break;
                case EDirtState.Built:
                    BuiltState_Enter();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void ChangeDirtState(EDirtState newState)
        {
            if (_dirtState != newState)
            {
                switch (_dirtState)
                {
                    case EDirtState.Blueprint:
                        BlueprintState_Exit();
                        break;
                    case EDirtState.Built:
                        BuiltState_Exit();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                AssignDirtState(newState);
            }
        }
        
        protected override void Awake()
        {
            base.Awake();

            _dirtTilemap = TilemapController.Instance.GetTilemap(TilemapLayer.Dirt);
        }

        public void Init()
        {
            AssignDirtState(EDirtState.Blueprint);
        }

        private void BlueprintState_Enter()
        {
            SetTile();
            ColourTile(Librarian.Instance.GetColour("Blueprint"));
            CreateConstructTask();
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
            ClearTile();
        }

        private void SetTile()
        {
            var cell = _dirtTilemap.WorldToCell(transform.position);
            _dirtTilemap.SetTile(cell, _dirtRuleTile);
        }

        private void ClearTile()
        {
            var cell = _dirtTilemap.WorldToCell(transform.position);
            _dirtTilemap.SetTile(cell, null);
        }

        private void ColourTile(Color colour)
        {
            var cell = _dirtTilemap.WorldToCell(transform.position);
            _dirtTilemap.SetColor(cell, colour);
        }

        public override void CreateConstructTask(bool autoAssign = true)
        {
            Task task = new Task("Clear Grass", ETaskType.Farming, this, EToolType.None);
            task.Enqueue();
        }

        // public override void CancelConstruction()
        // {
        //     if (!_isBuilt)
        //     {
        //         CancelTasks();
        //         
        //         // Spawn All the resources used
        //         SpawnUsedResources(100f);
        //
        //         // Delete this blueprint
        //         Destroy(gameObject);
        //     }
        // }
        
        public override void CompleteConstruction()
        {
            base.CompleteConstruction();
            IsClickDisabled = true;
            ChangeDirtState(EDirtState.Built);
        }
    }
}
