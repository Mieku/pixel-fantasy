using System;
using System.Collections.Generic;
using Actions;
using Items;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "GrowingResourceData", menuName = "ResourceData/GrowingResourceData", order = 1)]
    public class GrowingResourceData : ScriptableObject
    {
        public string ResourceName;

        [SerializeField] private bool _reproduces;
        [SerializeField] private float _reproductiveRateSec;
        [SerializeField] private float _childRangeMin, _childRangeMax;
        [Tooltip("Don't grow next to invalid")][SerializeField] private bool _keepSpace;
        [SerializeField] private List<GrowthStage> _growthStages;
        [SerializeField] private List<string> _invalidPlacementTags;
        [SerializeField] private List<ActionBase> _availableActions;
        
        // Optional Fruit
        [BoxGroup("Fruit")] [SerializeField] private bool _hasFruit;
        [BoxGroup("Fruit")][ShowIf("_hasFruit")] [SerializeField] private float _growFruitTime;
        [BoxGroup("Fruit")][ShowIf("_hasFruit")] [SerializeField] private Sprite _fruitOverlay;
        [BoxGroup("Fruit")][ShowIf("_hasFruit")] [SerializeField] private Sprite _fruitFlowersOverlay;
        [BoxGroup("Fruit")][ShowIf("_hasFruit")] [SerializeField] private HarvestableItems _harvestableFruit;
        [BoxGroup("Fruit")][ShowIf("_hasFruit")] [SerializeField] private int _workToHarvest;

        public List<ActionBase> AvailableActions => _availableActions;
        
        public Vector2 GetReproductionPos(Vector2 parentPos)
        {
            var distance = Random.Range(_childRangeMin, _childRangeMax);
            
            var angle = Random.Range(0f, 360f);
            // convert to rads
            angle = angle * Mathf.PI / 180;

            var yPos = Mathf.Sin(angle) * distance;
            var xPos = Mathf.Cos(angle) * distance;

            Vector2 result = Helper.ConvertMousePosToGridPos(new Vector2(xPos + parentPos.x, yPos + parentPos.y));
            return result;
        }

        public bool IsReproductionPosValid(Vector2 pos)
        {
            List<Vector2> allPos = new List<Vector2>
            {
                pos
            };

            if (_keepSpace)
            {
                allPos = new List<Vector2>
                {
                    new Vector2(pos.x + 1, pos.y),
                    new Vector2(pos.x - 1, pos.y),
                    new Vector2(pos.x, pos.y + 1),
                    new Vector2(pos.x, pos.y - 1),
                    new Vector2(pos.x + 1, pos.y + 1),
                    new Vector2(pos.x - 1, pos.y + 1),
                    new Vector2(pos.x + 1, pos.y - 1),
                    new Vector2(pos.x - 1, pos.y - 1)
                };
            }

            foreach (var posToCheck in allPos)
            {
                if (!Helper.IsGridPosValidToBuild(posToCheck, _invalidPlacementTags))
                {
                    return false;
                }
            }

            return true;
        }
        
        public bool Reproduces => _reproduces;
        public float ReproductiveRateSec => _reproductiveRateSec;

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
