using System;
using System.Collections.Generic;
using Characters;
using DataPersistence;
using Handlers;
using Managers;
using QFSW.QC;
using UnityEngine;
using UnityEngine.Serialization;

namespace Items
{
    public class GrowingResource : BasicResource
    {
        [SerializeField] protected SpriteRenderer _fruitOverlay;

        public bool IsFruiting => GrowingResourceData.GrowingResourceSettings.HasFruit;

        public GrowingResourceData GrowingResourceData => RuntimeData as GrowingResourceData;
        
        public override void InitializeResource(ResourceSettings settings)
        {
            _settings = settings;
            RuntimeData = new GrowingResourceData();
            RuntimeData.InitData(settings);
            RuntimeData.Position = transform.position;
            ResourcesDatabase.Instance.RegisterResource(this);
            UpdateSprite();

            GrowthCheck();
            FruitCheck();
        }

        public void LoadResource(GrowingResourceData data)
        {
            RuntimeData = data;
            _settings = data.Settings;
            ResourcesDatabase.Instance.RegisterResource(this);
            UpdateSprite();

            GrowthCheck();
            FruitCheck();
        }

        public override string DisplayName
        {
            get
            {
                if (!GrowingResourceData.FullyGrown)
                {
                    var stageName = GrowingResourceData.GetGrowthStage().StageName;
                    return $"{GrowingResourceData.Settings.ResourceName} ({stageName})";
                }

                return GrowingResourceData.Settings.name;
            }
        }

        protected override void UpdateSprite()
        {
            var stage = GrowingResourceData.GetGrowthStage();
            if (stage != null)
            {
                var scaleOverride = stage.Scale;
                _spriteRenderer.sprite = stage.GrowthSprite;
                _spriteRenderer.gameObject.transform.localScale = new Vector3(scaleOverride, scaleOverride, 1);
            }
            else
            {
                Debug.LogWarning("Growth stage is null. Cannot update sprite.");
            }
        }

        public override HarvestableItems GetHarvestableItems()
        {
            var stage = GrowingResourceData.GetGrowthStage();
            return stage?.HarvestableItems;
        }

        public Sprite GetGrowthIcon()
        {
            return Librarian.Instance.GetSprite("Growth");
        }

        protected void GrowthCheck()
        {
            if (GrowingResourceData == null) return;

            if (!GrowingResourceData.FullyGrown)
            {
                GrowingResourceData.AgeSec += TimeManager.Instance.DeltaTime;
                if (GrowingResourceData.AgeSec >= GrowingResourceData.AgeForNextGrowth)
                {
                    var extractDiff = GrowingResourceData.MaxHealth - GrowingResourceData.Health;

                    GrowingResourceData.GrowthIndex++;

                    GrowingResourceData.Health = GrowingResourceData.MaxHealth - extractDiff;

                    RefreshSelection();
                    UpdateSprite();
                }
            }
        }

        [Command("instant_fruit", MonoTargetType.All)]
        private void CMD_InstantFruit()
        {
            if (!GrowingResourceData.HasFruitAvailable)
            {
                GrowingResourceData.FruitTimer = GrowingResourceData.GrowingResourceSettings.GrowFruitTime;
            }
        }

        public Sprite GetFruitIcon()
        {
            if (!IsFruiting) return null;

            return GrowingResourceData.GetFruitLoot()[0].Item.ItemSprite;
        }

