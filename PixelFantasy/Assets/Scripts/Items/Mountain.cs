using System.Collections.Generic;
using Controllers;
using Data.Resource;
using Interfaces;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Items
{
    public class Mountain : BasicResource, IClickableTile
    {
        [SerializeField] private GameObject _tempPlacementDisp;
        [SerializeField] private Color _selectionTintColour;
        [SerializeField] private RuleTile _dirtRuleTile;

        private Tilemap _mountainTM => TilemapController.Instance.GetTilemap(TilemapLayer.Mountain);
        private Tilemap _dirtTM => TilemapController.Instance.GetTilemap(TilemapLayer.Dirt);
        
        //protected float _remainingWork;

        //private MountainSettings _mountainSettings => ResourceSettings as MountainSettings;
        
        public MountainResourceData RuntimeMountainData => RuntimeData as MountainResourceData;
        private MountainResourceData MountainData => Data as MountainResourceData;
        
        protected override void Awake()
        {
            base.Awake();
            
            _tempPlacementDisp.SetActive(false);
        }
        
        public override void Init(ResourceData data)
        {
            base.Init(data);
            _tempPlacementDisp.SetActive(false);
        }

        protected override void InitialDataReady()
        {
            base.InitialDataReady();
            
            Refresh();
        }

        // public void Init(MountainSettings mountainSettings)
        // {
        //     _tempPlacementDisp.SetActive(false);
        //     ResourceSettings = mountainSettings;
        //     
        //     Refresh();
        // }

        // private void Start()
        // {
        //     if (_mountainSettings == null) return;
        //     
        //     Refresh();
        // }

        private void Refresh()
        {
            SetTile();
        }
        
        public override UnitAction GetExtractActionAnim()
        {
            return UnitAction.Swinging;
        }

        protected override void HarvestResource()
        {
            MineMountain();
            base.HarvestResource();
        }

        private void SetTile()
        {
            var cell = _mountainTM.WorldToCell(transform.position);
            _mountainTM.SetTile(cell, RuntimeMountainData.GetRuleTile());

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
            var minedDrops = RuntimeMountainData.GetMineDrop();
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

        // public override float GetWorkAmount()
        // {
        //     return _mountainSettings.GetWorkAmount();
        // }
        //
        // public float WorkDone(float workAmount)
        // {
        //     _remainingWork -= workAmount;
        //     return _remainingWork;
        // }

        // public override HarvestableItems GetHarvestableItems()
        // {
        //     return RuntimeMountainData.HarvestableItems;
        // }
    }
}
