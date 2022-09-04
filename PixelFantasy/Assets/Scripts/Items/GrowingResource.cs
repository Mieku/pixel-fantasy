using System;
using System.Collections.Generic;
using ScriptableObjects;
using Gods;
using UnityEngine;

namespace Items
{
    public class GrowingResource : Resource
    {
        [SerializeField] protected bool _overrideFullGrowth;
        [SerializeField] protected SpriteRenderer _fruitOverlay;

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
        public List<GameObject> TaskRequestors = new List<GameObject>();
        
        private void Awake()
        {
            if (_growingResourceData != null)
            {
                Init();
            }
        }

        public void Init(GrowingResourceData resourceData, GameObject prefab)
        {
            _growingResourceData = resourceData;
            Prefab = prefab;
            
            Init();
        }
        
        private void Init()
        {
            if (_overrideFullGrowth)
            {
                _fullyGrown = true;
                _growthIndex = _growingResourceData.GrowthStages.Count - 1;
            }
            
            var stage = _growingResourceData.GetGrowthStage(_growthIndex);
            _ageForNextGrowth += stage.SecsInStage;

            UpdateSprite();
            _remainingCutWork = GetWorkAmount();
            _remainingHarvestWork = GetHarvestWorkAmount();
        }
        
        protected void UpdateSprite()
        {
            var stage = _growingResourceData.GetGrowthStage(_growthIndex);
            var scaleOverride = stage.Scale;
            _spriteRenderer.sprite = stage.GrowthSprite;
            _spriteRenderer.gameObject.transform.localScale = new Vector3(scaleOverride, scaleOverride, 1);
        }
        
        protected void GrowthCheck()
        {
            if (!_fullyGrown)
            {
                _ageSec += TimeManager.Instance.DeltaTime;
                if (_ageSec >= _ageForNextGrowth)
                {
                    _growthIndex++;

                    if (_growthIndex < _growingResourceData.GrowthStages.Count)
                    {
                        var stage = _growingResourceData.GetGrowthStage(_growthIndex);
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

        protected void FruitCheck()
        {
            if (!_fullyGrown) return;
            
            if (_growingResourceData.HasFruit && !_hasFruitAvailable)
            {
                _fruitTimer += TimeManager.Instance.DeltaTime;
                if (_fruitTimer >= _growingResourceData.TimeToGrowFruit)
                {
                    _fruitTimer = 0;
                    _hasFruitAvailable = true;
                    _fruitOverlay.sprite = _growingResourceData.FruitOverlay;
                    _fruitOverlay.gameObject.SetActive(true);
                    RefreshSelection();
                } else if (_fruitTimer >= _growingResourceData.TimeToGrowFruit / 2f)
                {
                    if (!_showingFlowers && _growingResourceData.HasFruitFlowers)
                    {
                        _showingFlowers = true;
                        _fruitOverlay.sprite = _growingResourceData.FruitFlowersOverlay;
                        _fruitOverlay.gameObject.SetActive(true);
                    }
                }
            }

            if (_hasFruitAvailable)
            {
                _fruitOverlay.sprite = _growingResourceData.FruitOverlay;
                _fruitOverlay.gameObject.SetActive(true);
            }
        }

        public void HarvestFruit()
        {
            if (_hasFruitAvailable)
            {
                _fruitOverlay.gameObject.SetActive(false);
                List<ItemAmount> fruits = _growingResourceData.GetFruitLoot();
                foreach (var fruit in fruits)
                {
                    for (int i = 0; i < fruit.Quantity; i++)
                    {
                        spawner.SpawnItem(fruit.Item, transform.position, true);
                    }
                }
                _hasFruitAvailable = false;
                // _queuedToHarvest = false;
                RefreshSelection();
            }
            
            _remainingHarvestWork = GetHarvestWorkAmount();
        }
        
        private void Update()
        {
            GrowthCheck();
            FruitCheck();
        }
  
        public void CutDownPlant()
        {
            var resources = _growingResourceData.GetGrowthStage(_growthIndex).HarvestableItems.GetItemDrop();
            foreach (var resource in resources)
            {
                for (int i = 0; i < resource.Quantity; i++)
                {
                    spawner.SpawnItem(resource.Item, transform.position, true);
                }
            }
            
            HarvestFruit();

            foreach (var taskRequestor in TaskRequestors)
            {
                if (taskRequestor != null)
                {
                    var dirt = taskRequestor.GetComponent<DirtTile>();
                    if (dirt != null)
                    {
                        dirt.CreateTaskById("Clear Grass");
                    }
                }
            }
            
            TaskRequestors.Clear();
            
            RefreshSelection();
            
            Destroy(gameObject);
        }
        
        public override List<Order> GetOrders()
        {
            List<Order> results = new List<Order>();
            results.Add(Order.CutPlant);
            
            if (_hasFruitAvailable)
            {
                results.Add(Order.Harvest);
            }

            return results;
        }
        
        public override int GetWorkAmount()
        {
            return _growingResourceData.GetWorkToCut(_growthIndex);
        }

        public int GetHarvestWorkAmount()
        {
            return _growingResourceData.WorkToHarvest;
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
