using System;
using Data.Item;
using ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Systems.Details.Build_Details.Scripts
{
    public class SelectionOptionBtn : MonoBehaviour
    {
        [SerializeField] private Image _itemIcon;
        [SerializeField] private TextMeshProUGUI _itemName;
        [SerializeField] private Sprite _defaultBG, _activeBG;
        [SerializeField] private Image _btnBG;

        private Action<SelectionOptionBtn> _onPressedCallback;
        public FurnitureSettings FurnitureSettings;
        private bool _isActive;

        public void Init(FurnitureSettings furnitureSettings, Action<SelectionOptionBtn> onPressedCallback)
        {
            FurnitureSettings = furnitureSettings;
            _onPressedCallback = onPressedCallback;
            _itemIcon.sprite = FurnitureSettings.ItemSprite;
            _itemName.text = FurnitureSettings.ItemName;
            Highlight(false);
        }

        public void Highlight(bool isActive)
        {
            _isActive = isActive;

            if (_isActive)
            {
                _btnBG.sprite = _activeBG;
            }
            else
            {
                _btnBG.sprite = _defaultBG;
            }
        }
        
        public void OnPressed()
        {
            _onPressedCallback?.Invoke(this);
        }
    }
}
