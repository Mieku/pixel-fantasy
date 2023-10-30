using System;
using Buildings;
using ScriptableObjects;
using TaskSystem;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Zones;

namespace Items
{
    public class CraftingTable : Furniture
    {
        [SerializeField] private SpriteRenderer _craftingPreview;

        protected float _remainingCraftAmount;

        public bool IsInUse;
        public Task _curTask;
        
        protected override void Start()
        {
            base.Start();
            ShowCraftingPreview(null);
        }

        private void LateUpdate()
        {
            SearchForTask();
        }

        private void SearchForTask()
        {
            if (FurnitureState != EFurnitureState.Built) return;
            if (IsInUse || (_curTask != null && _curTask.TaskId != "")) return;

            if (_parentBuilding is ProductionBuilding prodBuilding)
            {
                _curTask = prodBuilding.CreateProductionTask(this);
            }
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
            _curTask = null;
        }
    }
}
