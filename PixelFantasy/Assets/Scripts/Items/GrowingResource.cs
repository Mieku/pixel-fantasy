using System;
using System.Collections.Generic;
using Data.Resource;
using ScriptableObjects;
using Managers;
using QFSW.QC;
using TaskSystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Items
{
    public class GrowingResource : BasicResource
    {
        //[SerializeField] protected bool _overrideFullGrowth;
        [SerializeField] protected SpriteRenderer _fruitOverlay;
        // [SerializeField] private Command _harvestCmd;

        // public int _growthIndex;
        // protected float _ageSec;
        // protected float _ageForNextGrowth;
        // protected bool _fullyGrown;
        // protected float _fruitTimer;
        // protected bool _hasFruitAvailable;
        protected bool _showingFlowers;
        // protected float _remainingCutWork;
        // protected float _remainingHarvestWork;

        // public bool HasFruitAvailable => _hasFruitAvailable;
        // public bool FullyGrown => _fullyGrown;
        public bool IsFruiting => GrowingResourceData.HasFruit;
        public List<GameObject> TaskRequestors = new List<GameObject>();

        // private GrowingResourceSettings growingResourceSettings => ResourceSettings as GrowingResourceSettings;
        public GrowingResourceData RuntimeGrowingResourceData => RuntimeData as GrowingResourceData;
        private GrowingResourceData GrowingResourceData => Data as GrowingResourceData;
        public override void Init(ResourceData data)
        {
            base.Init(data);
        }

        protected override void InitialDataReady()
        {
            base.InitialDataReady();
            
            GrowthCheck();
            FruitCheck();
        }

        // public override void Init(ResourceSettings settings)
        // {
        //     base.Init(settings);
        //     
        //     if (_overrideFullGrowth)
        //     {
        //         _fullyGrown = true;
        //         _growthIndex = growingResourceSettings.GrowthStages.Count - 1;
        //     }
        //     else
        //     {
        //         _growthIndex = Random.Range(0, growingResourceSettings.GrowthStages.Count);
        //     }
        //
        //     _ageSec = Random.Range(0, growingResourceSettings.TotalGrowTime() * 1.5f);
        //     _fullyGrown = _ageSec >= growingResourceSettings.TotalGrowTime();
        //     
        //     if (growingResourceSettings.HasFruit && _ageSec > growingResourceSettings.TotalGrowTime())
        //     {
        //         _fruitTimer = Random.Range(0, growingResourceSettings.TimeToGrowFruit * 1.5f);
        //         _hasFruitAvailable = _fruitTimer >= growingResourceSettings.TimeToGrowFruit;
        //     }
        //     
        //     _remainingCutWork = GetWorkAmount();
        //     _remainingHarvestWork = GetHarvestWorkAmount();
        //     Health = GetWorkAmount();
        //     
        //     GrowthCheck();
        //     FruitCheck();
        // }

        public override string DisplayName
        {
            get
            {
                if (!RuntimeGrowingResourceData.FullyGrown)
                {
                    var stageName = RuntimeGrowingResourceData.GetGrowthStage().StageName;
                    return $"{Data.title} ({stageName})";
                }

                return Data.title;
            }
        }
        
        protected void UpdateSprite()
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
            if (!RuntimeGrowingResourceData.FullyGrown)
            {
                RuntimeGrowingResourceData.AgeSec += TimeManager.Instance.DeltaTime;
                if (RuntimeGrowingResourceData.AgeSec >= RuntimeGrowingResourceData.AgeForNextGrowth)
                {
                    RuntimeGrowingResourceData.GrowthIndex++;

                    if (RuntimeGrowingResourceData.GrowthIndex < RuntimeGrowingResourceData.GrowthStages.Count)
                    {
                        var stage = RuntimeGrowingResourceData.GetGrowthStage();
                        RuntimeGrowingResourceData.AgeForNextGrowth += stage.SecsInStage;
                        RefreshSelection();
                    }
                    // else
                    // {
                    //     _fullyGrown = true;
                    //     RuntimeGrowingResourceData.FullyGrown = true;
                    // }
                    
                    UpdateSprite();
                }
            }
        }

        [Command("instant_fruit", MonoTargetType.All)]
        private void CMD_InstantFruit()
        {
            if (!RuntimeGrowingResourceData.HasFruitAvailable)
            {
                RuntimeGrowingResourceData.FruitTimer = RuntimeGrowingResourceData.GrowFruitTime;
            }
        }

        public Sprite GetFruitIcon()
        {
            if (!IsFruiting) return null;
            
            return RuntimeGrowingResourceData.GetFruitLoot()[0].Item.ItemSprite;
        }

        protected void FruitCheck()
        {
            if (!RuntimeGrowingResourceData.FullyGrown) return;
            
            if (RuntimeGrowingResourceData.HasFruit && !RuntimeGrowingResourceData.HasFruitAvailable)
            {
                RuntimeGrowingResourceData.FruitTimer += TimeManager.Instance.DeltaTime;
                if (RuntimeGrowingResourceData.FruitTimer >= RuntimeGrowingResourceData.GrowFruitTime)
                {
                    RuntimeGrowingResourceData.FruitTimer = 0;
                    _fruitOverlay.sprite = RuntimeGrowingResourceData.FruitOverlay;
                    _fruitOverlay.gameObject.SetActive(true);
                    RuntimeGrowingResourceData.HasFruitAvailable = true;
                    RefreshSelection();
                } 
                else if (RuntimeGrowingResourceData.FruitTimer >= RuntimeGrowingResourceData.GrowFruitTime / 2f)
                {
                    if (!_showingFlowers && RuntimeGrowingResourceData.HasFruitFlowers)
                    {
                        _showingFlowers = true;
                        _fruitOverlay.sprite = RuntimeGrowingResourceData.FruitFlowersOverlay;
                        _fruitOverlay.gameObject.SetActive(true);
                    }
                }
            }

            if (RuntimeGrowingResourceData.HasFruitAvailable)
            {
                _fruitOverlay.sprite = RuntimeGrowingResourceData.FruitOverlay;
                _fruitOverlay.gameObject.SetActive(true);
            }
        }

        public void HarvestFruit()
        {
            if (RuntimeGrowingResourceData.HasFruitAvailable)
            {
                _fruitOverlay.gameObject.SetActive(false);
                List<ItemAmount> fruits = RuntimeGrowingResourceData.GetFruitLoot();
                foreach (var fruit in fruits)
                {
                    for (int i = 0; i < fruit.Quantity; i++)
                    {
                        spawner.SpawnItem(fruit.Item, transform.position, true);
                    }
                }
                RuntimeGrowingResourceData.HasFruitAvailable = false;
                RefreshSelection();
                DisplayTaskIcon(null);

                if (PendingCommand == RuntimeGrowingResourceData.HarvestCmd)
                {
                    PendingCommand = null;
                }
            }

            RuntimeGrowingResourceData.RemainingHarvestWork = RuntimeGrowingResourceData.WorkToHarvest;
        }

        public bool DoHarvest(float workAmount)
        {
            RuntimeGrowingResourceData.RemainingHarvestWork -= workAmount;
            if (RuntimeGrowingResourceData.RemainingHarvestWork <= 0)
            {
                HarvestFruit();
                return true;
            }
            
            return false;
        }

        public override List<Command> GetCommands()
        {
            var result = new List<Command>(Commands);
            if (RuntimeGrowingResourceData.HasFruitAvailable)
            {
                result.Add(RuntimeGrowingResourceData.HarvestCmd);
            }

            return result;
        }

        private void Update()
        {
            GrowthCheck();
            FruitCheck();
        }

        protected override void HarvestResource()
        {
            HarvestFruit();
            
            var resources = RuntimeGrowingResourceData.GetGrowthStage().HarvestableItems.GetItemDrop();
            foreach (var resource in resources)
            {
                for (int i = 0; i < resource.Quantity; i++)
                {
                    spawner.SpawnItem(resource.Item, transform.position, true);
                }
            }
            
            RefreshSelection();
            
            base.HarvestResource();
        }
        
        public override float GetWorkAmount()
        {
            return RuntimeGrowingResourceData.GetWorkToCut();
        }
    }
}
