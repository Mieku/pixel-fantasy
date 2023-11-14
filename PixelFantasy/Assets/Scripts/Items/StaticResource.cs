using System;
using ScriptableObjects;
using UnityEngine;

namespace Items
{
    public class StaticResource : Resource
    {
        private StaticResourceData _staticResourceData => ResourceData as StaticResourceData;
        private int _spriteIndex;
        
        protected override void Awake()
        {
            base.Awake();
            if (ResourceData != null)
            {
                Init();
            }
        }
        
        private void Init()
        {
            UpdateSprite();
            Health = GetWorkAmount();
        }

        private void OnValidate()
        {
            if (ResourceData != null)
            {
                UpdateSprite();
            }
        }

        public override int GetWorkAmount()
        {
            return _staticResourceData.WorkToExtract;
        }

        public override float MinWorkDistance => 0.75f;

        protected void UpdateSprite()
        {
            _spriteIndex = _staticResourceData.GetRandomSpriteIndex();
            _spriteRenderer.sprite = _staticResourceData.GetSprite(_spriteIndex);
        }
        
        public override HarvestableItems GetHarvestableItems()
        {
            return _staticResourceData.HarvestableItems;
        }

        protected override void DestroyResource()
        {
            var resources = _staticResourceData.HarvestableItems.GetItemDrop();
            foreach (var resource in resources)
            {
                for (int i = 0; i < resource.Quantity; i++)
                {
                    spawner.SpawnItem(resource.Item, transform.position, true);
                }
            }
            
            RefreshSelection();
            
            Destroy(gameObject);
        }
    }
}