        protected void FruitCheck()
        {
            if (GrowingResourceData == null) return;

            if (!GrowingResourceData.FullyGrown) return;

            if (GrowingResourceData.GrowingResourceSettings.HasFruit && !GrowingResourceData.HasFruitAvailable)
            {
                GrowingResourceData.FruitTimer += TimeManager.Instance.DeltaTime;
                if (GrowingResourceData.FruitTimer >= GrowingResourceData.GrowingResourceSettings.GrowFruitTime)
                {
                    GrowingResourceData.FruitTimer = 0;
                    _fruitOverlay.sprite = GrowingResourceData.GrowingResourceSettings.FruitOverlay;
                    _fruitOverlay.gameObject.SetActive(true);
                    GrowingResourceData.HasFruitAvailable = true;
                    RefreshSelection();
                }
                else if (GrowingResourceData.FruitTimer >= GrowingResourceData.GrowingResourceSettings.GrowFruitTime / 2f)
                {
                    if (!GrowingResourceData.ShowingFlowers && GrowingResourceData.GrowingResourceSettings.HasFruitFlowers)
                    {
                        GrowingResourceData.ShowingFlowers = true;
                        _fruitOverlay.sprite = GrowingResourceData.GrowingResourceSettings.FruitFlowersOverlay;
                        _fruitOverlay.gameObject.SetActive(true);
                    }
                }
            }

            if (GrowingResourceData.HasFruitAvailable)
            {
                _fruitOverlay.sprite = GrowingResourceData.GrowingResourceSettings.FruitOverlay;
                _fruitOverlay.gameObject.SetActive(true);
            }
        }

        public void HarvestFruit(StatsData stats)
        {
            if (GrowingResourceData.HasFruitAvailable)
            {
                _fruitOverlay.gameObject.SetActive(false);
                List<CostSettings> fruits = GrowingResourceData.GetFruitLoot();
                foreach (var fruit in fruits)
                {
                    int amount = stats.DetermineAmountYielded(
                        GrowingResourceData.GrowingResourceSettings.ExtractionSkillType, fruit.Quantity);
                    for (int i = 0; i < amount; i++)
                    {
                        var data = fruit.Item.CreateItemData();
                        ItemsDatabase.Instance.CreateItemObject(data, transform.position, true);
                    }
                }
                GrowingResourceData.HasFruitAvailable = false;
                RefreshSelection();
                RefreshTaskIcon(0);
            }

            GrowingResourceData.RemainingHarvestWork = GrowingResourceData.GrowingResourceSettings.WorkToHarvest;
        }

        public bool DoHarvest(StatsData stats)
        {
            var workAmount = stats.GetActionSpeedForSkill(GrowingResourceData.Settings.ExtractionSkillType, true);
            GrowingResourceData.RemainingHarvestWork -= workAmount;
            
            // Update progress
            RuntimeData.PendingTaskProgress = 1f - (RuntimeData.Health / RuntimeData.MaxHealth);
            RefreshTaskIcon(RuntimeData.PendingTaskProgress);
            
            if (GrowingResourceData.RemainingHarvestWork <= 0)
            {
                HarvestFruit(stats);
                return true;
            }

            return false;
        }

        public override List<Command> GetCommands()
        {
            var result = new List<Command>(Commands);
            if (GrowingResourceData.HasFruitAvailable)
            {
                result.Add(GrowingResourceData.GrowingResourceSettings.HarvestCmd);
            }

            return result;
        }

        private void Update()
        {
            GrowthCheck();
            FruitCheck();
        }

        protected override void ExtractResource(StatsData stats)
        {
            HarvestFruit(stats);

            var resources = GrowingResourceData.GetGrowthStage().HarvestableItems.GetItemDrop();
            foreach (var resource in resources)
            {
                int amount = stats.DetermineAmountYielded(
                    GrowingResourceData.GrowingResourceSettings.ExtractionSkillType, resource.Quantity);
                for (int i = 0; i < amount; i++)
                {
                    var data = resource.Item.CreateItemData();
                    ItemsDatabase.Instance.CreateItemObject(data, transform.position, true);
                }
            }

            Destroy(gameObject);

            RefreshSelection();

            if (_onResourceClearedCallback != null) _onResourceClearedCallback.Invoke();
        }

        public override float GetWorkAmount()
        {
            return GrowingResourceData.GetWorkToCut();
        }
    }
    
    [Serializable]
     public class GrowthStage
     {
         public string StageName;
         public Sprite GrowthSprite;
         public float Scale;
         public float SecsInStage;
         public HarvestableItems HarvestableItems;
         [FormerlySerializedAs("WorkToCut")] public int Health;
     }
}
