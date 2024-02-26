using System;
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
            _remainingResourceCosts = _doorSO.ResourceCosts;
            CreateConstructionHaulingTasks();
            OnPlaced();
        }

        private void Update()
        {
            if (_doorState == EDoorState.BeingPlaced)
            {
                FollowCursor();
                CheckPlacement();
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
        }

        public override void CreateConstructTask(bool autoAssign = true)
        {
            Task constuctTask = new Task("Build Construction", this, _doorSO.RequiredJob, EToolType.BuildersHammer, SkillType.Construction);
            constuctTask.Enqueue();
        }
        
        public override void CreateDeconstructionTask(bool autoAssign = true, Action onDeconstructed = null)
        {
            _onDeconstructed = onDeconstructed;
            Task constuctTask = new Task("Deconstruct", this, _doorSO.RequiredJob, EToolType.BuildersHammer, SkillType.Construction);
            constuctTask.Enqueue();
        }

        private void BlueprintState_Enter()
        {
            _remainingResourceCosts = _doorSO.ResourceCosts;
            OnPlaced();
            // ClearTile();
            // ColourTile(Librarian.Instance.GetColour("Blueprint"));
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

        public override void RefreshTile()
        {
            
        }

        public override void CompleteDeconstruction()
        {
            OnDeconstructed();
            base.CompleteDeconstruction();
        }

        private void ClearTile()
        {
            var cell = _wallTilemap.WorldToCell(transform.position);
            _wallTilemap.SetTile(cell, null);
        }

        private void ColourTile(Color colour)
        {
            var cell = _wallTilemap.WorldToCell(transform.position);
            _wallTilemap.SetColor(cell, colour);
        }
        
        private void CreateConstructionHaulingTasks()
        {
            var resourceCosts = _doorSO.ResourceCosts;
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
            return true; // TODO: Make this check walls
        }
    }
}
