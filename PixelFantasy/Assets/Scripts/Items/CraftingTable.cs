using System;
using System.Collections.Generic;
using Managers;
using ScriptableObjects;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;

namespace Items
{
    public class CraftingTable : Furniture, IFurnitureInitializable
    {
        [TitleGroup("South")] [SerializeField] private SpriteRenderer _southCraftingPreview;
        [TitleGroup("West")] [SerializeField] private SpriteRenderer _westCraftingPreview;
        [TitleGroup("North")] [SerializeField] private SpriteRenderer _northCraftingPreview;
        [TitleGroup("East")] [SerializeField] private SpriteRenderer _eastCraftingPreview;
        
        public CraftingTableData TableData => Data as CraftingTableData;
        
        public new bool Init(FurnitureSettings settings, FurnitureVarient varient = null, DyeSettings dye = null)
        {
            if (settings is CraftingTableSettings craftingTableSettings)
            {
                Data = new CraftingTableData(craftingTableSettings, varient, dye);
                
                AssignDirection(Data.Direction);
                foreach (var spriteRenderer in _allSprites)
                {
                    _materials.Add(spriteRenderer.material);
                }
                
                return true; // Initialization successful
            }
            else
            {
                Debug.LogError("Invalid settings type provided.");
                return false; // Initialization failed
            }
        }

        protected override void Planning_Enter()
        {
            base.Planning_Enter();
            HideCraftingPreview();
        }

        private SpriteRenderer CraftingPreview
        {
            get
            {
                switch (Data.Direction)
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
        
        public void AssignItemToTable(CraftedItemSettings craftedItem)
        {
            if (craftedItem != null)
            {
                ShowCraftingPreview(craftedItem.ItemSprite);
                TableData.ItemBeingCrafted = craftedItem;
                TableData.RemainingCraftingWork = craftedItem.CraftRequirements.WorkCost;
                TableData.RemainingMaterials = new List<ItemAmount>(craftedItem.CraftRequirements.GetResourceCosts());
            }
            else
            {
                ShowCraftingPreview(null);
                TableData.ItemBeingCrafted = null;
                TableData.RemainingCraftingWork = 0;
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
            TableData.RemainingCraftingWork -= workAmount;
            
            if (TableData.RemainingCraftingWork <= 0)
            {
                CompleteCraft();
                return true;
            }
            
            return false;
        }

        public void ReceiveMaterial(Item item)
        {
            foreach (var remainingMaterial in TableData.RemainingMaterials)
            {
                if (remainingMaterial.Item == item.GetItemData())
                {
                    remainingMaterial.Quantity -= 1;
                }
            }
            Destroy(item.gameObject);
        }

        private void CompleteCraft()
        {
            AssignItemToTable(null);
        }

        public List<CraftedItemSettings> GetCraftableItems()
        {
            return TableData.TableSettings.CraftableItems;
        }
    }
}
