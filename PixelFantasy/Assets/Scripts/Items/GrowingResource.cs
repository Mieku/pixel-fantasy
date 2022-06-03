using System;
using System.Collections.Generic;
using Interfaces;
using ScriptableObjects;
using Tasks;
using Unit;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Items
{
    public class GrowingResource : Resource
    {
        [SerializeField] protected bool _overrideFullGrowth;
        [SerializeField] protected SpriteRenderer _fruitOverlay;
        
        protected int _growthIndex;
        protected float _ageSec;
        protected float _ageForNextGrowth;
        protected bool _fullyGrown;
        protected float _reproductionTimer;
        protected float _fruitTimer;
        protected bool _hasFruitAvailable;
        protected bool _queuedToHarvest;

        public bool HasFruitAvailable => _hasFruitAvailable;
        public bool QueuedToHarvest => _queuedToHarvest;
        public List<GameObject> TaskRequestors = new List<GameObject>();
        
        private void Start()
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
            _reproductionTimer = _growingResourceData.ReproductiveRateSec;

            UpdateSprite();
        }
        
        protected void UpdateSprite()
        {
            var stage = _growingResourceData.GetGrowthStage(_growthIndex);
            var scaleOverride = stage.Scale;
            _spriteRenderer.sprite = stage.GrowthSprite;
            _spriteRenderer.gameObject.transform.localScale = new Vector3(scaleOverride, scaleOverride, 1);
        }
        
        protected virtual void AttemptReproduction()
        {
            var pos = _growingResourceData.GetReproductionPos(transform.position);
            var valid = _growingResourceData.IsReproductionPosValid(pos);
            if (valid)
            {
                spawner.SpawnPlant(pos, GetResourceData());
            }
        }
        
        protected void ReproductionCheck()
        {
            if (_fullyGrown && _growingResourceData.Reproduces)
            {
                _reproductionTimer -= Time.deltaTime;
                if (_reproductionTimer < 0)
                {
                    _reproductionTimer = _growingResourceData.ReproductiveRateSec;
                    AttemptReproduction();
                }
            }
        }
        
        protected void GrowthCheck()
        {
            if (!_fullyGrown)
            {
                _ageSec += Time.deltaTime;
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
                _fruitTimer += Time.deltaTime;
                if (_fruitTimer >= _growingResourceData.TimeToGrowFruit)
                {
                    _fruitTimer = 0;
                    _hasFruitAvailable = true;
                    _fruitOverlay.sprite = _growingResourceData.FruitOverlay;
                    _fruitOverlay.gameObject.SetActive(true);
                    RefreshSelection();
                }
            }

            if (_hasFruitAvailable)
            {
                _fruitOverlay.sprite = _growingResourceData.FruitOverlay;
                _fruitOverlay.gameObject.SetActive(true);
            }
        }

        protected void HarvestFruit()
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
                _queuedToHarvest = false;
                RefreshSelection();
            }
            
            SetIcon(null);
        }
        
        private void Update()
        {
            GrowthCheck();
            ReproductionCheck();
            FruitCheck();
        }
        
        public void CreateCutPlantTask()
        {
            if (_queuedToCut)
            {
                CancelCutPlantTask();
                return;
            }
            
            CancelTasks();
            _queuedToCut = true;
            SetIcon("Scythe");

            var task = new FarmingTask.CutPlant()
            {
                claimPlant = (UnitTaskAI unitTaskAI) =>
                {
                    _incomingUnit = unitTaskAI;
                },
                plantPosition = transform.position,
                workAmount = _growingResourceData.GetWorkToCut(_growthIndex),
                completeWork = CutDownPlant
            };

            var taskHash = task.GetHashCode();
            _assignedTaskRefs.Add(taskHash);
            taskMaster.FarmingTaskSystem.AddTask(task);
            
            RefreshSelection();
        }

        public void CancelCutPlantTask()
        {
            _queuedToCut = false;
            SetIcon(null);
            CancelTasks();
        }

        public void CreateHarvestFruitTask()
        {
            if (_queuedToHarvest)
            {
                CancelHarvestFruitTask();
                return;
            }
            
            CancelTasks();
            _queuedToHarvest = true;
            SetIcon("Scythe");
            
            // Choose a random side of the tree
            var sideMod = 1;
            var rand = Random.Range(0, 2);
            if (rand == 1)
            {
                sideMod *= -1;
            }

            var cutPos = transform.position;
            cutPos.x += sideMod;

            var task = new FarmingTask.HarvestFruit()
            {
                claimPlant = (UnitTaskAI unitTaskAI) =>
                {
                    _incomingUnit = unitTaskAI;
                },
                plantPosition = cutPos,
                workAmount = _growingResourceData.WorkToHarvest,
                completeWork = HarvestFruit
            };
            
            _assignedTaskRefs.Add(task.GetHashCode());
            taskMaster.FarmingTaskSystem.AddTask(task);
            
            RefreshSelection();
        }

        public void CancelHarvestFruitTask()
        {
            _queuedToHarvest = false;
            SetIcon(null);
            CancelTasks();
        }
        
        protected void CutDownPlant()
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
                        dirt.CreateClearGrassTask();
                    }
                }
            }
            
            TaskRequestors.Clear();
            
            RefreshSelection();
            
            Destroy(gameObject);
        }

        protected override void CancelTasks()
        {
            base.CancelTasks();

            _queuedToHarvest = false;
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

        public override bool IsOrderActive(Order order)
        {
            switch (order)
            {
                case Order.CutPlant:
                    return _queuedToCut;
                case Order.Harvest:
                    return _queuedToHarvest;
                default:
                    throw new ArgumentOutOfRangeException(nameof(order), order, null);
            }
        }
        
        public override void AssignOrder(Order orderToAssign)
        {
            switch (orderToAssign)
            {
                case Order.CutPlant:
                    CreateCutPlantTask();
                    break;
                case Order.Harvest:
                    CreateHarvestFruitTask();
                    break;
            }
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
                ReproductionTimer = _reproductionTimer,
                FruitTimer = _fruitTimer,
                HasFruitAvailable = _hasFruitAvailable,
                QueuedToHarvest = _queuedToHarvest,
            };

            return resourceData;
        }

        public override void RestoreState(object stateData)
        {
            var resourceData = (Data)stateData;
            var growingData = resourceData.GrowingData;

            _growthIndex = growingData.GrowthIndex;
            _ageSec = growingData.AgeSec;
            _ageForNextGrowth = growingData.AgeForNextGrowth;
            _fullyGrown = growingData.FullyGrown;
            _reproductionTimer = growingData.ReproductionTimer;
            _fruitTimer = growingData.FruitTimer;
            _hasFruitAvailable = growingData.HasFruitAvailable;
            _queuedToHarvest = growingData.QueuedToHarvest;
    
            base.RestoreState(stateData);
        }

        public struct GrowingData
        {
            public int GrowthIndex;
            public float AgeSec;
            public float AgeForNextGrowth;
            public bool FullyGrown;
            public float ReproductionTimer;
            public float FruitTimer;
            public bool HasFruitAvailable;
            public bool QueuedToHarvest;
        }
    }
}
