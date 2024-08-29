using Characters;
using Controllers;
using DataPersistence;
using Handlers;
using Systems.Appearance.Scripts;
using UnityEngine;

namespace Items
{
    public class Mountain : BasicResource
    {
        [SerializeField] private GameObject _tempPlacementDisp;
        [SerializeField] private Color _selectionTintColour;
        [SerializeField] private RuleTile _dirtRuleTile;

        public MountainResourceData RuntimeMountainData => RuntimeData as MountainResourceData;

        public override string DisplayName
        {
            get
            {
                if (IsUnknown())
                {
                    return "Unknown Mountain";
                }
                else
                {
                    return MountainSettings.ResourceName;
                }
            }
        }

        protected void Awake()
        {
            _tempPlacementDisp.SetActive(false);
        }

        /// <summary>
        /// The player does not know the mountain type if it is fully surrounded
        /// </summary>
        public bool IsUnknown()
        {
            Vector2 topPos = RuntimeData.Position + Vector2.up;
            Vector2 rightPos = RuntimeData.Position + Vector2.right;
            Vector2 bottomPos = RuntimeData.Position + Vector2.down;
            Vector2 leftPos = RuntimeData.Position + Vector2.left;
            
            Vector2 topRightPos = RuntimeData.Position + Vector2.up + Vector2.right;
            Vector2 topLeftPos = RuntimeData.Position + Vector2.up + Vector2.left;
            Vector2 bottomRightPos = RuntimeData.Position + Vector2.down + Vector2.right;
            Vector2 bottomLeftPos = RuntimeData.Position + Vector2.down + Vector2.left;

            if (Helper.GetObjectAtPosition<Mountain>(topPos) == null) return false;
            if (Helper.GetObjectAtPosition<Mountain>(rightPos) == null) return false;
            if (Helper.GetObjectAtPosition<Mountain>(bottomPos) == null) return false;
            if (Helper.GetObjectAtPosition<Mountain>(leftPos) == null) return false;
            
            if (Helper.GetObjectAtPosition<Mountain>(topRightPos) == null) return false;
            if (Helper.GetObjectAtPosition<Mountain>(topLeftPos) == null) return false;
            if (Helper.GetObjectAtPosition<Mountain>(bottomRightPos) == null) return false;
            if (Helper.GetObjectAtPosition<Mountain>(bottomLeftPos) == null) return false;
            
            return true;
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

        protected override void OnSelection()
        {
            base.OnSelection();
            TintTile();
        }

        protected override void OnDeselection()
        {
            base.OnDeselection();
            UnTintTile();
        }

        public void TintTile()
        {
            TilemapController.Instance.ColourTile(TilemapLayer.Mountain, transform.position, _selectionTintColour);
        }

        public void UnTintTile()
        {
            TilemapController.Instance.ColourTile(TilemapLayer.Mountain, transform.position, Color.white);
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
            base.ExtractResource(stats);
            TilemapController.Instance.SetTile(TilemapLayer.Mountain, transform.position, null);
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
                progress = RuntimeMountainData.HealthPercent;
                return false;
            }
        }

        public override bool IsSimilar(PlayerInteractable otherPI)
        {
            if (otherPI is Mountain mountain)
            {
                if (IsUnknown())
                {
                    if (mountain.IsUnknown())
                    {
                        return true;
                    }
                }
                else
                {
                    if (mountain.IsUnknown()) return false;

                    if (mountain.MountainSettings == MountainSettings)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
