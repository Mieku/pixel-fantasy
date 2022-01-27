using System.Collections.Generic;
using ScriptableObjects;
using Tasks;
using Unit;
using UnityEngine;

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
        
        private void Start()
        {
            if (_growingResourceData != null)
            {
                Init();
            }
        }

        public void Init(GrowingResourceData resourceData)
        {
            _growingResourceData = resourceData;
            
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
                }
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
            CancelTasks();
            _queuedToCut = true;
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

            var task = new FarmingTask.CutPlant()
            {
                claimPlant = (UnitTaskAI unitTaskAI) =>
                {
                    _incomingUnit = unitTaskAI;
                },
                plantPosition = cutPos,
                workAmount = _growingResourceData.GetWorkToCut(_growthIndex),
                completeWork = CutDownTree
            };
            
            _assignedTaskRefs.Add(task.GetHashCode());
            taskMaster.FarmingTaskSystem.AddTask(task);
        }

        public void CancelCutPlantTask()
        {
            _queuedToCut = false;
            SetIcon(null);
            CancelTasks();
        }

        public void CreateHarvestFruitTask()
        {
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
        }

        public void CancelHarvestFruitTask()
        {
            _queuedToHarvest = false;
            SetIcon(null);
            CancelTasks();
        }
        
        private void CutDownTree()
        {
            var resources = _growingResourceData.GetGrowthStage(_growthIndex).HarvestableItems.GetItemDrop();
            foreach (var resource in resources)
            {
                for (int i = 0; i < resource.Quantity; i++)
                {
                    spawner.SpawnItem(resource.Item, transform.position, true);
                }
            }
            
            Destroy(gameObject);
        }

        protected override void CancelTasks()
        {
            base.CancelTasks();

            _queuedToHarvest = false;
        }
    }
}
