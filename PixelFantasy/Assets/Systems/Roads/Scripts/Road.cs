using System;
using Controllers;
using Items;
using Managers;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Systems.Roads.Scripts
{
    public class Road : Construction
    {
        private Tilemap _roadTilemap;
        private RoadOption _roadOption;
        private ERoadState _roadState;

        public enum ERoadState
        {
            
            Blueprint,
            Built,
        }

        private void AssignRoadState(ERoadState state)
        {
            _roadState = state;
            switch (_roadState)
            {
                case ERoadState.Blueprint:
                    BlueprintState_Enter();
                    break;
                case ERoadState.Built:
                    BuiltState_Enter();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void ChangeRoadState(ERoadState newState)
        {
            if (_roadState != newState)
            {
                switch (_roadState)
                {
                    case ERoadState.Blueprint:
                        BlueprintState_Exit();
                        break;
                    case ERoadState.Built:
                        BuiltState_Exit();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                AssignRoadState(newState);
            }
        }
        
        protected override void Awake()
        {
            base.Awake();

            _roadTilemap = TilemapController.Instance.GetTilemap(TilemapLayer.Flooring);
        }

        public void Init(RoadOption roadOption)
        {
            _roadOption = roadOption;
            
            AssignRoadState(ERoadState.Blueprint);
        }

        private void BlueprintState_Enter()
        {
            _remainingResourceCosts = _roadOption.OptionResourceCosts;
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
            ClearTile();
        }

        private void SetTile()
        {
            var cell = _roadTilemap.WorldToCell(transform.position);
            _roadTilemap.SetTile(cell, _roadOption.RoadRuleTile);
        }

        private void ClearTile()
        {
            var cell = _roadTilemap.WorldToCell(transform.position);
            _roadTilemap.SetTile(cell, null);
        }

        private void ColourTile(Color colour)
        {
            var cell = _roadTilemap.WorldToCell(transform.position);
            _roadTilemap.SetColor(cell, colour);
        }
        
        private void CreateConstructionHaulingTasks()
        {
            var resourceCosts = _roadOption.OptionResourceCosts;
            CreateConstuctionHaulingTasksForItems(resourceCosts);
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

        private void CancelTasks()
        {
            // Drop all incoming resources
            foreach (var incomingItem in _incomingItems)
            {
                incomingItem.LinkedItem.SeekForSlot();
            }
            _pendingResourceCosts.Clear();
            _incomingItems.Clear();
        }
        
        public override void CompleteConstruction()
        {
            base.CompleteConstruction();
            _isBuilt = true;
            IsClickDisabled = true;
            ChangeRoadState(ERoadState.Built);
        }
    }
}
