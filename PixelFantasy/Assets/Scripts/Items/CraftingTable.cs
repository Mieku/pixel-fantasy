using System;
using System.Collections.Generic;
using Characters;
using Managers;
using ScriptableObjects;
using Sirenix.OdinInspector;
using Systems.Crafting.Scripts;
using TaskSystem;
using UnityEngine;
using UnityEngine.Rendering;

namespace Items
{
    public class CraftingTable : Furniture
    {
        [TitleGroup("South")] [SerializeField] private SpriteRenderer _southCraftingPreview;
        [TitleGroup("West")] [SerializeField] private SpriteRenderer _westCraftingPreview;
        [TitleGroup("North")] [SerializeField] private SpriteRenderer _northCraftingPreview;
        [TitleGroup("East")] [SerializeField] private SpriteRenderer _eastCraftingPreview;
        
        public CraftingTableData RuntimeTableData => RuntimeData as CraftingTableData;

        public override void StartPlanning(FurnitureSettings furnitureSettings, PlacementDirection initialDirection, DyeSettings dye)
        {
            base.StartPlanning(furnitureSettings, initialDirection, dye);
            HideCraftingPreview();
        }

        public override void CompletePlanning()
        {
            base.CompletePlanning();
            HideCraftingPreview();
        }

        protected override void InProduction_Enter()
        {
            base.InProduction_Enter();
        }

        protected override void Built_Enter()
        {
            base.Built_Enter();
        }

        protected override void Update()
        {
            base.Update();

            if (!_isPlanning)
            {
                SearchForCraftingOrder();
            }
        }
        
        private void SearchForCraftingOrder()
        {
            if (IsAvailable && RuntimeTableData.CurrentOrder?.State == CraftingOrder.EOrderState.None)
            {
                CraftingOrder order = RuntimeTableData.LocalCraftingQueue.GetNextCraftableOrder(RuntimeTableData);
          
                if (order != null)
                {
                    RuntimeTableData.CurrentOrder = order;
                    var task = order.CreateTask(OnCraftingComplete, OnCraftingCancelled, this);
                    TaskManager.Instance.AddTask(task);
                    RuntimeTableData.CurrentOrder.State = CraftingOrder.EOrderState.Claimed;
                    OnChanged?.Invoke();
                }
            }
        }

        private void OnCraftingComplete(Task task)
        {
            if (RuntimeTableData.CurrentOrder.FulfillmentType == CraftingOrder.EFulfillmentType.Amount)
            {
                RuntimeTableData.CurrentOrder.Amount = Mathf.Clamp(RuntimeTableData.CurrentOrder.Amount - 1, 0, 999);
            }
            
            RuntimeTableData.CurrentOrder.State = CraftingOrder.EOrderState.None;
            RuntimeTableData.CurrentOrder.RefreshOrderRequirements();
            OnChanged?.Invoke();
        }

        private void OnCraftingCancelled()
        {
            RuntimeTableData.CurrentOrder.State = CraftingOrder.EOrderState.None;
            AssignItemToTable(null, null);
        }

        private SpriteRenderer CraftingPreview
        {
            get
            {
                switch (_direction)
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
        
        public void AssignItemToTable(CraftedItemSettings craftedItem, List<ItemData> claimedMats)
        {
            if (craftedItem != null)
            {
                ShowCraftingPreview(craftedItem.ItemSprite);
                RuntimeTableData.CurrentOrder.ClaimedItems = claimedMats;
            }
            else
            {
                ShowCraftingPreview(null);
                RuntimeTableData.CurrentOrder.ClaimedItems = null;
            }
            
            OnChanged?.Invoke();
        }

        public void AssignMealToTable(MealSettings mealSettings, List<ItemData> claimedIngredients)
        {
            if (mealSettings != null)
            {
                ShowCraftingPreview(mealSettings.ItemSprite);
                RuntimeTableData.CurrentOrder.ClaimedItems = claimedIngredients;
            }
            else
            {
                ShowCraftingPreview(null);
                RuntimeTableData.CurrentOrder.ClaimedItems = null;
            }
            OnChanged?.Invoke();
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

        public bool DoCraft(StatsData stats)
        {
            var workAmount = stats.GetActionSpeedForSkill(RuntimeTableData.CraftingSkillType(), true);
            RuntimeTableData.CurrentOrder.RemainingCraftingWork -= workAmount;
            
            if (RuntimeTableData.CurrentOrder.RemainingCraftingWork <= 0)
            {
                CompleteCraft();
                return true;
            }
            
            return false;
        }

        public void ReceiveMaterial(ItemData item)
        {
            RuntimeTableData.CurrentOrder.ReceiveItem(item);
            Destroy(item.LinkedItem.gameObject);
        }

        private void CompleteCraft()
        {
            AssignItemToTable(null, null);
        }

        public List<CraftedItemSettings> GetCraftableItems()
        {
            return RuntimeTableData.CraftingTableSettings.CraftableItems;
        }
    }
}
