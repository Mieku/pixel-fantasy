using System;
using ScriptableObjects;
using UnityEngine;

namespace Items
{
    public class CraftingTable : Furniture
    {
        [SerializeField] private SpriteRenderer _craftingPreview;

        protected float _remainingCraftAmount;

        public bool IsInUse;
        
        protected override void Start()
        {
            base.Start();
            ShowCraftingPreview(null);
        }

        public void AssignItemToTable(CraftedItemData craftedItem)
        {
            if (craftedItem != null)
            {
                IsInUse = true;
                ShowCraftingPreview(craftedItem.ItemSprite);
                _remainingCraftAmount = craftedItem.WorkCost;
            }
            else
            {
                IsInUse = false;
                ShowCraftingPreview(null);
                _remainingCraftAmount = 0;
            }
        }

        public void ShowCraftingPreview(Sprite craftingImg)
        {
            if (craftingImg == null)
            {
                _craftingPreview.gameObject.SetActive(false);
            }
            else
            {
                _craftingPreview.sprite = craftingImg;
                _craftingPreview.gameObject.SetActive(true);
            }
        }

        public bool DoCraft(float workAmount)
        {
            _remainingCraftAmount -= workAmount;
            if (_remainingCraftAmount <= 0)
            {
                CompleteCraft();
                return true;
            }
            
            return false;
        }

        private void CompleteCraft()
        {
            AssignItemToTable(null);
        }
    }
}
