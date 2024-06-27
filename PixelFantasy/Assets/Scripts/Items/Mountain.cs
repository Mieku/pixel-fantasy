using System;
using System.Collections.Generic;
using Characters;
using Controllers;
using Interfaces;
using Systems.Appearance.Scripts;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Items
{
    [Serializable]
    public class MountainResourceData : BasicResourceData
    {
        public float RemainingWork;
        
        public MountainSettings MountainSettings => Settings as MountainSettings;

        public override void InitData(ResourceSettings settings)
        {
            base.InitData(settings);
            RemainingWork = MountainSettings.WorkToExtract;
        }
        
        public RuleTile GetRuleTile()
        {
            return MountainSettings.RuleTile;
        }

        public List<ItemAmount> GetMineDrop()
        {
            if (MountainSettings.HarvestableItems != null)
            {
                return MountainSettings.HarvestableItems.GetItemDrop();
            }

            return new List<ItemAmount>();
        }
    }
    
    public class Mountain : BasicResource, IClickableTile
    {
        [SerializeField] private GameObject _tempPlacementDisp;
        [SerializeField] private Color _selectionTintColour;
        [SerializeField] private RuleTile _dirtRuleTile;

        private Tilemap _mountainTM => TilemapController.Instance.GetTilemap(TilemapLayer.Mountain);
        private Tilemap _dirtTM => TilemapController.Instance.GetTilemap(TilemapLayer.Dirt);

        public MountainResourceData RuntimeMountainData => RuntimeData as MountainResourceData;
        public override string DisplayName => MountainSettings.ResourceName;

        protected override void Awake()
        {
            base.Awake();
            _tempPlacementDisp.SetActive(false);
        }

        public MountainSettings MountainSettings { get; private set; }

        public void InitializeMountain(MountainSettings settings, Vector3 position)
        {
            transform.position = position;
            MountainSettings = settings;
            
            RuntimeData = new MountainResourceData();
            RuntimeData.InitData(MountainSettings);
            RuntimeData.Position = transform.position;
            
            
            
            SetTile();

            _tempPlacementDisp.SetActive(false);
        }

        private void SetTile()
        {
            var cell = _mountainTM.WorldToCell(transform.position);
            _mountainTM.SetTile(cell, MountainSettings.RuleTile);

            var dirtCell = _dirtTM.WorldToCell(transform.position);
            _dirtTM.SetTile(dirtCell, _dirtRuleTile);
        }

        // public void InitializeRuntimeDataIfNeeded()
        // {
        //     if (RuntimeData == null)
        //     {
        //         RuntimeData = new MountainResourceData();
        //         RuntimeData.InitData(MountainSettings);
        //         RuntimeData.Position = transform.position;
        //         //
        //         //
        //         // var data = MountainSettings.CreateInitialDataObject();
        //         // RuntimeData = (ResourceData)DataLibrary.CloneDataObjectToRuntime(data, gameObject);
        //         // RuntimeData.InitData(MountainSettings);
        //         // RuntimeData.Position = transform.position;
        //     }
        // }

        public void TintTile()
        {
            var cell = _mountainTM.WorldToCell(transform.position);
            _mountainTM.SetColor(cell, _selectionTintColour);
        }

        public void UnTintTile()
        {
            if (_mountainTM != null)
            {
                var cell = _mountainTM.WorldToCell(transform.position);
                _mountainTM.SetColor(cell, Color.white);
            }
        }

        public void MineMountain()
        {
            var mountainCell = _mountainTM.WorldToCell(transform.position);
            _mountainTM.SetTile(mountainCell, null);

            var minedDrops = RuntimeMountainData.GetMineDrop();
            foreach (var minedDrop in minedDrops)
            {
                for (int i = 0; i < minedDrop.Quantity; i++)
                {
                    spawner.SpawnItem(minedDrop.Item, transform.position, true);
                }
            }

            RefreshSelection();
        }

        public override float GetCurrentHealth()
        {
            if (RuntimeData != null)
            {
                return RuntimeData.Health;
            }
            else
            {
                return MountainSettings.MaxHealth;
            }
        }

        public override float GetMaxHealth()
        {
            return MountainSettings.MaxHealth;
        }

        public override HarvestableItems GetHarvestableItems()
        {
            return MountainSettings.HarvestableItems;
        }

        public override UnitAction GetExtractActionAnim()
        {
            return UnitAction.Swinging;
        }

        protected override void ExtractResource(StatsData stats)
        {
            MineMountain();
            base.ExtractResource(stats);
        }

        public override bool DoExtractionWork(StatsData stats)
        {
            var workAmount = stats.GetActionSpeedForSkill(MountainSettings.ExtractionSkillType, transform);
            RuntimeData.Health -= workAmount;
            if (RuntimeData.Health <= 0)
            {
                ExtractResource(stats);
                return true;
            }
            
            return false;
        }
    }
}
