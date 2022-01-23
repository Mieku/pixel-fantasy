using System;
using System.Collections.Generic;
using Items;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "ResourceData", menuName = "ResourceData", order = 1)]
    public class ResourceData : ScriptableObject
    {
        public string ResourceName;

        [SerializeField] private bool _reproduces;
        [SerializeField] private float _reproductiveRateSec;
        [SerializeField] private float _childRangeMin, _childRangeMax;
        [SerializeField] private List<GrowthStage> _growthStages;
        [SerializeField] private List<Option> _options;
        [SerializeField] private List<string> _invalidPlacementTags;

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
                pos,
                new Vector2(pos.x + 1, pos.y),
                new Vector2(pos.x - 1, pos.y),
                new Vector2(pos.x, pos.y + 1),
                new Vector2(pos.x, pos.y - 1),
                new Vector2(pos.x + 1, pos.y + 1),
                new Vector2(pos.x - 1, pos.y + 1),
                new Vector2(pos.x + 1, pos.y - 1),
                new Vector2(pos.x - 1, pos.y + 1)
            };

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
        
        public List<Option> Options
        {
            get
            {
                List<Option> clone = new List<Option>();
                foreach (var option in _options)
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
    }

    [Serializable]
    public class GrowthStage
    {
        public string StageName;
        public Sprite GrowthSprite;
        public float SecsInStage;
        public HarvestableItems HarvestableItems;
    }
}
