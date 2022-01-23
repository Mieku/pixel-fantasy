using System;
using Gods;
using ScriptableObjects;
using UnityEngine;

namespace Items
{
    public class TreeResource : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private SpriteRenderer _icon;
        [SerializeField] private ResourceData _resourceData;
        [SerializeField] private bool _overrideFullGrowth;
        
        private TaskMaster taskMaster => TaskMaster.Instance;
        private int _growthIndex;
        private float _ageSec;
        private float _ageForNextGrowth;
        private bool _fullyGrown;
        private float _reproductionTimer;

        private void Awake()
        {
            if (_overrideFullGrowth)
            {
                _fullyGrown = true;
                _growthIndex = _resourceData.GrowthStages.Count - 1;
            }
            
            var stage = _resourceData.GetGrowthStage(_growthIndex);
            _ageForNextGrowth += stage.SecsInStage;
            _reproductionTimer = _resourceData.ReproductiveRateSec;

            UpdateSprite();
        }

        private void Update()
        {
            GrowthCheck();
            ReproductionCheck();
        }

        private void GrowthCheck()
        {
            if (!_fullyGrown)
            {
                _ageSec += Time.deltaTime;
                if (_ageSec >= _ageForNextGrowth)
                {
                    _growthIndex++;

                    if (_growthIndex < _resourceData.GrowthStages.Count)
                    {
                        var stage = _resourceData.GetGrowthStage(_growthIndex);
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

        private void ReproductionCheck()
        {
            if (_fullyGrown && _resourceData.Reproduces)
            {
                _reproductionTimer -= Time.deltaTime;
                if (_reproductionTimer < 0)
                {
                    _reproductionTimer = _resourceData.ReproductiveRateSec;
                    AttemptReproduction();
                }
            }
        }

        private void UpdateSprite()
        {
            var stage = _resourceData.GetGrowthStage(_growthIndex);
            _spriteRenderer.sprite = stage.GrowthSprite;
        }

        private void AttemptReproduction()
        {
            var pos = _resourceData.GetReproductionPos(transform.position);
            var valid = _resourceData.IsReproductionPosValid(pos);
            if (valid)
            {
                Spawner.Instance.SpawnTree(pos);
            }
        }
    }
}
