using System;
using System.Collections.Generic;
using Items;
using Newtonsoft.Json;
using UnityEngine;

namespace DataPersistence
{
    [Serializable]
    public class GrowingResourceData : BasicResourceData
    {
        [JsonIgnore]
        public float AgeForNextGrowth
        {
            get
            {
                float result = 0;
                for (int i = GrowthIndex; i >= 0; i--)
                {
                    result += GrowingResourceSettings.GrowthStages[i].SecsInStage;
                }
                return result;
            }
        }

        [JsonIgnore]
        public bool FullyGrown => AgeSec >= GrowingResourceSettings.TotalGrowTime();

        // Runtime
        public int GrowthIndex;
        public float AgeSec;
        public float FruitTimer;
        public bool ShowingFlowers;
        public float RemainingHarvestWork;
        public bool HasFruitAvailable;

        [JsonIgnore]
        public GrowingResourceSettings GrowingResourceSettings => Settings as GrowingResourceSettings;
        
        [JsonIgnore]
        public override ResourceSettings Settings => Resources.Load<GrowingResourceSettings>($"Settings/Resource/Growing Resources/{SettingsName}");

        public override void InitData(ResourceSettings settings)
        {
            base.InitData(settings);

            if (GrowingResourceSettings == null)
            {
                Debug.LogError("Settings are not of type GrowingResourceSettings.");
                return;
            }

            GrowthIndex = UnityEngine.Random.Range(0, GrowingResourceSettings.GrowthStages.Count);

            float minAgeSec = 0f;
            for (int i = 0; i < GrowthIndex - 1; i++)
            {
                minAgeSec += GrowingResourceSettings.GrowthStages[i].SecsInStage;
            }

            if (FullyGrown)
            {
                AgeSec = UnityEngine.Random.Range(minAgeSec, GrowingResourceSettings.TotalGrowTime() * 1.5f);
            }
            else
            {
                AgeSec = UnityEngine.Random.Range(minAgeSec, GetGrowthStage().SecsInStage);
            }

            if (GrowingResourceSettings.HasFruit && AgeSec > GrowingResourceSettings.TotalGrowTime())
            {
                FruitTimer = UnityEngine.Random.Range(0, GrowingResourceSettings.GrowFruitTime * 1.5f);
                HasFruitAvailable = FruitTimer >= GrowingResourceSettings.GrowFruitTime;
            }

            Health = GetGrowthStage().Health;
            RemainingHarvestWork = GrowingResourceSettings.WorkToHarvest;
        }

        public override float MaxHealth => GetGrowthStage().Health;

        public GrowthStage GetGrowthStage()
        {
            var stages = GrowingResourceSettings.GrowthStages;

            if (GrowthIndex >= stages.Count)
                GrowthIndex = stages.Count - 1;

            return stages[GrowthIndex];
        }

        public int GetWorkToCut()
        {
            return GetGrowthStage().Health;
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
}