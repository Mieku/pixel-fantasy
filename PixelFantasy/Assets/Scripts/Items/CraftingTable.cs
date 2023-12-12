using System;
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

        private void SearchForTask()
        {
            // if (FurnitureState != EFurnitureState.Built) return;
            // if (IsInUse || (_curTask != null && _curTask.TaskId != "")) return;
            //
            // if (_parentBuilding is CraftingBuilding craftingBuilding)
            // {
            //     _curTask = craftingBuilding.CreateProductionTask(this);
            // }
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
