using System;
using System.Collections.Generic;
using AI;
using Characters;
using Managers;
using ScriptableObjects;
using Sirenix.OdinInspector;
using Systems.Crafting.Scripts;
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
        
        [ShowInInspector] public CraftingTableData RuntimeTableData => RuntimeData as CraftingTableData;

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

        public override void LoadData(FurnitureData data)
        {
            HideCraftingPreview();
            base.LoadData(data);

            if (RuntimeTableData.CurrentOrder?.State == CraftingOrder.EOrderState.Claimed)
            {
                ShowCraftingPreview(RuntimeTableData.CurrentOrder.GetOrderedItemSettings.ItemSprite);
            }
        }

        public override void InitializeFurniture(FurnitureSettings furnitureSettings, PlacementDirection direction, DyeSettings dye)
        {
            var tableSettings = (CraftingTableSettings) furnitureSettings;
            
            // _dyeOverride = dye;
            RuntimeData = new CraftingTableData();
            RuntimeTableData.InitData(tableSettings);
            RuntimeData.Direction = direction;
            
            SetState(RuntimeData.FurnitureState);
            AssignDirection(direction);
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
            if (IsAvailable && (RuntimeTableData.CurrentOrder == null || RuntimeTableData.CurrentOrder.State == CraftingOrder.EOrderState.None))
            {
                CraftingOrder order = RuntimeTableData.LocalCraftingQueue.GetNextCraftableOrder(RuntimeTableData);
          
                if (order != null)
                {
                    RuntimeTableData.CurrentOrderID = order.UniqueID;
                    var task = order.CreateTask(this);
                    TasksDatabase.Instance.AddTask(task);
                    order.State = CraftingOrder.EOrderState.Claimed;
                    InformChanged();
                }
            }
        }

        public override void OnTaskComplete(Task task, bool success)
        {
            base.OnTaskComplete(task, success);

            if (task.TaskID == "Craft Item")
            {
                if (success)
                {
                    RuntimeTableData.CurrentOrder.SetOrderState(CraftingOrder.EOrderState.Completed);
            
                    AssignItemToTable(null, null);
                    InformChanged();
                }
            }
        }

        public override void OnTaskCancelled(Task task) 
        {
            base.OnTaskCancelled(task);

            if (task.TaskID == "Craft Item")
            {
                ShowCraftingPreview(null);
            }
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
        
        public void AssignItemToTable(CraftedItemSettings craftedItem, List<string> claimedMatsUIDs)
        {
            if (craftedItem != null)
            {
                ShowCraftingPreview(craftedItem.ItemSprite);
                RuntimeTableData.CurrentOrder.ClaimedItemUIDs = claimedMatsUIDs;
            }
            else
            {
                ShowCraftingPreview(null);
                RuntimeTableData.CurrentOrder.ClaimedItemUIDs = null;
                RuntimeTableData.CurrentOrderID = null;
            }
            
            InformChanged();
        }

        public void AssignMealToTable(MealSettings mealSettings, List<string> claimedIngredientsUIDs)
        {
            if (mealSettings != null)
            {
                ShowCraftingPreview(mealSettings.ItemSprite);
                RuntimeTableData.CurrentOrder.ClaimedItemUIDs = claimedIngredientsUIDs;
            }
            else
            {
                ShowCraftingPreview(null);
                RuntimeTableData.CurrentOrder.ClaimedItemUIDs = null;
            }
            InformChanged();
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

        public bool DoCraft(StatsData stats, out float progress)
        {
            var workAmount = stats.GetActionSpeedForSkill(RuntimeTableData.CraftingSkillType(), true);
            RuntimeTableData.CurrentOrder.RemainingCraftingWork -= workAmount;
            
            if (RuntimeTableData.CurrentOrder.RemainingCraftingWork <= 0)
            {
                CompleteCraft();
                progress = 1;
                return true;
            }
            else
            {
                progress = RuntimeTableData.CurrentOrder.OrderProgress;
                return false;
            }
        }

        public void ReceiveMaterial(ItemData item)
        {
            Destroy(item.GetLinkedItem().gameObject);
            RuntimeTableData.CurrentOrder.ReceiveItem(item);
        }

        private void CompleteCraft()
        {
            ShowCraftingPreview(null);
            
            RuntimeTableData.CurrentOrder?.ClearMaterialsReceived();
        }
    }
}
