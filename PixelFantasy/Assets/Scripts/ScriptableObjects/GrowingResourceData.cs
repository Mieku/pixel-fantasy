using System;
using System.Collections.Generic;
using Items;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "ResourceData", menuName = "ResourceData/GrowingResourceData", order = 1)]
    public class GrowingResourceData : ResourceData
    {
        [SerializeField] private List<GrowthStage> _growthStages;
        [SerializeField] private List<string> _invalidPlacementTags;

        // Optional Fruit
        [BoxGroup("Fruit")] [SerializeField] private bool _hasFruit;
        [BoxGroup("Fruit")][ShowIf("_hasFruit")] [SerializeField] private float _growFruitTime;
        [BoxGroup("Fruit")][ShowIf("_hasFruit")] [SerializeField] private Sprite _fruitOverlay;
        [BoxGroup("Fruit")][ShowIf("_hasFruit")] [SerializeField] private Sprite _fruitFlowersOverlay;
        [BoxGroup("Fruit")][ShowIf("_hasFruit")] [SerializeField] private HarvestableItems _harvestableFruit;
        [BoxGroup("Fruit")][ShowIf("_hasFruit")] [SerializeField] private int _workToHarvest;

        public List<GrowthStage> GrowthStages
        {
            get
            {
                List<GrowthStage> clone = new List<GrowthStage>();
                foreach (var option in _growthStages)
                {
                    clone.Add(option);
                }

                return clone;
            }
        }
        
        public GrowthStage GetGrowthStage(int growthIndex)
        {
            var stages = GrowthStages;

            if (growthIndex >= GrowthStages.Count) 
                growthIndex = GrowthStages.Count - 1;
            
            return stages[growthIndex];
        }

        public int GetWorkToCut(int growthIndex)
        {
            return GetGrowthStage(growthIndex).WorkToCut;
        }

        public bool HasFruit => _hasFruit;
        public float TimeToGrowFruit => _growFruitTime;
        public Sprite FruitOverlay => _fruitOverlay;
        public Sprite FruitFlowersOverlay => _fruitFlowersOverlay;
        public int WorkToHarvest => _workToHarvest;
        public bool HasFruitFlowers => _fruitFlowersOverlay != null;

        public List<ItemAmount> GetFruitLoot()
        {
            if (_harvestableFruit != null)
            {
                return _harvestableFruit.GetItemDrop();
            }

            return new List<ItemAmount>();
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
