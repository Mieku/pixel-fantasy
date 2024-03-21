using System;
using Data.Item;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Building_Details.Scripts
{
    public class CraftingOrderOption : MonoBehaviour
    {
        [SerializeField] private Image _itemIcon;
        [SerializeField] private GameObject _selectedHandle;

        public CraftedItemDataSettings Item { get; private set; }

        private Action<CraftedItemDataSettings> _onSelectedCallback;

        public void Init(CraftedItemDataSettings item, Action<CraftedItemDataSettings> onSelectedCallback)
        {
            Item = item;
            _itemIcon.sprite = item.ItemSprite;
            _selectedHandle.SetActive(false);
            _onSelectedCallback = onSelectedCallback;
        }

        public void ShowSelection(bool isShown)
        {
            _selectedHandle.SetActive(isShown);
        }
        
        public void OnOptionPressed()
        {
            _onSelectedCallback.Invoke(Item);
        }
    }
}
