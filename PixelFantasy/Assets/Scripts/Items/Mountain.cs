using System.Collections.Generic;
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

        private Tilemap _mountainTM => TilemapController.Instance.GetTilemap(TilemapLayer.Mountain);
        private Tilemap _dirtTM => TilemapController.Instance.GetTilemap(TilemapLayer.Dirt);
        
        protected float _remainingWork;

        private MountainSettings _mountainSettings => ResourceSettings as MountainSettings;
        
        protected override void Awake()
        {
            base.Awake();
            
            _tempPlacementDisp.SetActive(false);
        }

        public void Init(MountainSettings mountainSettings)
        {
            _tempPlacementDisp.SetActive(false);
            ResourceSettings = mountainSettings;
            
            Refresh();
        }

        private void Start()
        {
            if (_mountainSettings == null) return;
            
            Refresh();
        }

        private void Refresh()
        {
            Health = GetWorkAmount();
            SetTile();
            _remainingWork = GetWorkAmount();
        }
        
        public override UnitAction GetExtractActionAnim()
        {
            return UnitAction.Swinging;
        }

        protected override void DestroyResource()
        {
            MineMountain();
            base.DestroyResource();
        }

        private void SetTile()
        {
            var cell = _mountainTM.WorldToCell(transform.position);
            _mountainTM.SetTile(cell, _mountainSettings.GetRuleTile());
            

            var dirtCell = _dirtTM.WorldToCell(transform.position);
            _dirtTM.SetTile(dirtCell, _dirtRuleTile);
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
            var minedDrops = _mountainSettings.GetMineDrop();
            foreach (var minedDrop in minedDrops)
            {
                for (int i = 0; i < minedDrop.Quantity; i++)
                {
                    spawner.SpawnItem(minedDrop.Item, transform.position, true);
                }
            }

            // Delete Self
            RefreshSelection();
        }

        public override float GetWorkAmount()
        {
            return _mountainSettings.GetWorkAmount();
        }
        
        public float WorkDone(float workAmount)
        {
            _remainingWork -= workAmount;
            return _remainingWork;
        }

        public override HarvestableItems GetHarvestableItems()
        {
            return _mountainSettings.HarvestableItems;
        }

        public override object CaptureState()
        {
            return new State
            {
                UID = UniqueId,
                Position = transform.position,
                MountainSettings = _mountainSettings,
                RemainingWork = _remainingWork,
            };
        }

        public override void RestoreState(object data)
        {
            var stateData = (State)data;
            UniqueId = stateData.UID;
            transform.position = stateData.Position;
            ResourceSettings = stateData.MountainSettings;
            _remainingWork = stateData.RemainingWork;

            Refresh();
        }
        
        public struct State
        {
            public string UID;
            public Vector3 Position;
            public MountainSettings MountainSettings;
            public float RemainingWork;
        }
    }
}
