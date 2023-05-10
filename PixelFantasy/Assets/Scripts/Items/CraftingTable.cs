using System;
using UnityEngine;

namespace Items
{
    public class CraftingTable : Furniture
    {
        [SerializeField] private SpriteRenderer _craftingPreview;

        protected override void Start()
        {
            base.Start();
            ShowCraftingPreview(null);
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
    }
}
