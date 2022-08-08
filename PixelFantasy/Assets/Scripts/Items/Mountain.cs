using System;
using System.Collections.Generic;
using Actions;
using Controllers;
using DataPersistence;
using Gods;
using Interfaces;
using Pathfinding;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

namespace Items
{
    public class Mountain : Interactable, IClickableTile, IPersistent
    {
        [SerializeField] private GameObject _tempPlacementDisp;
        [SerializeField] private MountainData _mountainData;
        [SerializeField] private Color _selectionTintColour;
        [SerializeField] private RuleTile _dirtRuleTile;

        private Tilemap _mountainTM;
        private Tilemap _dirtTM;
        
        protected Spawner spawner => Spawner.Instance;
        
        private void Awake()
        {
            _tempPlacementDisp.SetActive(false);
            _mountainTM =
                TilemapController.Instance.GetTilemap(TilemapLayer.Mountain);
            _dirtTM =
                TilemapController.Instance.GetTilemap(TilemapLayer.Dirt);
        }

        private void Start()
        {
            SetTile();
        }

        private void SetTile()
        {
            var cell = _mountainTM.WorldToCell(transform.position);
            _mountainTM.SetTile(cell, _mountainData.GetRuleTile());
        }

        public SelectionData GetSelectionData()
        {
            var actions = GetActions();
            var cancellableActions = GetCancellableActions();
        
            SelectionData result = new SelectionData
            {
                ItemName = _mountainData.ResourceName,
                Actions = actions,
                CancellableActions = cancellableActions,
                ClickObject = GetClickObject(),
                Requestor = GetComponent<Interactable>(),
            };

            return result;
        }

        public ClickObject GetClickObject()
        {
            var clickObj = gameObject.GetComponent<ClickObject>();
            return clickObj;
        }

        public bool IsClickDisabled { get; set; }
        public bool IsAllowed { get; set; }
        public void ToggleAllowed(bool isAllowed)
        {
            
        }

        public MountainData GetMountainData()
        {
            return _mountainData;
        }

        public List<ActionBase> GetActions()
        {
            return AvailableActions;
        }

        public List<ActionBase> GetCancellableActions()
        {
            return CancellableActions();
        }

        public void AssignOrder(ActionBase orderToAssign)
        {
            CreateTask(orderToAssign);
        }

        public void TintTile()
        {
            var cell = _mountainTM.WorldToCell(transform.position);
            _mountainTM.SetColor(cell, _selectionTintColour);
        }

        public void UnTintTile()
        {
            var cell = _mountainTM.WorldToCell(transform.position);
            _mountainTM.SetColor(cell, Color.white);
        }
        
        public void RefreshSelection()
        {
            if (GetClickObject().IsSelected)
            {
                GameEvents.Trigger_RefreshSelection();
            }
        }

        public void MineMountain()
        {
            // Clear Mountain Tile
            var mountainCell = _mountainTM.WorldToCell(transform.position);
            _mountainTM.SetTile(mountainCell, null);
            
            // Put dirt where Mountain Tile was
            var dirtCell = _dirtTM.WorldToCell(transform.position);
            _dirtTM.SetTile(dirtCell, _dirtRuleTile);
            
            // Spawn Resources
            var minedDrops = _mountainData.GetMineDrop();
            foreach (var minedDrop in minedDrops)
            {
                for (int i = 0; i < minedDrop.Quantity; i++)
                {
                    spawner.SpawnItem(minedDrop.Item, transform.position, true);
                }
            }

            // Delete Self
            RefreshSelection();
            
            Destroy(gameObject);
        }

        public override int GetWorkAmount()
        {
            return _mountainData.GetWorkAmount();
        }

        public object CaptureState()
        {
            return new State
            {
                UID = UniqueId,
                Position = transform.position,
                MountainData = _mountainData,
                PendingTasks = PendingTasks,
            };
        }

        public void RestoreState(object data)
        {
            var stateData = (State)data;
            UniqueId = stateData.UID;
            transform.position = stateData.Position;
            _mountainData = stateData.MountainData;

            RestoreTasks(stateData.PendingTasks);
        }
        
        public struct State
        {
            public string UID;
            public Vector3 Position;
            public MountainData MountainData;
            public List<ActionBase> PendingTasks;
        }
    }
}
