using System;
using System.Collections.Generic;
using Characters;
using Data.Resource;
using ScriptableObjects;
using Managers;
using QFSW.QC;
using Systems.Stats.Scripts;
using TaskSystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Items
{
    public class GrowingResource : BasicResource
    {
        [SerializeField] protected SpriteRenderer _fruitOverlay;

        public bool IsFruiting => RuntimeGrowingResourceData.GrowingResourceSettings.HasFruit;
        public List<GameObject> TaskRequestors = new List<GameObject>();
        
        public GrowingResourceData RuntimeGrowingResourceData => RuntimeData as GrowingResourceData;
        
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
                
                GrowthCheck();
                FruitCheck();
            });
        }

        public override string DisplayName
        {
            get
            {
                if (!RuntimeGrowingResourceData.FullyGrown)
                {
                    var stageName = RuntimeGrowingResourceData.GetGrowthStage().StageName;
                    return $"{RuntimeData.Settings.title} ({stageName})";
                }

                return RuntimeData.title;
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
}
