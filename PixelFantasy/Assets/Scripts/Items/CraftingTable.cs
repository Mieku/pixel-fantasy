using System;
using System.Collections.Generic;
using Data.Dye;
using Data.Item;
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
        
        //public CraftingTableData TableData => Data as CraftingTableData;
        public CraftingTableData RuntimeTableData => RuntimeData as CraftingTableData;

        public override void StartPlanning(FurnitureDataSettings furnitureData, PlacementDirection initialDirection, DyeData dye)
        {
            base.StartPlanning(furnitureData, initialDirection, dye);
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
            if (IsAvailable && RuntimeTableData.CurrentOrder.State == CraftingOrder.EOrderState.None)
            {
                var order = CraftingOrdersManager.Instance.GetNextCraftableOrder(this);
                if (order != null)
                {
                    RuntimeTableData.CurrentOrder = order;
                    var task = order.CreateTask(OnCraftingComplete, this);
                    TaskManager.Instance.AddTask(task);
                }
            }
        }

        private void OnCraftingComplete(Task task)
        {
            RuntimeTableData.CurrentOrder.State = CraftingOrder.EOrderState.None;
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
        
        public void AssignItemToTable(CraftedItemDataSettings craftedItem)
        {
            if (craftedItem != null)
            {
                ShowCraftingPreview(craftedItem.icon);
                RuntimeTableData.ItemBeingCrafted = craftedItem;
                RuntimeTableData.RemainingCraftingWork = craftedItem.CraftRequirements.WorkCost;
                RuntimeTableData.RemainingMaterials = new List<ItemAmount>(craftedItem.CraftRequirements.GetMaterialCosts());
            }
            else
            {
                ShowCraftingPreview(null);
                RuntimeTableData.ItemBeingCrafted = null;
                RuntimeTableData.RemainingCraftingWork = 0;
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
            RuntimeTableData.RemainingCraftingWork -= workAmount;
            
            if (RuntimeTableData.RemainingCraftingWork <= 0)
            {
                CompleteCraft();
                return true;
            }
            
            return false;
        }

        public void ReceiveMaterial(ItemData item)
        {
            foreach (var remainingMaterial in RuntimeTableData.RemainingMaterials)
            {
                if (remainingMaterial.Item == item.Settings)
                {
                    remainingMaterial.Quantity -= 1;
                }
            }
            Destroy(item.LinkedItem.gameObject);
        }

        private void CompleteCraft()
        {
            AssignItemToTable(null);
        }

        public List<CraftedItemDataSettings> GetCraftableItems()
        {
            return RuntimeTableData.CraftingTableSettings.CraftableItems;
        }
    }
}
