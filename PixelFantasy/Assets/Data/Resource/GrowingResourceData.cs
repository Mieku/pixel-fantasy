using System;
using System.Collections.Generic;
using Databrain.Attributes;
using Items;
using ScriptableObjects;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Data.Resource
{
    public class GrowingResourceData : ResourceData
    {
        // Settings
        [SerializeField] private List<GrowthStage> _growthStages;
        [SerializeField] private Command _harvestCmd;

        // Optional Fruit
        [BoxGroup("Fruit")] [SerializeField] private bool _hasFruit;
        [BoxGroup("Fruit")][ShowIf("_hasFruit")] [SerializeField] private float _growFruitTime;
        [BoxGroup("Fruit")][ShowIf("_hasFruit")] [SerializeField] private Sprite _fruitOverlay;
        [BoxGroup("Fruit")][ShowIf("_hasFruit")] [SerializeField] private Sprite _fruitFlowersOverlay;
        [BoxGroup("Fruit")][ShowIf("_hasFruit")] [SerializeField] private HarvestableItems _harvestableFruit;
        [BoxGroup("Fruit")][ShowIf("_hasFruit")] [SerializeField] private int _workToHarvest;
        
        // Accessors
        public bool HasFruit => _hasFruit;
        public bool FullyGrown => AgeForNextGrowth == 0f;//AgeSec >= TotalGrowTime();
        public List<GrowthStage> GrowthStages => _growthStages;
        public float GrowFruitTime => _growFruitTime;
        public Sprite FruitOverlay => _fruitOverlay;
        public Sprite FruitFlowersOverlay => _fruitFlowersOverlay;
        public int WorkToHarvest => _workToHarvest;
        public bool HasFruitFlowers => _fruitFlowersOverlay != null;
        public Command HarvestCmd => _harvestCmd;
        public float AgeForNextGrowth => GetGrowthStage().SecsInStage;
        
        // Runtime
        [Foldout("Runtime"), ExposeToInspector, DatabrainSerialize] public int GrowthIndex;
        [Foldout("Runtime"), ExposeToInspector, DatabrainSerialize] public float AgeSec;
        [Foldout("Runtime"), ExposeToInspector, DatabrainSerialize] public float FruitTimer;
        [Foldout("Runtime"), ExposeToInspector, DatabrainSerialize] public bool ShowingFlowers;
        [Foldout("Runtime"), ExposeToInspector, DatabrainSerialize] public float RemainingHarvestWork;
        [Foldout("Runtime"), ExposeToInspector, DatabrainSerialize] public bool HasFruitAvailable;
        
        public override void InitData()
        {
            base.InitData();
            
            GrowthIndex = Random.Range(0, _growthStages.Count);

            float minAgeSec = 0f;
            for (int i = 0; i < GrowthIndex - 1; i++)
            {
                minAgeSec += _growthStages[i].SecsInStage;
            }

            if (FullyGrown)
            {
                AgeSec = Random.Range(minAgeSec, TotalGrowTime() * 1.5f);
            }
            else
            {
                AgeSec = Random.Range(minAgeSec, GetGrowthStage().SecsInStage);
            }
            
            
            if (_hasFruit && AgeSec > TotalGrowTime())
            {
                FruitTimer = Random.Range(0, _growFruitTime * 1.5f);
                HasFruitAvailable = FruitTimer >= _growFruitTime;
            }
            
            RemainingExtractWork = GetWorkToCut();
            RemainingHarvestWork = _workToHarvest;
        }
        
        public GrowthStage GetGrowthStage()
        {
            var stages = _growthStages;

            if (GrowthIndex >= _growthStages.Count) 
                GrowthIndex = _growthStages.Count - 1;
            
            return stages[GrowthIndex];
        }

        public float TotalGrowTime()
        {
            float totalGrowTime = 0;
            foreach (var stage in _growthStages)
            {
                totalGrowTime += stage.SecsInStage;
            }

            return totalGrowTime;
        }

        public int GetWorkToCut()
        {
            return GetGrowthStage().WorkToCut;
        }

        public List<ItemAmount> GetFruitLoot()
        {
            if (_harvestableFruit != null)
            {
                return _harvestableFruit.GetItemDrop();
            }

            return new List<ItemAmount>();
        }
        
        public float GetGrowthPercentage()
        {
            if (FullyGrown) return 1f;
            
            var growthPercent = AgeSec / TotalGrowTime();
            return Mathf.Clamp01(growthPercent);
        }
        
        public float GetFruitingPercentage()
        {
            if (HasFruitAvailable) return 1f;
            
            var percent = FruitTimer / GrowFruitTime;
            return Mathf.Clamp01(percent);
        }
        
        // public float CutWorkDone(float workAmount)
        // {
        //     RemainingCutWork -= workAmount;
        //     return RemainingCutWork;
        // }

        public float HarvestWorkDone(float workAmount)
        {
            RemainingHarvestWork -= workAmount;
            return RemainingHarvestWork;
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
        public int WorkToCut;
    }
}
