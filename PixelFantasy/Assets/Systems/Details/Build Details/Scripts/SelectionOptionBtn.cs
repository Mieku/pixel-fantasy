using System;
using Data.Item;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Systems.Details.Build_Details.Scripts
{
    public class SelectionOptionBtn : MonoBehaviour
    {
        [SerializeField] private Image _itemIcon;
        [SerializeField] private Image _selectionFrame;

        private Action<SelectionOptionBtn> _onPressedCallback;
        [FormerlySerializedAs("FurnitureData")] [FormerlySerializedAs("FurnitureItemData")] public FurnitureSettings FurnitureSettings;

        public void Init(FurnitureSettings furnitureSettings, Action<SelectionOptionBtn> onPressedCallback)
        {
            FurnitureSettings = furnitureSettings;
            _onPressedCallback = onPressedCallback;
            _itemIcon.sprite = FurnitureSettings.ItemSprite;
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
