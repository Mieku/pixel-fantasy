using System;
using System.Collections.Generic;
using Buildings;
using Managers;
using ScriptableObjects;
using Sirenix.OdinInspector;
using TaskSystem;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering;
using Zones;

namespace Items
{
    public class CraftingTable : Furniture
    {
        [TitleGroup("South")] [SerializeField] private SpriteRenderer _southCraftingPreview;
        [TitleGroup("West")] [SerializeField] private SpriteRenderer _westCraftingPreview;
        [TitleGroup("North")] [SerializeField] private SpriteRenderer _northCraftingPreview;
        [TitleGroup("East")] [SerializeField] private SpriteRenderer _eastCraftingPreview;

        private CraftedItemData _craftedItem;
        protected float _remainingCraftAmount;
        private List<ItemAmount> _remainingMaterials;

        public bool IsInUse;
        public Task _curTask;
        
        protected override void Start()
        {
            base.Start();
            ShowCraftingPreview(null);
        }

        private SpriteRenderer CraftingPreview
        {
            get
            {
                switch (CurrentDirection)
                {
                    case PlacementDirection.South:
                        return _southCraftingPreview;
                    case PlacementDirection.North:
                        return _northCraftingPreview;
                    case PlacementDirection.West:
                        return _westCraftingPreview;
                    case PlacementDirection.East:
                        return _eastCraftingPreview;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void HideCraftingPreview()
        {
            if(_southCraftingPreview != null) _southCraftingPreview.gameObject.SetActive(false);
            if(_northCraftingPreview != null) _northCraftingPreview.gameObject.SetActive(false);
            if(_westCraftingPreview != null) _westCraftingPreview.gameObject.SetActive(false);
            if(_eastCraftingPreview != null) _eastCraftingPreview.gameObject.SetActive(false);
        }
        
        public void AssignItemToTable(CraftedItemData craftedItem)
        {
            if (craftedItem != null)
            {
                _craftedItem = craftedItem;
                IsInUse = true;
                ShowCraftingPreview(craftedItem.ItemSprite);
                _remainingCraftAmount = craftedItem.WorkCost;
                _remainingMaterials = new List<ItemAmount>(craftedItem.GetResourceCosts());
            }
            else
            {
                _craftedItem = null;
                IsInUse = false;
                ShowCraftingPreview(null);
                _remainingCraftAmount = 0;
            }
        }

        public void ShowCraftingPreview(Sprite craftingImg)
        {
            HideCraftingPreview();
            
            if (craftingImg == null)
            {
                CraftingPreview.gameObject.SetActive(false);
            }
            else
            {
                CraftingPreview.sortingOrder = SpritesRoot().GetComponent<SortingGroup>().sortingOrder + 1;
                CraftingPreview.sprite = craftingImg;
                CraftingPreview.gameObject.SetActive(true);
            }
        }

        public bool DoCraft(float workAmount)
        {
            _remainingCraftAmount -= workAmount;
            
            if (_parentBuilding != null)
            {
                GameEvents.Trigger_OnBuildingChanged(_parentBuilding);
            }
            
            if (_remainingCraftAmount <= 0)
            {
                CompleteCraft();
                return true;
            }
            
            return false;
        }

        public float GetPercentCraftingComplete()
        {
            if (_craftedItem == null) return 0f;
            
            return 1 - (_remainingCraftAmount / _craftedItem.WorkCost);
        }

        public void ReceiveMaterial(Item item)
        {
            foreach (var remainingMaterial in _remainingMaterials)
            {
                if (remainingMaterial.Item == item.GetItemData())
                {
                    remainingMaterial.Quantity -= 1;
                }
            }
            Destroy(item.gameObject);
            
            // Refresh building
            if (_parentBuilding != null)
            {
                GameEvents.Trigger_OnBuildingChanged(_parentBuilding);
            }
        }

        public float GetPercentMaterialsReceived()
        {
            if (_craftedItem == null) return 0f;
            
            int numItemsNeeded = 0;
            foreach (var cost in _craftedItem.GetResourceCosts())
            {
                numItemsNeeded += cost.Quantity;
            }

            int numItemsRemaining = 0;
            foreach (var remaining in _remainingMaterials)
            {
                numItemsRemaining += remaining.Quantity;
            }
            
            if (numItemsNeeded == 0)
            {
                return 1f;
            }
            else
            {
                return 1f - (numItemsRemaining / (float)numItemsNeeded);
            }
        }

        private void CompleteCraft()
        {
            AssignItemToTable(null);
            _curTask = null;
        }
    }
}
