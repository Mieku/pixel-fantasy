using System;
using System.Collections.Generic;
using Controllers;
using Managers;
using ScriptableObjects;
using TaskSystem;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Items
{
    public class Roof : Construction
    {
        [SerializeField] private GameObject _shadeHandle;
        
        private Tilemap _roofTM;
        private RoofData _roofData;
        private State _state;

        public void Init(RoofData roofData)
        {
            _roofTM = TilemapController.Instance.GetTilemap(TilemapLayer.Roof);
            _roofData = roofData;
            SetTile();
            SetState(State.NotBuilt);
            
            PrepForConstruction();
        }

        private void SetState(State state)
        {
            _state = state;
            switch (state)
            {
                case State.NotBuilt:
                    SetNotBuilt();
                    break;
                case State.Built:
                    SetBuilt();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private void SetNotBuilt()
        {
            _shadeHandle.SetActive(false);
            ColourTile(Librarian.Instance.GetColour("Blueprint"));
        }

        private void SetBuilt()
        {
            _isBuilt = true;
            _shadeHandle.SetActive(true);
            ColourTile(Color.white);
        }
        
        private void PrepForConstruction()
        {
            _isBuilt = false;
            _remainingResourceCosts = new List<ItemAmount> (_roofData.GetResourceCosts());
            _pendingResourceCosts = new List<ItemAmount>();
            
            CreateConstructionHaulingTasks();
        }
        
        private void CreateConstructionHaulingTasks()
        {
            var resourceCosts = _roofData.GetResourceCosts();
            CreateConstuctionHaulingTasksForItems(resourceCosts);
        }
        
        protected override void EnqueueCreateTakeResourceToBlueprintTask(ItemData resourceData)
        {
            Task task = new Task
            {
                TaskId = "Withdraw Item",
                Requestor = this,
                Payload = resourceData.ItemName,
                TaskType = TaskType.Haul,
            };
            TaskManager.Instance.AddTask(task);
        }
        
        public override void CreateConstructTask(bool autoAssign = true)
        {
            Task constuctTask = new Task()
            {
                TaskId = "Build Construction",
                Requestor = this,
                TaskType = TaskType.Construction,
            };
            constuctTask.Enqueue();
        }
    
        public override void CompleteConstruction()
        {
            base.CompleteConstruction();
            SetState(State.Built);
        }

        private void SetTile()
        {
            var cell = _roofTM.WorldToCell(GetCellPos());
            _roofTM.SetTile(cell, _roofData.RuleTile);
        }

        private void RemoveTile()
        {
            var cell = _roofTM.WorldToCell(GetCellPos());
            _roofTM.SetTile(cell, null);
        }

        private Vector2 GetCellPos()
        {
            // The cell is actually offset
            return new Vector2(transform.position.x, transform.position.y + 1f);
        }

        private void ColourTile(Color colour)
        {
            var cell = _roofTM.WorldToCell(GetCellPos());
            _roofTM.SetColor(cell, colour);
        }

        private enum State
        {
            NotBuilt,
            Built,
        }
    }
}
