using System;
using System.Collections.Generic;
using Data.Resource;
using ScriptableObjects;
using Managers;
using QFSW.QC;
using TaskSystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Items
{
    public class GrowingResource : BasicResource
    {
        [SerializeField] protected SpriteRenderer _fruitOverlay;

        public bool IsFruiting => GrowingResourceData.HasFruit;
        public List<GameObject> TaskRequestors = new List<GameObject>();
        
        public GrowingResourceData RuntimeGrowingResourceData => RuntimeData as GrowingResourceData;
        private GrowingResourceData GrowingResourceData => Data as GrowingResourceData;
        
        public override void Init(ResourceData data)
        {
            base.Init(data);
        }

        protected override void InitialDataReady()
        {
            base.InitialDataReady();
            
            GrowthCheck();
            FruitCheck();
        }

        public override string DisplayName
        {
            get
            {
                if (!RuntimeGrowingResourceData.FullyGrown)
                {
                    var stageName = RuntimeGrowingResourceData.GetGrowthStage().StageName;
                    return $"{Data.title} ({stageName})";
                }

                return Data.title;
            }
        }
        
        protected override void UpdateSprite()
        {
            var stage = RuntimeGrowingResourceData.GetGrowthStage();
            var scaleOverride = stage.Scale;
            _spriteRenderer.sprite = stage.GrowthSprite;
            _spriteRenderer.gameObject.transform.localScale = new Vector3(scaleOverride, scaleOverride, 1);
        }

        public override HarvestableItems GetHarvestableItems()
        {
            var stage = RuntimeGrowingResourceData.GetGrowthStage();
            return stage.HarvestableItems;
        }

        public Sprite GetGrowthIcon()
        {
            return Librarian.Instance.GetSprite("Growth");
        }
        
        protected void GrowthCheck()
        {
            if (RuntimeGrowingResourceData == null) return;
            
            if (!RuntimeGrowingResourceData.FullyGrown)
            {
                RuntimeGrowingResourceData.AgeSec += TimeManager.Instance.DeltaTime;
                if (RuntimeGrowingResourceData.AgeSec >= RuntimeGrowingResourceData.AgeForNextGrowth)
                {
                    var extractDiff = RuntimeGrowingResourceData.RemainingExtractWork - RuntimeGrowingResourceData.GetWorkToCut();
                    
                    RuntimeGrowingResourceData.GrowthIndex++;

                    RuntimeGrowingResourceData.RemainingExtractWork =
                        RuntimeGrowingResourceData.GetWorkToCut() - extractDiff;

                    RefreshSelection();
                    UpdateSprite();
                }
            }
        }

        [Command("instant_fruit", MonoTargetType.All)]
        private void CMD_InstantFruit()
        {
            if (!RuntimeGrowingResourceData.HasFruitAvailable)
            {
                RuntimeGrowingResourceData.FruitTimer = RuntimeGrowingResourceData.GrowFruitTime;
            }
        }

        public Sprite GetFruitIcon()
        {
            if (!IsFruiting) return null;
            
            return RuntimeGrowingResourceData.GetFruitLoot()[0].Item.ItemSprite;
        }

        protected void FruitCheck()
        {
            if (RuntimeGrowingResourceData == null) return;
            
            if (!RuntimeGrowingResourceData.FullyGrown) return;
            
            if (RuntimeGrowingResourceData.HasFruit && !RuntimeGrowingResourceData.HasFruitAvailable)
            {
                RuntimeGrowingResourceData.FruitTimer += TimeManager.Instance.DeltaTime;
                if (RuntimeGrowingResourceData.FruitTimer >= RuntimeGrowingResourceData.GrowFruitTime)
                {
                    RuntimeGrowingResourceData.FruitTimer = 0;
                    _fruitOverlay.sprite = RuntimeGrowingResourceData.FruitOverlay;
                    _fruitOverlay.gameObject.SetActive(true);
                    RuntimeGrowingResourceData.HasFruitAvailable = true;
                    RefreshSelection();
                } 
                else if (RuntimeGrowingResourceData.FruitTimer >= RuntimeGrowingResourceData.GrowFruitTime / 2f)
                {
                    if (!RuntimeGrowingResourceData.ShowingFlowers && RuntimeGrowingResourceData.HasFruitFlowers)
                    {
                        RuntimeGrowingResourceData.ShowingFlowers = true;
                        _fruitOverlay.sprite = RuntimeGrowingResourceData.FruitFlowersOverlay;
                        _fruitOverlay.gameObject.SetActive(true);
                    }
                }
            }

            if (RuntimeGrowingResourceData.HasFruitAvailable)
            {
                _fruitOverlay.sprite = RuntimeGrowingResourceData.FruitOverlay;
                _fruitOverlay.gameObject.SetActive(true);
            }
        }

        public void HarvestFruit()
        {
            if (RuntimeGrowingResourceData.HasFruitAvailable)
            {
                _fruitOverlay.gameObject.SetActive(false);
                List<ItemAmount> fruits = RuntimeGrowingResourceData.GetFruitLoot();
                foreach (var fruit in fruits)
                {
                    for (int i = 0; i < fruit.Quantity; i++)
                    {
                        spawner.SpawnItem(fruit.Item, transform.position, true);
                    }
                }
                RuntimeGrowingResourceData.HasFruitAvailable = false;
                RefreshSelection();
                DisplayTaskIcon(null);

                if (PendingCommand == RuntimeGrowingResourceData.HarvestCmd)
                {
                    PendingCommand = null;
                }
            }

            RuntimeGrowingResourceData.RemainingHarvestWork = RuntimeGrowingResourceData.WorkToHarvest;
        }

        public bool DoHarvest(float workAmount)
        {
            RuntimeGrowingResourceData.RemainingHarvestWork -= workAmount;
            if (RuntimeGrowingResourceData.RemainingHarvestWork <= 0)
            {
                HarvestFruit();
                return true;
            }
            
            return false;
        }

        public override List<Command> GetCommands()
        {
            var result = new List<Command>(Commands);
            if (RuntimeGrowingResourceData.HasFruitAvailable)
            {
                result.Add(RuntimeGrowingResourceData.HarvestCmd);
            }

            return result;
        }

        private void Update()
        {
            GrowthCheck();
            FruitCheck();
        }

        protected override void ExtractResource()
        {
            HarvestFruit();
            
            var resources = RuntimeGrowingResourceData.GetGrowthStage().HarvestableItems.GetItemDrop();
            foreach (var resource in resources)
            {
                for (int i = 0; i < resource.Quantity; i++)
                {
                    spawner.SpawnItem(resource.Item, transform.position, true);
                }
            }
            
            RefreshSelection();
            
            Destroy(gameObject);
            
            if(_onResourceClearedCallback != null) _onResourceClearedCallback.Invoke();
            
            //base.ExtractResource();
        }
        
        public override float GetWorkAmount()
        {
            return RuntimeGrowingResourceData.GetWorkToCut();
        }
    }
}
