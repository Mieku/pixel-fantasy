using System;
using UnityEngine;

namespace Items
{
    public class FurniturePrefab : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _furnitureSprite;
        [SerializeField] private Transform _usagePosition;
        [SerializeField] private SpriteRenderer _craftedItemRenderer;

        public Transform UsagePostion => _usagePosition;
        public SpriteRenderer FurnitureRenderer => _furnitureSprite;
        public SpriteRenderer CraftedItemRenderer => _craftedItemRenderer;

        private void Start()
        {
            ShowItemRenderer(false);
        }

        public void ShowItemRenderer(bool show)
        {
            if (_craftedItemRenderer != null)
            {
                _craftedItemRenderer.gameObject.SetActive(show);
            }
        }
    }
}
