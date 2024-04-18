using System.Collections.Generic;
using Items;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Data.Resource
{
    public class GrowingResourceSettings : ResourceSettings
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
        public List<GrowthStage> GrowthStages => _growthStages;
        public float GrowFruitTime => _growFruitTime;
        public Sprite FruitOverlay => _fruitOverlay;
        public Sprite FruitFlowersOverlay => _fruitFlowersOverlay;
        public int WorkToHarvest => _workToHarvest;
        public bool HasFruitFlowers => _fruitFlowersOverlay != null;
        public Command HarvestCmd => _harvestCmd;
        public HarvestableItems HarvestableFruit => _harvestableFruit.Clone();
        
        public float TotalGrowTime()
        {
            float totalGrowTime = 0;
            foreach (var stage in GrowthStages)
            {
                totalGrowTime += stage.SecsInStage;
            }

            return totalGrowTime;
        }
    }
}
