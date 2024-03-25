using System;
using Data.Structure;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Build_Details.Scripts
{
    public class StyleOptionBtn : MonoBehaviour
    {
        [SerializeField] private Image _borderImg;
        [SerializeField] private Image _icon;
        
        private Action<StyleOptionBtn, StyleOption> _onPressedCallback;
        private StyleOption _style;

        public void Init(StyleOption style, Action<StyleOptionBtn, StyleOption> onPressedCallback)
        {
            _style = style;
            _onPressedCallback = onPressedCallback;

            _icon.sprite = style.StyleIcon;
            
            RemoveHighlight();
        }
        
        public void ShowHighlight()
        {
            _borderImg.enabled = true;
        }

        public void RemoveHighlight()
        {
            _borderImg.enabled = false;
        }

        public void OnPressed()
        {
            _onPressedCallback?.Invoke(this, _style);
            ShowHighlight();
        }
    }
}
