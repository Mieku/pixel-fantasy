using System;
using System.Collections.Generic;
using ScriptableObjects;
using Managers;
using QFSW.QC;
using TaskSystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Items
{
    public class GrowingResource : Resource
    {
        [SerializeField] protected bool _overrideFullGrowth;
        [SerializeField] protected SpriteRenderer _fruitOverlay;
        [SerializeField] private Command _harvestCmd;

        public int _growthIndex;
        protected float _ageSec;
        protected float _ageForNextGrowth;
        protected bool _fullyGrown;
        protected float _fruitTimer;
        protected bool _hasFruitAvailable;
        protected bool _showingFlowers;
        protected float _remainingCutWork;
        protected float _remainingHarvestWork;

        public bool HasFruitAvailable => _hasFruitAvailable;
        public bool FullyGrown => _fullyGrown;
        public bool IsFruiting => growingResourceData.HasFruit;
        public List<GameObject> TaskRequestors = new List<GameObject>();

        private GrowingResourceData growingResourceData => ResourceData as GrowingResourceData;

        public override void Init(ResourceData data)
        {
            base.Init(data);
            
            if (_overrideFullGrowth)
            {
                _fullyGrown = true;
                _growthIndex = growingResourceData.GrowthStages.Count - 1;
            }
            else
            {
                _growthIndex = Random.Range(0, growingResourceData.GrowthStages.Count);
            }

            _ageSec = Random.Range(0, growingResourceData.TotalGrowTime() * 1.5f);
            _fullyGrown = _ageSec >= growingResourceData.TotalGrowTime();
            
            if (growingResourceData.HasFruit && _ageSec > growingResourceData.TotalGrowTime())
            {
                _fruitTimer = Random.Range(0, growingResourceData.TimeToGrowFruit * 1.5f);
                _hasFruitAvailable = _fruitTimer >= growingResourceData.TimeToGrowFruit;
            }
            
            _remainingCutWork = GetWorkAmount();
            _remainingHarvestWork = GetHarvestWorkAmount();
            Health = GetWorkAmount();
            
            GrowthCheck();
            FruitCheck();
        }

        public override string DisplayName
        {
            get
            {
                if (!_fullyGrown)
                {
                    var stageName = growingResourceData.GetGrowthStage(_growthIndex).StageName;
                    return $"{ResourceData.ResourceName} ({stageName})";
                }

                return ResourceData.ResourceName;
            }
        }
        
        protected void UpdateSprite()
        {
            var stage = growingResourceData.GetGrowthStage(_growthIndex);
            var scaleOverride = stage.Scale;
            _spriteRenderer.sprite = stage.GrowthSprite;
            _spriteRenderer.gameObject.transform.localScale = new Vector3(scaleOverride, scaleOverride, 1);
        }

        public override HarvestableItems GetHarvestableItems()
        {
            var stage = growingResourceData.GetGrowthStage(_growthIndex);
            return stage.HarvestableItems;
        }

        public float GetGrowthPercentage()
        {
            if (_fullyGrown) return 1f;
            
            var growthPercent = _ageSec / growingResourceData.TotalGrowTime();
            return Mathf.Clamp01(growthPercent);
        }

        public Sprite GetGrowthIcon()
        {
            return Librarian.Instance.GetSprite("Growth");
        }
        
        protected void GrowthCheck()
        {
            if (!_fullyGrown)
            {
                _ageSec += TimeManager.Instance.DeltaTime;
                if (_ageSec >= _ageForNextGrowth)
                {
                    _growthIndex++;

                    if (_growthIndex < growingResourceData.GrowthStages.Count)
                    {
                        var stage = growingResourceData.GetGrowthStage(_growthIndex);
                        _ageForNextGrowth += stage.SecsInStage;
                        RefreshSelection();
                    }
                    else
                    {
                        _fullyGrown = true;
                    }
                    
                    UpdateSprite();
                }
            }
        }

        [Command("instant_fruit", MonoTargetType.All)]
        private void CMD_InstantFruit()
        {
            if (!_hasFruitAvailable)
            {
                _fruitTimer = growingResourceData.TimeToGrowFruit;
            }
        }

        public Sprite GetFruitIcon()
        {
            if (!IsFruiting) return null;
            
            return growingResourceData.GetFruitLoot()[0].Item.ItemSprite;
        }

        public float GetFruitingPercentage()
        {
            if (_hasFruitAvailable) return 1f;
            
            var percent = _fruitTimer / growingResourceData.TimeToGrowFruit;
            return Mathf.Clamp01(percent);
        }

        protected void FruitCheck()
        {
            if (!_fullyGrown) return;
            
            if (growingResourceData.HasFruit && !_hasFruitAvailable)
            {
                _fruitTimer += TimeManager.Instance.DeltaTime;
                if (_fruitTimer >= growingResourceData.TimeToGrowFruit)
                {
                    _fruitTimer = 0;
                    _hasFruitAvailable = true;
                    _fruitOverlay.sprite = growingResourceData.FruitOverlay;
                    _fruitOverlay.gameObject.SetActive(true);
                    RefreshSelection();
                } else if (_fruitTimer >= growingResourceData.TimeToGrowFruit / 2f)
                {
                    if (!_showingFlowers && growingResourceData.HasFruitFlowers)
                    {
                        _showingFlowers = true;
                        _fruitOverlay.sprite = growingResourceData.FruitFlowersOverlay;
                        _fruitOverlay.gameObject.SetActive(true);
                    }
                }
            }

            if (_hasFruitAvailable)
            {
                _fruitOverlay.sprite = growingResourceData.FruitOverlay;
                _fruitOverlay.gameObject.SetActive(true);
            }
        }

        public void HarvestFruit()
        {
            if (_hasFruitAvailable)
            {
                _fruitOverlay.gameObject.SetActive(false);
                List<ItemAmount> fruits = growingResourceData.GetFruitLoot();
                foreach (var fruit in fruits)
                {
                    for (int i = 0; i < fruit.Quantity; i++)
                    {
                        spawner.SpawnItem(fruit.Item, transform.position, true);
                    }
                }
                _hasFruitAvailable = false;
                RefreshSelection();
                DisplayTaskIcon(null);

                if (PendingCommand == _harvestCmd)
                {
                    PendingCommand = null;
                }
            }
            
            _remainingHarvestWork = GetHarvestWorkAmount();
        }

        public bool DoHarvest(float workAmount)
        {
            _remainingHarvestWork -= workAmount;
            if (_remainingHarvestWork <= 0)
            {
                HarvestFruit();
                return true;
            }
            
            return false;
        }

        public override List<Command> GetCommands()
        {
            var result = new List<Command>(Commands);
            if (_hasFruitAvailable)
            {
                result.Add(_harvestCmd);
            }

            return result;
        }

        private void Update()
        {
            GrowthCheck();
            FruitCheck();
        }

        protected override void DestroyResource()
        {
            HarvestFruit();
            
            var resources = growingResourceData.GetGrowthStage(_growthIndex).HarvestableItems.GetItemDrop();
            foreach (var resource in resources)
            {
                for (int i = 0; i < resource.Quantity; i++)
                {
                    spawner.SpawnItem(resource.Item, transform.position, true);
                }
            }
            
            RefreshSelection();
            
            base.DestroyResource();
        }
        
        public override float GetWorkAmount()
        {
            return growingResourceData.GetWorkToCut(_growthIndex);
        }

        public int GetHarvestWorkAmount()
        {
            return growingResourceData.WorkToHarvest;
        }
        
        public float CutWorkDone(float workAmount)
        {
            _remainingCutWork -= workAmount;
            return _remainingCutWork;
        }

        public float HarvestWorkDone(float workAmount)
        {
            _remainingHarvestWork -= workAmount;
            return _remainingHarvestWork;
        }
     
        public override object CaptureState()
        {
            var resourceData = (Data)base.CaptureState();
            resourceData.GrowingData = new GrowingData
            {
                GrowthIndex = _growthIndex,
                AgeSec = _ageSec,
                AgeForNextGrowth = _ageForNextGrowth,
                FullyGrown = _fullyGrown,
                FruitTimer = _fruitTimer,
                HasFruitAvailable = _hasFruitAvailable,
                RemainingCutWork = _remainingCutWork,
                RemainingHarvestWork = _remainingHarvestWork
            };

            return resourceData;
        }

        public override void RestoreState(object stateData)
        {
            var resourceData = (Data)stateData;
            var growingData = resourceData.GrowingData;

            base.RestoreState(stateData);

            _growthIndex = growingData.GrowthIndex;
            _ageSec = growingData.AgeSec;
            _ageForNextGrowth = growingData.AgeForNextGrowth;
            _fullyGrown = growingData.FullyGrown;
            _fruitTimer = growingData.FruitTimer;
            _hasFruitAvailable = growingData.HasFruitAvailable;
            _remainingCutWork = growingData.RemainingCutWork;
            _remainingHarvestWork = growingData.RemainingHarvestWork;
            
            UpdateSprite();
        }

        public struct GrowingData
        {
            public int GrowthIndex;
            public float AgeSec;
            public float AgeForNextGrowth;
            public bool FullyGrown;
            public float FruitTimer;
            public bool HasFruitAvailable;
            public float RemainingCutWork;
            public float RemainingHarvestWork;
        }
    }
}
