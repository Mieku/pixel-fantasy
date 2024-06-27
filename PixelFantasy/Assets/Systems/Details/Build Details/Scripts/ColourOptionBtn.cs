using System;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Details.Build_Details.Scripts
{
    public class ColourOptionBtn : MonoBehaviour
    {
        [SerializeField] private Image _borderImg;
        [SerializeField] private Image _bgImg;
        
        private Action<ColourOptionBtn, DyeSettings> _onPressedCallback;
        private DyeSettings _colour;

        public void Init(DyeSettings colour, Action<ColourOptionBtn, DyeSettings> onPressedCallback)
        {
            _colour = colour;
            _onPressedCallback = onPressedCallback;
            _bgImg.color = _colour.Primary;
            
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
            _onPressedCallback?.Invoke(this, _colour);
            ShowHighlight();
        }
    }
}
