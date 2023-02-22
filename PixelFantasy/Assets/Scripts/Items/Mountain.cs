using System;
using System.Collections.Generic;
using Actions;
using Controllers;
using Interfaces;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Items
{
    public class Mountain : Resource, IClickableTile
    {
        [SerializeField] private GameObject _tempPlacementDisp;
        [SerializeField] private Color _selectionTintColour;
        [SerializeField] private RuleTile _dirtRuleTile;

        private Tilemap _mountainTM;
        private Tilemap _dirtTM;
        
        protected float _remainingWork;

        private MountainData _mountainData => ResourceData as MountainData;
        
        protected override void Awake()
        {
            base.Awake();
            
            _tempPlacementDisp.SetActive(false);
            _mountainTM =
                TilemapController.Instance.GetTilemap(TilemapLayer.Mountain);
            _dirtTM =
                TilemapController.Instance.GetTilemap(TilemapLayer.Dirt);

            Init();
        }

        private void Init()
        {
            if (_mountainData == null) return;
            
            Health = GetWorkAmount();
        }

        private void Start()
        {
            SetTile();
            _remainingWork = GetWorkAmount();
        }
        
        public override UnitAction GetExtractActionAnim()
        {
            return UnitAction.Mining;
        }

        protected override void DestroyResource()
        {
            MineMountain();
        }

        private void SetTile()
        {
            var cell = _mountainTM.WorldToCell(transform.position);
            _mountainTM.SetTile(cell, _mountainData.GetRuleTile());
            

            var dirtCell = _dirtTM.WorldToCell(transform.position);
            _dirtTM.SetTile(dirtCell, _dirtRuleTile);
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

        public MountainData GetMountainData()
        {
            return _mountainData;
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
        
        public void MineMountain()
        {
            // Clear Mountain Tile
            var mountainCell = _mountainTM.WorldToCell(transform.position);
            _mountainTM.SetTile(mountainCell, null);
                        
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
        
        public float WorkDone(float workAmount)
        {
            _remainingWork -= workAmount;
            return _remainingWork;
        }

        public override object CaptureState()
        {
            return new State
            {
                UID = UniqueId,
                Position = transform.position,
                MountainData = _mountainData,
                PendingTasks = PendingTasks,
                RemainingWork = _remainingWork,
            };
        }

        public override void RestoreState(object data)
        {
            var stateData = (State)data;
            UniqueId = stateData.UID;
            transform.position = stateData.Position;
            ResourceData = stateData.MountainData;
            _remainingWork = stateData.RemainingWork;

            Init();

            RestoreTasks(stateData.PendingTasks);
        }
        
        public struct State
        {
            public string UID;
            public Vector3 Position;
            public MountainData MountainData;
            public List<ActionBase> PendingTasks;
            public float RemainingWork;
        }
    }
}
