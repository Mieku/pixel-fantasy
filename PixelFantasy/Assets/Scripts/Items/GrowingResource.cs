using System;
using System.Collections.Generic;
using Characters;
using Managers;
using QFSW.QC;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Items
{
    [Serializable]
    public class GrowingResourceData : BasicResourceData
    {
        public float AgeForNextGrowth
        {
            get
            {
                float result = 0;
                for (int i = GrowthIndex; i >= 0; i--)
                {
                    result += GrowingResourceSettings.GrowthStages[i].SecsInStage;
                }
                return result;
            }
        }
        
        public bool FullyGrown => AgeSec >= GrowingResourceSettings.TotalGrowTime();
        
        // Runtime
        public int GrowthIndex;
        public float AgeSec;
        public float FruitTimer;
        public bool ShowingFlowers;
        public float RemainingHarvestWork;
        public bool HasFruitAvailable;
        
        public GrowingResourceSettings GrowingResourceSettings => Settings as GrowingResourceSettings;
        
        public override void InitData(ResourceSettings settings)
        {
            base.InitData(settings);
            
            GrowthIndex = Random.Range(0, GrowingResourceSettings.GrowthStages.Count);

            float minAgeSec = 0f;
            for (int i = 0; i < GrowthIndex - 1; i++)
            {
                minAgeSec += GrowingResourceSettings.GrowthStages[i].SecsInStage;
            }

            if (FullyGrown)
            {
                AgeSec = Random.Range(minAgeSec, GrowingResourceSettings.TotalGrowTime() * 1.5f);
            }
            else
            {
                AgeSec = Random.Range(minAgeSec, GetGrowthStage().SecsInStage);
            }
            
            
            if (GrowingResourceSettings.HasFruit && AgeSec > GrowingResourceSettings.TotalGrowTime())
            {
                FruitTimer = Random.Range(0, GrowingResourceSettings.GrowFruitTime * 1.5f);
                HasFruitAvailable = FruitTimer >= GrowingResourceSettings.GrowFruitTime;
            }
            
            //RemainingExtractWork = GetWorkToCut();
            Health = GetGrowthStage().Health;
            RemainingHarvestWork = GrowingResourceSettings.WorkToHarvest;
        }

        public override float MaxHealth => GetGrowthStage().Health;

        public GrowthStage GetGrowthStage()
        {
            var stages = GrowingResourceSettings.GrowthStages;

            if (GrowthIndex >= stages.Count) 
                GrowthIndex = stages.Count - 1;
            
            return stages[GrowthIndex];
        }
        
        public int GetWorkToCut()
        {
            return GetGrowthStage().Health;
        }

        public List<ItemAmount> GetFruitLoot()
        {
            if (GrowingResourceSettings.HarvestableFruit != null)
            {
                return GrowingResourceSettings.HarvestableFruit.GetItemDrop();
            }

            return new List<ItemAmount>();
        }
        
        public float GetGrowthPercentage()
        {
            if (FullyGrown) return 1f;
            
            var growthPercent = AgeSec / GrowingResourceSettings.TotalGrowTime();
            return Mathf.Clamp01(growthPercent);
        }
        
        public float GetFruitingPercentage()
        {
            if (HasFruitAvailable) return 1f;
            
            var percent = FruitTimer / GrowingResourceSettings.GrowFruitTime;
            return Mathf.Clamp01(percent);
        }

        public float HarvestWorkDone(float workAmount)
        {
            RemainingHarvestWork -= workAmount;
            return RemainingHarvestWork;
        }
    }
    
    public class GrowingResource : BasicResource
    {
        [SerializeField] protected SpriteRenderer _fruitOverlay;

        public bool IsFruiting => RuntimeGrowingResourceData.GrowingResourceSettings.HasFruit;
        public List<GameObject> TaskRequestors = new List<GameObject>();
        
        public GrowingResourceData RuntimeGrowingResourceData => RuntimeData as GrowingResourceData;
        
        public override void InitializeResource(ResourceSettings settings)
        {
            _settings = settings;
            RuntimeData = new GrowingResourceData();
            RuntimeData.InitData(settings);
            RuntimeData.Position = transform.position;
            
            UpdateSprite();
            
            GrowthCheck();
            FruitCheck();
            //
            //
            // var data = settings.CreateInitialDataObject();
            //
            // DataLibrary.RegisterInitializationCallback(() =>
            // {
            //     RuntimeData = (ResourceData)DataLibrary.CloneDataObjectToRuntime(data, gameObject);
            //     RuntimeData.InitData(settings);
            //     RuntimeData.Position = transform.position;
            //     
            //     UpdateSprite();
            //     
            //     DataLibrary.OnSaved += Saved;
            //     DataLibrary.OnLoaded += Loaded;
            //     
            //     GrowthCheck();
            //     FruitCheck();
            // });
        }

        public override string DisplayName
        {
            get
            {
                if (!RuntimeGrowingResourceData.FullyGrown)
                {
                    var stageName = RuntimeGrowingResourceData.GetGrowthStage().StageName;
                    return $"{RuntimeData.Settings.ResourceName} ({stageName})";
                }

                return RuntimeData.Settings.name;
            }
        }
        
        protected override void UpdateSprite()
        {
            var stage = RuntimeGrowingResourceData.GetGrowthStage();
            var scaleOverride = stage.Scale;
            _spriteRenderer.sprite = stage.GrowthSprite;
            _spriteRenderer.gameObject.transform.localScale = new Vector3(scaleOverride, scaleOverride, 1);
        }

        public override HarvestableItems GetHarvestableItems()
        {
            var stage = RuntimeGrowingResourceData.GetGrowthStage();
            return stage.HarvestableItems;
        }

        public Sprite GetGrowthIcon()
        {
            return Librarian.Instance.GetSprite("Growth");
        }
        
        protected void GrowthCheck()
        {
            if (RuntimeGrowingResourceData == null) return;
            
            if (!RuntimeGrowingResourceData.FullyGrown)
            {
                RuntimeGrowingResourceData.AgeSec += TimeManager.Instance.DeltaTime;
                if (RuntimeGrowingResourceData.AgeSec >= RuntimeGrowingResourceData.AgeForNextGrowth)
                {
                    var extractDiff = RuntimeGrowingResourceData.MaxHealth - RuntimeGrowingResourceData.Health;
                    
                    RuntimeGrowingResourceData.GrowthIndex++;

                    RuntimeGrowingResourceData.Health = RuntimeGrowingResourceData.MaxHealth - extractDiff;

                    RefreshSelection();
                    UpdateSprite();
                }
            }
        }

        [Command("instant_fruit", MonoTargetType.All)]
        private void CMD_InstantFruit()
        {
            if (!RuntimeGrowingResourceData.HasFruitAvailable)
            {
                RuntimeGrowingResourceData.FruitTimer = RuntimeGrowingResourceData.GrowingResourceSettings.GrowFruitTime;
            }
        }

        public Sprite GetFruitIcon()
        {
            if (!IsFruiting) return null;
            
            return RuntimeGrowingResourceData.GetFruitLoot()[0].Item.ItemSprite;
        }

        protected void FruitCheck()
        {
            if (RuntimeGrowingResourceData == null) return;
            
            if (!RuntimeGrowingResourceData.FullyGrown) return;
            
            if (RuntimeGrowingResourceData.GrowingResourceSettings.HasFruit && !RuntimeGrowingResourceData.HasFruitAvailable)
            {
                RuntimeGrowingResourceData.FruitTimer += TimeManager.Instance.DeltaTime;
                if (RuntimeGrowingResourceData.FruitTimer >= RuntimeGrowingResourceData.GrowingResourceSettings.GrowFruitTime)
                {
                    RuntimeGrowingResourceData.FruitTimer = 0;
                    _fruitOverlay.sprite = RuntimeGrowingResourceData.GrowingResourceSettings.FruitOverlay;
                    _fruitOverlay.gameObject.SetActive(true);
                    RuntimeGrowingResourceData.HasFruitAvailable = true;
                    RefreshSelection();
                } 
                else if (RuntimeGrowingResourceData.FruitTimer >= RuntimeGrowingResourceData.GrowingResourceSettings.GrowFruitTime / 2f)
                {
                    if (!RuntimeGrowingResourceData.ShowingFlowers && RuntimeGrowingResourceData.GrowingResourceSettings.HasFruitFlowers)
                    {
                        RuntimeGrowingResourceData.ShowingFlowers = true;
                        _fruitOverlay.sprite = RuntimeGrowingResourceData.GrowingResourceSettings.FruitFlowersOverlay;
                        _fruitOverlay.gameObject.SetActive(true);
                    }
                }
            }

            if (RuntimeGrowingResourceData.HasFruitAvailable)
            {
                _fruitOverlay.sprite = RuntimeGrowingResourceData.GrowingResourceSettings.FruitOverlay;
                _fruitOverlay.gameObject.SetActive(true);
            }
        }

        public void HarvestFruit(StatsData stats)
        {
            if (RuntimeGrowingResourceData.HasFruitAvailable)
            {
                _fruitOverlay.gameObject.SetActive(false);
                List<ItemAmount> fruits = RuntimeGrowingResourceData.GetFruitLoot();
                foreach (var fruit in fruits)
                {
                    int amount = stats.DetermineAmountYielded(
                        RuntimeGrowingResourceData.GrowingResourceSettings.ExtractionSkillType, fruit.Quantity);
                    for (int i = 0; i < amount; i++)
                    {
                        spawner.SpawnItem(fruit.Item, transform.position, true);
                    }
                }
                RuntimeGrowingResourceData.HasFruitAvailable = false;
                RefreshSelection();
                DisplayTaskIcon(null);

                if (PendingCommand == RuntimeGrowingResourceData.GrowingResourceSettings.HarvestCmd)
                {
                    PendingCommand = null;
                }
            }

            RuntimeGrowingResourceData.RemainingHarvestWork = RuntimeGrowingResourceData.GrowingResourceSettings.WorkToHarvest;
        }

        public bool DoHarvest(StatsData stats)
        {
            var workAmount = stats.GetActionSpeedForSkill(RuntimeGrowingResourceData.Settings.ExtractionSkillType, true);
            RuntimeGrowingResourceData.RemainingHarvestWork -= workAmount;
            if (RuntimeGrowingResourceData.RemainingHarvestWork <= 0)
            {
                HarvestFruit(stats);
                return true;
            }
            
            return false;
        }

        public override List<Command> GetCommands()
        {
            var result = new List<Command>(Commands);
            if (RuntimeGrowingResourceData.HasFruitAvailable)
            {
                result.Add(RuntimeGrowingResourceData.GrowingResourceSettings.HarvestCmd);
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
            
            var resources = RuntimeGrowingResourceData.GetGrowthStage().HarvestableItems.GetItemDrop();
            foreach (var resource in resources)
            {
                int amount = stats.DetermineAmountYielded(
                    RuntimeGrowingResourceData.GrowingResourceSettings.ExtractionSkillType, resource.Quantity);
                for (int i = 0; i < amount; i++)
                {
                    spawner.SpawnItem(resource.Item, transform.position, true);
                }
            }
            
            Destroy(gameObject);
            
            RefreshSelection();
            
            if(_onResourceClearedCallback != null) _onResourceClearedCallback.Invoke();
        }
        
        public override float GetWorkAmount()
        {
            return RuntimeGrowingResourceData.GetWorkToCut();
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
