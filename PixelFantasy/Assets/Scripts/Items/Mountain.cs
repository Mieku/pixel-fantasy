using Controllers;
using Data.Resource;
using Interfaces;
using Systems.Stats.Scripts;
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
        
        public MountainResourceData RuntimeMountainData => RuntimeData as MountainResourceData;
        
        protected override void Awake()
        {
            base.Awake();
            
            _tempPlacementDisp.SetActive(false);
        }
        
        public override void InitializeResource(ResourceSettings settings)
        {
            var data = settings.CreateInitialDataObject();

            DataLibrary.RegisterInitializationCallback(() =>
            {
                RuntimeData = (ResourceData)DataLibrary.CloneDataObjectToRuntime(data, gameObject);
                RuntimeData.InitData(settings);
                RuntimeData.Position = transform.position;
                
                UpdateSprite();
                
                DataLibrary.OnSaved += Saved;
                DataLibrary.OnLoaded += Loaded;
                
                Refresh();
            });
            
            _tempPlacementDisp.SetActive(false);
        }

        protected override void UpdateSprite()
        {
            // No sprite in need of updating
        }

        private void Refresh()
        {
            SetTile();
        }
        
        public override UnitAction GetExtractActionAnim()
        {
            return UnitAction.Swinging;
        }

        protected override void ExtractResource(KinlingStats stats)
        {
            MineMountain();
            base.ExtractResource(stats);
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
    }
}
