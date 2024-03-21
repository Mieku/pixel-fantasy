using System;
using System.Collections.Generic;
using Databrain.Attributes;
using Items;
using ScriptableObjects;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Data.Resource
{
    public class GrowingResourceData : ResourceData
    {
        public float AgeForNextGrowth => GetGrowthStage().SecsInStage;
        public bool FullyGrown => AgeForNextGrowth == 0f;
        
        // Runtime
        [ExposeToInspector, DatabrainSerialize] public int GrowthIndex;
        [ExposeToInspector, DatabrainSerialize] public float AgeSec;
        [ExposeToInspector, DatabrainSerialize] public float FruitTimer;
        [ExposeToInspector, DatabrainSerialize] public bool ShowingFlowers;
        [ExposeToInspector, DatabrainSerialize] public float RemainingHarvestWork;
        [ExposeToInspector, DatabrainSerialize] public bool HasFruitAvailable;
        
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
            
            RemainingExtractWork = GetWorkToCut();
            RemainingHarvestWork = GrowingResourceSettings.WorkToHarvest;
        }
        
        public GrowthStage GetGrowthStage()
        {
            var stages = GrowingResourceSettings.GrowthStages;

            if (GrowthIndex >= stages.Count) 
                GrowthIndex = stages.Count - 1;
            
            return stages[GrowthIndex];
        }
        
        public int GetWorkToCut()
        {
            return GetGrowthStage().WorkToCut;
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
