using Characters;
using Controllers;
using DataPersistence;
using Handlers;
using Interfaces;
using Systems.Appearance.Scripts;
using UnityEngine;

namespace Items
{
    public class Mountain : BasicResource, IClickableTile
    {
        [SerializeField] private GameObject _tempPlacementDisp;
        [SerializeField] private Color _selectionTintColour;
        [SerializeField] private RuleTile _dirtRuleTile;

        public MountainResourceData RuntimeMountainData => RuntimeData as MountainResourceData;
        public override string DisplayName => MountainSettings.ResourceName;

        protected override void Awake()
        {
            base.Awake();
            _tempPlacementDisp.SetActive(false);
        }
        
        public override void InitializeResource(ResourceSettings settings)
        {
            _settings = settings;
            RuntimeData = new MountainResourceData();
            RuntimeData.InitData(MountainSettings);
            RuntimeData.Position = transform.position;
            ResourcesDatabase.Instance.RegisterResource(this);
            
            SetTile();

            _tempPlacementDisp.SetActive(false);
        }

        public override void LoadResource(BasicResourceData data)
        {
            RuntimeData = data;
            _settings = data.Settings;
            ResourcesDatabase.Instance.RegisterResource(this);
            SetTile();

            _tempPlacementDisp.SetActive(false);
            
            RefreshTaskIcon();
        }

        public MountainSettings MountainSettings => _settings as MountainSettings;

        private void SetTile()
        {
            TilemapController.Instance.SetTile(TilemapLayer.Mountain, transform.position, MountainSettings.RuleTile, false);
            TilemapController.Instance.SetTile(TilemapLayer.Dirt, transform.position, _dirtRuleTile, false);
        }

        public void TintTile()
        {
            TilemapController.Instance.ColourTile(TilemapLayer.Mountain, transform.position, _selectionTintColour);
        }

        public void UnTintTile()
        {
            TilemapController.Instance.ColourTile(TilemapLayer.Mountain, transform.position, Color.white);
        }

        public void MineMountain()
        {
            TilemapController.Instance.SetTile(TilemapLayer.Mountain, transform.position, null);
            
            var minedDrops = RuntimeMountainData.GetMineDrop();
            foreach (var minedDrop in minedDrops)
            {
                for (int i = 0; i < minedDrop.Quantity; i++)
                {
                    var data = minedDrop.Item.CreateItemData();
                    ItemsDatabase.Instance.CreateItemObject(data, transform.position, true);
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

        public override bool DoExtractionWork(StatsData stats, out float progress)
        {
            var workAmount = stats.GetActionSpeedForSkill(MountainSettings.ExtractionSkillType, transform);
            RuntimeData.Health -= workAmount;
            
            if (RuntimeData.Health <= 0)
            {
                ExtractResource(stats);
                progress = 1;
                return true;
            }
            else
            {
                progress = 1f - (RuntimeData.Health / RuntimeData.MaxHealth);
                return false;
            }
        }
    }
}
