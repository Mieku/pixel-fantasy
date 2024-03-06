using System;
using ScriptableObjects;
using UnityEngine;

namespace Items
{
    public class StaticResource : Resource
    {
        private StaticResourceSettings _staticResourceSettings => ResourceSettings as StaticResourceSettings;
        private int _spriteIndex;
        
        protected override void Awake()
        {
            base.Awake();
            // if (ResourceData != null)
            // {
            //     Init();
            // }
        }

        public override void Init(ResourceSettings settings)
        {
            base.Init(settings);
            
            UpdateSprite();
            Health = GetWorkAmount();
        }

        // private void Init()
        // {
        //     UpdateSprite();
        //     Health = GetWorkAmount();
        // }
        
        public override float GetWorkAmount()
        {
            return _staticResourceSettings.WorkToExtract;
        }

        public override float MinWorkDistance => 0.75f;

        protected void UpdateSprite()
        {
            _spriteIndex = _staticResourceSettings.GetRandomSpriteIndex();
            _spriteRenderer.sprite = _staticResourceSettings.GetSprite(_spriteIndex);
        }
        
        public override HarvestableItems GetHarvestableItems()
        {
            return _staticResourceSettings.HarvestableItems;
        }

        protected override void DestroyResource()
        {
            var resources = _staticResourceSettings.HarvestableItems.GetItemDrop();
            foreach (var resource in resources)
            {
                for (int i = 0; i < resource.Quantity; i++)
                {
                    spawner.SpawnItem(resource.Item, transform.position, true);
                }
            }
            
            RefreshSelection();
            
            base.DestroyResource();
        }
    }
}
