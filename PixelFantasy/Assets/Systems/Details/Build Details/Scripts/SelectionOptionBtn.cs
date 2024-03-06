using System;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Build_Details.Scripts
{
    public class SelectionOptionBtn : MonoBehaviour
    {
        [SerializeField] private Image _itemIcon;
        [SerializeField] private Image _selectionFrame;

        private Action<SelectionOptionBtn> _onPressedCallback;
        public FurnitureItemData FurnitureItemData;

        public void Init(FurnitureItemData furnitureItemData, Action<SelectionOptionBtn> onPressedCallback)
        {
            FurnitureItemData = furnitureItemData;
            _onPressedCallback = onPressedCallback;
            _itemIcon.sprite = FurnitureItemData.ItemSprite;
            Highlight(false);
        }

        public void Highlight(bool showHighlight)
        {
            if (showHighlight)
            {
                _selectionFrame.gameObject.SetActive(true);
            }
            else
            {
                _selectionFrame.gameObject.SetActive(false);
            }
        }
        
        public void OnPressed()
        {
            _onPressedCallback?.Invoke(this);
        }
    }
}
