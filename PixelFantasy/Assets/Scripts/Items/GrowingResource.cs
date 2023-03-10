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
        public List<GameObject> TaskRequestors = new List<GameObject>();

        private GrowingResourceData growingResourceData => ResourceData as GrowingResourceData;

        protected override void Awake()
        {
            base.Awake();
            if (ResourceData != null)
            {
                Init();
            }
        }

        public void Init(GrowingResourceData growingResourceData, GameObject prefab)
        {
            base.ResourceData = growingResourceData;
            Prefab = prefab;
            
            Init();
        }
        
        private void Init()
        {
            if (_overrideFullGrowth)
            {
                _fullyGrown = true;
                _growthIndex = growingResourceData.GrowthStages.Count - 1;
            }
            
            var stage = growingResourceData.GetGrowthStage(_growthIndex);
            _ageForNextGrowth += stage.SecsInStage;

            UpdateSprite();
            _remainingCutWork = GetWorkAmount();
            _remainingHarvestWork = GetHarvestWorkAmount();
            Health = GetWorkAmount();
        }
        
        protected void UpdateSprite()
        {
            var stage = growingResourceData.GetGrowthStage(_growthIndex);
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
            
            Destroy(gameObject);
        }

        public void CutDownPlant()
        {
            var resources = growingResourceData.GetGrowthStage(_growthIndex).HarvestableItems.GetItemDrop();
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
                        //dirt.CreateTaskById("Clear Grass");
                    }
                }
            }
            
            TaskRequestors.Clear();
            
            RefreshSelection();
            
            Destroy(gameObject);
        }
        
        public override int GetWorkAmount()
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
