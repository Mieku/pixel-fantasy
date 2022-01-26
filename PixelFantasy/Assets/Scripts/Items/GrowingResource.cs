using ScriptableObjects;
using Tasks;
using Unit;
using UnityEngine;

namespace Items
{
    public class GrowingResource : Resource
    {
        [SerializeField] protected bool _overrideFullGrowth;
        
        protected int _growthIndex;
        protected float _ageSec;
        protected float _ageForNextGrowth;
        protected bool _fullyGrown;
        protected float _reproductionTimer;
        
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
        
        private void Update()
        {
            GrowthCheck();
            ReproductionCheck();
        }
        
        public void CreateCutPlantTask()
        {
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

            var task = new FarmingTask.CutPlant()
            {
                claimPlant = (UnitTaskAI unitTaskAI) =>
                {
                    _incomingUnit = unitTaskAI;
                },
                plantPosition = cutPos,
                workAmount = _growingResourceData.GetWorkToCut(_growthIndex),
                completeWork = HarvestPlant
            };
            
            _assignedTaskRefs.Add(task.GetHashCode());
            taskMaster.FarmingTaskSystem.AddTask(task);
        }

        public void CancelCutPlantTask()
        {
            _queuedToHarvest = false;
            SetIcon(null);
            CancelTasks();
        }
        
        private void HarvestPlant()
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
    }
}
